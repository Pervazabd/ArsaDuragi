using ArsaDuragi.Application.DTOs.Auth;
using ArsaDuragi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ArsaDuragi.API.Controllers;

[Route("api/auth")]
[EnableRateLimiting("auth")]
public class AuthController : ApiControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken ct)
        => CreatedResponse(await _auth.RegisterAsync(dto, ct), "Registration successful");

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken ct)
        => Success(await _auth.LoginAsync(dto, ct), "Login successful");

    /// <summary>Exchanges a valid refresh token for a new access token + a new refresh token (rotation).</summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto dto, CancellationToken ct)
        => Success(await _auth.RefreshAsync(dto, ct), "Token refreshed successfully");

    /// <summary>Revokes the given refresh token. Anonymous on purpose: the access token may already have expired.</summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto dto, CancellationToken ct)
    {
        await _auth.LogoutAsync(dto, ct);
        return NoContent();
    }
}
