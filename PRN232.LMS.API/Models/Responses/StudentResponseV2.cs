namespace PRN232.LMS.API.Models.Responses;

public class StudentResponseV2
{
    public int StudentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? Phone { get; set; }
    public int Age { get; set; }
}