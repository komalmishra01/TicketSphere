using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketingSystem_DotNetMVC.Data;
using TicketingSystem_DotNetMVC.Models;
using System.ComponentModel.DataAnnotations;

namespace TicketingSystem_DotNetMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(ApplicationDbContext context, ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            if (HttpContext.Session.GetString("UserId") != null)
                return RedirectToAction("Dashboard", "Dashboard");
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("FullName,Email,Password")] RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists
                if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email already registered.");
                    return View(model);
                }

                var user = new User
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    Role = "User"
                };

                _context.Add(user);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Registration successful! Please login.";
                return RedirectToAction(nameof(Login));
            }
            return View(model);
        }

        // GET: Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("UserId") != null)
                return RedirectToAction("Dashboard", "Dashboard");
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind("Email,Password")] LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

                if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                {
                    if (!user.IsActive)
                    {
                        ModelState.AddModelError("", "Your account has been deactivated.");
                        return View(model);
                    }

                    // Store session
                    HttpContext.Session.SetString("UserId", user.UserId.ToString());
                    HttpContext.Session.SetString("UserName", user.FullName);
                    HttpContext.Session.SetString("UserEmail", user.Email);
                    HttpContext.Session.SetString("UserRole", user.Role);

                    // Update last login
                    user.LastLoginDate = DateTime.Now;
                    _context.Update(user);
                    await _context.SaveChangesAsync();

                    if (user.Role == "Admin")
                        return RedirectToAction("Dashboard", "Admin");
                    else if (user.Role == "Agent")
                        return RedirectToAction("Dashboard", "Agent");
                    else
                        return RedirectToAction("Dashboard", "Dashboard");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid email or password.");
                }
            }
            return View(model);
        }

        // GET: Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "You have been logged out successfully.";
            return RedirectToAction(nameof(Login));
        }

        // GET: Account/Profile
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
                return RedirectToAction(nameof(Login));

            var role = HttpContext.Session.GetString("UserRole") ?? "User";
            if (role == "Admin")
                return RedirectToAction("Profile", "Admin");
            if (role == "Agent")
                return RedirectToAction("Profile", "Agent");

            return RedirectToAction("Profile", "Dashboard");
        }

        // POST: Account/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(int userId, string fullName, string? currentPassword, string? newPassword)
        {
            var sessionUserId = HttpContext.Session.GetString("UserId");
            if (sessionUserId != userId.ToString())
                return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            user.FullName = fullName;

            // If new password is provided, verify current password first
            if (!string.IsNullOrEmpty(newPassword))
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

    public class RegisterViewModel
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
