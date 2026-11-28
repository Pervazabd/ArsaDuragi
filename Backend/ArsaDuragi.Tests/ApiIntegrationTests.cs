using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Collections.Concurrent;
using ArsaDuragi.Application.Interfaces.Services;
using ArsaDuragi.API.Common;
using ArsaDuragi.Application.Common;
using ArsaDuragi.Application.DTOs.Auth;
using ArsaDuragi.Application.DTOs.Location;
using ArsaDuragi.Application.DTOs.PropertyAd;
using ArsaDuragi.Application.DTOs.PropertyImage;
using ArsaDuragi.Domain.Enums;
using ArsaDuragi.Persistence.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ArsaDuragi.Tests;

/// <summary>Boots the real API in memory (real controllers, JWT, middleware, validation) on an in-memory database.</summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    public const string AdminEmail = "admin@arsaduragi.test";
    public const string AdminPassword = "Admin#Test123";

    private readonly string _dbName = "api-tests-" + Guid.NewGuid();

    static ApiFactory()
    {
        // Environment variables are read very early by the host, so Program.cs sees them. No secrets are stored in the repo.
        Environment.SetEnvironmentVariable("Jwt__Secret", "integration-test-secret-key-0123456789-abcdef");
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", "Server=unused;Database=unused");
        Environment.SetEnvironmentVariable("Seed__AdminEmail", AdminEmail);
        Environment.SetEnvironmentVariable("Seed__AdminPassword", AdminPassword);
        Environment.SetEnvironmentVariable("Seed__DemoListings", "false");
        Environment.SetEnvironmentVariable("RateLimiting__AuthPermitLimit", "100000");
    }

    public async Task<string> RequestVerificationAsync(string email)
    {
        using var client = CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/register/request-verification",
            new EmailRequestDto { Email = email });

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

        return Services.GetRequiredService<TestEmailService>().TakeToken(email);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null) services.Remove(descriptor);
            services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(_dbName));

            services.AddSingleton<TestEmailService>();
            services.AddSingleton<IEmailService>(
                sp => sp.GetRequiredService<TestEmailService>());

            // The general flow tests below let customers post (moderation flow). The real default is admin-only,
            // which is covered by AdminOnlyApiFactory / AdminOnlyIntegrationTests.
            services.Configure<ModerationOptions>(o => o.AdminOnlyPosting = false);
        });
    }
}

internal sealed class TestEmailService : IEmailService
{
    private readonly ConcurrentDictionary<string, string> _messages = new();

    public Task SendAsync(
        string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        _messages[to] = htmlBody;
        return Task.CompletedTask;
    }

    public string TakeToken(string email)
    {
        Assert.True(_messages.TryRemove(email, out var body),
            $"No test email was sent to {email}.");

        var html = body!;
        var start = html.LastIndexOf("<p>", StringComparison.Ordinal) + 3;
        return WebUtility.HtmlDecode(html[start..^4]);
    }
}

public class ApiIntegrationTests : IClassFixture<ApiFactory>
{
    private static readonly byte[] Jpeg = { 0xFF, 0xD8, 0xFF, 0xE0, 0, 0x10, (byte)'J', (byte)'F', (byte)'I', (byte)'F', 0, 1, 2, 3, 4, 5 };
    private readonly ApiFactory _factory;

    public ApiIntegrationTests(ApiFactory factory) => _factory = factory;

    // ------------------------------------------------------------------ helpers

    private HttpClient Anonymous() => _factory.CreateClient();

    private HttpClient WithToken(string token)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static async Task<T> Data<T>(HttpResponseMessage response)
    {
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<T>>();
        Assert.NotNull(body);
        return body!.Data!;
    }

    private async Task<AuthResponseDto> RegisterAsync(
    string? email = null, string? token = null)
    {
        email ??= $"user{Guid.NewGuid():N}@gmail.com";
        token ??= await _factory.RequestVerificationAsync(email);

        using var client = Anonymous();
        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            FullName = "Integration User",
            Email = email,
            PhoneNumber = "+994501234567",
            Password = "Passw0rd1",
            ConfirmPassword = "Passw0rd1",
            VerificationToken = token
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await Data<AuthResponseDto>(response);
    }

    private async Task<AuthResponseDto> LoginAdminAsync()
    {
        var response = await Anonymous().PostAsJsonAsync("/api/auth/login", new LoginDto { Email = ApiFactory.AdminEmail, Password = ApiFactory.AdminPassword });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await Data<AuthResponseDto>(response);
    }

    private async Task<(int cityId, int districtId)> FirstLocationAsync()
    {
        var cities = await Data<List<CityDto>>(await Anonymous().GetAsync("/api/cities"));
        var city = cities.First();
        var districts = await Data<List<DistrictDto>>(await Anonymous().GetAsync($"/api/districts?cityId={city.Id}"));
        return (city.Id, districts.First().Id);
    }

    private static PropertyAdCreateDto NewProperty(int cityId, int districtId, string title = "Integration test arsa") => new()
    {
        Title = title,
        Description = "An integration test listing with a long enough description.",
        Address = "Merdakan, Bakü",
        Price = 123_000,
        Area = 750,
        PropertyType = PropertyType.Land,
        Zoning = PropertyZoning.Residential,
        Terrain = TerrainType.Flat,
        RoadAccess = RoadAccessType.AsphaltFrontage,
        Utilities = new List<UtilityType> { UtilityType.Electricity, UtilityType.Water },
        CityId = cityId,
        DistrictId = districtId
    };

    private async Task<PropertyAdGetDto> CreatePropertyAsync(string token, string title)
    {
        var (cityId, districtId) = await FirstLocationAsync();
        var response = await WithToken(token).PostAsJsonAsync("/api/properties", NewProperty(cityId, districtId, title));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await Data<PropertyAdGetDto>(response);
    }

    // ------------------------------------------------------------------ tests

    [Fact]
    public async Task Register_login_and_invalid_login()
    {
        var email = $"login{Guid.NewGuid():N}@gmail.com";
        var token = await _factory.RequestVerificationAsync(email);
        await RegisterAsync(email, token);

        var ok = await Anonymous().PostAsJsonAsync("/api/auth/login", new LoginDto { Email = email, Password = "Passw0rd1" });
        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        var auth = await Data<AuthResponseDto>(ok);
        Assert.Contains("User", auth.Roles);
        Assert.False(string.IsNullOrEmpty(auth.AccessToken));
        Assert.False(string.IsNullOrEmpty(auth.RefreshToken));

        var bad = await Anonymous().PostAsJsonAsync("/api/auth/login", new LoginDto { Email = email, Password = "WrongPassw0rd" });
        Assert.Equal(HttpStatusCode.Unauthorized, bad.StatusCode);

        var duplicate = await Anonymous().PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            FullName = "Dup",
            Email = email,
            Password = "Passw0rd1",
            ConfirmPassword = "Passw0rd1",
            VerificationToken = token,
        });
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
    }

    [Fact]
    public async Task Registration_rejects_non_gmail_and_tokens_issued_for_other_addresses()
    {
        var email = $"user{Guid.NewGuid():N}@gmail.com";
        var token = await _factory.RequestVerificationAsync(email);
        using var client = Anonymous();

        var dto = new RegisterDto
        {
            FullName = "Test User",
            Email = email,
            Password = "Passw0rd1",
            ConfirmPassword = "Passw0rd1"
        };

        // Token olmadan qeydiyyat.
        var noToken = await client.PostAsJsonAsync("/api/auth/register", dto);
        Assert.Equal(HttpStatusCode.BadRequest, noToken.StatusCode);

        // Gmail xaricindəki ünvanla token sorğusu və qeydiyyat.
        dto.Email = "example@outlook.com";
        dto.VerificationToken = token;

        var nonGmailRequest = await client.PostAsJsonAsync(
            "/api/auth/register/request-verification",
            new EmailRequestDto { Email = dto.Email });
        Assert.Equal(HttpStatusCode.BadRequest, nonGmailRequest.StatusCode);

        var nonGmailRegister = await client.PostAsJsonAsync("/api/auth/register", dto);
        Assert.Equal(HttpStatusCode.BadRequest, nonGmailRegister.StatusCode);

        // Bir Gmail ünvanının tokeni ilə başqa Gmail ünvanını qeydiyyatdan keçirmək.
        dto.Email = $"other{Guid.NewGuid():N}@gmail.com";

        var wrongAddress = await client.PostAsJsonAsync("/api/auth/register", dto);
        Assert.Equal(HttpStatusCode.BadRequest, wrongAddress.StatusCode);
    }

    [Fact]
    public async Task Registration_code_stops_working_after_five_wrong_attempts()
    {
        var email = $"user{Guid.NewGuid():N}@gmail.com";
        var code = await _factory.RequestVerificationAsync(email);
        Assert.Matches(@"\A[0-9]{6}\z", code);

        using var client = Anonymous();
        var dto = new RegisterDto
        {
            FullName = "Integration User",
            Email = email,
            Password = "Passw0rd1",
            ConfirmPassword = "Passw0rd1",
            VerificationToken = code == "000000" ? "000001" : "000000"
        };

        for (var i = 0; i < 5; i++)
        {
            var wrong = await client.PostAsJsonAsync("/api/auth/register", dto);
            Assert.Equal(HttpStatusCode.BadRequest, wrong.StatusCode);
        }

        dto.VerificationToken = code;
        var exhausted = await client.PostAsJsonAsync("/api/auth/register", dto);
        Assert.Equal(HttpStatusCode.BadRequest, exhausted.StatusCode);
    }

    [Fact]
    public async Task Password_reset_changes_password_revokes_refresh_token_and_rejects_reuse()
    {
        var auth = await RegisterAsync();
        using var client = Anonymous();

        var unknownAccount = await client.PostAsJsonAsync(
            "/api/auth/forgot-password",
            new EmailRequestDto { Email = "unknown@example.com" });
        Assert.Equal(HttpStatusCode.Accepted, unknownAccount.StatusCode);

        var knownAccount = await client.PostAsJsonAsync(
            "/api/auth/forgot-password",
            new EmailRequestDto { Email = auth.Email });
        Assert.Equal(HttpStatusCode.Accepted, knownAccount.StatusCode);

        var token = _factory.Services
            .GetRequiredService<TestEmailService>()
            .TakeToken(auth.Email);

        Assert.Matches(@"\A[0-9]{6}\z", token);

        var reset = new ResetPasswordDto
        {
            Email = auth.Email,
            Token = token == "000000" ? "000001" : "000000",
            NewPassword = "NewPassw0rd",
            ConfirmPassword = "NewPassw0rd"
        };

        var invalidReset = await client.PostAsJsonAsync("/api/auth/reset-password", reset);
        Assert.Equal(HttpStatusCode.BadRequest, invalidReset.StatusCode);

        reset.Token = token;
        var validReset = await client.PostAsJsonAsync("/api/auth/reset-password", reset);
        Assert.Equal(HttpStatusCode.NoContent, validReset.StatusCode);

        var reusedToken = await client.PostAsJsonAsync("/api/auth/reset-password", reset);
        Assert.Equal(HttpStatusCode.BadRequest, reusedToken.StatusCode);

        var oldRefresh = await client.PostAsJsonAsync(
            "/api/auth/refresh",
            new RefreshTokenRequestDto { RefreshToken = auth.RefreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, oldRefresh.StatusCode);

        var oldPassword = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginDto { Email = auth.Email, Password = "Passw0rd1" });
        Assert.Equal(HttpStatusCode.Unauthorized, oldPassword.StatusCode);

        var newPassword = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginDto { Email = auth.Email, Password = reset.NewPassword });
        Assert.Equal(HttpStatusCode.OK, newPassword.StatusCode);
    }

    [Fact]
    public async Task Password_reset_code_is_rejected_after_five_wrong_attempts()
    {
        var auth = await RegisterAsync();
        using var client = Anonymous();

        var response = await client.PostAsJsonAsync(
            "/api/auth/forgot-password",
            new EmailRequestDto { Email = auth.Email });

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

        var code = _factory.Services
            .GetRequiredService<TestEmailService>()
            .TakeToken(auth.Email);

        var reset = new ResetPasswordDto
        {
            Email = auth.Email,
            Token = code == "000000" ? "000001" : "000000",
            NewPassword = "NewPassw0rd",
            ConfirmPassword = "NewPassw0rd"
        };

        for (var attempt = 0; attempt < 5; attempt++)
        {
            var wrong = await client.PostAsJsonAsync(
                "/api/auth/reset-password", reset);

            Assert.Equal(HttpStatusCode.BadRequest, wrong.StatusCode);
        }

        reset.Token = code;

        var exhausted = await client.PostAsJsonAsync(
            "/api/auth/reset-password", reset);

        Assert.Equal(HttpStatusCode.BadRequest, exhausted.StatusCode);
    }


    [Fact]
    public async Task Invalid_registration_returns_400_with_the_standard_error_envelope()
    {
        var response = await Anonymous().PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            FullName = "X",
            Email = "nope",
            Password = "weak",
            ConfirmPassword = "different"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.False(body!.Success);
        Assert.NotEmpty(body.Errors);
    }

    [Fact]
    public async Task Anonymous_gets_401_and_normal_user_gets_403_on_admin_endpoints()
    {
        var anonymous = await Anonymous().PostAsJsonAsync("/api/properties", NewProperty(1, 1));
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);

        var user = await RegisterAsync();
        var forbidden = await WithToken(user.AccessToken).GetAsync("/api/admin/properties");
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);

        var noAdminForCities = await WithToken(user.AccessToken).PostAsJsonAsync("/api/cities", new CityUpsertDto { Name = "Hacker City" });
        Assert.Equal(HttpStatusCode.Forbidden, noAdminForCities.StatusCode);
    }

    [Fact]
    public async Task Admin_listing_is_published_immediately_and_visible_to_public()
    {
        var user = await RegisterAsync();
        var admin = await LoginAdminAsync();

        var created = await CreatePropertyAsync(admin.AccessToken, "Moderation flow arsa");

        // Admin-created listings are published immediately.
        Assert.Equal("Active", created.Status);

        // The listing is immediately visible to anonymous visitors.
        var publicList = await Data<PagedResult<PropertyAdListDto>>(
            await Anonymous().GetAsync("/api/properties?search=Moderation%20flow"));

        Assert.Equal(1, publicList.TotalCount);
        Assert.Contains(publicList.Items, p => p.Id == created.Id);

        Assert.Equal(
            HttpStatusCode.OK,
            (await Anonymous().GetAsync($"/api/properties/{created.Slug}")).StatusCode);

        // Admin sees the listing in their own active listings.
        var mine = await Data<PagedResult<PropertyAdListDto>>(
            await WithToken(admin.AccessToken).GetAsync("/api/users/me/properties?status=Active"));

        Assert.Contains(mine.Items, p => p.Id == created.Id);

        // A normal user cannot access the approval endpoint.
        Assert.Equal(
            HttpStatusCode.Forbidden,
            (await WithToken(user.AccessToken)
                .PostAsync($"/api/admin/properties/{created.Id}/approve", null)).StatusCode);
    }

    [Fact]
    public async Task Another_user_cannot_edit_or_delete_my_listing()
    {
        var owner = await RegisterAsync();
        var stranger = await RegisterAsync();
        var admin = await LoginAdminAsync();
        var created = await CreatePropertyAsync(admin.AccessToken, "Ownership arsa");
        var (cityId, districtId) = await FirstLocationAsync();

        var update = new PropertyAdUpdateDto
        {
            Title = "Stolen title",
            Description = "A long enough description for the update.",
            Address = "Somewhere",
            Price = 1,
            Area = 1,
            PropertyType = PropertyType.Land,
            Zoning = PropertyZoning.Residential,
            Terrain = TerrainType.Flat,
            RoadAccess = RoadAccessType.RoadFrontage,
            Utilities = new List<UtilityType>(),
            CityId = cityId,
            DistrictId = districtId
        };

        Assert.Equal(HttpStatusCode.Forbidden, (await WithToken(stranger.AccessToken).PutAsJsonAsync($"/api/properties/{created.Id}", update)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await WithToken(stranger.AccessToken).DeleteAsync($"/api/properties/{created.Id}")).StatusCode);

        Assert.Equal(HttpStatusCode.Forbidden, (await WithToken(owner.AccessToken).PutAsJsonAsync($"/api/properties/{created.Id}", update)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await WithToken(owner.AccessToken).DeleteAsync($"/api/properties/{created.Id}")).StatusCode);

        var adminClient = WithToken(admin.AccessToken);
        Assert.Equal(HttpStatusCode.OK, (await adminClient.PutAsJsonAsync($"/api/properties/{created.Id}", update)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await adminClient.DeleteAsync($"/api/properties/{created.Id}")).StatusCode);
    }

    [Fact]
    public async Task Favorites_flow_add_duplicate_list_remove()
    {
        var owner = await RegisterAsync();
        var admin = await LoginAdminAsync();
        var fan = await RegisterAsync();
        var created = await CreatePropertyAsync(admin.AccessToken, "Favorite flow arsa");
        await WithToken(admin.AccessToken).PostAsync($"/api/admin/properties/{created.Id}/approve", null);
        var client = WithToken(fan.AccessToken);

        Assert.Equal(HttpStatusCode.Created, (await client.PostAsync($"/api/favorites/{created.Id}", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsync($"/api/favorites/{created.Id}", null)).StatusCode);

        var list = await Data<PagedResult<ArsaDuragi.Application.DTOs.Favorite.FavoriteDto>>(await client.GetAsync("/api/favorites"));
        Assert.Single(list.Items);
        Assert.Equal(created.Id, list.Items[0].Property.Id);

        Assert.Equal(HttpStatusCode.Unauthorized, (await Anonymous().GetAsync("/api/favorites")).StatusCode);

        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/favorites/{created.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync($"/api/favorites/{created.Id}")).StatusCode);
    }

    [Fact]
    public async Task Refresh_token_is_rotated_and_reuse_is_detected()
    {
        var first = await RegisterAsync();

        var refreshed = await Anonymous().PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequestDto { RefreshToken = first.RefreshToken });
        Assert.Equal(HttpStatusCode.OK, refreshed.StatusCode);
        var second = await Data<AuthResponseDto>(refreshed);
        Assert.NotEqual(first.RefreshToken, second.RefreshToken);

        // the new access token works
        Assert.Equal(HttpStatusCode.OK, (await WithToken(second.AccessToken).GetAsync("/api/users/me")).StatusCode);

        // using the OLD refresh token again = theft signal: refused, and every token of the user is revoked
        var reuse = await Anonymous().PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequestDto { RefreshToken = first.RefreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, reuse.StatusCode);

        var newestNowDead = await Anonymous().PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequestDto { RefreshToken = second.RefreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, newestNowDead.StatusCode);
    }

    [Fact]
    public async Task Logout_revokes_the_refresh_token()
    {
        var auth = await RegisterAsync();

        var logout = await Anonymous().PostAsJsonAsync("/api/auth/logout", new RefreshTokenRequestDto { RefreshToken = auth.RefreshToken });
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);

        var refresh = await Anonymous().PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequestDto { RefreshToken = auth.RefreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);
    }

    [Fact]
    public async Task Image_upload_accepts_real_images_rejects_fakes_and_cleans_up()
    {
        var admin = await LoginAdminAsync();
        var created = await CreatePropertyAsync(admin.AccessToken, "Image upload arsa");
        var client = WithToken(admin.AccessToken);

        // a text file pretending to be a JPEG
        var fake = new MultipartFormDataContent();
        fake.Add(new ByteArrayContent("<script>alert(1)</script>"u8.ToArray()), "files", "evil.jpg");
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync($"/api/properties/{created.Id}/images", fake)).StatusCode);

        // a real (minimal) JPEG
        var good = new MultipartFormDataContent();
        var file = new ByteArrayContent(Jpeg);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        good.Add(file, "files", "photo.jpg");
        var upload = await client.PostAsync($"/api/properties/{created.Id}/images", good);
        Assert.Equal(HttpStatusCode.Created, upload.StatusCode);

        var images = await Data<List<PropertyImageDto>>(upload);
        Assert.Single(images);
        Assert.True(images[0].IsMain);
        Assert.Contains($"/uploads/properties/{created.Id}/", images[0].Url);

        // the stored file is served as a static file with the anti-sniffing header
        var served = await Anonymous().GetAsync(new Uri(images[0].Url).AbsolutePath);
        Assert.Equal(HttpStatusCode.OK, served.StatusCode);
        Assert.Equal("image/jpeg", served.Content.Headers.ContentType?.MediaType);
        Assert.Contains("nosniff", served.Headers.GetValues("X-Content-Type-Options"));

        // another user cannot touch the image
        var stranger = await RegisterAsync();
        Assert.Equal(HttpStatusCode.Forbidden,
            (await WithToken(stranger.AccessToken).DeleteAsync($"/api/properties/{created.Id}/images/{images[0].Id}")).StatusCode);

        // admin can delete the image; the file disappears from disk and from the URL
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/properties/{created.Id}/images/{images[0].Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await Anonymous().GetAsync(new Uri(images[0].Url).AbsolutePath)).StatusCode);

        await client.DeleteAsync($"/api/properties/{created.Id}");
    }

    [Fact]
    public async Task Admin_can_feature_listings_and_featured_endpoint_returns_them()
    {
        var owner = await RegisterAsync();
        var admin = await LoginAdminAsync();
        var created = await CreatePropertyAsync(admin.AccessToken, "Featured arsa");
        var adminClient = WithToken(admin.AccessToken);

        await adminClient.PostAsync($"/api/admin/properties/{created.Id}/approve", null);
        var feature = await adminClient.PutAsJsonAsync($"/api/admin/properties/{created.Id}/featured", new SetFeaturedDto { IsFeatured = true });
        Assert.Equal(HttpStatusCode.OK, feature.StatusCode);

        var featured = await Data<List<PropertyAdListDto>>(await Anonymous().GetAsync("/api/properties/featured"));
        Assert.Contains(featured, p => p.Id == created.Id && p.Featured);

        // owner cannot feature his own listing
        Assert.Equal(HttpStatusCode.Forbidden,
            (await WithToken(owner.AccessToken).PutAsJsonAsync($"/api/admin/properties/{created.Id}/featured", new SetFeaturedDto { IsFeatured = true })).StatusCode);
    }

    [Fact]
    public async Task Invalid_query_parameters_return_400()
    {
        Assert.Equal(HttpStatusCode.BadRequest, (await Anonymous().GetAsync("/api/properties?pageSize=1000")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Anonymous().GetAsync("/api/properties?minPrice=10&maxPrice=5")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Anonymous().GetAsync("/api/properties?propertyType=Castle")).StatusCode);
    }
}


/// <summary>Same API, but with the production default: only an Admin may post listings.</summary>
public sealed class AdminOnlyApiFactory : ApiFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureTestServices(services => services.Configure<ModerationOptions>(o => o.AdminOnlyPosting = true));
    }
}

public class AdminOnlyIntegrationTests : IClassFixture<AdminOnlyApiFactory>
{
    private readonly AdminOnlyApiFactory _factory;

    public AdminOnlyIntegrationTests(AdminOnlyApiFactory factory) => _factory = factory;

    private HttpClient WithToken(string? token = null)
    {
        var client = _factory.CreateClient();
        if (token is not null) client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static async Task<T> Data<T>(HttpResponseMessage response)
        => (await response.Content.ReadFromJsonAsync<ApiResponse<T>>())!.Data!;

    [Fact]
    public async Task Customer_cannot_post_but_admin_can_and_customer_sees_the_result()
    {
        var email = $"customer{Guid.NewGuid():N}@gmail.com";
        var token = await _factory.RequestVerificationAsync(email);

        var customerResponse = await WithToken().PostAsJsonAsync(
            "/api/auth/register", new RegisterDto
            {
                FullName = "Customer",
                Email = email,
                Password = "Passw0rd1",
                ConfirmPassword = "Passw0rd1",
                VerificationToken = token
            });
        Assert.Equal(HttpStatusCode.Created, customerResponse.StatusCode);
        var customer = await Data<AuthResponseDto>(customerResponse);

        var adminLogin = await WithToken().PostAsJsonAsync("/api/auth/login", new LoginDto { Email = ApiFactory.AdminEmail, Password = ApiFactory.AdminPassword });
        var admin = await Data<AuthResponseDto>(adminLogin);

        var cities = await Data<List<CityDto>>(await WithToken().GetAsync("/api/cities"));
        var districts = await Data<List<DistrictDto>>(await WithToken().GetAsync($"/api/districts?cityId={cities[0].Id}"));
        var dto = new PropertyAdCreateDto
        {
            Title = "Admin only arsa",
            Description = "A long enough description for the admin-only test.",
            Address = "Merdakan, Bakü",
            Price = 150_000,
            Area = 600,
            PropertyType = PropertyType.Land,
            Zoning = PropertyZoning.Residential,
            Terrain = TerrainType.Flat,
            RoadAccess = RoadAccessType.AsphaltFrontage,
            Utilities = new List<UtilityType> { UtilityType.Water },
            CityId = cities[0].Id,
            DistrictId = districts[0].Id
        };

        // customer: forbidden
        Assert.Equal(HttpStatusCode.Forbidden, (await WithToken(customer.AccessToken).PostAsJsonAsync("/api/properties", dto)).StatusCode);

        // admin: created and immediately visible (no moderation step for admin's own listings)
        var created = await WithToken(admin.AccessToken).PostAsJsonAsync("/api/properties", dto);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var ad = await Data<PropertyAdGetDto>(created);
        Assert.Equal("Active", ad.Status);

        var list = await Data<PagedResult<PropertyAdListDto>>(await WithToken().GetAsync("/api/properties?search=Admin%20only"));
        Assert.Equal(1, list.TotalCount);

        // customer still cannot edit/delete/upload, but can favorite
        Assert.Equal(HttpStatusCode.Forbidden, (await WithToken(customer.AccessToken).DeleteAsync($"/api/properties/{ad.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await WithToken(customer.AccessToken).PostAsync($"/api/favorites/{ad.Id}", null)).StatusCode);
    }

}
