using ArsaDuragi.API.Common;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ArsaDuragi.API.Filters;

/// <summary>
/// Runs the FluentValidation validator (if one exists) for every action argument before the action executes.
/// This is what makes controllers "thin": they never contain validation code.
/// </summary>
public sealed class ValidationFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _services;

    public ValidationFilter(IServiceProvider services) => _services = services;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var errors = new List<string>();

        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null) continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (_services.GetService(validatorType) is not IValidator validator) continue;

            var result = await validator.ValidateAsync(new ValidationContext<object>(argument), context.HttpContext.RequestAborted);
            errors.AddRange(result.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
        }

        if (errors.Count > 0)
        {
            context.Result = new BadRequestObjectResult(ApiResponse<object?>.Fail("Validation failed.", errors));
            return;
        }

        await next();
    }
}
