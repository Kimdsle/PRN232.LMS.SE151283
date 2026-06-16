using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.API.Models.Requests;

public class EnrollmentCreateRequest
{
    [Range(1, int.MaxValue)]
    public int StudentId { get; set; }

    [Range(1, int.MaxValue)]
    public int CourseId { get; set; }

    public DateTime EnrollDate { get; set; }

    [Required]
    [StringLength(50)]
    [RegularExpression(@"^[A-Za-z ]+$")]
    public string Status { get; set; } = string.Empty;
}