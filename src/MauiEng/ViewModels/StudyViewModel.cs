using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiEng.Core;
using MauiEng.Data;
using MauiEng.Services;

namespace MauiEng.ViewModels;

public partial class StudyViewModel : ObservableObject
{
    private readonly WordRepository _words;
    private readonly ProgressRepository _progress;
    private readonly SpeechService _speech;
    private readonly List<WordCard> _queue = [];
    private int? _deckId;
    private bool _started;

    public StudyViewModel(WordRepository words, ProgressRepository progress, SpeechService speech)
    {
        _words = words;
        _progress = progress;
        _speech = speech;
    }

    public string DeckId
    {
        set
        {
            if (int.TryParse(value, out var deckId))
                _deckId = deckId;
        }
    }

    [ObservableProperty]
    public partial string Title { get; set; } = "Сегодня";

    [ObservableProperty]
    public partial string Term { get; set; } = "";

    [ObservableProperty]
    public partial string Translation { get; set; } = "";

    [ObservableProperty]
    public partial string Transcription { get; set; } = "";

    [ObservableProperty]
    public partial string Example { get; set; } = "";

    [ObservableProperty]
    public partial string BoxLabel { get; set; } = "";

    [ObservableProperty]
    public partial string RemainingText { get; set; } = "";

    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = "";

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial WordCard? Current { get; set; }

    [ObservableProperty]
    public partial int CardGeneration { get; set; }

    public bool HasCard => Current is not null;

    public bool IsEmpty => !IsLoading && Current is null;

    public bool HasError => ErrorMessage.Length > 0;

    public async Task LoadAsync()
    {
        if (_started)
            return;

        _started = true;
        try
        {
            IsLoading = true;
            ErrorMessage = "";
            var today = DateOnly.FromDateTime(DateTime.Today);
            Title = _deckId is int deckId
                ? await _words.GetDeckNameAsync(deckId) ?? "Колода"
                : "Сегодня";

            var cards = await _words.GetDueAsync(today, _deckId);
            _queue.Clear();
            _queue.AddRange(cards);
            ShowCurrent();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Не удалось открыть занятие. {ex.Message}";
            _queue.Clear();
            ShowCurrent();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanAnswer))]
    private Task RememberAsync() => AnswerAsync(remember: true);

    [RelayCommand(CanExecute = nameof(CanAnswer))]
    private Task ForgotAsync() => AnswerAsync(remember: false);

    [RelayCommand(CanExecute = nameof(CanSpeak))]
    private Task SpeakAsync() => _speech.SpeakAsync(Term);

    private async Task AnswerAsync(bool remember)
    {
        var card = Current;
        if (card is null || IsBusy)
            return;

        try
        {
            IsBusy = true;
            ErrorMessage = "";
            var today = DateOnly.FromDateTime(DateTime.Today);
            var updated = remember
                ? Sm2.ApplyRemember(card.Review, today)
                : Sm2.ApplyForgot(card.Review, today);
            await _words.SaveReviewAsync(card.Id, updated);
            await _progress.AddReviewAsync(today);
            card.Review = updated;

            _queue.RemoveAt(0);
            if (!remember)
                _queue.Add(card);

            ShowCurrent();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Не удалось сохранить ответ. {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ShowCurrent()
    {
        var card = _queue.FirstOrDefault();
        Current = card;
        Term = card?.Term ?? "";
        Translation = card?.Translation ?? "";
        Transcription = card?.Transcription ?? "";
        Example = card?.Example ?? "";
        BoxLabel = card is null ? "" : MemoryBoxes.Label(card.Review);
        RemainingText = card is null ? "" : $"Осталось {_queue.Count}";
        CardGeneration++;
    }

    private bool CanAnswer() => Current is not null && !IsBusy && !IsLoading;

    private bool CanSpeak() => Current is not null && !IsBusy;

    partial void OnCurrentChanged(WordCard? value)
    {
        OnPropertyChanged(nameof(HasCard));
        OnPropertyChanged(nameof(IsEmpty));
        NotifyAnswers();
    }

    partial void OnIsLoadingChanged(bool value)
    {
        OnPropertyChanged(nameof(IsEmpty));
        NotifyAnswers();
    }

    partial void OnIsBusyChanged(bool value) => NotifyAnswers();

    partial void OnErrorMessageChanged(string value) => OnPropertyChanged(nameof(HasError));

    private void NotifyAnswers()
    {
        RememberCommand.NotifyCanExecuteChanged();
        ForgotCommand.NotifyCanExecuteChanged();
        SpeakCommand.NotifyCanExecuteChanged();
    }
}
