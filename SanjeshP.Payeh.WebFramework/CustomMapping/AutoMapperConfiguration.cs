
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

            // شناسایی Profileهای معمولی با سازنده بدون پارامتر
            // CustomMappingProfile جداگانه ساخته می‌شود.
            var profileTypes = assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type =>
                    type is { IsClass: true, IsAbstract: false } &&
                    typeof(Profile).IsAssignableFrom(type) &&
                    type != typeof(CustomMappingProfile) &&
                    type.GetConstructor(Type.EmptyTypes) != null)
                .Distinct()
                .ToArray();

            services.AddAutoMapper(cfg =>
            {
                if (!string.IsNullOrWhiteSpace(licenseKey))
                    cfg.LicenseKey = licenseKey;

                // اول مپ‌های خودکار BaseDto
                cfg.AddProfile(new CustomMappingProfile(
                    mappingTypes.Select(type =>
                        (IHaveCustomMapping)Activator.CreateInstance(type)!)));

                // بعد پروفایل‌های دستی، تا در تداخل برنده باشند
                foreach (var profileType in profileTypes)
                    cfg.AddProfile((Profile)Activator.CreateInstance(profileType)!);
            });
        }

    }
}
