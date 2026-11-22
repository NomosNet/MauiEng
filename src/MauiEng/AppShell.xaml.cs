using MauiEng.Views;

namespace MauiEng;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(StudyPage), typeof(StudyPage));
        Routing.RegisterRoute(nameof(SpeakPage), typeof(SpeakPage));
        Routing.RegisterRoute(nameof(AddWordPage), typeof(AddWordPage));
        Routing.RegisterRoute(nameof(AddPhrasePage), typeof(AddPhrasePage));
    }
}
