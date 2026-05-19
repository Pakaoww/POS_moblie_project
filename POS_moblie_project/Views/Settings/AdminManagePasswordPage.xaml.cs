using POS_moblie_project.ViewModels.Settings;

namespace POS_moblie_project.Views.Settings;

public partial class AdminManagePasswordPage : ContentPage
{
    private readonly AdminManagePasswordViewModel _viewModel;
    private AdminManagePasswordViewModel.AdminPinMode _mode = AdminManagePasswordViewModel.AdminPinMode.Change;

    public AdminManagePasswordPage(AdminManagePasswordViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        _viewModel.OnPinError += HandlePinError;
        _viewModel.OnStepChanged += HandleStepChanged;
        _viewModel.OnPinSuccess += HandlePinSuccess;
    }

    /// <summary>ให้ AdminPanelViewModel เรียกเพื่อตั้ง mode</summary>
    public void SetMode(AdminManagePasswordViewModel.AdminPinMode mode)
    {
        _mode = mode;
        _viewModel.Initialize(mode);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Initialize ใหม่ด้วย mode ที่เคย set ไว้ เพื่อให้ step กลับมาถูกต้อง
        _viewModel.Initialize(_mode);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.OnPinError -= HandlePinError;
        _viewModel.OnStepChanged -= HandleStepChanged;
        _viewModel.OnPinSuccess -= HandlePinSuccess;
    }

    private async void HandlePinError(string _)
        => await ShakePinDotsAsync();

    private async void HandleStepChanged(string _)
    {
        await PinDotsLayout.FadeTo(0, 120);
        await PinDotsLayout.FadeTo(1, 120);
    }

    private void HandlePinSuccess() { }

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