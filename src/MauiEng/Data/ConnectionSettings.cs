using Npgsql;

namespace MauiEng.Data;

public sealed class ConnectionSettings
{
    public string Host { get; set; } = "localhost";

    public int Port { get; set; } = 5432;

    public string Database { get; set; } = "mauieng";

    public string Username { get; set; } = "postgres";

    public string Password { get; set; } = "";

    public string ToConnectionString()
    {
        return new NpgsqlConnectionStringBuilder
        {
            Host = Host.Trim(),
            Port = Port,
            Database = Database.Trim(),
            Username = Username.Trim(),
            Password = Password,
            Timeout = 8,
            CommandTimeout = 30,
            SslMode = SslMode.Disable
        }.ConnectionString;
    }
}
