using ArsaDuragi.Application.Common;
using ArsaDuragi.Application.DTOs.Location;
using ArsaDuragi.Application.Exceptions;
using ArsaDuragi.Application.Interfaces.Repositories;
using ArsaDuragi.Application.Interfaces.Services;
using ArsaDuragi.Domain.Entities;
using AutoMapper;

namespace ArsaDuragi.Application.Services;

public sealed class LocationService : ILocationService
{
    private readonly ICityRepository _cities;
    private readonly IDistrictRepository _districts;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _current;
    private readonly IMapper _mapper;

    public LocationService(ICityRepository cities, IDistrictRepository districts, IUnitOfWork uow,
        ICurrentUserService current, IMapper mapper)
    {
        _cities = cities;
        _districts = districts;
        _uow = uow;
        _current = current;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CityDto>> GetCitiesAsync(CancellationToken ct = default)
        => (await _cities.GetAllAsync(ct)).Select(c => _mapper.Map<CityDto>(c)).ToList();

    public async Task<CityDto> CreateCityAsync(CityUpsertDto dto, CancellationToken ct = default)
    {
        OwnershipGuard.EnsureAdmin(_current);
        var name = dto.Name.Trim();
        if (await _cities.NameExistsAsync(name, null, ct)) throw new ConflictException("A city with this name already exists.");

        var city = new City { Name = name };
        await _cities.AddAsync(city, ct);
        await _uow.SaveChangesAsync(ct);
        return _mapper.Map<CityDto>(city);
    }

    public async Task<CityDto> UpdateCityAsync(int id, CityUpsertDto dto, CancellationToken ct = default)
    {
        OwnershipGuard.EnsureAdmin(_current);
        var city = await _cities.GetByIdAsync(id, true, ct) ?? throw new NotFoundException("City not found.");
        var name = dto.Name.Trim();
        if (await _cities.NameExistsAsync(name, id, ct)) throw new ConflictException("A city with this name already exists.");

        city.Name = name;
        await _uow.SaveChangesAsync(ct);
        return _mapper.Map<CityDto>(city);
    }

    public async Task DeleteCityAsync(int id, CancellationToken ct = default)
    {
        OwnershipGuard.EnsureAdmin(_current);
        var city = await _cities.GetByIdAsync(id, true, ct) ?? throw new NotFoundException("City not found.");
        if (await _cities.HasDistrictsAsync(id, ct)) throw new ConflictException("Delete the districts of this city first.");

        _cities.Remove(city);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<DistrictDto>> GetDistrictsAsync(int? cityId, CancellationToken ct = default)
        => (await _districts.GetAllAsync(cityId, ct)).Select(d => _mapper.Map<DistrictDto>(d)).ToList();

    public async Task<DistrictDto> CreateDistrictAsync(DistrictUpsertDto dto, CancellationToken ct = default)
    {
        OwnershipGuard.EnsureAdmin(_current);
        var city = await _cities.GetByIdAsync(dto.CityId, true, ct) ?? throw new BadRequestException("The selected city does not exist.");
        var name = dto.Name.Trim();
        if (await _districts.NameExistsAsync(city.Id, name, null, ct)) throw new ConflictException("This city already has a district with this name.");

        var district = new District { Name = name, CityId = city.Id, City = city };
        await _districts.AddAsync(district, ct);
        await _uow.SaveChangesAsync(ct);
        return _mapper.Map<DistrictDto>(district);
    }

    public async Task<DistrictDto> UpdateDistrictAsync(int id, DistrictUpsertDto dto, CancellationToken ct = default)
    {
        OwnershipGuard.EnsureAdmin(_current);
        var district = await _districts.GetByIdAsync(id, true, ct) ?? throw new NotFoundException("District not found.");
        var city = await _cities.GetByIdAsync(dto.CityId, true, ct) ?? throw new BadRequestException("The selected city does not exist.");
        var name = dto.Name.Trim();
        if (await _districts.NameExistsAsync(city.Id, name, id, ct)) throw new ConflictException("This city already has a district with this name.");

        if (district.CityId != city.Id && await _districts.HasPropertiesAsync(id, ct))
            throw new ConflictException("A district that has listings cannot be moved to another city.");

        district.Name = name;
        district.CityId = city.Id;
        district.City = city;
        await _uow.SaveChangesAsync(ct);
        return _mapper.Map<DistrictDto>(district);
    }

    public async Task DeleteDistrictAsync(int id, CancellationToken ct = default)
    {
        OwnershipGuard.EnsureAdmin(_current);
        var district = await _districts.GetByIdAsync(id, true, ct) ?? throw new NotFoundException("District not found.");
        if (await _districts.HasPropertiesAsync(id, ct)) throw new ConflictException("This district still has listings.");

        _districts.Remove(district);
        await _uow.SaveChangesAsync(ct);
    }
}
