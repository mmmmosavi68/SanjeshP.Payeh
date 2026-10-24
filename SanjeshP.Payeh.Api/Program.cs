using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.WebFramework.Configuration;
using SanjeshP.Payeh.WebFramework.Middlewares;
using Serilog;

// logger موقت: خطاهای startup (مثل اعتبارسنجی کلید JWT) هم ثبت شوند
Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, services, cfg) => cfg
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // ── اعتبارسنجی تنظیمات حساس هنگام startup ──
    var jwtSettings = builder.Configuration.GetSection("SiteSettings:JwtSettings").Get<JwtSettings>() ?? new JwtSettings();
    JwtSettingsValidator.Validate(jwtSettings, builder.Environment.IsDevelopment());

    var siteSettings = builder.Configuration.GetSection("SiteSettings").Get<SiteSettings>() ?? new SiteSettings();

    if (!builder.Environment.IsDevelopment())
    {
        if (string.Equals(siteSettings.SmsSettings.Provider, "Mock", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                "SmsSettings:Provider روی Mock است. در محیط غیر Development باید ارائه‌دهنده‌ی واقعی پیامک تنظیم شود.");

        if (siteSettings.OtpSettings.ReturnCodeInResponse)
            throw new InvalidOperationException(
                "OtpSettings:ReturnCodeInResponse در محیط غیر Development باید false باشد.");
    }

    // ── ثبت سرویس‌ها (همه قبل از Build) ──
    builder.Services.AddMinimalServices(builder.Configuration);

    // مدیریت مرکزی خطا با IExceptionHandler — همه‌ی خطاها در قالب ApiResult برمی‌گردند.
    builder.Services.AddExceptionHandler<CustomExceptionHandler>();
    builder.Services.AddProblemDetails();

    var app = builder.Build();

    // ── pipeline ──
    // اولین میان‌افزار زنجیره تا هر خطایی (حتی خارج از MVC) گرفته شود.
    app.UseExceptionHandler();
    app.UseSerilogRequestLogging();

    // مستندات Scalar: در Development همیشه، در بقیه‌ی محیط‌ها با ApiDocsSettings:Enabled
    app.MapApiDocuments();

    app.UseHttpsRedirection();

    // محدودیت نرخ درخواست
    app.UseRateLimiter();

    app.UseAuthentication();
    app.UseAuthorization();
    app.UseMenuAuthorization();
    app.MapControllers();

    // اعمال Migrationهای معوق و داده‌های اولیه (نقش‌ها و کاربر ادمین)
    app.InitializeDatabase();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "برنامه هنگام startup متوقف شد");
    throw;
}
finally
{
    Log.CloseAndFlush();
}