using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiEng.Core;
using MauiEng.Data;
using MauiEng.Services;

namespace MauiEng.ViewModels;

public partial class TodayViewModel : ObservableObject
{
    private readonly ProgressRepository _progress;
    private readonly IStudyNavigation _navigation;

    public TodayViewModel(ProgressRepository progress, IStudyNavigation navigation)
    {
        _progress = progress;
        _navigation = navigation;
        Goal = StudyStreak.DailyGoal;
    }

    [ObservableProperty]
    public partial int Streak { get; set; }

    [ObservableProperty]
    public partial int ReviewsToday { get; set; }

    [ObservableProperty]
    public partial int Goal { get; set; }

    [ObservableProperty]
    public partial int DueCount { get; set; }

    [ObservableProperty]
    public partial double GoalProgress { get; set; }

    [ObservableProperty]
    public partial string GoalText { get; set; } = "0 / 20";

    [ObservableProperty]
    public partial string DueText { get; set; } = "";

    [ObservableProperty]
    public partial string StreakCaption { get; set; } = "дней подряд";

    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = "";

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    public bool HasError => ErrorMessage.Length > 0;

    [RelayCommand]
    private async Task LoadAsync()
    {
        try
        {
            IsBusy = true;
            ErrorMessage = "";
            var snapshot = await _progress.GetTodayAsync(DateOnly.FromDateTime(DateTime.Today));
            Streak = snapshot.Streak;
            ReviewsToday = snapshot.ReviewsToday;
            Goal = snapshot.Goal;
            DueCount = snapshot.DueCount;
            GoalText = $"{snapshot.ReviewsToday} / {snapshot.Goal}";
            DueText = snapshot.DueCount == 0
                ? "На сегодня всё"
                : $"К повторению: {snapshot.DueCount}";
            GoalProgress = snapshot.Goal == 0
                ? 0
                : Math.Min(1, (double)snapshot.ReviewsToday / snapshot.Goal);
            StreakCaption = DaysCaption(snapshot.Streak);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Не удалось загрузить прогресс. {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task StartAsync() => _navigation.OpenAsync(null);

    partial void OnErrorMessageChanged(string value) => OnPropertyChanged(nameof(HasError));

    private static string DaysCaption(int count)
    {
        var lastTwo = count % 100;
        if (lastTwo is >= 11 and <= 14)
            return "дней подряд";

        return (count % 10) switch
        {
            1 => "день подряд",
            2 or 3 or 4 => "дня подряд",
            _ => "дней подряд"
        };
    }
}
