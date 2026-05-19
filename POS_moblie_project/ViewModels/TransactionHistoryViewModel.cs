using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS_moblie_project.Models;
using POS_moblie_project.Services;
using System.Collections.ObjectModel;

namespace POS_moblie_project.ViewModels;

public partial class TransactionHistoryViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;
    private List<Transaction> _allTransactions = new();

    [ObservableProperty]
    private ObservableCollection<Transaction> transactions = new();

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private DateTime fromDate = DateTime.Now.Date;

    [ObservableProperty]
    private DateTime toDate = DateTime.Now.Date.AddDays(1).AddSeconds(-1);

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private int totalTransactions;

    [ObservableProperty]
    private decimal averageRevenue;

    [ObservableProperty]
    private decimal totalRevenue;

    [ObservableProperty]
    private string _activePeriod = string.Empty;

    public bool IsTodayActive => ActivePeriod == "Today";
    public bool IsThisWeekActive => ActivePeriod == "ThisWeek";
    public bool IsThisMonthActive => ActivePeriod == "ThisMonth";
    public bool IsThisYearActive => ActivePeriod == "ThisYear";  // ← เพิ่ม

    public TransactionHistoryViewModel()
    {
        _databaseService = ServiceHelper.GetService<DatabaseService>();

        ServiceHelper.GetService<CurrencyService>().SettingChanged += () =>
        {
            OnPropertyChanged(nameof(TotalRevenue));
            OnPropertyChanged(nameof(AverageRevenue));
        };

        var today = DateTime.Today;
        FromDate = today;
        ToDate = today;
        ActivePeriod = "Today";
    }

    partial void OnFromDateChanged(DateTime value)
    {
        if (value > ToDate) ToDate = value;
    }

    partial void OnToDateChanged(DateTime value)
    {
        if (value < FromDate) FromDate = value;
    }

    partial void OnSearchTextChanged(string value) => ApplySearch();

    [RelayCommand]
    public async Task LoadTransactionsAsync()
    {
        IsLoading = true;
        try
        {
            ClearPeriodIfNotMatching();
            _allTransactions = await _databaseService.GetTransactionsAsync(FromDate, ToDate);
            ApplySearch();
            CalculateStats();
        }
        catch (Exception ex)
        {
            await AppAlert.ShowErrorAsync("Error", ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ApplySearch()
    {
        var filtered = _allTransactions.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var s = SearchText.Trim().ToLowerInvariant();
            filtered = filtered.Where(t =>
                t.TransactionId.ToLowerInvariant().Contains(s));
        }

        Transactions.Clear();
        foreach (var t in filtered)
            Transactions.Add(t);
    }

    private void ClearPeriodIfNotMatching()
    {
        if (string.IsNullOrEmpty(ActivePeriod)) return;

        var today = DateTime.Today;
        bool stillMatches = ActivePeriod switch
        {
            "Today" => FromDate.Date == today && ToDate.Date == today,
            "ThisWeek" => FromDate.Date == today.AddDays(-((7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7))
                        && ToDate.Date == FromDate.Date.AddDays(6),
            "ThisMonth" => FromDate.Date == new DateTime(today.Year, today.Month, 1)
                        && ToDate.Date == new DateTime(today.Year, today.Month, 1).AddMonths(1).AddDays(-1),
            "ThisYear" => FromDate.Date == new DateTime(today.Year, 1, 1)      // ← เพิ่ม
                        && ToDate.Date == new DateTime(today.Year, 12, 31),
            _ => false
        };

        if (!stillMatches)
        {
            ActivePeriod = string.Empty;
            NotifyPeriodChanged();
        }
    }

    private void CalculateStats()
    {
        TotalTransactions = _allTransactions.Count;
        TotalRevenue = _allTransactions.Sum(t => t.GrandTotal);
        AverageRevenue = TotalTransactions > 0
            ? Math.Round(TotalRevenue / TotalTransactions, 2)
            : 0;
    }

    [RelayCommand]
    private async Task ViewDetailAsync(Transaction transaction)
    {
        if (transaction == null) return;
        await Shell.Current.GoToAsync(
            $"TransactionDetailPage?id={transaction.TransactionId}");
    }

    [RelayCommand]
    private async Task ExportExcelAsync()
    {
        if (_allTransactions.Count == 0)
        {
            await AppAlert.ShowWarningAsync("No Data", "No transactions to export.");
            return;
        }

        try
        {
            var backupService = ServiceHelper.GetService<BackupService>();
            var filePath = await backupService.ExportTransactionHistoryAsync(_allTransactions);

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Export Transaction History",
                File = new ShareFile(filePath)
            });
        }
        catch (Exception ex)
        {
            await AppAlert.ShowErrorAsync("Export Failed", ex.Message);
        }
    }

    [RelayCommand]
    private async Task SetPeriod(string period)
    {
        if (ActivePeriod == period)
        {
            ActivePeriod = string.Empty;
            NotifyPeriodChanged();
            return;
        }

        var today = DateTime.Today;

        switch (period)
        {
            case "Today":
                FromDate = today;
                ToDate = today;
                break;
            case "ThisWeek":
                int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                FromDate = today.AddDays(-diff);
                ToDate = FromDate.AddDays(6);
                break;
            case "ThisMonth":
                FromDate = new DateTime(today.Year, today.Month, 1);
                ToDate = FromDate.AddMonths(1).AddDays(-1);
                break;
            case "ThisYear":                                // ← เพิ่ม
                FromDate = new DateTime(today.Year, 1, 1);
                ToDate = new DateTime(today.Year, 12, 31);
                break;
        }

        ActivePeriod = period;
        NotifyPeriodChanged();

        await LoadTransactionsCommand.ExecuteAsync(null);
    }

    private void NotifyPeriodChanged()
    {
        OnPropertyChanged(nameof(IsTodayActive));
        OnPropertyChanged(nameof(IsThisWeekActive));
        OnPropertyChanged(nameof(IsThisMonthActive));
        OnPropertyChanged(nameof(IsThisYearActive));  // ← เพิ่ม
    }
}