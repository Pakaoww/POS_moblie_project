using POS_moblie_project.ViewModels;

namespace POS_moblie_project.Views.Transactions;

public partial class TransactionDetailPage : ContentPage
{
    public TransactionDetailPage(TransactionDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}