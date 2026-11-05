using ArsaDuragi.Application.Interfaces.Services;
using ArsaDuragi.Application.Mappings;
using ArsaDuragi.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ArsaDuragi.Application;

public static class DependencyInjection
{
    /// <summary>Registers everything the Application layer owns. Called once from Program.cs.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile).Assembly);
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<PropertyDtoMapper>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPropertyAdService, PropertyAdService>();
        services.AddScoped<IPropertyImageService, PropertyImageService>();
        services.AddScoped<IFavoriteService, FavoriteService>();
        services.AddScoped<ILocationService, LocationService>();

        return services;
    }
}
