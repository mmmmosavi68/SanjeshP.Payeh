using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using SanjeshP.Payeh.Common;
using Scalar.AspNetCore;

namespace SanjeshP.Payeh.WebFramework.Configuration;

public static class ScalarConfigurationExtensions
{
    private static readonly string[] DocumentVersions = { "v1" };

    public static void AddOpenApiDocuments(this IServiceCollection services)
    {
        foreach (var version in DocumentVersions)
        {
            var documentName = version;

            services.AddOpenApi(documentName, options =>
            {
 //               options.ShouldInclude = description =>
 //string.Equals(
 //    description.GroupName,
 //    documentName,
 //    StringComparison.OrdinalIgnoreCase);

                options.AddDocumentTransformer((document, _, _) =>
                {
                    document.Info = new OpenApiInfo
                    {
                        Title = "SanjeshP.Payeh API",
                        Version = documentName,
                        Description =
                            "مستندات رسمی API سنجش پایه — همه‌ی پاسخ‌ها در قالب ApiResult هستند."
                    };

                    document.Components ??= new OpenApiComponents();

                    document.Components.SecuritySchemes ??=
                        new Dictionary<string, IOpenApiSecurityScheme>();

                    document.Components.SecuritySchemes["Bearer"] =
                        new OpenApiSecurityScheme
                        {
                            Type = SecuritySchemeType.Http,
                            Scheme = "bearer",
                            BearerFormat = "JWT",
                            In = ParameterLocation.Header,
                            Description =
                                "توکن دسترسی را وارد کنید."
                        };

                    foreach (var operation in document.Paths.Values
                                 .SelectMany(path => path.Operations))
                    {
                        operation.Value.Security ??=
                            new List<OpenApiSecurityRequirement>();

                        operation.Value.Security.Add(
                            new OpenApiSecurityRequirement
                            {
                                [new OpenApiSecuritySchemeReference(
                                    "Bearer", document)] =
                                    new List<string>()
                            });
                    }

                    return Task.CompletedTask;
                });
            });
        }
    }

    public static WebApplication MapApiDocuments(this WebApplication app)
    {
        var settings = app.Services
            .GetRequiredService<
                Microsoft.Extensions.Options.IOptions<SiteSettings>>()
            .Value.ApiDocsSettings;

        if (!app.Environment.IsDevelopment() && !settings.Enabled)
            return app;

        app.MapOpenApi();

        app.MapScalarApiReference(settings.Path, options =>
        {
            options.WithTitle("SanjeshP.Payeh — مستندات API");
            options.WithOpenApiRoutePattern("/openapi/{documentName}.json");

            foreach (var version in DocumentVersions)
                options.AddDocument(version, $"نسخه‌ی {version}");

            options.WithPersistentAuthentication();
        });

        return app;
    }
}