using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly ITokenService _tokenService;

    public AuthService(IUserRepository users, IRefreshTokenRepository refreshTokens, ITokenService tokenService)
    {
        _users = users;
        _refreshTokens = refreshTokens;
        _tokenService = tokenService;
    }

    public async Task<AuthTokensModel?> LoginAsync(string username, string password)
    {
        var user = await _users.Query().FirstOrDefaultAsync(u => u.Username == username);
        if (user is null) return null;
        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) return null;

        var tokens = _tokenService.GenerateTokens(user);
        await _refreshTokens.AddAsync(new RefreshToken
        {
            Token = tokens.RefreshToken,
            UserId = user.UserId,
            ExpiresAt = tokens.RefreshTokenExpiresAt,
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        });
        await _refreshTokens.SaveChangesAsync();

        return new AuthTokensModel { AccessToken = tokens.AccessToken, RefreshToken = tokens.RefreshToken, ExpiresIn = tokens.ExpiresIn };
    }

    public async Task<AuthTokensModel?> RefreshAsync(string refreshToken)
    {
        var stored = await _refreshTokens.Query().FirstOrDefaultAsync(rt => rt.Token == refreshToken);
        if (stored is null || stored.IsRevoked || stored.ExpiresAt <= DateTime.UtcNow)
            return null;

        var user = await _users.GetByIdAsync(stored.UserId);
        if (user is null) return null;

        var tokens = _tokenService.GenerateTokens(user);

        stored.IsRevoked = true;                       // rotate: revoke the presented token
        await _refreshTokens.UpdateAsync(stored);
        await _refreshTokens.AddAsync(new RefreshToken
        {
            Token = tokens.RefreshToken,
            UserId = user.UserId,
            ExpiresAt = tokens.RefreshTokenExpiresAt,
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        });
        await _refreshTokens.SaveChangesAsync();

        return new AuthTokensModel { AccessToken = tokens.AccessToken, RefreshToken = tokens.RefreshToken, ExpiresIn = tokens.ExpiresIn };
    }
}
