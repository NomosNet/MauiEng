using Microsoft.Extensions.DependencyInjection;

namespace MauiEng;

public partial class App : Application
{
    private readonly IServiceProvider _services;

    public App(IServiceProvider services)
    {
        InitializeComponent();
        _services = services;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(_services.GetRequiredService<AppShell>())
        {
            Title = "MauiEng"
        };

        window.Created += (_, _) =>
        {
            if (!OperatingSystem.IsWindows())
                return;

            window.Width = 420;
            window.Height = 860;
        };

        return window;
    }
}
