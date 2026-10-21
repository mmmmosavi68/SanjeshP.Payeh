using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SanjeshP.Payeh.WebFramework.Api;

namespace SanjeshP.Payeh.Api.Controllers.V1
{
    /// <summary>
    /// اکشن‌های آزمایشی/سلامت سرویس.
    /// اصلاحات: انتقال به نسخه‌ی v1، ارث‌بری از BaseController (پوشش ApiResult)
    /// و حذف خروجی‌های شیء ناشناس خارج از قرارداد.
    /// </summary>
    [ApiVersion("1")]
    public class HomeController : BaseController
    {
        /// <summary>بررسی سلامت سرویس (بدون احراز هویت).</summary>
        [HttpGet("Test")]
        [AllowAnonymous]
        public ApiResult<object> Test()
        {
            return Ok(new
            {
                Id = 1,
                Name = "Test User",
                Email = "test@example.com"
            });
        }

        /// <summary>اکشن آزمایشی دریافت داده (بدون احراز هویت).</summary>
        [HttpPost("Test")]
        [AllowAnonymous]
        public ApiResult<TestRequestDto> TestPost([FromBody] TestRequestDto request)
        {
            return request;
        }
    }

    /// <summary>ورودی آزمایشی اکشن Test.</summary>
    public class TestRequestDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}