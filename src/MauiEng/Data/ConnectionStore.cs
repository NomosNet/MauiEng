namespace MauiEng.Data;

public sealed class ConnectionStore
{
    private const string HostKey = "pg_host";
    private const string PortKey = "pg_port";
    private const string DatabaseKey = "pg_database";
    private const string UsernameKey = "pg_username";
    private const string PasswordKey = "pg_password";

    public bool HasSaved => Preferences.Default.ContainsKey(HostKey);

    public ConnectionSettings Load()
    {
        var preferences = Preferences.Default;
        return new ConnectionSettings
        {
            Host = preferences.Get(HostKey, "localhost"),
            Port = preferences.Get(PortKey, 5432),
            Database = preferences.Get(DatabaseKey, "mauieng"),
            Username = preferences.Get(UsernameKey, "postgres"),
            Password = preferences.Get(PasswordKey, "")
        };
    }

    public void Save(ConnectionSettings settings)
    {
        var preferences = Preferences.Default;
        preferences.Set(HostKey, settings.Host);
        preferences.Set(PortKey, settings.Port);
        preferences.Set(DatabaseKey, settings.Database);
        preferences.Set(UsernameKey, settings.Username);
        preferences.Set(PasswordKey, settings.Password);
    }
}
