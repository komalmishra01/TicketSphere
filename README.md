# TicketSphere - Service Request & Ticketing System

A professional-grade ASP.NET Core MVC application for managing service requests and tickets with role-based access (Customer, Agent, Admin).

## 📋 Project Overview

TicketSphere is a complete ticketing system that allows:

- **Users/Customers**: Create and track service requests
- **Agents/Staff**: View, assign, and resolve tickets
- **Admins**: Manage the entire system with full control

## 🎯 Current Implementation Status

### ✅ COMPLETED - User Panel (T+4 to T+16)

- [x] User Registration & Login
- [x] Dashboard with Ticket Statistics
- [x] Create Ticket (with Priority)
- [x] My Tickets (with Search & Filters)
- [x] Ticket Details Page
- [x] Comments & Updates System
- [x] Timeline/Activity History
- [x] User Profile Management
- [x] Rating & Feedback System

### 📋 TODO - Agent & Admin Panels

- [ ] Agent Dashboard & Assigned Tickets
- [ ] Admin Dashboard & System Management
- [ ] File Attachment Support
- [ ] Email Notifications
- [ ] Advanced Reporting

## 🛠 Tech Stack

- **Framework**: .NET 8.0 ASP.NET Core MVC
- **Database**: SQL Server (LocalDB in development)
- **ORM**: Entity Framework Core
- **Frontend**: Bootstrap 5, Responsive Design
- **Authentication**: Session-based (Cookie)
- **Security**: BCrypt Password Hashing

## 📁 Project Structure

```
TicketSphere/
├── Controllers/
│   ├── AccountController.cs      (Login, Register, Profile)
│   ├── DashboardController.cs    (User Tickets)
│   ├── AdminController.cs        (Admin Panel)
│   └── AgentController.cs        (Agent Panel)
├── Models/
│   ├── User.cs                   (User model with roles)
│   ├── Ticket.cs                 (Ticket model)
│   ├── Comment.cs                (Comments model)
│   └── ErrorViewModel.cs
├── Views/
│   ├── Account/
│   │   ├── Login.cshtml
│   │   ├── Register.cshtml
│   │   └── Profile.cshtml
│   ├── Dashboard/
│   │   ├── Dashboard.cshtml      (Main Dashboard)
│   │   ├── CreateTicket.cshtml   (Create Form)
│   │   ├── MyTickets.cshtml      (Ticket List)
│   │   └── TicketDetails.cshtml  (Full Ticket View)
│   ├── Shared/
│   │   ├── _Layout.cshtml        (Main Layout)
│   │   └── _ValidationScriptsPartial.cshtml
│   └── Home/
├── Data/
│   └── ApplicationDbContext.cs   (EF Core Context)
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── lib/
├── appsettings.json
└── Program.cs
```

## 🚀 Installation & Setup

### Prerequisites

- **.NET 8.0 SDK** - Download from [dotnet.microsoft.com](https://dotnet.microsoft.com/download)
- **Visual Studio 2022** or **VS Code** with C# extension
- **SQL Server** (LocalDB comes with Visual Studio)

### Step 1: Restore NuGet Packages

Open PowerShell in the project directory and run:

```powershell
dotnet restore
```

This installs all required dependencies:

- Microsoft.EntityFrameworkCore
- Microsoft.EntityFrameworkCore.SqlServer
- Microsoft.EntityFrameworkCore.Tools
- BCrypt.Net-Next

### Step 2: Create & Apply Database Migrations

Run these commands in PowerShell (in the project directory):

```powershell
# Create initial migration
dotnet ef migrations add InitialCreate

# Apply migration to create database
dotnet ef database update
```

This will:

- Create the `TicketSphereDb` database in LocalDB
- Create tables: Users, Tickets, Comments
- Add default Admin user

**Default Admin Account:**

- Email: `admin@ticketsphere.com`
- Password: `admin123`

### Step 3: Build the Project

```powershell
dotnet build
```

Verify there are no compilation errors.

## ▶️ Running the Application

### Option A: Using PowerShell

```powershell
dotnet run
```

### Option B: Using Visual Studio

1. Open the `.sln` file in Visual Studio
2. Press `F5` to start debugging
3. Application will open at `https://localhost:5001`

### Option C: Using VS Code

1. Install C# extension and launch configuration
2. Press `F5` to debug
3. Application opens at configured port

## 🔐 Login & Testing

### Admin Access

- **URL**: https://localhost:5001/Account/Login
- **Email**: admin@ticketsphere.com
- **Password**: admin123

### Create New User Account

1. Click "Create Account" on login page
2. Fill in: Full Name, Email, Password
3. Click "Create Account"
4. Login with your new credentials

## 📖 User Panel Features

### 🏠 Dashboard

- View ticket statistics (Total, Open, In Progress, Closed)
- Quick access to recent tickets
- Create new ticket button

### ➕ Create Ticket

- **Title**: Brief description (max 200 chars)
- **Description**: Detailed explanation
- **Priority**: Low, Medium, High

### 📋 My Tickets

- **Search**: Find tickets by title
- **Filter**: By Status (Open/In Progress/Closed)
- **Filter**: By Priority (Low/Medium/High)
- **View Details**: Click to open full ticket

### 🔍 Ticket Details

- **Full Information**: Title, Description, Priority, Status
- **Agent Assigned**: See which agent is handling
- **Comments**: Add & view updates
- **Timeline**: Track ticket lifecycle
- **Rating**: Rate closed tickets (1-5 stars)
- **Feedback**: Leave comments on resolved tickets

### 👤 Profile

- Update Full Name
- Change Password
- View Login History

## 🎨 UI Features

### Design Elements

- **Sidebar Navigation**: Fixed left sidebar with menu
- **Responsive Layout**: Works on mobile, tablet, desktop
- **Color Coded Badges**:
  - Blue: Open tickets
  - Orange: In Progress
  - Green: Closed
- **Priority Indicators**:
  - Red: High Priority
  - Yellow: Medium Priority
  - Blue: Low Priority

### Icons

- Uses **Font Awesome 6.4** for professional icons
- Bootstrap 5 for responsive components
- Smooth transitions and hover effects

## 🔄 User Workflow

```
1. User Registers → 2. User Logs In → 3. Views Dashboard
   ↓
4. Creates Ticket → 5. Views My Tickets → 6. Opens Ticket Details
   ↓
7. Adds Comments → 8. Tracks Status → 9. Rates & Provides Feedback
```

## 📊 Database Schema

### Users Table

```sql
- UserId (PK)
- FullName
- Email (Unique)
- PasswordHash (BCrypt)
- Role (User/Agent/Admin)
- CreatedDate
- LastLoginDate
- IsActive
```

### Tickets Table

```sql
- TicketId (PK)
- Title
- Description
- Priority (Low/Medium/High)
- Status (Open/In Progress/Closed)
- UserId (FK) - Creator
- AssignedAgentId (FK) - Handler
- CreatedDate
- UpdatedDate
- ClosedDate
- Rating (0-5)
- Feedback
- Attachments (JSON)
```

### Comments Table

```sql
- CommentId (PK)
- Message
- TicketId (FK)
- UserId (FK)
- CreatedDate
```

## 🔐 Security Features

✅ **Password Security**

- Hashed with BCrypt (salted)
- Never stored in plain text

✅ **Session Management**

- 30-minute idle timeout
- Secure session cookies
- Auto-logout on inactivity

✅ **Authorization**

- Role-based access control
- Users can only view own tickets
- Agents/Admins have restricted access

✅ **Data Validation**

- Server-side validation
- Type checking on all inputs
- SQL injection prevention (EF Core)

## 📝 API Endpoints (Current)

| Method | Path                     | Purpose        |
| ------ | ------------------------ | -------------- |
| GET    | /Account/Login           | Login page     |
| POST   | /Account/Login           | Process login  |
| GET    | /Account/Register        | Register page  |
| POST   | /Account/Register        | Create user    |
| GET    | /Account/Logout          | Sign out       |
| GET    | /Account/Profile         | User profile   |
| POST   | /Account/UpdateProfile   | Update info    |
| GET    | /Dashboard/Dashboard     | Main dashboard |
| GET    | /Dashboard/CreateTicket  | Create form    |
| POST   | /Dashboard/CreateTicket  | Save ticket    |
| GET    | /Dashboard/MyTickets     | List tickets   |
| GET    | /Dashboard/TicketDetails | View ticket    |
| POST   | /Dashboard/AddComment    | Post comment   |
| POST   | /Dashboard/RateTicket    | Submit rating  |

## 🐛 Troubleshooting

### Database Connection Issues

```
Error: "Cannot connect to database"
Solution:
- Ensure SQL Server LocalDB is installed
- Run: dotnet ef database update
```

### Migration Errors

```
Error: "Pending migrations"
Solution:
- dotnet ef database update --force
- Or delete database and re-run migration
```

### Port Already in Use

```
Error: "Address already in use"
Solution:
- Change port in launchSettings.json
- Or kill process using port 5001
```

### Session Not Persisting

```
Error: "Logout after each page"
Solution:
- Clear browser cookies
- Check Program.cs has AddSession()
```

## 📚 Key Files to Understand

1. **Program.cs** - Application configuration & startup
2. **\_Layout.cshtml** - Main layout with sidebar
3. **ApplicationDbContext.cs** - Database configuration
4. **AccountController.cs** - Auth logic
5. **DashboardController.cs** - Ticket operations

## 🎓 Learning Path

For developers new to this project:

1. Understand Models (User, Ticket, Comment)
2. Review AccountController (Authentication)
3. Study DashboardController (Business Logic)
4. Explore Views (UI/UX)
5. Check Program.cs (Configuration)

## 📅 Development Roadmap

### Phase 2 (Future)

- [ ] Agent panel improvements
- [ ] Advanced admin dashboard
- [ ] File attachment system
- [ ] Email notifications
- [ ] Report generation
- [ ] SLA tracking
- [ ] Knowledge base

### Phase 3 (Future)

- [ ] Mobile app (React Native)
- [ ] API wrapper (REST)
- [ ] Real-time updates (SignalR)
- [ ] Analytics & charts
- [ ] Automation & workflows

## 📞 Support & Contact

For questions or issues:

1. Check the troubleshooting section
2. Review code comments in Models & Controllers
3. Check appsettings.json configuration
4. Verify database connection string

## 📄 License

This project is created for hackathon purposes. All rights reserved.

---

**Created**: March 27, 2026  
**Version**: 1.0 - User Panel MVP  
**Status**: ✅ Ready for Testing
