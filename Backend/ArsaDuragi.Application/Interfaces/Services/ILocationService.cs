using ArsaDuragi.Application.DTOs.Location;

namespace ArsaDuragi.Application.Interfaces.Services;

public interface ILocationService
{
    Task<IReadOnlyList<CityDto>> GetCitiesAsync(CancellationToken ct = default);
    Task<CityDto> CreateCityAsync(CityUpsertDto dto, CancellationToken ct = default);
    Task<CityDto> UpdateCityAsync(int id, CityUpsertDto dto, CancellationToken ct = default);
    Task DeleteCityAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyList<DistrictDto>> GetDistrictsAsync(int? cityId, CancellationToken ct = default);
    Task<DistrictDto> CreateDistrictAsync(DistrictUpsertDto dto, CancellationToken ct = default);
    Task<DistrictDto> UpdateDistrictAsync(int id, DistrictUpsertDto dto, CancellationToken ct = default);
    Task DeleteDistrictAsync(int id, CancellationToken ct = default);
}
