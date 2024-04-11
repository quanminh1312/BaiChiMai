using BlogWebsite.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Reflection.Metadata;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace blog.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly BlogContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public HomeController(ILogger<HomeController> logger, BlogContext context, IWebHostEnvironment hostingEnvironment)
        {
            _logger = logger;
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index()
        {
            ViewBag.Id = HttpContext.Session.GetInt32("UserId");
			if (HttpContext.Session.GetInt32("UserId")==null)
            {
                return RedirectToAction("Login");
            }
            var blog = _context.UserBlogs.Include(u => u.Comments).ToList();
            return View(blog);
        }
		[HttpPost]
        public IActionResult Register(User user)
        {
			ViewBag.Id = HttpContext.Session.GetInt32("UserId");
			//check if the email is already exists
			var userInDb = _context.Users.FirstOrDefault(u => u.Email == user.Email);
            if (userInDb != null)
            {
                ModelState.AddModelError("Email", "Email already exists");
                return View(user);
            }

            //check for the admin password
            var users = _context.Users.ToList();
            if (users.Count == 0 && user.Password=="admin")
            {
                ModelState.AddModelError("Password","admin can't not user password as admin");
                return View(user);
            }

            //check for model validation
            if (ModelState.IsValid)
            {
                _context.Add(user);
                _context.SaveChanges();
                //CreateUserFolder(user.Id.ToString());
                return RedirectToAction("Index");
            }

            //error in the model validation
            return View();
        }
        public async Task<bool> UploadFile(IFormFile file)
        {
            string fileName = "img";
            if (file == null || file.Length == 0)
            {
                return false;
            }

            var folderPath = Path.Combine(_hostingEnvironment.WebRootPath,fileName);
            var filePath = Path.Combine(folderPath, file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);

            }
            return true;
        }
        public IActionResult Login()
        {
			ViewBag.Id = HttpContext.Session.GetInt32("UserId");
			if (HttpContext.Session.GetInt32("UserId") == null) RedirectToAction("Index");
				return View();
        }
        [HttpPost]
        public IActionResult Login(User user)
        {
			ViewBag.Id = HttpContext.Session.GetInt32("UserId");
			var userInDb = _context.Users.FirstOrDefault(u => u.Email == user.Email);
            if (userInDb == null)
            {
                ModelState.AddModelError("Email", "Invalid Email/Password");
                return View(user);
            }
            //just check the password no need to hash it
            if (userInDb.Password != user.Password)
            {
                if (user.Password!.Equals("admin") && userInDb.Id ==1)
                {
                    HttpContext.Session.SetInt32("UserId", userInDb.Id);
                    return RedirectToAction("Index", "Admin");
                }
                ModelState.AddModelError("Email", "Invalid Email/Password");
                return View(user);
            }
            HttpContext.Session.SetInt32("UserId", userInDb.Id);
            return RedirectToAction("Index");
        }
        public IActionResult Post()
        {
			ViewBag.Id = HttpContext.Session.GetInt32("UserId");
			if (HttpContext.Session.GetInt32("UserId") != 1)
			{
				return RedirectToAction("Index");
			}
			return View();
        }
        [HttpPost]
        public IActionResult Post(UserBlog blog, IFormFile image)
        {
			ViewBag.Id = HttpContext.Session.GetInt32("UserId");
			if (HttpContext.Session.GetInt32("UserId") != 1) return RedirectToAction("Index");  
            blog.UserId = (int)HttpContext.Session.GetInt32("UserId")!;
            blog.DateTime = DateTime.Now;
            if (image != null)
            {
                blog.Image = image.FileName;
                if (!UploadFile(image).Result)
                {
                    // if the file is not uploaded successfully pop up a message 
                    ModelState.AddModelError("Image", "The image is not uploaded successfully");
                    return View();
                }
            }
            _context.Add(blog);
            _context.SaveChanges();
            if (ModelState.IsValid)
                return RedirectToAction("Index");
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Comment(string content, string blogId)
        {
			ViewBag.Id = HttpContext.Session.GetInt32("UserId");
			if (HttpContext.Session.GetInt32("UserId") == null) return RedirectToAction("Login");
            Comment comment = new Comment();
            comment.Content = content;
            var userBlog = await _context.UserBlogs.Include(u => u.User).Include(u => u.Comments).Where(u => u.Id! == int.Parse(blogId)).FirstOrDefaultAsync();
            if (userBlog != null)
            {
                comment.UserBlog = userBlog;
                comment.UserBlogId = int.Parse(blogId);
            }
            comment.UserCommentId = (int)HttpContext.Session.GetInt32("UserId")!;
            var user = await _context.Users.Include(u => u.UserBlogs).Include(u => u.Comments).Where(u => u.Id! == comment.UserCommentId).FirstOrDefaultAsync();
            if (user != null) 
            {
                comment.userComment = user;
            }
            comment.DateTime = DateTime.Now;
            if (ModelState.IsValid)
            {
                _context.Add(comment);
                _context.SaveChanges();
                return RedirectToAction("Portfolio", new { id = blogId });
            }
            //pop up a message if the comment is not valid
            return View("index");
        }
        public async Task<IActionResult> DeleteComment(string Id)
        {
			ViewBag.Id = HttpContext.Session.GetInt32("UserId");
			int id = int.Parse(Id);
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == id);
            if (comment == null) return RedirectToAction("Index");
            _context.Remove(comment);
            _context.SaveChanges();
            return RedirectToAction("Blog", new { id = comment.UserBlogId });
        }
        //delete blog
        public async Task<IActionResult> DeleteBlog(string Id)
        {
			ViewBag.Id = HttpContext.Session.GetInt32("UserId");
			int id = int.Parse(Id);
            var blog = await _context.UserBlogs.FirstOrDefaultAsync(b => b.Id == id);
            if (blog == null) return RedirectToAction("Index");
            _context.Remove(blog);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        public IActionResult LogOut()
        {
			ViewBag.Id = HttpContext.Session.GetInt32("UserId");
			HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Portfolio(int id)
        {
			var blog = await _context.UserBlogs.Include(b => b.Comments)!.ThenInclude(c => c.userComment).Include(c => c.User).FirstOrDefaultAsync(b => b.Id == id);
            ViewBag.Id = HttpContext.Session.GetInt32("UserId");
            if (blog == null) return RedirectToAction("Index");
            return View(blog);
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
			ViewBag.Id = HttpContext.Session.GetInt32("UserId");
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult About()
        {
			ViewBag.Id = HttpContext.Session.GetInt32("UserId");
			if (HttpContext.Session.GetInt32("UserId") == null)
			{
				return RedirectToAction("Login");
			}
			return View();
        }
    }
}
