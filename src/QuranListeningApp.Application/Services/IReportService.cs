using QuranListeningApp.Application.DTOs.Reports;

namespace QuranListeningApp.Application.Services;

public interface IReportService
{
    // Student Progress Report
    Task<StudentProgressReportDto> GetStudentProgressReportAsync(Guid studentId, DateTime fromDate, DateTime toDate);
    Task<byte[]> ExportStudentProgressToExcelAsync(Guid studentId, DateTime fromDate, DateTime toDate);
    Task<byte[]> ExportStudentProgressToPdfAsync(Guid studentId, DateTime fromDate, DateTime toDate);

    // Teacher Activity Report
    Task<TeacherActivityReportDto> GetTeacherActivityReportAsync(Guid teacherId, DateTime fromDate, DateTime toDate);
    Task<byte[]> ExportTeacherActivityToExcelAsync(Guid teacherId, DateTime fromDate, DateTime toDate);
    Task<byte[]> ExportTeacherActivityToPdfAsync(Guid teacherId, DateTime fromDate, DateTime toDate);

    // System Summary Report (Admin only)
    Task<SystemSummaryReportDto> GetSystemSummaryReportAsync(DateTime fromDate, DateTime toDate);
    Task<byte[]> ExportSystemSummaryToExcelAsync(DateTime fromDate, DateTime toDate);
    Task<byte[]> ExportSystemSummaryToPdfAsync(DateTime fromDate, DateTime toDate);
}