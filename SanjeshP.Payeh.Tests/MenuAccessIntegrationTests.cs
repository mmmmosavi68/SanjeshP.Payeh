using System.Net;
using Xunit;

namespace SanjeshP.Payeh.Tests;

public class MenuAccessIntegrationTests : IClassFixture<TestApiFactory>
{
    private readonly TestApiFactory _factory;

    public MenuAccessIntegrationTests(TestApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Anonymous_IsUnauthorized()
    {
        var response = await _factory.CreateClient().GetAsync("/api/v1/Menus");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AuthenticatedWithoutMenuAccess_IsForbidden()
    {
        await _factory.CreateUserAsync("no_menu_user");
        var client = _factory.CreateClient();
        TestApiFactory.Bearer(client, (await TestApiFactory.LoginAsync(client, "no_menu_user")).AccessToken);

        var response = await client.GetAsync("/api/v1/Menus");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AuthenticatedWithMenuAccess_IsAllowed_AndPagingHeadersAreAdditive()
    {
        await _factory.CreateUserAsync("menu_user", ("Menus", "Get"));
        var client = _factory.CreateClient();
        TestApiFactory.Bearer(client, (await TestApiFactory.LoginAsync(client, "menu_user")).AccessToken);

        var all = await client.GetAsync("/api/v1/Menus");
        Assert.Equal(HttpStatusCode.OK, all.StatusCode);
        Assert.False(all.Headers.Contains("X-Total-Count"));

        var paged = await client.GetAsync("/api/v1/Menus?page=1&pageSize=1");
        Assert.Equal(HttpStatusCode.OK, paged.StatusCode);
        Assert.True(paged.Headers.Contains("X-Total-Count"));
    }
}
