using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiEng.Core;
using MauiEng.Data;
using MauiEng.Services;

namespace MauiEng.ViewModels;

public partial class DecksViewModel : ObservableObject
{
    private readonly WordRepository _words;
    private readonly IStudyNavigation _navigation;

    public DecksViewModel(WordRepository words, IStudyNavigation navigation)
    {
        _words = words;
        _navigation = navigation;
    }

    public ObservableCollection<DeckSummary> Decks { get; } = [];

    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = "";

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial bool IsEmpty { get; set; }

    public bool HasError => ErrorMessage.Length > 0;

    [RelayCommand]
    private async Task LoadAsync()
    {
        try
        {
            IsBusy = true;
            ErrorMessage = "";
            var decks = await _words.GetDecksAsync(DateOnly.FromDateTime(DateTime.Today));
            Decks.Clear();
            foreach (var deck in decks)
                Decks.Add(deck);
            IsEmpty = Decks.Count == 0;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Не удалось загрузить колоды. {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task OpenDeckAsync(int deckId) => _navigation.OpenAsync(deckId);

    partial void OnErrorMessageChanged(string value) => OnPropertyChanged(nameof(HasError));
}
