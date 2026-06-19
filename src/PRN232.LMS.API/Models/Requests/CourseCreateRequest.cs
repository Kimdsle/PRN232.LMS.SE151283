namespace PRN232.LMS.API.Models.Requests;

public class CourseCreateRequest
{
    public string CourseName { get; set; } = string.Empty;
    public int SemesterId { get; set; }
}