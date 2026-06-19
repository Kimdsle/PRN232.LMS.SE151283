using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.API.Models.Requests;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
