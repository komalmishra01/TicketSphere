using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketingSystem_DotNetMVC.Data;
using TicketingSystem_DotNetMVC.Models;

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
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
                return Unauthorized();

            ViewBag.TotalTickets = await _context.Tickets.CountAsync();
            ViewBag.TotalUsers = await _context.Users.CountAsync(u => u.Role == "User");
            ViewBag.TotalAgents = await _context.Users.CountAsync(u => u.Role == "Agent");
            ViewBag.OpenTickets = await _context.Tickets.CountAsync(t => t.Status == TicketStatus.Open);
            ViewBag.ClosedTickets = await _context.Tickets.CountAsync(t => t.Status == TicketStatus.Closed);
            
            var recentTickets = await _context.Tickets
                .Include(t => t.User)
                .OrderByDescending(t => t.CreatedDate)
                .Take(5)
                .ToListAsync();

            return View(recentTickets);
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            return await Index();
        }

        [HttpGet]
        public async Task<IActionResult> AllTickets()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
                return Unauthorized();

            var tickets = await _context.Tickets
                .Include(t => t.User)
                .Include(t => t.AssignedAgent)
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync();

            return View(tickets);
        }

        [HttpGet]
        public async Task<IActionResult> ManageUsers()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
                return Unauthorized();

            var users = await _context.Users
                .Where(u => u.Role == "User")
                .ToListAsync();

            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> ManageAgents()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
                return Unauthorized();

            var agents = await _context.Users
                .Where(u => u.Role == "Agent")
                .ToListAsync();

            return View(agents);
        }

        [HttpGet]
        public async Task<IActionResult> Reports()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
                return Unauthorized();

            var tickets = await _context.Tickets.ToListAsync();
            return View(tickets);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
                return Unauthorized();

            var user = await _context.Users.FindAsync(int.Parse(userId));
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAgent(string fullName, string email, string password)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
                return Unauthorized();

            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                TempData["Error"] = "Email already exists.";
                return RedirectToAction(nameof(ManageAgents));
            }

            var agent = new User
            {
                FullName = fullName,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = "Agent",
                IsActive = true,
                CreatedDate = DateTime.Now
            };

            _context.Add(agent);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Agent added successfully.";
            return RedirectToAction(nameof(ManageAgents));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAgentStatus(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
                return Unauthorized();

            var agent = await _context.Users.FindAsync(id);
            if (agent == null || agent.Role != "Agent")
                return NotFound();

            agent.IsActive = !agent.IsActive;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Agent status updated to {(agent.IsActive ? "Active" : "Deactive")}.";
            return RedirectToAction(nameof(ManageAgents));
        }
    }
}
