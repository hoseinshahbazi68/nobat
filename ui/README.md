# پنل نوبت دهی

پروژه Angular برای مدیریت سیستم نوبت دهی

## ویژگی‌ها

- ✅ فرم ثبت نام و لاگین
- ✅ مدیریت نقش کاربران
- ✅ مدیریت کاربران
- ✅ مدیریت روزهای تعطیل
- ✅ مدیریت شیفت‌ها
- ✅ مدیریت خدمات
- ✅ مدیریت پزشکان
- ✅ مدیریت بیمه‌ها
- ✅ مدیریت تعرفه خدمات براساس بیمه و خدمت
- ✅ برنامه حضور پزشکان
- ✅ تولید زمان‌بندی پزشک

## نصب و راه‌اندازی

1. نصب وابستگی‌ها:
```bash
npm install
```

2. اجرای پروژه:
```bash
npm start
```

3. باز کردن مرورگر:
```
http://localhost:4200
```

## ساختار پروژه

```
src/
├── app/
│   ├── components/
│   │   ├── auth/          # کامپوننت‌های احراز هویت
│   │   ├── layout/        # کامپوننت‌های Layout
│   │   └── dashboard/     # داشبورد
│   └── pages/             # صفحات مدیریتی
│       ├── user-roles/
│       ├── users/
│       ├── holidays/
│       ├── shifts/
│       ├── services/
│       ├── doctors/
│       ├── insurances/
│       ├── service-tariffs/
│       ├── doctor-schedules/
│       └── generate-schedule/
```

## تکنولوژی‌ها

- Angular 15
- Angular Material
- TypeScript
- RxJS

