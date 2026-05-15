using POS_moblie_project.ViewModels;

namespace POS_moblie_project.Views.Stock;

public partial class AddStockPage : ContentPage
{
    public AddStockPage(AddStockViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}