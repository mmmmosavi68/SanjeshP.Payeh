using System.Net;
using Xunit;

namespace SanjeshP.Payeh.Tests;

public class AuthIntegrationTests : IClassFixture<TestApiFactory>
{
    private readonly TestApiFactory _factory;

    public AuthIntegrationTests(TestApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Login_Refresh_Logout_FullFlow()
    {
        await _factory.CreateUserAsync("flow_user");
        var client = _factory.CreateClient();

        var (accessToken, refreshToken) = await TestApiFactory.LoginAsync(client, "flow_user");
        Assert.False(string.IsNullOrWhiteSpace(accessToken));

        var (refreshResponse, refreshJson) = await TestApiFactory.PostAsync(client, "/api/v1/Auth/refresh", new { refreshToken });
        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
        var newAccess = refreshJson.GetProperty("data").GetProperty("accessToken").GetString()!;
        var newRefresh = refreshJson.GetProperty("data").GetProperty("refreshToken").GetString()!;
        Assert.NotEqual(refreshToken, newRefresh);

        // استفاده‌ی مجدد از Refresh Token قدیمی باید رد شود.
        var (reuseResponse, _) = await TestApiFactory.PostAsync(client, "/api/v1/Auth/refresh", new { refreshToken });
        Assert.NotEqual(HttpStatusCode.OK, reuseResponse.StatusCode);

        var authed = _factory.CreateClient();
        var (loginAgain, _) = await TestApiFactory.LoginAsync(authed, "flow_user");
        TestApiFactory.Bearer(authed, loginAgain);
        Assert.Equal(HttpStatusCode.OK, (await authed.GetAsync("/api/v1/Auth/me")).StatusCode);

        var logout = await authed.PostAsync("/api/v1/Auth/logout", null);
        Assert.Equal(HttpStatusCode.OK, logout.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await authed.GetAsync("/api/v1/Auth/me")).StatusCode);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsErrorCode()
    {
        await _factory.CreateUserAsync("wrong_pass_user");
        var client = _factory.CreateClient();

        var (response, json) = await TestApiFactory.PostAsync(client, "/api/v1/Auth/login", new { userName = "wrong_pass_user", password = "bad-password-1" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False(json.GetProperty("isSuccess").GetBoolean());
        Assert.Equal("COMMON_CREDENTIALS", json.GetProperty("errorCode").GetString());
    }
}
