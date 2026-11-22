using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiEng.Data;

namespace MauiEng.ViewModels;

public partial class AddPhraseViewModel : ObservableObject
{
    private readonly PhraseRepository _phrases;

    public AddPhraseViewModel(PhraseRepository phrases) => _phrases = phrases;

    [ObservableProperty]
    private string text = "";

    [ObservableProperty]
    private string translation = "";

    [ObservableProperty]
    private string errorMessage = "";

    [ObservableProperty]
    private bool isBusy;

    public bool HasError => ErrorMessage.Length > 0;

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Text))
        {
            ErrorMessage = "Введите фразу.";
            return;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = "";
            await _phrases.AddAsync(
                Text,
                string.IsNullOrWhiteSpace(Translation) ? null : Translation,
                DateOnly.FromDateTime(DateTime.Today));
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
