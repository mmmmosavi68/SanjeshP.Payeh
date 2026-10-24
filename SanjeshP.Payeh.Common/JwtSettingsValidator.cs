using System.Text;

namespace SanjeshP.Payeh.Common
{
    /// <summary>جلوگیری از بالا آمدن برنامه با کلید JWT خالی، کوتاه یا placeholder.</summary>
    public static class JwtSettingsValidator
    {
        public static void Validate(JwtSettings s, bool isDevelopment)
        {
            var key = s.SecretKey ?? string.Empty;

            if (Encoding.UTF8.GetByteCount(key) < 32 || key.StartsWith("__"))
                throw new InvalidOperationException(
                    "SiteSettings:JwtSettings:SecretKey مقداردهی نشده یا کوتاه‌تر از ۳۲ بایت است. " +
                    "با User Secrets یا متغیر محیطی SiteSettings__JwtSettings__SecretKey ست کنید.");

            if (!isDevelopment && key.StartsWith("DEV-ONLY", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("کلید توسعه (DEV-ONLY) در محیط غیر Development مجاز نیست.");

            if (string.IsNullOrWhiteSpace(s.Issuer) || string.IsNullOrWhiteSpace(s.Audience))
                throw new InvalidOperationException("JwtSettings: Issuer و Audience الزامی‌اند.");

            if (s.ExpirationMinutes is < 1 or > 60)
                throw new InvalidOperationException("JwtSettings:ExpirationMinutes باید بین ۱ تا ۶۰ باشد.");
        }
    }
}