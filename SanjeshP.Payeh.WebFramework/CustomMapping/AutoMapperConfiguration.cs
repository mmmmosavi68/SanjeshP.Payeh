using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace SanjeshP.Payeh.WebFramework.CustomMapping
{
    /// <summary>
    /// راه‌اندازی AutoMapper.
    /// ⚠️ نکته‌ی لایسنس: از نسخه‌ی 15 به بعد AutoMapper تجاری شده و به LicenseKey نیاز دارد
    /// (نسخه‌ی فعلی پروژه: 16.2.0). سه گزینه دارید — تصمیم نهایی با شماست:
    /// ۱) ادامه با 16.x و دریافت کلید (تجاری یا Community رایگان برای پروژه‌های واجد شرایط) از automapper.io
    ///    و قرار دادن آن در تنظیمات AutoMapper:LicenseKey (پیشنهاد: از User-Secrets/متغیر محیطی).
    /// ۲) تنزل به آخرین نسخه‌ی MIT یعنی 14.0.0 (بدون کلید؛ کد فعلی با آن هم سازگار است).
    /// ۳) جایگزینی با Mapster (MIT، سریع‌تر، پشتیبانی از ProjectTo) — نمونه‌ی رجیستر آن در گزارش آمده است.
    /// این پیاده‌سازی گزینه‌ی ۱ را مطابق درخواست شما فعال می‌کند و برای گزینه‌ی ۲ هم آماده است.
    /// </summary>
    public static class AutoMapperConfiguration
    {
        public static void InitializeAutoMapper(this IServiceCollection services, IConfiguration configuration, params Assembly[] assemblies)
        {
            // اگر خالی باشد و نسخه‌ی 14 استفاده شود مشکلی نیست؛ در 16 باید مقدار داشته باشد.
            var licenseKey = configuration["AutoMapper:LicenseKey"];

            services.AddAutoMapper(cfg =>
            {
                if (!string.IsNullOrWhiteSpace(licenseKey))
                    cfg.LicenseKey = licenseKey;

                // ثبت خودکار همه‌ی Profileهای اسمبلی‌های معرفی‌شده (WebFramework و Api)
                cfg.AddMaps(assemblies);
            }, assemblies);
        }
    }
}