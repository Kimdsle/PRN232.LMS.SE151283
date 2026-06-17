using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Common;
using PRN232.LMS.API.Models.Requests;
using PRN232.LMS.API.Models.Responses;
using PRN232.LMS.Services.Interfaces;
using System.Security.Claims;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[ApiVersionNeutral]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    /// <summary>Authenticates a user and returns JWT access and refresh tokens.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var tokens = await _authService.LoginAsync(request.Username, request.Password);
        if (tokens is null) return Unauthorized(ApiResponse<object>.Fail("Invalid username or password"));
        var response = new LoginResponse { AccessToken = tokens.AccessToken, RefreshToken = tokens.RefreshToken, ExpiresIn = tokens.ExpiresIn };
        return Ok(ApiResponse<LoginResponse>.Ok(response, "Login successful"));
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var username = User.FindFirstValue(ClaimTypes.Name);
        var role = User.FindFirstValue(ClaimTypes.Role);
        return Ok(ApiResponse<object>.Ok(new { userId, username, role }, "Current user"));
    }
}
