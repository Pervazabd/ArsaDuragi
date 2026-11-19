using ArsaDuragi.Application.Common;
using ArsaDuragi.Application.DTOs.Admin;
using ArsaDuragi.Application.DTOs.Auth;
using ArsaDuragi.Application.Exceptions;
using ArsaDuragi.Application.Interfaces.Repositories;
using ArsaDuragi.Application.Interfaces.Services;
using AutoMapper;

namespace ArsaDuragi.Application.Services;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _current;
    private readonly IMapper _mapper;

    public UserService(IUserRepository users, IRefreshTokenRepository refreshTokens, IUnitOfWork uow,
        ICurrentUserService current, IMapper mapper)
    {
        _users = users;
        _refreshTokens = refreshTokens;
        _uow = uow;
        _current = current;
        _mapper = mapper;
    }

    public async Task<UserDto> GetMeAsync(CancellationToken ct = default)
    {
        var id = OwnershipGuard.RequireUserId(_current);
        var user = await _users.FindByIdAsync(id, ct) ?? throw new NotFoundException("User not found.");
        return await ToDtoAsync(user);
    }

    public async Task<PagedResult<UserDto>> GetUsersAsync(UserListFilterDto filter, CancellationToken ct = default)
    {
        OwnershipGuard.EnsureAdmin(_current);
        var page = await _users.GetPagedAsync(filter.Search, filter.Page, filter.PageSize, ct);

        var items = new List<UserDto>();
        foreach (var user in page.Items) items.Add(await ToDtoAsync(user));

        return new PagedResult<UserDto> { Items = items, Page = page.Page, PageSize = page.PageSize, TotalCount = page.TotalCount };
    }

    public async Task LockUserAsync(string userId, CancellationToken ct = default)
    {
        OwnershipGuard.EnsureAdmin(_current);
        if (string.Equals(userId, _current.UserId, StringComparison.Ordinal))
            throw new BadRequestException("You cannot lock your own account.");

        var user = await _users.FindByIdAsync(userId, ct) ?? throw new NotFoundException("User not found.");
        await _users.SetLockedAsync(user, true);

        // A locked user must not be able to keep refreshing tokens.
        await _refreshTokens.RevokeAllForUserAsync(user.Id, ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task UnlockUserAsync(string userId, CancellationToken ct = default)
    {
        OwnershipGuard.EnsureAdmin(_current);
        var user = await _users.FindByIdAsync(userId, ct) ?? throw new NotFoundException("User not found.");
        await _users.SetLockedAsync(user, false);
    }

    private async Task<UserDto> ToDtoAsync(Domain.Entities.User user)
    {
        var dto = _mapper.Map<UserDto>(user);
        dto.Roles = (await _users.GetRolesAsync(user)).ToList();
        return dto;
    }
}
