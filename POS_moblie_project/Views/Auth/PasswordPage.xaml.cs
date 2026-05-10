using POS_moblie_project.ViewModels;

namespace POS_moblie_project.Views.Auth;

public partial class PasswordPage : ContentPage
{
    private readonly PasswordViewModel _viewModel;
    private HorizontalStackLayout _pinDotsLayout;

    public PasswordPage(PasswordViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        // Resolve the named element after InitializeComponent
        _pinDotsLayout = this.FindByName<HorizontalStackLayout>("PinDotsLayout");

        _viewModel.OnError += HandleError;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeCommand.ExecuteAsync(null);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.OnError -= HandleError;
    }

    private async void HandleError(string _)
    {
        await ShakePinDotsAsync();
    }

    private async Task ShakePinDotsAsync()
    {
        if (_pinDotsLayout == null) return;

        for (int i = 0; i < 3; i++)
        {
            await _pinDotsLayout.TranslateTo(-8, 0, 50);
            await _pinDotsLayout.TranslateTo(8, 0, 50);
        }
        await _pinDotsLayout.TranslateTo(0, 0, 50);
    }
}