using Microsoft.AspNetCore.Mvc;
using TicketingSystem_DotNetMVC.Data;

namespace TicketingSystem_DotNetMVC.Controllers
{
    public class AgentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AgentController(ApplicationDbContext context)
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
            if (userRole != "Agent")
                return Unauthorized();

            var agentId = int.Parse(userId);
            
            // Agent dashboard - will be implemented in next phase
            ViewBag.AssignedTickets = _context.Tickets.Count(t => t.AssignedAgentId == agentId);
            ViewBag.CompletedTickets = _context.Tickets.Count(t => t.AssignedAgentId == agentId && t.Status == "Closed");
            ViewBag.PendingTickets = _context.Tickets.Count(t => t.AssignedAgentId == agentId && t.Status != "Closed");

            return View();
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            return Index();
        }

        [HttpGet]
        public IActionResult AssignedTickets()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Agent")
                return Unauthorized();

            return View();
        }

        [HttpGet]
        public IActionResult TicketDetails()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Agent")
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
            if (userRole != "Agent")
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
            if (userRole != "Agent")
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
