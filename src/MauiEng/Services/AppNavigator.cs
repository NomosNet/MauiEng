using Microsoft.Extensions.DependencyInjection;

namespace MauiEng.Services;

public sealed class AppNavigator : IAppNavigator
{
    private readonly IServiceProvider _services;

    public AppNavigator(IServiceProvider services) => _services = services;

    public void ShowShell()
    {
        var shell = _services.GetRequiredService<AppShell>();
        var window = Application.Current?.Windows.FirstOrDefault();
        if (window is null)
            throw new InvalidOperationException("Окно приложения ещё не создано.");

        window.Page = shell;
    }
}
