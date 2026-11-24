namespace SanjeshP.Payeh.Services.Contracts
{
    /// <summary>اطلاعات لازم برای نمایش کپچای تصویری در کلاینت.</summary>
    public sealed record CaptchaChallenge(string CaptchaId, string ImageBase64, string ContentType, int ExpiresInSeconds);

    /// <summary>قرارداد عمومی اعتبارسنجی کپچا.</summary>
    public interface ICaptchaService
    {
        bool IsEnabled { get; }
        /// <summary>توکن مرکب شناسه.پاسخ را اعتبارسنجی می‌کند.</summary>
        Task<bool> ValidateAsync(string? token, string? remoteIp, CancellationToken cancellationToken);
    }

    /// <summary>قرارداد تولید تصویر کپچای محلی.</summary>
    public interface ICaptchaImageService
    {
        Task<CaptchaChallenge> CreateAsync(string? remoteIp, CancellationToken cancellationToken);
    }
}
