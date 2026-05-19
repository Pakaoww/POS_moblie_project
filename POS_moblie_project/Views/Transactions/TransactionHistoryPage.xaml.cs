using POS_moblie_project.Services;
using POS_moblie_project.ViewModels;

namespace POS_moblie_project.Views.Transactions;

public partial class TransactionHistoryPage : ContentPage
{
    private readonly TransactionHistoryViewModel _viewModel;

    public TransactionHistoryPage(TransactionHistoryViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (Preferences.Get("admin_lock_transaction_history", false) &&
            !PageLockService.ConsumeAuthorization(PageLockService.RouteTransactions))
        {
            await Shell.Current.GoToAsync($"AdminPasswordPage?target={Uri.EscapeDataString(PageLockService.RouteTransactions)}");
            return;
        }

        await _viewModel.LoadTransactionsCommand.ExecuteAsync(null);
    }
}