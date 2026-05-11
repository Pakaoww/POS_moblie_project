using POS_moblie_project.ViewModels;

namespace POS_moblie_project.Views.Stock;

public partial class StockPage : ContentPage
{
    private readonly StockViewModel _viewModel;

    public StockPage(StockViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadStockCommand.ExecuteAsync(null);
    }
}