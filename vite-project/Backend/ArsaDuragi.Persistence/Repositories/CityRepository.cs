using ArsaDuragi.Application.Interfaces.Repositories;
using ArsaDuragi.Domain.Entities;
using ArsaDuragi.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ArsaDuragi.Persistence.Repositories;

public class CityRepository : ICityRepository
{
    private readonly AppDbContext _db;

    public CityRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<City>> GetAllAsync(CancellationToken ct = default)
        => await _db.Cities.AsNoTracking().OrderBy(c => c.Name).ToListAsync(ct);

    public Task<City?> GetByIdAsync(int id, bool track = false, CancellationToken ct = default)
        => (track ? _db.Cities : _db.Cities.AsNoTracking()).FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default)
        => _db.Cities.AnyAsync(c => c.Name == name && (excludeId == null || c.Id != excludeId), ct);

    public Task<bool> HasDistrictsAsync(int cityId, CancellationToken ct = default)
        => _db.Districts.AnyAsync(d => d.CityId == cityId, ct);

    public async Task AddAsync(City city, CancellationToken ct = default)
        => await _db.Cities.AddAsync(city, ct);

    public void Remove(City city) => _db.Cities.Remove(city);
}
