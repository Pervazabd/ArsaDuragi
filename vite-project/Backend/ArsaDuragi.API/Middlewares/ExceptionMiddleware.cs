using ArsaDuragi.API.Common;
using ArsaDuragi.Application.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ArsaDuragi.API.Middlewares;

/// <summary>
/// The single place that turns exceptions into HTTP responses. Services just throw
/// NotFoundException / ForbiddenException / ...; stack traces never reach the client.
/// </summary>
public sealed class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            context.Response.StatusCode = 499; // client closed the request; nothing to send
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        switch (ex)
        {
            case NotFoundException e:
                await ApiResponseWriter.WriteAsync(context.Response, StatusCodes.Status404NotFound, e.Message);
                break;
            case BadRequestException e:
                await ApiResponseWriter.WriteAsync(context.Response, StatusCodes.Status400BadRequest, e.Message, e.Errors);
                break;
            case ValidationException e:
                await ApiResponseWriter.WriteAsync(context.Response, StatusCodes.Status400BadRequest, "Validation failed.",
                    e.Errors.Select(x => $"{x.PropertyName}: {x.ErrorMessage}"));
                break;
            case UnauthorizedException e:
                await ApiResponseWriter.WriteAsync(context.Response, StatusCodes.Status401Unauthorized, e.Message);
                break;
            case ForbiddenException e:
                await ApiResponseWriter.WriteAsync(context.Response, StatusCodes.Status403Forbidden, e.Message);
                break;
            case ConflictException e:
                await ApiResponseWriter.WriteAsync(context.Response, StatusCodes.Status409Conflict, e.Message);
                break;
            case DbUpdateException e:
                // e.g. two simultaneous "add favorite" clicks hitting the unique index
                _logger.LogWarning(e, "Database update conflict");
                await ApiResponseWriter.WriteAsync(context.Response, StatusCodes.Status409Conflict,
                    "The operation conflicts with existing data.");
                break;
            default:
                _logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
                await ApiResponseWriter.WriteAsync(context.Response, StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred.");
                break;
        }
    }
}
