# Quran Listening Management System - Workspace Instructions

## Project Overview
Blazor Server web application (.NET 8) for managing Quran recitation listening sessions with Clean Architecture pattern.

## Project Structure
- **QuranListeningApp.Domain**: Entities, enums, interfaces (no dependencies)
- **QuranListeningApp.Infrastructure**: EF Core, DbContext, repositories (depends on Domain)
- **QuranListeningApp.Application**: Services, business logic (depends on Domain, Infrastructure)
- **QuranListeningApp.Web**: Blazor Server UI (depends on Application)

## Technology Stack
- .NET 8
- Blazor Server
- Entity Framework Core 8.0 with SQL Server
- ASP.NET Core Identity 8.0
- MudBlazor 8.13 (RTL-ready UI framework)

## Key Requirements
- Arabic language support with RTL layout
- Mobile-first responsive design
- Role-based access: Admin, Teacher, Student
- Single Users table for all user types (optimized design)

## Workspace Setup - Completed ✅

### ✅ All Steps Completed
- [x] Created .github/copilot-instructions.md file
- [x] Got project setup information for .NET
- [x] Scaffolded .NET solution with 4 projects
- [x] Added project references (Web → Application → Infrastructure → Domain)
- [x] Installed NuGet packages:
  - Microsoft.EntityFrameworkCore.SqlServer 9.0.10 (Infrastructure)
  - Microsoft.EntityFrameworkCore.Tools 9.0.10 (Infrastructure)
  - Microsoft.AspNetCore.Identity.EntityFrameworkCore 8.0.10 (Web)
  - MudBlazor 8.13.0 (Web)
- [x] Verified project compiles successfully
- [x] Created README.md with complete setup instructions

## Next Steps

Follow the development tasks in `dev-tasks.md` starting with **Phase 1: Foundation & Setup**:
1. Create domain entities (User, ListeningSession, SurahReference, AuditLog)
2. Set up DbContext and Entity Framework configurations
3. Create and apply initial database migrations
4. Seed Surah reference data
5. Configure authentication and authorization

The solution is ready for development!
