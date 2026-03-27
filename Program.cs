using TicketingSystem_DotNetMVC.Data;
using TicketingSystem_DotNetMVC.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Apply migrations on startup (with error handling)
try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        // Try to apply pending migrations
        db.Database.Migrate();

        // Ensure default demo accounts exist for each role if configured.
        if (builder.Configuration.GetValue<bool>("SeedDemoData"))
        {
            if (!db.Users.Any(u => u.Email == "admin@ticketsphere.com"))
            {
                db.Users.Add(new User
                {
                    FullName = "Admin User",
                    Email = "admin@ticketsphere.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Role = "Admin",
                    IsActive = true,
                    CreatedDate = DateTime.Now
                });
            }

            if (!db.Users.Any(u => u.Email == "agent@ticketsphere.com"))
            {
                db.Users.Add(new User
                {
                    FullName = "Agent User",
                    Email = "agent@ticketsphere.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("agent123"),
                    Role = "Agent",
                    IsActive = true,
                    CreatedDate = DateTime.Now
                });
            }

            if (!db.Users.Any(u => u.Email == "user@ticketsphere.com"))
            {
                db.Users.Add(new User
                {
                    FullName = "Normal User",
                    Email = "user@ticketsphere.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("user123"),
                    Role = "User",
                    IsActive = true,
                    CreatedDate = DateTime.Now
                });
            }

            db.SaveChanges();
        }
    }
}
catch (Exception ex)
{
    // Log the error but continue running
    Console.WriteLine($"Database initialization error: {ex.Message}");
    Console.WriteLine("Application will continue without database. Please check your connection string.");
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
