namespace SanjeshP.Payeh.Services.Otp
{
    /// <param name="DevCode">فقط در توسعه (OtpSettings.ReturnCodeInResponse=true) مقدار دارد.</param>
    public record OtpRequestResult(string? DevCode);

    /// <summary>نتیجه‌ی صدور کد مرحله‌ی دوم ورود با رمز عبور.</summary>
    public record LoginChallengeResult(Guid ChallengeId, string MaskedMobile, string? DevCode, int ResendAfterSeconds);

    /// <summary>
    /// سرویس کد یک‌بارمصرف پیامکی: ذخیره‌ی Hash، انقضا، سقف تلاش، فاصله‌ی ارسال مجدد و یک‌بارمصرف بودن.
    /// دو سناریو: ورود با موبایل (Login) و مرحله‌ی دوم ورود با رمز (PasswordLogin، متصل به challengeId).
    /// </summary>
    public interface IOtpService
    {
        Task<OtpRequestResult> RequestLoginCodeAsync(string mobileNumber, CancellationToken cancellationToken);

        Task<Entities.User.User> VerifyLoginCodeAsync(string mobileNumber, string code, CancellationToken cancellationToken);

        /// <summary>بعد از اعتبارسنجی موفق رمز: کد را برای موبایل کاربر می‌فرستد و challengeId برمی‌گرداند.</summary>
        Task<LoginChallengeResult> RequestPasswordLoginCodeAsync(Entities.User.User user, CancellationToken cancellationToken);

        /// <summary>تأیید کد مرحله‌ی دوم؛ فقط challengeIdای که از مرحله‌ی رمز صادر شده معتبر است.</summary>
        Task<Entities.User.User> VerifyPasswordLoginCodeAsync(Guid challengeId, string code, CancellationToken cancellationToken);

        /// <summary>ارسال دوباره‌ی کد مرحله‌ی دوم؛ challengeId جدید برمی‌گرداند.</summary>
        Task<LoginChallengeResult> ResendPasswordLoginCodeAsync(Guid challengeId, CancellationToken cancellationToken);
    }
}
