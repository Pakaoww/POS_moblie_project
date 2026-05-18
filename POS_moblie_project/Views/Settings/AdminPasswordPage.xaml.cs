using POS_moblie_project.ViewModels;

namespace POS_moblie_project.Views.Settings;

public partial class AdminPasswordPage : ContentPage
{
    private readonly AdminPasswordViewModel _viewModel;

    public AdminPasswordPage(AdminPasswordViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.InitializeCommand.Execute(null);
    }

    // Shake animation เมื่อ PIN ผิด
    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        if (_viewModel is not null)
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private async void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AdminPasswordViewModel.IsError) && _viewModel.IsError)
        {
            await ShakePinDotsAsync();
        }
    }

    private async Task ShakePinDotsAsync()
    {
        for (int i = 0; i < 3; i++)
        {
            await PinDotsLayout.TranslateTo(-8, 0, 50);
            await PinDotsLayout.TranslateTo(8, 0, 50);
        }
        await PinDotsLayout.TranslateTo(0, 0, 50);
    }
}