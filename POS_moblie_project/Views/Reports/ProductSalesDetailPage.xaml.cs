using POS_moblie_project.ViewModels;

namespace POS_moblie_project.Views.Reports;

public partial class ProductSalesDetailPage : ContentPage
{
    public ProductSalesDetailPage(ProductSalesDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}