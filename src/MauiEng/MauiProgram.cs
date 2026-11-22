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
        builder.Services.AddSingleton<WordRepository>();
        builder.Services.AddSingleton<ProgressRepository>();
        builder.Services.AddSingleton<PhraseRepository>();
        builder.Services.AddSingleton<SpeechService>();
        builder.Services.AddSingleton<MediaLauncher>();
        builder.Services.AddSingleton<IStudyNavigation, StudyNavigation>();
#if ANDROID
        builder.Services.AddSingleton<IStatsWidgetUpdater, AndroidStatsWidgetUpdater>();
#else
        builder.Services.AddSingleton<IStatsWidgetUpdater, NullStatsWidgetUpdater>();
#endif


        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddTransient<TodayViewModel>();
        builder.Services.AddTransient<TodayPage>();
        builder.Services.AddTransient<DecksViewModel>();
        builder.Services.AddTransient<DecksPage>();
        builder.Services.AddTransient<StudyViewModel>();
        builder.Services.AddTransient<StudyPage>();
        builder.Services.AddTransient<SpeakViewModel>();
        builder.Services.AddTransient<SpeakPage>();
        builder.Services.AddTransient<AddWordViewModel>();
        builder.Services.AddTransient<AddWordPage>();
        builder.Services.AddTransient<AddPhraseViewModel>();
        builder.Services.AddTransient<AddPhrasePage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
