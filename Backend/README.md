# ArsaDuragi Backend

Torpaq / əmlak alqı-satqı platforması **ArsaDuragi** üçün ASP.NET Core Web API (.NET 8), Clean Architecture.

> **Status (vacib):** Bu kod yazılan mühitdə .NET SDK və NuGet girişi yox idi. Buna görə **kod hələ compile edilməyib və testlər işə salınmayıb.**
> Yalnız statik yoxlamalar aparılıb (mötərizə balansı, `using`/namespace uyğunluğu). İlk dəfə `dotnet build` və `dotnet test` işlədin.
> Xəta çıxarsa, mətnini mənə göndərin, düzəldərəm. EF Core **migration faylları da yaradılmayıb**: onları aşağıdakı əmrlə siz yaradırsınız.

## 1. Texnologiyalar

ASP.NET Core 8 · EF Core 8 + SQL Server · ASP.NET Core Identity · JWT + Refresh Token · AutoMapper 13 · FluentValidation 11 · Swagger · xUnit + Moq.
Şəkillər yalnız **lokal diskdə** saxlanılır (`wwwroot/uploads/properties/`). MinIO və ya başqa cloud storage yoxdur.

Paket versiyalarını oflayn yoxlaya bilmədim. İlk build-dən sonra: `dotnet list package --vulnerable` və `dotnet list package --outdated`.

## 2. Arxitektura

```
Domain  <-  Application  <-  Infrastructure
                 ^                 
                 +-------------  Persistence      API  ->  Application, Infrastructure, Persistence
```

| Layer | Nə üçün var | Nə var |
|---|---|---|
| **Domain** | Biznesin özəyi. Heç nədən asılı deyil | Entity-lər, enum-lar |
| **Application** | Biznes qaydaları. Verilənlər bazasını və ya HTTP-ni bilmir | DTO, servis və repository **interfeysləri**, servislər, validator-lar, AutoMapper, exception-lar |
| **Persistence** | Verilənlər bazası | `AppDbContext`, Fluent API konfiqurasiyaları, repository **implementasiyaları**, seed |
| **Infrastructure** | Xarici sistemlər | JWT, lokal fayl saxlama (`IFileStorageService`), e-mail |
| **API** | HTTP girişi | Nazik controller-lar, middleware, DI, JWT, CORS, Swagger |

**Bir sorğunun yolu** (məs. `PUT /api/properties/5`):
`Controller` (yalnız HTTP) → `ValidationFilter` (FluentValidation) → `PropertyAdService` (sahiblik yoxlaması, biznes qaydası) → `IPropertyAdRepository` (interfeys Application-dadır) → `PropertyAdRepository` (EF Core, SQL) → `IUnitOfWork.SaveChangesAsync`.
Servis konkret repository-ni tanımır, yalnız interfeysi tanıyır. Konkret sinifi **Dependency Injection** konteyneri verir (`DependencyInjection.cs` fayllarında qeydiyyat).

**Əsas qərarlar**

- DTO → entity çevrilməsi servisdə **əl ilə** edilir (over-posting qorunması). Entity → DTO üçün AutoMapper istifadə olunur.
- İstifadəçi id həmişə **JWT-dən** (`sub` claim) oxunur. Heç vaxt request body-dən yox.
- Elan silmə = **soft delete** (`IsDeleted` + global query filter). Şəkillər isə diskdən və cədvəldən silinir.
- **Elanı yalnız Admin yerləşdirir** (`Moderation:AdminOnlyPosting=true`, standart). Müştərilər qeydiyyatdan keçib yalnız baxır və favoritə əlavə edir. Elan yaratma/redaktə/silmə/şəkil əməliyyatları müştəri üçün 403 verir. Müştərilərə də icazə vermək üçün `AdminOnlyPosting=false` edin: onda aşağıdakı moderasiya axını işləyir.
- Moderasiya (müştəri postinq rejimində): yeni elan `Pending` olur. Yalnız `Active` elanlar ictimaiyyətə görünür. `Moderation:RequireApproval=false` ilə söndürmək olar.
- İstifadəçi aktiv (`Active`) və ya rədd edilmiş elanı redaktə edəndə yenidən `Pending` olur (təsdiqdən sonra məzmunu dəyişmək olmasın deyə). Admin redaktəsi statusu dəyişmir.
- "Bir elanda yalnız bir əsas şəkil" qaydası servisdə təmin edilir. DB filtered unique index-i istifadə edilməyib: iki UPDATE-in sırası unique pozuntusu yarada bilər.
- Enum-lar JSON-da **string** kimi gedir (`"Residential"`).

## 3. Qovluq strukturu

```
ArsaDuragi/
├── ArsaDuragi.sln, Directory.Build.props
├── ArsaDuragi.Domain/          Entities (User, PropertyAd, PropertyImage, Favorite, City, District, RefreshToken), Enums
├── ArsaDuragi.Application/     DTOs, Interfaces, Services, Validators, Mappings, Exceptions, Common
├── ArsaDuragi.Infrastructure/  Services (JwtService), FileStorage (LocalFileStorageService), Email
├── ArsaDuragi.Persistence/     Context, Configurations, Repositories, Seed, Migrations (boş: aşağıya baxın)
├── ArsaDuragi.API/             Controllers, Middlewares, Filters, Extensions, wwwroot/uploads/properties
├── ArsaDuragi.Tests/           xUnit (unit + API integration)
├── frontend-patched/           Frontend üçün yeni api/ faylları və minimal patch-lənmiş komponentlər
└── docs/FRONTEND_CONTRACT.md   Frontend ↔ backend DTO uyğunluğu
```

## 4. Quraşdırma və işə salma

Tələb: .NET 8 SDK, SQL Server (və ya Windows-da LocalDB).

```bash
cd ArsaDuragi

# 1) Gizli məlumatlar (heç vaxt faylda saxlanmır)
cd ArsaDuragi.API
dotnet user-secrets set "Jwt:Secret" "ən-azı-32-simvolluq-təsadüfi-uzun-mətn-yazın!!"
dotnet user-secrets set "Seed:AdminEmail" "admin@arsaduragi.local"
dotnet user-secrets set "Seed:AdminPassword" "GüclüŞifrə123"
cd ..

# 2) Connection string: appsettings.Development.json-da LocalDB üçün hazırdır.
#    Başqa SQL Server üçün:
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=ArsaDuragiDb;User Id=sa;Password=...;TrustServerCertificate=True" --project ArsaDuragi.API

# 3) Build
dotnet build

# 4) Migration (YALNIZ BİR DƏFƏ yaradılır, sonra Git-ə əlavə edilir)
dotnet tool install --global dotnet-ef --version 8.*
dotnet ef migrations add InitialCreate --project ArsaDuragi.Persistence --startup-project ArsaDuragi.API --output-dir Migrations

# 5) İşə sal (Development-də migration avtomatik tətbiq olunur və seed işləyir)
dotnet run --project ArsaDuragi.API --launch-profile http
```

- Swagger: **http://localhost:5080/swagger**
- `dotnet ef` Program.cs-i yükləyir. Ona görə `Jwt:Secret` əvvəlcədən təyin olunmalıdır (addım 1).
- Əl ilə miqrasiya tətbiqi: `dotnet ef database update --project ArsaDuragi.Persistence --startup-project ArsaDuragi.API`
- Production-da `Database:AutoMigrate` standart olaraq **söndürülüb**. Migration-u CI/CD-də və ya əl ilə tətbiq edin. Gizli məlumatları mühit dəyişənləri ilə verin (`Jwt__Secret`, `ConnectionStrings__DefaultConnection`, `Seed__AdminPassword`).
- Seed (idempotent): `Admin`/`User` rolları, admin hesabı (yalnız `Seed:AdminEmail/Password` verilibsə), şəhər **Bakü** + 9 rayon, və `Seed:DemoListings=true` olduqda (Development-də standart) frontend-dəki **9 demo elan** (`IsDemo=true`, `Verified=false`, sahibi `demo-owner@arsaduragi.invalid`, bu hesaba giriş mümkün deyil).

## 5. Test

```bash
dotnet test
```

Unit testlər: axtarış/filtr/sıralama/pagination (EF InMemory), servislər (sahiblik, moderasiya, favorit, şəkil), Auth (Moq), validator-lar, AutoMapper konfiqurasiyası, fayl yoxlaması (real fayl sistemi, müvəqqəti qovluq).
Integration testlər: real API pipeline (`WebApplicationFactory`), JWT, 401/403/409, moderasiya axını, refresh token rotasiyası, şəkil yükləmə.
InMemory provider SQL Server deyil. Unique index-lər, SQL `LIKE` kollasiyası və cascade qaydaları orada yoxlanmır. Real SQL Server-də əl ilə sınayın (aşağıdakı siyahı).

## 6. Autentifikasiya

1. `POST /api/auth/register` və ya `/login` → `accessToken` (15 dəq) + `refreshToken` (7 gün).
2. Sorğulara `Authorization: Bearer <accessToken>` əlavə edin.
3. 401 alanda `POST /api/auth/refresh` `{ "refreshToken": "..." }` → yeni cüt. **Köhnə refresh token ləğv olunur (rotasiya).** Ləğv olunmuş token yenidən göndərilsə (oğurluq əlaməti), həmin istifadəçinin bütün token-ləri ləğv edilir.
4. `POST /api/auth/logout` refresh token-i ləğv edir.

Refresh token DB-də yalnız **SHA-256 hash** kimi saxlanır. 5 uğursuz girişdən sonra hesab 15 dəqiqə bloklanır. `/api/auth/*` üçün IP başına dəqiqədə 10 sorğu limiti var.
Frontend nümunəsi token-ləri `localStorage`-də saxlayır (sadə, amma XSS-ə açıq). Daha sərt variant: refresh token-i `HttpOnly; Secure; SameSite` cookie-də saxlamaq (backend dəyişikliyi tələb edir).

## 7. Endpoint-lər

Cavab zərfi: `{ "success": true, "message": "...", "data": ..., "errors": [] }`

| Metod | URL | Giriş | Qeyd |
|---|---|---|---|
| POST | `/api/auth/register` | açıq | 201; 409 e-poçt təkrarı |
| POST | `/api/auth/login` | açıq | 401 səhv, 403 bloklanıb |
| POST | `/api/auth/refresh` · `/logout` | açıq | rotasiya · 204 |
| GET | `/api/users/me` | giriş | profil + rollar |
| GET | `/api/users/me/properties?status=` | giriş | öz elanlarım (Pending/Rejected daxil) |
| GET | `/api/properties` | açıq | yalnız `Active`; filtrlər aşağıda |
| GET | `/api/properties/featured?take=6` | açıq | |
| GET | `/api/properties/{idOrSlug}` | açıq | id və ya slug. Pending yalnız sahibə/admin-ə görünür, başqaları üçün 404 |
| POST | `/api/properties` | giriş | 201, `Pending` (admin üçün dərhal `Active`) |
| PUT | `/api/properties/{id}` | sahib/admin | |
| DELETE | `/api/properties/{id}` | sahib/admin | 204, soft delete |
| PUT | `/api/properties/{id}/status` | sahib/admin | `Sold`, `Archived`, `Pending` (yenidən göndər) |
| POST | `/api/properties/{id}/images` | sahib/admin | multipart, sahə adı `files`, max 10 şəkil, 5 MB, JPEG/PNG/WebP |
| PUT | `/api/properties/{id}/images/{imageId}` | sahib/admin | şəkli yenisi ilə əvəz et (sahə `file`) |
| DELETE | `/api/properties/{id}/images/{imageId}` | sahib/admin | |
| PUT | `/api/properties/{id}/images/{imageId}/main` | sahib/admin | |
| GET | `/api/favorites` · `/api/favorites/ids` | giriş | |
| POST/DELETE | `/api/favorites/{propertyId}` | giriş | 201 / 409 təkrar / 204 |
| GET | `/api/cities` · `/api/districts?cityId=` | açıq | |
| POST/PUT/DELETE | `/api/cities[/{id}]` · `/api/districts[/{id}]` | Admin | silmə: 409 (asılı data varsa) |
| GET | `/api/admin/properties?status=Pending` | Admin | moderasiya növbəsi |
| POST | `/api/admin/properties/{id}/approve` · `/reject` | Admin | reject: `{ "reason": "..." }` |
| PUT | `/api/admin/properties/{id}/featured` | Admin | `{ "isFeatured": true }` |
| GET | `/api/admin/users` · PUT `/users/{id}/lock` · `/unlock` | Admin | lock refresh token-ləri də ləğv edir |

**`GET /api/properties` parametrləri:** `search, districtId, cityId, propertyType (Land|Field|VineyardOrchard), zoning (Residential|Agricultural|Investment), minPrice, maxPrice, minArea, maxArea, featured, sortBy (listedAt|price|area|featured), sortOrder (asc|desc), page, pageSize (1..50, default 12)`. Hamısı birlikdə işləyir.
`search` başlıq, ünvan, təsvir, rayon və şəhər adında axtarır (SQL Server `LIKE`, kiçik/böyük hərfə həssas deyil).

### Nümunələr

```http
POST /api/auth/register
{ "fullName": "Elvin Məmmədov", "email": "elvin@example.com", "phoneNumber": "+994501234567",
  "password": "Passw0rd1", "confirmPassword": "Passw0rd1" }
```
```json
{ "success": true, "message": "Registration successful",
  "data": { "userId": "…", "fullName": "Elvin Məmmədov", "email": "elvin@example.com", "roles": ["User"],
            "accessToken": "eyJ…", "accessTokenExpiresAt": "2026-11-02T10:15:00Z",
            "refreshToken": "kX3…", "refreshTokenExpiresAt": "2026-11-09T10:00:00Z" }, "errors": [] }
```
```http
POST /api/properties      (Authorization: Bearer …)
{ "title": "Merdakan'da konut imarlı arsa", "description": "Müstakil konut yapımına uygun, düz arsa …",
  "address": "Merdakan, Bakü", "price": 180000, "area": 600,
  "propertyType": "Land", "zoning": "Residential", "terrain": "Flat",
  "roadAccess": "MainRoadNearby", "roadDistanceMeters": 300,
  "utilities": ["Electricity", "Water"], "cityId": 1, "districtId": 1 }
```
```http
GET /api/properties?districtId=1&minPrice=50000&maxPrice=200000&sortBy=area&sortOrder=desc&page=1&pageSize=12
```
```json
{ "success": true, "message": "Properties retrieved successfully",
  "data": { "items": [ { "id": 1, "slug": "merdakan-konut-arsasi", "title": "…", "district": "Merdakan", "type": "Arsa",
                          "zoning": "Konut imarlı", "area": 600, "price": 180000, "featured": true, "image": "https://…" } ],
            "page": 1, "pageSize": 12, "totalCount": 1, "totalPages": 1 }, "errors": [] }
```
Şəkil yükləmə (curl): `curl -X POST -H "Authorization: Bearer $TOKEN" -F "files=@foto1.jpg" -F "files=@foto2.png" http://localhost:5080/api/properties/1/images`

## 8. Şəkil yükləmə təhlükəsizliyi

`LocalFileStorageService`: icazəli uzantı (`.jpg .jpeg .png .webp`) → **fayl imzası** (magic bytes) yoxlanır → uzantı real məzmunla uyğun olmalıdır → ölçü yükləmə zamanı axında yoxlanır (5 MB) → fayl adı **serverdə yaranır** (`{guid}.{uzantı}`, istifadəçinin adı heç vaxt işlədilmir) → yol `wwwroot/uploads` kökündən kənara çıxa bilməz (path traversal qorunması) → statik fayllar `X-Content-Type-Options: nosniff` ilə verilir. Uğursuzluqda yarımçıq fayl silinir.
DB-də yalnız nisbi URL (`/uploads/properties/{propertyId}/{guid}.jpg`) və `ObjectKey` saxlanır. API cavabında URL mütləq (absolute) olaraq qurulur (`FileStorage:PublicBaseUrl` və ya cari request-dən).
Əlavə sərtləşdirmə (istəyə görə): şəkli yenidən encode etmək (məs. ImageSharp), antivirus, CDN.

## 9. Frontend inteqrasiyası

`docs/FRONTEND_CONTRACT.md` və `frontend-patched/` qovluğuna baxın. Orijinal layihə dəyişdirilməyib. `frontend-patched/src` içindəki faylları `vite-project/src` üzərinə kopyalayın.
`.env`: `VITE_API_URL=http://localhost:5080`. CORS `http://localhost:5173` üçün açıqdır (`Cors:AllowedOrigins`).

## 10. İlk işə salmadan sonra əl ilə yoxlama siyahısı (real SQL Server)

1. `dotnet build` xətasızdır, `dotnet test` yaşıldır.
2. `dotnet ef migrations add …` işləyir; `dotnet run` bazanı yaradır, Swagger açılır.
3. Swagger-də admin ilə login → `GET /api/properties` 9 demo elan qaytarır.
4. İstifadəçi qeydiyyatı → elan yarat → ictimai siyahıda görünmür → admin approve → görünür.
5. Eyni favoriti iki dəfə əlavə et → ikincisi 409.
6. Şəkil yüklə → `wwwroot/uploads/properties/{id}/` altında fayl yaranır, URL brauzerdə açılır; `.jpg`-ə çevrilmiş `.exe` rədd edilir.
7. Başqa istifadəçinin elanını PUT/DELETE etməyə cəhd → 403.

## 11. Bilinən məhdudiyyətlər

- Migration faylları sizin tərəfinizdən yaradılmalıdır (yuxarıda).
- Backend mesajları ingiliscədir. Property mətnləri (etiketlər) frontend-in mövcud dilində (türkcə) qaytarılır (`PropertyLabels.cs`).
- Frontend-də hələ **UI-ı olmayan** funksiyalar: elan yaratma/redaktə, favorit düymələri, "elanlarım", admin paneli. API və `src/api/*.js` funksiyaları hazırdır. Səhifələrin dizaynı ayrıca işdir.
- Axtarış `Mərdəkan` (ə hərfi ilə) sorğusunu `Merdakan` kimi saxlanan datada tapmır (frontend datası `Merdakan` yazır). Rayon adını Admin vasitəsilə düzəldə bilərsiniz.
- `Verified` (`PropertyCard`-dakı "Doğrulanmış ilan" nişanı) = admin tərəfindən təsdiqlənmiş **və demo olmayan** elan.
