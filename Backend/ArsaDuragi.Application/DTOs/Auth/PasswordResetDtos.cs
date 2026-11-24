namespace ArsaDuragi.Application.DTOs.Auth;

public class EmailRequestDto
{
    public string Email { get; set; } = string.Empty;
}

public sealed class ResetPasswordDto : EmailRequestDto
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}