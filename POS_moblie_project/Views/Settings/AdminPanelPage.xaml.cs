using POS_moblie_project.ViewModels.Settings;

namespace POS_moblie_project.Views.Settings;

public partial class AdminPanelPage : ContentPage
{
    private readonly AdminPanelViewModel _viewModel;

    public AdminPanelPage(AdminPanelViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var vm = BindingContext as AdminPanelViewModel;
        if (vm != null)
            await vm.RefreshPinStateAsync();
    }
}
