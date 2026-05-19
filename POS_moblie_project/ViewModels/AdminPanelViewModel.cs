using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS_moblie_project.Services;
using System.Collections.ObjectModel;

namespace POS_moblie_project.ViewModels.Settings;

public partial class AdminPanelViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;

    private const string AdminPinKey = "admin_pin";
    private const string ShowSalesReportKey = "admin_show_sales_report";
    private const string ShowProfitReportKey = "admin_show_profit_report";
    private const string ShowTransactionKey = "admin_show_transaction_history";

    [ObservableProperty] private bool _showSalesReport;
    [ObservableProperty] private bool _showProfitReport;
    [ObservableProperty] private bool _showTransactionHistory;

    // ── Timeframe ─────────────────────────────────────────
    public ObservableCollection<TimeframeOption> TimeframeOptions { get; } = new()
    {
        new("weekly",    "Weekly (7 days)"),
        new("monthly",   "Monthly (30 days)"),
        new("quarterly", "Quarterly (90 days)"),
        new("yearly",    "Yearly (365 days)"),
    };

    [ObservableProperty]
    private TimeframeOption? _selectedTimeframe;

    partial void OnSelectedTimeframeChanged(TimeframeOption? value)
    {
        if (value is null) return;
        _ = _databaseService.SetSettingAsync("dashboard_timeframe", value.Key);

        // Refresh chart ใน HomeViewModel ทันที
        var homeVm = ServiceHelper.GetService<HomeViewModel>();
        _ = homeVm.LoadDashboardCommand.ExecuteAsync(null);
    }

    // ════════════════════════════════════════════════════════

    public AdminPanelViewModel()
    {
        _databaseService = ServiceHelper.GetService<DatabaseService>();

        _showSalesReport = Preferences.Get(ShowSalesReportKey, true);
        _showProfitReport = Preferences.Get(ShowProfitReportKey, true);
        _showTransactionHistory = Preferences.Get(ShowTransactionKey, true);

        // โหลด timeframe ที่บันทึกไว้
        _ = LoadTimeframeAsync();
    }

    private async Task LoadTimeframeAsync()
    {
        var tf = await _databaseService.GetSettingAsync("dashboard_timeframe");
        SelectedTimeframe = TimeframeOptions.FirstOrDefault(o => o.Key == tf)
                            ?? TimeframeOptions[0];
    }

    // ── Toggle Visibility ─────────────────────────────────

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
        if (Shell.Current is AppShell appShell)
            appShell.RefreshFlyoutVisibility();
    }

    [RelayCommand] private void ToggleShowSalesReport() => ShowSalesReport = !ShowSalesReport;
    [RelayCommand] private void ToggleShowProfitReport() => ShowProfitReport = !ShowProfitReport;
    [RelayCommand] private void ToggleShowTransactionHistory() => ShowTransactionHistory = !ShowTransactionHistory;

    // ── Admin Password ────────────────────────────────────

    [RelayCommand]
    private async Task ChangeAdminPasswordAsync()
        => await Shell.Current.GoToAsync("AdminManagePasswordPage");

    public static async Task<bool> VerifyAdminAsync(string pin)
    {
        var saved = await SecureStorage.GetAsync(AdminPinKey);
        return saved is null || saved == pin;
    }
}
public record TimeframeOption(string Key, string DisplayName);