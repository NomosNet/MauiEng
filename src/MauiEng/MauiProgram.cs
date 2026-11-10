using MauiEng.Data;
using MauiEng.Services;
using MauiEng.ViewModels;
using MauiEng.Views;
using Microsoft.Extensions.Logging;

namespace MauiEng;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<Database>();
        builder.Services.AddSingleton<ConnectionStore>();
        builder.Services.AddSingleton<WordRepository>();
        builder.Services.AddSingleton<ProgressRepository>();
        builder.Services.AddSingleton<SpeechService>();
        builder.Services.AddSingleton<IAppNavigator, AppNavigator>();
        builder.Services.AddSingleton<IStudyNavigation, StudyNavigation>();

        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddTransient<ConnectionViewModel>();
        builder.Services.AddTransient<ConnectionPage>();
        builder.Services.AddTransient<TodayViewModel>();
        builder.Services.AddTransient<TodayPage>();
        builder.Services.AddTransient<DecksViewModel>();
        builder.Services.AddTransient<DecksPage>();
        builder.Services.AddTransient<StudyViewModel>();
        builder.Services.AddTransient<StudyPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
