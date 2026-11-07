using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using SanjeshP.Payeh.Api.Models.Auth;
using SanjeshP.Payeh.Api.Models.Users;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Common.Exceptions;
using SanjeshP.Payeh.Data.Contracts;
using SanjeshP.Payeh.Entities.Menu;
using SanjeshP.Payeh.Services.Captcha;
using SanjeshP.Payeh.Services.JwtService;
using SanjeshP.Payeh.Services.Otp;
using SanjeshP.Payeh.Services.Sms;
using SanjeshP.Payeh.Services.Sms.PayamSMS;
using SanjeshP.Payeh.Services.Users;
using SanjeshP.Payeh.WebFramework.Api;
using SanjeshP.Payeh.WebFramework.Middlewares;
using Serilog;
using System.Runtime;
using UserEntity = SanjeshP.Payeh.Entities.User.User;

namespace SanjeshP.Payeh.Api.Controllers.V1
{
    /// <summary>
    /// احراز هویت: ورود با موبایل + کد پیامکی، یا نام کاربری + رمز (با کد پیامکی اختیاری/اجباری)،
    /// کپچا در همه‌ی مراحل ورود، و مدیریت نشست‌ها.
    /// هر ورود یک نشست مستقل است؛ خروج از یک دستگاه بقیه را باطل نمی‌کند.
    /// </summary>
    [ApiVersion("1")]
    public class AuthController : BaseController
    {
        private readonly IOtpService _otpService;
        private readonly IJwtService _jwtService;
        private readonly ICaptchaService _captchaService;
        private readonly IUserService _userService;
        private readonly ISmsSender _smsSender;
        private readonly IPayamSMS _payamSMS;
        private readonly SiteSettings _siteSettings;
        private readonly IAuditLogService _auditLogService;
        
        public AuthController(
            IOtpService otpService,
            IJwtService jwtService,
            ICaptchaService captchaService,
            IUserService userService,
            ISmsSender smsSender,
            IOptions<SiteSettings> siteSettings,
            IPayamSMS payamSMS,
            IAuditLogService auditLogService
            )
        {
            _otpService = otpService;
            _jwtService = jwtService;
            _captchaService = captchaService;
            _userService = userService;
            _smsSender = smsSender;
            _payamSMS = payamSMS;
            _siteSettings = siteSettings.Value;
            _auditLogService = auditLogService;

        }

        /// <summary>تنظیمات عمومی ورود (روش‌های فعال و کپچا) برای ساخت صفحه‌ی ورود.</summary>
        [HttpGet("config")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResult<AuthConfigDto>), StatusCodes.Status200OK)]
        public ApiResult<AuthConfigDto> GetConfig()
        {
            var auth = _siteSettings.AuthSettings;
            var captcha = _siteSettings.CaptchaSettings;

            return new AuthConfigDto
            {
                CaptchaEnabled = _captchaService.IsEnabled,
                CaptchaProvider = _captchaService.IsEnabled ? captcha.Provider : null,
                CaptchaSiteKey = _captchaService.IsEnabled ? captcha.SiteKey : null,
                MobileLoginEnabled = auth.MobileLoginEnabled,
                PasswordLoginEnabled = auth.PasswordLoginEnabled,
                PasswordLoginRequiresOtp = auth.PasswordLoginRequiresOtp,
                OtpLength = _siteSettings.OtpSettings.CodeLength
            };
        }

        // ───────────── ورود با موبایل ─────────────

        /// <summary>درخواست کد ورود پیامکی (ضد افشای عضویت: پاسخ برای شماره‌ی نامعتبر هم عمومی است).</summary>
        [HttpPost("otp/request")]
        [AllowAnonymous]
        [EnableRateLimiting("otp")]
        [ProducesResponseType(typeof(ApiResult<RequestOtpResponseDto>), StatusCodes.Status200OK)]
        public async Task<ApiResult<RequestOtpResponseDto>> RequestOtp([FromBody] RequestOtpDto dto, CancellationToken cancellationToken)
        {
            EnsureMobileLoginEnabled();
            await EnsureCaptchaAsync(dto.CaptchaToken, cancellationToken);

            var result = await _otpService.RequestLoginCodeAsync(dto.MobileNumber, cancellationToken);
            // ارسال پیامک
            //await _smsSender.SendAsync(dto.MobileNumber, $"کد ورود شما: {result.DevCode}\nاعتبار: {_siteSettings.OtpSettings.ExpirationSeconds} ثانیه", cancellationToken);
            await _payamSMS.Send(dto.MobileNumber, $"کد ورود شما: {result.DevCode}\nاعتبار: {_siteSettings.OtpSettings.ExpirationSeconds} ثانیه");
            await _auditLogService.LogActivityAsync(
                action: "Auth.Otp.Requested",
                description: "درخواست کد ورود پیامکی",
                eventType: "Security",
                cancellationToken: cancellationToken);
            return new RequestOtpResponseDto
            {
                DevCode = result.DevCode,
                ExpiresInSeconds = _siteSettings.OtpSettings.ExpirationSeconds,
                ResendAfterSeconds = _siteSettings.OtpSettings.ResendIntervalSeconds
            };
        }

        /// <summary>تأیید کد پیامکی و ورود؛ نشست جدید ساخته می‌شود.</summary>
        [HttpPost("otp/verify")]
        [AllowAnonymous]
        [EnableRateLimiting("otp-verify")]
        [ProducesResponseType(typeof(ApiResult<AuthResponseDto>), StatusCodes.Status200OK)]
        public async Task<ApiResult<AuthResponseDto>> VerifyOtp([FromBody] VerifyOtpDto dto, CancellationToken cancellationToken)
        {
            EnsureMobileLoginEnabled();
            await EnsureCaptchaAsync(dto.CaptchaToken, cancellationToken);

            UserEntity user;
            try
            {
                user = await _otpService.VerifyLoginCodeAsync(dto.MobileNumber, dto.Code, cancellationToken);
            }
            catch (InvalidOtpException)
            {
                await _auditLogService.LogActivityAsync(
                    action: "Auth.Otp.VerificationFailed",
                    description: "اعتبارسنجی کد ورود پیامکی ناموفق بود",
                    eventType: "Security",
                    cancellationToken: CancellationToken.None);
                throw;
            }

            return await IssueTokensAsync(user, dto.DeviceName, "MobileOtp", cancellationToken);
        }

        // ───────────── ورود با نام کاربری و رمز ─────────────

        /// <summary>
        /// مرحله‌ی اول: نام کاربری و رمز. اگر دو مرحله‌ای لازم باشد requiresOtp=true و challenge برمی‌گردد؛
        /// وگرنه همین‌جا توکن‌ها صادر می‌شوند.
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("login")]
        [ProducesResponseType(typeof(ApiResult<PasswordLoginResponseDto>), StatusCodes.Status200OK)]
        public async Task<ApiResult<PasswordLoginResponseDto>> Login([FromBody] PasswordLoginDto dto, CancellationToken cancellationToken)
        {
            EnsurePasswordLoginEnabled();
            await EnsureCaptchaAsync(dto.CaptchaToken, cancellationToken);

            var user = await _userService.ValidateCredentialsAsync(dto.UserName, dto.Password, cancellationToken);
            if (user is null)
            {
                await _auditLogService.LogActivityAsync(
                    action: "Auth.PasswordLogin.Failed",
                    description: "ورود با نام کاربری و رمز عبور ناموفق بود",
                    eventType: "Security",
                    cancellationToken: CancellationToken.None);
                throw new InvalidCredentialsException();
            }

            var requiresOtp = _siteSettings.AuthSettings.PasswordLoginRequiresOtp || user.TwoFactorEnabled == true;
            if (requiresOtp)
            {
                var challenge = await _otpService.RequestPasswordLoginCodeAsync(user, cancellationToken);
                //ارسال پیامک
                //await _smsSender.SendAsync(challenge.MaskedMobile, $"کد ورود شما: {challenge.DevCode}\nاعتبار: {_siteSettings.OtpSettings.ExpirationSeconds} ثانیه", cancellationToken);
                await _payamSMS.Send(challenge.MaskedMobile, $"کد ورود شما: {challenge.DevCode}\nاعتبار: {_siteSettings.OtpSettings.ExpirationSeconds} ثانیه");
                await _auditLogService.LogActivityAsync(
                    action: "Auth.PasswordLogin.OtpChallengeCreated",
                    description: "مرحله اول ورود موفق بود و کد تأیید ارسال شد",
                    entityName: nameof(UserEntity),
                    entityId: user.Id.ToString(),
                    eventType: "Security",
                    actorUserId: user.Id,
                    actorUserName: user.UserName,
                    cancellationToken: cancellationToken);

                return new PasswordLoginResponseDto { RequiresOtp = true, Challenge = ToChallengeDto(challenge) };
            }

            return new PasswordLoginResponseDto
            {
                RequiresOtp = false,
                Auth = await IssueTokensAsync(user, dto.DeviceName, "Password", cancellationToken)
            };
        }

        /// <summary>مرحله‌ی دوم ورود با رمز: تأیید کد پیامکی.</summary>
        [HttpPost("login/verify")]
        [AllowAnonymous]
        [EnableRateLimiting("otp-verify")]
        [ProducesResponseType(typeof(ApiResult<AuthResponseDto>), StatusCodes.Status200OK)]
        public async Task<ApiResult<AuthResponseDto>> VerifyLogin([FromBody] VerifyLoginChallengeDto dto, CancellationToken cancellationToken)
        {
            EnsurePasswordLoginEnabled();
            await EnsureCaptchaAsync(dto.CaptchaToken, cancellationToken);

            UserEntity user;
            try
            {
                user = await _otpService.VerifyPasswordLoginCodeAsync(dto.ChallengeId, dto.Code, cancellationToken);
            }
            catch (InvalidOtpException)
            {
                await _auditLogService.LogActivityAsync(
                    action: "Auth.PasswordLogin.OtpVerificationFailed",
                    description: "تأیید مرحله دوم ورود ناموفق بود",
                    eventType: "Security",
                    cancellationToken: CancellationToken.None);
                throw;
            }

            return await IssueTokensAsync(user, dto.DeviceName, "PasswordOtp", cancellationToken);
        }

        /// <summary>ارسال دوباره‌ی کد مرحله‌ی دوم (challengeId جدید برمی‌گردد).</summary>
        [HttpPost("login/resend")]
        [AllowAnonymous]
        [EnableRateLimiting("otp")]
        [ProducesResponseType(typeof(ApiResult<LoginChallengeDto>), StatusCodes.Status200OK)]
        public async Task<ApiResult<LoginChallengeDto>> ResendLogin([FromBody] ResendLoginChallengeDto dto, CancellationToken cancellationToken)
        {
            EnsurePasswordLoginEnabled();
            await EnsureCaptchaAsync(dto.CaptchaToken, cancellationToken);

            var challenge = await _otpService.ResendPasswordLoginCodeAsync(dto.ChallengeId, cancellationToken);
            // ارسال پیامک
            //await _smsSender.SendAsync(challenge.MaskedMobile, $"کد ورود شما: {challenge.DevCode}\nاعتبار: {_siteSettings.OtpSettings.ExpirationSeconds} ثانیه", cancellationToken);
            await _payamSMS.Send(challenge.MaskedMobile, $"کد ورود شما: {challenge.DevCode}\nاعتبار: {_siteSettings.OtpSettings.ExpirationSeconds} ثانیه");

            return ToChallengeDto(challenge);
        }

        // ───────────── توکن و نشست‌ها ─────────────

        /// <summary>دریافت جفت توکن جدید با Refresh Token (چرخشی؛ استفاده‌ی مجدد از توکن قدیمی نشست را باطل می‌کند).</summary>
        [HttpPost("refresh")]
        [AllowAnonymous]
        [EnableRateLimiting("refresh")]
        [ProducesResponseType(typeof(ApiResult<AuthResponseDto>), StatusCodes.Status200OK)]
        public async Task<ApiResult<AuthResponseDto>> Refresh([FromBody] RefreshTokenRequestDto dto, CancellationToken cancellationToken)
        {
            var token = await _jwtService.RefreshAsync(dto.RefreshToken, cancellationToken);
            var sessionId = Guid.Parse(ReadSessionIdFromToken(token));
            var refreshedUserId = ReadUserIdFromToken(token);
            await _auditLogService.LogActivityAsync(
                action: "Auth.Token.Refreshed",
                description: "توکن نشست با موفقیت تمدید شد",
                entityName: "UserSession",
                entityId: sessionId.ToString(),
                eventType: "Security",
                actorUserId: refreshedUserId,
                cancellationToken: cancellationToken);
            return ToAuthResponse(token, sessionId);
        }

        /// <summary>اطلاعات کاربر واردشده (نام، موبایل، نقش‌ها).</summary>
        [HttpGet("me")]
        [SkipMenuPermission]
        [ProducesResponseType(typeof(ApiResult<MeDto>), StatusCodes.Status200OK)]
        public async Task<ApiResult<MeDto>> Me(CancellationToken cancellationToken)
        {
            var user = await _userService.GetByIdAsync(CurrentUserId, cancellationToken);
            var profile = user.UserProfile.FirstOrDefault();

            return new MeDto
            {
                Id = user.Id,
                UserName = user.UserName,
                FirstName = profile?.FirstName,
                LastName = profile?.LastName,
                PhoneNumber = user.PhoneNumber,
                Roles = user.UserRoles
                    .Where(ur => ur.Role != null)
                    .Select(ur => new RoleDto { Id = ur.Role.Id, NameEn = ur.Role.RoleNameEn, NameFa = ur.Role.RoleNameFa })
                    .ToList()
            };
        }

        /// <summary>خروج از دستگاه فعلی؛ فقط نشستِ همین توکن باطل می‌شود.</summary>
        [HttpPost("logout")]
        [SkipMenuPermission]
        public async Task<ApiResult> Logout(CancellationToken cancellationToken)
        {
            await _jwtService.RevokeSessionAsync(CurrentSessionId, CurrentUserId, cancellationToken);
            await _auditLogService.LogActivityAsync(
                action: "Auth.Logout",
                description: "خروج از نشست فعلی",
                entityName: "UserSession",
                entityId: CurrentSessionId.ToString(),
                eventType: "Security",
                actorUserId: CurrentUserId,
                cancellationToken: cancellationToken);
            return new ApiResult(true, ApiResultStatusCode.Success, "با موفقیت از این دستگاه خارج شدید.");
        }

        /// <summary>خروج از همه‌ی دستگاه‌ها؛ همه‌ی نشست‌های کاربر باطل می‌شوند.</summary>
        [HttpPost("logout-all")]
        [SkipMenuPermission]
        public async Task<ApiResult> LogoutAll(CancellationToken cancellationToken)
        {
            await _jwtService.RevokeAllAsync(CurrentUserId, cancellationToken);
            await _auditLogService.LogActivityAsync(
                action: "Auth.LogoutAll",
                description: "خروج از تمام نشست‌های کاربر",
                entityName: "User",
                entityId: CurrentUserId.ToString(),
                eventType: "Security",
                actorUserId: CurrentUserId,
                cancellationToken: cancellationToken);
            return new ApiResult(true, ApiResultStatusCode.Success, "از همه‌ی دستگاه‌ها خارج شدید.");
        }

        /// <summary>ابطال یک نشست مشخص با شناسه (مثلاً از صفحه‌ی دستگاه‌های من).</summary>
        [HttpDelete("sessions/{id:guid}")]
        [SkipMenuPermission]
        public async Task<ApiResult> RevokeSession([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            await _jwtService.RevokeSessionAsync(id, CurrentUserId, cancellationToken);
            await _auditLogService.LogActivityAsync(
                action: "Auth.Session.Revoked",
                description: "ابطال یک نشست توسط کاربر",
                entityName: "UserSession",
                entityId: id.ToString(),
                eventType: "Security",
                actorUserId: CurrentUserId,
                cancellationToken: cancellationToken);
            return new ApiResult(true, ApiResultStatusCode.Success, "نشست مورد نظر باطل شد.");
        }

        /// <summary>فهرست نشست‌های فعال من (دستگاه‌های واردشده).</summary>
        [HttpGet("sessions")]
        [SkipMenuPermission]
        [ProducesResponseType(typeof(ApiResult<List<SessionDto>>), StatusCodes.Status200OK)]
        public async Task<ApiResult<List<SessionDto>>> GetSessions(CancellationToken cancellationToken)
        {
            var sessions = await _jwtService.GetActiveSessionsAsync(CurrentUserId, cancellationToken);

            return sessions.Select(s => new SessionDto
            {
                Id = s.Id,
                DeviceName = s.DeviceName,
                UserAgent = s.UserAgent,
                IpAddress = s.IpAddress,
                CreatedDate = s.CreatedDate,
                LastUsedAt = s.LastUsedAt,
                RefreshTokenExpireDate = s.RefreshTokenExpireDate,
                IsCurrent = s.Id == CurrentSessionId
            }).ToList();
        }

        // ───────────── کمکی ─────────────

        private string ClientIp() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        private void EnsureMobileLoginEnabled()
        {
            if (!_siteSettings.AuthSettings.MobileLoginEnabled)
                throw new ForbiddenException("ورود با شماره موبایل غیرفعال است.");
        }

        private void EnsurePasswordLoginEnabled()
        {
            if (!_siteSettings.AuthSettings.PasswordLoginEnabled)
                throw new ForbiddenException("ورود با نام کاربری و رمز عبور غیرفعال است.");
        }

        /// <summary>اعتبارسنجی کپچا؛ هر توکن فقط یک‌بار قابل استفاده است، پس هر مرحله توکن تازه می‌خواهد.</summary>
        private async Task EnsureCaptchaAsync(string? token, CancellationToken cancellationToken)
        {
            if (!_captchaService.IsEnabled) return;

            if (!await _captchaService.ValidateAsync(token, ClientIp(), cancellationToken))
                throw new CaptchaInvalidException();
        }

        private async Task<AuthResponseDto> IssueTokensAsync(UserEntity user, string? deviceName, string loginMethod, CancellationToken cancellationToken)
        {
            var ip = ClientIp();
            var token = await _jwtService.GenerateAsync(
                user,
                deviceName ?? "دستگاه ناشناس",
                ip,
                Request.Headers.UserAgent.ToString(),
                cancellationToken);

            var sessionId = Guid.Parse(ReadSessionIdFromToken(token));
            Log.ForContext("Audit", true).Information("{Action} کاربر {UserId} از {Ip}", "Login", user.Id, ip);
            return ToAuthResponse(token, sessionId);
        }

        private LoginChallengeDto ToChallengeDto(LoginChallengeResult challenge) => new()
        {
            ChallengeId = challenge.ChallengeId,
            MaskedMobile = challenge.MaskedMobile,
            ExpiresInSeconds = _siteSettings.OtpSettings.ExpirationSeconds,
            ResendAfterSeconds = challenge.ResendAfterSeconds,
            DevCode = challenge.DevCode
        };

        private static string ReadSessionIdFromToken(Services.AccessToken token)
        {
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token.AccessTokenValue);
            return jwt.Claims.First(c => c.Type == IJwtService.SessionIdClaimType).Value;
        }

        private static Guid? ReadUserIdFromToken(Services.AccessToken token)
        {
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token.AccessTokenValue);
            var value = jwt.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(value, out var userId) ? userId : null;
        }

        private static AuthResponseDto ToAuthResponse(Services.AccessToken token, Guid sessionId) => new()
        {
            AccessToken = token.AccessTokenValue,
            RefreshToken = token.RefreshToken,
            TokenType = token.TokenType,
            ExpiresIn = token.ExpiresIn,
            SessionId = sessionId
        };
    }
}
