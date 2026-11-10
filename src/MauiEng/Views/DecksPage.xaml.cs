using MauiEng.Core;
using MauiEng.ViewModels;

namespace MauiEng.Views;

public partial class DecksPage : ContentPage
{
    private readonly DecksViewModel _viewModel;

    public DecksPage(DecksViewModel viewModel)
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

    private async void OnDeckSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not DeckSummary deck)
            return;

        if (sender is CollectionView view)
            view.SelectedItem = null;

        await _viewModel.OpenDeckCommand.ExecuteAsync(deck.Id);
    }
}
