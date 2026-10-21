using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Services.JwtService;

namespace SanjeshP.Payeh.WebFramework.Configuration
{
    public static class IdentityConfigurationExtensions
    {
        public static void AddJwtAuthentication(this IServiceCollection services, JwtSettings jwtSettings)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var secretKey = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);
                var issuerSigningKey = new SymmetricSecurityKey(secretKey);

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = issuerSigningKey,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var jwtService = context.HttpContext.RequestServices.GetRequiredService<IJwtService>();
                        var jti = context.Principal?.Claims.FirstOrDefault(x => x.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti)?.Value;
                        var userIdStr = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                        if (string.IsNullOrEmpty(jti) || !Guid.TryParse(userIdStr, out var userId))
                        {
                            context.Fail("Unauthorized");
                            return;
                        }

                        var isValid = await jwtService.ValidateTokenAsync(jti, userId, context.HttpContext.RequestAborted);
                        if (!isValid)
                        {
                            context.Fail("Token is revoked or expired.");
                        }
                    }
                };
            });
        }
    }
}