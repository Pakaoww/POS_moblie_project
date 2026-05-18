using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace POS_moblie_project.ViewModels.Settings;

public partial class AdminPanelViewModel : ObservableObject
{
    private const string ShowSalesReportKey = "admin_show_sales_report";
    private const string ShowProfitReportKey = "admin_show_profit_report";
    private const string ShowTransactionKey = "admin_show_transaction_history";

    [ObservableProperty] private bool _showSalesReport;
    [ObservableProperty] private bool _showProfitReport;
    [ObservableProperty] private bool _showTransactionHistory;

    public AdminPanelViewModel()
    {
        _showSalesReport = Preferences.Get(ShowSalesReportKey, true);
        _showProfitReport = Preferences.Get(ShowProfitReportKey, true);
        _showTransactionHistory = Preferences.Get(ShowTransactionKey, true);
    }

    partial void OnShowSalesReportChanged(bool value) => Preferences.Set(ShowSalesReportKey, value);
    partial void OnShowProfitReportChanged(bool value) => Preferences.Set(ShowProfitReportKey, value);
    partial void OnShowTransactionHistoryChanged(bool value) => Preferences.Set(ShowTransactionKey, value);

    [RelayCommand] private void ToggleShowSalesReport() => ShowSalesReport = !ShowSalesReport;
    [RelayCommand] private void ToggleShowProfitReport() => ShowProfitReport = !ShowProfitReport;
    [RelayCommand] private void ToggleShowTransactionHistory() => ShowTransactionHistory = !ShowTransactionHistory;

    [RelayCommand]
    private async Task ChangeAdminPasswordAsync()
        => await Shell.Current.GoToAsync(nameof(Views.Settings.AdminManagePasswordPage));

    internal static async Task<bool> VerifyAdminAsync(string input)
    {
        throw new NotImplementedException();
    }
}