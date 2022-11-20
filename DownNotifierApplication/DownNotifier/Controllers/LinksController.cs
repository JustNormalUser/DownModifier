using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DownNotifier.Data;
using DownNotifier.Models;
using Microsoft.AspNetCore.Authorization;
using DownNotifier.Service;
using System.Net;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using System.Net.Mail;
using Raven.Abstractions.Util.MiniMetrics;

namespace DownNotifier.Controllers
{
    public class LinksController : Controller
    {
        private readonly LinkDbContext _context;
        private readonly IUserService _userService;
        private readonly ApplicationDbContext _applicationContext;

        public LinksController(LinkDbContext context, IUserService userService, ApplicationDbContext applicationContext)
        {
            _context = context;
            _userService = userService;
            _applicationContext = applicationContext;
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLastTimeModified(int id)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    var link = await _context.Link.FindAsync(id);
                    link.LastTimeModified = DateTime.Now;
                    Console.WriteLine("+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ " + link.LastTimeModified);
                    _context.Update(link);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {

                }
                return RedirectToAction(nameof(Index));
            }
            return View();
        }


        [Authorize]
        // GET: Links
        public async Task<IActionResult> Index()
        {

            var userId = _userService.GetUserId();
            var pair = _context.Link.Where(x => x.UserId == userId).OrderBy(x => x.UserId);

            //var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));

            //while (await timer.WaitForNextTickAsync())
            //{
            //    ForLoopRequest();
            //}

            ForLoopRequest();

            //await EditLastTimeModified(2);

            return View(await pair.ToListAsync());

        }


        public void CallFor()
        {

        }

        public void ForLoopRequest()
        {
            var count = _context.Link.Count();
            var url2 = _context.Link.Where(x => x.Id == count).FirstOrDefault(x => x.Id == count)?.LinkUrl;
            //Console.WriteLine("*****************************************************************" + count + "\n " + url2);

            var periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(10));
            //while (await periodicTimer.WaitForNextTickAsync())
            //{

            for (int i = 11; i <= count + 10; i++)
            {
                if (UrlIsValid(i) == false)
                {
                    SmtpSendEmail(i);
                 }
                //}
            }
        }

        public void SmtpSendEmail(int i)
        {
            var url = _context.Link.Where(x => x.Id == i).FirstOrDefault(x => x.Id == i)?.LinkUrl;
            var userIdLinkDb = _context.Link.Where(x => x.Id == i).FirstOrDefault(x => x.Id == i)?.UserId;
            var normalizedUserNameFromUserIdLinkDb = _applicationContext.Users.Where(x => x.Id == userIdLinkDb).FirstOrDefault(x => x.Id == userIdLinkDb)?.NormalizedEmail;

            Console.WriteLine("Smtp Çalıştı *************");
            Console.WriteLine("Url: " + url + " /UserIdLinDb: " + userIdLinkDb + " /UserEmailAppDb: " + normalizedUserNameFromUserIdLinkDb);

            try
            {
                SmtpClient client = new SmtpClient("smtp-mail.outlook.com");
                client.Port = 587;
                client.DeliveryMethod= SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials= false;
                System.Net.NetworkCredential credential = new System.Net.NetworkCredential("", "");
                client.EnableSsl= true;
                client.Credentials = credential;

                MailMessage message = new MailMessage("", "");
                message.Subject= "URL Warning";
                message.Body = "Your URL: " + url + " has returned different than 2XX!";
                message.IsBodyHtml = false;
                client.Send(message);
            }
            catch
            {
                throw;
            }

        }

        public bool UrlIsValid(int i)
        {
                //var task = Task.Run(async () => await EditLastTimeModified(4));

                var url = _context.Link.Where(x => x.Id == i).FirstOrDefault(x => x.Id == i)?.LinkUrl;
                var duration = _context.Link.Where(x => x.Id == i).FirstOrDefault(x => x.Id == i)?.LinkCheckTime;
                DateTime lastTimeModified = (DateTime)(_context.Link.Where(x => x.Id == i).FirstOrDefault(x => x.Id == i)?.LastTimeModified);
                DateTime CurrentTime = DateTime.Now;
                TimeSpan difference = CurrentTime - lastTimeModified;
                
                Console.WriteLine("Id:" + i +  " Duration:" + duration + " lastTime:" + lastTimeModified + " difference:" + difference + " url:" + url);

                Console.WriteLine(difference.TotalMinutes);


                var minutesPassed = (int)(CurrentTime - lastTimeModified).TotalMinutes;

                if (minutesPassed >= duration)
                {

                    //Console.WriteLine("if deneme");
                    EditLastTimeModified(i).GetAwaiter().GetResult();


                    try
                    {
                        HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;
                        request.Timeout = 5000;
                        request.Method = "HEAD";

                        using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                        {
                            int statusCode = (int)response.StatusCode;
                            if (statusCode >= 200 && statusCode < 300)
                            {
                                Console.WriteLine("*****************************************************************" + "200");
                                return true;
                            }
                            else if (statusCode >= 300 && statusCode < 200)
                            {
                                Console.WriteLine("*****************************************************************" + "=!200");
                                return false;
                            }
                        }
                    }
                    catch (WebException ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                return true;
        }

        [Authorize]
        // GET: Links/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Link == null)
            {
                return NotFound();
            }

            var link = await _context.Link
                .FirstOrDefaultAsync(m => m.Id == id);
            if (link == null)
            {
                return NotFound();
            }

            return View(link);
        }

        [Authorize]
        // GET: Links/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Links/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,LinkName,LinkUrl,LinkCheckTime")] Link link)
        {
            if (ModelState.IsValid)
            {
                var userId = _userService.GetUserId();
                link.LastTimeModified= DateTime.Now;
                link.UserId = userId;
                _context.Add(link);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(link);
        }

        [Authorize]
        // GET: Links/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Link == null)
            {
                return NotFound();
            }

            var link = await _context.Link.FindAsync(id);
            if (link == null)
            {
                return NotFound();
            }
            return View(link);
        }

        // POST: Links/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,LinkName,LinkUrl,LinkCheckTime")] Link link)
        {
            if (id != link.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(link);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LinkExists(link.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(link);
        }

        [Authorize]
        // GET: Links/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Link == null)
            {
                return NotFound();
            }

            var link = await _context.Link
                .FirstOrDefaultAsync(m => m.Id == id);
            if (link == null)
            {
                return NotFound();
            }

            return View(link);
        }

        [Authorize]
        // POST: Links/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Link == null)
            {
                return Problem("Entity set 'LinkDbContext.Link'  is null.");
            }
            var link = await _context.Link.FindAsync(id);
            if (link != null)
            {
                _context.Link.Remove(link);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LinkExists(int id)
        {
          return _context.Link.Any(e => e.Id == id);
        }
    }
}
