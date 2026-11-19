using ArsaDuragi.Application.Common;
using ArsaDuragi.Domain.Entities;
using ArsaDuragi.Domain.Enums;
using ArsaDuragi.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ArsaDuragi.Persistence.Seed;

/// <summary>
/// Idempotent: running it many times never creates duplicates.
/// Seeds roles, an optional admin (credentials come ONLY from configuration/user-secrets), the city/districts
/// and the nine demo listings that used to live in the frontend's properties.js.
/// </summary>
public sealed class DataSeeder
{
    public const string DemoOwnerEmail = "demo-owner@arsaduragi.invalid";
    private const string CityName = "Bakü"; // spelled exactly as the frontend addresses ("Merdakan, Bakü")

    // Spelled exactly as in the frontend data.
    private static readonly string[] DistrictNames =
    {
        "Merdakan", "Bilgah", "Sabuncu", "Hövsan", "Maştağa", "Nardaran", "Zabrat", "Pirşağı", "Ramana"
    };

    private const string ReferenceLandImage =
        "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQ1PjYayqh6d0JVx4GTAPtP7sKohx8EzgaOtrmf5bNhc1jlbKzARLpxhH2J&s=10";
    private const string Img1 = "https://images.unsplash.com/photo-1500382017468-9049fed747ef?auto=format&fit=crop&w=1200&q=85";
    private const string Img2 = "https://images.unsplash.com/photo-1472396961693-142e6e269027?auto=format&fit=crop&w=1200&q=85";
    private const string Img3 = "https://images.unsplash.com/photo-1500530855697-b586d89ba3ee?auto=format&fit=crop&w=1200&q=85";

    private sealed record Demo(string Slug, string Title, string District, string Address, PropertyZoning Zoning,
        decimal Area, decimal Price, bool Featured, DateTime ListedAt, TerrainType Terrain, RoadAccessType Road,
        int? RoadDistance, UtilityType Utilities, string Description, string Image);

    private static DateTime D(int y, int m, int d) => new(y, m, d, 0, 0, 0, DateTimeKind.Utc);

    private static readonly Demo[] DemoListings =
    {
        new("merdakan-konut-arsasi", "Merdakan'da konut imarlı arsa", "Merdakan", "Merdakan, Bakü", PropertyZoning.Residential,
            600, 180000, true, D(2026,10,24), TerrainType.Flat, RoadAccessType.AsphaltFrontage, null,
            UtilityType.Electricity | UtilityType.Water | UtilityType.NaturalGas,
            "Merdakan'da yerleşim alanına yakın, müstakil konut yapımına uygun arsa. Düz yapısı ve yola cepheli konumuyla kullanıma hazır durumdadır.",
            ReferenceLandImage),
        new("bilgah-yatirim-arsasi", "Bilgah'ta yatırıma uygun arsa", "Bilgah", "Bilgah, Bakü", PropertyZoning.Investment,
            850, 212500, true, D(2026,10,21), TerrainType.SlightlySloped, RoadAccessType.MainRoadNearby, 300,
            UtilityType.Electricity | UtilityType.Water,
            "Bilgah bölgesinde gelişen yerleşim alanlarının yakınında bulunan geniş yatırım arsası. Arazi sınırları belirgindir ve bölge yoluna kolay erişim sunar.",
            Img1),
        new("sabuncu-tarim-arazisi", "Sabuncu'da verimli tarım arazisi", "Sabuncu", "Sabuncu, Bakü", PropertyZoning.Agricultural,
            2400, 96000, true, D(2026,10,19), TerrainType.FlatFertile, RoadAccessType.DirtRoadFrontage, null,
            UtilityType.IrrigationWater,
            "Sabuncu'da tarımsal kullanım için uygun, düz ve geniş arazi. Sulama suyu bağlantısı ve ulaşım yolu mevcuttur.",
            Img2),
        new("hovsan-konut-arsasi", "Hövsan'da yola cepheli arsa", "Hövsan", "Hövsan, Bakü", PropertyZoning.Residential,
            500, 115000, false, D(2026,10,16), TerrainType.Flat, RoadAccessType.AsphaltFrontage, null,
            UtilityType.Electricity | UtilityType.Water,
            "Hövsan yerleşim alanında konut yapımına uygun, düz ve yola cepheli arsa. Elektrik ve su hatları parselin yakınından geçmektedir.",
            Img3),
        new("mashtaga-yatirim-arazisi", "Maştağa'da geniş yatırım arazisi", "Maştağa", "Maştağa, Bakü", PropertyZoning.Investment,
            1200, 168000, false, D(2026,10,12), TerrainType.Flat, RoadAccessType.MainRoadNearby, 500,
            UtilityType.Electricity | UtilityType.Water | UtilityType.NaturalGas,
            "Maştağa'da gelişime açık bölgede bulunan geniş yatırım arazisi. Düz topografyası farklı arazi projeleri için elverişlidir.",
            Img1),
        new("nardaran-tarim-arazisi", "Nardaran'da geniş tarım arazisi", "Nardaran", "Nardaran, Bakü", PropertyZoning.Agricultural,
            3200, 128000, false, D(2026,10,9), TerrainType.Flat, RoadAccessType.VillageRoadFrontage, null,
            UtilityType.IrrigationWater | UtilityType.Electricity,
            "Nardaran çevresinde tarım ve açık alan kullanımı için uygun geniş parsel. Araziye köy yolu üzerinden ulaşılabilir.",
            Img2),
        new("zabrat-konut-arsasi", "Zabrat'ta konut yapımına uygun arsa", "Zabrat", "Zabrat, Bakü", PropertyZoning.Residential,
            400, 88000, false, D(2026,10,6), TerrainType.Flat, RoadAccessType.RoadFrontage, null,
            UtilityType.Electricity | UtilityType.Water | UtilityType.NaturalGas,
            "Zabrat'ta yerleşim alanı içinde bulunan, konut yapımı için uygun arsa. Altyapı hatlarına ve ana ulaşım güzergahına yakındır.",
            Img3),
        new("pirshagi-yatirim-arsasi", "Pirşağı'da sahile yakın arsa", "Pirşağı", "Pirşağı, Bakü", PropertyZoning.Investment,
            700, 154000, false, D(2026,10,2), TerrainType.Flat, RoadAccessType.AsphaltRoadNearby, 200,
            UtilityType.Electricity | UtilityType.Water,
            "Pirşağı'da sahil bölgesine ve yerleşim noktalarına yakın konumda arsa. Yatırım veya gelecekteki arazi projeniz için değerlendirilebilir.",
            Img1),
        new("ramana-tarim-arazisi", "Ramana'da tarıma uygun arazi", "Ramana", "Ramana, Bakü", PropertyZoning.Agricultural,
            1800, 72000, false, D(2026,9,28), TerrainType.SlightlySloped, RoadAccessType.DirtRoadFrontage, null,
            UtilityType.IrrigationWater,
            "Ramana'da tarımsal kullanıma elverişli, geniş ve ulaşılabilir arazi. Sulama imkanı bulunan parsel farklı tarım faaliyetleri için uygundur.",
            Img2),
    };

    private readonly AppDbContext _db;
    private readonly UserManager<User> _users;
    private readonly RoleManager<IdentityRole> _roles;
    private readonly IConfiguration _config;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(AppDbContext db, UserManager<User> users, RoleManager<IdentityRole> roles,
        IConfiguration config, ILogger<DataSeeder> logger)
    {
        _db = db;
        _users = users;
        _roles = roles;
        _config = config;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await SeedRolesAsync();
        await SeedAdminAsync();
        await SeedLocationsAsync(ct);

        var demoEnabled = string.Equals(_config["Seed:DemoListings"], "true", StringComparison.OrdinalIgnoreCase);
        if (demoEnabled) await SeedDemoListingsAsync(ct);
    }

    private async Task SeedRolesAsync()
    {
        foreach (var role in new[] { Roles.Admin, Roles.User })
        {
            if (!await _roles.RoleExistsAsync(role))
                await _roles.CreateAsync(new IdentityRole(role));
        }
    }

    private async Task SeedAdminAsync()
    {
        var email = _config["Seed:AdminEmail"];
        var password = _config["Seed:AdminPassword"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning("No admin account seeded: set Seed:AdminEmail and Seed:AdminPassword (user-secrets / environment variables).");
            return;
        }

        var admin = await _users.FindByEmailAsync(email);
        if (admin is null)
        {
            admin = new User { UserName = email, Email = email, EmailConfirmed = true, FullName = "Administrator" };
            var result = await _users.CreateAsync(admin, password);
            if (!result.Succeeded)
            {
                _logger.LogError("Admin seeding failed: {Errors}", string.Join("; ", result.Errors.Select(e => e.Description)));
                return;
            }
        }

        if (!await _users.IsInRoleAsync(admin, Roles.Admin)) await _users.AddToRoleAsync(admin, Roles.Admin);
        if (!await _users.IsInRoleAsync(admin, Roles.User)) await _users.AddToRoleAsync(admin, Roles.User);
    }

    private async Task SeedLocationsAsync(CancellationToken ct)
    {
        var city = await _db.Cities.FirstOrDefaultAsync(c => c.Name == CityName, ct);
        if (city is null)
        {
            city = new City { Name = CityName };
            _db.Cities.Add(city);
            await _db.SaveChangesAsync(ct);
        }

        var existing = await _db.Districts.Where(d => d.CityId == city.Id).Select(d => d.Name).ToListAsync(ct);
        foreach (var name in DistrictNames.Where(n => !existing.Contains(n)))
            _db.Districts.Add(new District { Name = name, CityId = city.Id });

        await _db.SaveChangesAsync(ct);
    }

    private async Task SeedDemoListingsAsync(CancellationToken ct)
    {
        if (await _db.PropertyAds.IgnoreQueryFilters().AnyAsync(p => p.IsDemo, ct)) return;

        var owner = await _users.FindByEmailAsync(DemoOwnerEmail);
        if (owner is null)
        {
            owner = new User { UserName = DemoOwnerEmail, Email = DemoOwnerEmail, FullName = "Demo listings" };
            // Random throw-away password nobody ever sees: nobody can log in as the demo owner.
            var result = await _users.CreateAsync(owner, Guid.NewGuid().ToString("N") + "aA1");
            if (!result.Succeeded)
            {
                _logger.LogError("Demo owner seeding failed: {Errors}", string.Join("; ", result.Errors.Select(e => e.Description)));
                return;
            }
            await _users.AddToRoleAsync(owner, Roles.User);
        }

        var city = await _db.Cities.FirstAsync(c => c.Name == CityName, ct);
        var districts = await _db.Districts.Where(d => d.CityId == city.Id).ToDictionaryAsync(d => d.Name, ct);

        foreach (var demo in DemoListings)
        {
            var ad = new PropertyAd
            {
                Slug = demo.Slug,
                Title = demo.Title,
                Description = demo.Description,
                Address = demo.Address,
                Price = demo.Price,
                Area = demo.Area,
                PropertyType = PropertyType.Land,
                Zoning = demo.Zoning,
                Terrain = demo.Terrain,
                RoadAccess = demo.Road,
                RoadDistanceMeters = demo.RoadDistance,
                Utilities = demo.Utilities,
                IsFeatured = demo.Featured,
                Status = PropertyStatus.Active,
                ListedAt = demo.ListedAt,
                IsDemo = true,
                UserId = owner.Id,
                CityId = city.Id,
                DistrictId = districts[demo.District].Id
            };
            ad.Images.Add(new PropertyImage { ImageUrl = demo.Image, ObjectKey = null, IsMain = true });
            _db.PropertyAds.Add(ad);
        }

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Seeded {Count} demo listings.", DemoListings.Length);
    }
}
