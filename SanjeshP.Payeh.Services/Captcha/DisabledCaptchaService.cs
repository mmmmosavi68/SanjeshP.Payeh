namespace SanjeshP.Payeh.Services.Captcha
{
    /// <summary>
    /// پیاده‌سازی «کپچای خاموش»: وقتی CaptchaSettings:Enabled=false باشد
    /// این کلاس در DI ثبت می‌شود و همیشه معتبر برمی‌گرداند.
    /// به این ترتیب کنترلر هیچ if پراکنده‌ای برای وضعیت کپچا ندارد.
    /// </summary>
    public class DisabledCaptchaService : ICaptchaService
    {
        /// <inheritdoc />
        public bool IsEnabled => false;

        /// <inheritdoc />
        public Task<bool> ValidateAsync(string? token, string? remoteIp, CancellationToken cancellationToken)
            => Task.FromResult(true);
    }
}