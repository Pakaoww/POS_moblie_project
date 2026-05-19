using POS_moblie_project.ViewModels.Settings;

namespace POS_moblie_project.Views.Settings;

public partial class AdminManagePasswordPage : ContentPage, IQueryAttributable
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

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("mode", out var modeObj) && modeObj is string modeStr)
        {
            if (Enum.TryParse<AdminManagePasswordViewModel.AdminPinMode>(modeStr, out var mode))
            {
                _mode = mode;
            }
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
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