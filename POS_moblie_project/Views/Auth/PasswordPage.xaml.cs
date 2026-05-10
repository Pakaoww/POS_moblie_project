using POS_moblie_project.ViewModels;

namespace POS_moblie_project.Views.Auth;

public partial class PasswordPage : ContentPage
{
    private readonly PasswordViewModel _viewModel;

    public PasswordPage(PasswordViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeCommand.ExecuteAsync(null);
    }
}