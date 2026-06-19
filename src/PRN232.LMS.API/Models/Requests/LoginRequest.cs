using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.API.Models.Requests;

public class LoginRequest
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
