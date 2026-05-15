using POS_moblie_project.ViewModels;

namespace POS_moblie_project.Views.Stock;

public partial class AddStockPage : ContentPage
{
    private readonly AddStockViewModel _viewModel;

    public AddStockPage(AddStockViewModel viewModel)
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