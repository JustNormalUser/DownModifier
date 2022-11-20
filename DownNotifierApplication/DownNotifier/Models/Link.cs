using System.ComponentModel.DataAnnotations;

namespace DownNotifier.Models
{
    public class Link
    {
        public int Id {  get; set; }

        public String? UserId { get; set; }

        [Required]
        [MaxLength(15)]
        [Display(Name = "Link's Name")]
        [DataType(DataType.Text)]
        public string? LinkName { get; set; }

        [Required]
        [MaxLength(300)]
        [Display(Name = "Link's URL")]
        [DataType(DataType.Url)]
        public string? LinkUrl { get; set; }

        [Required]
        [Range(1, 60)]
        [Display(Name = "Link's Check Time As a Minute")]
        public int LinkCheckTime { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{HH:mm}")]
        public DateTime LastTimeModified { get; set; }
    }
}
