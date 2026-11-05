using ArsaDuragi.Application.Common;
using ArsaDuragi.Application.Interfaces.Services;

namespace ArsaDuragi.API.Services;

/// <summary>Reads the caller's identity from the validated JWT. The frontend can never choose its own user id.</summary>
public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _http;

    public CurrentUserService(IHttpContextAccessor http) => _http = http;

    public string? UserId => _http.HttpContext?.User.FindFirst("sub")?.Value;

    public bool IsAuthenticated => _http.HttpContext?.User.Identity?.IsAuthenticated == true;

    public bool IsAdmin => _http.HttpContext?.User.IsInRole(Roles.Admin) == true;
}
