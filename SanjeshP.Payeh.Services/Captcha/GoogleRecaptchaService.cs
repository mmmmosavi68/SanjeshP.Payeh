using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SanjeshP.Payeh.Common;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace SanjeshP.Payeh.Services.Captcha
{
    /// <summary>
    /// نمونه‌ی ارائه‌دهنده‌ی واقعی کپچا: Google reCAPTCHA.
    /// فعال‌سازی: CaptchaSettings:Enabled=true و مقداردهی SecretKey در تنظیمات.
    /// برای ارائه‌دهنده‌ی دیگر (مثلاً hCaptcha) همین الگو را با آدرس و پارامترهای آن پیاده کنید.
    /// </summary>
    public class GoogleRecaptchaService : ICaptchaService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly CaptchaSettings _settings;
        private readonly ILogger<GoogleRecaptchaService> _logger;

        public GoogleRecaptchaService(
            IHttpClientFactory httpClientFactory,
            IOptions<SiteSettings> siteSettings,
            ILogger<GoogleRecaptchaService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _settings = siteSettings.Value.CaptchaSettings;
            _logger = logger;
        }

        /// <inheritdoc />
        public bool IsEnabled => true;

        /// <inheritdoc />
        public async Task<bool> ValidateAsync(string? token, string? remoteIp, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            var client = _httpClientFactory.CreateClient(nameof(GoogleRecaptchaService));

            var form = new Dictionary<string, string>
            {
                ["secret"] = _settings.SecretKey,
                ["response"] = token
            };
            if (!string.IsNullOrWhiteSpace(remoteIp))
                form["remoteip"] = remoteIp;

            try
            {
                var response = await client.PostAsync(_settings.VerifyUrl, new FormUrlEncodedContent(form), cancellationToken);
                if (!response.IsSuccessStatusCode)
                    return false;

                var result = await response.Content.ReadFromJsonAsync<RecaptchaResponse>(cancellationToken);
                return result?.Success == true;
            }
            catch (Exception ex)
            {
                // خطای شبکه با ارائه‌دهنده‌ی کپچا نباید باعث خطای ۵۰۰ شود؛ صرفاً نامعتبر تلقی می‌شود.
                _logger.LogWarning(ex, "خطا در ارتباط با سرویس کپچا");
                return false;
            }
        }

        /// <summary>پاسخ سرویس siteverify گوگل.</summary>
        private sealed class RecaptchaResponse
        {
            [JsonPropertyName("success")]
            public bool Success { get; set; }

            [JsonPropertyName("error-codes")]
            public string[]? ErrorCodes { get; set; }
        }
    }
}