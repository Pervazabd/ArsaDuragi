using ArsaDuragi.Application.DTOs.PropertyAd;
using ArsaDuragi.Application.Interfaces.Services;
using ArsaDuragi.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArsaDuragi.API.Controllers;

[Authorize]
[Route("api/users")]
public class UserController : ApiControllerBase
{
    private readonly IUserService _users;
    private readonly IPropertyAdService _properties;

    public UserController(IUserService users, IPropertyAdService properties)
    {
        _users = users;
        _properties = properties;
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
        => Success(await _users.GetMeAsync(ct), "Profile retrieved successfully");

    /// <summary>All of my listings, including Pending / Rejected ones. Optional ?status=Pending filter.</summary>
    [HttpGet("me/properties")]
    public async Task<IActionResult> MyProperties([FromQuery] PropertyAdFilterDto filter, [FromQuery] PropertyStatus? status, CancellationToken ct)
        => Success(await _properties.GetMineAsync(filter, status, ct), "Your properties retrieved successfully");
}
