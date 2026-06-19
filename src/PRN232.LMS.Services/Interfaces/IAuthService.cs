using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Interfaces;

public interface IAuthService
{
    Task<AuthTokensModel?> LoginAsync(string username, string password);
    Task<AuthTokensModel?> RefreshAsync(string refreshToken);
}
