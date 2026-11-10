using Npgsql;

namespace MauiEng.Data;

public sealed class Database
{
    private string? _connectionString;

    public async Task ConnectAsync(ConnectionSettings settings, CancellationToken cancellationToken = default)
    {
        var connectionString = settings.ToConnectionString();
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await Schema.EnsureAsync(connection, cancellationToken);
        await SeedData.EnsureAsync(connection, cancellationToken);
        _connectionString = connectionString;
    }

    public async Task<NpgsqlConnection> OpenAsync(CancellationToken cancellationToken = default)
    {
        if (_connectionString is null)
            throw new InvalidOperationException("База не подключена.");

        var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
