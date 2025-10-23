using Microsoft.EntityFrameworkCore;
using QuranListeningApp.Domain.Entities;
using QuranListeningApp.Domain.Enums;

namespace QuranListeningApp.Infrastructure.Data;

/// <summary>
/// Database context for the Quran Listening Management System
/// </summary>
public class QuranAppDbContext : DbContext
{
    public QuranAppDbContext(DbContextOptions<QuranAppDbContext> options)
        : base(options)
    {
    }

    // DbSets for all entities
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<ListeningSession> ListeningSessions { get; set; } = null!;
    public DbSet<SurahReference> SurahReferences { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public DbSet<AppSettings> AppSettings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);

            // Indexes for performance
            entity.HasIndex(e => e.Username)
                .IsUnique()
                .HasDatabaseName("IX_Users_Username");

            entity.HasIndex(e => e.IdNumber)
                .IsUnique()
                .HasFilter("[IdNumber] IS NOT NULL") // Partial index (IdNumber can be null for admins)
                .HasDatabaseName("IX_Users_IdNumber");

            entity.HasIndex(e => e.PhoneNumber)
                .HasDatabaseName("IX_Users_PhoneNumber");

            entity.HasIndex(e => e.FullNameArabic)
                .HasDatabaseName("IX_Users_FullNameArabic");

            entity.HasIndex(e => e.Role)
                .HasDatabaseName("IX_Users_Role");

            // Self-referential relationship: User created by another User
            entity.HasOne(e => e.CreatedByUser)
                .WithMany(e => e.CreatedUsers)
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            // Configure string properties with nvarchar for Arabic support
            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.PasswordHash)
                .IsRequired();

            entity.Property(e => e.FullNameArabic)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.IdNumber)
                .HasMaxLength(20);

            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20);

            entity.Property(e => e.Email)
                .HasMaxLength(100);

            entity.Property(e => e.GradeLevel)
                .HasMaxLength(50);

            // Store Role as integer
            entity.Property(e => e.Role)
                .IsRequired()
                .HasConversion<int>();
        });

        // Configure ListeningSession entity
        modelBuilder.Entity<ListeningSession>(entity =>
        {
            entity.ToTable("ListeningSessions");
            entity.HasKey(e => e.Id);

            // Indexes for performance
            entity.HasIndex(e => e.StudentUserId)
                .HasDatabaseName("IX_ListeningSessions_StudentUserId");

            entity.HasIndex(e => e.TeacherUserId)
                .HasDatabaseName("IX_ListeningSessions_TeacherUserId");

            entity.HasIndex(e => e.SessionDate)
                .HasDatabaseName("IX_ListeningSessions_SessionDate");

            entity.HasIndex(e => e.IsCompleted)
                .HasDatabaseName("IX_ListeningSessions_IsCompleted");

            // Relationship: ListeningSession -> User (Student)
            entity.HasOne(e => e.Student)
                .WithMany(u => u.SessionsAsStudent)
                .HasForeignKey(e => e.StudentUserId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            // Relationship: ListeningSession -> User (Teacher)
            entity.HasOne(e => e.Teacher)
                .WithMany(u => u.SessionsAsTeacher)
                .HasForeignKey(e => e.TeacherUserId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            // Configure properties
            entity.Property(e => e.SurahNumber)
                .IsRequired();

            entity.Property(e => e.FromAyahNumber)
                .IsRequired();

            entity.Property(e => e.ToAyahNumber)
                .IsRequired();

            entity.Property(e => e.MajorErrorsCount)
                .HasDefaultValue(0);

            entity.Property(e => e.MinorErrorsCount)
                .HasDefaultValue(0);

            entity.Property(e => e.IsCompleted)
                .HasDefaultValue(false);

            entity.Property(e => e.Grade)
                .HasPrecision(5, 2); // 999.99 (0-100 range, 2 decimal places)

            entity.Property(e => e.Notes)
                .HasColumnType("nvarchar(max)"); // For long Arabic text
        });

        // Configure SurahReference entity
        modelBuilder.Entity<SurahReference>(entity =>
        {
            entity.ToTable("SurahReferences");
            entity.HasKey(e => e.SurahNumber);

            // Disable identity for SurahNumber as we provide explicit values (1-114)
            entity.Property(e => e.SurahNumber)
                .ValueGeneratedNever();

            entity.Property(e => e.SurahNameArabic)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.SurahNameEnglish)
                .HasMaxLength(100);

            entity.Property(e => e.TotalAyahs)
                .IsRequired();
        });

        // Configure AuditLog entity
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.HasKey(e => e.Id);

            // Index on timestamp for querying audit history
            entity.HasIndex(e => e.Timestamp)
                .HasDatabaseName("IX_AuditLogs_Timestamp");

            // Index on entity type and ID for finding audits for specific entities
            entity.HasIndex(e => new { e.EntityType, e.EntityId })
                .HasDatabaseName("IX_AuditLogs_EntityType_EntityId");

            // Relationship: AuditLog -> User
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            // Configure properties
            entity.Property(e => e.Action)
                .IsRequired()
                .HasConversion<int>(); // Store as integer

            entity.Property(e => e.EntityType)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.OldValues)
                .HasColumnType("nvarchar(max)"); // JSON

            entity.Property(e => e.NewValues)
                .HasColumnType("nvarchar(max)"); // JSON

            entity.Property(e => e.IpAddress)
                .HasMaxLength(45); // IPv6 max length
        });

        // Configure AppSettings entity
        modelBuilder.Entity<AppSettings>(entity =>
        {
            entity.ToTable("AppSettings");
            entity.HasKey(e => e.Id);

            // Ensure single row table (Id = 1)
            entity.Property(e => e.Id)
                .ValueGeneratedNever(); // Don't auto-generate, always use 1

            entity.Property(e => e.SchoolNameArabic)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.SchoolLogoPath)
                .HasMaxLength(500);

            entity.Property(e => e.PledgeText)
                .HasColumnType("nvarchar(max)"); // For long Arabic text
        });
    }
}
