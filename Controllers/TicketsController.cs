using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketingSystem_DotNetMVC.Data;
using TicketingSystem_DotNetMVC.Models;

namespace TicketingSystem_DotNetMVC.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TicketsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var tickets = await _context.Tickets.ToListAsync();
            ViewBag.TotalTickets = tickets.Count;
            ViewBag.OpenTickets = tickets.Count(t => t.Status == TicketStatus.Open);
            ViewBag.InProgressTickets = tickets.Count(t => t.Status == TicketStatus.InProgress);
            ViewBag.ClosedTickets = tickets.Count(t => t.Status == TicketStatus.Closed);

            var recentTickets = tickets.OrderByDescending(t => t.CreatedDate).Take(5).ToList();
            return View(recentTickets);
        }

        public async Task<IActionResult> Index()
        {
            var tickets = await _context.Tickets.Include(t => t.User).ToListAsync();
            return View(tickets);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                var userIdStr = HttpContext.Session.GetString("UserId");
                if (userIdStr == null) return RedirectToAction("Login", "Account");

                ticket.UserId = int.Parse(userIdStr);
                ticket.CreatedDate = DateTime.Now;
                ticket.Status = TicketStatus.Open;
                
                _context.Add(ticket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(ticket);
        }

        public async Task<IActionResult> Details(int id)
        {
            var ticket = await _context.Tickets
                .Include(t => t.User)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(t => t.TicketId == id);

            if (ticket == null) return NotFound();

            return View(ticket);
        }
    }
}
