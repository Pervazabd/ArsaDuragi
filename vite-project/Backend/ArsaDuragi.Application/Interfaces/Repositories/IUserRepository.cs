using ArsaDuragi.Application.Common;
using ArsaDuragi.Domain.Entities;

namespace ArsaDuragi.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> FindByIdAsync(string id, CancellationToken ct = default);
    Task<User?> FindByEmailAsync(string email, CancellationToken ct = default);

    Task<IdentityOperationResult> CreateAsync(User user, string password);
    Task AddToRoleAsync(User user, string role);
    Task<IList<string>> GetRolesAsync(User user);

    /// <summary>Verifies the password and applies the lockout policy (5 wrong passwords => locked for 15 minutes).</summary>
    Task<CredentialCheckResult> CheckCredentialsAsync(User user, string password);

    Task<bool> IsLockedOutAsync(User user);
    Task SetLockedAsync(User user, bool locked);

    Task<PagedResult<User>> GetPagedAsync(string? search, int page, int pageSize, CancellationToken ct = default);
}
