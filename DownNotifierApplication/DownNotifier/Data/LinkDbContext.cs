using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DownNotifier.Models;

namespace DownNotifier.Data
{
    public class LinkDbContext : DbContext
    {

        public LinkDbContext (DbContextOptions<LinkDbContext> options)
            : base(options)
        {
        }


        public DbSet<DownNotifier.Models.Link> Link { get; set; } = default!;
    }
}
