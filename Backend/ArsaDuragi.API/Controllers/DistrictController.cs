using ArsaDuragi.Application.Common;
using ArsaDuragi.Application.DTOs.Location;
using ArsaDuragi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArsaDuragi.API.Controllers;

[Route("api/districts")]
public class DistrictController : ApiControllerBase
{
    private readonly ILocationService _locations;

    public DistrictController(ILocationService locations) => _locations = locations;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? cityId, CancellationToken ct)
        => Success(await _locations.GetDistrictsAsync(cityId, ct), "Districts retrieved successfully");

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] DistrictUpsertDto dto, CancellationToken ct)
        => CreatedResponse(await _locations.CreateDistrictAsync(dto, ct), "District created successfully");

    [Authorize(Roles = Roles.Admin)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] DistrictUpsertDto dto, CancellationToken ct)
        => Success(await _locations.UpdateDistrictAsync(id, dto, ct), "District updated successfully");

    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _locations.DeleteDistrictAsync(id, ct);
        return NoContent();
    }
}
