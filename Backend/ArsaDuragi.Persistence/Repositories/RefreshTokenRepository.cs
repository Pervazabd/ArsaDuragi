using ArsaDuragi.Application.Interfaces.Repositories;
using ArsaDuragi.Domain.Entities;
using ArsaDuragi.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ArsaDuragi.Persistence.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _db;

    public RefreshTokenRepository(AppDbContext db) => _db = db;

    public Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken ct = default)
        => _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

    public async Task AddAsync(RefreshToken token, CancellationToken ct = default)
        => await _db.RefreshTokens.AddAsync(token, ct);

    public async Task RevokeAllForUserAsync(string userId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var active = await _db.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null && t.ExpiresAt > now)
            .ToListAsync(ct);

        foreach (var token in active) token.RevokedAt = now;
    }
}
