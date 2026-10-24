using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using SanjeshP.Payeh.Api.Models.Auth;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Common.Exceptions;
using SanjeshP.Payeh.Services.Captcha;
using SanjeshP.Payeh.Services.JwtService;
using SanjeshP.Payeh.Services.Otp;
using SanjeshP.Payeh.WebFramework.Api;
using SanjeshP.Payeh.WebFramework.Middlewares;
using Serilog;

namespace SanjeshP.Payeh.Api.Controllers.V1
{
    /// <summary>
    /// احراز هویت با کد یک‌بارمصرف پیامکی + مدیریت نشست‌ها.
    /// جریان ورود: otp/request → otp/verify → دریافت AccessToken/RefreshToken.
    /// هر ورود یک نشست مستقل است؛ خروج از یک دستگاه بقیه را باطل نمی‌کند.
    /// </summary>
    [ApiVersion("1")]
    public class AuthController : BaseController
    {
        private readonly IOtpService _otpService;
        private readonly IJwtService _jwtService;
        private readonly ICaptchaService _captchaService;
        private readonly SiteSettings _siteSettings;

        public AuthController(
            IOtpService otpService,
            IJwtService jwtService,
            ICaptchaService captchaService,
            IOptions<SiteSettings> siteSettings)
        {
            _otpService = otpService;
            _jwtService = jwtService;
            _captchaService = captchaService;
            _siteSettings = siteSettings.Value;
        }

        /// <summary>درخواست کد ورود پیامکی. کپچا در صورت فعال بودن از تنظیمات بررسی می‌شود.</summary>
        /// <response code="200">در صورت ثبت‌نام بودن شماره، کد ارسال شد (پاسخ عمداً عمومی است).</response>
        /// <response code="429">ارسال مجدد زودتر از فاصله‌ی مجاز یا عبور از سقف درخواست.</response>
        [HttpPost("otp/request")]
        [AllowAnonymous]
        [EnableRateLimiting("otp")]
        [ProducesResponseType(typeof(ApiResult<RequestOtpResponseDto>), StatusCodes.Status200OK)]
        public async Task<ApiResult<RequestOtpResponseDto>> RequestOtp([FromBody] RequestOtpDto dto, CancellationToken cancellationToken)
        {
            if (_captchaService.IsEnabled &&
                !await _captchaService.ValidateAsync(dto.CaptchaToken, HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken))
            {
                throw new BadRequestException("کپچا نامعتبر است. لطفاً دوباره تلاش کنید.");
            }

            var result = await _otpService.RequestLoginCodeAsync(dto.MobileNumber, cancellationToken);

            return new RequestOtpResponseDto
            {
                DevCode = result.DevCode,
                ExpiresInSeconds = _siteSettings.OtpSettings.ExpirationSeconds,
                ResendAfterSeconds = _siteSettings.OtpSettings.ResendIntervalSeconds
            };
        }

        /// <summary>تأیید کد پیامکی و ورود؛ خروجی شامل جفت توکن و شناسه‌ی نشست است.</summary>
        /// <response code="200">ورود موفق.</response>
        /// <response code="400">کد اشتباه/منقضی یا عبور از سقف تلاش.</response>
        [HttpPost("otp/verify")]
        [AllowAnonymous]
        [EnableRateLimiting("otp-verify")]
        [ProducesResponseType(typeof(ApiResult<AuthResponseDto>), StatusCodes.Status200OK)]
        public async Task<ApiResult<AuthResponseDto>> VerifyOtp([FromBody] VerifyOtpDto dto, CancellationToken cancellationToken)
        {
            var user = await _otpService.VerifyLoginCodeAsync(dto.MobileNumber, dto.Code, cancellationToken);

            var token = await _jwtService.GenerateAsync(
                user,
                dto.DeviceName ?? "دستگاه ناشناس",
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                Request.Headers.UserAgent.ToString(),
                cancellationToken);

            var sessionId = Guid.Parse(ReadSessionIdFromToken(token));
            Log.ForContext("Audit", true).Information("{Action} کاربر {UserId} از {Ip}", "Login", user.Id, HttpContext.Connection.RemoteIpAddress?.ToString());
            return ToAuthResponse(token, sessionId);
        }

        /// <summary>دریافت جفت توکن‌ی جدید با Refresh Token (چرخشی؛ توکن قبلی باطل می‌شود).</summary>
        /// <response code="401">توکن نامعتبر/منقضی یا استفاده‌ی مجدد از توکن چرخش‌یافته.</response>
        [HttpPost("refresh")]
        [AllowAnonymous]
        [EnableRateLimiting("refresh")]
        [ProducesResponseType(typeof(ApiResult<AuthResponseDto>), StatusCodes.Status200OK)]
        public async Task<ApiResult<AuthResponseDto>> Refresh([FromBody] RefreshTokenRequestDto dto, CancellationToken cancellationToken)
        {
            var token = await _jwtService.RefreshAsync(dto.RefreshToken, cancellationToken);
            var sessionId = Guid.Parse(ReadSessionIdFromToken(token));
            return ToAuthResponse(token, sessionId);
        }

        /// <summary>خروج از دستگاه فعلی؛ فقط نشستِ همین توکن باطل می‌شود.</summary>
        [HttpPost("logout")]
        [SkipMenuPermission]
        public async Task<ApiResult> Logout(CancellationToken cancellationToken)
        {
            await _jwtService.RevokeSessionAsync(CurrentSessionId, CurrentUserId, cancellationToken);
            return new ApiResult(true, ApiResultStatusCode.Success, "با موفقیت از این دستگاه خارج شدید.");
        }

        /// <summary>خروج از همه‌ی دستگاه‌ها؛ همه‌ی نشست‌های کاربر باطل می‌شوند.</summary>
        [HttpPost("logout-all")]
        [SkipMenuPermission]
        public async Task<ApiResult> LogoutAll(CancellationToken cancellationToken)
        {
            await _jwtService.RevokeAllAsync(CurrentUserId, cancellationToken);
            return new ApiResult(true, ApiResultStatusCode.Success, "از همه‌ی دستگاه‌ها خارج شدید.");
        }

        /// <summary>ابطال یک نشست مشخص با شناسه (مثلاً از صفحه‌ی دستگاه‌های من).</summary>
        [HttpDelete("sessions/{id:guid}")]
        [SkipMenuPermission]
        public async Task<ApiResult> RevokeSession([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            await _jwtService.RevokeSessionAsync(id, CurrentUserId, cancellationToken);
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

        /// <summary>استخراج شناسه‌ی نشست از claim توکن صادرشده.</summary>
        private static string ReadSessionIdFromToken(Services.AccessToken token)
        {
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token.AccessTokenValue);
            return jwt.Claims.First(c => c.Type == IJwtService.SessionIdClaimType).Value;
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