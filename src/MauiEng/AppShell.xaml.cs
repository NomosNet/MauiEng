using MauiEng.Views;

namespace MauiEng;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(StudyPage), typeof(StudyPage));
    }
}
