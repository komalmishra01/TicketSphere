# 🎟️ TicketSphere - Role-Based Architecture

## Overview

The application now has **complete role-based separation** with distinct panels for Users, Agents, and Administrators.

---

## 📋 Architecture Structure

### 1️⃣ **User Panel** 👤 (Customer)

**Color Theme:** Blue (#0d6efd)  
**Layout:** `_Layout.User.cshtml`

#### Pages:

```
Dashboard
└── Create Ticket
└── My Tickets
└── Profile
└── Logout
```

#### Features:

- ✅ Create new tickets (Title, Description, Priority)
- ✅ View personal tickets with status tracking
- ✅ View ticket details and add comments
- ✅ Rate and feedback on closed tickets
- ✅ Update profile and change password

#### Controllers:

- **DashboardController** (`/Dashboard/`) - Handles user workflows

#### Views:

- `Views/Dashboard/Dashboard.cshtml` - Dashboard with stats
- `Views/Dashboard/CreateTicket.cshtml` - Form to create ticket
- `Views/Dashboard/MyTickets.cshtml` - List user's tickets with filters
- `Views/Dashboard/TicketDetails.cshtml` - View ticket details & comments
- `Views/Account/Profile.cshtml` - User profile settings

#### Dashboard Stats:

- 📊 Total Tickets
- 🔵 Open Tickets
- ⏳ In Progress Tickets
- ✅ Closed Tickets

---

### 2️⃣ **Agent Panel** 🧑‍💼 (Support Staff)

**Color Theme:** Orange (#ff6b35)  
**Layout:** `_Layout.Agent.cshtml`

#### Pages:

```
Dashboard
└── Assigned Tickets
└── Profile
└── Logout
```

#### Features:

- ✅ View tickets assigned to agent
- ✅ Update ticket status (Open → In Progress → Closed)
- ✅ Change ticket priority
- ✅ Add comments to tickets
- ✅ View customer details

#### Controllers:

- **AgentController** (`/Agent/`) - Handles agent ticket management

#### Views:

- `Views/Agent/Dashboard.cshtml` - Dashboard with agent stats
- `Views/Agent/AssignedTickets.cshtml` - Table of assigned tickets
- `Views/Agent/TicketDetails.cshtml` - Ticket details with update interface
- `Views/Account/Profile.cshtml` - Agent profile

#### Dashboard Stats:

- 📌 Assigned Tickets
- ⏳ In Progress Count
- ✅ Resolved Count

---

### 3️⃣ **Admin Panel** 👑 (Administrator)

**Color Theme:** Purple (#7c3aed)  
**Layout:** `_Layout.Admin.cshtml`

#### Pages:

```
Dashboard
├── All Tickets
├── Manage Users
├── Manage Agents
├── Reports
└── Profile
└── Logout
```

#### Features:

**Ticket Management:**

- ✅ View all system tickets (no user filter)
- ✅ Assign tickets to agents
- ✅ Update ticket status and priority
- ✅ Add comments to any ticket

**User Management:**

- ✅ Add/Edit/Delete users
- ✅ View all registered users
- ✅ Assign roles to users
- ✅ Activate/Deactivate accounts

**Agent Management:**

- ✅ Add/Edit agent staff
- ✅ View agent performance metrics
- ✅ Assign workload to agents

**Analytics:**

- ✅ View system statistics
- ✅ Generate reports
- ✅ View ticket distribution
- ✅ View agent performance

#### Controllers:

- **AdminController** (`/Admin/`) - Handles all admin operations

#### Views:

- `Views/Admin/Dashboard.cshtml` - Admin dashboard with comprehensive stats
- `Views/Admin/AllTickets.cshtml` - All tickets with agent assignment
- `Views/Admin/ManageUsers.cshtml` - User management interface
- `Views/Admin/ManageAgents.cshtml` - Agent management
- `Views/Admin/Reports.cshtml` - System reports and analytics
- `Views/Account/Profile.cshtml` - Admin profile

#### Dashboard Stats:

- 📊 Total Tickets (All)
- 🔵 Open Tickets
- ⏳ In Progress Tickets
- ✅ Closed Tickets
- 👥 Total Users
- 👨‍💼 Total Agents

---

## 🔐 Default Credentials

Use these accounts to test each panel:

| Role      | Email                  | Password | Dashboard            |
| --------- | ---------------------- | -------- | -------------------- |
| **User**  | user@ticketsphere.com  | user123  | User Panel (Blue)    |
| **Agent** | agent@ticketsphere.com | agent123 | Agent Panel (Orange) |
| **Admin** | admin@ticketsphere.com | admin123 | Admin Panel (Purple) |

---

## 🎨 Layout System

### How Layout Selection Works:

The application automatically selects the correct layout based on the **controller name**:

```csharp
// Views/_ViewStart.cshtml
var controller = ViewContext.RouteData.Values["controller"]?.ToString() ?? "";

if (controller == "Dashboard")
    Layout = "_Layout.User";          // Blue theme, user menu
else if (controller == "Agent")
    Layout = "_Layout.Agent";         // Orange theme, agent menu
else if (controller == "Admin")
    Layout = "_Layout.Admin";         // Purple theme, admin menu
else
    Layout = "_Layout";               // Default (for Account controller)
```

### Layout Files:

1. **`_Layout.User.cshtml`** - User-only sidebar (Dashboard, Create Ticket, My Tickets, Profile)
2. **`_Layout.Agent.cshtml`** - Agent-only sidebar (Dashboard, Assigned Tickets, Profile)
3. **`_Layout.Admin.cshtml`** - Admin-only sidebar (Dashboard, All Tickets, Manage Users, Manage Agents, Reports, Profile)
4. **`_Layout.cshtml`** - Default fallback for Account pages (Login, Register)

### Theme Colors:

| Role  | Primary Color    | Sidebar Hover | Text    |
| ----- | ---------------- | ------------- | ------- |
| User  | #0d6efd (Blue)   | #eef4ff       | #1f2937 |
| Agent | #ff6b35 (Orange) | #ffeeea       | #1f2937 |
| Admin | #7c3aed (Purple) | #f3e8ff       | #1f2937 |

---

## 🔑 Key Features

### ✅ Automatic Layout Selection

- No need to manually specify layouts in each view
- Controller name automatically determines which sidebar to show

### ✅ Role-Based Access Control

Each controller checks user role:

```csharp
var userRole = HttpContext.Session.GetString("UserRole");
if (userRole != "Admin")
    return Unauthorized();
```

### ✅ Session Management

After login, user info is stored:

- `UserId` - Unique user identifier
- `UserName` - Full name display
- `UserEmail` - Email address
- `UserRole` - User role (User/Agent/Admin)

### ✅ Auto-Redirect After Login

Based on role:

- **User** → `/Dashboard/Dashboard`
- **Agent** → `/Agent/Dashboard`
- **Admin** → `/Admin/Dashboard`

### ✅ Separate Sidebars

Each role sees only their relevant menu items; no cross-panel confusion

---

## 📁 Project Structure

```
Views/
├── Account/                    # Auth pages (Login, Register, Profile)
│   ├── Login.cshtml           # Public page (Layout = null)
│   ├── Register.cshtml        # Public page (Layout = null)
│   └── Profile.cshtml         # Uses role-specific layout
│
├── Dashboard/                  # USER PANEL VIEWS
│   ├── Dashboard.cshtml
│   ├── CreateTicket.cshtml
│   ├── MyTickets.cshtml
│   └── TicketDetails.cshtml
│
├── Agent/                      # AGENT PANEL VIEWS
│   ├── Dashboard.cshtml
│   ├── AssignedTickets.cshtml
│   └── TicketDetails.cshtml
│
├── Admin/                      # ADMIN PANEL VIEWS
│   ├── Dashboard.cshtml
│   ├── AllTickets.cshtml
│   ├── ManageUsers.cshtml
│   ├── ManageAgents.cshtml
│   └── Reports.cshtml
│
├── Shared/
│   ├── _Layout.cshtml          # Default layout (fallback)
│   ├── _Layout.User.cshtml     # User panel layout (BLUE)
│   ├── _Layout.Agent.cshtml    # Agent panel layout (ORANGE)
│   ├── _Layout.Admin.cshtml    # Admin panel layout (PURPLE)
│   ├── _ViewStart.cshtml       # Layout selector logic
│   ├── _ViewImports.cshtml
│   ├── Error.cshtml
│   └── _ValidationScriptsPartial.cshtml
│
└── (Other shared views)

Controllers/
├── AccountController.cs        # Login, Register, Profile, Logout
├── DashboardController.cs      # User panel operations
├── AgentController.cs          # Agent panel operations
└── AdminController.cs          # Admin panel operations

Models/
├── User.cs                     # User/Agent/Admin accounts
├── Ticket.cs                   # Tickets
├── Comment.cs                  # Comments on tickets
└── (ViewModels)

Program.cs                       # Auto-seeds 3 default users on startup
```

---

## 🚀 Running the Application

```bash
dotnet build
dotnet run
```

Application runs on: **http://localhost:5039**

1. Go to login page
2. Enter credentials from the table above
3. Automatically redirected to appropriate dashboard:
   - User: **Blue panel** with ticket management
   - Agent: **Orange panel** with assigned tickets
   - Admin: **Purple panel** with full system control

---

## 📊 Database Schema

### Users Table

- UserId (PK)
- FullName
- Email (Unique)
- PasswordHash
- **Role** (User | Agent | Admin)
- IsActive
- CreatedDate
- LastLoginDate

### Tickets Table

- TicketId (PK)
- UserId (FK) - Who created the ticket
- Title
- Description
- Status (Open | In Progress | Closed)
- Priority (Low | Medium | High)
- AssignedAgentId (FK) - Assigned agent (nullable)
- CreatedDate
- UpdatedDate

### Comments Table

- CommentId (PK)
- TicketId (FK)
- UserId (FK)
- CommentText
- CreatedDate

---

## 🎯 Summary

| Feature                 | User    | Agent     | Admin     |
| ----------------------- | ------- | --------- | --------- |
| Create Tickets          | ✅      | ❌        | ❌        |
| View Own Tickets        | ✅      | ❌        | ❌        |
| View Assigned Tickets   | ❌      | ✅        | ✅        |
| Update Ticket Status    | ❌      | ✅        | ✅        |
| Assign Tickets to Agent | ❌      | ❌        | ✅        |
| Manage Users            | ❌      | ❌        | ✅        |
| Manage Agents           | ❌      | ❌        | ✅        |
| View Reports            | ❌      | ❌        | ✅        |
| Panel Color             | 🔵 Blue | 🟠 Orange | 🟣 Purple |

---

## ✨ Next Steps (Optional Enhancements)

- [ ] API endpoints for frontend frameworks
- [ ] Email notifications on ticket updates
- [ ] Advanced search and filtering
- [ ] Attachment uploads
- [ ] Ticket escalation system
- [ ] Performance analytics dashboard
- [ ] Bulk user import
- [ ] Audit logging
