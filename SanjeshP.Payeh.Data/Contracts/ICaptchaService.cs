namespace SanjeshP.Payeh.Services.Captcha
{
    /// <summary>
    /// انتزاع اعتبارسنجی کپچا. کپچا از طریق CaptchaSettings:Enabled روشن/خاموش می‌شود
    /// و ارائه‌دهنده با تعویض پیاده‌سازی این اینترفیس قابل تغییر است
    /// (نمونه‌ی واقعی: GoogleRecaptchaService).
    /// </summary>
    public interface ICaptchaService
    {
        /// <summary>آیا کپچا در تنظیمات فعال است؟</summary>
        bool IsEnabled { get; }

        /// <summary>اعتبارسنجی توکن کپچای ارسالی کلاینت.</summary>
        /// <param name="token">توکن دریافتی از ویجت کپچا در کلاینت.</param>
        /// <param name="remoteIp">آی‌پی کاربر (اختیاری، برای دقت بیشتر ارائه‌دهنده).</param>
        /// <param name="cancellationToken">توکن لغو عملیات.</param>
        /// <returns>true یعنی کپچا معتبر است (یا کپچا غیرفعال است).</returns>
        Task<bool> ValidateAsync(string? token, string? remoteIp, CancellationToken cancellationToken);
    }
}