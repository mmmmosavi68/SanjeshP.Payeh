using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Common.Exceptions;
using SanjeshP.Payeh.Common.IdentityExtensions;
using SanjeshP.Payeh.Common.Utilities;
using SanjeshP.Payeh.Data;
using SanjeshP.Payeh.Data.Contracts;
using Microsoft.AspNetCore.Mvc.Versioning;

namespace SanjeshP.Payeh.WebFramework.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static void AddDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options
                    .UseSqlServer(configuration.GetConnectionString("SqlServer"));
            });
        }

        public static void AddJwtAuthentication(this IServiceCollection services, JwtSettings jwtSettings)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                var secret_key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);
                var encryption_key = Encoding.UTF8.GetBytes(jwtSettings.EncryptKey);

                var validationParameters = new TokenValidationParameters
                {
                    ClockSkew = TimeSpan.Zero, // default: 5 min
                    RequireSignedTokens = true,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(secret_key),

                    RequireExpirationTime = true,
                    ValidateLifetime = true,

                    ValidateAudience = true, //default : false
                    ValidAudience = jwtSettings.Audience,

                    ValidateIssuer = true, //default : false
                    ValidIssuer = jwtSettings.Issuer,

                    TokenDecryptionKey = new SymmetricSecurityKey(encryption_key)
                };

                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = validationParameters;
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        //var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(JwtBearerEvents));
                        //logger.LogError("Authentication failed.", context.Exception);

                        if (context.Exception != null)
                            throw new AppException(ApiResultStatusCode.UnAuthorized, "Authentication failed.", HttpStatusCode.Unauthorized, context.Exception, null);

                        return Task.CompletedTask;
                    },
                    OnTokenValidated = async context =>
                    {
                        //var applicationSignInManager = context.HttpContext.RequestServices.GetRequiredService<IApplicationSignInManager>();
                        var userRepository = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
                        var userTokenRepository = context.HttpContext.RequestServices.GetRequiredService<IUserTokenRepository>();
                        var View_UserMenubarRepository = context.HttpContext.RequestServices.GetRequiredService<IView_UserMenubarRepository>();

                        var claimsIdentity = context.Principal.Identity as ClaimsIdentity;
                        if (claimsIdentity.Claims?.Any() != true)
                            context.Fail(failureMessage: "This token has no claims.");

                        var tokenId = claimsIdentity.GetUserId();

                        if (!tokenId.HasValue())
                            context.Fail(failureMessage: "This token has no secret key");

                        var userToken = await userTokenRepository.GetByIdAsync(context.HttpContext.RequestAborted, new Guid(tokenId));
                        if ((userToken.ExpirationDate - DateTime.Now).TotalSeconds <= 0 || userToken.IsDelete == true)
                            context.Fail(failureMessage: "The token is invalid.");

                        //Find user and token from database and perform your custom validation
                        var user = await userRepository.GetByIdAsync(context.HttpContext.RequestAborted, userToken.UserId);

                        #region When use user data
                        //var securityStamp = claimsIdentity.FindFirstValue(new ClaimsIdentityOptions().SecurityStampClaimType);
                        //if (!securityStamp.HasValue())
                        //    context.Fail(failureMessage: "This token has no security stamp");

                        //if (user.SecurityStamp != Guid.Parse(securityStamp))
                        //    context.Fail(failureMessage: "Token security stamp is not valid.");
                        #endregion

                        //var securityStamp = claimsIdentity.FindFirstValue(new ClaimsIdentityOptions().SecurityStampClaimType);


                        //var validatedUser = await applicationSignInManager.ValidateSecurityStampAsync(context.Principal);
                        //if (validatedUser == null)
                        //    context.Fail("Token security stamp is not valid.");

                        if (!user.IsActive)
                            context.Fail(failureMessage: "User is not active.");

                        #region When use user data
                        //await userRepository.UpdateLastLoginDateAsync(user, context.HttpContext.RequestAborted);
                        #endregion

                        string controllerName = context.Request.RouteValues["controller"].ToString();
                        string actionName = context.Request.RouteValues["action"].ToString();
                        var Check_Permission = false;
                        if (controllerName == "User" && actionName == "GetUserAccess")
                        {
                            Check_Permission = true;
                        }
                        else
                        {
                            Check_Permission = await View_UserMenubarRepository.GetUserMenuByControllerNameAndActionName(user.Id, controllerName, actionName, context.HttpContext.RequestAborted);
                        }
                        if (!Check_Permission)
                            context.Fail(failureMessage: "The user does not have access to this menu");

                    },
                    OnChallenge = context =>
                    {
                        //var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(JwtBearerEvents));
                        //logger.LogError("OnChallenge error", context.Error, context.ErrorDescription);

                        if (context.AuthenticateFailure != null)
                            throw new AppException(ApiResultStatusCode.UnAuthorized, context.AuthenticateFailure.Message, HttpStatusCode.Unauthorized, context.AuthenticateFailure, null);
                        throw new AppException(ApiResultStatusCode.UnAuthorized, "You are unauthorized to access this resource.", HttpStatusCode.Unauthorized);

                        //return Task.CompletedTask;
                    }
                };
            });
        }
        public static void AddCustomApiVersioning(this IServiceCollection services)
        {
            services.AddApiVersioning(options =>
            {
                //url segment => {version}
                options.AssumeDefaultVersionWhenUnspecified = true; //default => false;
                options.DefaultApiVersion = new ApiVersion(1, 0); //v1.0 == v1
                options.ReportApiVersions = true;

                ApiVersion.TryParse("1.0", out var version10);
                ApiVersion.TryParse("1", out var version1);
                var a = version10 == version1;

                //options.ApiVersionReader = new QueryStringApiVersionReader("api-version");
                // api/posts?api-version=1

                //options.ApiVersionReader = new UrlSegmentApiVersionReader();   //default
                // api/v1/posts

                //options.ApiVersionReader = new HeaderApiVersionReader(new[] { "Api-Version" });
                // header => Api-Version : 1

                //options.ApiVersionReader = new MediaTypeApiVersionReader()

                options.ApiVersionReader = ApiVersionReader.Combine(new QueryStringApiVersionReader("api-version"), new UrlSegmentApiVersionReader());
                // combine of [querystring] & [urlsegment]

                options.ReportApiVersions = true;
            });
        }
    }
}
