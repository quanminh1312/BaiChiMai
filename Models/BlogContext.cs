using Microsoft.EntityFrameworkCore;

namespace BlogWebsite.Models
{
    public class BlogContext : DbContext
    {
        public BlogContext(DbContextOptions<BlogContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<UserBlog> UserBlogs { get; set; }
        public DbSet<Comment> Comments { get; set; }
    }
}
