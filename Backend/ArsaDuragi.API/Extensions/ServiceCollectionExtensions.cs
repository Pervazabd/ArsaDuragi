using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using ArsaDuragi.API.Common;
using ArsaDuragi.API.Filters;
using ArsaDuragi.API.Services;
using ArsaDuragi.Application.Common;
using ArsaDuragi.Application.Interfaces.Services;
using ArsaDuragi.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace ArsaDuragi.API.Extensions;

public static class ServiceCollectionExtensions
{
    public const string CorsPolicy = "Frontend";
    public const string AuthRateLimitPolicy = "auth";

    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.Configure<ModerationOptions>(configuration.GetSection(ModerationOptions.SectionName));

        AddControllers(services);
        AddJwtAuthentication(services, configuration);
        services.AddAuthorization();
        AddCors(services, configuration);
        AddRateLimiting(services, configuration);
        AddSwagger(services);

        return services;
    }

    private static void AddControllers(IServiceCollection services)
    {
        services.AddControllers(options => options.Filters.Add<ValidationFilter>())
            .AddJsonOptions(options =>
            {
                // enums travel as readable strings: "Residential" instead of 1
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                // model-binding problems (bad JSON, "propertyType=abc") use the same envelope as everything else
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(e => e.Value?.Errors.Count > 0)
                        .SelectMany(e => e.Value!.Errors.Select(x =>
                            string.IsNullOrWhiteSpace(x.ErrorMessage) ? $"{e.Key}: invalid value." : $"{e.Key}: {x.ErrorMessage}"))
                        .ToList();
                    return new BadRequestObjectResult(ApiResponse<object?>.Fail("Request is invalid.", errors));
                };
            });
    }

    private static void AddJwtAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        var jwt = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();
        jwt.EnsureValid();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false; // keep claim names exactly as written in the token ("sub", "role")
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = "name",
                    RoleClaimType = "role"
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse(); // we write the response ourselves
                        return ApiResponseWriter.WriteAsync(context.Response, StatusCodes.Status401Unauthorized,
                            "Authentication is required, or the token is invalid or expired.");
                    },
                    OnForbidden = context => ApiResponseWriter.WriteAsync(context.Response, StatusCodes.Status403Forbidden,
                        "You do not have permission to perform this action.")
                };
            });
    }

    private static void AddCors(IServiceCollection services, IConfiguration configuration)
    {
        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() is { Length: > 0 } configured
            ? configured
            : new[] { "http://localhost:5173" }; // default Vite dev server

        services.AddCors(options => options.AddPolicy(CorsPolicy, policy =>
            policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod()));
    }

    private static void AddRateLimiting(IServiceCollection services, IConfiguration configuration)
    {
        var permitLimit = configuration.GetValue<int?>("RateLimiting:AuthPermitLimit") ?? 10;

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            // login / register / refresh: N requests per minute per IP address (slows down password guessing)
            options.AddPolicy(AuthRateLimitPolicy, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = permitLimit,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));
        });
    }

    private static void AddSwagger(IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "ArsaDuragi API", Version = "v1" });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Paste the accessToken returned by /api/auth/login (without the word 'Bearer')."
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
                    Array.Empty<string>()
                }
            });
        });
    }
}
