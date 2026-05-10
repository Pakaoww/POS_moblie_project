using POS_moblie_project.ViewModels;

namespace POS_moblie_project.Views.Settings;

public partial class ManagePasswordPage : ContentPage
{
    private readonly ManagePasswordViewModel _viewModel;
    private ManagePasswordMode _mode;

    public ManagePasswordPage(ManagePasswordViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        _viewModel.OnPinError += HandlePinError;
        _viewModel.OnStepChanged += HandleStepChanged;
        _viewModel.OnPinSuccess += HandlePinSuccess;
    }

    // ── รับ mode จาก SettingsViewModel ──────────────────────
    public void SetMode(ManagePasswordMode mode)
    {
        _mode = mode;
    }

    // ── Lifecycle ─────────────────────────────────────────

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.InitializeCommand.Execute(_mode);
    }

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
        var message = _mode == ManagePasswordMode.ChangePin
            ? "PIN changed successfully."
            : "Password changed successfully.";

        await DisplayAlert("Success", message, "OK");

        // กลับไปหน้า Settings
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