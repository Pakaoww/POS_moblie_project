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
}