using ArsaDuragi.Application.Common;
using ArsaDuragi.Application.DTOs.Auth;
using FluentValidation;

namespace ArsaDuragi.Application.Validators;

internal static class AuthPasswordRules
{
    public static IRuleBuilderOptions<T, string> MeetsPasswordPolicy<T>(this IRuleBuilder<T, string> rule)
        => rule
            .NotEmpty()
            .MinimumLength(8).MaximumLength(100)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.");
}

public class ResetPasswordValidator : AbstractValidator<ResetPasswordDto>
{
    public ResetPasswordValidator()
    {
        RuleFor(x => x.Email)
    .NotEmpty()
    .EmailAddress()
    .MaximumLength(256);

        RuleFor(x => x.Token).NotEmpty().MaximumLength(8192);
        RuleFor(x => x.NewPassword).MeetsPasswordPolicy();
        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.NewPassword).WithMessage("Passwords do not match.");
    }
}

public sealed class EmailRequestValidator : AbstractValidator<EmailRequestDto>
{
    public EmailRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);
    }
}
