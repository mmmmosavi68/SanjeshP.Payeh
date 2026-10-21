using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Common.Exceptions;
using SanjeshP.Payeh.Common.Utilities;
using SanjeshP.Payeh.Data;
using SanjeshP.Payeh.Entities.User;
using SanjeshP.Payeh.Services.Sms;
using System.Security.Cryptography;

namespace SanjeshP.Payeh.Services.Otp
{
    /// <summary>
    /// پیاده‌سازی امن OTP:
    /// - کد با RandomNumberGenerator (رمزنگارانه) تولید می‌شود.
    /// - فقط هش SHA-256 با نمکِ شناسه‌ی رکورد ذخیره می‌شود.
    /// - انقضا، سقف تلاش ناموفق، فاصله‌ی ارسال مجدد و یک‌بارمصرف بودن اعمال می‌شود.
    /// - مقایسه‌ی هش به‌صورت زمان‌ثابت (FixedTimeEquals) است.
    /// </summary>
    public class OtpService : IOtpService, IScopedDependency
    {
        private const string LoginPurpose = "Login";

        private readonly ApplicationDbContext _dbContext;
        private readonly ISmsSender _smsSender;
        private readonly OtpSettings _settings;
        private readonly ILogger<OtpService> _logger;

        public OtpService(
            ApplicationDbContext dbContext,
            ISmsSender smsSender,
            IOptions<SiteSettings> siteSettings,
            ILogger<OtpService> logger)
        {
            _dbContext = dbContext;
            _smsSender = smsSender;
            _settings = siteSettings.Value.OtpSettings;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<OtpRequestResult> RequestLoginCodeAsync(string mobileNumber, CancellationToken cancellationToken)
        {
            mobileNumber = NormalizeMobile(mobileNumber);
            var now = DateTime.UtcNow;

            var user = await FindActiveUserByMobileAsync(mobileNumber, cancellationToken);

            // ضد افشای عضویت: اگر کاربر وجود ندارد، همان پاسخ عمومی برگردانده می‌شود (بدون ارسال پیامک).
            if (user is null)
            {
                _logger.LogInformation("درخواست OTP برای شماره‌ی ثبت‌نشده {Mobile}", mobileNumber);
                return new OtpRequestResult(null);
            }

            var otps = _dbContext.Set<UserOtp>();

            var lastOtp = await otps
                .Where(x => x.UserId == user.Id && x.Purpose == LoginPurpose)
                .OrderByDescending(x => x.CreatedDate)
                .FirstOrDefaultAsync(cancellationToken);

            // محدودیت فاصله‌ی ارسال مجدد
            if (lastOtp is not null)
            {
                var elapsed = (now - lastOtp.CreatedDate).TotalSeconds;
                if (elapsed < _settings.ResendIntervalSeconds)
                {
                    var remaining = (int)Math.Ceiling(_settings.ResendIntervalSeconds - elapsed);
                    throw new TooManyRequestsException($"برای دریافت کد جدید، {remaining} ثانیه صبر کنید.");
                }
            }

            // یک‌بارمصرف بودن: همه‌ی کدهای قبلیِ مصرف‌نشده باطل می‌شوند.
            await otps
                .Where(x => x.UserId == user.Id && x.Purpose == LoginPurpose && !x.IsUsed)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsUsed, true), cancellationToken);

            var code = GenerateNumericCode(_settings.CodeLength);

            var otp = new UserOtp
            {
                UserId = user.Id,
                SentTo = mobileNumber,
                Purpose = LoginPurpose,
                ExpireAt = now.AddSeconds(_settings.ExpirationSeconds),
                CreatedBy = user.Id
            };
            // شناسه‌ی رکورد (Guid) در سازنده ساخته شده و نقش نمک هش را دارد.
            otp.CodeHash = HashCode(otp.Id, code);

            otps.Add(otp);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _smsSender.SendAsync(mobileNumber, $"کد ورود شما: {code}\nاعتبار: {_settings.ExpirationSeconds} ثانیه", cancellationToken);

            return new OtpRequestResult(_settings.ReturnCodeInResponse ? code : null);
        }

        /// <inheritdoc />
        public async Task<User> VerifyLoginCodeAsync(string mobileNumber, string code, CancellationToken cancellationToken)
        {
            mobileNumber = NormalizeMobile(mobileNumber);
            code = NormalizeDigits(code).Trim();

            var user = await FindActiveUserByMobileAsync(mobileNumber, cancellationToken, includeRoles: true)
                ?? throw new BadRequestException("کد وارد شده نامعتبر یا منقضی شده است.");

            var otp = await _dbContext.Set<UserOtp>()
                .Where(x => x.UserId == user.Id && x.Purpose == LoginPurpose && !x.IsUsed)
                .OrderByDescending(x => x.CreatedDate)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new BadRequestException("کد وارد شده نامعتبر یا منقضی شده است.");

            var now = DateTime.UtcNow;

            if (otp.ExpireAt <= now)
                throw new BadRequestException("کد وارد شده منقضی شده است؛ کد جدید درخواست کنید.");

            if (otp.FailedAttempts >= _settings.MaxAttempts)
                throw new BadRequestException("تعداد تلاش‌های ناموفق بیش از حد مجاز است؛ کد جدید درخواست کنید.");

            // مقایسه‌ی زمان‌ثابت برای جلوگیری از حملات زمان‌سنجی
            var expected = Convert.FromBase64String(otp.CodeHash);
            var actual = Convert.FromBase64String(HashCode(otp.Id, code));

            if (!CryptographicOperations.FixedTimeEquals(expected, actual))
            {
                otp.FailedAttempts++;
                await _dbContext.SaveChangesAsync(cancellationToken);

                var remainingAttempts = _settings.MaxAttempts - otp.FailedAttempts;
                throw new BadRequestException(remainingAttempts > 0
                    ? $"کد وارد شده اشتباه است. {remainingAttempts} تلاش دیگر باقی مانده است."
                    : "تعداد تلاش‌های ناموفق بیش از حد مجاز است؛ کد جدید درخواست کنید.");
            }

            // مصرف کد + به‌روزرسانی وضعیت کاربر
            otp.IsUsed = true;
            otp.ConsumedAt = now;
            user.PhoneNumberConfirmed = true;
            user.LastLoginDate = now;
            user.AccessFailedCount = 0;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return user;
        }

        /// <summary>هش کد با نمکِ شناسه‌ی رکورد.</summary>
        private static string HashCode(Guid otpId, string code)
            => SecurityHelper.GetSha256Hash($"{otpId}:{code}");

        /// <summary>تولید کد عددی رمزنگارانه (بدون سوگیری modulo).</summary>
        private static string GenerateNumericCode(int length)
        {
            var max = (int)Math.Pow(10, length);
            return RandomNumberGenerator.GetInt32(0, max).ToString().PadLeft(length, '0');
        }

        private async Task<User?> FindActiveUserByMobileAsync(string mobileNumber, CancellationToken cancellationToken, bool includeRoles = false)
        {
            var query = _dbContext.Set<User>().AsNoTracking().Where(x => !x.IsDeleted);
            if (includeRoles)
                query = _dbContext.Set<User>().Where(x => !x.IsDeleted)
                    .Include(x => x.UserRoles).ThenInclude(ur => ur.Role);

            return await query.FirstOrDefaultAsync(
                x => x.PhoneNumber == mobileNumber && x.IsActive && (x.LockoutEnd == null || x.LockoutEnd <= DateTimeOffset.UtcNow),
                cancellationToken);
        }

        /// <summary>نرمال‌سازی شماره به فرمت 09xxxxxxxxx.</summary>
        private static string NormalizeMobile(string mobile)
        {
            mobile = NormalizeDigits(mobile).Trim().Replace(" ", "").Replace("-", "");
            if (mobile.StartsWith("+98")) mobile = "0" + mobile[3..];
            else if (mobile.StartsWith("98") && mobile.Length == 12) mobile = "0" + mobile[2..];
            else if (mobile.StartsWith("9") && mobile.Length == 10) mobile = "0" + mobile;
            return mobile;
        }

        /// <summary>تبدیل ارقام فارسی/عربی به انگلیسی.</summary>
        private static string NormalizeDigits(string input)
        {
            return string.Concat(input.Select(c => c switch
            {
                >= '\u06F0' and <= '\u06F9' => (char)('0' + c - '\u06F0'),
                >= '\u0660' and <= '\u0669' => (char)('0' + c - '\u0660'),
                _ => c
            }));
        }
    }
}