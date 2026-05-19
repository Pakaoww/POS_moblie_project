using POS_moblie_project.ViewModels;

namespace POS_moblie_project.Views.POS;

public partial class POSPage : ContentPage
{
    private readonly POSViewModel _viewModel;

    public POSPage(POSViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_viewModel.Products.Count == 0)
            await _viewModel.LoadProductsCommand.ExecuteAsync(null);
    }
}