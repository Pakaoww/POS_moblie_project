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

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowChangePinButton))]
    [NotifyPropertyChangedFor(nameof(ShowLockSettings))]
    private bool _isPinLockEnabled;

    public bool ShowChangePinButton => IsPinLockEnabled;
    public bool ShowLockSettings => IsPinLockEnabled;

    private const string LockSalesReportKey = "admin_lock_sales_report";
    private const string LockProfitReportKey = "admin_lock_profit_report";
    private const string LockTransactionKey = "admin_lock_transaction_history";

    [ObservableProperty] private bool _lockSalesReport;
    [ObservableProperty] private bool _lockProfitReport;
    [ObservableProperty] private bool _lockTransactionHistory;

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

        _lockSalesReport = Preferences.Get(LockSalesReportKey, false);
        _lockProfitReport = Preferences.Get(LockProfitReportKey, false);
        _lockTransactionHistory = Preferences.Get(LockTransactionKey, false);

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        var tf = await _databaseService.GetSettingAsync("dashboard_timeframe");
        SelectedTimeframe = TimeframeOptions.FirstOrDefault(o => o.Key == tf)
                            ?? TimeframeOptions[0];

        await RefreshPinStateAsync();
    }

    /// <summary>เรียกจาก AdminPanelPage.OnAppearing ทุกครั้งที่กลับมา</summary>
    public async Task RefreshPinStateAsync()
    {
        if (_suppressRefresh)
        {
            _suppressRefresh = false;
            return;
        }
        var saved = await SecureStorage.GetAsync(AdminPinKey);
        IsPinLockEnabled = !string.IsNullOrWhiteSpace(saved);
    }

    // ── Toggle PIN Lock ───────────────────────────────────

    private bool _isToggling = false;

    /// <summary>
    /// เมื่อปิด PIN Lock จะ set flag นี้ เพื่อป้องกันไม่ให้
    /// RefreshPinStateAsync (ที่เรียกจาก OnAppearing หลังจาก modal dismiss)
    /// อ่าน SecureStorage ที่ยังเป็นค่าเก่าอยู่ แล้ว override ค่า IsPinLockEnabled
    /// </summary>
    private bool _suppressRefresh;

    [RelayCommand]
    private async Task TogglePinLockAsync()
    {
        if (_isToggling) return;
        _isToggling = true;
        _suppressRefresh = false;

        try
        {
            if (!IsPinLockEnabled)
            {
                await Shell.Current.GoToAsync("PasswordPage?mode=admin");
            }
            else
            {
                _suppressRefresh = true;

                bool confirm = await AppAlert.ConfirmAsync(
                    "Disable PIN Lock",
                    "Admin Panel will be accessible without a password. Continue?",
                    "Disable", "Cancel", isDanger: true);

                if (!confirm)
                {
                    return;
                }

                SecureStorage.Remove("admin_pin");
                IsPinLockEnabled = false;
            }
        }
        finally
        {
            _suppressRefresh = false;
            _isToggling = false;
        }
    }

    // ── Change Admin Password ─────────────────────────────

    [RelayCommand]
    private async Task ChangeAdminPasswordAsync()
    {
        await Shell.Current.GoToAsync("AdminManagePasswordPage?mode=Change");
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

    // ── Toggle Lock Settings ──────────────────────────────

    partial void OnLockSalesReportChanged(bool value)
    {
        Preferences.Set(LockSalesReportKey, value);
    }

    partial void OnLockProfitReportChanged(bool value)
    {
        Preferences.Set(LockProfitReportKey, value);
    }

    partial void OnLockTransactionHistoryChanged(bool value)
    {
        Preferences.Set(LockTransactionKey, value);
    }

    [RelayCommand] private void ToggleLockSalesReport() => LockSalesReport = !LockSalesReport;
    [RelayCommand] private void ToggleLockProfitReport() => LockProfitReport = !LockProfitReport;
    [RelayCommand] private void ToggleLockTransactionHistory() => LockTransactionHistory = !LockTransactionHistory;
}

public record TimeframeOption(string Key, string DisplayName);