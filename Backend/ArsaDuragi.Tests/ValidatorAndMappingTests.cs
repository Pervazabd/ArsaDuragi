using ArsaDuragi.Application.Common;
using ArsaDuragi.Application.DTOs.Auth;
using ArsaDuragi.Application.DTOs.PropertyAd;
using ArsaDuragi.Application.Mappings;
using ArsaDuragi.Application.Validators;
using ArsaDuragi.Domain.Enums;
using AutoMapper;
using Xunit;

namespace ArsaDuragi.Tests;

public class ValidatorAndMappingTests
{
    [Fact]
    public void AutoMapper_configuration_is_valid()
        => new MapperConfiguration(c => c.AddProfile<MappingProfile>()).AssertConfigurationIsValid();

    private static RegisterDto Register() => new()
    {
        FullName = "Test User",
        Email = "test@gmail.com",
        Password = "Passw0rd1",
        ConfirmPassword = "Passw0rd1",
        VerificationToken = "012345"
    };

    [Fact]
    public void Valid_registration_passes()
        => Assert.True(new RegisterValidator().Validate(Register()).IsValid);

    [Theory]
    [InlineData("short1A")]        // too short
    [InlineData("alllowercase1")]  // no uppercase
    [InlineData("ALLUPPERCASE1")]  // no lowercase
    [InlineData("NoDigitsHere")]   // no digit
    public void Weak_passwords_are_rejected(string password)
    {
        var dto = Register();
        dto.Password = dto.ConfirmPassword = password;
        Assert.False(new RegisterValidator().Validate(dto).IsValid);
    }

    [Fact]
    public void Registration_rejects_bad_email_mismatched_passwords_and_bad_phone()
    {
        var dto = Register();
        dto.Email = "not-an-email";
        dto.ConfirmPassword = "Different1";
        dto.PhoneNumber = "abc";

        var result = new RegisterValidator().Validate(dto);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterDto.Email));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterDto.ConfirmPassword));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterDto.PhoneNumber));
    }

    private static PropertyAdCreateDto Property() => new()
    {
        Title = "Merdakan'da arsa",
        Description = "A long enough description for this listing.",
        Address = "Merdakan, Bakü",
        Price = 100_000,
        Area = 500,
        PropertyType = PropertyType.Land,
        Zoning = PropertyZoning.Residential,
        Terrain = TerrainType.Flat,
        RoadAccess = RoadAccessType.AsphaltFrontage,
        Utilities = new List<UtilityType> { UtilityType.Water },
        CityId = 1,
        DistrictId = 1
    };

    [Fact]
    public void Valid_property_passes()
        => Assert.True(new PropertyAdCreateValidator().Validate(Property()).IsValid);

    [Fact]
    public void Property_rejects_non_positive_price_and_area()
    {
        var dto = Property();
        dto.Price = 0;
        dto.Area = -5;

        var result = new PropertyAdCreateValidator().Validate(dto);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(PropertyAdCreateDto.Price));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(PropertyAdCreateDto.Area));
    }

    [Fact]
    public void Nearby_road_access_requires_a_distance()
    {
        var dto = Property();
        dto.RoadAccess = RoadAccessType.MainRoadNearby;
        Assert.False(new PropertyAdCreateValidator().Validate(dto).IsValid);

        dto.RoadDistanceMeters = 300;
        Assert.True(new PropertyAdCreateValidator().Validate(dto).IsValid);
    }

    [Fact]
    public void Duplicate_utilities_and_undefined_enums_are_rejected()
    {
        var dto = Property();
        dto.Utilities = new List<UtilityType> { UtilityType.Water, UtilityType.Water };
        dto.Zoning = (PropertyZoning)99;

        var result = new PropertyAdCreateValidator().Validate(dto);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(PropertyAdCreateDto.Utilities));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(PropertyAdCreateDto.Zoning));
    }

    [Fact]
    public void Null_utilities_do_not_crash_the_validator()
    {
        var dto = Property();
        dto.Utilities = null!;
        Assert.False(new PropertyAdCreateValidator().Validate(dto).IsValid);
    }

    [Theory]
    [InlineData(0, 10, false)]   // page must be >= 1
    [InlineData(1, 0, false)]    // pageSize must be >= 1
    [InlineData(1, 500, false)]  // pageSize max 50
    [InlineData(1, 50, true)]
    public void Filter_paging_limits(int page, int pageSize, bool valid)
        => Assert.Equal(valid, new PropertyAdFilterValidator().Validate(new PropertyAdFilterDto { Page = page, PageSize = pageSize }).IsValid);

    [Fact]
    public void Filter_rejects_min_greater_than_max_and_unknown_sort()
    {
        Assert.False(new PropertyAdFilterValidator().Validate(new PropertyAdFilterDto { MinPrice = 10, MaxPrice = 5 }).IsValid);
        Assert.False(new PropertyAdFilterValidator().Validate(new PropertyAdFilterDto { MinArea = 10, MaxArea = 5 }).IsValid);
        Assert.False(new PropertyAdFilterValidator().Validate(new PropertyAdFilterDto { SortBy = "password" }).IsValid);
        Assert.True(new PropertyAdFilterValidator().Validate(new PropertyAdFilterDto { SortBy = "Price", SortOrder = "ASC" }).IsValid);
    }

    // ----- the Turkish texts the existing frontend displays must be reproduced exactly -----

    [Theory]
    [InlineData(RoadAccessType.AsphaltFrontage, null, "Asfalt yola cepheli")]
    [InlineData(RoadAccessType.RoadFrontage, null, "Yola cepheli")]
    [InlineData(RoadAccessType.DirtRoadFrontage, null, "Toprak yola cepheli")]
    [InlineData(RoadAccessType.VillageRoadFrontage, null, "Köy yoluna cepheli")]
    [InlineData(RoadAccessType.MainRoadNearby, 300, "Ana yola 300 m")]
    [InlineData(RoadAccessType.AsphaltRoadNearby, 200, "Asfalt yola 200 m")]
    public void Road_access_labels_match_the_frontend_texts(RoadAccessType type, int? distance, string expected)
        => Assert.Equal(expected, PropertyLabels.RoadAccess(type, distance));

    [Fact]
    public void Zoning_terrain_and_utility_labels_match_the_frontend_texts()
    {
        Assert.Equal("Konut imarlı", PropertyLabels.Zoning(PropertyZoning.Residential));
        Assert.Equal("Tarım arazisi", PropertyLabels.Zoning(PropertyZoning.Agricultural));
        Assert.Equal("Yatırımlık arsa", PropertyLabels.Zoning(PropertyZoning.Investment));
        Assert.Equal("Düz ve verimli toprak", PropertyLabels.Terrain(TerrainType.FlatFertile));
        Assert.Equal("Arsa", PropertyLabels.Type(PropertyType.Land));
        Assert.Equal(new[] { "Elektrik", "Su", "Doğalgaz" },
            PropertyLabels.UtilityLabels(UtilityType.Electricity | UtilityType.Water | UtilityType.NaturalGas));
        Assert.Equal(new[] { "Elektrik", "Sulama suyu" },
            PropertyLabels.UtilityLabels(UtilityType.IrrigationWater | UtilityType.Electricity));
    }

    [Theory]
    [InlineData("Merdakan'da konut imarlı arsa", "merdakan-da-konut-imarli-arsa")]
    [InlineData("Pirşağı'da sahile yakın arsa", "pirsagi-da-sahile-yakin-arsa")]
    [InlineData("Ağdam Əhməd", "agdam-ehmed")]
    [InlineData("!!!", "listing")]
    public void Slugs_are_url_safe_ascii(string title, string expected)
        => Assert.Equal(expected, SlugHelper.Generate(title));
}
