namespace PRN232.LMS.Services.Models;

public class SubjectBusinessModel
{
    public int SubjectId { get; set; }
    public string SubjectCode { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public int Credit { get; set; }
}
