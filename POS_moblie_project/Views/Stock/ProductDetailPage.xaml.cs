using POS_moblie_project.ViewModels;

namespace POS_moblie_project.Views.Stock;

public partial class ProductDetailPage : ContentPage
{
    private readonly ProductDetailViewModel _viewModel;

    public ProductDetailPage(ProductDetailViewModel viewModel)
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