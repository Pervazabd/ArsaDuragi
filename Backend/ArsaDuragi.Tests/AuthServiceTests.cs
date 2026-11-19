using ArsaDuragi.Application.Common;
using ArsaDuragi.Application.DTOs.Auth;
using ArsaDuragi.Application.Exceptions;
using ArsaDuragi.Application.Interfaces.Repositories;
using ArsaDuragi.Application.Interfaces.Services;
using ArsaDuragi.Application.Services;
using ArsaDuragi.Domain.Entities;
using Moq;
using Xunit;

namespace ArsaDuragi.Tests;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IRefreshTokenRepository> _tokens = new();
    private readonly Mock<IJwtService> _jwt = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _jwt.Setup(j => j.CreateAccessToken(It.IsAny<User>(), It.IsAny<IEnumerable<string>>()))
            .Returns(new AccessTokenResult("access-token", DateTime.UtcNow.AddMinutes(15)));
        _jwt.Setup(j => j.CreateRefreshToken())
            .Returns(new GeneratedRefreshToken("raw-new", "hash-new", DateTime.UtcNow.AddDays(7)));
        _jwt.Setup(j => j.HashToken(It.IsAny<string>())).Returns((string s) => "hash-of-" + s);

        _users.Setup(u => u.GetRolesAsync(It.IsAny<User>())).ReturnsAsync(new List<string> { "User" });
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _service = new AuthService(_users.Object, _tokens.Object, _jwt.Object, _uow.Object);
    }

    private static RegisterDto ValidRegister() => new()
    {
        FullName = "Test User", Email = "test@example.com", PhoneNumber = "+994501234567",
        Password = "Passw0rd1", ConfirmPassword = "Passw0rd1"
    };

    private static User ExistingUser() => new() { Id = "u1", Email = "test@example.com", UserName = "test@example.com", FullName = "Test User" };

    [Fact]
    public async Task Register_creates_user_assigns_User_role_and_returns_tokens()
    {
        _users.Setup(u => u.FindByEmailAsync("test@example.com", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        _users.Setup(u => u.CreateAsync(It.IsAny<User>(), "Passw0rd1")).ReturnsAsync(new IdentityOperationResult(true, Array.Empty<string>()));

        var result = await _service.RegisterAsync(ValidRegister());

        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("raw-new", result.RefreshToken);
        Assert.Contains("User", result.Roles);
        _users.Verify(u => u.AddToRoleAsync(It.IsAny<User>(), Roles.User), Times.Once);
        _tokens.Verify(t => t.AddAsync(It.Is<RefreshToken>(r => r.TokenHash == "hash-new"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Register_with_existing_email_is_a_conflict()
    {
        _users.Setup(u => u.FindByEmailAsync("test@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(ExistingUser());

        await Assert.ThrowsAsync<ConflictException>(() => _service.RegisterAsync(ValidRegister()));
        _users.Verify(u => u.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Register_surfaces_identity_errors_as_bad_request()
    {
        _users.Setup(u => u.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        _users.Setup(u => u.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(new IdentityOperationResult(false, new[] { "Password too weak" }));

        var ex = await Assert.ThrowsAsync<BadRequestException>(() => _service.RegisterAsync(ValidRegister()));
        Assert.Contains("Password too weak", ex.Errors);
    }

    [Fact]
    public async Task Login_with_valid_credentials_returns_tokens()
    {
        var user = ExistingUser();
        _users.Setup(u => u.FindByEmailAsync("test@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _users.Setup(u => u.CheckCredentialsAsync(user, "Passw0rd1")).ReturnsAsync(CredentialCheckResult.Success);

        var result = await _service.LoginAsync(new LoginDto { Email = "test@example.com", Password = "Passw0rd1" });

        Assert.Equal("u1", result.UserId);
        Assert.Equal("access-token", result.AccessToken);
    }

    [Fact]
    public async Task Login_with_wrong_password_or_unknown_email_is_unauthorized_with_same_message()
    {
        var user = ExistingUser();
        _users.Setup(u => u.FindByEmailAsync("test@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _users.Setup(u => u.FindByEmailAsync("nobody@example.com", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        _users.Setup(u => u.CheckCredentialsAsync(user, It.IsAny<string>())).ReturnsAsync(CredentialCheckResult.InvalidPassword);

        var wrongPassword = await Assert.ThrowsAsync<UnauthorizedException>(() => _service.LoginAsync(new LoginDto { Email = "test@example.com", Password = "bad" }));
        var unknownEmail = await Assert.ThrowsAsync<UnauthorizedException>(() => _service.LoginAsync(new LoginDto { Email = "nobody@example.com", Password = "bad" }));

        Assert.Equal(wrongPassword.Message, unknownEmail.Message); // does not reveal whether the email exists
    }

    [Fact]
    public async Task Login_of_locked_account_is_forbidden()
    {
        var user = ExistingUser();
        _users.Setup(u => u.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _users.Setup(u => u.CheckCredentialsAsync(user, It.IsAny<string>())).ReturnsAsync(CredentialCheckResult.LockedOut);

        await Assert.ThrowsAsync<ForbiddenException>(() => _service.LoginAsync(new LoginDto { Email = "test@example.com", Password = "x" }));
    }

    [Fact]
    public async Task Refresh_rotates_the_token()
    {
        var stored = new RefreshToken { UserId = "u1", TokenHash = "hash-of-old", ExpiresAt = DateTime.UtcNow.AddDays(1) };
        _tokens.Setup(t => t.GetByHashAsync("hash-of-old", It.IsAny<CancellationToken>())).ReturnsAsync(stored);
        _users.Setup(u => u.FindByIdAsync("u1", It.IsAny<CancellationToken>())).ReturnsAsync(ExistingUser());
        _users.Setup(u => u.IsLockedOutAsync(It.IsAny<User>())).ReturnsAsync(false);

        var result = await _service.RefreshAsync(new RefreshTokenRequestDto { RefreshToken = "old" });

        Assert.Equal("raw-new", result.RefreshToken);
        Assert.NotNull(stored.RevokedAt);                       // old token is now dead
        Assert.Equal("hash-new", stored.ReplacedByTokenHash);   // and points to its replacement
    }

    [Fact]
    public async Task Reusing_a_revoked_refresh_token_revokes_all_tokens_of_the_user()
    {
        var stored = new RefreshToken { UserId = "u1", TokenHash = "hash-of-old", ExpiresAt = DateTime.UtcNow.AddDays(1), RevokedAt = DateTime.UtcNow.AddMinutes(-1) };
        _tokens.Setup(t => t.GetByHashAsync("hash-of-old", It.IsAny<CancellationToken>())).ReturnsAsync(stored);

        await Assert.ThrowsAsync<UnauthorizedException>(() => _service.RefreshAsync(new RefreshTokenRequestDto { RefreshToken = "old" }));

        _tokens.Verify(t => t.RevokeAllForUserAsync("u1", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Unknown_or_expired_refresh_tokens_are_unauthorized()
    {
        _tokens.Setup(t => t.GetByHashAsync("hash-of-unknown", It.IsAny<CancellationToken>())).ReturnsAsync((RefreshToken?)null);
        _tokens.Setup(t => t.GetByHashAsync("hash-of-expired", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RefreshToken { UserId = "u1", ExpiresAt = DateTime.UtcNow.AddMinutes(-1) });

        await Assert.ThrowsAsync<UnauthorizedException>(() => _service.RefreshAsync(new RefreshTokenRequestDto { RefreshToken = "unknown" }));
        await Assert.ThrowsAsync<UnauthorizedException>(() => _service.RefreshAsync(new RefreshTokenRequestDto { RefreshToken = "expired" }));
    }

    [Fact]
    public async Task Refresh_for_locked_user_is_refused()
    {
        var stored = new RefreshToken { UserId = "u1", ExpiresAt = DateTime.UtcNow.AddDays(1) };
        _tokens.Setup(t => t.GetByHashAsync("hash-of-old", It.IsAny<CancellationToken>())).ReturnsAsync(stored);
        _users.Setup(u => u.FindByIdAsync("u1", It.IsAny<CancellationToken>())).ReturnsAsync(ExistingUser());
        _users.Setup(u => u.IsLockedOutAsync(It.IsAny<User>())).ReturnsAsync(true);

        await Assert.ThrowsAsync<UnauthorizedException>(() => _service.RefreshAsync(new RefreshTokenRequestDto { RefreshToken = "old" }));
    }

    [Fact]
    public async Task Logout_revokes_the_token()
    {
        var stored = new RefreshToken { UserId = "u1", ExpiresAt = DateTime.UtcNow.AddDays(1) };
        _tokens.Setup(t => t.GetByHashAsync("hash-of-old", It.IsAny<CancellationToken>())).ReturnsAsync(stored);

        await _service.LogoutAsync(new RefreshTokenRequestDto { RefreshToken = "old" });

        Assert.NotNull(stored.RevokedAt);
    }
}
