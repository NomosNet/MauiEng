using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiEng.Data;

namespace MauiEng.ViewModels;

public partial class AddWordViewModel : ObservableObject
{
    private readonly WordRepository _words;

    public AddWordViewModel(WordRepository words) => _words = words;

    [ObservableProperty]
    private string term = "";

    [ObservableProperty]
    private string translation = "";

    [ObservableProperty]
    private string transcription = "";

    [ObservableProperty]
    private string example = "";

    [ObservableProperty]
    private string errorMessage = "";

    [ObservableProperty]
    private bool isBusy;

    public bool HasError => ErrorMessage.Length > 0;

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Term) || string.IsNullOrWhiteSpace(Translation))
        {
            ErrorMessage = "Нужны слово и перевод.";
            return;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = "";
            var today = DateOnly.FromDateTime(DateTime.Today);
            var deckId = await _words.GetOrCreateDeckIdAsync("Из книг");
            await _words.AddWordAsync(
                deckId,
                Term,
                Translation,
                string.IsNullOrWhiteSpace(Transcription) ? null : Transcription,
                string.IsNullOrWhiteSpace(Example) ? null : Example,
                today);
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Не удалось сохранить. {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnErrorMessageChanged(string value) => OnPropertyChanged(nameof(HasError));
}
