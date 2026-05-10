using POS_moblie_project.ViewModels;

namespace POS_moblie_project.Views.Settings;

public partial class ManagePasswordPage : ContentPage
{
    private readonly ManagePasswordViewModel _viewModel;

    public ManagePasswordPage(ManagePasswordViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        // Subscribe events จาก ViewModel
        _viewModel.OnPinError += HandlePinError;
        _viewModel.OnStepChanged += HandleStepChanged;
        _viewModel.OnPinSuccess += HandlePinSuccess;
    }

    // ── Lifecycle ─────────────────────────────────────────

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.InitializeCommand.Execute(null);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Unsubscribe ป้องกัน memory leak
        _viewModel.OnPinError -= HandlePinError;
        _viewModel.OnStepChanged -= HandleStepChanged;
        _viewModel.OnPinSuccess -= HandlePinSuccess;
    }

    // ── Event Handlers ────────────────────────────────────

    /// <summary>PIN ผิด — shake แล้ว clear จุด</summary>
    private async void HandlePinError(string errorMessage)
    {
        await ShakePinDotsAsync();
    }

    /// <summary>เปลี่ยน step — flash animation บอกผู้ใช้</summary>
    private async void HandleStepChanged(string newTitle)
    {
        await PinDotsLayout.FadeTo(0, 120);
        await PinDotsLayout.FadeTo(1, 120);
    }

    /// <summary>สำเร็จ — แสดง alert แล้วกลับหน้า Settings</summary>
    private async void HandlePinSuccess()
    {
        await DisplayAlert("Success", "PIN changed successfully.", "OK");
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