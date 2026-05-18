using POS_moblie_project.ViewModels.Settings;

namespace POS_moblie_project.Views.Settings;

public partial class AdminPanelPage : ContentPage
{
    public AdminPanelPage(AdminPanelViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
