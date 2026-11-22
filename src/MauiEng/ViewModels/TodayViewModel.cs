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
    private readonly MediaLauncher _media;

    public TodayViewModel(ProgressRepository progress, IStudyNavigation navigation, MediaLauncher media)
    {
        _progress = progress;
        _navigation = navigation;
        _media = media;
        Goal = StudyStreak.DailyGoal;
        TasksTotal = StudyStreak.TasksPerDay;
    }

    [ObservableProperty]
    private int streak;

    [ObservableProperty]
    private int reviewsToday;

    [ObservableProperty]
    private int goal;

    [ObservableProperty]
    private int dueCount;

    [ObservableProperty]
    private int tasksDone;

    [ObservableProperty]
    private int tasksTotal;

    [ObservableProperty]
    private double goalProgress;

    [ObservableProperty]
    private string goalText = "0 / 4";

    [ObservableProperty]
    private string cardsText = "Карточки";

    [ObservableProperty]
    private string readingText = "Чтение";

    [ObservableProperty]
    private string watchingText = "Сериал";

    [ObservableProperty]
    private string speakingText = "Проговаривание";

    [ObservableProperty]
    private bool cardsDone;

    [ObservableProperty]
    private bool readingDone;

    [ObservableProperty]
    private bool watchingDone;

    [ObservableProperty]
    private bool speakingDone;

    [ObservableProperty]
    private string bookName = "Книга не выбрана";

    [ObservableProperty]
    private string videoName = "Видео не выбрано";

    [ObservableProperty]
    private string? bookUri;

    [ObservableProperty]
    private string? videoUri;

    [ObservableProperty]
    private string streakCaption = "дней подряд";

    [ObservableProperty]
    private string errorMessage = "";

    [ObservableProperty]
    private bool isBusy;

    public bool HasError => ErrorMessage.Length > 0;

    [RelayCommand]
    private async Task LoadAsync()
    {
        try
        {
            IsBusy = true;
            ErrorMessage = "";
            var snapshot = await _progress.GetTodayAsync(DateOnly.FromDateTime(DateTime.Today));
            Apply(snapshot);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Не удалось загрузить план. {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task StartCardsAsync() => _navigation.OpenAsync(null);

    [RelayCommand]
    private async Task OpenReadingAsync()
    {
        if (string.IsNullOrWhiteSpace(BookUri))
        {
            ErrorMessage = "Сначала выберите книгу.";
            return;
        }

        if (!await _media.OpenAsync(BookUri))
            ErrorMessage = "Не удалось открыть книгу.";
    }

    [RelayCommand]
    private Task CompleteReadingAsync() => CompleteAsync(DayTaskKind.Reading);

    [RelayCommand]
    private async Task OpenWatchingAsync()
    {
        if (string.IsNullOrWhiteSpace(VideoUri))
        {
            ErrorMessage = "Сначала выберите видео.";
            return;
        }

        if (!await _media.OpenAsync(VideoUri))
            ErrorMessage = "Не удалось открыть видео.";
    }

    [RelayCommand]
    private Task CompleteWatchingAsync() => CompleteAsync(DayTaskKind.Watching);

    [RelayCommand]
    private Task StartSpeakingAsync() => _navigation.OpenSpeakAsync();

    [RelayCommand]
    private Task AddWordAsync() => _navigation.OpenAddWordAsync();

    [RelayCommand]
    private Task AddPhraseAsync() => _navigation.OpenAddPhraseAsync();

    [RelayCommand]
    private async Task PickBookAsync()
    {
        try
        {
            var picked = await _media.PickBookAsync();
            if (picked is null)
                return;

            await _progress.SetMediaAsync(MediaKind.Book, picked.Value.Name, picked.Value.Uri);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Не удалось выбрать книгу. {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task PickVideoAsync()
    {
        try
        {
            var picked = await _media.PickVideoAsync();
            if (picked is null)
                return;

            await _progress.SetMediaAsync(MediaKind.Video, picked.Value.Name, picked.Value.Uri);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Не удалось выбрать видео. {ex.Message}";
        }
    }

    private async Task CompleteAsync(DayTaskKind kind)
    {
        try
        {
            IsBusy = true;
            ErrorMessage = "";
            await _progress.MarkTaskDoneAsync(DateOnly.FromDateTime(DateTime.Today), kind);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Не удалось отметить блок. {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void Apply(DaySnapshot snapshot)
    {
        Streak = snapshot.Streak;
        ReviewsToday = snapshot.ReviewsToday;
        Goal = snapshot.Goal;
        DueCount = snapshot.DueCount;
        TasksDone = snapshot.TasksDone;
        TasksTotal = snapshot.TasksTotal;
        CardsDone = snapshot.CardsDone;
        ReadingDone = snapshot.ReadingDone;
        WatchingDone = snapshot.WatchingDone;
        SpeakingDone = snapshot.SpeakingDone;
        BookUri = snapshot.BookUri;
        VideoUri = snapshot.VideoUri;
        BookName = string.IsNullOrWhiteSpace(snapshot.BookName) ? "Книга не выбрана" : snapshot.BookName;
        VideoName = string.IsNullOrWhiteSpace(snapshot.VideoName) ? "Видео не выбрано" : snapshot.VideoName;
        GoalText = $"{snapshot.TasksDone} / {snapshot.TasksTotal}";
        GoalProgress = snapshot.TasksTotal == 0
            ? 0
            : Math.Min(1, (double)snapshot.TasksDone / snapshot.TasksTotal);
        CardsText = snapshot.CardsDone
            ? $"Карточки — готово ({snapshot.ReviewsToday}/{snapshot.Goal})"
            : $"Карточки — {snapshot.ReviewsToday}/{snapshot.Goal}, к повторению {snapshot.DueCount}";
        ReadingText = snapshot.ReadingDone ? "Чтение — готово" : "Чтение — 20–30 мин";
        WatchingText = snapshot.WatchingDone ? "Сериал — готово" : "Сериал — 20–30 мин";
        SpeakingText = snapshot.SpeakingDone ? "Проговаривание — готово" : "Проговаривание — 5 мин";
        StreakCaption = DaysCaption(snapshot.Streak);
    }

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
