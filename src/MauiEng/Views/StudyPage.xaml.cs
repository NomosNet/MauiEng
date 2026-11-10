using System.ComponentModel;
using MauiEng.ViewModels;

namespace MauiEng.Views;

[QueryProperty(nameof(DeckId), "deckId")]
public partial class StudyPage : ContentPage
{
    private readonly StudyViewModel _viewModel;
    private bool _showingBack;
    private bool _animating;
    private bool _loadQueued;
    private int _flipVersion;

    public StudyPage(StudyViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    public string DeckId
    {
        set => _viewModel.DeckId = value;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_loadQueued)
            return;

        _loadQueued = true;
        Dispatcher.Dispatch(async () => await _viewModel.LoadAsync());
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(StudyViewModel.CardGeneration))
            return;

        if (MainThread.IsMainThread)
            ResetFaces();
        else
            MainThread.BeginInvokeOnMainThread(ResetFaces);
    }

    private async void OnCardTapped(object? sender, TappedEventArgs e)
    {
        if (_animating || !_viewModel.HasCard)
            return;

        _animating = true;
        var version = _flipVersion;
        try
        {
            var outgoing = _showingBack ? BackFace : FrontFace;
            var incoming = _showingBack ? FrontFace : BackFace;
            await outgoing.RotateYToAsync(90, 160, Easing.Linear);
            if (version != _flipVersion)
                return;

            outgoing.IsVisible = false;
            incoming.RotationY = -90;
            incoming.IsVisible = true;
            await incoming.RotateYToAsync(0, 160, Easing.Linear);
            if (version != _flipVersion)
                return;

            _showingBack = !_showingBack;
        }
        finally
        {
            if (version == _flipVersion)
                _animating = false;
        }
    }

    private void ResetFaces()
    {
        _flipVersion++;
        _animating = false;
        _showingBack = false;
        FrontFace.CancelAnimations();
        BackFace.CancelAnimations();
        FrontFace.RotationY = 0;
        BackFace.RotationY = 0;
        FrontFace.IsVisible = true;
        BackFace.IsVisible = false;
    }
}
