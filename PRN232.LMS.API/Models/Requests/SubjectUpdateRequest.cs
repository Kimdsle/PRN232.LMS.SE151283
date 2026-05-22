namespace PRN232.LMS.API.Models.Requests;

public class SubjectUpdateRequest
{
    public string SubjectCode { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public int Credit { get; set; }
}