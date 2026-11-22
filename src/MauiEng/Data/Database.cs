using Microsoft.Data.Sqlite;

namespace MauiEng.Data;

public sealed class Database
{
    private readonly string _path;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _ready;

    public Database()
    {
        _path = System.IO.Path.Combine(FileSystem.AppDataDirectory, "mauieng.db");
    }

    public string FilePath => _path;

    public async Task EnsureInitializedAsync(CancellationToken cancellationToken = default)
    {
        if (_ready)
            return;

        await _initLock.WaitAsync(cancellationToken);
        try
        {
            if (_ready)
                return;

            await using var connection = CreateConnection();
            await connection.OpenAsync(cancellationToken);
            await Schema.EnsureAsync(connection, cancellationToken);
            await SeedData.EnsureAsync(connection, cancellationToken);
            _ready = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task<SqliteConnection> OpenAsync(CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);
        var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    private SqliteConnection CreateConnection() => new($"Data Source={_path}");
}
