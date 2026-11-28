# ArsaDuragi Backend

Torpaq və əmlak alqı-satqı platforması **ArsaDuragi** üçün ASP.NET Core Web API (.NET 8), Clean Architecture.

## 1. Texnologiyalar

ASP.NET Core 8 · EF Core 8 + SQL Server · ASP.NET Core Identity · JWT + Refresh Token · AutoMapper 13 · FluentValidation 11 · Swagger · xUnit + Moq.

Şəkillər lokal diskdə saxlanılır (`wwwroot/uploads/properties/`).

Paketləri yoxlamaq üçün `dotnet list package --vulnerable` və `dotnet list package --outdated` əmrlərindən istifadə edin.

## 2. Arxitektura

```text
Domain  <-  Application  <-  Infrastructure
                 ^
                 +-------------  Persistence      API  ->  Application, Infrastructure, Persistence
```

| Layer | Nə üçün var | Nə var |
|---|---|---|
| **Domain** | Biznesin özəyi | Entity-lər, enum-lar |
| **Application** | Biznes qaydaları | DTO-lar, servis və repository interfeysləri, servislər, validatorlar, AutoMapper, exception-lar |
| **Persistence** | Verilənlər bazası | `AppDbContext`, Fluent API konfiqurasiyaları, repository implementasiyaları, seed |
| **Infrastructure** | Xarici sistemlərlə əlaqə | JWT, lokal fayl saxlama, e-poçt, Gmail qeydiyyat kodu |
| **API** | HTTP girişi | Controller-lər, middleware, DI, JWT, CORS, Swagger |

Məsələn, `PUT /api/properties/5` sorğusu `Controller` → `ValidationFilter` → `PropertyAdService` → `IPropertyAdRepository` → `PropertyAdRepository` → `IUnitOfWork.SaveChangesAsync` yolu ilə emal olunur. Repository implementasiyaları `DependencyInjection.cs` fayllarında qeydiyyata alınır.

**Əsas qərarlar**

- DTO → entity çevrilməsi servisdə əl ilə edilir. Entity → DTO çevrilməsi üçün AutoMapper istifadə olunur.
- İstifadəçi ID-si JWT-dəki `sub` claim-indən oxunur.
- Elanlar `IsDeleted` və global query filter vasitəsilə soft delete edilir. Şəkillər diskdən və cədvəldən silinir.
- Standart `Moderation:AdminOnlyPosting=true` sazlamasında elanı yalnız admin yerləşdirir. Müştərilər elanlara baxa və favoritə əlavə edə bilirlər. Müştəri tərəfindən elan yaratmaq, redaktə etmək, silmək və şəkil əməliyyatları `403` qaytarır.
- `Moderation:AdminOnlyPosting=false` olduqda müştərinin yeni elanı `Pending` statusu alır. İctimai siyahıda yalnız `Active` elanlar görünür. Təsdiq tələbi `Moderation:RequireApproval=false` ilə söndürülə bilər.
- Müştəri aktiv və ya rədd edilmiş elanı redaktə edəndə elan yenidən `Pending` olur. Admin redaktəsində status dəyişmir.
- Hər elanda bir əsas şəkil olmasını servis təmin edir.
- Enum-lar JSON-da `"Residential"` kimi mətn şəklində göndərilir.

## 3. Qovluq strukturu

```text
Backend/
├── ArsaDuragi.sln, Directory.Build.props
├── ArsaDuragi.Domain/          Entities, Enums
├── ArsaDuragi.Application/     DTOs, Interfaces, Services, Validators, Mappings, Exceptions, Common
├── ArsaDuragi.Infrastructure/  Services, FileStorage, Email
├── ArsaDuragi.Persistence/     Context, Configurations, Repositories, Seed, Migrations
├── ArsaDuragi.API/             Controllers, Middlewares, Filters, Extensions, wwwroot/uploads/properties
├── ArsaDuragi.Tests/           xUnit unit və API integration testləri
└── docs/FRONTEND_CONTRACT.md   Frontend və backend arasında API müqaviləsi
```

## 4. Quraşdırma və işə salma

.NET 8 SDK və SQL Server tələb olunur. Windows-da LocalDB istifadə edə bilərsiniz. Əmrləri `Backend/` qovluğunda işlədin.

```bash
dotnet user-secrets set "Jwt:Secret" "<ən-azı-32-simvolluq-təsadüfi-açar>" --project ArsaDuragi.API
dotnet user-secrets set "Seed:AdminEmail" "admin@arsaduragi.local" --project ArsaDuragi.API
dotnet user-secrets set "Seed:AdminPassword" "<admin-şifrəsi>" --project ArsaDuragi.API

# LocalDB əvəzinə başqa SQL Server istifadə edilirsə
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=ArsaDuragiDb;User Id=sa;Password=...;TrustServerCertificate=True" --project ArsaDuragi.API

dotnet build
dotnet run --project ArsaDuragi.API --launch-profile http
```

- Swagger ünvanı `http://localhost:5080/swagger`-dir.
- `dotnet ef` `Program.cs` faylını yükləyir. `Jwt:Secret` əvvəlcədən təyin edilməlidir.
- Mövcud migration-ları əl ilə tətbiq etmək üçün `dotnet ef database update --project ArsaDuragi.Persistence --startup-project ArsaDuragi.API` işlədin.
- Production mühitində `Database:AutoMigrate` standart olaraq söndürülüb. Migration-ları CI/CD-də və ya əl ilə tətbiq edin. Məxfi dəyərləri `Jwt__Secret`, `ConnectionStrings__DefaultConnection` və `Seed__AdminPassword` kimi mühit dəyişənləri ilə verin.
- Seed əməliyyatı `Admin` və `User` rollarını, Bakı şəhərini və 9 rayonu yaradır. Admin hesabı `Seed:AdminEmail` və `Seed:AdminPassword` verildikdə yaradılır.
- `Seed:DemoListings=true` olduqda 9 demo elan yaradılır. Onların sahibi `demo-owner@arsaduragi.invalid` hesabıdır və həmin hesabla giriş mümkün deyil.

### SMTP sazlaması

Gmail qeydiyyat kodlarının və şifrə sıfırlama məktublarının göndərilməsi üçün `ArsaDuragi.API` layihəsində **Manage User Secrets** vasitəsilə və ya serverin mühit dəyişənlərində SMTP-ni sazlayın.

| User Secrets açarı | Gmail SMTP nümunəsi |
|---|---|
| `Email:Enabled` | `true` |
| `Email:Host` | `smtp.gmail.com` |
| `Email:Port` | `587` |
| `Email:EnableSsl` | `true` |
| `Email:Username` | Göndərici Gmail ünvanı |
| `Email:Password` | Göndərici hesabın Google App Password dəyəri |
| `Email:FromAddress` | Göndərici Gmail ünvanı |
| `Email:FromName` | `ArsaDuragi` |

Mühit dəyişənlərində `Email:Host` əvəzinə `Email__Host`, `Email:Password` əvəzinə `Email__Password` formatından istifadə edin. App Password-u User Secrets və ya serverin məxfi sazlamasında saxlayın. Sazlamadan sonra API-ni yenidən başladın.

## 5. Test

```bash
dotnet test ArsaDuragi.sln
```

Unit testlər axtarış, filtr, sıralama, səhifələmə, sahiblik, moderasiya, favorit, şəkil, autentifikasiya, validatorlar və AutoMapper konfiqurasiyasını yoxlayır. API inteqrasiya testləri `WebApplicationFactory` vasitəsilə JWT, status kodları, refresh token rotasiyası, Gmail qeydiyyatı və şifrə sıfırlama axınlarını yoxlayır.

6 rəqəmli qeydiyyat kodu əlavə olunan istinad versiyasında 106 backend testi keçib. Testlər e-poçt üçün yaddaşda işləyən servis istifadə edir. Gmail-ə real çatdırılma SMTP sazlandıqdan sonra əl ilə yoxlanılır.

InMemory provider istifadə edən testlər SQL Server-in unique index, `LIKE` kollasiyası və cascade davranışını yoxlamır. Bu qaydaları real SQL Server-də sınayın.

## 6. Autentifikasiya

### Gmail ilə qeydiyyat

1. `POST /api/auth/register/request-verification` endpoint-inə `@gmail.com` ünvanını göndərin.
2. E-poçta gələn 6 rəqəmli kodu `POST /api/auth/register` sorğusunun `verificationToken` sahəsinə yazın. Kodu `"012345"` kimi mətn formatında göndərin ki, əvvəlindəki sıfır saxlanılsın.
3. Uğurlu qeydiyyat `201` və `accessToken` ilə `refreshToken` qaytarır.

Kod həmin Gmail ünvanı üçün 30 dəqiqə etibarlıdır. Beş səhv cəhddən sonra yeni kod tələb olunur. Eyni ünvana yeni kod ən tez bir dəqiqə sonra göndərilə bilər.

Qeydiyyat kodları işləyən API instansiyasının yaddaşında saxlanılır. API yenidən başladıqda yeni kod tələb olunur. Bir neçə API instansiyası istifadə edilirsə, kodlar üçün ortaq saxlama tətbiq edilməlidir.

### Şifrə sıfırlama

1. `POST /api/auth/forgot-password` endpoint-inə hesabın e-poçt ünvanını göndərin. Sorğu qəbul edildikdə API `202` qaytarır.
2. Məktubdakı 6 rəqəmli kodu `POST /api/auth/reset-password` sorğusunda `email`, `token`, `newPassword` və `confirmPassword` sahələri ilə göndərin. Kodu `token` sahəsinə `"065432"` kimi mətn şəklində yazın ki, əvvəlindəki sıfır saxlanılsın.
3. Uğurlu sıfırlama `204` qaytarır. Köhnə refresh tokenlər ləğv edilir və sıfırlama kodu təkrar istifadə oluna bilmir. Daha əvvəl verilmiş access tokenlər bitmə vaxtına qədər qüvvədə qalır.

Kod 30 dəqiqə etibarlıdır. Beş səhv cəhddən sonra yeni kod tələb olunur. Eyni hesab üçün yeni kod ən tez bir dəqiqə sonra istənə bilər.

Kod API instansiyasının yaddaşında saxlanılır. API yenidən başladıqda yeni kod istəyin. Bir neçə API instansiyası istifadə olunursa, sıfırlama kodları üçün ortaq saxlama tətbiq edin və serverdəki Identity tokenləri üçün Data Protection açarlarını instansiyalar arasında paylaşın.

### Giriş və sessiya

- `POST /api/auth/login` `accessToken` (15 dəqiqə) və `refreshToken` (7 gün) qaytarır.
- Giriş tələb edən sorğulara `Authorization: Bearer <accessToken>` başlığını əlavə edin.
- Access token bitdikdə `POST /api/auth/refresh` endpoint-inə `{ "refreshToken": "..." }` göndərin. Köhnə refresh token ləğv olunur və yeni token cütü qaytarılır.
- Ləğv edilmiş refresh token yenidən göndərilərsə, həmin hesabın refresh tokenləri ləğv edilir.
- `POST /api/auth/logout` göndərilən refresh tokeni ləğv edir.

Refresh token DB-də **SHA-256 hash** şəklində saxlanılır. Beş uğursuz girişdən sonra hesab 15 dəqiqə bloklanır. `/api/auth/*` üçün standart limit IP başına dəqiqədə 10 sorğudur. Frontend sessiya məlumatını `localStorage`-da saxlayır.

## 7. Endpoint-lər

Standart JSON cavab zərfi belədir:

```json
{ "success": true, "message": "...", "data": {}, "errors": [] }
```

Aşağıdakı `202` və `204` auth cavablarının gövdəsi boşdur. Token sorğusuna verilən `202` məktubun Gmail-ə çatdığını təsdiqləmir.

| Metod | URL | Giriş | Qeyd |
|---|---|---|---|
| POST | `/api/auth/register/request-verification` | açıq | `202` qəbul edildi, `400` etibarsız ünvan və ya başqa domen |
| POST | `/api/auth/register` | açıq | `201` hesab yaradıldı, `400` etibarsız kod, `409` e-poçt təkrarı |
| POST | `/api/auth/forgot-password` | açıq | `202` sorğu qəbul edildi |
| POST | `/api/auth/reset-password` | açıq | `204` şifrə yeniləndi, `400` etibarsız sorğu |
| POST | `/api/auth/login` | açıq | `401` səhv məlumat, `403` hesab bloklanıb |
| POST | `/api/auth/refresh` · `/api/auth/logout` | açıq | Token rotasiyası · `204` |
| GET | `/api/users/me` | giriş | Profil və rollar |
| GET | `/api/users/me/properties?status=` | giriş | Öz elanlarım, `Pending` və `Rejected` daxil |
| GET | `/api/properties` | açıq | Yalnız `Active` elanlar |
| GET | `/api/properties/featured?take=6` | açıq | Seçilmiş elanlar |
| GET | `/api/properties/{idOrSlug}` | açıq | ID və ya slug. `Pending` yalnız sahibə və adminə görünür |
| POST | `/api/properties` | giriş | `201`, `Pending` və ya admin üçün dərhal `Active` |
| PUT | `/api/properties/{id}` | sahib/admin | Elanı redaktə edir |
| DELETE | `/api/properties/{id}` | sahib/admin | `204`, soft delete |
| PUT | `/api/properties/{id}/status` | sahib/admin | `Sold`, `Archived`, `Pending` |
| POST | `/api/properties/{id}/images` | sahib/admin | Multipart, `files`, maksimum 10 şəkil, hər fayl 5 MB, JPEG/PNG/WebP |
| PUT | `/api/properties/{id}/images/{imageId}` | sahib/admin | Şəkli əvəz edir, sahə adı `file` |
| DELETE | `/api/properties/{id}/images/{imageId}` | sahib/admin | Şəkli silir |
| PUT | `/api/properties/{id}/images/{imageId}/main` | sahib/admin | Əsas şəkli təyin edir |
| GET | `/api/favorites` · `/api/favorites/ids` | giriş | Favoritlər |
| POST/DELETE | `/api/favorites/{propertyId}` | giriş | `201`, təkrarda `409`, uğurlu silmədə `204` |
| GET | `/api/cities` · `/api/districts?cityId=` | açıq | Şəhərlər və rayonlar |
| POST/PUT/DELETE | `/api/cities[/{id}]` · `/api/districts[/{id}]` | Admin | Asılı məlumat varsa silmə `409` qaytarır |
| GET | `/api/admin/properties?status=Pending` | Admin | Moderasiya növbəsi |
| POST | `/api/admin/properties/{id}/approve` · `/reject` | Admin | Rədd sorğusunda `{ "reason": "..." }` |
| PUT | `/api/admin/properties/{id}/featured` | Admin | `{ "isFeatured": true }` |
| GET | `/api/admin/users` | Admin | İstifadəçi siyahısı |
| PUT | `/api/admin/users/{id}/lock` · `/unlock` | Admin | Bloklama zamanı refresh tokenlər də ləğv edilir |

`GET /api/properties` aşağıdakı sorğu parametrlərini dəstəkləyir:

- `search`, `districtId`, `cityId`
- `propertyType` (`Land`, `Field`, `VineyardOrchard`)
- `zoning` (`Residential`, `Agricultural`, `Investment`)
- `minPrice`, `maxPrice`, `minArea`, `maxArea`, `featured`
- `sortBy` (`listedAt`, `price`, `area`, `featured`), `sortOrder` (`asc`, `desc`)
- `page`, `pageSize` (`1`–`50`, standart `12`)

`search` başlıq, ünvan, təsvir, rayon və şəhər üzrə SQL Server `LIKE` axtarışı aparır. Filtrləri birlikdə istifadə etmək olar.

### Nümunələr

Əvvəl Gmail kodunu istəyin:

```http
POST /api/auth/register/request-verification
Content-Type: application/json

{ "email": "elvin@gmail.com" }
```

Sonra məktubdakı faktiki kodla qeydiyyatı tamamlayın:

```http
POST /api/auth/register
Content-Type: application/json

{
  "fullName": "Elvin Məmmədov",
  "email": "elvin@gmail.com",
  "phoneNumber": "+994501234567",
  "password": "Passw0rd1",
  "confirmPassword": "Passw0rd1",
  "verificationToken": "012345"
}
```

Uğurlu qeydiyyat cavabı:

```json
{
  "success": true,
  "message": "Registration successful",
  "data": {
    "userId": "…",
    "fullName": "Elvin Məmmədov",
    "email": "elvin@gmail.com",
    "roles": ["User"],
    "accessToken": "eyJ…",
    "accessTokenExpiresAt": "2026-11-02T10:15:00Z",
    "refreshToken": "kX3…",
    "refreshTokenExpiresAt": "2026-11-09T10:00:00Z"
  },
  "errors": []
}
```

Şifrə sıfırlama məktubunu istəyin:

```http
POST /api/auth/forgot-password
Content-Type: application/json

{ "email": "elvin@gmail.com" }
```

Məktubdakı sıfırlama tokeni ilə şifrəni yeniləyin:

```http
POST /api/auth/reset-password
Content-Type: application/json

{
  "email": "elvin@gmail.com",
  "token": "məktubdakı-sıfırlama-tokeni",
  "newPassword": "NewPassw0rd",
  "confirmPassword": "NewPassw0rd"
}
```

Elan yaratmaq üçün:

```http
POST /api/properties
Authorization: Bearer …
Content-Type: application/json

{
  "title": "Merdakan'da konut imarlı arsa",
  "description": "Müstakil konut yapımına uygun, düz arsa …",
  "address": "Merdakan, Bakü",
  "price": 180000,
  "area": 600,
  "propertyType": "Land",
  "zoning": "Residential",
  "roadAccess": "MainRoadNearby",
  "roadDistanceMeters": 300,
  "utilities": ["Electricity", "Water"],
  "cityId": 1,
  "districtId": 1
}
```

Elanları filtrləmək üçün:

```http
GET /api/properties?districtId=1&minPrice=50000&maxPrice=200000&sortBy=area&sortOrder=desc&page=1&pageSize=12
```

```json
{
  "success": true,
  "message": "Properties retrieved successfully",
  "data": {
    "items": [
      {
        "id": 1,
        "slug": "merdakan-konut-arsasi",
        "title": "…",
        "district": "Merdakan",
        "type": "Arsa",
        "zoning": "Konut imarlı",
        "area": 600,
        "price": 180000,
        "featured": true,
        "image": "https://…"
      }
    ],
    "page": 1,
    "pageSize": 12,
    "totalCount": 1,
    "totalPages": 1
  },
  "errors": []
}
```

Şəkil yükləmə:

```bash
curl -X POST -H "Authorization: Bearer $TOKEN" -F "files=@foto1.jpg" -F "files=@foto2.png" http://localhost:5080/api/properties/1/images
```

## 8. Şəkil yükləmə təhlükəsizliyi

`LocalFileStorageService` icazəli uzantıları (`.jpg`, `.jpeg`, `.png`, `.webp`), fayl imzasını və 5 MB ölçü həddini yoxlayır. Fayl adı serverdə `{guid}.{uzantı}` formatında yaradılır. Fayl yolu `wwwroot/uploads` qovluğu daxilində qalır. Statik fayllar `X-Content-Type-Options: nosniff` başlığı ilə verilir. Yazma uğursuz olduqda yarımçıq fayl silinir.

DB-də nisbi URL (`/uploads/properties/{propertyId}/{guid}.jpg`) və `ObjectKey` saxlanılır. API cavabında mütləq URL `FileStorage:PublicBaseUrl` və ya cari HTTP sorğusu əsasında qurulur.

## 9. Frontend inteqrasiyası

DTO sahələri və status kodları üçün [frontend API müqaviləsinə](docs/FRONTEND_CONTRACT.md) baxın.

Frontend-də qeydiyyat kodu `verificationToken` sahəsində mətn kimi göndərilir. Login səhifəsində “Şifrəni unutdum” forması sıfırlama tokenini ayrıca `token` sahəsində göndərir. Lokal inkişafda Vite proxy-dən istifadə edin və ya `VITE_API_BASE_URL` dəyərini `http://localhost:5080/api` olaraq təyin edin. CORS-da frontend ünvanı `Cors:AllowedOrigins` ilə sazlanır.

## 10. İlk işə salmadan sonra əl ilə yoxlama

1. `dotnet build` və `dotnet test` əmrlərini işlədin.
2. SQL Server-ə migration-ları tətbiq edin və Swagger-i açın.
3. `Seed:DemoListings=true` olduqda `GET /api/properties` cavabında 9 demo elanın göründüyünü yoxlayın.
4. SMTP-ni aktivləşdirin. Yeni Gmail ünvanına qeydiyyat kodu istəyin və məktubdakı 6 rəqəmlə qeydiyyatı tamamlayın.
5. Həmin hesab üçün şifrə sıfırlama məktubu istəyin. Şifrəni yeniləyin və yeni şifrə ilə daxil olun.
6. Müştəri moderasiyası aktivdirsə, yeni `Pending` elanın admin təsdiqindən sonra ictimai siyahıda göründüyünü yoxlayın.
7. Eyni elanı iki dəfə favoritə əlavə etdikdə ikinci sorğunun `409` qaytardığını yoxlayın.
8. Şəkil yükləyib onun URL-nin brauzerdə açıldığını yoxlayın.
9. Başqa istifadəçinin elanını redaktə və ya silmə sorğusunun `403` qaytardığını yoxlayın.

## 11. Bilinən məhdudiyyətlər

- Qeydiyyat kodları API instansiyasının yaddaşında saxlanılır. Restart və ya başqa instansiyaya keçid zamanı istifadəçiyə yeni kod lazım ola bilər. Çoxinstansiyalı mühit üçün ortaq saxlama tətbiq edilməlidir.
- Backend mesajları ingiliscədir. Elan etiketləri `PropertyLabels.cs` vasitəsilə frontend-in mövcud dilində qaytarılır.
- `Mərdəkan` axtarışı məlumat bazasında `Merdakan` kimi saxlanan rayon adını tapmaya bilər. Rayon adını admin vasitəsilə düzəltmək olar.
- `Verified` nişanı admin tərəfindən təsdiqlənmiş və demo olmayan elanı göstərir.