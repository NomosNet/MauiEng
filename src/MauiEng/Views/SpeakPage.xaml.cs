using MauiEng.ViewModels;

namespace MauiEng.Views;

public partial class SpeakPage : ContentPage
{
    private readonly SpeakViewModel _viewModel;

    public SpeakPage(SpeakViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadCommand.ExecuteAsync(null);
    }
}
