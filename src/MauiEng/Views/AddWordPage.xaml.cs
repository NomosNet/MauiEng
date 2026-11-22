using MauiEng.ViewModels;

namespace MauiEng.Views;

public partial class AddWordPage : ContentPage
{
    public AddWordPage(AddWordViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
