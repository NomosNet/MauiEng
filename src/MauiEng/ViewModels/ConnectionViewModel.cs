using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiEng.Data;
using MauiEng.Services;

namespace MauiEng.ViewModels;

public partial class ConnectionViewModel : ObservableObject
{
    private readonly Database _database;
    private readonly ConnectionStore _store;
    private readonly IAppNavigator _navigator;

    public ConnectionViewModel(Database database, ConnectionStore store, IAppNavigator navigator)
    {
        _database = database;
        _store = store;
        _navigator = navigator;
    }

    [ObservableProperty]
    public partial string Host { get; set; } = "localhost";

    [ObservableProperty]
    public partial string PortText { get; set; } = "5432";

    [ObservableProperty]
    public partial string DatabaseName { get; set; } = "mauieng";

    [ObservableProperty]
    public partial string Username { get; set; } = "postgres";

    [ObservableProperty]
    public partial string Password { get; set; } = "";

    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = "";

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    public bool HasError => ErrorMessage.Length > 0;

    public async Task TryRestoreAsync()
    {
        var saved = _store.Load();
        Host = saved.Host;
        PortText = saved.Port.ToString();
        DatabaseName = saved.Database;
        Username = saved.Username;
        Password = saved.Password;

        if (_store.HasSaved)
            await ConnectAsync();
    }

    [RelayCommand(CanExecute = nameof(CanConnect))]
    private async Task ConnectAsync()
    {
        ErrorMessage = "";
        var host = Host.Trim();
        var databaseName = DatabaseName.Trim();
        var username = Username.Trim();

        if (host.Length == 0)
        {
            ErrorMessage = "Укажите хост.";
            return;
        }

        if (!int.TryParse(PortText, out var port) || port is < 1 or > 65535)
        {
            ErrorMessage = "Порт должен быть числом от 1 до 65535.";
            return;
        }

        if (databaseName.Length == 0)
        {
            ErrorMessage = "Укажите имя базы.";
            return;
        }

        if (username.Length == 0)
        {
            ErrorMessage = "Укажите пользователя.";
            return;
        }

        var settings = new ConnectionSettings
        {
            Host = host,
            Port = port,
            Database = databaseName,
            Username = username,
            Password = Password
        };

        try
        {
            IsBusy = true;
            await _database.ConnectAsync(settings);
            _store.Save(settings);
            _navigator.ShowShell();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Не удалось подключиться. {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanConnect() => !IsBusy;

    partial void OnErrorMessageChanged(string value) => OnPropertyChanged(nameof(HasError));

    partial void OnIsBusyChanged(bool value) => ConnectCommand.NotifyCanExecuteChanged();
}
