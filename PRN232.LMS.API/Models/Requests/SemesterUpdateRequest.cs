namespace PRN232.LMS.API.Models.Requests;

public class SemesterUpdateRequest
{
    public string SemesterName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}