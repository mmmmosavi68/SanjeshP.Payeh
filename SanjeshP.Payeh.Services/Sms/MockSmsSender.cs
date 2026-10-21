using Microsoft.Extensions.Logging;

namespace SanjeshP.Payeh.Services.Sms
{
    /// <summary>
    /// پیاده‌سازی نمونه (Mock) برای محیط توسعه: پیامک واقعاً ارسال نمی‌شود
    /// و متن فقط در لاگ ثبت می‌گردد. ارائه‌دهنده‌ی واقعی را بعداً با
    /// پیاده‌سازی ISmsSender جایگزین کنید (SmsSettings.Provider).
    /// </summary>
    public class MockSmsSender : ISmsSender
    {
        private readonly ILogger<MockSmsSender> _logger;

        public MockSmsSender(ILogger<MockSmsSender> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public Task<bool> SendAsync(string phoneNumber, string message, CancellationToken cancellationToken)
        {
            // در توسعه، متن پیامک (شامل کد OTP) در لاگ قابل مشاهده است.
            _logger.LogInformation("[MockSms] ارسال پیامک به {PhoneNumber}: {Message}", phoneNumber, message);
            return Task.FromResult(true);
        }
    }
}