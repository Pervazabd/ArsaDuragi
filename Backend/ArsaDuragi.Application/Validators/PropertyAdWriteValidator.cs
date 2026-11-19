using ArsaDuragi.Application.Common;
using ArsaDuragi.Application.DTOs.PropertyAd;
using FluentValidation;

namespace ArsaDuragi.Application.Validators;

/// <summary>Rules shared by create and update. FluentValidation skips abstract classes when scanning.</summary>
public abstract class PropertyAdWriteValidator<T> : AbstractValidator<T> where T : PropertyAdWriteDto
{
    protected PropertyAdWriteValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MinimumLength(5).MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MinimumLength(20).MaximumLength(4000);
        RuleFor(x => x.Address).NotEmpty().MinimumLength(3).MaximumLength(300);

        RuleFor(x => x.Price).GreaterThan(0).LessThanOrEqualTo(1_000_000_000_000m);
        RuleFor(x => x.Area).GreaterThan(0).LessThanOrEqualTo(100_000_000m);

        RuleFor(x => x.PropertyType).IsInEnum();
        RuleFor(x => x.Zoning).IsInEnum();
        RuleFor(x => x.Terrain).IsInEnum();
        RuleFor(x => x.RoadAccess).IsInEnum();

        RuleFor(x => x.RoadDistanceMeters)
            .NotNull().WithMessage("Road distance is required for this road access type.")
            .InclusiveBetween(1, 100_000)
            .When(x => PropertyLabels.RequiresDistance(x.RoadAccess));

        RuleFor(x => x.Utilities)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Must(u => u.Distinct().Count() == u.Count).WithMessage("Utilities contain duplicates.");
        RuleForEach(x => x.Utilities).IsInEnum();

        RuleFor(x => x.CityId).GreaterThan(0);
        RuleFor(x => x.DistrictId).GreaterThan(0);
    }
}

public class PropertyAdCreateValidator : PropertyAdWriteValidator<PropertyAdCreateDto> { }

public class PropertyAdUpdateValidator : PropertyAdWriteValidator<PropertyAdUpdateDto> { }
