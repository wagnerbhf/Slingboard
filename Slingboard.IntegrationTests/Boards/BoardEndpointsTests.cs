using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Slingboard.IntegrationTests.Common;

namespace Slingboard.IntegrationTests.Boards;

[Collection("Integration Tests")]
public class BoardEndpointsTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CreateBoard_ComUsuarioAutenticado_DeveCriarComTresColunasPadrao()
    {
        var token = await RegisterAndLoginAsync("Board Owner", $"boardowner-{Guid.NewGuid()}@teste.com", "SenhaForte123!");
        AuthenticateAs(token);

        var response = await Client.PostAsJsonAsync("/api/v1/boards", new
        {
            title = "Board de Integração",
            description = "Teste ponta a ponta",
            backgroundColor = "#1E3A8A"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        content.GetProperty("columns").GetArrayLength().Should().Be(3);
    }

    [Fact]
    public async Task CreateBoard_SemAutenticacao_DeveRetornar401()
    {
        Client.DefaultRequestHeaders.Authorization = null;

        var response = await Client.PostAsJsonAsync("/api/v1/boards", new { title = "Board Sem Auth" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetBoardById_ComUsuarioNaoMembro_DeveRetornar403()
    {
        var ownerToken = await RegisterAndLoginAsync("Owner", $"owner-{Guid.NewGuid()}@teste.com", "SenhaForte123!");
        AuthenticateAs(ownerToken);

        var createResponse = await Client.PostAsJsonAsync("/api/v1/boards", new { title = "Board Privado" });
        var createdBoard = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var boardId = createdBoard.GetProperty("id").GetString();

        var outsiderToken = await RegisterAndLoginAsync("Outsider", $"outsider-{Guid.NewGuid()}@teste.com", "SenhaForte123!");
        AuthenticateAs(outsiderToken);

        var response = await Client.GetAsync($"/api/v1/boards/{boardId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}