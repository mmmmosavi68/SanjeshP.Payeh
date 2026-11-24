# CHANGES (branch `refactor/professional-cleanup`)

## معماری
- سرویس‌های Business (`User/Otp/Group/Codes/Permission/Captcha/UserManagement/AuditLog*/Role`) از Data به `Services` (Contracts/ و Implementations/) منتقل شدند. Data فقط DbContext + Repository + `IUnitOfWork`.
- Services مستقیم به `ApplicationDbContext` دسترسی ندارد؛ همه‌چیز از `IUnitOfWork.Repository<T>()` / Repositoryها.
- `UserToken`, `UserVerificationCode`, `UnprocessableEntityException`, `CrudController`, AutoMapper حذف شدند.

## Mapping
- AutoMapper حذف شد (نسخه‌ی رایگان 14 آسیب‌پذیری شناخته‌شده دارد، 15+ لایسنس می‌خواهد). جایگزین: **Mapperly** (Source Generator، بدون لایسنس).
- `IApiMapper` (ثبت‌شده در DI): Code, University, Exam, ExamSession, Menu, User. اعضای نگاشت‌نشده = خطای Build.
- لیست‌های خواندنی با `ProjectToDto` (بدون Include، بدون N+1) و نگاشت Null-safe روی Navigation/Codes.

## Codes
- DTOها علاوه بر `...Id` فیلدهای `...PrimaryCode/...SubCode` را می‌پذیرند و برمی‌گردانند (ExamType, ExamMode, UniversityType, Gender, FieldType, Parent). فیلدهای Id حفظ شدند.
- `ICodesService.ResolveIdAsync` کد را به Id تبدیل و فقط با (PrimaryCode, SubCode) اعتبارسنجی می‌کند. فیلتر `...PrimaryCode/...SubCode` به GetAll اضافه شد.
- ایندکس یکتای فیلترشده `UX_Codes_PrimaryCode_SubCode`. `Code.json` به JSON معتبر نرمال و یکتا شد (ردیف تکراری در فایل موجود پیدا نشد؛ تکراری‌های دیتابیس در Migration پاک می‌شوند).

## پیام‌ها / ErrorCode
- `ApiMessages` (+`ApiMessages.Generated.cs`): ~۲۳۰ پیام Literal با `ErrorCode` پایدار (`AREA_NNN`). پاسخ خطا فیلد افزودنیِ `errorCode` دارد.
- خطای Mapping/ناشناخته از Global Exception Handler با `COMMON_MAPPING` / `COMMON_500` برمی‌گردد.

## Pagination
- `page` / `pageSize` اختیاری روی GET: Codes, Universities, Exams, ExamSessions, Menus. بدون پارامتر = رفتار قبلی. هدرها: `X-Total-Count, X-Page, X-Page-Size, X-Total-Pages`.

## امنیت (IDOR / Privilege escalation)
- تغییر دسترسی منو/دانشگاه یا عضویت گروه برای خود کاربر ممنوع (`AUTHZ_001`).
- اعطای منو (کاربر/گروه) فقط در محدوده‌ی منوهای خود اعطاکننده (`AUTHZ_002`).
- مالکیت ثبت‌نام‌ها و دسترسی دانشگاه قبلاً در Service بررسی می‌شد و حفظ شد.

## Migrationها (اجرا نشده‌اند)
1. `20261124180549_CodesUniquePrimarySubCode` – پاک‌سازی تکراری‌ها + Index یکتا.
2. `RemoveLegacyUserTokenTables` – حذف جداول UserTokens/UserVerificationCodes.

## Breaking Changeها
- هیچ تغییر شکستنده‌ی JSON نیست (فقط افزودنی). `GET /Codes` و ... همان لیست را برمی‌گردانند.
- `ParentId` در Create/UpdateCodeDto دیگر `[Required]` نیست؛ نبودن والد → 400 با `LOOKUP_007`.
- اعطای منو/تغییر دسترسی خود → 403 (رفتار امنیتی جدید).

## تست و اجرا
- `SanjeshP.Payeh.Tests`: Integration (Login/Refresh/Logout، دسترسی منو) با EF InMemory بدون SQL Server + تست Mapping/Projection.
- `dotnet build SanjeshP.Payeh.slnx && dotnet test SanjeshP.Payeh.Tests`
- اعمال Migration: `dotnet ef database update --project SanjeshP.Payeh.Data --startup-project SanjeshP.Payeh.Api`

## باقی‌مانده
- Warningهای Nullable معنایی (CS8602/8603/8625/8604) هنوز هست؛ Nav/DTO/Entityها اصلاح شدند.
- نگاشت دستی ExamRegistration/Capacity/Groups هنوز در کنترلر است (فیلدهای Code اضافه شده).
