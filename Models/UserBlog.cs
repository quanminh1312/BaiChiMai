using System.ComponentModel.DataAnnotations;

namespace BlogWebsite.Models
{
    public class UserBlog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string? Title { get; set; }
        [Required]
        [StringLength(255)]
        public string? Content { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DateTime { get; set; } = DateTime.Now;
        public string? Image { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public List<Comment>? Comments { get; set; }
    }
}
