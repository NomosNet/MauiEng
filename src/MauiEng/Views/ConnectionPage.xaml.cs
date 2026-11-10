using MauiEng.ViewModels;

namespace MauiEng.Views;

public partial class ConnectionPage : ContentPage
{
    private readonly ConnectionViewModel _viewModel;
    private bool _started;

    public ConnectionPage(ConnectionViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_started)
            return;

        _started = true;
        await _viewModel.TryRestoreAsync();
    }
}
