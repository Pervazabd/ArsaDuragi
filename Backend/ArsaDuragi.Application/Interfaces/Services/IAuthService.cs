using ArsaDuragi.Application.DTOs.Auth;

namespace ArsaDuragi.Application.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken ct = default);
    Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken ct = default);
    Task<AuthResponseDto> RefreshAsync(RefreshTokenRequestDto dto, CancellationToken ct = default);
    Task LogoutAsync(RefreshTokenRequestDto dto, CancellationToken ct = default);
    Task RequestPasswordResetAsync(EmailRequestDto dto, CancellationToken ct = default);
    Task ResetPasswordAsync(ResetPasswordDto dto, CancellationToken ct = default);
    Task RequestRegistrationVerificationAsync(EmailRequestDto dto, CancellationToken ct = default);
}
