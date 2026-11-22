using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiEng.Core;
using MauiEng.Data;
using MauiEng.Services;

namespace MauiEng.ViewModels;

public partial class SpeakViewModel : ObservableObject
{
    private readonly PhraseRepository _phrases;
    private readonly ProgressRepository _progress;
    private readonly SpeechService _speech;

    public SpeakViewModel(PhraseRepository phrases, ProgressRepository progress, SpeechService speech)
    {
        _phrases = phrases;
        _progress = progress;
        _speech = speech;
    }

    public ObservableCollection<PhraseItem> Phrases { get; } = [];

    [ObservableProperty]
    private string errorMessage = "";

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool isEmpty;

    public bool HasError => ErrorMessage.Length > 0;

    [RelayCommand]
    private async Task LoadAsync()
    {
        try
        {
            IsBusy = true;
            ErrorMessage = "";
            var items = await _phrases.GetRecentAsync(DateOnly.FromDateTime(DateTime.Today));
            Phrases.Clear();
            foreach (var item in items)
                Phrases.Add(item);
            IsEmpty = Phrases.Count == 0;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Не удалось загрузить фразы. {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task SpeakAsync(PhraseItem? item)
    {
        if (item is null)
            return Task.CompletedTask;
        return _speech.SpeakAsync(item.Text);
    }

    [RelayCommand]
    private async Task CompleteAsync()
    {
        try
        {
            IsBusy = true;
            ErrorMessage = "";
            await _progress.MarkTaskDoneAsync(DateOnly.FromDateTime(DateTime.Today), DayTaskKind.Speaking);
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Не удалось закрыть блок. {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnErrorMessageChanged(string value) => OnPropertyChanged(nameof(HasError));
}
