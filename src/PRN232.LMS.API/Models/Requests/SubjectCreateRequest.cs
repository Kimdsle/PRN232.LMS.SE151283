using System.ComponentModel.DataAnnotations;
using PRN232.LMS.API.Validation;

namespace PRN232.LMS.API.Models.Requests;

public class SubjectCreateRequest
{
    [Required]
    [FptCode]
    public string SubjectCode { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string SubjectName { get; set; } = string.Empty;

    [Range(1, 10)]
    public int Credit { get; set; }
}