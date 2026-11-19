using ArsaDuragi.API.Middlewares;
using ArsaDuragi.Persistence.Context;
using ArsaDuragi.Persistence.Seed;
using Microsoft.EntityFrameworkCore;

namespace ArsaDuragi.API.Extensions;

public static class ApplicationBuilderExtensions
{
    /// <summary>Applies migrations (Development/Testing by default) and runs the idempotent seeder.</summary>
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var db = services.GetRequiredService<AppDbContext>();

        var autoMigrate = app.Configuration.GetValue<bool?>("Database:AutoMigrate")
                          ?? (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"));

        if (autoMigrate)
        {
            if (db.Database.IsRelational())
            {
                if (!db.Database.GetMigrations().Any())
                    throw new InvalidOperationException(
                        "No EF Core migration exists yet. Create it once with: " +
                        "dotnet ef migrations add InitialCreate --project ArsaDuragi.Persistence --startup-project ArsaDuragi.API --output-dir Migrations");

                await db.Database.MigrateAsync();
            }
            else
            {
                await db.Database.EnsureCreatedAsync(); // in-memory provider used by the integration tests
            }
        }

        await services.GetRequiredService<DataSeeder>().SeedAsync();
    }

    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        // First, so it can catch exceptions thrown by everything below it.
        app.UseMiddleware<ExceptionMiddleware>();

        if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing"))
            app.UseHttpsRedirection();

        // Local image storage: wwwroot/uploads/** is served as static files.
        Directory.CreateDirectory(Path.Combine(app.Environment.ContentRootPath, "wwwroot", "uploads", "properties"));
        app.UseStaticFiles(new StaticFileOptions
        {
            // Tell browsers never to "guess" a different content type for an uploaded file.
            OnPrepareResponse = ctx => ctx.Context.Response.Headers["X-Content-Type-Options"] = "nosniff"
        });

        app.UseCors(ServiceCollectionExtensions.CorsPolicy);
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();

        if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Swagger:Enabled"))
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapControllers();
        return app;
    }
}
