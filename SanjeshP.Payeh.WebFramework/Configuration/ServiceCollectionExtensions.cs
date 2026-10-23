using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Data;
using SanjeshP.Payeh.Data.Contracts;
using SanjeshP.Payeh.Data.Repositories;
using SanjeshP.Payeh.Entities.User;
using SanjeshP.Payeh.Services.Captcha;
using SanjeshP.Payeh.Services.Sms;
using SanjeshP.Payeh.WebFramework.CustomMapping;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Threading.RateLimiting;

namespace SanjeshP.Payeh.WebFramework.Configuration
{
    /// <summary>
    /// ثبت همه‌ی سرویس‌های برنامه. نام متد AddMinimalServices حفظ شده تا Program.cs نشکند،
    /// اما حالا واقعاً کامل است: DI، احراز هویت، نسخه‌بندی، OpenAPI، AutoMapper،
    /// اعتبارسنجی فارسی، Rate Limiting و سرویس‌های OTP/پیامک/کپچا.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        public static void AddMinimalServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SiteSettings>(configuration.GetSection(nameof(SiteSettings)));
            var siteSettings = configuration.GetSection(nameof(SiteSettings)).Get<SiteSettings>() ?? new SiteSettings();

            services.AddHttpContextAccessor();
            services.AddHttpClient();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("SqlServer")));

            AddControllersWithPersianValidation(services);
            AddVersioning(services);
            AddApplicationServices(services, siteSettings, configuration);
            AddRateLimiting(services);

            services.AddJwtAuthentication(siteSettings.JwtSettings);
            services.AddAuthorizationPolicies();

            // AutoMapper — نکته‌ی لایسنس در AutoMapperConfiguration توضیح داده شده است.
            // اسمبلی Api از طریق نقطه‌ی ورود برنامه پیدا می‌شود تا Profileهای DTO آنجا هم ثبت شوند.
            var entryAssembly = System.Reflection.Assembly.GetEntryAssembly() ?? typeof(ServiceCollectionExtensions).Assembly;
            //services.InitializeAutoMapper(configuration, typeof(ServiceCollectionExtensions).Assembly, entryAssembly);

            services.InitializeAutoMapper(
                configuration,
                typeof(ServiceCollectionExtensions).Assembly,
                entryAssembly);

            // OpenAPI داخلی .NET برای هر نسخه‌ی API یک سند مستقل می‌سازد.
            services.AddOpenApiDocuments();
        }

        /// <summary>MVC + یکدست‌سازی JSON (camelCase، بدون null، فارسی خوانا) + پیام‌های اعتبارسنجی فارسی.</summary>
        private static void AddControllersWithPersianValidation(IServiceCollection services)
        {
            services.AddControllers()
       .AddJsonOptions(options =>
       {
           options.JsonSerializerOptions.PropertyNamingPolicy =
               System.Text.Json.JsonNamingPolicy.CamelCase;

           options.JsonSerializerOptions.DefaultIgnoreCondition =
               System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;

           options.JsonSerializerOptions.Encoder =
               System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

           options.JsonSerializerOptions.ReferenceHandler =
               System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
       });

            // پیام‌های پیش‌فرض ModelBinding به فارسی
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(e => e.Value?.Errors.Count > 0)
                        .SelectMany(e => e.Value!.Errors.Select(x =>
                            string.IsNullOrWhiteSpace(x.ErrorMessage) ? "مقدار ارسالی معتبر نیست" : x.ErrorMessage))
                        .Distinct();

                    return new BadRequestObjectResult(new SerializableError
                    {
                        ["ModelState"] = errors.ToArray()
                    });
                };
            });
        }

        /// <summary>
        /// نسخه‌بندی با Asp.Versioning (جایگزین پکیج قدیمی Microsoft.AspNetCore.Mvc.Versioning).
        /// پیش‌فرض: URL Segment مثل /api/v1/users + پشتیبانی هم‌زمان از هدر X-Api-Version.
        /// </summary>
        private static void AddVersioning(IServiceCollection services)
        {
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                // اعلام نسخه‌های پشتیبانی‌شده و منسوخ در هدرهای پاسخ
                options.ReportApiVersions = true;
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),
                    new HeaderApiVersionReader("X-Api-Version"));
            })
            .AddMvc()
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });
        }

        /// <summary>ثبت مخازن و سرویس‌ها: مخزن عمومی + اسکن خودکار نشانگر IScopedDependency.</summary>
        private static void AddApplicationServices(IServiceCollection services, SiteSettings siteSettings, IConfiguration configuration)
        {
            // مخزن عمومی ژنریک
            services.AddScoped(typeof(IEFRepository<>), typeof(EFRepository<>));

            // اسکن اسمبلی‌های Data و Services و ثبت هر کلاسی که IScopedDependency دارد.
            // (در نسخه‌ی قبل هیچ‌کدام از این سرویس‌ها ثبت نمی‌شدند و احراز هویت در عمل کار نمی‌کرد!)
            var assemblies = new[]
            {
                typeof(EFRepository<>).Assembly,             // Data
                typeof(Services.JwtService.IJwtService).Assembly // Services
            };

            foreach (var type in assemblies.SelectMany(a => a.GetTypes())
                .Where(t => t is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false }
                            && typeof(IScopedDependency).IsAssignableFrom(t)))
            {
                // قرارداد نام‌گذاری: IUserRepository ← UserRepository و IDataInitializer ← UserDataInitializer
                var interfaces = type.GetInterfaces()
                    .Where(i => i != typeof(IScopedDependency)
                                && i.Name.StartsWith('I')
                                && type.Name.EndsWith(i.Name[1..], StringComparison.Ordinal));

                foreach (var serviceType in interfaces)
                    services.AddScoped(serviceType, type);
            }

            // هش رمز عبور (PBKDF2) بدون نیاز به Identity کامل
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

            // پیامک: فعلاً Mock؛ ارائه‌دهنده‌ی واقعی را اینجا بر اساس SmsSettings.Provider اضافه کنید.
            services.AddScoped<ISmsSender, MockSmsSender>();

            // کپچا: روشن/خاموش از تنظیمات
            if (siteSettings.CaptchaSettings.Enabled)
                services.AddScoped<ICaptchaService, GoogleRecaptchaService>();
            else
                services.AddScoped<ICaptchaService, DisabledCaptchaService>();
        }

        /// <summary>محدودیت نرخ درخواست برای اکشن‌های حساس (درخواست کد پیامکی) بر اساس IP.</summary>
        private static void AddRateLimiting(IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.AddPolicy("otp", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 5,
                            Window = TimeSpan.FromMinutes(10),
                            QueueLimit = 0
                        }));
            });
        }

        /// <summary>Policyهای مجوزدهی؛ به‌جای نقشِ پراکنده در کنترلرها، Policy مرکزی تعریف می‌شود.</summary>
        private static void AddAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("UserManagement", policy => policy.RequireRole("Admin"));
                options.AddPolicy("SupportOrAdmin", policy => policy.RequireRole("Admin", "Support"));
            });
        }
    }
}