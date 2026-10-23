using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Services.JwtService;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SanjeshP.Payeh.WebFramework.Configuration
{
    /// <summary>
    /// پیکربندی احراز هویت JWT.
    /// تغییر مهم: اعتبارسنجی هر درخواست بر اساس «شناسه‌ی نشست» (claim به نام session_id)
    /// انجام می‌شود، نه jti؛ چون در Refresh چرخشی، jti عوض می‌شود ولی نشست همان است
    /// و Access Tokenهای قبلی باید تا انقضای طبیعی خودشان معتبر بمانند.
    /// </summary>
    public static class IdentityConfigurationExtensions
    {
        public static void AddJwtAuthentication(this IServiceCollection services, JwtSettings jwtSettings)
        {
            // نام claimها بدون بازنویسی مایکروسافتی نگه داشته شود.
            //JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    NameClaimType = ClaimTypes.Name,
                    RoleClaimType = ClaimTypes.Role
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var sessionIdStr = context.Principal?.FindFirst(IJwtService.SessionIdClaimType)?.Value;
                        var userIdStr = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                        if (!Guid.TryParse(sessionIdStr, out var sessionId) || !Guid.TryParse(userIdStr, out var userId))
                        {
                            context.Fail("توکن فاقد شناسه‌ی نشست معتبر است.");
                            return;
                        }

                        var jwtService = context.HttpContext.RequestServices.GetRequiredService<IJwtService>();
                        var isValid = await jwtService.ValidateSessionAsync(sessionId, userId, context.HttpContext.RequestAborted);
                        if (!isValid)
                            context.Fail("نشست باطل یا منقضی شده است.");
                    }
                };
            });
        }
    }
}