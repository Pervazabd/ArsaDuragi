using ArsaDuragi.Application.Common;
using ArsaDuragi.Application.Interfaces.Repositories;
using ArsaDuragi.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ArsaDuragi.Persistence.Repositories;

/// <summary>Thin wrapper around ASP.NET Core Identity's UserManager so the Application layer stays free of Identity details.</summary>
public class UserRepository : IUserRepository
{
    private readonly UserManager<User> _userManager;

    public UserRepository(UserManager<User> userManager) => _userManager = userManager;

    public Task<User?> FindByIdAsync(string id, CancellationToken ct = default)
        => _userManager.FindByIdAsync(id);

    public Task<User?> FindByEmailAsync(string email, CancellationToken ct = default)
        => _userManager.FindByEmailAsync(email);

    public async Task<IdentityOperationResult> CreateAsync(User user, string password)
    {
        var result = await _userManager.CreateAsync(user, password);
        return new IdentityOperationResult(result.Succeeded, result.Errors.Select(e => e.Description).ToList());
    }

    public async Task AddToRoleAsync(User user, string role)
    {
        var result = await _userManager.AddToRoleAsync(user, role);
        if (!result.Succeeded)
            throw new InvalidOperationException("Could not assign role: " + string.Join("; ", result.Errors.Select(e => e.Description)));
    }

    public Task<IList<string>> GetRolesAsync(User user) => _userManager.GetRolesAsync(user);

    public async Task<CredentialCheckResult> CheckCredentialsAsync(User user, string password)
    {
        if (await _userManager.IsLockedOutAsync(user)) return CredentialCheckResult.LockedOut;

        if (await _userManager.CheckPasswordAsync(user, password))
        {
            await _userManager.ResetAccessFailedCountAsync(user);
            return CredentialCheckResult.Success;
        }

        await _userManager.AccessFailedAsync(user); // counts the failure and locks after the configured maximum
        return await _userManager.IsLockedOutAsync(user) ? CredentialCheckResult.LockedOut : CredentialCheckResult.InvalidPassword;
    }

    public Task<bool> IsLockedOutAsync(User user) => _userManager.IsLockedOutAsync(user);

    public async Task SetLockedAsync(User user, bool locked)
    {
        await _userManager.SetLockoutEnabledAsync(user, true);
        await _userManager.SetLockoutEndDateAsync(user, locked ? DateTimeOffset.MaxValue : null);
        if (!locked) await _userManager.ResetAccessFailedCountAsync(user);
    }

    public async Task<PagedResult<User>> GetPagedAsync(string? search, int page, int pageSize, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _userManager.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");
            var pattern = $"%{s}%";
            query = query.Where(u => EF.Functions.Like(u.FullName, pattern) || (u.Email != null && EF.Functions.Like(u.Email, pattern)));
        }

        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(u => u.CreatedAt).ThenBy(u => u.Id)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return new PagedResult<User> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }
    public Task<string> GeneratePasswordResetTokenAsync(User user)
    => _userManager.GeneratePasswordResetTokenAsync(user);

    public async Task<IdentityOperationResult> ResetPasswordAsync(
        User user, string token, string password)
    {
        var result = await _userManager.ResetPasswordAsync(user, token, password);
        return new IdentityOperationResult(
            result.Succeeded,
            result.Errors.Select(e => e.Description).ToList());
    }
}
