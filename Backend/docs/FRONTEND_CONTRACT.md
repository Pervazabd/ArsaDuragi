# Frontend ↔ Backend müqaviləsi (DTO uyğunluğu)

Bu sənəd `vite-project/src` faylları oxunaraq hazırlanıb: `data/properties.js`, `PropertyCard.jsx`, `PropertiesPage.jsx`,
`PropertyDetailsPage.jsx`, `DropdownSelect.jsx`, `Products.jsx`, `AuthPage.jsx`, `App.jsx`, `routes/index.jsx`.
Auth bölməsi cari `Frontend/src/pages/AuthPage.jsx` və `Frontend/src/services/api.js` fayllarına uyğun yenilənib.

## 1. Frontend-in oxuduğu sahələr (`property.*`)

Komponentlərdə faktiki istifadə olunanlar: `title, price, zoning, area, address, type, id, district, transaction, image, featured,
description, verified, utilities, terrain, roadAccess`. (`investment` yalnız köhnə `Products.jsx` məntiqində idi, datada yoxdur. `listedAt` yalnız sıralamada işlənirdi, ekranda göstərilmir.)

## 2. Sahə uyğunluğu

| `properties.js` sahəsi | API sahəsi (`PropertyAdListDto` / `PropertyAdGetDto`) | Qeyd |
|---|---|---|
| `id` (string slug) | `slug` (string) və `id` (int) | URL-də `slug` istifadə olunur → köhnə linklər (`/properties/merdakan-konut-arsasi`) işləməyə davam edir. Detail endpoint həm id, həm slug qəbul edir |
| `title` | `title` | eyni |
| `district` (ad) | `district` (ad) + `districtId` | filtr üçün **id** göndərilir |
| `address` | `address` | eyni |
| (yox) | `city`, `cityId` | yeni |
| `type` (`"Arsa"`) | `type` (`"Arsa"`) + `typeCode` (`"Land"`) | etiket eynidir; filtr kodla |
| `zoning` (`"Konut imarlı"`) | `zoning` (eyni mətn) + `zoningCode` (`"Residential"`) | etiketlər eyni; filtr kodla |
| `area` (number) | `area` (number, m²) | eyni |
| `price` (number) | `price` (number, AZN) + `pricePerSquareMeter` | kart özü də hesablayır, dəyişiklik lazım deyil |
| `featured` | `featured` | eyni |
| `listedAt` (`"2026-10-24"`) | `listedAt` (ISO UTC datetime) | |
| `terrain` (mətn) | `terrain` (eyni mətn) + `terrainCode` | |
| `roadAccess` (mətn, məs. `"Ana yola 300 m"`) | `roadAccess` (**eyni mətn**) + `roadAccessCode` + `roadDistanceMeters` | mətn serverdə enum + məsafədən qurulur |
| `utilities` (string[]) | `utilities` (string[], eyni etiketlər) + `utilityCodes` | `.map` olduğu kimi işləyir |
| `description` | `description` (yalnız detail) | |
| `image` (tək URL) | `image` (əsas şəklin **mütləq** URL-i) + `images[]` (`{id,url,isMain}`) | kart/detail `property.image` ilə eyni qalır |
| (datada yox, kart oxuyur) `transaction` | `transaction` = `"Satılık"` | |
| (datada yox, kart oxuyur) `verified` | `verified` | admin təsdiqli və demo olmayan |
| (yox) | `status`, `rejectionReason`, `isDemo`, `ownerName`, `ownerPhone` | |

`DropdownSelect` seçimləri `{ value, label }` formasındadır və `value` **sətir müqayisəsi** ilə işləyir (`===`). Ona görə rayon dəyəri `String(district.id)` kimi verilir.

## 3. Filtr və sıralama uyğunluğu

| UI (`PropertiesPage`) | Sorğu parametri |
|---|---|
| axtarış qutusu `query` | `search` |
| `zoning` (`"Konut imarlı"` …) | `zoning=Residential \| Agricultural \| Investment` (çevirmə `src/api/properties.js`-də) |
| `district` | `districtId` (rayon siyahısı `GET /api/districts`-dən) |
| `minPrice / maxPrice` | `minPrice / maxPrice` |
| `minArea / maxArea` | `minArea / maxArea` |
| `sort=newest` | `sortBy=listedAt&sortOrder=desc` |
| `sort=price-asc` / `price-desc` | `sortBy=price&sortOrder=asc` / `desc` |
| `sort=area-desc` | `sortBy=area&sortOrder=desc` |

| UI (`Products`, ana səhifə) | Sorğu |
|---|---|
| (həmişə yalnız seçilmişlər) | `featured=true` |
| `landType = arsa / tarla / bağ` | `propertyType = Land / Field / VineyardOrchard` |
| `landType = yatırım` | `zoning=Investment` (datada "yatırım" tip yox, **imar statusudur**) |
| `sort = featured / priceAsc / priceDesc / areaDesc` | `listedAt desc` / `price asc` / `price desc` / `area desc` |

## 4. Auth uyğunluğu (`AuthPage`)

### Gmail ilə qeydiyyat

| Form sahəsi (`name=`) | `RegisterDto` |
|---|---|
| `name` | `fullName` |
| `email` | `email` |
| `phone` | `phoneNumber` |
| `password` | `password` |
| `confirmPassword` | `confirmPassword` |
| `token` | `verificationToken` (6 rəqəmli mətn) |

1. `POST /api/auth/register/request-verification` endpoint-inə `{ "email": "user@gmail.com" }` göndərin. Sorğu yalnız `@gmail.com` ünvanları üçün qəbul edilir.
2. `202 Accepted` cavabı gələndə qeydiyyat sahələrini və **Gmail təsdiq kodu** sahəsini göstərin. `202` cavabının gövdəsi boşdur. Məktubun çatması SMTP sazlamasından asılıdır.
3. Məktubdakı 6 rəqəmli kodu `POST /api/auth/register` sorğusundakı `verificationToken` sahəsinə mətn kimi yazın. Məsələn, `"verificationToken": "012345"`. Dırnaqlar əvvəlindəki sıfırı qoruyur.
4. Uğurlu qeydiyyat `201 Created` və `AuthResponseDto` qaytarır. Etibarsız və ya vaxtı keçmiş kod `400`, etibarlı kodla eyni ünvanın təkrar qeydiyyatı `409` qaytarır.

Kod 30 dəqiqə etibarlıdır və sorğudakı Gmail ünvanına bağlıdır. Beş səhv cəhddən sonra yeni kod tələb olunur. Eyni ünvana yeni kod ən tez bir dəqiqə sonra göndərilə bilər. API yenidən başladıqda yeni kod istəyin. İstifadəçi formadakı Gmail ünvanını dəyişəndə kod istəmə mərhələsinə qayıdın.

### Giriş

`POST /api/auth/login` sorğusunda `email` və `password` göndərilir. Uğurlu giriş və qeydiyyatdan sonra `AuthResponseDto` məlumatları (`accessToken`, `refreshToken`, `roles` və s.) `localStorage["arsaDuragi.session"]` açarında saxlanılır. `AuthPage` istifadəçini `/` səhifəsinə yönləndirir.

### Şifrə sıfırlama

1. `POST /api/auth/forgot-password` endpoint-inə `{ "email": "user@gmail.com" }` göndərin. Düzgün formatlı sorğuya hesabın mövcudluğundan asılı olmayaraq `202 Accepted` və boş gövdə qaytarılır.
2. Məktubdakı sıfırlama tokenini `POST /api/auth/reset-password` endpoint-inə `email`, `token`, `newPassword` və `confirmPassword` sahələri ilə göndərin. `token` sahəsi məktubdakı tam şifrə sıfırlama tokenini qəbul edir.
3. Uğurlu sıfırlama `204 No Content` qaytarır. İstifadəçini giriş formasına qaytarın. Etibarsız token və ya sorğu üçün `400` qaytarılır.

Şifrə sıfırlama tokeni 30 dəqiqə etibarlıdır və uğurlu sıfırlamadan sonra təkrar istifadə edilə bilmir. Köhnə refresh tokenlər ləğv olunur. Artıq verilmiş access tokenlər öz bitmə vaxtına qədər qüvvədə qalır.

## 5. Frontend-də tapılan problemlər (hamısı sənədləşdirilib, bəziləri patch-də həll olunub)

1. **`properties.js`-də 9 elan var** (10 deyil). Hamısı seed olunur.
2. **`Products.jsx`-də rayon siyahısı köhnədir** (Bilecik Merkez, Bozüyük, Söğüt…). Datanın ünvanları isə Bakü rayonlarıdır (Merdakan, Bilgah…). Ona görə rayon filtri **heç bir nəticə vermirdi**. Patch: siyahı `GET /api/districts`-dən gəlir.
3. `Products.jsx` mətnləri hələ də "Bilecik ve çevresi" yazır. **Dəyişdirilməyib** (məzmun qərarıdır).
4. **`routes/index.jsx` ölü koddur.** Mövcud olmayan fayllara import edir (`Home`, `Properties` …). Real router `App.jsx`-dir. Toxunulmayıb. Silmək məsləhətdir.
5. `AuthPage` əvvəl demo idi. Cari auth forması giriş, Gmail kodu ilə qeydiyyat və şifrə sıfırlama endpoint-lərinə qoşulur.
6. `Navbar` həmişə "Giriş / Kayıt Ol" göstərir. Giriş etmiş istifadəçi üçün (profil, çıxış) UI **yazılmayıb**. Dizayn qərarı lazımdır (`isLoggedIn()`, `logout()` hazırdır).
7. **Elanı yalnız admin yerləşdirir** (backend-də standart). Müştəri tərəfində elan yaratma səhifəsi lazım deyil. Admin üçün elan idarəetmə paneli lazımdır (hələlik Swagger). Favorit düymələri, "elanlarım", admin paneli səhifələri **frontend-də mövcud deyil**. API hazırdır (`src/api/*.js`).
8. Axtarış davranışı dəyişir: köhnə frontend `type` və `zoning` mətnində də axtarırdı. API başlıq, ünvan, təsvir, rayon və şəhərdə axtarır (`"konut"` axtarışı artıq zoning etiketinə görə tapmır, yalnız təsvir/başlıqda keçirsə tapır). Lazım olsa, backend-ə əlavə olunur.
9. `Merdakan` / `Mərdəkan`: data Latın-türk yazılışındadır. Master promptdakı `search=Mərdəkan` nümunəsi bu datada nəticə verməz.
10. Səhifələmə UI-ı yoxdur: patch bir sorğuda `pageSize=50` istəyir (API maksimumu). 50-dən çox elan olduqda "daha çox göstər" düyməsi lazım olacaq.

## 6. İlkin patch faylları (`frontend-patched/src`)

Yeni: `api/http.js` (fetch + token + avto-refresh), `api/auth.js`, `api/properties.js`, `api/locations.js`, `api/favorites.js`, `utils/format.js` (`formatPrice`).
Dəyişən (minimal): `components/PropertyCard.jsx` (import + link `slug`), `components/Products.jsx`, `pages/PropertiesPage.jsx`, `pages/PropertyDetailsPage.jsx`, `pages/AuthPage.jsx`.
UI/CSS dəyişməyib. `data/properties.js` artıq import olunmur (silmək olar). Fayllarda orijinal CRLF sətir sonları saxlanılıb.
**İlkin patch-in JSX faylları build edilməyib** (`npm run dev` ilə yoxlayın).

Cari auth inteqrasiyası `Frontend/src/pages/AuthPage.jsx` və `Frontend/src/services/api.js` fayllarındadır.

## 7. Fetch nümunələri

### Cari frontend-də autentifikasiya

Aşağıdakı import `Frontend/src` qovluğundakı modul üçün nəzərdə tutulub. `src/pages/AuthPage.jsx` daxilində nisbi yol `../services/api.js` olur.

```js
import {
  login,
  register,
  requestRegistrationVerification,
  requestPasswordReset,
  resetPassword,
} from "./services/api.js";
```

Qeydiyyat kodunu istəyin. İstifadəçi məktubdakı kodu daxil etdikdən sonra `register` çağırın. `"012345"` yalnız sorğu formatını göstərən nümunədir.

```js
await requestRegistrationVerification("user@gmail.com");

// Bu çağırış istifadəçi məktubdakı kodu formaya daxil etdikdən sonra edilir.
await register({
  fullName: "Elvin Məmmədov",
  email: "user@gmail.com",
  phoneNumber: "+994501234567",
  password: "Passw0rd1",
  confirmPassword: "Passw0rd1",
  verificationToken: "012345",
});
```

Mövcud hesabla giriş:

```js
await login({
  email: "user@gmail.com",
  password: "Passw0rd1",
});
```

Şifrə sıfırlama:

```js
await requestPasswordReset("user@gmail.com");

// İstifadəçi məktubdakı 6 rəqəmli kodu daxil etdikdən sonra çağırılır.
await resetPassword({
  email: "user@gmail.com",
  token: "065432",
  newPassword: "NewPassw0rd",
  confirmPassword: "NewPassw0rd",
});

### İlkin patch-də elan və favorit sorğuları

```js
import { searchProperties, getProperty, createProperty, uploadImages } from "./api/properties.js";
import { addFavorite, removeFavorite } from "./api/favorites.js";

await searchProperties({ query: "Merdakan", district: "1", minPrice: "50000", maxPrice: "200000", sort: "area-desc" });
await getProperty("merdakan-konut-arsasi");
const ad = await createProperty({ title: "…", description: "…", address: "…", price: 100000, area: 500,
  propertyType: "Land", zoning: "Residential", cityId: 1, districtId: 1,
  terrain: "Flat", roadAccess: "AsphaltFrontage", utilities: ["Electricity", "Water"] });
await uploadImages(ad.id, fileInput.files);
await addFavorite(ad.id);
await removeFavorite(ad.id);
```