using Autofac.Core;
using SanjeshP.Payeh.Services.Permissions;
using SanjeshP.Payeh.WebFramework.Configuration;
using SanjeshP.Payeh.WebFramework.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// ثبت همه‌ی سرویس‌ها: DI، EF، JWT، نسخه‌بندی، AutoMapper، OpenAPI، OTP/پیامک/کپچا و Rate Limiting
builder.Services.AddMinimalServices(builder.Configuration);

// مدیریت مرکزی خطا با IExceptionHandler — همه‌ی خطاها در قالب ApiResult برمی‌گردند.
builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services.AddProblemDetails();
var app = builder.Build();

// اولین میان‌افزار زنجیره تا هر خطایی (حتی خارج از MVC) گرفته شود.
app.UseExceptionHandler();

// مستندات Scalar: در Development همیشه، در بقیه‌ی محیط‌ها با ApiDocsSettings:Enabled
app.MapApiDocuments();

app.UseHttpsRedirection();

// محدودیت نرخ درخواست (سیاست otp برای درخواست کد پیامکی)
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();
app.UseMenuAuthorization();
app.MapControllers();

// اعمال Migrationهای معوق و داده‌های اولیه (نقش‌ها و کاربر ادمین)
app.InitializeDatabase();

app.Run();