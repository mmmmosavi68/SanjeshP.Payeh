using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SanjeshP.Payeh.Common.Utilities;
using SanjeshP.Payeh.WebFramework.Filters;
using System.Security.Claims;

namespace SanjeshP.Payeh.WebFramework.Api
{
    /// <summary>
    /// کنترلر پایه‌ی همه‌ی کنترلرهای API:
    /// - مسیر نسخه‌دار: api/v{version}/[controller]
    /// - پوشش خودکار پاسخ‌ها با ApiResult (فیلتر)
    /// - احراز هویت پیش‌فرض؛ اکشن‌های عمومی باید [AllowAnonymous] بگیرند.
    /// </summary>
    [ApiController]
    [ApiResultFilter]
    [Authorize]
    [ApiVersion("1")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class BaseController : ControllerBase
    {
        /// <summary>آیا درخواست‌دهنده احراز هویت شده است؟ (اصلاح غلط تایپی UserIsAutheticated)</summary>
        public bool UserIsAuthenticated => HttpContext.User.Identity?.IsAuthenticated == true;

        /// <summary>شناسه‌ی کاربر فعلی از روی claimها.</summary>
        public Guid CurrentUserId
        {
            get
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                return Guid.TryParse(userId, out var id)
                    ? id
                    : throw new UnauthorizedAccessException(
                        "شناسه کاربر معتبر نیست.");
            }
        }

        /// <summary>شناسه‌ی نشست فعلی از claim مخصوص session_id.</summary>
        public Guid CurrentSessionId =>
            Guid.TryParse(User.FindFirstValue(Services.JwtService.IJwtService.SessionIdClaimType), out var sessionId)
                ? sessionId
                : Guid.Empty;
    }
}