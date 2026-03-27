using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using TicketingSystem_DotNetMVC.Data;
using TicketingSystem_DotNetMVC.Models;

namespace TicketingSystem_DotNetMVC.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ApplicationDbContext context, ILogger<DashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private int? GetUserId()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            return int.TryParse(userIdStr, out var userId) ? userId : null;
        }

        private bool IsUser()
        {
            return HttpContext.Session.GetString("UserRole") == "User";
        }

        // GET: Dashboard/ExportTicketPdf
        [HttpGet]
        public async Task<IActionResult> ExportTicketPdf(int id)
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var ticket = await _context.Tickets
                .Include(t => t.User)
                .Include(t => t.AssignedAgent)
                .Include(t => t.Comments)
                .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(t => t.TicketId == id);

            if (ticket == null || (ticket.UserId != userId && HttpContext.Session.GetString("UserRole") == "User"))
                return NotFound();

            var sb = new StringBuilder();
            sb.AppendLine("TICKET REPORT");
            sb.AppendLine("=============");
            sb.AppendLine($"Ticket ID: #{ticket.TicketId}");
            sb.AppendLine($"Title: {ticket.Title}");
            sb.AppendLine($"Status: {ticket.Status}");
            sb.AppendLine($"Priority: {ticket.Priority}");
            sb.AppendLine($"Created: {ticket.CreatedDate:dd MMM yyyy, HH:mm}");
            sb.AppendLine($"User: {ticket.User?.FullName} ({ticket.User?.Email})");
            sb.AppendLine($"Agent: {ticket.AssignedAgent?.FullName ?? "Unassigned"}");
            sb.AppendLine("-------------");
            sb.AppendLine("DESCRIPTION:");
            sb.AppendLine(ticket.Description);
            sb.AppendLine("-------------");
            sb.AppendLine("CONVERSATION:");
            foreach (var comment in ticket.Comments.OrderBy(c => c.CreatedDate))
            {
                sb.AppendLine($"[{comment.CreatedDate:dd MMM HH:mm}] {comment.User?.FullName}: {comment.Message}");
            }

            var content = sb.ToString();
            var bytes = Encoding.UTF8.GetBytes(content);
            return File(bytes, "text/plain", $"Ticket_{id}.txt");
        }

        private IActionResult RedirectToCorrectDashboard()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role == "Admin") return RedirectToAction("Dashboard", "Admin");
            if (role == "Agent") return RedirectToAction("Dashboard", "Agent");
            return RedirectToAction("Login", "Account");
        }

        // GET: Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var userId = GetUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");
            if (!IsUser())
                return RedirectToCorrectDashboard();

            var user = await _context.Users.FindAsync(userId);
            var tickets = await _context.Tickets.Where(t => t.UserId == userId).ToListAsync();

            ViewBag.TotalTickets = tickets.Count;
            ViewBag.Open = tickets.Count(t => t.Status == TicketStatus.Open);
            ViewBag.InProgress = tickets.Count(t => t.Status == TicketStatus.InProgress);
            ViewBag.Closed = tickets.Count(t => t.Status == TicketStatus.Closed);
            
            var recentTickets = tickets.OrderByDescending(t => t.CreatedDate).Take(5).ToList();

            return View(recentTickets);
        }

        // GET: Dashboard/CreateTicket
        [HttpGet]
        public IActionResult CreateTicket()
        {
            var userId = GetUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");
            if (!IsUser())
                return RedirectToCorrectDashboard();
            return View();
        }

        // POST: Dashboard/CreateTicket
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTicket([Bind("Title,Description,Priority")] Ticket ticket, IFormFile? attachment)
        {
            var userId = GetUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");
            if (!IsUser())
                return RedirectToCorrectDashboard();

            if (ModelState.IsValid)
            {
                if (attachment != null && attachment.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "tickets");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + attachment.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await attachment.CopyToAsync(fileStream);
                    }

                    ticket.Attachments = "/uploads/tickets/" + uniqueFileName;
                }

                ticket.UserId = userId.Value;
                ticket.Status = TicketStatus.Open;
                ticket.CreatedDate = DateTime.Now;

                _context.Add(ticket);
                await _context.SaveChangesAsync();

                TempData["Success"] = "your ticket is submitted";
                return RedirectToAction(nameof(MyTickets));
            }
            return View(ticket);
        }

        // GET: Dashboard/MyTickets
        [HttpGet]
        public async Task<IActionResult> MyTickets(string? statusFilter = null, string? priorityFilter = null, string? searchQuery = null)
        {
            var userId = GetUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");
            if (!IsUser())
                return RedirectToCorrectDashboard();

            var query = _context.Tickets.Where(t => t.UserId == userId);

            if (!string.IsNullOrEmpty(statusFilter) && Enum.TryParse<TicketStatus>(statusFilter, out var status))
            {
                query = query.Where(t => t.Status == status);
            }

            if (!string.IsNullOrEmpty(priorityFilter) && Enum.TryParse<TicketPriority>(priorityFilter, out var priority))
            {
                query = query.Where(t => t.Priority == priority);
            }

            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(t => t.Title.Contains(searchQuery) || t.Description.Contains(searchQuery));
            }

            var tickets = await query.OrderByDescending(t => t.CreatedDate).ToListAsync();

            ViewBag.StatusFilter = statusFilter;
            ViewBag.PriorityFilter = priorityFilter;
            ViewBag.SearchQuery = searchQuery;

            return View(tickets);
        }

        // GET: Dashboard/TicketDetails
        [HttpGet]
        public async Task<IActionResult> TicketDetails(int id)
        {
            var userId = GetUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");
            if (!IsUser())
                return RedirectToCorrectDashboard();

            var ticket = await _context.Tickets
                .Include(t => t.User)
                .Include(t => t.Comments)
                .ThenInclude(c => c.User)
                .Include(t => t.AssignedAgent)
                .FirstOrDefaultAsync(t => t.TicketId == id);

            if (ticket == null)
                return NotFound();

            if (ticket.UserId != userId)
                return RedirectToCorrectDashboard();

            return View(ticket);
        }

        [HttpGet]
        public async Task<IActionResult> Notifications()
        {
            var userId = GetUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");
            if (!IsUser())
                return RedirectToCorrectDashboard();

            // Fetch system notifications
            var systemNotifications = await _context.Notifications
                .Where(n => n.TargetRole == "All" || n.TargetRole == "User" || n.UserId == userId.Value)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();

            // Fetch ticket comments as notifications
            var ticketComments = await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Ticket)
                .Where(c => c.Ticket.UserId == userId.Value && c.UserId != userId.Value)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();

            ViewBag.SystemNotifications = systemNotifications;
            return View(ticketComments);
        }

        [HttpGet]
        public async Task<IActionResult> TicketTimeline(int? ticketId = null)
        {
            var userId = GetUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");
            if (!IsUser())
                return RedirectToCorrectDashboard();

            var userTickets = await _context.Tickets
                .Where(t => t.UserId == userId.Value)
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync();

            Ticket? selectedTicket = null;

            if (ticketId.HasValue)
            {
                selectedTicket = await _context.Tickets
                    .Include(t => t.AssignedAgent)
                    .FirstOrDefaultAsync(t => t.TicketId == ticketId.Value && t.UserId == userId.Value);
            }

            if (selectedTicket == null)
            {
                selectedTicket = await _context.Tickets
                    .Include(t => t.AssignedAgent)
                    .Where(t => t.UserId == userId.Value)
                    .OrderByDescending(t => t.CreatedDate)
                    .FirstOrDefaultAsync();
            }

            ViewBag.UserTickets = userTickets;
            ViewBag.SelectedTicketId = selectedTicket?.TicketId;

            return View(selectedTicket);
        }

        // POST: Dashboard/AddComment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int ticketId, string message)
        {
            var userId = GetUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");
            if (!IsUser())
                return RedirectToCorrectDashboard();

            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null)
                return NotFound();

            if (ticket.UserId != userId)
                return RedirectToCorrectDashboard();

            if (!string.IsNullOrEmpty(message))
            {
                var comment = new Comment
                {
                    TicketId = ticketId,
                    UserId = userId.Value,
                    Message = message,
                    CreatedDate = DateTime.Now
                };

                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Comment added successfully!";
            }

            return RedirectToAction(nameof(TicketDetails), new { id = ticketId });
        }

        // POST: Dashboard/RateTicket
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RateTicket(int ticketId, double rating, string feedback)
        {
            var userId = GetUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");
            if (!IsUser())
                return Unauthorized();

            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null)
                return NotFound();

            if (ticket.UserId != userId)
                return Unauthorized();

            if (ticket.Status != TicketStatus.Closed)
            {
                TempData["Error"] = "You can only rate closed tickets.";
                return RedirectToAction(nameof(TicketDetails), new { id = ticketId });
            }

            ticket.Rating = rating;
            ticket.Feedback = feedback;

            _context.Update(ticket);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thank you for your feedback!";
            return RedirectToAction(nameof(TicketDetails), new { id = ticketId });
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = GetUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");
            if (!IsUser())
                return Unauthorized();

            var user = await _context.Users
                .Include(u => u.CreatedTickets)
                .FirstOrDefaultAsync(u => u.UserId == userId.Value);

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
        public async Task<IActionResult> UpdateProfile(int userId, string fullName, string? currentPassword, string? newPassword)
        {
            var sessionUserId = GetUserId();
            if (sessionUserId == null)
                return RedirectToAction("Login", "Account");
            if (!IsUser())
                return Unauthorized();
            if (sessionUserId.Value != userId)
                return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
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
            await _context.SaveChangesAsync();

            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction(nameof(Profile));
        }
    }
}
