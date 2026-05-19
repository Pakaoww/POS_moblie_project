using POS_moblie_project.ViewModels;

namespace POS_moblie_project.Views.Settings;

public partial class AdminPasswordPage : ContentPage, IQueryAttributable
{
    private readonly AdminPasswordViewModel _viewModel;

    public AdminPasswordPage(AdminPasswordViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("target", out var target) && target is string targetStr)
        {
            _viewModel.SetTarget(Uri.UnescapeDataString(targetStr));
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.InitializeCommand.Execute(null);
    }
}