using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        // GET: Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var userId = GetUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");
            if (!IsUser())
                return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            var tickets = await _context.Tickets.Where(t => t.UserId == userId).ToListAsync();

            var dashboardStats = new
            {
                TotalTickets = tickets.Count,
                Open = tickets.Count(t => t.Status == "Open"),
                InProgress = tickets.Count(t => t.Status == "In Progress"),
                Closed = tickets.Count(t => t.Status == "Closed"),
                User = user,
                RecentTickets = tickets.OrderByDescending(t => t.CreatedDate).Take(5).ToList()
            };

            return View(dashboardStats);
        }

        // GET: Dashboard/CreateTicket
        [HttpGet]
        public IActionResult CreateTicket()
        {
            var userId = GetUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");
            if (!IsUser())
                return Unauthorized();
            return View();
        }

        // POST: Dashboard/CreateTicket
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTicket([Bind("Title,Description,Priority")] Ticket ticket)
        {
            var userId = GetUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");
            if (!IsUser())
                return Unauthorized();

            if (ModelState.IsValid)
            {
                ticket.UserId = userId.Value;
                ticket.Status = "Open";
                ticket.CreatedDate = DateTime.Now;

                _context.Add(ticket);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Ticket #{ticket.TicketId} created successfully!";
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
                return Unauthorized();

            IQueryable<Ticket> query = _context.Tickets
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedDate);

            // Apply filters
            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
                query = query.Where(t => t.Status == statusFilter);

            if (!string.IsNullOrEmpty(priorityFilter) && priorityFilter != "All")
                query = query.Where(t => t.Priority == priorityFilter);

            if (!string.IsNullOrEmpty(searchQuery))
                query = query.Where(t => t.Title.Contains(searchQuery) || t.Description.Contains(searchQuery));

            var tickets = await query.ToListAsync();

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
                return Unauthorized();

            var ticket = await _context.Tickets
                .Include(t => t.Comments)
                .ThenInclude(c => c.User)
                .Include(t => t.AssignedAgent)
                .FirstOrDefaultAsync(t => t.TicketId == id);

            if (ticket == null)
                return NotFound();

            if (ticket.UserId != userId)
                return Unauthorized();

            return View(ticket);
        }

        [HttpGet]
        public async Task<IActionResult> Notifications()
        {
            var userId = GetUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");
            if (!IsUser())
                return Unauthorized();

            var notifications = await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Ticket)
                .Where(c => c.Ticket.UserId == userId.Value && c.UserId != userId.Value)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();

            return View(notifications);
        }

        [HttpGet]
        public async Task<IActionResult> TicketTimeline(int? ticketId = null)
        {
            var userId = GetUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");
            if (!IsUser())
                return Unauthorized();

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
                return Unauthorized();

            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null)
                return NotFound();

            if (ticket.UserId != userId)
                return Unauthorized();

            if (!string.IsNullOrEmpty(message))
            {
                var comment = new Comment
                {
                    TicketId = ticketId,
                    UserId = userId.Value,
                    Message = message,
                    CreatedDate = DateTime.Now
                };

                _context.Add(comment);
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

            if (ticket.Status != "Closed")
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

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
                return NotFound();

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
