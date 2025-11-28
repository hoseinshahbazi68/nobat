# راهنمای نصب و راه‌اندازی API

## پیش‌نیازها

- .NET 8.0 SDK
- SQL Server (یا SQL Server Express)
- Redis (اختیاری - می‌تواند غیرفعال شود)
- Visual Studio 2022 یا VS Code

## مراحل نصب

### 1. تنظیم Connection String

فایل `appsettings.json` را باز کرده و Connection String را تنظیم کنید:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=NobatDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

### 2. تنظیم JWT Secret Key

یک Secret Key قوی برای JWT ایجاد کنید (حداقل 32 کاراکتر):

```json
{
  "Jwt": {
    "SecretKey": "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!"
  }
}
```

### 3. تنظیم Redis (اختیاری)

اگر می‌خواهید Redis را غیرفعال کنید:

```json
{
  "Cache": {
    "Redis": {
      "Enabled": false
    }
  }
}
```

اگر Redis فعال است، Connection String را تنظیم کنید:

```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  }
}
```

### 4. نصب Migration

```bash
cd src/Nobat.API
dotnet ef migrations add InitialCreate --project ../Nobat.Infrastructure --startup-project .
dotnet ef database update --project ../Nobat.Infrastructure --startup-project .
```

### 5. اجرای پروژه

```bash
cd src/Nobat.API
dotnet run
```

یا از Visual Studio:
- پروژه `Nobat.API` را به عنوان Startup Project تنظیم کنید
- F5 را بزنید

## دسترسی به API

- API: `https://localhost:5001`
- Swagger UI: `https://localhost:5001/swagger`

## استفاده از API

### 1. ثبت‌نام کاربر جدید

```http
POST /api/v1/auth/register
Content-Type: application/json

{
  "username": "testuser",
  "email": "test@example.com",
  "password": "password123",
  "firstName": "Test",
  "lastName": "User",
  "phoneNumber": "09123456789"
}
```

### 2. ورود

```http
POST /api/v1/auth/login
Content-Type: application/json

{
  "username": "testuser",
  "password": "password123"
}
```

پاسخ شامل Token JWT خواهد بود:

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "username": "testuser",
    "email": "test@example.com",
    "roles": ["User"]
  },
  "expiresAt": "2024-01-01T12:00:00Z"
}
```

### 3. استفاده از Token

در Header درخواست‌ها:

```http
Authorization: Bearer {token}
```

### 4. دریافت لیست شیفت‌ها با Sieve

```http
GET /api/v1/shifts?page=1&pageSize=10&filters=name@=شیفت&sorts=name
Authorization: Bearer {token}
```

پارامترهای Sieve:
- `page`: شماره صفحه
- `pageSize`: تعداد آیتم در هر صفحه
- `filters`: فیلترها (مثال: `name@=شیفت`)
- `sorts`: مرتب‌سازی (مثال: `name` یا `-name` برای نزولی)

### 5. ایجاد شیفت جدید (نیاز به نقش Admin)

```http
POST /api/v1/shifts
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "شیفت صبح",
  "startTime": "08:00",
  "endTime": "16:00",
  "description": "شیفت کاری صبح"
}
```

## تنظیمات Rate Limiting

در `appsettings.json`:

```json
{
  "IpRateLimiting": {
    "GeneralRules": [
      {
        "Endpoint": "*",
        "Period": "1m",
        "Limit": 60
      },
      {
        "Endpoint": "*",
        "Period": "1h",
        "Limit": 1000
      }
    ]
  }
}
```

## ساختار پروژه

```
api/
├── src/
│   ├── Nobat.API/              # لایه API
│   │   ├── Controllers/         # کنترلرها
│   │   ├── Middleware/          # Middleware ها
│   │   ├── Extensions/          # Extension Methods
│   │   └── Program.cs           # Startup
│   ├── Nobat.Application/       # لایه Application
│   │   ├── DTOs/                # Data Transfer Objects
│   │   ├── Interfaces/          # Interfaces سرویس‌ها
│   │   ├── Services/            # سرویس‌های Business Logic
│   │   └── Mappings/            # AutoMapper Profiles
│   ├── Nobat.Domain/            # لایه Domain
│   │   ├── Entities/            # موجودیت‌ها
│   │   ├── Interfaces/          # Repository Interfaces
│   │   └── Common/              # کلاس‌های مشترک
│   └── Nobat.Infrastructure/    # لایه Infrastructure
│       ├── Data/                # DbContext و Configurations
│       ├── Repositories/        # پیاده‌سازی Repository
│       └── Services/            # سرویس‌های Infrastructure
```

## ویژگی‌های پیاده‌سازی شده

✅ Clean Architecture (چندلایه)
✅ SQL Server با Code First
✅ Swagger با Versioning
✅ IP Rate Limiting
✅ Redis Cache (با قابلیت غیرفعال کردن)
✅ AutoMapper
✅ Minimal MVC
✅ JWT Authentication
✅ Custom API Versioning
✅ Role Policy (Admin, User)
✅ Sieve (فیلتر و مرتب‌سازی)
✅ Exception Handling Middleware
✅ Soft Delete
✅ Logging با Serilog

## نکات مهم

1. **امنیت**: در Production حتماً JWT Secret Key را تغییر دهید
2. **Database**: Migration ها به صورت خودکار اجرا می‌شوند
3. **Cache**: Redis می‌تواند غیرفعال شود و از In-Memory Cache استفاده می‌کند
4. **Rate Limiting**: برای جلوگیری از سوء استفاده تنظیم شده است
5. **Versioning**: API از Versioning پشتیبانی می‌کند (v1, v2, ...)

