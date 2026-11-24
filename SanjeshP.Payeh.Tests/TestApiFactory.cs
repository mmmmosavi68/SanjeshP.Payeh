using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using SanjeshP.Payeh.Data;
using SanjeshP.Payeh.Entities.Menu;
using SanjeshP.Payeh.Entities.User;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace SanjeshP.Payeh.Tests;

/// <summary>میزبان تست بدون SQL Server: ApplicationDbContext روی EF InMemory و کپچا غیرفعال.</summary>
public sealed class TestApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = "tests-" + Guid.NewGuid();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("Testing", "true");
        builder.UseSetting("SiteSettings:CaptchaSettings:Enabled", "false");
        builder.ConfigureServices(services =>
        {
            var toRemove = services.Where(d => d.ServiceType == typeof(ApplicationDbContext) || (d.ServiceType.IsGenericType && d.ServiceType.GenericTypeArguments.Contains(typeof(ApplicationDbContext)))).ToList();
            foreach (var descriptor in toRemove)
                services.Remove(descriptor);

            services.AddDbContext<ApplicationDbContext>(options => options
                .UseInMemoryDatabase(_databaseName)
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
        });
    }

    public const string Password = "Test@12345";

    public async Task<Guid> CreateUserAsync(string userName, params (string Controller, string Action)[] grantedActions)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

        var user = new User
        {
            UserName = userName,
            NormalizedUserName = userName.ToUpperInvariant(),
            PhoneNumber = "09120000000",
            EmailAddress = userName + "@test.local",
            NormalizedEmailAddress = (userName + "@test.local").ToUpperInvariant(),
            SecurityStamp = Guid.NewGuid(),
            IsActive = true
        };
        user.PasswordHash = hasher.HashPassword(user, Password);
        db.Set<User>().Add(user);

        foreach (var (controller, action) in grantedActions)
        {
            var menu = new Menu { MenuTitle = controller + action, version = "1", ControllerName = controller, ActionName = action, Path = "/" + controller, CssClass = "", Icon = "", Link = "", IsActive = true };
            db.Set<Menu>().Add(menu);
            db.Set<UserAccessMenus>().Add(new UserAccessMenus { UserId = user.Id, MenuId = menu.Id, IsActive = true });
        }

        await db.SaveChangesAsync();
        return user.Id;
    }

    public static async Task<(HttpResponseMessage Response, JsonElement Json)> PostAsync(HttpClient client, string url, object body)
    {
        var response = await client.PostAsJsonAsync(url, body);
        var text = await response.Content.ReadAsStringAsync();
        return (response, string.IsNullOrWhiteSpace(text) ? default : JsonDocument.Parse(text).RootElement.Clone());
    }

    public static async Task<(string AccessToken, string RefreshToken)> LoginAsync(HttpClient client, string userName)
    {
        var (response, json) = await PostAsync(client, "/api/v1/Auth/login", new { userName, password = Password });
        response.EnsureSuccessStatusCode();
        var auth = json.GetProperty("data").GetProperty("auth");
        return (auth.GetProperty("accessToken").GetString()!, auth.GetProperty("refreshToken").GetString()!);
    }

    public static void Bearer(HttpClient client, string token) => client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
}
