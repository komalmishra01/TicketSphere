# 🚀 TicketSphere - QUICK START GUIDE

## ⚡ Get Up & Running in 5 Minutes

### Step 1: Prerequisites Check

Ensure you have installed:

- **.NET 8.0 SDK** → https://dotnet.microsoft.com/download
- **Visual Studio 2022** or **VS Code** (with C# extension)
- **SQL Server Express/LocalDB** (comes with Visual Studio)

### Step 2: Restore & Build (1 minute)

Open PowerShell in the project directory:

```powershell
cd d:\MCA\SEM_2\Hackathon_2026\TicketSphere
dotnet restore
dotnet build
```

✅ **Output should show: "Build succeeded"**

### Step 3: Run the Application (1 minute)

```powershell
dotnet run
```

⏳ **Wait for**: `Application started. Press Ctrl+C to shut down.`

🌐 **Open browser**: https://localhost:5001/Account/Login

### Step 4: Login with Demo Account

**Email**: `admin@ticketsphere.com`  
**Password**: `admin123`

✅ **You should see the Admin Dashboard**

---

## 🎯 What to Test (3 steps)

### 1️⃣ Create a New User Account

1. Click "Logout" (top right)
2. Click "Create Account"
3. Fill in:
   - Full Name: `John Doe`
   - Email: `john@example.com`
   - Password: `Test1234`
   - Confirm: `Test1234`
4. Click "Create Account"
5. Login with new credentials

### 2️⃣ Create Your First Ticket

1. Click **"Create Ticket"** in sidebar
2. Fill in:
   - **Title**: "Website not loading"
   - **Description**: "The website is showing 500 error for past 2 hours"
   - **Priority**: "High"
3. Click **"Create Ticket"**
4. See success message ✅

### 3️⃣ View & Manage Tickets

1. Click **"My Tickets"** in sidebar
2. See your ticket in the table
3. Click **"View Details"**
4. Add a comment: "Still waiting for response"
5. Click **"Post Comment"**
6. See your comment appear

---

## 📊 Application Routes

| Page           | URL                                             |
| -------------- | ----------------------------------------------- |
| Login          | http://localhost:5001/Account/Login             |
| Register       | http://localhost:5001/Account/Register          |
| Dashboard      | http://localhost:5001/Dashboard/Dashboard       |
| Create Ticket  | http://localhost:5001/Dashboard/CreateTicket    |
| My Tickets     | http://localhost:5001/Dashboard/MyTickets       |
| Ticket Details | http://localhost:5001/Dashboard/TicketDetails/1 |
| Profile        | http://localhost:5001/Account/Profile           |

---

## ⚠️ Troubleshooting

### Problem: "Port 5001 already in use"

```powershell
# Kill existing process
netstat -ano | findstr :5001
taskkill /PID <PID> /F

# Then run dotnet run again
```

### Problem: Database doesn't exist

✅ **Good news**: The database is created automatically on first run!

- Check `%LOCALAPPDATA%\Microsoft\Microsoft SQL Server Local DB\Instances`
- Or use SQL Server Management Studio to verify `TicketSphereDb` exists

### Problem: "dotnet not found"

```powershell
# Add .NET to PATH and restart PowerShell
[Environment]::SetEnvironmentVariable("PATH", $env:PATH + ";C:\Program Files\dotnet", "User")
```

### Problem: CSS/images not loading

1. Stop the app (Ctrl+C)
2. Run: `dotnet clean`
3. Run: `dotnet build`
4. Run: `dotnet run`

---

## 🎨 Features You Can Test

### User Panel Features:

✅ Register & Login  
✅ Dashboard with stats  
✅ Create Ticket with Priority  
✅ My Tickets with Search & Filter  
✅ Ticket Details with full info  
✅ Comments System  
✅ Timeline/Activity  
✅ Profile Management  
✅ Rate Closed Tickets

---

## 📱 UI Highlights

- **Responsive Sidebar**: Fixed navigation on left
- **Color-Coded Status**: Open (Blue) | In Progress (Orange) | Closed (Green)
- **Priority Badges**: High (Red) | Medium (Yellow) | Low (Blue)
- **Font Awesome Icons**: Professional icons throughout
- **Bootstrap 5**: Beautiful, mobile-friendly design

---

## 🔑 User Accounts

### Admin Account (Pre-created):

- **Email**: admin@ticketsphere.com
- **Password**: admin123
- **Role**: Admin (Full system access)

### Create Test Account:

- Go to Register page
- Fill in any details
- Automatically assigned "User" role
- Can create/manage own tickets

---

## 💾 Database Seeding

When you run the app for the first time:

1. ✅ `TicketSphereDb` is created automatically
2. ✅ All tables (Users, Tickets, Comments) are created
3. ✅ Default Admin user is inserted
4. ✅ Database is ready to use

**No manual migration needed!** Everything happens in `Program.cs`:

```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();  // <-- Creates DB on startup
}
```

---

## 🎯 Next Steps

### To Add More Features:

1. Stop the app (Ctrl+C)
2. Make changes to Controllers/Views
3. Run: `dotnet run`
4. Changes appear immediately

### For Agent/Admin Panels:

- See the sidebar - routes are ready
- Controllers exist at:
  - `Controllers/AgentController.cs`
  - `Controllers/AdminController.cs`
- Next phase can implement these panels

---

## 📚 File Structure (Important)

```
TicketingSystem-DotNetMVC/
├── Controllers/
│   ├── AccountController.cs    ← Login/Register logic
│   ├── DashboardController.cs  ← Ticket management
│   ├── AdminController.cs      ← Admin routes
│   └── AgentController.cs      ← Agent routes
├── Models/
│   ├── User.cs          ← User entity
│   ├── Ticket.cs        ← Ticket entity
│   └── Comment.cs       ← Comment entity
├── Views/
│   ├── Account/
│   │   ├── Login.cshtml
│   │   ├── Register.cshtml
│   │   └── Profile.cshtml
│   ├── Dashboard/
│   │   ├── Dashboard.cshtml
│   │   ├── CreateTicket.cshtml
│   │   ├── MyTickets.cshtml
│   │   └── TicketDetails.cshtml
│   └── Shared/
│       └── _Layout.cshtml      ← Main layout with sidebar
├── Data/
│   └── ApplicationDbContext.cs ← Database context
├── Program.cs          ← Startup configuration
└── appsettings.json    ← Connection string
```

---

## 🎓 Demo Flow (For Presentation)

When demoing to judges:

1. **Start App**: `dotnet run`
2. **Show Login Page**: Beautiful UI with gradient
3. **Login as Admin**: Show admin access
4. **Create User Account**: Live registration
5. **Create Ticket**: Demonstrate priority system
6. **Show Filters**: Search and filter tickets
7. **Add Comment**: Show real-time updates
8. **View Timeline**: Show activity history
9. **Rate Ticket**: Show feedback system

---

## ⏱️ Time Spent

- ✅ Models & Database: Complete
- ✅ Controllers & Logic: Complete
- ✅ User Panel UI: Complete
- ✅ Authentication: Complete
- 📋 Next: Agent & Admin panels (optional)

---

## 🎉 You're Ready!

Run the application and start testing. Everything is configured and ready to go!

```powershell
dotnet run
# Then visit: https://localhost:5001/Account/Login
```

**Questions?** Check the README.md for detailed documentation.

---

**Created**: March 27, 2026  
**Status**: ✅ MVP Ready for Testing & Demo  
**Version**: 1.0 - User Panel Complete
