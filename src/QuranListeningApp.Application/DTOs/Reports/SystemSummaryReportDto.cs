using QuranListeningApp.Domain.Entities;

namespace QuranListeningApp.Application.DTOs.Reports;

public class SystemSummaryReportDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public SystemSummary Summary { get; set; } = new();
    public List<TeacherSummary> Teachers { get; set; } = new();
    public List<StudentSummary> TopStudents { get; set; } = new();
    public List<ListeningSession> RecentSessions { get; set; } = new();
}

public class SystemSummary
{
    public int TotalTeachers { get; set; }
    public int ActiveTeachers { get; set; }
    public int TotalStudents { get; set; }
    public int ActiveStudents { get; set; }
    public int TotalSessions { get; set; }
    public int CompletedSessions { get; set; }
    public int TotalMajorErrors { get; set; }
    public int TotalMinorErrors { get; set; }
    public double CompletionRate => TotalSessions > 0 ? (double)CompletedSessions / TotalSessions * 100 : 0;
    public double AverageMajorErrors => TotalSessions > 0 ? (double)TotalMajorErrors / TotalSessions : 0;
    public double AverageMinorErrors => TotalSessions > 0 ? (double)TotalMinorErrors / TotalSessions : 0;
}

public class TeacherSummary
{
    public User Teacher { get; set; } = null!;
    public int SessionsCount { get; set; }
    public int StudentsCount { get; set; }
    public int CompletedSessions { get; set; }
    public int MajorErrors { get; set; }
    public int MinorErrors { get; set; }
    public double CompletionRate => SessionsCount > 0 ? (double)CompletedSessions / SessionsCount * 100 : 0;
}

public class StudentSummary
{
    public User Student { get; set; } = null!;
    public int SessionsCount { get; set; }
    public int CompletedSessions { get; set; }
    public int MajorErrors { get; set; }
    public int MinorErrors { get; set; }
    public double CompletionRate => SessionsCount > 0 ? (double)CompletedSessions / SessionsCount * 100 : 0;
    public DateTime? LastSessionDate { get; set; }
}