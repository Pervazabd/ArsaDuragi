using ArsaDuragi.API.Extensions;
using ArsaDuragi.Application;
using ArsaDuragi.Infrastructure;
using ArsaDuragi.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Each layer registers its own services (dependency injection). Program.cs only wires the layers together.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

await app.InitializeDatabaseAsync();
app.UseApiPipeline();

app.Run();

// Makes the entry point visible to the integration tests (WebApplicationFactory<Program>).
public partial class Program { }
