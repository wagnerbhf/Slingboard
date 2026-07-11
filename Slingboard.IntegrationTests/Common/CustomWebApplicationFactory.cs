using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Slingboard.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Slingboard.IntegrationTests.Common;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:16")
        .WithDatabase("slingboard_test")
        .WithUsername("postgres")
        .WithPassword("postgres_test")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            var testSettings = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _dbContainer.GetConnectionString(),
                ["Jwt:Secret"] = "chave-de-teste-super-secreta-com-mais-de-32-caracteres",
                ["Jwt:Issuer"] = "Slingboard.Tests",
                ["Jwt:Audience"] = "Slingboard.Tests.Client"
            };

            config.AddInMemoryCollection(testSettings);
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        // Aplica as migrations reais no banco do container
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
    }
}