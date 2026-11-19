using ArsaDuragi.Domain.Entities;

namespace ArsaDuragi.Application.Interfaces.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken ct = default);
    Task AddAsync(RefreshToken token, CancellationToken ct = default);

    /// <summary>Marks every still-active token of the user as revoked (used on reuse detection, lock-out and logout-all).</summary>
    Task RevokeAllForUserAsync(string userId, CancellationToken ct = default);
}
