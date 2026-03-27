using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketingSystem_DotNetMVC.Data;
using TicketingSystem_DotNetMVC.Models;

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
        public async Task<IActionResult> Index()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (userIdStr == null)
                return RedirectToAction("Login", "Account");

            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Agent")
                return Unauthorized();

            var agentId = int.Parse(userIdStr);
            
            ViewBag.AssignedTickets = await _context.Tickets.CountAsync(t => t.AssignedAgentId == agentId);
            ViewBag.UnassignedTickets = await _context.Tickets.CountAsync(t => t.AssignedAgentId == null && t.Status == TicketStatus.Open);
            ViewBag.CompletedTickets = await _context.Tickets.CountAsync(t => t.AssignedAgentId == agentId && t.Status == TicketStatus.Closed);
            ViewBag.PendingTickets = await _context.Tickets.CountAsync(t => t.AssignedAgentId == agentId && t.Status != TicketStatus.Closed);

            var recentTickets = await _context.Tickets
                .Include(t => t.User)
                .Where(t => (t.AssignedAgentId == agentId || t.AssignedAgentId == null) && t.Status != TicketStatus.Closed)
                .OrderByDescending(t => t.CreatedDate)
                .Take(10)
                .ToListAsync();

            return View(recentTickets);
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            return await Index();
        }

        [HttpGet]
        public async Task<IActionResult> AssignedTickets()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (userIdStr == null)
                return RedirectToAction("Login", "Account");

            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Agent")
                return Unauthorized();

            var agentId = int.Parse(userIdStr);
            var tickets = await _context.Tickets
                .Include(t => t.User)
                .Where(t => t.AssignedAgentId == agentId)
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync();

            return View(tickets);
        }

        [HttpGet]
        public async Task<IActionResult> TicketDetails(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Agent")
                return Unauthorized();

            var ticket = await _context.Tickets
                .Include(t => t.User)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(t => t.TicketId == id);

            if (ticket == null)
                return NotFound();

            return View(ticket);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Agent")
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

        [HttpGet]
        public async Task<IActionResult> Reports()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (userIdStr == null)
                return RedirectToAction("Login", "Account");

            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Agent")
                return Unauthorized();

            var agentId = int.Parse(userIdStr);

            var tickets = await _context.Tickets
                .Where(t => t.AssignedAgentId == agentId)
                .ToListAsync();

            ViewBag.Total = tickets.Count;
            ViewBag.Open = tickets.Count(t => t.Status == TicketStatus.Open);
            ViewBag.InProgress = tickets.Count(t => t.Status == TicketStatus.InProgress);
            ViewBag.Closed = tickets.Count(t => t.Status == TicketStatus.Closed);

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Notifications()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (userIdStr == null)
                return RedirectToAction("Login", "Account");

            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Agent")
                return Unauthorized();

            var agentId = int.Parse(userIdStr);

            // Fetch notifications (for simplicity, comments on tickets assigned to the agent where the comment isn't by the agent)
            var notifications = await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Ticket)
                .Where(c => c.Ticket.AssignedAgentId == agentId && c.UserId != agentId)
                .OrderByDescending(c => c.CreatedDate)
                .Take(20)
                .ToListAsync();

            return View(notifications);
        }
    }
}
