using ArsaDuragi.Application.Interfaces.Repositories;
using ArsaDuragi.Domain.Entities;
using ArsaDuragi.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ArsaDuragi.Persistence.Repositories;

public class DistrictRepository : IDistrictRepository
{
    private readonly AppDbContext _db;

    public DistrictRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<District>> GetAllAsync(int? cityId = null, CancellationToken ct = default)
    {
        var query = _db.Districts.AsNoTracking().Include(d => d.City).AsQueryable();
        if (cityId.HasValue) query = query.Where(d => d.CityId == cityId.Value);
        return await query.OrderBy(d => d.Name).ToListAsync(ct);
    }

    public Task<District?> GetByIdAsync(int id, bool track = false, CancellationToken ct = default)
    {
        var query = _db.Districts.Include(d => d.City).AsQueryable();
        return (track ? query : query.AsNoTracking()).FirstOrDefaultAsync(d => d.Id == id, ct);
    }

    public Task<bool> NameExistsAsync(int cityId, string name, int? excludeId = null, CancellationToken ct = default)
        => _db.Districts.AnyAsync(d => d.CityId == cityId && d.Name == name && (excludeId == null || d.Id != excludeId), ct);

    public Task<bool> HasPropertiesAsync(int districtId, CancellationToken ct = default)
        => _db.PropertyAds.IgnoreQueryFilters().AnyAsync(p => p.DistrictId == districtId, ct);

    public async Task AddAsync(District district, CancellationToken ct = default)
        => await _db.Districts.AddAsync(district, ct);

    public void Remove(District district) => _db.Districts.Remove(district);
}
