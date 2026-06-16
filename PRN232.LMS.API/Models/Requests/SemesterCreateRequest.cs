using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.API.Models.Requests;

public class SemesterCreateRequest
{
    [Required]
    [StringLength(100)]
    public string SemesterName { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}