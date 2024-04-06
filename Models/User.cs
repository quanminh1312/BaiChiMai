using System.ComponentModel.DataAnnotations;

namespace BlogWebsite.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string? Username { get; set; }
        [Required]
        [StringLength(50)]
        public string? Password { get; set; }
        [Required]
        [StringLength(50)]
        public string? Email { get; set; }
        public string? Image { get; set; }
        public List<UserBlog>? UserBlogs { get; set; }
        public List<Comment>? Comments { get; set; }
    }
}
