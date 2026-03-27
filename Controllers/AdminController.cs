using Microsoft.AspNetCore.Mvc;
using TicketingSystem_DotNetMVC.Data;

namespace TicketingSystem_DotNetMVC.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
                return Unauthorized();

            // Admin dashboard - will be implemented in next phase
            ViewBag.TotalTickets = _context.Tickets.Count();
            ViewBag.TotalUsers = _context.Users.Count();
            ViewBag.OpenTickets = _context.Tickets.Count(t => t.Status == "Open");

            return View();
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            return Index();
        }

        [HttpGet]
        public IActionResult AllTickets()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
                return Unauthorized();

            return View();
        }

        [HttpGet]
        public IActionResult ManageUsers()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
                return Unauthorized();

            return View();
        }

        [HttpGet]
        public IActionResult ManageAgents()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
                return Unauthorized();

            return View();
        }

        [HttpGet]
        public IActionResult Reports()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
                return Unauthorized();

            return View();
        }

        [HttpGet]
        public IActionResult Profile()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
                return Unauthorized();

            var user = _context.Users.Find(int.Parse(userId));
            if (user == null)
                return NotFound();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfile(int userId, string fullName, string? currentPassword, string? newPassword)
        {
            var sessionUserId = HttpContext.Session.GetString("UserId");
            if (sessionUserId == null)
                return RedirectToAction("Login", "Account");

            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
                return Unauthorized();

            if (sessionUserId != userId.ToString())
                return Unauthorized();

            var user = _context.Users.Find(userId);
            if (user == null)
                return NotFound();

            user.FullName = fullName;
            HttpContext.Session.SetString("UserName", fullName);

            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
                {
                    TempData["Error"] = "Current password is incorrect.";
                    return RedirectToAction(nameof(Profile));
                }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            }

            _context.Update(user);
            _context.SaveChanges();

            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction(nameof(Profile));
        }
    }
}
