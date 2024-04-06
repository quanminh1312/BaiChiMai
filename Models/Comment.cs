using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogWebsite.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DateTime { get; set; } = DateTime.Now;
        public int UserBlogId { get; set; }
        public UserBlog? UserBlog { get; set; }

        [ForeignKey("UserCommentId")]
        public User? userComment { get; set; }
        public int? UserCommentId { get; set; }
    }
}
