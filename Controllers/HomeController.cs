using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TicketingSystem_DotNetMVC.Models;
using TicketingSystem_DotNetMVC.Services;

namespace TicketingSystem_DotNetMVC.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return RedirectToAction("Dashboard", "Tickets");
    }

    public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> TestEmail([FromServices] IEmailService emailService)
        {
            try 
            {
                await emailService.SendEmailAsync("km208337@gmail.com", "Test Email", "This is a test email from TicketSphere.");
                return Ok("Email sent successfully! Check your inbox.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to send email: {ex.Message}");
            }
        }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
