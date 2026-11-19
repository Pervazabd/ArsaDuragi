using ArsaDuragi.Domain.Entities;

namespace ArsaDuragi.Application.Interfaces.Repositories;

public interface ICityRepository
{
    Task<IReadOnlyList<City>> GetAllAsync(CancellationToken ct = default);
    Task<City?> GetByIdAsync(int id, bool track = false, CancellationToken ct = default);
    Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default);
    Task<bool> HasDistrictsAsync(int cityId, CancellationToken ct = default);
    Task AddAsync(City city, CancellationToken ct = default);
    void Remove(City city);
}
