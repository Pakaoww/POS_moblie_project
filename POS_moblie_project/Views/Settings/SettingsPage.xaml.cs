using POS_moblie_project.ViewModels.Settings;

namespace POS_moblie_project.Views.Settings;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}