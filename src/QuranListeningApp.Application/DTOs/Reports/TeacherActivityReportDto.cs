using QuranListeningApp.Domain.Entities;

namespace QuranListeningApp.Application.DTOs.Reports;

public class TeacherActivityReportDto
{
    public User Teacher { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<ListeningSession> Sessions { get; set; } = new();
    public TeacherActivitySummary Summary { get; set; } = new();
}

public class TeacherActivitySummary
{
    public int TotalSessions { get; set; }
    public int CompletedSessions { get; set; }
    public int IncompleteSessions { get; set; }
    public int UniqueStudents { get; set; }
    public int TotalMajorErrors { get; set; }
    public int TotalMinorErrors { get; set; }
    public double CompletionRate => TotalSessions > 0 ? (double)CompletedSessions / TotalSessions * 100 : 0;
    public double AverageMajorErrors => TotalSessions > 0 ? (double)TotalMajorErrors / TotalSessions : 0;
    public double AverageMinorErrors => TotalSessions > 0 ? (double)TotalMinorErrors / TotalSessions : 0;
    public List<StudentSessionSummary> SessionsByStudent { get; set; } = new();
}

public class StudentSessionSummary
{
    public User Student { get; set; } = null!;
    public int SessionCount { get; set; }
    public int CompletedCount { get; set; }
    public int MajorErrors { get; set; }
    public int MinorErrors { get; set; }
    public DateTime? LastSessionDate { get; set; }
}