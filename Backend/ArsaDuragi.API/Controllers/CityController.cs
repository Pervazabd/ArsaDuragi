using ArsaDuragi.Application.Common;
using ArsaDuragi.Application.DTOs.Location;
using ArsaDuragi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArsaDuragi.API.Controllers;

[Route("api/cities")]
public class CityController : ApiControllerBase
{
    private readonly ILocationService _locations;

    public CityController(ILocationService locations) => _locations = locations;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Success(await _locations.GetCitiesAsync(ct), "Cities retrieved successfully");

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CityUpsertDto dto, CancellationToken ct)
        => CreatedResponse(await _locations.CreateCityAsync(dto, ct), "City created successfully");

    [Authorize(Roles = Roles.Admin)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CityUpsertDto dto, CancellationToken ct)
        => Success(await _locations.UpdateCityAsync(id, dto, ct), "City updated successfully");

    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _locations.DeleteCityAsync(id, ct);
        return NoContent();
    }
}
