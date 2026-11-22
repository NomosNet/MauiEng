using MauiEng.ViewModels;

namespace MauiEng.Views;

public partial class AddPhrasePage : ContentPage
{
    public AddPhrasePage(AddPhraseViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
