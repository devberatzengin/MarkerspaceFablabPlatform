# Makerspace & Fablab Platform

Makerspace/Fablab işletmeleri için ekipman kiralama, rezervasyon, ücretlendirme ve duyuru yönetimi sağlayan REST API ve React arayüzü.

Kullanıcılar taşınabilir ekipmanı **kiralar**, sabit/tezgah üstü ekipmanı zaman dilimi bazlı **rezerve eder**. Üyelik seviyesi eşzamanlı ekipman kotasını, indirim oranını ve gecikme cezasını belirler. Ekipman iadesinde ücret hesaplanır ve kullanıcı bakiyesinden tahsil edilir. Takip edilen ekipman/kategorilerde olay gerçekleştiğinde uygulama içi veya e-posta bildirimi gönderilir.

---

## İçindekiler

- [Teknoloji Yığını](#teknoloji-yığını)
- [Mimari](#mimari)
- [Uygulanan Tasarım Desenleri](#uygulanan-tasarım-desenleri)
- [Domain Modeli](#domain-modeli)
- [İş Kuralları](#iş-kuralları)
- [Kimlik Doğrulama ve Yetkilendirme](#kimlik-doğrulama-ve-yetkilendirme)
- [API Referansı](#api-referansı)
- [Hata Yönetimi](#hata-yönetimi)
- [Bildirim Sistemi](#bildirim-sistemi)
- [Zaman / UTC Politikası](#zaman--utc-politikası)
- [Yapılandırma](#yapılandırma)
- [Kurulum ve Çalıştırma](#kurulum-ve-çalıştırma)
- [Frontend](#frontend)
- [Bilinen Eksikler ve Teknik Borç](#bilinen-eksikler-ve-teknik-borç)

---

## Teknoloji Yığını

| Katman | Teknoloji |
|---|---|
| Runtime | .NET 10 (`net10.0`) |
| Web | ASP.NET Core Controllers |
| ORM | Entity Framework Core 10.0.10 |
| Veritabanı | PostgreSQL 16 (Npgsql 10.0.3) |
| Kimlik doğrulama | JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`) |
| Şifre hash | ASP.NET Core Identity `PasswordHasher<User>` |
| Doğrulama | FluentValidation 11.3.1 |
| Nesne eşleme | AutoMapper 16.2.0 |
| Loglama | Serilog (Console + günlük dosya, 14 gün saklama) |
| E-posta | MailKit 4.17.0 / MimeKit |
| API dokümantasyonu | Microsoft.AspNetCore.OpenApi + Scalar 2.16.16 |
| Önbellek | `IMemoryCache` |
| Frontend | React + TypeScript + Vite + Tailwind CSS + Axios |

---

## Mimari

Katmanlı mimari; `Controller → Service → Repository → DbContext` zinciri. Controller'lar iş kuralı içermez, yalnızca servisi çağırıp `ICurrentUserService` üzerinden kimlik bilgisini geçer.

```
Controllers/          HTTP uçları, yetkilendirme attribute'ları
Services/             İş kuralları, doğrulama tetikleme
Services/Interfaces/  Servis sözleşmeleri
Data/
  AppDbContext.cs     DbSet'ler, global UTC dönüştürücü
  UnitOfWork.cs       Repository toplayıcı + SaveChanges
  Repositories/       Generic Repository<T> + varlığa özel sorgular
  CachedRepositoties/ Decorator: CachedCategoryRepository
  Configurations/     Fluent API yapılandırmaları, query filter'lar
  Interfaces/         Repository sözleşmeleri
Entities/             Domain varlıkları
Entities/Enums/       Sabit listeler
Dtos/                 İstek/yanıt modelleri (varlık asla dışa açılmaz)
Profiles/             AutoMapper profilleri
Validators/           FluentValidation kuralları
States/               State pattern (Equipment + Announcement)
Strategies/           Strategy pattern (üyelik kademeleri)
Events/               Domain event yayıncısı + handler'lar
Notifications/        Bildirim kanalları ve fabrikası
Handlers/             GlobalExceptionHandler
Helpers/              ClaimsPrincipal uzantıları, SmtpOptions, DateTimeExtensions
Excepitons/           Uygulama exception hiyerarşisi
Migrations/           EF Core migration'ları
frontend/             React SPA
```

---

## Uygulanan Tasarım Desenleri

| Desen | Nerede | Amaç |
|---|---|---|
| **Repository** | `Data/Repositories/Repository<T>` | Generic CRUD; `Query(asNoTracking = true)` varsayılan izlemesiz |
| **Unit of Work** | `Data/UnitOfWork.cs` | Tüm repository'ler tek `DbContext` ve tek `SaveChangesAsync` paylaşır |
| **Decorator** | `CachedCategoryRepository` | `ICategoryRepository`'yi sarar, 5 dakikalık bellek önbelleği ekler; `DbContext.SavedChanges` olayına bağlanıp commit sonrası önbelleği temizler |
| **State** | `States/EquipmentStates`, `States/AnnouncementStates` | Geçersiz durum geçişlerini exception ile engeller |
| **Strategy** | `Strategies/MembershipStrategies` | Üyelik kademesine göre kota/indirim/ceza hesabı |
| **Factory** | `StateFactory`, `MembershipStrategyFactory`, `NotificationChannelFactory` | Duruma/kademeye/kanala göre doğru nesneyi üretir |
| **Observer (Domain Events)** | `Events/DomainEventPublisher` | Yayıncı, DI'dan `IDomainEventHandler<T>` listesini çözer; handler hataları yutulup loglanır, ana işlem düşmez |
| **Options** | `SmtpOptions` + `IOptionsMonitor<T>` | Yapılandırmanın tipli okunması |

---

## Domain Modeli

### Varlıklar

**User** — `Id`, `Type` (UserType), `Status` (MembershipStatus), `Username`, `PasswordHash`, `Email`, `FirstName`, `LastName`, `PhoneNumber`, `EquipmentLevel` (short, varsayılan 1), `Balance` (decimal), `IsActive`, `IsDeleted`, `CreatedAt`, `UpdatedAt`

**Equipment** — `Id`, `Name`, `Description`, `Type` (EquipmentType), `PlacementType` (EquipmentPlacementType), `Status` (EquipmentStatus), `RequiredUserLevel` (short), `IsDeleted`, `EquipmentRentals` (koleksiyon), `CreatedAt`, `UpdatedAt`

**EquipmentRental** — `Id`, `UserId`, `EquipmentId`, `RentedAt`, `ReleasedAt?`, `ExpectedReturnAt`, `PaymentId?`, `IsPaid`, `PaidAt?`, `IsOverdue`, `OverdueBy?` (TimeSpan → `interval`)

**Payment** — `Id`, `PaymentNumber`, `EquipmentRentalId`, `UserId`, `RentalFee`, `LateFee`, `DiscountAmount`, `TotalAmount`, `PaymentMethod`, `Status`, `CreatedAt`, `PaidAt?`, `RefundedAt?`, `IsDeleted`

**Announcement** — `Id`, `Title`, `Content`, `CreatedByUserId`, `CategoryId`, `Status` (ContentStatus), `CreatedAt`, `UpdatedAt`

**Category** — `Id`, `Name`, `Type` (CategoryType), `IsActive`, `IsDeleted`, `CreatedAt`, `UpdatedAt`

**Subscription** — `Id`, `UserId`, `TargetType`, `CategoryId?`, `EquipmentId?`, `Channel`, `IsActive`, `IsDeleted`, `CreatedAt`, `UpdatedAt`

**Notification** — `Id`, `UserId`, `Type`, `Channel`, `Title`, `Message`, `RelatedEntityId?`, `ThresholdMinutes?`, `IsRead`, `ReadAt?`, `IsDeleted`, `CreatedAt`

### Enum'lar

```
UserType                 Unknown, Admin, User, Staff
MembershipStatus         Unknown, Free, Bronze, Silver, Gold, Professional
UserEquipmentLevel       Unknown, Beginner, Intermediate, Advanced
EquipmentType            Unknown, DigitalFabrication, HandTools, PowerTools, Electronics, Measurement
EquipmentPlacementType   Unknown, Portable, Benchtop, FloorStationary
EquipmentStatus          Unknown, Available, Reserved, Rented, Maintenance
ContentStatus            Draft, Published, Unpublished, Archived
CategoryType             Undefined, Draft, Published, Unpublished, Archived
PaymentStatus            Pending, Paid, Failed, Refunded, Cancelled
PaymentMethod            Balance, Cash, CreditCard, DebitCard, BankTransfer, Stripe, PayPal, Other
NotificationType         AnnouncementPublished, EquipmentMaintenance, EquipmentAvailable, RentalEndsSoon
NotificationChannelType  InApp, Email
SubscriptionTargetType   Category, Equipment
```

Enum'lar JSON'da **string** olarak serileştirilir (`JsonStringEnumConverter`).

### Global Query Filter'lar

| Varlık | Filtre |
|---|---|
| User, Equipment, Category | `!IsDeleted` |
| Payment, Subscription, Notification | `!IsDeleted` |
| Announcement | `Status != Archived` |

Arşivlenmiş duyuru ve soft-delete edilmiş kayıtlar hiçbir sorguda görünmez.

---

## İş Kuralları

### Yerleşim tipi kiralama biçimini belirler

- `Portable` → **kiralanır** (`/rent`, `/rent-later`). Kullanıcı fiziksel olarak alıp götürür.
- `Benchtop`, `FloorStationary` → **rezerve edilir** (`/reserve`, `/reserve-later`). Yerinde, zaman dilimi bazlı kullanılır.

Yanlış tipte istek `EquipmentNotPortableException` (409) döner.

### Üyelik kademeleri

| Kademe | Aidat | Eşzamanlı kota | İndirim | Gecikme cezası |
|---|---|---|---|---|
| Free | 0 ₺ | 1 | %0 | 100 ₺/saat |
| Bronze | 10 ₺ | 3 | %5 | 75 ₺/saat |
| Silver | 50 ₺ | 5 | %10 | 50 ₺/saat |
| Gold | 100 ₺ | 10 | %20 | 25 ₺/saat |
| Professional | 1000 ₺ | 20 | %40 | 0 ₺ |

`Unknown` durumundaki kullanıcı `Free` kademesine düşer. Yeni kayıtlar `Unknown` ile başlar.

### Kiralama akışı

1. **Bakım kontrolü** — `Maintenance` durumundaki ekipman kiralanamaz/rezerve edilemez.
2. **Yerleşim tipi kontrolü** — yukarıdaki eşleşme.
3. **Seviye kontrolü** — `user.EquipmentLevel >= equipment.RequiredUserLevel`, aksi halde `InsufficientEquipmentLevelException` (403).
4. **Kota kontrolü** — kullanıcının açık (`ReleasedAt == null`) kayıt sayısı kademesinin kotasına ulaştıysa `RentalLimitExceededException` (409). **İleri tarihli rezervasyonlar da kotaya sayılır** — kasıtlı: teslim etmeden yenisi alınamaz.
5. **Çakışma kontrolü** — `IsAlreadyTakenThisTimespan` aynı ekipman için örtüşen açık kayıt varsa `ConflictException` (409).
6. **Durum geçişi** — pencere şu an başlıyorsa (`start <= now + 5dk`) ekipman `Rented`/`Reserved` olur; ileri tarihli kayıtlarda ekipman durumu değişmez.

### İade ve ücretlendirme

- İade `ReleaseItAsync` ile yapılır ve **çağıranın kendi açık kaydını** hedefler (`GetOpenRentalForUserAsync`). Aynı ekipmanda birden fazla açık kayıt olabildiği için "en eski kayıt" varsayımı kullanılmaz.
- `ReleasedAt` dolduğunda kayıt ödenebilir hale gelir.
- Günlük tarife **100 ₺**. Ücret `Math.Ceiling(ExpectedReturnAt - RentedAt)` gün üzerinden hesaplanır — yani 2 saatlik kiralama da 1 tam gün ücreti alır.
- Gecikme cezası `ReleasedAt - ExpectedReturnAt` süresi üzerinden kademe oranıyla hesaplanır.
- İndirim `(RentalFee + LateFee)` üzerine uygulanır.
- `TotalAmount = max(0, RentalFee + LateFee - DiscountAmount)`, tüm tutarlar 2 haneye `AwayFromZero` yuvarlanır.
- Ödeme yalnızca **bakiyeden** yapılır (`PaymentMethod.Balance`). Bakiye yetersizse `InsufficientBalanceException` (409).
- Aynı kiralama için ikinci ödeme `DuplicateEntityException` (409) döner.

### Bakıma alma

Admin bakıma aldığında ekipmanı tutan aktif kayıt kapatılır (`ReleasedAt` yazılır) ki kullanıcının kotası serbest kalsın, ancak **ücret alınmaz** (`IsPaid = true`, `PaidAt = null`). Kullanıcıya uygulama içi bildirim gönderilir.

### Duyuru durum makinesi

`Draft → Published → Unpublished → Published` geçişleri serbest; `Archived` terminal durumdur ve global query filter yüzünden listelerde görünmez. Yayınlama/geri çekme/arşivleme **yalnızca Admin**. Duyuru oluşturma tüm kullanıcılara açıktır ve `Draft` durumunda başlar.

---

## Kimlik Doğrulama ve Yetkilendirme

JWT Bearer. Token içeriği:

| Claim | Değer |
|---|---|
| `sub` | Kullanıcı Id (GUID) |
| `email` | E-posta |
| `role` | `UserType` (Admin/User/Staff) |
| `jti` | Rastgele GUID |
| `exp` | `UtcNow + Jwt:ExpiresMinutes` |

Doğrulama parametreleri: issuer, audience, lifetime ve imza doğrulanır; `ClockSkew = TimeSpan.Zero` (tolerans yok); `RoleClaimType` uzun şema URI'si.

`sub`, .NET'in varsayılan gelen claim haritası tarafından `ClaimTypes.NameIdentifier`'a eşlenir — `CurrentUserService` bunu okur.

Rol kuralları: tüm controller'lar sınıf düzeyinde `[Authorize]` taşır (**AuthController hariç**, anonim). Admin gerektiren uçlar API tablolarında işaretli.

---

## API Referansı

Taban yol `/api`. Tüm yanıtlar JSON. Sayfalı yanıtlar `PagedResponse<T>` sarmalayıcısı kullanır: `items`, `page`, `pageSize`, `totalCount`, `totalPages`.

`page` en az 1'e, `pageSize` 1–100 aralığına sıkıştırılır.

### Auth — `/api/Auth` (anonim)

| Metot | Yol | Gövde | Yanıt |
|---|---|---|---|
| POST | `/register` | `RegisterRequest` | `AuthResponse` |
| POST | `/login` | `LoginRequest` | `AuthResponse` |

- `RegisterRequest`: `username` (6–30), `password` (8–20), `email` (geçerli e-posta, 3–100), `firstName` (≤20, zorunlu), `lastName` (≤20), `phoneNumber`
- `LoginRequest`: `email`, `password`
- `AuthResponse`: `token`, `email`, `type`

Kayıt sırasında e-posta benzersizliği kontrol edilir. Giriş; kullanıcı yoksa, şifre yanlışsa veya hesap pasif/silinmişse aynı `401` mesajını döner (bilgi sızdırmamak için).

### Equipment — `/api/Equipment`

| Metot | Yol | Yetki | Parametre |
|---|---|---|---|
| GET | `/` | Auth | `page`, `pageSize`, `status`, `type`, `placementType`, `search` (query) |
| GET | `/{id:guid}` | Auth | — |
| GET | `/my-equipments` | Auth | `page`, `pageSize`, `includePast` (bool, varsayılan `false`) |
| POST | `/` | **Admin** | `CreateRequest` (gövde) |
| PUT | `/` | **Admin** | `UpdateRequest` (gövde) |
| DELETE | `/` | **Admin** | `id` (query) — soft delete |
| PATCH | `/{id:guid}/rent` | Auth | `span` (query, `HH:MM:SS`) |
| PATCH | `/{id:guid}/rent-later` | Auth | `ScheduleRequest` (gövde) |
| PATCH | `/{id:guid}/reserve` | Auth | `span` (query) |
| PATCH | `/{id:guid}/reserve-later` | Auth | `ScheduleRequest` (gövde) |
| PATCH | `/{id:guid}/release` | Auth | — |
| PATCH | `/{id:guid}/maintenance` | **Admin** | — |
| PATCH | `/{id:guid}/unmaintenance` | **Admin** | — |

- `CreateRequest`: `name` (zorunlu), `description`, `type`, `placementType`, `requiredUserLevel`
- `UpdateRequest`: `id` (zorunlu), `name?`, `status?`, `usageTime?`, `requiredUserLevel?` — yalnızca `null` olmayan alanlar uygulanır
- `ScheduleRequest`: `startAt` (ISO-8601 UTC, `...Z`), `span` (.NET TimeSpan, `[d.]hh:mm:ss`). `startAt` 5 dakikadan fazla geçmişte olamaz, `span > 0` olmalı
- `Equipment.Response`: `id`, `name`, `description`, `type`, `placementType`, `status`, `requiredUserLevel`, `isDeleted`, `currentUserId?`, `availableAt`
- `EquipmentRental.Response`: `id`, `userId`, `equipmentId`, `equipmentName`, `equipmentDescription`, `rentedAt`, `releasedAt?`, `expectedReturnAt`, `isActive`

`currentUserId` ve `availableAt`, penceresi başlamış (`RentedAt <= now`) ve iade edilmemiş kayıtların en eskisinden hesaplanır — ileri tarihli rezervasyon "şu anki kullanıcı" olarak görünmez.

### Users — `/api/Users`

| Metot | Yol | Yetki |
|---|---|---|
| GET | `/` | **Admin** |
| GET | `/me` | Auth |
| GET | `/{id:guid}` | **Admin** |
| PUT | `/{id:guid}` | Auth — yalnızca kendi kaydı (`NotResourceOwnerException` aksi halde) |
| PATCH | `/{id:guid}/activate` | **Admin** |
| PATCH | `/{id:guid}/deactivate` | **Admin** |
| DELETE | `/{id:guid}` | **Admin** — soft delete |
| POST | `/me/change-password` | Auth — `ChangePasswordRequest` |
| POST | `/me/add-balance` | Auth — `balance` (query, > 0) |

- `UserResponse`: `id`, `username`, `firstName`, `lastName`, `email`, `phoneNumber`, `type`, `equipmentLevel`, `balance`, `status`, `isActive`, `createdAt`
- `UpdateRequest`: `firstName?`, `lastName?`, `email?`, `phoneNumber?`
- `ChangePasswordRequest`: `currentPassword`, `newPassword`

E-posta güncellenirken benzersizlik kontrol edilir; çakışma `DuplicateEntityException` (409).

### Payment — `/api/Payment`

| Metot | Yol | Yetki | Açıklama |
|---|---|---|---|
| GET | `/` | **Admin** | Tüm ödemeler; `page`, `pageSize`, `status`, `paymentMethod`, `search` (ödeme numarası) |
| GET | `/my-payments` | Auth | Kendi ödemeleri |
| GET | `/user/{userId:guid}` | **Admin** | Belirli kullanıcının ödemeleri |
| GET | `/{id:guid}` | Auth | Sahibi veya Admin |
| GET | `/pending` | Auth | Teslim edilmiş + ödenmemiş kiralamalar, hesaplanmış tutarlarla |
| GET | `/preview/{equipmentRentalId:guid}` | Auth | Tutar önizlemesi, hiçbir şey kaydetmez |
| POST | `/` | Auth | `CreateRequest`: `equipmentRentalId`, `userId?` (Admin başkası adına ödeyebilir) |

- `Payment.Response`: `id`, `paymentNumber`, `equipmentRentalId`, `userId`, `rentalFee`, `lateFee`, `totalAmount`, `discountAmount`, `paymentMethod`, `status`, `createdAt`, `paidAt?`
- `PendingResponse`: `equipmentRentalId`, `equipmentId`, `equipmentName`, `rentedAt`, `expectedReturnAt`, `releasedAt`, `isOverdue`, `overdueBy?`, `rentalFee`, `lateFee`, `discountAmount`, `totalAmount`, `userBalance`, `hasSufficientBalance`

`PaymentNumber` formatı: `PAY-{yyyyMMddHHmmssfff}-{8 karakter}`.

### Announcement — `/api/Announcement`

| Metot | Yol | Yetki |
|---|---|---|
| GET | `/` | Auth — `page`, `pageSize`, `categoryId`, `status`, `search`, `createdFrom`, `createdTo` |
| GET | `/{id:guid}` | Auth — Admin değilse yalnızca `Published` |
| POST | `/` | Auth — `Draft` olarak oluşur |
| PUT | `/{id}` | Auth — Admin veya duyuru sahibi |
| PATCH | `/{id:guid}/publish` | **Admin** |
| PATCH | `/{id:guid}/unpublish` | **Admin** |
| PATCH | `/{id:guid}/archive` | **Admin** |

- `CreateRequest`: `title` (5–100), `content` (≤500), `categoryId` (zorunlu)
- `UpdateRequest`: `id`, `title` (5–100), `content` (≤500, zorunlu), `categoryId`
- `Response`: `id`, `title`, `content`, `createdByUserId`, `createdByName`, `categoryId`, `categoryName`, `status`, `createdAt`, `updatedAt`

Admin olmayan kullanıcılar listede yalnızca `Published` kayıtları görür. Aynı başlık varsa sona sıra numarası eklenir. Arama `title` ve `content` üzerinde `ILIKE` ile yapılır.

### Category — `/api/Category`

| Metot | Yol | Yetki |
|---|---|---|
| GET | `/` | Auth — `includeUnactivated` (bool) |
| GET | `/{id}` | Auth — `includeUnactivated` |
| POST | `/` | **Admin** |
| PUT | `/` | **Admin** — `UpdateRequest` (gövde) |
| PATCH | `/{id}/activate` | **Admin** |
| PATCH | `/{id}/deactivate` | **Admin** |
| DELETE | `/` | **Admin** — `categoryId` (query), soft delete |

- `CreateRequest`: `name` (3–20), `type`
- `UpdateRequest`: `id`, `name` (3–20), `type`, `isActive`
- `Response`: `id`, `name`, `type`, `isActive`

İsim benzersizliği zorunlu (`NameExistsAsync`). Okumalar 5 dakika önbelleklenir.

### Subscription — `/api/Subscription`

| Metot | Yol | Açıklama |
|---|---|---|
| POST | `/` | `CreateRequest`: `targetType`, `categoryId?`, `equipmentId?`, `channel`. Aynı hedef için pasif kayıt varsa yeniden etkinleştirilir; aktif ve aynı kanaldaysa `DuplicateEntityException` |
| GET | `/` | Kendi **aktif** abonelikleri |
| DELETE | `/{id}` | Takibi bırakır (`IsActive = false`) |

`Response`: `id`, `targetType`, `categoryId?`, `categoryName?`, `equipmentId?`, `equipmentName?`, `channel`, `isActive`, `createdAt`

### Notification — `/api/Notification`

| Metot | Yol | Açıklama |
|---|---|---|
| GET | `/` | `page`, `pageSize`, `onlyUnread` |
| GET | `/unread-count` | Okunmamış sayısı (int) |
| PATCH | `/{id}/read` | Okundu işaretle (idempotent) |

`Response`: `id`, `type`, `channel`, `title`, `message`, `relatedEntityId?`, `isRead`, `readAt?`, `createdAt`

---

## Hata Yönetimi

`GlobalExceptionHandler` (`IExceptionHandler`) tüm exception'ları RFC 7807 `ProblemDetails`'a çevirir. 500'lerde iç detay **sızdırılmaz**, yalnızca "Beklenmeyen bir hata oluştu." döner ve `Error` seviyesinde loglanır; beklenen hatalar `Warning` loglanır.

| Exception | HTTP |
|---|---|
| `NotFoundException` | 404 |
| `ValidationException` | 400 (+ `errors` uzantısında alan hataları) |
| `UnauthorizedException`, `InvalidCredentialsException` | 401 |
| `ForbiddenException`, `NotResourceOwnerException`, `InsufficientEquipmentLevelException` | 403 |
| `ConflictException` ve türevleri | 409 |
| `OperationCanceledException` | 499 |
| Diğer | 500 |

409 türevleri: `EquipmentNotAvailableException`, `EquipmentNotPortableException`, `EquipmentNotRentedException`, `RentalLimitExceededException`, `InvalidStateTransitionException`, `InvalidCurrentPasswordException`, `DuplicateEntityException`, `InsufficientBalanceException`

---

## Bildirim Sistemi

Olay yayınlandığında `DomainEventPublisher` DI'dan ilgili handler'ları çözer ve sırayla çalıştırır. Handler hatası yakalanıp loglanır — bildirim hatası ana işlemi düşürmez.

| Olay | Tetikleyen | Handler | Alıcı |
|---|---|---|---|
| `AnnouncementPublishedEvent` | Duyuru yayınlama | `AnnouncementPublishedNotificationHandler` | Kategori aboneleri |
| `EquipmentRelasedEvent` | Ekipman iadesi | `EquipmentRelasedNotificationHandler` | Ekipman aboneleri (**iade eden kişi hariç**) |

`NotificationChannelFactory`, aboneliğin `Channel` değerine göre kanalı seçer:

- **`InAppNotificationChannel`** — `Notifications` tablosuna kayıt ekler, kaydetme işini çağırana bırakır.
- **`SmtpEmailChannel`** — `SmtpService` üzerinden MailKit ile e-posta gönderir. Adres yoksa `InvalidOperationException` fırlatır (sessiz kayıp olmasın diye).

`SmtpService`, `SmtpOptions`'tan `MimeMessage` kurar. Güvenlik modu:

| `UseSsl` | Mod | Tipik port |
|---|---|---|
| `true` | `SslOnConnect` | 465 |
| `false` | `StartTlsWhenAvailable` | 587 |
| boş | `Auto` (porta göre karar verir) | — |

Bağlantı zaman aşımı 15 saniye.

---

## Zaman / UTC Politikası

Tek kural: **her tarih UTC**.

- Tüm `DateTime` kolonları PostgreSQL `timestamp with time zone`.
- `AppDbContext`'te global bir `ValueConverter` her `DateTime` ve `DateTime?` alanını yazarken `ToUtc()`, okurken `Kind = Utc` yapar.
- `DateTimeExtensions.ToUtc()` sözleşmesi: `Kind = Utc` → dokunulmaz, `Local` → çevrilir, **`Unspecified` → zaten UTC kabul edilir** (sunucu yerel saatine göre kaydırılmaz). Doğrulama ve servis katmanı aynı kuralı kullanır.
- Kod tabanında zaman kaynağı olarak yalnızca `DateTime.UtcNow` kullanılır.
- API JSON'da tarihler `Z` ekiyle döner; istemci gösterimde yerel saate çevirir.

> **Not:** Veritabanında saat, girdiğinizden farklı görünüyorsa bu bir hata değildir — `timestamptz` anı UTC saklar. 10:00 (UTC+3) girdiyseniz DB'de `07:00+00` görürsünüz; ikisi aynı andır.

---

## Yapılandırma

`appsettings.Development.json`:

```json
{
  "Jwt": {
    "Issuer": "AnnouncementApp",
    "Audience": "AnnouncementAppUsers",
    "Key": "<en az 32 karakter>",
    "ExpiresMinutes": 60
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=MakerspacePlatformDb;Username=postgres;Password=postgres;"
  },
  "SeedAdmin": {
    "Email": "admin@example.com",
    "Password": "Admin123!"
  },
  "Smtp": {
    "Server": "localhost",
    "Port": 1025,
    "Username": null,
    "Password": null,
    "UseSsl": false,
    "From": "noreply@makerspace.local",
    "DisplayName": "Makerspace Fablab"
  }
}
```

**Gizli değerleri dosyaya yazmayın.** SMTP şifresi ve JWT anahtarı için:

```bash
dotnet user-secrets set "Smtp:Password" "<sifre>"
```

veya ortam değişkeni: `Smtp__Password`, `Jwt__Key`.

Uygulama açılışta `SeedData.EnsureAdminAsync` ile admin yoksa oluşturur.

---

## Kurulum ve Çalıştırma

**Gereksinimler:** .NET 10 SDK, Docker, Node.js 18+

### 1. PostgreSQL

```bash
docker run -d --name makerspace-postgres --restart unless-stopped -e POSTGRES_PASSWORD=postgres -p 5432:5432 postgres:16-alpine
```

### 2. Mailpit (geliştirme SMTP yakalayıcısı)

```bash
docker run -d --name mailpit --restart unless-stopped -p 1025:1025 -p 8025:8025 axllent/mailpit
```

Gönderilen mailler http://localhost:8025 adresinde görüntülenir; kimseye gerçek mail gitmez.

### 3. Migration

```bash
dotnet ef database update
```

### 4. API

```bash
dotnet run
```

### 5. Frontend

```bash
cd frontend && npm install && npm run dev
```

Vite `:3000`'de açılır ve `/api` isteklerini `http://localhost:5281`'e proxy'ler.

### Faydalı komutlar

```bash
dotnet build
```

```bash
dotnet list package --vulnerable --include-transitive
```

```bash
dotnet ef migrations has-pending-model-changes
```

```bash
cd frontend && npx tsc -b --noEmit
```

---

## Frontend

React SPA. `AuthContext` token'ı `localStorage`'da tutar, açılışta `/api/Users/me` ile kullanıcıyı doğrular; Axios interceptor'ı isteklere `Authorization` başlığı ekler ve 401'de oturumu temizleyip `/login`'e yönlendirir.

| Rota | Sayfa | Erişim |
|---|---|---|
| `/login`, `/register` | Giriş / kayıt | Anonim |
| `/dashboard` | Özet + aktivite takvimi | Auth |
| `/equipment` | Ekipman listesi, kiralama/rezervasyon/ileri tarihli işlem | Auth |
| `/my-equipment` | Kendi kiralamaları, iade | Auth |
| `/announcements` | Duyurular | Auth |
| `/payments` | Ödemeler ve bekleyen borçlar | Auth |
| `/notifications` | Bildirimler | Auth |
| `/subscriptions` | Abonelikler | Auth |
| `/categories` | Kategori yönetimi | **Admin** |
| `/users` | Kullanıcı yönetimi | **Admin** |
| `/profile` | Profil, şifre, bakiye | Auth |

Tarihler her yerde `new Date(iso).toLocaleString('tr-TR')` ile tarayıcının yerel saatine çevrilerek gösterilir. İleri tarihli işlemde kullanıcının seçtiği bitiş tarihi `end - start` ile .NET TimeSpan biçimine çevrilip gönderilir.

---

## Bilinen Eksikler ve Teknik Borç

Şeffaflık için: aşağıdakiler bilinen ve henüz kapatılmamış maddelerdir.

### Yüksek

- **`POST /api/Users/me/add-balance` yetkilendirilmemiş.** Giriş yapmış herhangi bir kullanıcı kendine sınırsız bakiye yükleyebilir. `[Authorize(Roles = "Admin")]` gerekiyor. Frontend profil sayfası bu ucu kullandığı için o akış da ele alınmalı.
- **Etkinleşmemiş rezervasyon iade edilemiyor.** `AvailableState.AvailableAsync` koşulsuz hata fırlattığı için, ekipman durumu hiç değişmemiş bir rezervasyonun sahibi iade edemez → kotası kalıcı dolar, ücret de tahsil edilemez.
- **Rezervasyon başlangıcında etkinleştirme mekanizması yok.** Saati gelen rezervasyon ekipmanı `Rented`/`Reserved` yapmaz; projede hiç `BackgroundService`/`IHostedService` bulunmuyor.
- **API dokümantasyonu kapalı.** `app.MapOpenApi()` çağrılıyor ama `builder.Services.AddOpenApi()` yok, dolayısıyla `/openapi/v1.json` ve Scalar arayüzü çalışmaz. `BearerSecuritySchemeTransformer` yazılmış ama kayıtlı değil.

### Orta

- **Çifte rezervasyon yarışı.** Çakışma kontrolü ile insert arasında kilit veya DB kısıtı yok. Exclusion constraint (`btree_gist`) veya serializable transaction gerekiyor; `UnitOfWork`'te transaction API'si bulunmuyor.
- **`IsOverdue`/`OverdueBy` yalnızca ödeme anında yazılıyor.** Ödenmemiş geciken kiralamalarda gecikme raporlanamaz.
- **`EquipmentRental`'da iptal kavramı yok.** Bakım iptalinde `IsPaid = true` işaretlemesi bir ödün; doğrusu `Cancelled`/`IsBillable` alanı (migration gerektirir).
- **Önbellek EF varlıklarını saklıyor.** `CachedCategoryRepository` `Category` nesnelerini `IMemoryCache`'e koyuyor, `CategoryService` de onları yerinde değiştiriyor. Varlık yerine DTO önbelleklenmeli.
- **E-posta gönderimi HTTP isteğini bloklar.** Abone başına ayrı SMTP turu açılır; 15 saniyelik timeout felaketi önler ama kuyruk mekanizması gerekiyor.

### Düşük

- `Program.cs`'te `IMembershipStrategy` fabrikası `.Result` ile senkron DB çağrısı yapıyor (thread-pool açlığı riski). `IMembershipStrategyFactory` desenine geçilmeli — `PaymentService` bunu doğru yapıyor.
- Ödeme iadesi (refund) altyapısı şemada var (`PaymentStatus.Refunded`, `RefundedAt`) ama hiçbir uç yazmıyor.
- Şifre değişimi mevcut JWT'leri geçersiz kılmıyor (token sürümü / `SecurityStamp` yok).
- Üyelik satın alma akışı yok; `CalculateMembershipCost()` hiçbir yerden çağrılmıyor, `User.Status` hiç güncellenmiyor — pratikte tüm kullanıcılar `Free` kademesinde.
- Duyuru başlığı tekilleştirme (`"{Title} {n}"`) yarışa açık ve tekrar üretebilir.
- Ölü kod: `Dtos/Event`, `Validators/EventValidator` (karşılığında servis/controller yok), `Policies/PaymentPolicies/` boş klasör, `EquipmentNotAvailableException`, iki serviste kullanılmayan `GetStateFor()` metotları.
- **Test yok.** Projede hiçbir test projesi bulunmuyor.
