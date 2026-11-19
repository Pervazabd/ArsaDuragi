using ArsaDuragi.Application.Common;
using ArsaDuragi.Application.DTOs.Admin;
using ArsaDuragi.Application.DTOs.Auth;

namespace ArsaDuragi.Application.Interfaces.Services;

public interface IUserService
{
    Task<UserDto> GetMeAsync(CancellationToken ct = default);

    // Admin only
    Task<PagedResult<UserDto>> GetUsersAsync(UserListFilterDto filter, CancellationToken ct = default);
    Task LockUserAsync(string userId, CancellationToken ct = default);
    Task UnlockUserAsync(string userId, CancellationToken ct = default);
}
