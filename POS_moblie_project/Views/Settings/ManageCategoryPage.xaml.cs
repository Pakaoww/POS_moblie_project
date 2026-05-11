using POS_moblie_project.ViewModels;

namespace POS_moblie_project.Views.Settings;

public partial class ManageCategoryPage : ContentPage
{
    private readonly ManageCategoryViewModel _viewModel;

    public ManageCategoryPage(ManageCategoryViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadCategoriesCommand.ExecuteAsync(null);
    }
}