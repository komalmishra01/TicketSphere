using Microsoft.AspNetCore.Mvc;
using TicketingSystem_DotNetMVC.Models;

namespace TicketingSystem_DotNetMVC.Controllers
{
    public class TicketsController : Controller
    {
        // Mock data for UI demonstration
        private static List<Ticket> _mockTickets = new List<Ticket>
        {
            new Ticket { Id = 101, Title = "Login Issue", Description = "Cannot login to the portal", Priority = TicketPriority.High, Status = TicketStatus.Open, CreatedDate = DateTime.Now.AddHours(-5), UserId = "User1" },
            new Ticket { Id = 102, Title = "Printer Setup", Description = "Need help setting up the new printer", Priority = TicketPriority.Medium, Status = TicketStatus.InProgress, CreatedDate = DateTime.Now.AddDays(-1), UserId = "User1" },
            new Ticket { Id = 103, Title = "Software Install", Description = "Request for Visual Studio installation", Priority = TicketPriority.Low, Status = TicketStatus.Closed, CreatedDate = DateTime.Now.AddDays(-2), UserId = "User1" },
            new Ticket { Id = 104, Title = "Network Slow", Description = "Internet speed is very slow since morning", Priority = TicketPriority.High, Status = TicketStatus.Open, CreatedDate = DateTime.Now.AddHours(-2), UserId = "User1" },
            new Ticket { Id = 105, Title = "Email Not Working", Description = "Outlook is not syncing", Priority = TicketPriority.Medium, Status = TicketStatus.InProgress, CreatedDate = DateTime.Now.AddHours(-10), UserId = "User1" }
        };

        public IActionResult Dashboard()
        {
            ViewBag.TotalTickets = _mockTickets.Count;
            ViewBag.OpenTickets = _mockTickets.Count(t => t.Status == TicketStatus.Open);
            ViewBag.InProgressTickets = _mockTickets.Count(t => t.Status == TicketStatus.InProgress);
            ViewBag.ClosedTickets = _mockTickets.Count(t => t.Status == TicketStatus.Closed);

            var recentTickets = _mockTickets.OrderByDescending(t => t.CreatedDate).Take(5).ToList();
            return View(recentTickets);
        }

        public IActionResult Index()
        {
            return View(_mockTickets);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                ticket.Id = _mockTickets.Max(t => t.Id) + 1;
                ticket.CreatedDate = DateTime.Now;
                ticket.Status = TicketStatus.Open;
                ticket.UserId = "User1";
                _mockTickets.Add(ticket);
                return RedirectToAction(nameof(Index));
            }
            return View(ticket);
        }

        public IActionResult Details(int id)
        {
            var ticket = _mockTickets.FirstOrDefault(t => t.Id == id);
            if (ticket == null) return NotFound();

            // Mock comments
            ticket.Comments = new List<Comment>
            {
                new Comment { Id = 1, Message = "We are looking into it.", CreatedBy = "Agent Smith", CreatedDate = DateTime.Now.AddHours(-1), TicketId = id },
                new Comment { Id = 2, Message = "Please provide more details.", CreatedBy = "Agent Smith", CreatedDate = DateTime.Now.AddMinutes(-30), TicketId = id }
            };

            return View(ticket);
        }
    }
}
