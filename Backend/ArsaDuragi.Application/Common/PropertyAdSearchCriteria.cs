using ArsaDuragi.Application.DTOs.PropertyAd;
using ArsaDuragi.Domain.Enums;

namespace ArsaDuragi.Application.Common;

/// <summary>
/// What the repository needs to run a search. The public filter (from the query string) is wrapped together
/// with trusted, server-decided restrictions (statuses, owner) so a client can never override them.
/// </summary>
public sealed class PropertyAdSearchCriteria
{
    public PropertyAdFilterDto Filter { get; init; } = new();

    /// <summary>null = any status.</summary>
    public IReadOnlyCollection<PropertyStatus>? Statuses { get; init; }

    /// <summary>When set, only this user's listings are returned.</summary>
    public string? OwnerId { get; init; }
}
