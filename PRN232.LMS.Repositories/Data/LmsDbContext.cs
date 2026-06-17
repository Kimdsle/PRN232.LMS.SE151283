using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Data;

public class LmsDbContext : DbContext
{
    public LmsDbContext(DbContextOptions<LmsDbContext> options) : base(options) { }

    public DbSet<Semester> Semesters => Set<Semester>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── Fluent API: column types & constraints ─────────────────
        modelBuilder.Entity<Semester>(e =>
        {
            e.ToTable("Semester");
            e.HasKey(x => x.SemesterId);
            e.Property(x => x.SemesterName).HasMaxLength(100).IsRequired();
            e.Property(x => x.StartDate).HasColumnType("datetime2");
            e.Property(x => x.EndDate).HasColumnType("datetime2");
        });

        modelBuilder.Entity<Course>(e =>
        {
            e.ToTable("Course");
            e.HasKey(x => x.CourseId);
            e.Property(x => x.CourseName).HasMaxLength(100).IsRequired();
            e.HasOne(x => x.Semester)
                .WithMany(s => s.Courses)
                .HasForeignKey(x => x.SemesterId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Subject>(e =>
        {
            e.ToTable("Subject");
            e.HasKey(x => x.SubjectId);
            e.Property(x => x.SubjectCode).HasMaxLength(20).IsRequired();
            e.Property(x => x.SubjectName).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<Student>(e =>
        {
            e.ToTable("Student");
            e.HasKey(x => x.StudentId);
            e.Property(x => x.FullName).HasMaxLength(100).IsRequired();
            e.Property(x => x.Email).HasMaxLength(100).IsRequired();
            e.Property(x => x.DateOfBirth).HasColumnType("datetime2");
        });

        modelBuilder.Entity<Enrollment>(e =>
        {
            e.ToTable("Enrollment");
            e.HasKey(x => x.EnrollmentId);
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.Property(x => x.EnrollDate).HasColumnType("datetime2");
            e.HasOne(x => x.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("User");
            e.HasKey(x => x.UserId);
            e.Property(x => x.Username).HasMaxLength(100).IsRequired();
            e.Property(x => x.PasswordHash).IsRequired();
            e.Property(x => x.Role).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.Username).IsUnique();
        });

        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.ToTable("RefreshToken");
            e.HasKey(x => x.RefreshTokenId);
            e.Property(x => x.Token).HasMaxLength(256).IsRequired();
            e.Property(x => x.ExpiresAt).HasColumnType("datetime2");
            e.Property(x => x.CreatedAt).HasColumnType("datetime2");
            e.HasOne(x => x.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Seed data: deterministic, exact counts per lab spec ────
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // 5 Semesters
        var semesters = new List<Semester>();
        for (int i = 1; i <= 5; i++)
        {
            semesters.Add(new Semester
            {
                SemesterId = i,
                SemesterName = $"Semester {i}",
                StartDate = new DateTime(2024, 1, 1).AddMonths((i - 1) * 4),
                EndDate = new DateTime(2024, 4, 30).AddMonths((i - 1) * 4)
            });
        }
        modelBuilder.Entity<Semester>().HasData(semesters);

        // 10 Subjects
        var subjects = new List<Subject>();
        for (int i = 1; i <= 10; i++)
        {
            subjects.Add(new Subject
            {
                SubjectId = i,
                SubjectCode = $"SUB{i:D3}",
                SubjectName = $"Subject {i}",
                Credit = (i % 3) + 2  // credits in {2,3,4}
            });
        }
        modelBuilder.Entity<Subject>().HasData(subjects);

        // 20 Courses, distributed across 5 semesters (4 each)
        var courses = new List<Course>();
        for (int i = 1; i <= 20; i++)
        {
            courses.Add(new Course
            {
                CourseId = i,
                CourseName = $"Course {i}",
                SemesterId = ((i - 1) % 5) + 1
            });
        }
        modelBuilder.Entity<Course>().HasData(courses);

        // 50 Students
        var students = new List<Student>();
        for (int i = 1; i <= 50; i++)
        {
            students.Add(new Student
            {
                StudentId = i,
                FullName = $"Student {i:D3}",
                Email = $"student{i:D3}@lms.local",
                DateOfBirth = new DateTime(2003, 1, 1).AddDays((i * 11) % 365)
            });
        }
        modelBuilder.Entity<Student>().HasData(students);

        // 500 Enrollments (50 students × 10 courses each)
        var enrollments = new List<Enrollment>();
        int enrollmentId = 1;
        for (int s = 1; s <= 50; s++)
        {
            for (int c = 0; c < 10; c++)
            {
                int courseId = ((s + c - 1) % 20) + 1;
                enrollments.Add(new Enrollment
                {
                    EnrollmentId = enrollmentId++,
                    StudentId = s,
                    CourseId = courseId,
                    EnrollDate = new DateTime(2024, 1, 15).AddDays((enrollmentId * 3) % 730),
                    Status = (enrollmentId % 4) switch
                    {
                        0 => "Active",
                        1 => "Completed",
                        2 => "Pending",
                        _ => "Cancelled"
                    }
                });
            }
        }
        modelBuilder.Entity<Enrollment>().HasData(enrollments);
    }
}
