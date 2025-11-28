# Nobat API

API چندلایه با Clean Architecture برای سیستم نوبت‌دهی

## ویژگی‌ها

- ✅ Clean Architecture (Domain, Application, Infrastructure, API)
- ✅ SQL Server با Code First
- ✅ Swagger با پشتیبانی از Versioning
- ✅ IP Rate Limiting
- ✅ Redis Cache با قابلیت غیرفعال کردن
- ✅ AutoMapper
- ✅ Minimal MVC
- ✅ JWT Authentication
- ✅ Custom API Versioning
- ✅ Role Policy
- ✅ Sieve (فیلتر و مرتب‌سازی)

## ساختار پروژه

```
api/
├── src/
│   ├── Nobat.API/              # لایه API (Controllers, Startup)
│   ├── Nobat.Application/      # لایه Application (Services, DTOs, Mappings)
│   ├── Nobat.Domain/           # لایه Domain (Entities, Interfaces)
│   └── Nobat.Infrastructure/   # لایه Infrastructure (Data, Repositories, Services)
```

## نصب و راه‌اندازی

### پیش‌نیازها

- .NET 8.0 SDK
- SQL Server
- Redis (اختیاری - می‌تواند غیرفعال شود)

### تنظیمات

1. فایل `appsettings.json` را ویرایش کنید و Connection String را تنظیم کنید:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=NobatDb;Trusted_Connection=True;TrustServerCertificate=True;",
    "Redis": "localhost:6379"
  }
}
```

2. برای غیرفعال کردن Redis Cache:

```json
{
  "Cache": {
    "Redis": {
      "Enabled": false
    }
  }
}
```

3. JWT Secret Key را تغییر دهید:

```json
{
  "Jwt": {
    "SecretKey": "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!"
  }
}
```

### اجرای Migration

```bash
cd src/Nobat.API
dotnet ef migrations add InitialCreate --project ../Nobat.Infrastructure
dotnet ef database update --project ../Nobat.Infrastructure
```

### اجرای پروژه

```bash
cd src/Nobat.API
dotnet run
```

API در آدرس `https://localhost:5001` و Swagger در `https://localhost:5001/swagger` در دسترس خواهد بود.

## استفاده از API

### احراز هویت

1. ثبت‌نام:
```
POST /api/v1/auth/register
{
  "username": "testuser",
  "email": "test@example.com",
  "password": "password123",
  "firstName": "Test",
  "lastName": "User"
}
```

2. ورود:
```
POST /api/v1/auth/login
{
  "username": "testuser",
  "password": "password123"
}
```

3. استفاده از Token:
```
Authorization: Bearer {token}
```

### استفاده از Sieve

```
GET /api/v1/shifts?page=1&pageSize=10&filters=name@=شیفت&sorts=name
```

## تنظیمات Rate Limiting

در `appsettings.json` می‌توانید محدودیت‌های Rate Limiting را تنظیم کنید:

```json
{
  "IpRateLimiting": {
    "GeneralRules": [
      {
        "Endpoint": "*",
        "Period": "1m",
        "Limit": 60
      }
    ]
  }
}
```

