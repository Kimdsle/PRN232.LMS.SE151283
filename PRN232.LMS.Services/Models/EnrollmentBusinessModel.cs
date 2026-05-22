namespace PRN232.LMS.Services.Models;

public class EnrollmentBusinessModel
{
    public int EnrollmentId { get; set; }
    public int StudentId { get; set; }
    public string? StudentName { get; set; }
    public int CourseId { get; set; }
    public string? CourseName { get; set; }
    public DateTime EnrollDate { get; set; }
    public string Status { get; set; } = string.Empty;
}
