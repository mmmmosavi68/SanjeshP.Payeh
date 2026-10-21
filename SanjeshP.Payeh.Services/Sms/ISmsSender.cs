namespace SanjeshP.Payeh.Services.Sms
{
    /// <summary>
    /// انتزاع ارسال پیامک. برای اتصال پنل واقعی (کاوه‌نگار، ملی‌پیامک و ...)
    /// فقط یک پیاده‌سازی جدید از این اینترفیس بسازید و در تنظیمات/DI جایگزین کنید؛
    /// هیچ تغییری در کنترلرها و سرویس OTP لازم نیست.
    /// </summary>
    public interface ISmsSender
    {
        /// <summary>ارسال پیامک متنی به شماره‌ی مقصد.</summary>
        /// <param name="phoneNumber">شماره موبایل مقصد (فرمت 09xxxxxxxxx).</param>
        /// <param name="message">متن پیامک.</param>
        /// <param name="cancellationToken">توکن لغو عملیات.</param>
        /// <returns>true در صورت ارسال موفق.</returns>
        Task<bool> SendAsync(string phoneNumber, string message, CancellationToken cancellationToken);
    }
}