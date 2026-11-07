using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Common.Exceptions;
using SanjeshP.Payeh.Common.Utilities;
using SanjeshP.Payeh.Data;
using SanjeshP.Payeh.Entities.User;
//using SanjeshP.Payeh.Services.Sms;
using System.Security.Cryptography;

namespace SanjeshP.Payeh.Services.Otp
{
    /// <summary>
    /// پیاده‌سازی امن OTP:
    /// - کد با RandomNumberGenerator (رمزنگارانه) تولید می‌شود.
    /// - فقط هش SHA-256 با نمکِ شناسه‌ی رکورد ذخیره می‌شود.
    /// - انقضا، سقف تلاش ناموفق، فاصله‌ی ارسال مجدد و یک‌بارمصرف بودن اعمال می‌شود.
    /// - مقایسه‌ی هش به‌صورت زمان‌ثابت (FixedTimeEquals) است.
    /// دو هدف (Purpose): Login (ورود با موبایل) و PasswordLogin (مرحله‌ی دوم ورود با رمز).
    /// </summary>
    public class OtpService : IOtpService, IScopedDependency
    {
        private const string LoginPurpose = "Login";
        private const string PasswordLoginPurpose = "PasswordLogin";
        private const string InvalidCodeMessage = "کد وارد شده نامعتبر یا منقضی شده است.";
        private const string InvalidChallengeMessage = "درخواست ورود نامعتبر یا منقضی شده است؛ دوباره وارد شوید.";
        private const int ChallengeMaxAgeMinutes = 30;

        private readonly ApplicationDbContext _dbContext;
        //private readonly ISmsSender _smsSender;
        private readonly OtpSettings _settings;
        private readonly ILogger<OtpService> _logger;

        public OtpService(
            ApplicationDbContext dbContext,
            //ISmsSender smsSender,
            IOptions<SiteSettings> siteSettings,
            ILogger<OtpService> logger)
        {
            _dbContext = dbContext;
            //_smsSender = smsSender;
            _settings = siteSettings.Value.OtpSettings;
            _logger = logger;
        }

        // ───────────── ورود با موبایل ─────────────

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

            var lastOtp = await _dbContext.Set<UserOtp>()
                .Where(x => x.UserId == user.Id && x.Purpose == LoginPurpose)
                .OrderByDescending(x => x.CreatedDate)
                .FirstOrDefaultAsync(cancellationToken);

            // محدودیت فاصله‌ی ارسال مجدد
            if (lastOtp is not null)
            {
                var elapsed = (now - lastOtp.CreatedDate).TotalSeconds;
                if (elapsed < _settings.ResendIntervalSeconds)
                    throw new TooManyRequestsException(WaitMessage(elapsed));
            }

            var (_, code) = await CreateAndSendAsync(user, mobileNumber, LoginPurpose, cancellationToken);
            return new OtpRequestResult(_settings.ReturnCodeInResponse ? code : null);
        }

        /// <inheritdoc />
        public async Task<User> VerifyLoginCodeAsync(string mobileNumber, string code, CancellationToken cancellationToken)
        {
            mobileNumber = NormalizeMobile(mobileNumber);
            code = NormalizeDigits(code).Trim();

            var user = await FindActiveUserByMobileAsync(mobileNumber, cancellationToken, includeRoles: true)
                ?? throw new InvalidOtpException(InvalidCodeMessage);

            var otp = await _dbContext.Set<UserOtp>().AsNoTracking()
                .Where(x => x.UserId == user.Id && x.Purpose == LoginPurpose && !x.IsUsed)
                .OrderByDescending(x => x.CreatedDate)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new InvalidOtpException(InvalidCodeMessage);

            await ConsumeAsync(otp, code, cancellationToken);
            await MarkLoggedInAsync(user, cancellationToken);
            return user;
        }

        // ───────────── مرحله‌ی دوم ورود با رمز ─────────────

        /// <inheritdoc />
        public async Task<LoginChallengeResult> RequestPasswordLoginCodeAsync(User user, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(user.PhoneNumber))
                throw new BadRequestException("برای این حساب شماره موبایل ثبت نشده است؛ با مدیر سیستم تماس بگیرید.");

            var mobile = NormalizeMobile(user.PhoneNumber);
            var now = DateTime.UtcNow;

            var latest = await _dbContext.Set<UserOtp>().AsNoTracking()
                .Where(x => x.UserId == user.Id && x.Purpose == PasswordLoginPurpose)
                .OrderByDescending(x => x.CreatedDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (latest is not null)
            {
                var elapsed = (now - latest.CreatedDate).TotalSeconds;
                if (elapsed < _settings.ResendIntervalSeconds)
                {
                    var remaining = (int)Math.Ceiling(_settings.ResendIntervalSeconds - elapsed);

                    // ورود تکراری در فاصله‌ی مجاز: بدون ارسال پیامک جدید همان درخواست قبلی برمی‌گردد.
                    if (!latest.IsUsed && latest.ExpireAt > now && latest.FailedAttempts < _settings.MaxAttempts)
                        return new LoginChallengeResult(latest.Id, MaskMobile(mobile), null, remaining);

                    throw new TooManyRequestsException(WaitMessage(elapsed));
                }
            }

            var (otp, code) = await CreateAndSendAsync(user, mobile, PasswordLoginPurpose, cancellationToken);
            return new LoginChallengeResult(
                otp.Id, MaskMobile(mobile), _settings.ReturnCodeInResponse ? code : null, _settings.ResendIntervalSeconds);
        }

        /// <inheritdoc />
        public async Task<User> VerifyPasswordLoginCodeAsync(Guid challengeId, string code, CancellationToken cancellationToken)
        {
            code = NormalizeDigits(code).Trim();

            var otp = await _dbContext.Set<UserOtp>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == challengeId && x.Purpose == PasswordLoginPurpose && !x.IsUsed, cancellationToken)
                ?? throw new InvalidOtpException(InvalidCodeMessage);

            var user = await _dbContext.Set<User>()
                .Where(x => x.Id == otp.UserId && !x.IsDeleted && x.IsActive
                            && (x.LockoutEnd == null || x.LockoutEnd <= DateTimeOffset.UtcNow))
                .Include(x => x.UserRoles).ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new InvalidOtpException(InvalidCodeMessage);

            await ConsumeAsync(otp, code, cancellationToken);
            await MarkLoggedInAsync(user, cancellationToken);
            return user;
        }

        /// <inheritdoc />
        public async Task<LoginChallengeResult> ResendPasswordLoginCodeAsync(Guid challengeId, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var otps = _dbContext.Set<UserOtp>();

            var old = await otps.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == challengeId && x.Purpose == PasswordLoginPurpose && x.ConsumedAt == null, cancellationToken);

            if (old is null || old.CreatedDate < now.AddMinutes(-ChallengeMaxAgeMinutes))
                throw new InvalidOtpException(InvalidChallengeMessage);

            var user = await _dbContext.Set<User>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == old.UserId && !x.IsDeleted && x.IsActive
                                          && (x.LockoutEnd == null || x.LockoutEnd <= DateTimeOffset.UtcNow), cancellationToken)
                ?? throw new InvalidOtpException(InvalidChallengeMessage);

            var mobile = old.SentTo ?? user.PhoneNumber;
            if (string.IsNullOrWhiteSpace(mobile))
                throw new InvalidOtpException(InvalidChallengeMessage);

            var latest = await otps.AsNoTracking()
                .Where(x => x.UserId == user.Id && x.Purpose == PasswordLoginPurpose)
                .OrderByDescending(x => x.CreatedDate)
                .FirstAsync(cancellationToken);

            var elapsed = (now - latest.CreatedDate).TotalSeconds;
            if (elapsed < _settings.ResendIntervalSeconds)
                throw new TooManyRequestsException(WaitMessage(elapsed));

            var (otp, code) = await CreateAndSendAsync(user, mobile, PasswordLoginPurpose, cancellationToken);
            return new LoginChallengeResult(
                otp.Id, MaskMobile(mobile), _settings.ReturnCodeInResponse ? code : null, _settings.ResendIntervalSeconds);
        }

        // ───────────── اشتراکی ─────────────

        /// <summary>کدهای قبلیِ مصرف‌نشده را باطل می‌کند، کد جدید می‌سازد و پیامک می‌فرستد.</summary>
        private async Task<(UserOtp Otp, string Code)> CreateAndSendAsync(
            User user, string mobileNumber, string purpose, CancellationToken cancellationToken)
        {
            var otps = _dbContext.Set<UserOtp>();

            // یک‌بارمصرف بودن: همه‌ی کدهای قبلیِ مصرف‌نشده باطل می‌شوند.
            await otps
                .Where(x => x.UserId == user.Id && x.Purpose == purpose && !x.IsUsed)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsUsed, true), cancellationToken);

            var code = GenerateNumericCode(_settings.CodeLength);

            var otp = new UserOtp
            {
                UserId = user.Id,
                SentTo = mobileNumber,
                Purpose = purpose,
                ExpireAt = DateTime.UtcNow.AddSeconds(_settings.ExpirationSeconds),
                CreatedBy = user.Id
            };
            // شناسه‌ی رکورد (Guid) در سازنده ساخته شده و نقش نمک هش را دارد.
            otp.CodeHash = HashCode(otp.Id, code);

            otps.Add(otp);
            await _dbContext.SaveChangesAsync(cancellationToken);

            //await _smsSender.SendAsync(mobileNumber, $"کد ورود شما: {code}\nاعتبار: {_settings.ExpirationSeconds} ثانیه", cancellationToken);

            return (otp, code);
        }

        /// <summary>بررسی انقضا، رزرو اتمیک تلاش، مقایسه‌ی زمان‌ثابت و مصرف اتمیک کد.</summary>
        private async Task ConsumeAsync(UserOtp otp, string code, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            if (otp.ExpireAt <= now)
                throw new InvalidOtpException("کد وارد شده منقضی شده است؛ کد جدید درخواست کنید.");

            var otps = _dbContext.Set<UserOtp>();

            // ۱) رزرو اتمیک یک تلاش: حتی با درخواست‌های موازی، مجموع تلاش‌ها از MaxAttempts بیشتر نمی‌شود.
            var reserved = await otps
                .Where(x => x.Id == otp.Id && !x.IsUsed && x.FailedAttempts < _settings.MaxAttempts)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.FailedAttempts, x => x.FailedAttempts + 1), cancellationToken);

            if (reserved == 0)
                throw new InvalidOtpException("تعداد تلاش‌ها بیش از حد مجاز است؛ کد جدید درخواست کنید.");

            // ۲) مقایسه‌ی زمان‌ثابت هش‌ها
            var expected = Convert.FromBase64String(otp.CodeHash);
            var actual = Convert.FromBase64String(HashCode(otp.Id, code));

            if (!CryptographicOperations.FixedTimeEquals(expected, actual))
            {
                var remaining = Math.Max(0, _settings.MaxAttempts - (otp.FailedAttempts + 1));
                throw new InvalidOtpException(remaining > 0
                    ? $"کد وارد شده اشتباه است. {remaining} تلاش دیگر باقی مانده است."
                    : "تعداد تلاش‌های ناموفق بیش از حد مجاز است؛ کد جدید درخواست کنید.");
            }

            // ۳) مصرف اتمیک: اگر دو درخواست درست هم‌زمان برسند، فقط یکی نشست می‌گیرد.
            var consumed = await otps
                .Where(x => x.Id == otp.Id && !x.IsUsed)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.IsUsed, true)
                    .SetProperty(x => x.ConsumedAt, (DateTime?)now), cancellationToken);

            if (consumed == 0)
                throw new InvalidOtpException(InvalidCodeMessage);
        }

        private async Task MarkLoggedInAsync(User user, CancellationToken cancellationToken)
        {
            user.PhoneNumberConfirmed = true;
            user.LastLoginDate = DateTime.UtcNow;
            user.AccessFailedCount = 0;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private string WaitMessage(double elapsedSeconds)
        {
            var remaining = Math.Max(1, (int)Math.Ceiling(_settings.ResendIntervalSeconds - elapsedSeconds));
            return $"برای دریافت کد جدید، {remaining} ثانیه صبر کنید.";
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
            IQueryable<User> query = _dbContext.Set<User>().Where(x => !x.IsDeleted);

            query = includeRoles
                ? query.Include(x => x.UserRoles).ThenInclude(ur => ur.Role)
                : query.AsNoTracking();

            return await query.FirstOrDefaultAsync(
                x => x.PhoneNumber == mobileNumber && x.IsActive && (x.LockoutEnd == null || x.LockoutEnd <= DateTimeOffset.UtcNow),
                cancellationToken);
        }

        /// <summary>نمایش پوشیده‌ی شماره: 0937•••1160.</summary>
        private static string MaskMobile(string mobile)
            => mobile.Length >= 8 ? $"{mobile[..4]}•••{mobile[^4..]}" : "••••";

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
