using ArsaDuragi.Application.DTOs.Admin;
using ArsaDuragi.Application.DTOs.Location;
using ArsaDuragi.Application.DTOs.PropertyAd;
using FluentValidation;

namespace ArsaDuragi.Application.Validators;

public class PropertyAdFilterValidator : AbstractValidator<PropertyAdFilterDto>
{
    private static readonly string[] SortFields = { "listedat", "price", "area", "featured" };

    public PropertyAdFilterValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
        RuleFor(x => x.Search).MaximumLength(100);

        RuleFor(x => x.MinPrice).GreaterThanOrEqualTo(0).When(x => x.MinPrice.HasValue);
        RuleFor(x => x.MaxPrice).GreaterThanOrEqualTo(0).When(x => x.MaxPrice.HasValue);
        RuleFor(x => x.MinArea).GreaterThanOrEqualTo(0).When(x => x.MinArea.HasValue);
        RuleFor(x => x.MaxArea).GreaterThanOrEqualTo(0).When(x => x.MaxArea.HasValue);

        RuleFor(x => x).Must(x => !x.MinPrice.HasValue || !x.MaxPrice.HasValue || x.MinPrice <= x.MaxPrice)
            .WithMessage("minPrice cannot be greater than maxPrice.");
        RuleFor(x => x).Must(x => !x.MinArea.HasValue || !x.MaxArea.HasValue || x.MinArea <= x.MaxArea)
            .WithMessage("minArea cannot be greater than maxArea.");

        RuleFor(x => x.SortBy)
            .Must(s => SortFields.Contains(s!.ToLowerInvariant()))
            .WithMessage("sortBy must be one of: listedAt, price, area, featured.")
            .When(x => !string.IsNullOrWhiteSpace(x.SortBy));

        RuleFor(x => x.SortOrder)
            .Must(s => s!.Equals("asc", StringComparison.OrdinalIgnoreCase) || s.Equals("desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("sortOrder must be 'asc' or 'desc'.")
            .When(x => !string.IsNullOrWhiteSpace(x.SortOrder));
    }
}

public class RejectPropertyAdValidator : AbstractValidator<RejectPropertyAdDto>
{
    public RejectPropertyAdValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
    }
}

public class CityUpsertValidator : AbstractValidator<CityUpsertDto>
{
    public CityUpsertValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class DistrictUpsertValidator : AbstractValidator<DistrictUpsertDto>
{
    public DistrictUpsertValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.CityId).GreaterThan(0);
    }
}

public class UserListFilterValidator : AbstractValidator<UserListFilterDto>
{
    public UserListFilterValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Search).MaximumLength(100);
    }
}
