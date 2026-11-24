using System.Net;
using System.Net.Mail;
using ArsaDuragi.Application.Common;
using ArsaDuragi.Application.DTOs.Auth;
using ArsaDuragi.Application.Exceptions;
using ArsaDuragi.Application.Interfaces.Repositories;
using ArsaDuragi.Application.Interfaces.Services;
using ArsaDuragi.Domain.Entities;

namespace ArsaDuragi.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IJwtService _jwt;
    private readonly IUnitOfWork _uow;
    private readonly IEmailService _email;
    private readonly IRegistrationEmailToken _registrationEmailToken;

    public AuthService(
        IUserRepository users,
        IRefreshTokenRepository refreshTokens,
        IJwtService jwt,
        IUnitOfWork uow,
        IEmailService email,
        IRegistrationEmailToken registrationEmailToken)
    {
        _users = users;
        _refreshTokens = refreshTokens;
        _jwt = jwt;
        _uow = uow;
        _email = email;
        _registrationEmailToken = registrationEmailToken;
    }

    public async Task RequestPasswordResetAsync(
        EmailRequestDto dto, CancellationToken ct = default)
    {
        var user = await _users.FindByEmailAsync(dto.Email.Trim(), ct);
        if (user?.Email is not { } email || user.PasswordHash is null)
            return;

        var token = await _users.GeneratePasswordResetTokenAsync(user);
        await _email.SendAsync(
            email,
            "ArsaDuragi — şifrəni sıfırla",
            $"<p>Şifrənizi sıfırlamaq üçün bu tokeni 30 dəqiqə ərzində daxil edin.</p><p>{WebUtility.HtmlEncode(token)}</p>",
            ct);
    }

    public async Task ResetPasswordAsync(
        ResetPasswordDto dto, CancellationToken ct = default)
    {
        const string invalid = "Password reset failed. Check the token and password requirements.";
        var user = await _users.FindByEmailAsync(dto.Email.Trim(), ct)
                   ?? throw new BadRequestException(invalid);

        // Identity saves the password change and the tracked token revocations together.
        await _refreshTokens.RevokeAllForUserAsync(user.Id, ct);
        var result = await _users.ResetPasswordAsync(user, dto.Token.Trim(), dto.NewPassword);

        if (!result.Succeeded)
            throw new BadRequestException(invalid, result.Errors);
    }

    public async Task RequestRegistrationVerificationAsync(
        EmailRequestDto dto, CancellationToken ct = default)
    {
        var email = NormalizeGmail(dto.Email);
        if (await _users.FindByEmailAsync(email, ct) is not null)
            return;

        var token = _registrationEmailToken.Create(email);
        await _email.SendAsync(
            email,
            "ArsaDuragi — Gmail ünvanını təsdiqlə",
            $"<p>Qeydiyyatı tamamlamaq üçün bu tokeni 30 dəqiqə ərzində daxil edin.</p><p>{WebUtility.HtmlEncode(token)}</p>",
            ct);
    }

    public async Task<AuthResponseDto> RegisterAsync(
        RegisterDto dto, CancellationToken ct = default)
    {
        var email = NormalizeGmail(dto.Email);
        if (string.IsNullOrWhiteSpace(dto.VerificationToken) ||
            !_registrationEmailToken.IsValid(email, dto.VerificationToken.Trim()))
        {
            throw new BadRequestException("Invalid or expired registration verification token.");
        }

        if (await _users.FindByEmailAsync(email, ct) is not null)
            throw new ConflictException("An account with this email already exists.");

        var user = new User
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = dto.FullName.Trim(),
            PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber.Trim()
        };

        // Password hashing is done inside ASP.NET Core Identity (PBKDF2 with a per-user salt).
        var result = await _users.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            throw new BadRequestException("Registration failed.", result.Errors);

        await _users.AddToRoleAsync(user, Roles.User);
        return await IssueTokensAsync(user, rotatedFrom: null, ct);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        const string invalid = "Invalid email or password.";

        var user = await _users.FindByEmailAsync(dto.Email.Trim(), ct)
                   ?? throw new UnauthorizedException(invalid);

        var check = await _users.CheckCredentialsAsync(user, dto.Password);
        if (check == CredentialCheckResult.LockedOut)
            throw new ForbiddenException("Too many failed attempts. The account is temporarily locked.");
        if (check != CredentialCheckResult.Success)
            throw new UnauthorizedException(invalid);

        return await IssueTokensAsync(user, rotatedFrom: null, ct);
    }

    public async Task<AuthResponseDto> RefreshAsync(
        RefreshTokenRequestDto dto, CancellationToken ct = default)
    {
        var hash = _jwt.HashToken(dto.RefreshToken);
        var stored = await _refreshTokens.GetByHashAsync(hash, ct)
                     ?? throw new UnauthorizedException("Invalid refresh token.");

        if (stored.RevokedAt is not null)
        {
            // An already-used token was presented again: somebody may have stolen it.
            // Revoke every token of that user so both the thief and the owner have to log in again.
            await _refreshTokens.RevokeAllForUserAsync(stored.UserId, ct);
            await _uow.SaveChangesAsync(ct);
            throw new UnauthorizedException("Refresh token has already been used. Please log in again.");
        }

        if (stored.ExpiresAt <= DateTime.UtcNow)
            throw new UnauthorizedException("Refresh token has expired. Please log in again.");

        var user = await _users.FindByIdAsync(stored.UserId, ct)
                   ?? throw new UnauthorizedException("Invalid refresh token.");

        if (await _users.IsLockedOutAsync(user))
        {
            await _refreshTokens.RevokeAllForUserAsync(user.Id, ct);
            await _uow.SaveChangesAsync(ct);
            throw new UnauthorizedException("Account is locked.");
        }

        // Rotation: the old token is revoked and replaced by a brand-new one.
        return await IssueTokensAsync(user, rotatedFrom: stored, ct);
    }

    public async Task LogoutAsync(RefreshTokenRequestDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.RefreshToken)) return;

        var stored = await _refreshTokens.GetByHashAsync(_jwt.HashToken(dto.RefreshToken), ct);
        if (stored is null || stored.RevokedAt is not null) return;

        stored.RevokedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync(ct);
    }

    private static string NormalizeGmail(string value)
    {
        var email = value.Trim().ToLowerInvariant();
        if (!MailAddress.TryCreate(email, out var address) ||
            address.Address != email || address.Host != "gmail.com")
        {
            throw new BadRequestException("Registration requires a valid @gmail.com address.");
        }

        return email;
    }

    private async Task<AuthResponseDto> IssueTokensAsync(
        User user, RefreshToken? rotatedFrom, CancellationToken ct)
    {
        var roles = await _users.GetRolesAsync(user);
        var access = _jwt.CreateAccessToken(user, roles);
        var refresh = _jwt.CreateRefreshToken();

        if (rotatedFrom is not null)
        {
            rotatedFrom.RevokedAt = DateTime.UtcNow;
            rotatedFrom.ReplacedByTokenHash = refresh.TokenHash;
        }

        await _refreshTokens.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refresh.TokenHash,
            ExpiresAt = refresh.ExpiresAt
        }, ct);
        await _uow.SaveChangesAsync(ct);

        return new AuthResponseDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Roles = roles.ToList(),
            AccessToken = access.Token,
            AccessTokenExpiresAt = access.ExpiresAt,
            RefreshToken = refresh.RawToken,
            RefreshTokenExpiresAt = refresh.ExpiresAt
        };
    }
}