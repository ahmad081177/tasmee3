# Quran Listening Management System

A web application for teachers to record, view, edit, and export listening (recitation) sessions of students memorizing the Qur'an.

## Project Overview

This system enables teachers to track and manage student Quran recitation progress through systematic recording of listening sessions, error tracking, and progress reporting with Arabic language support and RTL (Right-to-Left) layout.

## Technology Stack

- **Framework**: .NET 8
- **Frontend**: Blazor Server
- **Database**: SQL Server (On-Premise)
- **ORM**: Entity Framework Core 8.0
- **Authentication**: ASP.NET Core Identity
- **UI Framework**: MudBlazor 8.13 (RTL-ready)
- **Architecture**: Clean Architecture / N-Layer Pattern

## Project Structure

```
QuranListeningApp/
├── QuranListeningApp.Domain/          # Domain entities, enums, interfaces
├── QuranListeningApp.Infrastructure/  # EF Core, DbContext, repositories
├── QuranListeningApp.Application/     # Services, business logic
└── QuranListeningApp.Web/             # Blazor Server UI
```

### Project Dependencies

- **Domain**: No dependencies (core business entities)
- **Infrastructure**: → Domain (data access)
- **Application**: → Domain, Infrastructure (business logic)
- **Web**: → Application (presentation layer)

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (2016 or later)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/)
- [SQL Server Management Studio (SSMS)](https://aka.ms/ssmsfullsetup) or Azure Data Studio

## Getting Started

### 1. Clone the Repository

```bash
git clone <repository-url>
cd qurann/src
```

### 2. Restore NuGet Packages

```bash
dotnet restore QuranListeningApp.sln
```

### 3. Configure Database Connection

Update the connection string in `QuranListeningApp.Web/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=QuranListeningDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Replace `YOUR_SERVER_NAME` with your SQL Server instance name (e.g., `localhost`, `.\SQLEXPRESS`, or remote server address).

### 4. Run Database Migrations

```bash
# Navigate to the src directory
cd src

# Add initial migration (if not already created)
dotnet ef migrations add InitialCreate --project QuranListeningApp.Infrastructure --startup-project QuranListeningApp.Web

# Apply migrations to database
dotnet ef database update --project QuranListeningApp.Infrastructure --startup-project QuranListeningApp.Web
```

### 5. Build the Solution

```bash
dotnet build QuranListeningApp.sln
```

### 6. Run the Application

```bash
dotnet run --project QuranListeningApp.Web
```

Or press **F5** in Visual Studio to run with debugging.

The application will be available at:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`

## User Roles

### Administrator
- Manage teacher and student accounts
- View all listening records across all teachers and students
- Access system-wide reports and analytics
- Configure system settings

### Teacher
- Secure login to personal account
- View all students (students are a pool, not assigned)
- Record new listening sessions for any student
- Edit and delete only their own recorded sessions
- Export student progress reports
- Search and filter students

### Student
- Secure login with personal credentials
- View personal recitation history and progress (read-only)
- View session details including dates, surahs, ayahs, errors, and teacher notes
- Filter sessions (passed/completed only or all sessions)

## Key Features

### Core Functionality
- ✅ Arabic language support with RTL layout
- ✅ Mobile-first responsive design
- ✅ Role-based access control (Admin, Teacher, Student)
- ✅ Single Users table for all user types (optimized design)
- ✅ Session recording with Surah/Ayah validation
- ✅ Error tracking (Major and Minor errors)
- ✅ Audit logging for sensitive operations

### Planned Features (See dev-tasks.md)
- [ ] Student and teacher management (CRUD)
- [ ] Listening session recording, editing, deleting
- [ ] Surah/Ayah range validation
- [ ] Progress reports and exports (Excel, PDF)
- [ ] Dashboard for each role
- [ ] Search and filtering capabilities

## Database Schema

### Main Tables

#### Users
Single table for all users (Admin, Teacher, Student):
- Personal information (name, national ID, phone, email)
- Authentication (username, password hash)
- Role designation
- Grade level (for students only)

#### ListeningSessions
Records of Quran recitation sessions:
- Student and teacher references
- Surah/Ayah range (From/To)
- Error counts (major/minor)
- Completion status
- Teacher notes

#### SurahReference (Lookup)
Static data for all 114 Surahs with Arabic names and Ayah counts.

#### AuditLog
Tracks all Create, Update, Delete operations on critical entities.

## Development

### Project Development Tasks

See `dev-tasks.md` for a comprehensive list of development tasks organized by phase:
1. Foundation & Setup
2. Admin Features
3. Teacher Features
4. Student Features
5. Reporting & Export
6. Testing & Refinement
7. Deployment

### Coding Standards

- Follow C# naming conventions
- Use async/await for database operations
- Implement proper error handling and logging
- Write XML comments for public methods
- Use dependency injection
- Keep methods small and focused (SRP)

### Running Tests

```bash
# Run all tests
dotnet test

# Run with code coverage
dotnet test /p:CollectCoverage=true
```

## Deployment

The application is designed to be deployed on:
- **Web Server**: Windows Server (IIS) or Linux (Kestrel)
- **Database**: SQL Server (on-premise or remote)

See `plan.md` section 10 for detailed deployment instructions.

## Documentation

- **plan.md**: Comprehensive development plan with architecture, database design, and implementation details
- **dev-tasks.md**: Detailed development tasks broken down by phase with checkboxes
- **req.md**: Original business requirements document

## Security

- Passwords hashed using ASP.NET Core Identity (PBKDF2)
- Role-based authorization on all endpoints
- SQL injection prevention (EF Core parameterized queries)
- XSS prevention (Blazor automatic encoding)
- HTTPS enforcement
- Audit logging for sensitive operations

## Arabic & RTL Support

- MudBlazor with built-in RTL support
- Arabic fonts: Cairo, Tajawal, or Amiri from Google Fonts
- All UI text in Arabic
- Right-to-left layout throughout
- Date/time and number formatting in Arabic or localized format

## Support & Contact

For questions or issues, please contact the development team.

## License

[Specify your license here]

## Acknowledgments

- Quran data sourced from Quran.com or similar trusted sources
- MudBlazor for the excellent RTL-ready UI framework

---

**Last Updated**: October 20, 2025
