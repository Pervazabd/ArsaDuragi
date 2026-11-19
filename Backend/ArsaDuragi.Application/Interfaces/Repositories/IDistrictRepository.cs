using ArsaDuragi.Domain.Entities;

namespace ArsaDuragi.Application.Interfaces.Repositories;

public interface IDistrictRepository
{
    Task<IReadOnlyList<District>> GetAllAsync(int? cityId = null, CancellationToken ct = default);
    Task<District?> GetByIdAsync(int id, bool track = false, CancellationToken ct = default);
    Task<bool> NameExistsAsync(int cityId, string name, int? excludeId = null, CancellationToken ct = default);

    /// <summary>Counts soft-deleted listings too, because the foreign key still points at the district.</summary>
    Task<bool> HasPropertiesAsync(int districtId, CancellationToken ct = default);

    Task AddAsync(District district, CancellationToken ct = default);
    void Remove(District district);
}
