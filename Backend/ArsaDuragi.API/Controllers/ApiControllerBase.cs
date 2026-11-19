using ArsaDuragi.API.Common;
using Microsoft.AspNetCore.Mvc;

namespace ArsaDuragi.API.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult Success<T>(T data, string message) => Ok(ApiResponse<T>.Ok(data, message));

    protected IActionResult CreatedResponse<T>(T data, string message) => StatusCode(StatusCodes.Status201Created, ApiResponse<T>.Ok(data, message));
}
