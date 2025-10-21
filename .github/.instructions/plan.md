# Development Plan - Quran Listening Management System

## 1. Project Overview

### 1.1 Technology Stack
- **Frontend Framework**: Blazor Server (.NET 8 or .NET 9)
- **Backend**: ASP.NET Core Web API (integrated with Blazor Server)
- **Database**: SQL Server (on-premises or remote server)
- **ORM**: Entity Framework Core
- **UI Framework**: Bootstrap 5 with RTL support or MudBlazor (supports RTL out-of-box)
- **Authentication**: ASP.NET Core Identity
- **Deployment Platform**: Windows Server with IIS

### 1.2 Architecture Pattern
- **Pattern**: Clean Architecture / N-Layer Architecture
  - Presentation Layer (Blazor Components)
  - Business Logic Layer (Services)
  - Data Access Layer (EF Core Repositories)
  - Domain Layer (Entities and Business Rules)

### 1.3 Key Technical Considerations
- Right-to-left (RTL) layout throughout the application
- Arabic language support for all UI elements
- Mobile-first responsive design
- Role-based access control (Admin, Teacher, Student)
- Secure password hashing (bcrypt or ASP.NET Core Identity default)
- Audit logging for data modifications

---

## 2. Database Design

### 2.1 Core Tables/Entities

#### Users Table
Single table for all users (Admin, Teacher, Student) - eliminates duplication and simplifies design.

- `Id` (Primary Key, GUID)
- `Username` (Unique, Required) - Used for login
- `PasswordHash` (Required) - Hashed password for authentication
- `FullNameArabic` (Required, nvarchar) - Full name in Arabic for all users
- `IdNumber` (nvarchar, Unique) - National ID number (required for Teachers and Students, nullable for Admin)
- `PhoneNumber` (nvarchar) - Contact phone number (required for Teachers and Students, nullable for Admin)
- `Email` (nvarchar, Optional) - Email address (optional for all roles)
- `Role` (Enum: Admin, Teacher, Student) - User role determines access level and functionality
- `GradeLevel` (nvarchar, Nullable) - Student's grade/class level (only populated for Students, null for Admin/Teacher)
- `IsActive` (Boolean, Default: true) - Account active status
- `CreatedDate` (DateTime) - When the user account was created
- `ModifiedDate` (DateTime, Nullable) - Last modification timestamp
- `CreatedByUserId` (Foreign Key to Users, Nullable) - Admin who created this user account
- `LastLoginDate` (DateTime, Nullable) - Last successful login timestamp

**Note:** This single table approach eliminates the need for separate Teachers and Students tables, reducing duplication of fields like FullNameArabic, PhoneNumber, and IsActive. The `Role` field determines user type, and `GradeLevel` is only used for students.

#### ListeningSessions Table
Records each Quran recitation examination session conducted by a teacher for a student.

- `Id` (Primary Key, GUID)
- `StudentUserId` (Foreign Key to Users, Required) - The student being examined (must have Role=Student)
- `TeacherUserId` (Foreign Key to Users, Required) - The teacher conducting the session (must have Role=Teacher)
- `SessionDate` (DateTime, Required) - Date and time of the session (defaults to current, but editable)
- `FromSurahNumber` (Integer, 1-114, Required) - Starting Surah number
- `FromAyahNumber` (Integer, Required) - Starting Ayah number within the Surah
- `ToSurahNumber` (Integer, 1-114, Required) - Ending Surah number
- `ToAyahNumber` (Integer, Required) - Ending Ayah number within the Surah
- `MajorErrorsCount` (Integer, Default: 0) - Count of major errors (خطأ جلي - Clear/Obvious Error)
- `MinorErrorsCount` (Integer, Default: 0) - Count of minor errors (خطأ خفي - Hidden/Subtle Error)
- `IsCompleted` (Boolean, Default: false) - Whether the student passed/completed the session successfully
- `Notes` (nvarchar(max), Optional) - Teacher's observations and comments in Arabic
- `CreatedDate` (DateTime) - When the session record was created
- `ModifiedDate` (DateTime, Nullable) - Last modification timestamp

**Note:** There is NO direct assignment relationship between teachers and students. Any teacher can examine any student from the pool. The relationship is established only through this ListeningSessions table when a session is recorded.

#### AuditLog Table
Tracks all sensitive operations for security and compliance.

- `Id` (Primary Key, GUID)
- `UserId` (Foreign Key to Users) - Who performed the action
- `Action` (nvarchar: Created, Updated, Deleted) - Type of operation
- `EntityType` (nvarchar: User, ListeningSession) - Type of entity modified
- `EntityId` (GUID) - ID of the entity that was modified
- `OldValues` (nvarchar(max), JSON) - Previous values before modification
- `NewValues` (nvarchar(max), JSON) - New values after modification
- `Timestamp` (DateTime) - When the action occurred
- `IpAddress` (nvarchar, Optional) - IP address of the user who performed the action

#### SurahReference Table (Lookup/Static Data)
- `SurahNumber` (Primary Key, Integer, 1-114)
- `SurahNameArabic` (nvarchar, Required)
- `SurahNameEnglish` (nvarchar, Optional)
- `TotalAyahs` (Integer, Required)
- `IsMakki` (Boolean)

### 2.2 Database Relationships
- **Users table** is self-referential: `CreatedByUserId` references another User (the admin who created the account)
- **ListeningSessions** has two foreign keys to Users:
  - `StudentUserId` → User with Role=Student (One student has many sessions, 1:N)
  - `TeacherUserId` → User with Role=Teacher (One teacher has many sessions, 1:N)
- **No direct teacher-student assignment**: Teachers are not assigned to specific students. Any teacher can examine any student. The relationship is created dynamically through ListeningSessions.
- **SurahReference** is referenced by ListeningSessions for Surah/Ayah validation (lookup table)
- **AuditLog** references Users table for tracking who performed actions

### 2.3 Indexes
- Index on `Users.Username` (Unique) - Fast login lookups
- Index on `Users.IdNumber` (Unique) - Fast search by national ID
- Index on `Users.PhoneNumber` - Fast search by phone number
- Index on `Users.FullNameArabic` - Fast search by Arabic name
- Index on `Users.Role` - Filter users by role
- Index on `ListeningSessions.StudentUserId` - Fast retrieval of student's session history
- Index on `ListeningSessions.TeacherUserId` - Fast retrieval of teacher's sessions
- Index on `ListeningSessions.SessionDate` - Date range queries and sorting
- Index on `ListeningSessions.IsCompleted` - Filter completed/incomplete sessions

### 2.4 Database Collation
- Use collation supporting Arabic language: `Arabic_CI_AS` or `Latin1_General_100_CI_AS_SC_UTF8`

---

## 3. Application Layers & Components

### 3.1 Domain Layer (Core)

#### Entities
- `User` entity with role enumeration (Admin, Teacher, Student)
  - Includes validation rules for required fields based on role
  - GradeLevel only validated/populated for Students
  - IdNumber and PhoneNumber required for Teachers and Students
- `ListeningSession` entity with business rules
  - Validates Surah/Ayah ranges
  - Enforces teacher ownership for edit/delete operations
- `SurahReference` entity (static/lookup data)
- `AuditLog` entity for tracking changes

#### Enumerations
- `UserRole`: Admin, Teacher, Student
- `ErrorType`: Major, Minor (for categorizing errors)
- `AuditAction`: Created, Updated, Deleted, Viewed

#### Value Objects (Optional)
- `SurahRange` (FromSurah, FromAyah, ToSurah, ToAyah with validation logic)
- `ErrorCount` (Major and Minor error counts with non-negative validation)

### 3.2 Data Access Layer

#### DbContext
- `QuranAppDbContext` with all DbSets (Users, ListeningSessions, SurahReference, AuditLog)
- Configure entity relationships and foreign keys
- Configure cascade delete rules (prevent deletion of users with existing sessions)
- Configure indexes and constraints
- Seed initial data (Surah references, default admin user)
- Configure check constraints (e.g., Role-based field validation)

#### Repositories (Repository Pattern - Optional)
- `IUserRepository` / `UserRepository` - CRUD for all users (Admin, Teacher, Student)
- `IListeningSessionRepository` / `ListeningSessionRepository` - Session management
- `IAuditLogRepository` / `AuditLogRepository` - Audit trail operations
- `ISurahReferenceRepository` / `SurahReferenceRepository` - Surah lookup operations

**Note:** Since we eliminated the Teachers and Students tables, we only need a single UserRepository to handle all user types. Role-based filtering is handled via LINQ queries on the Role property.

#### Unit of Work Pattern (Optional)
- `IUnitOfWork` for transaction management across multiple repository operations

### 3.3 Business Logic Layer (Services)

#### Authentication & Authorization Services
- `IAuthenticationService`: Handle login, logout, session management
- `IAuthorizationService`: Verify role-based permissions
- `IPasswordService`: Hash and verify passwords using ASP.NET Core Identity password hasher

#### Domain Services
- `IUserService`: User management (create, edit, delete, search)
  - Separate methods for creating teachers vs students (different required fields)
  - Role-based validation (e.g., GradeLevel only for students)
- `IListeningSessionService`: Record sessions, edit, delete with ownership checks
  - Verify teacher can only edit/delete their own sessions
  - Validate student and teacher exist and have correct roles
- `IAuditService`: Log all sensitive operations (user creation, session modifications)
- `IReportService`: Generate reports and export data
  - Student progress reports
  - Teacher activity reports
- `ISurahValidationService`: Validate Surah and Ayah references against SurahReference table

#### Search & Filter Services
- `ISearchService`: Unified search across users
  - Search students by name (Arabic), national ID, or phone number
  - Search teachers by name (Arabic), national ID, or phone number
- `IFilterService`: Complex filtering for sessions
  - Filter by date range, teacher, student, completion status
  - Filter by error count thresholds

### 3.4 Presentation Layer (Blazor Components)

#### Pages (Routable Components)
- `/` - Home/Dashboard (role-specific redirect)
- `/login` - Login page (anonymous)
- `/admin/dashboard` - Admin dashboard
- `/admin/teachers` - Teacher management (list, create, edit)
- `/admin/students` - Student management (list, create, edit)
- `/admin/reports` - System-wide reports
- `/teacher/dashboard` - Teacher dashboard
- `/teacher/students` - Assigned student list with search/filter
- `/teacher/student/{id}` - Student detail with session history
- `/teacher/session/new/{studentId}` - Record new listening session
- `/teacher/session/edit/{sessionId}` - Edit existing session
- `/student/dashboard` - Student dashboard (view own progress)
- `/student/sessions` - Student's own session history (read-only)

#### Shared Components
- `LoginComponent` - Login form with Arabic support
- `NavigationMenu` - RTL navigation menu (role-based)
- `StudentCard` - Display student summary
- `StudentListComponent` - Paginated student list with search
- `SessionFormComponent` - Reusable form for recording/editing sessions
- `SessionListComponent` - Display session history with filters
- `SurahAyahSelector` - Dropdown/selector for Surah and Ayah numbers
- `ErrorCountInput` - Input fields for major/minor errors
- `SearchBar` - Arabic-enabled search input
- `FilterPanel` - Multi-criteria filter component
- `ExportButton` - Trigger report export (Excel/PDF)
- `ConfirmationDialog` - Confirm delete operations
- `LoadingSpinner` - Loading indicator
- `ToastNotification` - Success/error messages (Arabic)
- `PaginationComponent` - Pagination controls

#### Layout Components
- `MainLayout` - Overall RTL layout with navigation
- `AdminLayout` - Admin-specific layout
- `TeacherLayout` - Teacher-specific layout
- `StudentLayout` - Student-specific layout

---

## 4. Security Implementation

### 4.1 Authentication
- Use ASP.NET Core Identity for user authentication
- Implement custom user store if needed for flexibility
- Session-based authentication (cookie-based)
- Automatic session timeout after 30 minutes of inactivity
- Secure password requirements (minimum length, complexity)
- Password hashing using bcrypt or PBKDF2 (Identity default)

### 4.2 Authorization
- Role-based authorization using `[Authorize(Roles = "...")]` attributes
- Custom authorization handlers for record ownership verification
- Teachers can only edit/delete their own sessions
- Students have read-only access to their own data
- Admins have full access to all resources

### 4.3 Data Protection
- HTTPS enforcement for all connections (configure in IIS)
- Connection string encryption in configuration files or use environment variables
- SQL injection prevention through EF Core parameterized queries
- XSS prevention through Blazor's automatic HTML encoding
- CSRF protection (built-in with Blazor Server)
- Store sensitive configuration in encrypted sections of web.config or use Windows DPAPI

### 4.4 Audit Logging
- Log all Create, Update, Delete operations on critical entities
- Store user ID, timestamp, action, entity type, and changes
- Implement as a service injected into repositories or services
- Use JSON serialization for old/new values

---

## 5. Arabic & RTL Support

### 5.1 UI Framework Selection
**Option 1: Bootstrap 5 with RTL**
- Use Bootstrap 5 RTL version (bootstrap.rtl.min.css)
- Custom CSS for Arabic font optimization (e.g., Cairo, Tajawal, Amiri fonts)
- Manual RTL adjustments where needed

**Option 2: MudBlazor**
- Built-in RTL support via `RightToLeft="true"` property
- Rich component library (dialogs, tables, forms, etc.)
- Good performance with Blazor Server
- Customizable theme for Arabic aesthetics

### 5.2 Implementation Requirements
- Set `dir="rtl"` on HTML root or body element
- Use Arabic fonts from Google Fonts (Cairo, Tajawal, Amiri)
- All labels, buttons, messages, validation errors in Arabic
- Date/time formatting in Arabic or localized format
- Number formatting (Arabic or Western Arabic numerals)
- Test on mobile devices for RTL rendering

### 5.3 Resource Files
- Create resource files (.resx) for all UI text
- Support future localization if needed
- Centralize all strings for consistency

---

## 6. Mobile-First Design

### 6.1 Responsive Breakpoints
- Mobile: < 768px (primary focus)
- Tablet: 768px - 1024px
- Desktop: > 1024px

### 6.2 Mobile Optimizations
- Touch-friendly buttons and inputs (minimum 44x44px)
- Compact data tables with horizontal scroll or cards
- Collapsible filters and search panels
- Bottom navigation for quick access (mobile)
- Fast loading with minimal initial payload
- Progressive enhancement for larger screens

### 6.3 Component Behavior
- Student list: Card view on mobile, table view on desktop
- Session form: Single column on mobile, multi-column on desktop
- Navigation: Hamburger menu on mobile, sidebar on desktop
- Filters: Slide-in panel on mobile, sidebar on desktop

---

## 7. Data Validation & Business Rules

### 7.1 Client-Side Validation (Blazor)
- Use `DataAnnotations` attributes on User entity/models
- Required fields validation:
  - All users: Username, PasswordHash, FullNameArabic, Role
  - Teachers and Students: IdNumber, PhoneNumber (conditional validation based on Role)
  - Students only: GradeLevel (conditional validation)
  - Session: StudentUserId, TeacherUserId, SessionDate, Surah/Ayah ranges
- Range validation: Surah numbers (1-114), Ayah numbers (positive integers)
- String length limits on text fields
- Phone number format validation
- National ID format validation
- Custom validators for:
  - Role-based conditional required fields
  - Surah/Ayah range logic (FromSurah/Ayah <= ToSurah/Ayah)
  - Ensure StudentUserId has Role=Student, TeacherUserId has Role=Teacher

### 7.2 Server-Side Validation
- Re-validate all inputs on the server (never trust client-side validation alone)
- Validate Surah/Ayah references against `SurahReference` table
- Ensure FromSurah/Ayah <= ToSurah/Ayah
- Check record ownership before edit/delete operations (teacher can only modify their own sessions)
- Verify user roles and permissions
- Validate role-specific required fields:
  - Teachers/Students must have IdNumber and PhoneNumber
  - Students must have GradeLevel
- Ensure ListeningSession references valid users with correct roles:
  - StudentUserId must point to a User with Role=Student
  - TeacherUserId must point to a User with Role=Teacher
- Prevent deletion of users who have associated listening sessions (data integrity)

### 7.3 Business Rule Enforcement
- Teacher ID auto-populated from logged-in user's ID (non-editable in UI)
- Session date defaults to current date/time (but editable by teacher)
- Error counts must be >= 0 (non-negative integers)
- Cannot delete users (admin, teacher, or student) who have associated listening sessions
  - Implement soft delete (set IsActive=false) or prevent deletion with clear error message
- Students can filter their own sessions to view only completed/passed sessions or all sessions
- Teachers can only edit/delete sessions they created (ownership check via TeacherUserId)
- Username must be unique across all users
- National ID (IdNumber) must be unique across all teachers and students
- Role cannot be changed after user creation (or implement with strict authorization)

---

## 8. Reporting & Export

### 8.1 Report Types
- **Student Progress Report**: Individual student's session history with error trends
- **Teacher Activity Report**: Sessions conducted by teacher within date range
- **System Summary Report**: Overall statistics (admins only)
- **Error Analysis Report**: Major/minor error trends over time

### 8.2 Export Formats
**Excel Export**
- Use EPPlus or ClosedXML library
- Maintain Arabic text (use UTF-8 encoding)
- Apply RTL cell alignment
- Include charts for visual representation (optional)

**PDF Export**
- Use QuestPDF or iTextSharp library
- Support Arabic fonts (embed Arabic font in PDF)
- RTL text direction
- Professional formatting with headers/footers

### 8.3 Export Features
- Date range filtering
- Single student or multiple students
- Teacher-specific exports
- Admin can export all data
- Asynchronous export for large datasets (background job)

---

## 9. Performance Optimization

### 9.1 Database Performance
- Use EF Core AsNoTracking for read-only queries
- Implement pagination for all list views (e.g., 20-50 records per page)
- Use projection (Select) to fetch only needed fields
- Implement database indexes on frequently queried columns
- Use stored procedures for complex reports (optional)

### 9.2 Blazor Server Performance
- Minimize component re-renders (use ShouldRender)
- Use streaming rendering for large lists
- Implement virtualization for long lists (Virtualize component)
- Reduce SignalR message size
- Use caching for lookup data (Surah references)

### 9.3 Caching Strategy
- Cache `SurahReference` data in memory (rarely changes)
- Cache user roles and permissions in session
- Use distributed caching (Azure Redis Cache) if scaling to multiple instances

### 9.4 Azure-Specific Optimizations
- Use SQL Server with appropriate edition (Standard or Express to start)
- Enable monitoring and logging using built-in .NET logging or third-party tools (e.g., Serilog, NLog)
- Configure IIS application pool settings for optimal performance
- Use Windows Performance Monitor for server monitoring

---

## 10. Deployment Architecture (Windows Server & SQL Server)

### 10.1 Server Requirements
- **Windows Server**: Windows Server 2019 or later (2022 recommended)
  - IIS (Internet Information Services) installed and configured
  - .NET 8 or .NET 9 Hosting Bundle installed
  - Firewall configured to allow HTTP/HTTPS traffic (ports 80, 443)
  - SSL certificate for HTTPS (Let's Encrypt or commercial certificate)
  
- **SQL Server**: SQL Server 2019 or later (2022 recommended)
  - Can be on the same server or separate machine
  - SQL Server Management Studio (SSMS) for database management
  - Appropriate edition: Express (free, up to 10GB), Standard, or Enterprise
  - Configure firewall to allow connections on port 1433 (if remote)
  - Enable SQL Server Authentication (mixed mode)
  - Create dedicated database login for the application

### 10.2 Deployment Configuration
- Connection string stored in `appsettings.json` or `appsettings.Production.json` (encrypted or in secure location)
- Use environment variables for sensitive settings (recommended)
- Enable HTTPS-only access (configure HTTPS binding in IIS)
- Configure custom domain and SSL certificate in IIS
- Set up IIS application pool with appropriate .NET version
- Configure IIS application pool identity (use service account with SQL access)

### 10.3 Deployment Process
- Publish application using Visual Studio or `dotnet publish` command
- Copy published files to server (via RDP, FTP, or network share)
- Create IIS website or application pointing to published folder
- Configure IIS bindings (HTTP/HTTPS, domain name)
- Set appropriate file permissions (IIS user needs read access to app files)
- Configure SQL Server connection string
- Run EF Core migrations to create/update database schema

### 10.4 Database Setup
- Create database on SQL Server instance
- Run EF Core migrations using:
  - `dotnet ef database update` from command line, or
  - Execute migration SQL script in SSMS
- Seed initial data (Surah references, default admin user)
- Configure SQL Server backups (full backup daily, transaction log backups hourly)
- Create SQL Server login for application with appropriate permissions (db_datareader, db_datawriter)

---

## 11. Testing Strategy

### 11.1 Unit Testing
- Test business logic in services
- Test validation logic
- Test Surah/Ayah range validation
- Use xUnit or NUnit
- Mock repositories using Moq

### 11.2 Integration Testing
- Test database operations with in-memory database or test SQL Server
- Test EF Core queries and relationships
- Test authentication and authorization flows

### 11.3 UI Testing (Optional)
- Use bUnit for Blazor component testing
- Test component rendering and user interactions
- Test RTL layout rendering

### 11.4 Manual Testing
- Test on mobile devices (iOS, Android)
- Test RTL layout on different browsers
- Test Arabic text input and display
- Test role-based access control
- Performance testing with realistic data volume

---

## 12. Initial Data Setup

### 12.1 Surah Reference Data
- Seed all 114 Surahs with Arabic names and total Ayah counts
- Use EF Core seed data or migration script
- Data source: Quran metadata (available online)

### 12.2 Default Admin User
- Create one default admin user during initial deployment
- Username: `admin`
- Password: Temporary password (must be changed on first login)
- Full access to all features

### 12.3 Test Data (Development Only)
- Create sample users with different roles:
  - 1 admin user (for testing admin features)
  - 2-3 teacher users (with realistic Arabic names, national IDs, phone numbers)
  - 10-20 student users (with realistic Arabic names, national IDs, phone numbers, grade levels)
- Create sample listening sessions (50-100):
  - Link to the sample teachers and students created above
  - Variety of Surah ranges, error counts, completion statuses
  - Distribute sessions across different dates for testing filters
- Use realistic Arabic names and data for authenticity

---

## 13. Error Handling & Logging

### 13.1 Error Handling
- Global exception handling in Blazor Server
- Try-catch blocks in services for expected errors
- User-friendly error messages in Arabic
- Log all exceptions to Application Insights

### 13.2 Logging Levels
- **Information**: User login, logout, session recording
- **Warning**: Failed validation, unauthorized access attempts
- **Error**: Exceptions, database errors, failed operations
- **Critical**: System failures, database connection issues

### 13.3 User Notifications
- Toast notifications for success/error messages
- Arabic messages for all notifications
- Differentiate between validation errors and system errors

---

## 14. Development Phases

### Phase 1: Foundation & Setup (Week 1-2)
- Set up development environment and Azure resources
- Create database schema and EF Core models
- Configure authentication and authorization
- Implement basic navigation and layout (RTL)
- Seed Surah reference data and default admin

### Phase 2: Core Features - Admin (Week 3-4)
- Admin dashboard
- User management (teachers, students)
- CRUD operations for teachers
- CRUD operations for students
- Audit logging implementation

### Phase 3: Core Features - Teacher (Week 5-7)
- Teacher dashboard
- Student listing with search and filters
- Student detail view with session history
- Record new listening session (full form with validation)
- Edit and delete own sessions
- Surah/Ayah validation

### Phase 4: Core Features - Student (Week 8)
- Student login and dashboard
- View own session history (read-only)
- Filter sessions (completed/passed only or all)
- Display session details

### Phase 5: Reporting & Export (Week 9-10)
- Implement report generation service
- Excel export functionality
- PDF export functionality
- Student progress report
- Teacher activity report

### Phase 6: Testing & Refinement (Week 11-12)
- Unit and integration testing
- Mobile device testing (iOS, Android)
- RTL and Arabic text testing
- Performance testing and optimization
- Security audit and fixes
- Bug fixes and UI refinement

### Phase 7: Deployment & Go-Live (Week 13)
- Deploy to Azure staging environment
- User acceptance testing (UAT)
- Deploy to Azure production environment
- Monitor for issues
- Create user documentation (Arabic)

---

## 15. Post-Launch Support

### 15.1 Monitoring
- Monitor application logs using Serilog or NLog (file-based or database logging)
- Track user activity and usage patterns through application logs
- Monitor database performance using SQL Server Management Studio
- Monitor query performance and execution plans
- Set up Windows Performance Monitor for server metrics
- Review IIS logs regularly

### 15.2 Maintenance
- Apply security patches and Windows updates to server
- Apply SQL Server updates and patches
- Optimize database indexes based on usage
- Backup database regularly using SQL Server maintenance plans
- Test backup restoration periodically
- Monitor disk space on server
- Clean up old log files periodically

### 15.3 User Support
- Provide user documentation in Arabic
- Create video tutorials for teachers (Arabic)
- Establish support channel (email/phone)
- Track feature requests and bugs

---

## 16. Key Third-Party Libraries

### 16.1 Required Libraries
- **Entity Framework Core** (Microsoft.EntityFrameworkCore.SqlServer)
- **ASP.NET Core Identity** (Microsoft.AspNetCore.Identity.EntityFrameworkCore)
- **Bootstrap 5 RTL** or **MudBlazor** (UI framework)
- **EPPlus** or **ClosedXML** (Excel export)
- **QuestPDF** or **iTextSharp** (PDF export)

### 16.2 Optional Libraries
- **AutoMapper** (object-to-object mapping)
- **FluentValidation** (advanced validation)
- **Serilog** (structured logging)
- **Polly** (retry policies for resilience)
- **Hangfire** (background job processing for large exports)

---

## 17. Scalability Considerations

### 17.1 Current Scope
- Target: ~250 students, 10-20 teachers
- Expected concurrent users: 5-10
- Windows Server with Standard or Express SQL Server sufficient
- Single server deployment adequate for current scope

### 17.2 Future Scalability
- Transition to Blazor WebAssembly for better scalability (if needed)
- Use distributed caching (Redis) if scaling to multiple servers
- Add load balancing if traffic increases significantly
- Upgrade SQL Server edition as data grows
- Consider cloud migration (Azure) if scaling beyond single server
- Implement background job processing (Hangfire) for heavy operations

---

## 18. Risk Mitigation

### 18.1 Technical Risks
- **Risk**: Arabic/RTL rendering issues on different browsers
  - **Mitigation**: Test early and frequently on multiple browsers and devices
  
- **Risk**: Blazor Server SignalR connection issues on mobile
  - **Mitigation**: Implement reconnection logic, consider Blazor WebAssembly for mobile
  
- **Risk**: Performance degradation with large datasets
  - **Mitigation**: Implement pagination, indexing, and caching early

### 18.2 Security Risks
- **Risk**: Unauthorized access to student data
  - **Mitigation**: Implement robust authorization checks, audit logging
  
- **Risk**: Password security breaches
  - **Mitigation**: Use bcrypt/PBKDF2, enforce password policies, HTTPS only

### 18.3 Deployment Risks
- **Risk**: Database migration failures during deployment
  - **Mitigation**: Test migrations in staging, use deployment slots, have rollback plan

---

## 19. Success Criteria

### 19.1 Functional Success
- All user roles (admin, teacher, student) can perform their designated tasks
- Teachers can record and manage listening sessions efficiently
- Students can view their progress securely
- Reports export correctly with Arabic text and RTL formatting
- Search and filter functions work accurately

### 19.2 Technical Success
- Application loads in < 3 seconds on mobile devices
- No critical security vulnerabilities
- 99% uptime on Azure
- Database queries execute in < 1 second
- Application handles 10+ concurrent users smoothly

### 19.3 User Satisfaction
- Teachers find the session recording process intuitive
- Mobile UI is easy to use and read (Arabic/RTL)
- System reduces time spent on manual record-keeping
- Admins have full control and visibility

---

## 20. Documentation Deliverables

### 20.1 Technical Documentation
- Database schema diagram with relationships
- API/service documentation (XML comments)
- Deployment guide for Azure
- Configuration and environment variables guide

### 20.2 User Documentation (Arabic)
- Admin user guide (manage teachers, students, view reports)
- Teacher user guide (record sessions, view students, export reports)
- Student user guide (view own progress)
- Login and password recovery instructions

### 20.3 Developer Documentation
- Code structure and architecture overview
- Component hierarchy and reusability guide
- Adding new features guide
- Testing guide

---

## 21. Next Steps for Team Lead

After reviewing this plan, the development team lead should:

1. **Break down into detailed tasks** - Create dev-tasks.md with specific implementation tasks for each phase
2. **Assign story points** - Estimate effort for each task
3. **Create sprint plan** - Organize tasks into 2-week sprints
4. **Set up development environment** - Provision Azure resources, set up repositories
5. **Define coding standards** - Establish naming conventions, code review process, branching strategy
6. **Create database design** - Finalize ERD diagram and table structures
7. **Set up CI/CD pipeline** - Configure GitHub Actions or Azure DevOps
8. **Schedule team meetings** - Daily standups, sprint planning, retrospectives

---

## Appendix: Additional Resources

### Arabic Fonts
- **Cairo** - Modern, clean, great for UI
- **Tajawal** - Traditional, readable
- **Amiri** - Excellent for Quranic text

### Quran Data Sources
- Surah names and Ayah counts: Available from Quran.com API or similar
- Can embed as JSON or seed in database

### UI/UX Best Practices for Arabic
- Consistent padding and alignment in RTL
- Mirror icons and directional elements
- Test with actual Arabic users
- Consider cultural design preferences

---

**End of Plan**

This plan provides a comprehensive blueprint for building the Quran Listening Management System using Blazor Server, SQL Server, and Azure deployment. The team lead can now create detailed development tasks based on this plan.
