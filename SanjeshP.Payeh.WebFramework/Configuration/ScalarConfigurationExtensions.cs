
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using SanjeshP.Payeh.Common;
using Scalar.AspNetCore;

namespace SanjeshP.Payeh.WebFramework.Configuration;

public static class ScalarConfigurationExtensions
{
    private static readonly string[] DocumentVersions = { "v1" };

    private const string DocsTokenCookie =
    "Payeh-DocsToken";

    private const string DocsAuthEndpoint =
        "/scalar/docs-auth";

    private const string OpenApiRoute =
        "/openapi";

    public static void AddOpenApiDocuments(this IServiceCollection services)
    {
        // یک HttpContextAccessor مشترک برای تولید مستندات
        services.RemoveAll<IHttpContextAccessor>();

        var httpContextAccessor = new HttpContextAccessor();

        services.AddSingleton<IHttpContextAccessor>(
            httpContextAccessor);

        foreach (var version in DocumentVersions)
        {
            var documentName = version;

            services.AddOpenApi(documentName, options =>
            {
                // فقط APIهای نسخه موردنظر
                // و APIهای مجاز برای کاربر جاری
                options.ShouldInclude = description =>
                {
                    if (!string.Equals(
                        description.GroupName,
                        documentName,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }

                    var isAuthenticated =
                        httpContextAccessor.HttpContext?
                            .User.Identity?.IsAuthenticated == true;

                    // کاربر واردشده تمام APIهای این نسخه را می‌بیند
                    if (isAuthenticated)
                        return true;

                    // مهمان فقط APIهای عمومی را می‌بیند
                    return !RequiresAuthorization(description);
                };

                // تنظیمات اصلی OpenAPI
                options.AddDocumentTransformer(
                    (document, _, _) =>
                    {
                        document.Info = new OpenApiInfo
                        {
                            Title = "SanjeshP.Payeh API",
                            Version = documentName,
                            Description =
                                "مستندات رسمی API سنجش پایه — همه‌ی پاسخ‌ها در قالب ApiResult هستند."
                        };

                        document.Components ??=
                            new OpenApiComponents();

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

                        // فقط عملیات محافظت‌شده Bearer می‌خواهند
                        foreach (var path in document.Paths.Values)
                        {
                            foreach (var operation in path.Operations)
                            {
                                // اینجا عملیات قبلاً با ShouldInclude
                                // فیلتر شده‌اند؛ اما برای تشخیص
                                // امنیت هر عملیات از OperationTransformer
                                // استفاده می‌کنیم.
                            }
                        }

                        return Task.CompletedTask;
                    });

                options.AddOperationTransformer(
                    (operation, context, _) =>
                    {
                        if (!RequiresAuthorization(
                            context.Description))
                        {
                            return Task.CompletedTask;
                        }

                        operation.Security ??=
                            new List<OpenApiSecurityRequirement>();

                        operation.Security.Add(
                            new OpenApiSecurityRequirement
                            {
                                [new OpenApiSecuritySchemeReference(
                                    "Bearer", context.Document)] =
                                    new List<string>()
                            });

                        return Task.CompletedTask;
                    });
            });
        }
    }

    private static bool RequiresAuthorization(ApiDescription description)
    {
        var metadata =
            description.ActionDescriptor.EndpointMetadata;

        // AllowAnonymous همیشه اولویت دارد
        if (metadata.OfType<IAllowAnonymous>().Any())
            return false;

        // بررسی Authorize روی Controller و Action
        return metadata.OfType<IAuthorizeData>().Any();
    }

    public static WebApplication MapApiDocuments(this WebApplication app)
    {
        var settings = app.Services
            .GetRequiredService<
                Microsoft.Extensions.Options.IOptions<SiteSettings>>()
            .Value.ApiDocsSettings;

        if (!app.Environment.IsDevelopment() && !settings.Enabled)
            return app;

        // مسیر دریافت OpenAPI
        app.MapOpenApi();

        // دریافت توکن از فرم اختصاصی Scalar
        app.MapPost(
            DocsAuthEndpoint,
            async (HttpContext context) =>
            {
                // جلوگیری از درخواست Cross-Origin
                var origin = context.Request.Headers.Origin.ToString();

                if (!Uri.TryCreate(
                    origin,
                    UriKind.Absolute,
                    out var originUri) ||
                    !string.Equals(
                        originUri.Scheme,
                        context.Request.Scheme,
                        StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(
                        originUri.Authority,
                        context.Request.Host.Value,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return Results.StatusCode(
                        StatusCodes.Status403Forbidden);
                }

                var payload =
                    await context.Request.ReadFromJsonAsync<
                        Dictionary<string, string>>();

                if (payload == null ||
                    !payload.TryGetValue(
                        "accessToken", out var accessToken) ||
                    string.IsNullOrWhiteSpace(accessToken) ||
                    accessToken.Length > 16_384)
                {
                    return Results.BadRequest(new
                    {
                        message = "توکن دسترسی معتبر نیست."
                    });
                }

                JwtSecurityToken jwt;

                try
                {
                    // فقط برای تعیین زمان انقضای Cookie
                    // اعتبارسنجی واقعی در JwtBearer انجام می‌شود
                    jwt = new JwtSecurityTokenHandler()
                        .ReadJwtToken(accessToken);
                }
                catch
                {
                    return Results.BadRequest(new
                    {
                        message = "ساختار JWT معتبر نیست."
                    });
                }

                var expiresAt = jwt.ValidTo;
                var remaining = expiresAt - DateTime.UtcNow;

                if (remaining <= TimeSpan.Zero)
                {
                    return Results.BadRequest(new
                    {
                        message = "توکن منقضی شده است."
                    });
                }

                // Cookie فقط برای دریافت OpenAPI ارسال می‌شود
                context.Response.Cookies.Append(
                    DocsTokenCookie,
                    accessToken,
                    new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = context.Request.IsHttps,
                        SameSite = SameSiteMode.Strict,
                        Path = OpenApiRoute,
                        IsEssential = true,
                        Expires = new DateTimeOffset(
                            expiresAt,
                            TimeSpan.Zero),
                        MaxAge = remaining
                    });

                context.Response.Headers.CacheControl = "no-store";

                return Results.Ok(new
                {
                    isSuccess = true,
                    message = "احراز هویت مستندات انجام شد."
                });
            })
            .AllowAnonymous();

        // خروج از مستندات
        app.MapDelete(
            DocsAuthEndpoint,
            (HttpContext context) =>
            {
                context.Response.Cookies.Delete(
                    DocsTokenCookie,
                    new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = context.Request.IsHttps,
                        SameSite = SameSiteMode.Strict,
                        Path = OpenApiRoute
                    });

                context.Response.Headers.CacheControl = "no-store";

                return Results.Ok(new
                {
                    isSuccess = true,
                    message = "از مستندات خارج شدید."
                });
            })
            .AllowAnonymous();

        // صفحه Scalar
        app.MapScalarApiReference(settings.Path, options =>
        {
            options.WithTitle(
                "SanjeshP.Payeh — مستندات API");

            options.WithOpenApiRoutePattern(
                "/openapi/{documentName}.json");

            options.WithCustomCss("""
                .markdown,
                .markdown p,
                .markdown li {
                    unicode-bidi: plaintext;
                    text-align: start;
                }

                .payeh-docs-auth {
                    display: flex;
                    align-items: center;
                    flex-wrap: wrap;
                    gap: 8px;
                    padding: 12px 16px;
                    border-bottom: 1px solid var(--scalar-border-color);
                }

                .payeh-docs-auth input {
                    min-width: 280px;
                    max-width: 480px;
                    padding: 9px;
                    border: 1px solid var(--scalar-border-color);
                    border-radius: 6px;
                }

                .payeh-docs-auth button {
                    padding: 9px 16px;
                    border-radius: 6px;
                    cursor: pointer;
                }

                .payeh-docs-auth-status {
                    font-size: 12px;
                }
                button:focus:not(:focus-visible) {
                    outline: 0;
                }
                .btn-check:active+.btn-info:focus, .btn-check:checked+.btn-info:focus, .btn-info.active:focus, .btn-info:active:focus, .show>.btn-info.dropdown-toggle:focus {
                    box-shadow: 0 0 0 .25rem rgba(11, 172, 204, .5);
                }
                .btn-check:active+.btn-danger:focus, .btn-check:checked+.btn-danger:focus, .btn-danger.active:focus, .btn-danger:active:focus, .show>.btn-danger.dropdown-toggle:focus {
                    box-shadow: 0 0 0 .25rem rgba(225, 83, 97, .5);
                }
                .btn {
                    display: inline-block;
                    font-weight: 400;
                    line-height: 1.5;
                    color: #212529;
                    text-align: center;
                    text-decoration: none;
                    vertical-align: middle;
                    cursor: pointer;
                    -webkit-user-select: none;
                    -moz-user-select: none;
                    user-select: none;
                    background-color: transparent;
                    border: 1px solid transparent;
                    padding: .375rem .75rem;
                    font-size: 1rem;
                    border-radius: .25rem;
                    transition: color .15s ease-in-out, background-color .15s ease-in-out, border-color .15s ease-in-out, box-shadow .15s ease-in-out;
                }   
                .btn-info {
                    color: #000;
                    background-color: #0dcaf0;
                    border-color: #0dcaf0;
                }
                .btn-info:hover {
                    color :#000;
                    background-color: #31d2f2;
                    border-color: #25cff2;
                }
                .btn-danger {
                    color: #fff;
                    background-color: #dc3545;
                    border-color: #dc3545;
                }
                .btn-danger: hover {
                    color:#fff;
                    background-color: #bb2d3b;
                    border-color:#b02a37;
                }
            """);

            // فرم اختصاصی ورود و خروج مستندات
            options.AddHeaderContent("""
                <div class="payeh-docs-auth">
                    <strong>دسترسی به مستندات خصوصی:</strong>

                    <input
                        id="payeh-docs-token"
                        type="password"
                        autocomplete="off"
                        placeholder="Access Token را وارد کنید" />

                    <button
                        id="payeh-docs-login"
                        type="button" class="btn btn-info">
                        ورود و بارگذاری مستندات
                    </button>

                    <button
                        id="payeh-docs-logout"
                        type="button" class="btn btn-danger">
                        خروج
                    </button>

                    <span
                        id="payeh-docs-status"
                        class="payeh-docs-auth-status">
                    </span>
                </div>

                <script>
                (() => {
                    const tokenInput =
                        document.getElementById('payeh-docs-token');

                    const status =
                        document.getElementById('payeh-docs-status');

                    const loginButton =
                        document.getElementById('payeh-docs-login');

                    const logoutButton =
                        document.getElementById('payeh-docs-logout');

                    loginButton.addEventListener('click', async () => {
                        const token = tokenInput.value.trim();

                        if (!token) {
                            status.textContent = 'توکن را وارد کنید.';
                            return;
                        }

                        loginButton.disabled = true;
                        status.textContent = 'در حال احراز هویت...';

                        try {
                            const response = await fetch(
                                '/scalar/docs-auth',
                                {
                                    method: 'POST',
                                    credentials: 'same-origin',
                                    headers: {
                                        'Content-Type': 'application/json'
                                    },
                                    body: JSON.stringify({
                                        accessToken: token
                                    })
                                }
                            );

                            const result = await response.json();

                            if (!response.ok) {
                                throw new Error(
                                    result.message || 'خطا در ورود'
                                );
                            }

                            tokenInput.value = '';
                            status.textContent =
                                'ورود انجام شد؛ در حال بارگذاری...';

                            window.location.reload();
                        }
                        catch (error) {
                            status.textContent =
                                error.message || 'خطا در ارتباط با سرور';
                        }
                        finally {
                            loginButton.disabled = false;
                        }
                    });

                    logoutButton.addEventListener('click', async () => {
                        logoutButton.disabled = true;

                        try {
                            const response = await fetch(
                                '/scalar/docs-auth',
                                {
                                    method: 'DELETE',
                                    credentials: 'same-origin'
                                }
                            );

                            if (!response.ok) {
                                throw new Error('خطا در خروج');
                            }

                            window.location.reload();
                        }
                        catch (error) {
                            status.textContent = error.message;
                            logoutButton.disabled = false;
                        }
                    });
                })();
                </script>
                """);

            foreach (var version in DocumentVersions)
            {
                options.AddDocument(
                    version,
                    $"نسخه‌ی {version}");
            }

            // حفظ احراز هویت برای اجرای APIها در Scalar
            options.WithPersistentAuthentication();
        });

        return app;
    }

    // Middleware انتقال JWT از Cookie به هدر Authorization
    // فقط برای درخواست‌های OpenAPI
    public static IApplicationBuilder
        UseScalarDocsCookieAuthentication(
            this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            if (context.Request.Path.StartsWithSegments(
                OpenApiRoute))
            {
                context.Response.OnStarting(() =>
                {
                    context.Response.Headers.CacheControl =
                        "no-store, no-cache, must-revalidate";

                    context.Response.Headers.Pragma = "no-cache";

                    return Task.CompletedTask;
                });

                if (!context.Request.Headers.ContainsKey(
                        "Authorization") &&
                    context.Request.Cookies.TryGetValue(
                        DocsTokenCookie, out var token) &&
                    !string.IsNullOrWhiteSpace(token))
                {
                    context.Request.Headers.Authorization =
                        $"Bearer {token}";
                }
            }

            await next();
        });

        return app;
    }
}
