using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Slingboard.IntegrationTests.Common;

namespace Slingboard.IntegrationTests.Auth;

[Collection("Integration Tests")]
public class AuthEndpointsTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Register_ComDadosValidos_DeveRetornar201()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            name = "Carlos Teste",
            email = $"carlos-{Guid.NewGuid()}@teste.com",
            password = "SenhaForte123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Register_ComEmailDuplicado_DeveRetornar409()
    {
        var email = $"duplicado-{Guid.NewGuid()}@teste.com";
        var payload = new { name = "Ana Teste", email, password = "SenhaForte123!" };

        await Client.PostAsJsonAsync("/api/v1/auth/register", payload);
        var response = await Client.PostAsJsonAsync("/api/v1/auth/register", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_ComCredenciaisValidas_DeveRetornarAccessToken()
    {
        var email = $"login-{Guid.NewGuid()}@teste.com";
        await Client.PostAsJsonAsync("/api/v1/auth/register", new { name = "Login Teste", email, password = "SenhaForte123!" });

        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "SenhaForte123!" });
        var content = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.GetProperty("accessToken").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_ComSenhaErrada_DeveRetornar401()
    {
        var email = $"senhaerrada-{Guid.NewGuid()}@teste.com";
        await Client.PostAsJsonAsync("/api/v1/auth/register", new { name = "Teste", email, password = "SenhaForte123!" });

        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "SenhaErrada!" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}