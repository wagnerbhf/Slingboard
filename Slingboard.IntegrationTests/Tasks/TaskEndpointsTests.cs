using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Slingboard.IntegrationTests.Common;

namespace Slingboard.IntegrationTests.Tasks;

[Collection("Integration Tests")]
public class TaskEndpointsTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    private async Task<(string boardId, string todoColumnId, string inProgressColumnId)> CreateBoardWithColumnsAsync()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/boards", new { title = $"Board {Guid.NewGuid()}" });
        var board = await response.Content.ReadFromJsonAsync<JsonElement>();

        var boardId = board.GetProperty("id").GetString()!;
        var columns = board.GetProperty("columns").EnumerateArray().ToList();
        var todoId = columns[0].GetProperty("id").GetString()!;
        var inProgressId = columns[1].GetProperty("id").GetString()!;

        return (boardId, todoId, inProgressId);
    }

    [Fact]
    public async Task CreateTask_ComDadosValidos_DeveRetornar201()
    {
        var token = await RegisterAndLoginAsync("User", $"taskuser-{Guid.NewGuid()}@teste.com", "SenhaForte123!");
        AuthenticateAs(token);
        var (boardId, todoColumnId, _) = await CreateBoardWithColumnsAsync();

        var response = await Client.PostAsJsonAsync($"/api/v1/boards/{boardId}/tasks", new
        {
            columnId = todoColumnId,
            title = "Task de Integração",
            priority = "High"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task MoveTask_ParaOutraColuna_DeveAtualizarColumnId()
    {
        var token = await RegisterAndLoginAsync("User", $"movetask-{Guid.NewGuid()}@teste.com", "SenhaForte123!");
        AuthenticateAs(token);
        var (boardId, todoColumnId, inProgressColumnId) = await CreateBoardWithColumnsAsync();

        var createResponse = await Client.PostAsJsonAsync($"/api/v1/boards/{boardId}/tasks", new
        {
            columnId = todoColumnId,
            title = "Task para mover",
            priority = "Medium"
        });
        var createdTask = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var taskId = createdTask.GetProperty("id").GetString();

        var moveResponse = await Client.PatchAsJsonAsync($"/api/v1/tasks/{taskId}/move", new
        {
            newColumnId = inProgressColumnId,
            newOrder = 0
        });

        moveResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var movedTask = await moveResponse.Content.ReadFromJsonAsync<JsonElement>();
        movedTask.GetProperty("columnId").GetString().Should().Be(inProgressColumnId);
    }

    [Fact]
    public async Task DeleteTask_ComSucesso_DeveRetornar204()
    {
        var token = await RegisterAndLoginAsync("User", $"deletetask-{Guid.NewGuid()}@teste.com", "SenhaForte123!");
        AuthenticateAs(token);
        var (boardId, todoColumnId, _) = await CreateBoardWithColumnsAsync();

        var createResponse = await Client.PostAsJsonAsync($"/api/v1/boards/{boardId}/tasks", new
        {
            columnId = todoColumnId,
            title = "Task para deletar",
            priority = "Low"
        });
        var createdTask = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var taskId = createdTask.GetProperty("id").GetString();

        var deleteResponse = await Client.DeleteAsync($"/api/v1/tasks/{taskId}");

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/api/v1/tasks/{taskId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}