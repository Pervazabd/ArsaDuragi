using ArsaDuragi.Application.Common;
using ArsaDuragi.Application.DTOs.Admin;
using ArsaDuragi.Application.DTOs.PropertyAd;
using ArsaDuragi.Application.Interfaces.Services;
using ArsaDuragi.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArsaDuragi.API.Controllers;

/// <summary>Moderation and user management. Every action needs the Admin role (checked here AND again in the services).</summary>
[Authorize(Roles = Roles.Admin)]
[Route("api/admin")]
public class AdminController : ApiControllerBase
{
    private readonly IPropertyAdService _properties;
    private readonly IUserService _users;

    public AdminController(IPropertyAdService properties, IUserService users)
    {
        _properties = properties;
        _users = users;
    }

    /// <summary>All listings in any status. Typical use: ?status=Pending to see the moderation queue.</summary>
    [HttpGet("properties")]
    public async Task<IActionResult> GetProperties([FromQuery] PropertyAdFilterDto filter, [FromQuery] PropertyStatus? status, CancellationToken ct)
        => Success(await _properties.GetAllForAdminAsync(filter, status, ct), "Properties retrieved successfully");

    [HttpPost("properties/{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, CancellationToken ct)
        => Success(await _properties.ApproveAsync(id, ct), "Property approved successfully");

    [HttpPost("properties/{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] RejectPropertyAdDto dto, CancellationToken ct)
        => Success(await _properties.RejectAsync(id, dto, ct), "Property rejected successfully");

    /// <summary>Only an admin can mark a listing as featured.</summary>
    [HttpPut("properties/{id:int}/featured")]
    public async Task<IActionResult> SetFeatured(int id, [FromBody] SetFeaturedDto dto, CancellationToken ct)
        => Success(await _properties.SetFeaturedAsync(id, dto.IsFeatured, ct), "Featured flag updated successfully");

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] UserListFilterDto filter, CancellationToken ct)
        => Success(await _users.GetUsersAsync(filter, ct), "Users retrieved successfully");

    [HttpPut("users/{id}/lock")]
    public async Task<IActionResult> LockUser(string id, CancellationToken ct)
    {
        await _users.LockUserAsync(id, ct);
        return NoContent();
    }

    [HttpPut("users/{id}/unlock")]
    public async Task<IActionResult> UnlockUser(string id, CancellationToken ct)
    {
        await _users.UnlockUserAsync(id, ct);
        return NoContent();
    }
}
