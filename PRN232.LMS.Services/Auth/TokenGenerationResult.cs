namespace PRN232.LMS.Services.Auth;

public class TokenGenerationResult
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public DateTime RefreshTokenExpiresAt { get; set; }
}
