using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.API.Models.Requests;

public class StudentUpdateRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    [Phone]
    public string? Phone { get; set; }
}