using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using TicketingSystem_DotNetMVC.Data;
using TicketingSystem_DotNetMVC.Models;
using TicketingSystem_DotNetMVC.Services;

namespace TicketingSystem_DotNetMVC.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public AdminController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
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

            var agents = await _context.Users
                .Where(u => u.Role == "Agent" && u.IsActive)
                .ToListAsync();

            ViewBag.Agents = agents;

            return View(tickets);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTicket(int ticketId, int agentId)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
                return Unauthorized();

            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null) return NotFound();

            var agent = await _context.Users.FindAsync(agentId);
            if (agent == null || agent.Role != "Agent")
            {
                TempData["Error"] = "Invalid agent selected.";
                return RedirectToAction(nameof(AllTickets));
            }

            ticket.AssignedAgentId = agentId;
            ticket.Status = TicketStatus.InProgress;
            ticket.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Ticket #{ticketId} assigned to {agent.FullName}.";
            return RedirectToAction(nameof(AllTickets));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
                return Unauthorized();

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            // Check if it's the current admin
            var currentUserId = HttpContext.Session.GetString("UserId");
            if (currentUserId == id.ToString())
            {
                TempData["Error"] = "You cannot delete yourself.";
                return RedirectToAction(user.Role == "Agent" ? nameof(ManageAgents) : nameof(ManageUsers));
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"{user.Role} deleted successfully.";
            return RedirectToAction(user.Role == "Agent" ? nameof(ManageAgents) : nameof(ManageUsers));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int userId, string fullName, string email)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
                return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.FullName = fullName;
            user.Email = email;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"{user.Role} updated successfully.";
            return RedirectToAction(user.Role == "Agent" ? nameof(ManageAgents) : nameof(ManageUsers));
        }

        [HttpGet]
        public async Task<IActionResult> ManageUsers()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole == null) return RedirectToAction("Login", "Account");
            if (userRole != "Admin") return RedirectToAction("Index", "Home");

            var users = await _context.Users
                .Where(u => u.Role == "User")
                .ToListAsync();

            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> ManageAgents()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole == null) return RedirectToAction("Login", "Account");
            if (userRole != "Admin") return RedirectToAction("Index", "Home");

            var agents = await _context.Users
                .Where(u => u.Role == "Agent")
                .ToListAsync();

            return View(agents);
        }

        [HttpGet]
        public async Task<IActionResult> ExportAllReports()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole == null) return RedirectToAction("Login", "Account");
            if (userRole != "Admin") return RedirectToAction("Index", "Home");

            var tickets = await _context.Tickets
                .Include(t => t.User)
                .Include(t => t.AssignedAgent)
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("SYSTEM ANALYTICS REPORT");
            sb.AppendLine("=======================");
            sb.AppendLine($"Report Date: {DateTime.Now:dd MMM yyyy, HH:mm}");
            sb.AppendLine($"Total Tickets: {tickets.Count}");
            sb.AppendLine($"Open: {tickets.Count(t => t.Status == TicketStatus.Open)}");
            sb.AppendLine($"In Progress: {tickets.Count(t => t.Status == TicketStatus.InProgress)}");
            sb.AppendLine($"Closed: {tickets.Count(t => t.Status == TicketStatus.Closed)}");
            sb.AppendLine("-----------------------");
            sb.AppendLine("DETAILED TICKET LIST:");
            sb.AppendLine("TicketId | Status | Priority | User | Agent | CreatedDate");
            foreach (var t in tickets)
            {
                sb.AppendLine($"#{t.TicketId} | {t.Status} | {t.Priority} | {t.User?.FullName} | {t.AssignedAgent?.FullName ?? "Unassigned"} | {t.CreatedDate:yyyy-MM-dd HH:mm}");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/plain", "System_Full_Report.txt");
        }

        [HttpGet]
        public async Task<IActionResult> Reports()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole == null) return RedirectToAction("Login", "Account");
            if (userRole != "Admin") return RedirectToAction("Index", "Home");

            var tickets = await _context.Tickets.ToListAsync();
            return View(tickets);
        }

        [HttpGet]
        public IActionResult SendNotification()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole == null) return RedirectToAction("Login", "Account");
            if (userRole != "Admin") return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendNotification(string targetRole, string message, string subject)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
                return Unauthorized();

            var adminId = int.Parse(HttpContext.Session.GetString("UserId")!);
            
            var usersToNotify = _context.Users.AsQueryable();
            if (targetRole != "All")
            {
                usersToNotify = usersToNotify.Where(u => u.Role == targetRole);
            }

            var userList = await usersToNotify.ToListAsync();

            var notification = new Notification
            {
                TargetRole = targetRole,
                Subject = subject,
                Message = message,
                CreatedDate = DateTime.Now
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            foreach (var user in userList)
            {
                // Send simulated email
                await _emailService.SendEmailAsync(user.Email, subject, message);
            }

            TempData["Success"] = $"Notification broadcasted to {userList.Count} users via Web and Email.";
            return RedirectToAction(nameof(Dashboard));
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

            var user = await _context.Users
                .Include(u => u.CreatedTickets)
                .FirstOrDefaultAsync(u => u.UserId == int.Parse(userId));
            if (user == null)
                return NotFound();

            ViewBag.CreatedTicketsCount = user.CreatedTickets.Count;
            ViewBag.OpenTicketsCount = user.CreatedTickets.Count(t => t.Status == TicketStatus.Open);
            ViewBag.InProgressTicketsCount = user.CreatedTickets.Count(t => t.Status == TicketStatus.InProgress);
            ViewBag.ClosedTicketsCount = user.CreatedTickets.Count(t => t.Status == TicketStatus.Closed);

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
