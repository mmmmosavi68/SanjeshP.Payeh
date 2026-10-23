
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace SanjeshP.Payeh.WebFramework.CustomMapping
{
    public static class AutoMapperConfiguration
    {
        public static void InitializeAutoMapper(
            this IServiceCollection services,
            IConfiguration configuration,
            params Assembly[] assemblies)
        {
            var licenseKey = configuration["AutoMapper:LicenseKey"];

            // ثبت خودکار پیاده‌سازی‌های IHaveCustomMapping
            var mappingTypes = assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type =>
                    type is { IsClass: true, IsAbstract: false } &&
                    typeof(IHaveCustomMapping).IsAssignableFrom(type))
                .Distinct()
                .ToArray();

            foreach (var mappingType in mappingTypes)
            {
                services.AddTransient(
                    typeof(IHaveCustomMapping),
                    mappingType);
            }

            // ثبت Profile به‌صورت مستقیم؛ از AddMaps استفاده نمی‌کنیم،
            // چون CustomMappingProfile سازنده پارامتردار دارد.
            services.AddAutoMapper(cfg =>
            {
                if (!string.IsNullOrWhiteSpace(licenseKey))
                {
                    cfg.LicenseKey = licenseKey;
                }

                cfg.AddProfile(new CustomMappingProfile(
                    mappingTypes.Select(type =>
                        (IHaveCustomMapping)Activator.CreateInstance(type)!)
                ));
            });

        }
    }
}
