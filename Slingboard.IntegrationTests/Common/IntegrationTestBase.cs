using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Slingboard.IntegrationTests.Common;

public abstract class IntegrationTestBase(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly HttpClient Client = factory.CreateClient();

    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    protected async Task<string> RegisterAndLoginAsync(string name, string email, string password)
    {
        await Client.PostAsJsonAsync("/api/v1/auth/register", new { name, email, password });

        var loginResponse = await Client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        loginResponse.EnsureSuccessStatusCode();

        var content = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        return content.GetProperty("accessToken").GetString()!;
    }

    protected void AuthenticateAs(string accessToken)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }
}