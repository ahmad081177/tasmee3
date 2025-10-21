# Business Requirements - Quran Listening Management System

## 1. Project Overview & Goals

A web application for teachers to record, view, edit, and export listening (recitation) sessions of students memorizing the Qur'an. The system must support Arabic language with right-to-left (RTL) layout and be accessible on both mobile and desktop devices.

### Core Purpose
Enable teachers to track and manage student Quran recitation progress through systematic recording of listening sessions, error tracking, and progress reporting.

---

## 2. User Roles & Permissions

### Administrator
- Manage teacher accounts (create, edit, remove)
- Manage student records (create, edit, remove with login credentials)
- View and manage all listening records across all teachers and students
- Access system-wide reports and analytics
- Configure system settings

### Teacher
- Secure login to personal account
- View assigned student list with search and filtering capabilities
- Access individual student details and recitation history
- Record new listening sessions for students
- Edit and delete only their own recorded sessions
- Export student progress reports
- Search students by name, ID, or phone number
- Filter sessions by completion status (passed/completed only)

### Student
- Secure login with personal credentials (username and password)
- View personal recitation history and progress (read-only access)
- View session details including dates, surahs, ayahs, errors, and teacher notes
- View only passed/completed sessions or all sessions
- No ability to add, edit, or delete any records

---

## 3. Core Business Processes

### Student Management
- Maintain student profiles including:
  - Full name (Arabic)
  - Grade level
  - id number
  - Contact phone number
  - Enrollment status

### Teacher Management
- Maintain teacher profiles including:
  - Full name (Arabic)
  - Username for system access
  - Contact information
  - Active status

### Listening Session Recording
Teachers must be able to record detailed information about each student's recitation session:
- **Session Details:**
  - Date and time of the session (auto-populated with current date/time, but can be modified by teacher)
  - Teacher conducting the session (auto-populated with logged-in teacher's name)
  - Student being evaluated
  
- **Recitation Range:**
  - Surah number (stored as integer, displayed with Surah name + number in UI) - ONE surah per session
  - Starting Ayah number
  - Ending Ayah number
  - **Note:** If teacher needs to record multiple surahs, they should create separate sessions
  
- **Error Tracking:**
  - Count of major errors (خطأ جلي - Obvious/Clear Error)
  - Count of minor errors (خطأ خفي - Hidden/Subtle Error)
  
- **Session Status:**
  - Is Completed/Passed checkbox (indicates whether student successfully passed the session)
  
- **Additional Notes:**
  - Free text field for teacher observations and comments

---

## 4. User Interface Requirements

### Language & Accessibility
- Primary language: Arabic
- Text direction: Right-to-left (RTL)
- Support for Arabic text input and display
- Mobile-responsive design (mobile-first approach)
- Make it mobile first thoughts, UI is compact clear.
- Accessible on both smartphones and desktop computers

### Teacher Dashboard
- Quick overview of assigned students
- Recent activity summary
- Easy navigation to student records
- Search functionality across students

### Student Listing
- Display students in organized, easy-to-scan format
- Show key information: name, ID, phone, grade
- Include recent session summary for each student
- Support filtering and searching capabilities

### Student Detail View
- Complete student information header
- Chronological listing of all listening sessions
- Most recent sessions displayed first
- Clear presentation of session data and error counts

---

## 5. Data Management Requirements

### Record Ownership
- Teachers can only edit or delete their own session records
- Administrators have unrestricted access to all records
- Clear audit trail of who created/modified each record

### Data Validation
- Ensure valid Surah and Ayah references
- Require all mandatory fields for session recording
- Validate date and time entries
- Ensure error counts are non-negative numbers

### Data Retention
- Maintain complete historical record of all sessions
- Support for marking records as inactive rather than deleting
- Preserve data integrity across system updates

---

## 6. Reporting & Export Requirements

### Export Formats
- Generate reports in common formats (Excel, PDF)
- Maintain Arabic text and RTL formatting in exports
- Include comprehensive student progress summaries

### Report Content
- Individual student progress over time
- Error trend analysis
- Session frequency tracking
- Teacher activity summaries
- Custom date range filtering

---

## 7. Search & Filtering Capabilities

### Student Search
- Search by student name (Arabic)
- Search by id number
- Search by phone number
- Filter by grade level
- Filter by recent activity

### Session History Filtering
- Filter by date range
- Filter by teacher
- Filter by error count thresholds
- Filter by completion status (show only passed/completed sessions)
- Filter by major vs minor errors
- Sort by various criteria (date, errors, etc.)

---

## 8. Security & Access Control

### Authentication
- Secure login system for administrators, teachers, and students
- Password protection with industry-standard security (bcrypt hashing)
- Session management and timeout controls
- Students and Teachers receive login credentials to access their own progress (Admin will create them the cred)
- Make it simple, no register/signup is needed, only login

### Authorization
- Role-based access control (admin, teacher, student)
- Prevent unauthorized access to student data
- Students can only view their own records (read-only)
- Teachers can view all students but only modify their own session records
- Ensure teachers can only modify their own records
- Administrative override capabilities

### Data Privacy
- Protect student personal information
- Secure handling of academic progress data
- Audit logging for sensitive operations

---

## 9. Performance Requirements

### Response Time
- Fast page loading on mobile devices
- Quick search results even with large student databases
- Efficient data entry and saving processes

### Scalability
- Support for multiple teachers working simultaneously
- Handle large numbers of students (around 250) and historical records

---

## 10. Future Enhancement Opportunities (Not for now)

### Advanced Analytics
- Automated progress analysis and insights
- Error pattern recognition
- Performance trend identification

### Communication Features
- Weekly progress summaries
- Automated notifications for milestones
- Teacher-parent communication tools

### Multi-Institution Support
- Support for multiple schools or educational institutions
- Centralized management across locations
- Institution-specific reporting and settings

---

## 11. Business Rules

### Session Recording Rules
- Each session must be linked to exactly one teacher and one student
- Teacher identity is automatically captured and cannot be modified
- Session date/time defaults to current date/time but can be adjusted by teacher
- Surah numbers stored as integers (1-114) in database
- All error counts (major and minor) must be non-negative integers
- Major errors (خطأ جلي) and minor errors (خطأ خفي) are tracked separately
- Session must be marked as completed/passed or incomplete
- From Surah, From Ayah, To Surah, and To Ayah are all required fields

### Access Control Rules
- Teachers can only view and manage students assigned to them
- Record editing is restricted to the teacher who created the record
- Administrators have unrestricted access for management purposes
- Student data access requires proper authentication

### Data Integrity Rules
- Student and teacher records cannot be permanently deleted if linked to sessions
- Historical session data must be preserved for academic record keeping
- All changes to existing records must maintain audit trail
- Surah and Ayah references must be validated against Quran structure