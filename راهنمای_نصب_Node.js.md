# راهنمای آپدیت Node.js برای اجرای پروژه Angular

## مشکل
پروژه Angular 17 شما نیاز به Node.js نسخه 18.13 یا بالاتر دارد، اما نسخه فعلی نصب شده 16.20.2 است.

## راه‌حل: آپدیت Node.js

### روش 1: دانلود و نصب مستقیم (پیشنهادی)

1. به آدرس زیر بروید:
   https://nodejs.org/

2. نسخه LTS (Long Term Support) را دانلود کنید (حداقل نسخه 18.13)

3. فایل نصب‌کننده را اجرا کرده و مراحل نصب را طی کنید

4. پس از نصب، ترمینال را بسته و دوباره باز کنید

5. نسخه جدید را بررسی کنید:
   ```bash
   node --version
   ```

6. سپس برنامه را اجرا کنید:
   ```bash
   cd ui
   npm install
   npm start
   ```

### روش 2: استفاده از nvm-windows (برای مدیریت چند نسخه)

1. nvm-windows را از آدرس زیر دانلود کنید:
   https://github.com/coreybutler/nvm-windows/releases

2. فایل `nvm-setup.exe` را نصب کنید

3. پس از نصب، ترمینال را بسته و دوباره باز کنید

4. نصب Node.js 18:
   ```bash
   nvm install 18.20.0
   nvm use 18.20.0
   ```

5. بررسی نسخه:
   ```bash
   node --version
   ```

6. سپس برنامه را اجرا کنید:
   ```bash
   cd ui
   npm install
   npm start
   ```

## پس از آپدیت Node.js

پس از آپدیت Node.js، ممکن است نیاز باشد node_modules را دوباره نصب کنید:

```bash
cd ui
rm -rf node_modules package-lock.json
npm install
npm start
```

## بررسی نسخه‌های مورد نیاز

- **Node.js**: حداقل 18.13 (پیشنهادی: 18.20.0 یا 20.x)
- **npm**: با نصب Node.js به‌طور خودکار نصب می‌شود

