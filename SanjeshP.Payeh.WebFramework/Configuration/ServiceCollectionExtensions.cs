using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Data;

namespace SanjeshP.Payeh.WebFramework.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static void AddMinimalServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SiteSettings>(configuration.GetSection(nameof(SiteSettings)));
            var siteSettings = configuration.GetSection(nameof(SiteSettings)).Get<SiteSettings>();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("SqlServer"));
            });

            services.AddControllers(options =>
            {
                // اعمال فیلتر پاسخ استاندارد JSON
            });

            services.AddJwtAuthentication(siteSettings.JwtSettings);
            services.AddOpenApi(); // OpenAPI داخلی برای .NET 10
        }
    }
}