using POS_moblie_project.ViewModels.Settings;

namespace POS_moblie_project.Views.Settings;

public partial class AdminManagePasswordPage : ContentPage
{
    private readonly AdminManagePasswordViewModel _viewModel;

    public AdminManagePasswordPage(AdminManagePasswordViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        _viewModel.OnPinError += HandlePinError;
        _viewModel.OnStepChanged += HandleStepChanged;
        _viewModel.OnPinSuccess += HandlePinSuccess;
    }

    // ── Lifecycle ─────────────────────────────────────────

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.OnPinError -= HandlePinError;
        _viewModel.OnStepChanged -= HandleStepChanged;
        _viewModel.OnPinSuccess -= HandlePinSuccess;
    }

    // ── Event Handlers ────────────────────────────────────

    private async void HandlePinError(string errorMessage)
    {
        await ShakePinDotsAsync();
    }

    private async void HandleStepChanged(string newTitle)
    {
        await PinDotsLayout.FadeTo(0, 120);
        await PinDotsLayout.FadeTo(1, 120);
    }

    private async void HandlePinSuccess()
    {
        await DisplayAlert("Success", "Admin PIN updated successfully.", "OK");
        await Shell.Current.GoToAsync("..");
    }

    // ── Shake Animation ───────────────────────────────────

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