using QuranListeningApp.Application.DTOs.Reports;
using QuranListeningApp.Application.Services;
using QuranListeningApp.Domain.Entities;
using QuranListeningApp.Domain.Enums;
using QuranListeningApp.Domain.Interfaces;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using DrawingColor = System.Drawing.Color;

namespace QuranListeningApp.Application.Services;

public class ReportService : IReportService
{
    private readonly IUserRepository _userRepository;
    private readonly IListeningSessionRepository _sessionRepository;
    private readonly ISurahReferenceRepository _surahRepository;

    public ReportService(
        IUserRepository userRepository, 
        IListeningSessionRepository sessionRepository,
        ISurahReferenceRepository surahRepository)
    {
        _userRepository = userRepository;
        _sessionRepository = sessionRepository;
        _surahRepository = surahRepository;
        
        // Configure QuestPDF license (Community license)
        QuestPDF.Settings.License = LicenseType.Community;
        
        // Configure EPPlus license (NonCommercial)
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    #region Student Progress Report

    public async Task<StudentProgressReportDto> GetStudentProgressReportAsync(Guid studentId, DateTime fromDate, DateTime toDate)
    {
        var student = await _userRepository.GetByIdAsync(studentId);
        if (student == null || student.Role != UserRole.Student)
            throw new ArgumentException("Student not found");

        var sessions = await _sessionRepository.GetSessionsByStudentIdAsync(studentId, fromDate, toDate);
        
        var report = new StudentProgressReportDto
        {
            Student = student,
            FromDate = fromDate,
            ToDate = toDate,
            Sessions = sessions.ToList(),
            Summary = CalculateStudentProgressSummary(sessions.ToList())
        };

        return report;
    }

    public async Task<byte[]> ExportStudentProgressToExcelAsync(Guid studentId, DateTime fromDate, DateTime toDate)
    {
        var report = await GetStudentProgressReportAsync(studentId, fromDate, toDate);
        
        using var package = new ExcelPackage();
        
        // Student Info Sheet
        var infoSheet = package.Workbook.Worksheets.Add("معلومات الطالب");
        CreateStudentInfoSheet(infoSheet, report);
        
        // Sessions Sheet
        var sessionsSheet = package.Workbook.Worksheets.Add("جلسات الاستماع");
        await CreateStudentSessionsSheet(sessionsSheet, report);
        
        // Summary Sheet
        var summarySheet = package.Workbook.Worksheets.Add("الملخص");
        CreateStudentSummarySheet(summarySheet, report);
        
        return package.GetAsByteArray();
    }

    public async Task<byte[]> ExportStudentProgressToPdfAsync(Guid studentId, DateTime fromDate, DateTime toDate)
    {
        var report = await GetStudentProgressReportAsync(studentId, fromDate, toDate);
        var surahs = await _surahRepository.GetAllAsync();
        var surahDict = surahs.ToDictionary(s => s.SurahNumber, s => s.SurahNameArabic);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Tahoma").DirectionAuto());

                page.Header()
                    .Text($"تقرير تقدم الطالب - {report.Student.FullNameArabic}")
                    .SemiBold().FontSize(16).AlignCenter();

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(column =>
                    {
                        // Student Information
                        column.Item().Text("معلومات الطالب").SemiBold().FontSize(14).AlignRight();
                        column.Item().PaddingTop(5).Text($"الاسم: {report.Student.FullNameArabic}").AlignRight();
                        column.Item().Text($"رقم الهوية: {report.Student.IdNumber}").AlignRight();
                        column.Item().Text($"الصف: {report.Student.GradeLevel}").AlignRight();
                        column.Item().Text($"فترة التقرير: {fromDate:dd/MM/yyyy} - {toDate:dd/MM/yyyy}").AlignRight();

                        column.Item().PaddingTop(20).Text("ملخص الأداء").SemiBold().FontSize(14).AlignRight();
                        column.Item().PaddingTop(5).Text($"إجمالي الجلسات: {report.Summary.TotalSessions}").AlignRight();
                        column.Item().Text($"الجلسات المكتملة: {report.Summary.CompletedSessions}").AlignRight();
                        column.Item().Text($"معدل الإكمال: {report.Summary.CompletionRate:F1}%").AlignRight();
                        column.Item().Text($"متوسط الأخطاء الكبيرة: {report.Summary.AverageMajorErrors:F1}").AlignRight();
                        column.Item().Text($"متوسط الأخطاء الصغيرة: {report.Summary.AverageMinorErrors:F1}").AlignRight();

                        if (report.Sessions.Any())
                        {
                            column.Item().PaddingTop(20).Text("تفاصيل الجلسات").SemiBold().FontSize(14).AlignRight();
                            
                            column.Item().PaddingTop(10).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(50);  // Status
                                    columns.ConstantColumn(40);  // Minor Errors
                                    columns.ConstantColumn(40);  // Major Errors
                                    columns.RelativeColumn();   // Surah Range
                                    columns.RelativeColumn();   // Teacher
                                    columns.ConstantColumn(60);  // Date
                                });

                                // Headers (RTL order)
                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("الحالة").SemiBold().AlignRight();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("أخطاء صغيرة").SemiBold().AlignRight();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("أخطاء كبيرة").SemiBold().AlignRight();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("نطاق السور").SemiBold().AlignRight();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("المعلم").SemiBold().AlignRight();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("التاريخ").SemiBold().AlignRight();
                                });

                                // Data rows (RTL order)
                                foreach (var session in report.Sessions.OrderByDescending(s => s.SessionDate))
                                {
                                    var fromSurah = surahDict.GetValueOrDefault(session.SurahNumber, $"سورة {session.SurahNumber}");
                                    var range = $"{fromSurah} ({session.FromAyahNumber}-{session.ToAyahNumber})";

                                    table.Cell().Padding(3).Text(session.IsCompleted ? "مكتمل" : "غير مكتمل").AlignRight();
                                    table.Cell().Padding(3).Text(session.MinorErrorsCount.ToString()).AlignRight();
                                    table.Cell().Padding(3).Text(session.MajorErrorsCount.ToString()).AlignRight();
                                    table.Cell().Padding(3).Text(range).AlignRight();
                                    table.Cell().Padding(3).Text(session.Teacher?.FullNameArabic ?? "غير محدد").AlignRight();
                                    table.Cell().Padding(3).Text(session.SessionDate.ToString("dd/MM/yyyy")).AlignRight();
                                }
                            });
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text($"تم إنشاء التقرير في {DateTime.Now:dd/MM/yyyy HH:mm}")
                    .FontSize(10);
            });
        });

        return document.GeneratePdf();
    }

    #endregion

    #region Teacher Activity Report

    public async Task<TeacherActivityReportDto> GetTeacherActivityReportAsync(Guid teacherId, DateTime fromDate, DateTime toDate)
    {
        var teacher = await _userRepository.GetByIdAsync(teacherId);
        if (teacher == null || teacher.Role != UserRole.Teacher)
            throw new ArgumentException("Teacher not found");

        var sessions = await _sessionRepository.GetSessionsByTeacherIdAsync(teacherId, fromDate, toDate);
        
        var report = new TeacherActivityReportDto
        {
            Teacher = teacher,
            FromDate = fromDate,
            ToDate = toDate,
            Sessions = sessions.ToList(),
            Summary = CalculateTeacherActivitySummary(sessions.ToList())
        };

        return report;
    }

    public async Task<byte[]> ExportTeacherActivityToExcelAsync(Guid teacherId, DateTime fromDate, DateTime toDate)
    {
        var report = await GetTeacherActivityReportAsync(teacherId, fromDate, toDate);
        
        using var package = new ExcelPackage();
        
        // Teacher Info Sheet
        var infoSheet = package.Workbook.Worksheets.Add("معلومات المعلم");
        CreateTeacherInfoSheet(infoSheet, report);
        
        // Sessions Sheet
        var sessionsSheet = package.Workbook.Worksheets.Add("الجلسات المسجلة");
        await CreateTeacherSessionsSheet(sessionsSheet, report);
        
        // Summary Sheet
        var summarySheet = package.Workbook.Worksheets.Add("الملخص");
        CreateTeacherSummarySheet(summarySheet, report);
        
        return package.GetAsByteArray();
    }

    public async Task<byte[]> ExportTeacherActivityToPdfAsync(Guid teacherId, DateTime fromDate, DateTime toDate)
    {
        var report = await GetTeacherActivityReportAsync(teacherId, fromDate, toDate);
        var surahs = await _surahRepository.GetAllAsync();
        var surahDict = surahs.ToDictionary(s => s.SurahNumber, s => s.SurahNameArabic);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Tahoma").DirectionAuto());

                page.Header()
                    .Text($"تقرير نشاط المعلم - {report.Teacher.FullNameArabic}")
                    .SemiBold().FontSize(16).AlignCenter();

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(column =>
                    {
                        // Teacher Information
                        column.Item().Text("معلومات المعلم").SemiBold().FontSize(14);
                        column.Item().PaddingTop(5).Text($"الاسم: {report.Teacher.FullNameArabic}");
                        column.Item().Text($"اسم المستخدم: {report.Teacher.Username}");
                        column.Item().Text($"فترة التقرير: {report.FromDate:dd/MM/yyyy} - {report.ToDate:dd/MM/yyyy}");

                        column.Item().PaddingTop(20).Text("ملخص النشاط").SemiBold().FontSize(14);
                        column.Item().PaddingTop(5).Text($"إجمالي الجلسات: {report.Summary.TotalSessions}");
                        column.Item().Text($"الجلسات المكتملة: {report.Summary.CompletedSessions}");
                        column.Item().Text($"عدد الطلاب: {report.Summary.UniqueStudents}");
                        column.Item().Text($"معدل الإكمال: {report.Summary.CompletionRate:F1}%");

                        // Sessions by student summary
                        if (report.Summary.SessionsByStudent.Any())
                        {
                            column.Item().PaddingTop(20).Text("ملخص الجلسات حسب الطالب").SemiBold().FontSize(14);
                            
                            column.Item().PaddingTop(10).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(60);  // Minor Errors
                                    columns.ConstantColumn(60);  // Major Errors
                                    columns.ConstantColumn(60);  // Completed
                                    columns.ConstantColumn(60);  // Total Sessions
                                    columns.RelativeColumn(2);   // Student Name
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("أخطاء صغيرة").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("أخطاء كبيرة").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("مكتملة").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("إجمالي الجلسات").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("اسم الطالب").SemiBold();
                                });

                                foreach (var studentSummary in report.Summary.SessionsByStudent.OrderByDescending(s => s.SessionCount))
                                {
                                    table.Cell().Padding(3).Text(studentSummary.MinorErrors.ToString());
                                    table.Cell().Padding(3).Text(studentSummary.MajorErrors.ToString());
                                    table.Cell().Padding(3).Text(studentSummary.CompletedCount.ToString());
                                    table.Cell().Padding(3).Text(studentSummary.SessionCount.ToString());
                                    table.Cell().Padding(3).Text(studentSummary.Student.FullNameArabic);
                                }
                            });
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text($"تم إنشاء التقرير في {DateTime.Now:dd/MM/yyyy HH:mm}")
                    .FontSize(10);
            });
        });

        return document.GeneratePdf();
    }

    #endregion

    #region System Summary Report

    public async Task<SystemSummaryReportDto> GetSystemSummaryReportAsync(DateTime fromDate, DateTime toDate)
    {
        var allUsers = await _userRepository.GetAllAsync();
        var allSessions = await _sessionRepository.GetSessionsByDateRangeAsync(fromDate, toDate);
        
        var teachers = allUsers.Where(u => u.Role == UserRole.Teacher).ToList();
        var students = allUsers.Where(u => u.Role == UserRole.Student).ToList();
        var sessions = allSessions.ToList();
        
        var report = new SystemSummaryReportDto
        {
            FromDate = fromDate,
            ToDate = toDate,
            Summary = CalculateSystemSummary(teachers, students, sessions),
            Teachers = CalculateTeacherSummaries(teachers, sessions),
            TopStudents = CalculateTopStudentSummaries(students, sessions, 10),
            RecentSessions = sessions.OrderByDescending(s => s.SessionDate).Take(20).ToList()
        };

        return report;
    }

    public async Task<byte[]> ExportSystemSummaryToExcelAsync(DateTime fromDate, DateTime toDate)
    {
        var report = await GetSystemSummaryReportAsync(fromDate, toDate);
        
        using var package = new ExcelPackage();
        
        // Overview Sheet
        var overviewSheet = package.Workbook.Worksheets.Add("نظرة عامة");
        CreateSystemOverviewSheet(overviewSheet, report);
        
        // Teachers Sheet
        var teachersSheet = package.Workbook.Worksheets.Add("المعلمون");
        CreateSystemTeachersSheet(teachersSheet, report);
        
        // Students Sheet
        var studentsSheet = package.Workbook.Worksheets.Add("الطلاب المتميزون");
        CreateSystemStudentsSheet(studentsSheet, report);
        
        // Recent Sessions Sheet
        var sessionsSheet = package.Workbook.Worksheets.Add("الجلسات الأخيرة");
        await CreateSystemSessionsSheet(sessionsSheet, report);
        
        return package.GetAsByteArray();
    }

    public async Task<byte[]> ExportSystemSummaryToPdfAsync(DateTime fromDate, DateTime toDate)
    {
        var report = await GetSystemSummaryReportAsync(fromDate, toDate);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Tahoma").DirectionAuto());

                page.Header()
                    .Text("تقرير ملخص النظام")
                    .SemiBold().FontSize(16).AlignCenter();

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(column =>
                    {
                        column.Item().Text($"فترة التقرير: {fromDate:dd/MM/yyyy} - {toDate:dd/MM/yyyy}").SemiBold();

                        column.Item().PaddingTop(20).Text("الإحصائيات العامة").SemiBold().FontSize(14);
                        column.Item().PaddingTop(5).Text($"إجمالي المعلمين: {report.Summary.TotalTeachers}");
                        column.Item().Text($"المعلمون النشطون: {report.Summary.ActiveTeachers}");
                        column.Item().Text($"إجمالي الطلاب: {report.Summary.TotalStudents}");
                        column.Item().Text($"الطلاب النشطون: {report.Summary.ActiveStudents}");
                        column.Item().Text($"إجمالي الجلسات: {report.Summary.TotalSessions}");
                        column.Item().Text($"الجلسات المكتملة: {report.Summary.CompletedSessions}");
                        column.Item().Text($"معدل الإكمال: {report.Summary.CompletionRate:F1}%");

                        if (report.Teachers.Any())
                        {
                            column.Item().PaddingTop(20).Text("أداء المعلمين").SemiBold().FontSize(14);
                            
                            column.Item().PaddingTop(10).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(60);  // Completion Rate
                                    columns.ConstantColumn(50);  // Students
                                    columns.ConstantColumn(50);  // Sessions
                                    columns.RelativeColumn(2);   // Teacher
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("معدل الإكمال").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("الطلاب").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("الجلسات").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("المعلم").SemiBold();
                                });

                                foreach (var teacher in report.Teachers.OrderByDescending(t => t.SessionsCount).Take(10))
                                {
                                    table.Cell().Padding(3).Text($"{teacher.CompletionRate:F1}%");
                                    table.Cell().Padding(3).Text(teacher.StudentsCount.ToString());
                                    table.Cell().Padding(3).Text(teacher.SessionsCount.ToString());
                                    table.Cell().Padding(3).Text(teacher.Teacher.FullNameArabic);
                                }
                            });
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text($"تم إنشاء التقرير في {DateTime.Now:dd/MM/yyyy HH:mm}")
                    .FontSize(10);
            });
        });

        return document.GeneratePdf();
    }

    #endregion

    #region Private Helper Methods

    private StudentProgressSummary CalculateStudentProgressSummary(List<ListeningSession> sessions)
    {
        var summary = new StudentProgressSummary
        {
            TotalSessions = sessions.Count,
            CompletedSessions = sessions.Count(s => s.IsCompleted),
            IncompleteSessions = sessions.Count(s => !s.IsCompleted),
            TotalMajorErrors = sessions.Sum(s => s.MajorErrorsCount),
            TotalMinorErrors = sessions.Sum(s => s.MinorErrorsCount)
        };

        // Group by teacher
        summary.SessionsByTeacher = sessions
            .Where(s => s.Teacher != null)
            .GroupBy(s => s.Teacher!)
            .Select(g => new TeacherSessionSummary
            {
                Teacher = g.Key,
                SessionCount = g.Count(),
                CompletedCount = g.Count(s => s.IsCompleted),
                MajorErrors = g.Sum(s => s.MajorErrorsCount),
                MinorErrors = g.Sum(s => s.MinorErrorsCount)
            })
            .OrderByDescending(t => t.SessionCount)
            .ToList();

        return summary;
    }

    private TeacherActivitySummary CalculateTeacherActivitySummary(List<ListeningSession> sessions)
    {
        var summary = new TeacherActivitySummary
        {
            TotalSessions = sessions.Count,
            CompletedSessions = sessions.Count(s => s.IsCompleted),
            IncompleteSessions = sessions.Count(s => !s.IsCompleted),
            UniqueStudents = sessions.Select(s => s.StudentUserId).Distinct().Count(),
            TotalMajorErrors = sessions.Sum(s => s.MajorErrorsCount),
            TotalMinorErrors = sessions.Sum(s => s.MinorErrorsCount)
        };

        // Group by student
        summary.SessionsByStudent = sessions
            .Where(s => s.Student != null)
            .GroupBy(s => s.Student!)
            .Select(g => new StudentSessionSummary
            {
                Student = g.Key,
                SessionCount = g.Count(),
                CompletedCount = g.Count(s => s.IsCompleted),
                MajorErrors = g.Sum(s => s.MajorErrorsCount),
                MinorErrors = g.Sum(s => s.MinorErrorsCount),
                LastSessionDate = g.Max(s => s.SessionDate)
            })
            .OrderByDescending(s => s.SessionCount)
            .ToList();

        return summary;
    }

    private SystemSummary CalculateSystemSummary(List<User> teachers, List<User> students, List<ListeningSession> sessions)
    {
        var activeTeacherIds = sessions.Select(s => s.TeacherUserId).Distinct().ToHashSet();
        var activeStudentIds = sessions.Select(s => s.StudentUserId).Distinct().ToHashSet();

        return new SystemSummary
        {
            TotalTeachers = teachers.Count,
            ActiveTeachers = teachers.Count(t => activeTeacherIds.Contains(t.Id)),
            TotalStudents = students.Count,
            ActiveStudents = students.Count(s => activeStudentIds.Contains(s.Id)),
            TotalSessions = sessions.Count,
            CompletedSessions = sessions.Count(s => s.IsCompleted),
            TotalMajorErrors = sessions.Sum(s => s.MajorErrorsCount),
            TotalMinorErrors = sessions.Sum(s => s.MinorErrorsCount)
        };
    }

    private List<TeacherSummary> CalculateTeacherSummaries(List<User> teachers, List<ListeningSession> sessions)
    {
        return sessions
            .Where(s => s.Teacher != null)
            .GroupBy(s => s.Teacher!)
            .Select(g => new TeacherSummary
            {
                Teacher = g.Key,
                SessionsCount = g.Count(),
                StudentsCount = g.Select(s => s.StudentUserId).Distinct().Count(),
                CompletedSessions = g.Count(s => s.IsCompleted),
                MajorErrors = g.Sum(s => s.MajorErrorsCount),
                MinorErrors = g.Sum(s => s.MinorErrorsCount)
            })
            .OrderByDescending(t => t.SessionsCount)
            .ToList();
    }

    private List<StudentSummary> CalculateTopStudentSummaries(List<User> students, List<ListeningSession> sessions, int topCount)
    {
        return sessions
            .Where(s => s.Student != null)
            .GroupBy(s => s.Student!)
            .Select(g => new StudentSummary
            {
                Student = g.Key,
                SessionsCount = g.Count(),
                CompletedSessions = g.Count(s => s.IsCompleted),
                MajorErrors = g.Sum(s => s.MajorErrorsCount),
                MinorErrors = g.Sum(s => s.MinorErrorsCount),
                LastSessionDate = g.Max(s => s.SessionDate)
            })
            .OrderByDescending(s => s.CompletionRate)
            .ThenByDescending(s => s.SessionsCount)
            .Take(topCount)
            .ToList();
    }

    #endregion

    #region Excel Helper Methods

    private void CreateStudentInfoSheet(ExcelWorksheet worksheet, StudentProgressReportDto report)
    {
        worksheet.Cells["A1"].Value = "معلومات الطالب";
        worksheet.Cells["A1"].Style.Font.Bold = true;
        worksheet.Cells["A1"].Style.Font.Size = 16;

        worksheet.Cells["A3"].Value = "الاسم:";
        worksheet.Cells["B3"].Value = report.Student.FullNameArabic;
        
        worksheet.Cells["A4"].Value = "رقم الهوية:";
        worksheet.Cells["B4"].Value = report.Student.IdNumber;
        
        worksheet.Cells["A5"].Value = "الصف:";
        worksheet.Cells["B5"].Value = report.Student.GradeLevel;
        
        worksheet.Cells["A6"].Value = "من تاريخ:";
        worksheet.Cells["B6"].Value = report.FromDate.ToString("dd/MM/yyyy");
        
        worksheet.Cells["A7"].Value = "إلى تاريخ:";
        worksheet.Cells["B7"].Value = report.ToDate.ToString("dd/MM/yyyy");

        // Auto-fit columns
        worksheet.Cells.AutoFitColumns();
        
        // RTL direction
        worksheet.View.RightToLeft = true;
    }

    private async Task CreateStudentSessionsSheet(ExcelWorksheet worksheet, StudentProgressReportDto report)
    {
        // Headers
        worksheet.Cells["A1"].Value = "التاريخ";
        worksheet.Cells["B1"].Value = "المعلم";
        worksheet.Cells["C1"].Value = "السورة";
        worksheet.Cells["D1"].Value = "من آية";
        worksheet.Cells["E1"].Value = "إلى آية";
        worksheet.Cells["F1"].Value = "أخطاء كبيرة";
        worksheet.Cells["G1"].Value = "أخطاء صغيرة";
        worksheet.Cells["H1"].Value = "مكتملة";
        worksheet.Cells["I1"].Value = "ملاحظات";

        // Header styling
        using (var range = worksheet.Cells["A1:I1"])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(DrawingColor.LightGray);
        }

        // Get Surah names
        var surahs = await _surahRepository.GetAllAsync();
        var surahDict = surahs.ToDictionary(s => s.SurahNumber, s => s.SurahNameArabic);

        // Data rows
        int row = 2;
        foreach (var session in report.Sessions.OrderByDescending(s => s.SessionDate))
        {
            worksheet.Cells[row, 1].Value = session.SessionDate.ToString("dd/MM/yyyy");
            worksheet.Cells[row, 2].Value = session.Teacher?.FullNameArabic ?? "غير محدد";
            worksheet.Cells[row, 3].Value = surahDict.GetValueOrDefault(session.SurahNumber, $"سورة {session.SurahNumber}");
            worksheet.Cells[row, 4].Value = session.FromAyahNumber;
            worksheet.Cells[row, 5].Value = session.ToAyahNumber;
            worksheet.Cells[row, 6].Value = session.MajorErrorsCount;
            worksheet.Cells[row, 7].Value = session.MinorErrorsCount;
            worksheet.Cells[row, 8].Value = session.IsCompleted ? "نعم" : "لا";
            worksheet.Cells[row, 9].Value = session.Notes ?? "";
            row++;
        }

        worksheet.Cells.AutoFitColumns();
        worksheet.View.RightToLeft = true;
    }

    private void CreateStudentSummarySheet(ExcelWorksheet worksheet, StudentProgressReportDto report)
    {
        worksheet.Cells["A1"].Value = "ملخص الأداء";
        worksheet.Cells["A1"].Style.Font.Bold = true;
        worksheet.Cells["A1"].Style.Font.Size = 16;

        worksheet.Cells["A3"].Value = "إجمالي الجلسات:";
        worksheet.Cells["B3"].Value = report.Summary.TotalSessions;
        
        worksheet.Cells["A4"].Value = "الجلسات المكتملة:";
        worksheet.Cells["B4"].Value = report.Summary.CompletedSessions;
        
        worksheet.Cells["A5"].Value = "معدل الإكمال:";
        worksheet.Cells["B5"].Value = $"{report.Summary.CompletionRate:F1}%";
        
        worksheet.Cells["A6"].Value = "متوسط الأخطاء الكبيرة:";
        worksheet.Cells["B6"].Value = report.Summary.AverageMajorErrors.ToString("F1");
        
        worksheet.Cells["A7"].Value = "متوسط الأخطاء الصغيرة:";
        worksheet.Cells["B7"].Value = report.Summary.AverageMinorErrors.ToString("F1");

        worksheet.Cells.AutoFitColumns();
        worksheet.View.RightToLeft = true;
    }

    private void CreateTeacherInfoSheet(ExcelWorksheet worksheet, TeacherActivityReportDto report)
    {
        worksheet.Cells["A1"].Value = "معلومات المعلم";
        worksheet.Cells["A1"].Style.Font.Bold = true;
        worksheet.Cells["A1"].Style.Font.Size = 16;

        worksheet.Cells["A3"].Value = "الاسم:";
        worksheet.Cells["B3"].Value = report.Teacher.FullNameArabic;
        
        worksheet.Cells["A4"].Value = "اسم المستخدم:";
        worksheet.Cells["B4"].Value = report.Teacher.Username;
        
        worksheet.Cells["A5"].Value = "من تاريخ:";
        worksheet.Cells["B5"].Value = report.FromDate.ToString("dd/MM/yyyy");
        
        worksheet.Cells["A6"].Value = "إلى تاريخ:";
        worksheet.Cells["B6"].Value = report.ToDate.ToString("dd/MM/yyyy");

        worksheet.Cells.AutoFitColumns();
        worksheet.View.RightToLeft = true;
    }

    private async Task CreateTeacherSessionsSheet(ExcelWorksheet worksheet, TeacherActivityReportDto report)
    {
        // Headers
        worksheet.Cells["A1"].Value = "التاريخ";
        worksheet.Cells["B1"].Value = "الطالب";
        worksheet.Cells["C1"].Value = "السورة";
        worksheet.Cells["D1"].Value = "من آية";
        worksheet.Cells["E1"].Value = "إلى آية";
        worksheet.Cells["F1"].Value = "أخطاء كبيرة";
        worksheet.Cells["G1"].Value = "أخطاء صغيرة";
        worksheet.Cells["H1"].Value = "مكتملة";
        worksheet.Cells["I1"].Value = "ملاحظات";

        // Header styling
        using (var range = worksheet.Cells["A1:I1"])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(DrawingColor.LightGray);
        }

        // Get Surah names
        var surahs = await _surahRepository.GetAllAsync();
        var surahDict = surahs.ToDictionary(s => s.SurahNumber, s => s.SurahNameArabic);

        // Data rows
        int row = 2;
        foreach (var session in report.Sessions.OrderByDescending(s => s.SessionDate))
        {
            worksheet.Cells[row, 1].Value = session.SessionDate.ToString("dd/MM/yyyy");
            worksheet.Cells[row, 2].Value = session.Student?.FullNameArabic ?? "غير محدد";
            worksheet.Cells[row, 3].Value = surahDict.GetValueOrDefault(session.SurahNumber, $"سورة {session.SurahNumber}");
            worksheet.Cells[row, 4].Value = session.FromAyahNumber;
            worksheet.Cells[row, 5].Value = session.ToAyahNumber;
            worksheet.Cells[row, 6].Value = session.MajorErrorsCount;
            worksheet.Cells[row, 7].Value = session.MinorErrorsCount;
            worksheet.Cells[row, 8].Value = session.IsCompleted ? "نعم" : "لا";
            worksheet.Cells[row, 9].Value = session.Notes ?? "";
            row++;
        }

        worksheet.Cells.AutoFitColumns();
        worksheet.View.RightToLeft = true;
    }

    private void CreateTeacherSummarySheet(ExcelWorksheet worksheet, TeacherActivityReportDto report)
    {
        worksheet.Cells["A1"].Value = "ملخص النشاط";
        worksheet.Cells["A1"].Style.Font.Bold = true;
        worksheet.Cells["A1"].Style.Font.Size = 16;

        worksheet.Cells["A3"].Value = "إجمالي الجلسات:";
        worksheet.Cells["B3"].Value = report.Summary.TotalSessions;
        
        worksheet.Cells["A4"].Value = "الجلسات المكتملة:";
        worksheet.Cells["B4"].Value = report.Summary.CompletedSessions;
        
        worksheet.Cells["A5"].Value = "عدد الطلاب:";
        worksheet.Cells["B5"].Value = report.Summary.UniqueStudents;
        
        worksheet.Cells["A6"].Value = "معدل الإكمال:";
        worksheet.Cells["B6"].Value = $"{report.Summary.CompletionRate:F1}%";

        worksheet.Cells.AutoFitColumns();
        worksheet.View.RightToLeft = true;
    }

    private void CreateSystemOverviewSheet(ExcelWorksheet worksheet, SystemSummaryReportDto report)
    {
        worksheet.Cells["A1"].Value = "ملخص النظام";
        worksheet.Cells["A1"].Style.Font.Bold = true;
        worksheet.Cells["A1"].Style.Font.Size = 16;

        worksheet.Cells["A3"].Value = "فترة التقرير:";
        worksheet.Cells["B3"].Value = $"{report.FromDate:dd/MM/yyyy} - {report.ToDate:dd/MM/yyyy}";

        worksheet.Cells["A5"].Value = "إجمالي المعلمين:";
        worksheet.Cells["B5"].Value = report.Summary.TotalTeachers;
        
        worksheet.Cells["A6"].Value = "المعلمون النشطون:";
        worksheet.Cells["B6"].Value = report.Summary.ActiveTeachers;
        
        worksheet.Cells["A7"].Value = "إجمالي الطلاب:";
        worksheet.Cells["B7"].Value = report.Summary.TotalStudents;
        
        worksheet.Cells["A8"].Value = "الطلاب النشطون:";
        worksheet.Cells["B8"].Value = report.Summary.ActiveStudents;
        
        worksheet.Cells["A9"].Value = "إجمالي الجلسات:";
        worksheet.Cells["B9"].Value = report.Summary.TotalSessions;
        
        worksheet.Cells["A10"].Value = "الجلسات المكتملة:";
        worksheet.Cells["B10"].Value = report.Summary.CompletedSessions;
        
        worksheet.Cells["A11"].Value = "معدل الإكمال:";
        worksheet.Cells["B11"].Value = $"{report.Summary.CompletionRate:F1}%";

        worksheet.Cells.AutoFitColumns();
        worksheet.View.RightToLeft = true;
    }

    private void CreateSystemTeachersSheet(ExcelWorksheet worksheet, SystemSummaryReportDto report)
    {
        // Headers
        worksheet.Cells["A1"].Value = "المعلم";
        worksheet.Cells["B1"].Value = "عدد الجلسات";
        worksheet.Cells["C1"].Value = "عدد الطلاب";
        worksheet.Cells["D1"].Value = "جلسات مكتملة";
        worksheet.Cells["E1"].Value = "معدل الإكمال";

        // Header styling
        using (var range = worksheet.Cells["A1:E1"])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(DrawingColor.LightGray);
        }

        // Data rows
        int row = 2;
        foreach (var teacher in report.Teachers)
        {
            worksheet.Cells[row, 1].Value = teacher.Teacher.FullNameArabic;
            worksheet.Cells[row, 2].Value = teacher.SessionsCount;
            worksheet.Cells[row, 3].Value = teacher.StudentsCount;
            worksheet.Cells[row, 4].Value = teacher.CompletedSessions;
            worksheet.Cells[row, 5].Value = $"{teacher.CompletionRate:F1}%";
            row++;
        }

        worksheet.Cells.AutoFitColumns();
        worksheet.View.RightToLeft = true;
    }

    private void CreateSystemStudentsSheet(ExcelWorksheet worksheet, SystemSummaryReportDto report)
    {
        // Headers
        worksheet.Cells["A1"].Value = "الطالب";
        worksheet.Cells["B1"].Value = "عدد الجلسات";
        worksheet.Cells["C1"].Value = "جلسات مكتملة";
        worksheet.Cells["D1"].Value = "معدل الإكمال";
        worksheet.Cells["E1"].Value = "آخر جلسة";

        // Header styling
        using (var range = worksheet.Cells["A1:E1"])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(DrawingColor.LightGray);
        }

        // Data rows
        int row = 2;
        foreach (var student in report.TopStudents)
        {
            worksheet.Cells[row, 1].Value = student.Student.FullNameArabic;
            worksheet.Cells[row, 2].Value = student.SessionsCount;
            worksheet.Cells[row, 3].Value = student.CompletedSessions;
            worksheet.Cells[row, 4].Value = $"{student.CompletionRate:F1}%";
            worksheet.Cells[row, 5].Value = student.LastSessionDate?.ToString("dd/MM/yyyy") ?? "";
            row++;
        }

        worksheet.Cells.AutoFitColumns();
        worksheet.View.RightToLeft = true;
    }

    private async Task CreateSystemSessionsSheet(ExcelWorksheet worksheet, SystemSummaryReportDto report)
    {
        // Headers
        worksheet.Cells["A1"].Value = "التاريخ";
        worksheet.Cells["B1"].Value = "المعلم";
        worksheet.Cells["C1"].Value = "الطالب";
        worksheet.Cells["D1"].Value = "السورة";
        worksheet.Cells["E1"].Value = "من آية";
        worksheet.Cells["F1"].Value = "إلى آية";
        worksheet.Cells["G1"].Value = "مكتملة";

        // Header styling
        using (var range = worksheet.Cells["A1:G1"])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(DrawingColor.LightGray);
        }

        // Get Surah names
        var surahs = await _surahRepository.GetAllAsync();
        var surahDict = surahs.ToDictionary(s => s.SurahNumber, s => s.SurahNameArabic);

        // Data rows
        int row = 2;
        foreach (var session in report.RecentSessions)
        {
            worksheet.Cells[row, 1].Value = session.SessionDate.ToString("dd/MM/yyyy");
            worksheet.Cells[row, 2].Value = session.Teacher?.FullNameArabic ?? "غير محدد";
            worksheet.Cells[row, 3].Value = session.Student?.FullNameArabic ?? "غير محدد";
            worksheet.Cells[row, 4].Value = surahDict.GetValueOrDefault(session.SurahNumber, $"سورة {session.SurahNumber}");
            worksheet.Cells[row, 5].Value = session.FromAyahNumber;
            worksheet.Cells[row, 6].Value = session.ToAyahNumber;
            worksheet.Cells[row, 7].Value = session.IsCompleted ? "نعم" : "لا";
            row++;
        }

        worksheet.Cells.AutoFitColumns();
        worksheet.View.RightToLeft = true;
    }

    #endregion
}
