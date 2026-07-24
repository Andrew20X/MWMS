<div align="center">
  <h1> Measuresoft Workforce Management System (MWMS) </h1>
  <p><span>An Enterprise-Grade Attendance and Workforce Management Solution</span></p>

  [![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
  [![React](https://img.shields.io/badge/React-18-61DAFB?style=for-the-badge&logo=react&logoColor=black)](https://reactjs.org/)
  [![TypeScript](https://img.shields.io/badge/TypeScript-5.0-3178C6?style=for-the-badge&logo=typescript&logoColor=white)](https://www.typescriptlang.org/)
  [![MUI](https://img.shields.io/badge/Material--UI-007FFF?style=for-the-badge&logo=mui&logoColor=white)](https://mui.com/)
  [![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/en-us/sql-server)
</div>

<br />

##  Overview

**MWMS** is a comprehensive, full-stack workforce management application developed for Measuresoft Oil Services. It is designed to streamline employee tracking, leave requests, attendance logs, and internal communications, effectively replacing legacy paperwork and spreadsheet-based workflows. 

The system provides a robust administrative dashboard alongside dedicated portals for both Managers and Employees, ensuring strict access control and role-based data isolation.

##  Key Features

-  **Secure Role-Based Authentication:** JWT-based session management isolating `Admin`, `Manager`, and `Employee` access.
-  **Employee Directory & Configuration:** Admins can manage staff profiles, reset passwords, and set precise **Annual & Emergency Leave Balances**.
-  **Job Positions Management:** Comprehensive CRUD operations for organizational positions, accurately mapping titles across the workforce dashboard.
-  **Advanced Leave & Time Off:** Employees can submit RDO/EDO requests. Managers and Admins can approve, reject, and leave feedback.
-  **Overtime & Corrections Tracking:** Digitized request systems for missed punches and logged overtime.
-  **Real-Time Dashboards & Analytics:** "My Insights" portal provides real-time statistics on leave utilization, attendance trends, and pending tasks.
-  **Company Announcements:** Broadcast important information to the entire workforce directly to their dashboard.
-  **Hardware Integration (ZKTeco):** Designed to bridge and ingest raw attendance punch logs from enterprise fingerprint/biometric devices.

##  Recent Updates

-  **Salary Deductions Management:** Implemented hard delete functionality for salary deductions with cascading cleanup. Added a "Delete All Deductions" bulk action for Admins.
-  **Automated Email Notifications:** Enhanced `EmailService` to dispatch automated, styled HTML email alerts to employees (e.g., unexpected leaves, deductions) and managers for workflow approvals.
-  **Dynamic Role Assignment:** The system now automatically grants the `Manager` role at login for any employee who has active subordinates, ensuring immediate access to the team management portal.
-  **CI/CD Pipeline:** Configured GitHub Actions (`deploy.yml`) to automatically build and deploy the React frontend to GitHub Pages on every push to the `main` branch.
-  **Database Integrity & Fixes:** Cleaned up duplicate synchronization accounts to resolve email uniqueness constraints and fortified cascading deletes for attendance records.

##  Technology Stack

This application follows **Clean Architecture** principles, enforcing separation of concerns across Domain, Application, Infrastructure, and Presentation layers.

**Backend (.NET Web API)**
- **Framework:** .NET 8 ASP.NET Core
- **Database ORM:** Entity Framework Core (EF Core)
- **Database:** Microsoft SQL Server
- **Security:** ASP.NET Core Identity & JWT Bearer Tokens

**Frontend (React SPA)**
- **Framework:** React (Vite environment)
- **Language:** TypeScript
- **Styling & UI Components:** Material-UI (MUI v5)
- **Icons:** Lucide-React
- **State/Routing:** React Router v6, Axios

---

##  Getting Started

Follow these instructions to set up the project on your local machine for development and testing purposes.

### Prerequisites

- [Node.js](https://nodejs.org/en/) (v18 or higher)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Microsoft SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (LocalDB or Developer Edition)

### 1. Database Setup
The backend utilizes Entity Framework Core Migrations to automatically build the database schema and seed initial data.
1. Ensure your SQL Server instance is running.
2. Update the `ConnectionStrings:DefaultConnection` inside `src/MWMS.API/appsettings.json` if your SQL Server credentials differ.

### 2. Running the Backend API
1. Open a terminal and navigate to the project root.
2. Run the application:
   ```bash
   dotnet run --project src/MWMS.API/MWMS.API.csproj
   ```
3. The API will start (typically on `http://localhost:5222`). EF Core will automatically create the database and seed it with default accounts upon startup.

### 3. Running the Frontend App
1. Open a *new* terminal and navigate to the frontend directory:
   ```bash
   cd src/frontend
   ```
2. Install the necessary NPM dependencies:
   ```bash
   npm install
   ```
3. Start the Vite development server:
   ```bash
   npm run dev
   ```
4. Access the application in your browser at the provided `http://localhost:XXXX` address.

---



---

<div align="center">
  <i>Built with love by Andrew Raafat for Measuresoft Oil Services (Using Clean Architecture and Modern Web Standards).</i>
</div>
