using System.Data.Common;
using EntityFramework.Exceptions.SqlServer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using realworld_net.Data;
using Respawn;
using Testcontainers.MsSql;

namespace realworld_net.Tests;

public class DbFixture : IAsyncLifetime
{
    public MsSqlContainer MsSqlContainer { get; } = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
    public string ConnectionString { get; private set; } = string.Empty;
    public Respawner DbRespawner { get; private set; } = null!;

    public async Task DisposeAsync() => await MsSqlContainer.DisposeAsync();
    public async Task InitializeAsync()
    {
        await MsSqlContainer.StartAsync();
        ConnectionString = MsSqlContainer.GetConnectionString();

        await using var context = CreateContext();
        await context.Database.MigrateAsync();

        using DbConnection connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();

        DbRespawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer
        });
    }

    public async Task ResetAsync()
    {
        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        await DbRespawner.ResetAsync(connection);
    }

    public AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(ConnectionString)
            .UseExceptionProcessor()
            .Options);

}
