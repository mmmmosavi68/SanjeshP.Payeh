namespace SanjeshP.Payeh.Services.Otp
{
    /// <summary>نتیجه‌ی درخواست کد یک‌بارمصرف.</summary>
    /// <param name="DevCode">
    /// فقط در حالت توسعه (OtpSettings.ReturnCodeInResponse=true) مقدار دارد
    /// تا بدون پنل پیامک بتوان در Scalar تست کرد؛ در Production همیشه null.
    /// </param>
    public record OtpRequestResult(string? DevCode);

    /// <summary>
    /// سرویس کد یک‌بارمصرف پیامکی با الزامات امنیتی:
    /// ذخیره‌ی Hash، انقضا، سقف تلاش ناموفق، فاصله‌ی ارسال مجدد و یک‌بارمصرف بودن.
    /// </summary>
    public interface IOtpService
    {
        /// <summary>تولید و ارسال کد ورود به شماره‌ی موبایل. کدهای قبلیِ مصرف‌نشده باطل می‌شوند.</summary>
        Task<OtpRequestResult> RequestLoginCodeAsync(string mobileNumber, CancellationToken cancellationToken);

        /// <summary>
        /// تأیید کد ورود. در صورت موفقیت، کد مصرف و کاربر (به‌همراه نقش‌ها) برگردانده می‌شود.
        /// خطاها به‌صورت AppException پرتاب می‌شوند تا مدیریت خطای مرکزی پاسخ استاندارد بسازد.
        /// </summary>
        Task<Entities.User.User> VerifyLoginCodeAsync(string mobileNumber, string code, CancellationToken cancellationToken);
    }
}