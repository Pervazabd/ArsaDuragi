using ArsaDuragi.Application.Exceptions;
using ArsaDuragi.Application.Interfaces.Services;

namespace ArsaDuragi.Application.Common;

/// <summary>Server-side ownership check. The user id always comes from the JWT, never from the request body.</summary>
public static class OwnershipGuard
{
    public static void EnsureOwnerOrAdmin(ICurrentUserService current, string ownerId)
    {
        if (current.IsAdmin) return;
        if (current.UserId is null || !string.Equals(current.UserId, ownerId, StringComparison.Ordinal))
            throw new ForbiddenException("You are not allowed to modify this listing.");
    }

    public static void EnsureAdmin(ICurrentUserService current)
    {
        if (!current.IsAdmin)
            throw new ForbiddenException("This action requires an administrator.");
    }

    public static string RequireUserId(ICurrentUserService current)
        => current.UserId ?? throw new UnauthorizedException("You must be logged in.");
}
