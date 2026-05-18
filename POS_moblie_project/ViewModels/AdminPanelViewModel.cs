using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace POS_moblie_project.ViewModels.Settings;

public partial class AdminPanelViewModel : ObservableObject
{
    private const string AdminPinKey = "admin_pin";
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

    // ── บันทึกและแจ้ง HomeViewModel + AppShell ───────────

    partial void OnShowSalesReportChanged(bool value)
    {
        Preferences.Set(ShowSalesReportKey, value);
        RefreshHome();
        RefreshShell();
    }

    partial void OnShowProfitReportChanged(bool value)
    {
        Preferences.Set(ShowProfitReportKey, value);
        RefreshHome();
        RefreshShell();
    }

    partial void OnShowTransactionHistoryChanged(bool value)
    {
        Preferences.Set(ShowTransactionKey, value);
        RefreshHome();
        RefreshShell();
    }

    private static void RefreshHome()
    {
        var homeVm = ServiceHelper.GetService<HomeViewModel>();
        homeVm.RefreshAdminSettings();
    }

    private static void RefreshShell()
    {
        // แจ้ง AppShell ให้ refresh FlyoutItem visibility
        if (Shell.Current is AppShell appShell)
            appShell.RefreshFlyoutVisibility();
    }

    // ── Toggle Commands ───────────────────────────────────

    [RelayCommand] private void ToggleShowSalesReport() => ShowSalesReport = !ShowSalesReport;
    [RelayCommand] private void ToggleShowProfitReport() => ShowProfitReport = !ShowProfitReport;
    [RelayCommand] private void ToggleShowTransactionHistory() => ShowTransactionHistory = !ShowTransactionHistory;

    // ── Change Admin Password ─────────────────────────────

    [RelayCommand]
    private async Task ChangeAdminPasswordAsync()
        => await Shell.Current.GoToAsync("AdminManagePasswordPage");

    // ── Static helpers ────────────────────────────────────

    public static async Task<bool> VerifyAdminAsync(string pin)
    {
        var saved = await SecureStorage.GetAsync(AdminPinKey);
        return saved is null || saved == pin;
    }
}