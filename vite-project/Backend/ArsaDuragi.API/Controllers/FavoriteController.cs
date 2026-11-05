using ArsaDuragi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArsaDuragi.API.Controllers;

[Authorize]
[Route("api/favorites")]
public class FavoriteController : ApiControllerBase
{
    private readonly IFavoriteService _favorites;

    public FavoriteController(IFavoriteService favorites) => _favorites = favorites;

    [HttpGet]
    public async Task<IActionResult> GetMine([FromQuery] int page = 1, [FromQuery] int pageSize = 12, CancellationToken ct = default)
        => Success(await _favorites.GetMineAsync(page, pageSize, ct), "Favorites retrieved successfully");

    /// <summary>Only the ids: lets the frontend paint the heart icons on the listing grid with one cheap request.</summary>
    [HttpGet("ids")]
    public async Task<IActionResult> GetIds(CancellationToken ct)
        => Success(await _favorites.GetMyPropertyIdsAsync(ct), "Favorite ids retrieved successfully");

    [HttpPost("{propertyId:int}")]
    public async Task<IActionResult> Add(int propertyId, CancellationToken ct)
        => CreatedResponse(await _favorites.AddAsync(propertyId, ct), "Property added to favorites");

    [HttpDelete("{propertyId:int}")]
    public async Task<IActionResult> Remove(int propertyId, CancellationToken ct)
    {
        await _favorites.RemoveAsync(propertyId, ct);
        return NoContent();
    }
}
