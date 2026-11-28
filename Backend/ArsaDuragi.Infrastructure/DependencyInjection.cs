using ArsaDuragi.Application.Interfaces.Services;
using ArsaDuragi.Application.Interfaces.Storage;
using ArsaDuragi.Infrastructure.Email;
using ArsaDuragi.Infrastructure.FileStorage;
using ArsaDuragi.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArsaDuragi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<FileStorageOptions>(configuration.GetSection(FileStorageOptions.SectionName));
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));

        services.AddHttpContextAccessor();

        services.AddSingleton<IJwtService, JwtService>();
        services.AddSingleton<IFileStorageService, LocalFileStorageService>(); // local disk only, by design
        services.AddSingleton<IEmailService, EmailService>();

        services.AddDataProtection();
        services.AddMemoryCache();
        services.AddSingleton<EmailVerificationCodes>();
        services.AddSingleton<IPasswordResetCode, PasswordResetCode>();
        services.AddSingleton<IRegistrationEmailToken, RegistrationEmailToken>();

        return services;
    }
}
