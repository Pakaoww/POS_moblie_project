using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microcharts;
using POS_moblie_project.Models;
using POS_moblie_project.Services;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace POS_moblie_project.ViewModels;

public partial class ProfitReportViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;
    private List<Transaction> _allTransactions = new();
    private List<ProductLot> _allLots = new();
    private List<ProfitReportEntry> _allEntries = new();
    private Dictionary<int, bool> _productDeletedStatus = new();

    [ObservableProperty]
    private ObservableCollection<ProfitReportEntry> reportEntries = new();

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private DateTime fromDate = DateTime.Now.Date;

    [ObservableProperty]
    private DateTime toDate = DateTime.Now.Date.AddDays(1).AddSeconds(-1);

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string activePeriod = "ThisWeek";

    public bool IsThisWeekActive => ActivePeriod == "ThisWeek";
    public bool IsThisMonthActive => ActivePeriod == "ThisMonth";
    public bool IsThisYearActive => ActivePeriod == "ThisYear";

    [ObservableProperty]
    private string activeFilter = "ALL";

    public bool IsAllFilterActive => ActiveFilter == "ALL";
    public bool IsIncomeFilterActive => ActiveFilter == "Income";
    public bool IsExpenseFilterActive => ActiveFilter == "Expense";

    [ObservableProperty]
    private decimal totalIncome;

    [ObservableProperty]
    private decimal totalExpense;

    [ObservableProperty]
    private decimal totalProfit;

    [ObservableProperty]
    private Chart? incomeChart;

    [ObservableProperty]
    private Chart? expenseChart;

    public ProfitReportViewModel()
    {
        _databaseService = ServiceHelper.GetService<DatabaseService>();

        // Refresh stats เมื่อ currency setting เปลี่ยน
        ServiceHelper.GetService<CurrencyService>().SettingChanged += () =>
        {
            OnPropertyChanged(nameof(TotalIncome));
            OnPropertyChanged(nameof(TotalExpense));
            OnPropertyChanged(nameof(TotalProfit));
        };

        var today = DateTime.Today;
        int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
        FromDate = today.AddDays(-diff);
        ToDate = FromDate.AddDays(6);
        ActivePeriod = "ThisWeek";
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    partial void OnActiveFilterChanged(string value)
    {
        OnPropertyChanged(nameof(IsAllFilterActive));
        OnPropertyChanged(nameof(IsIncomeFilterActive));
        OnPropertyChanged(nameof(IsExpenseFilterActive));
        ApplyFilter();
    }

    [RelayCommand]
    public async Task LoadReportAsync()
    {
        IsLoading = true;
        try
        {
            ClearPeriodIfNotMatching();

            var from = FromDate.Date;
            var to = ToDate.Date.AddDays(1).AddTicks(-1);

            _allTransactions = await _databaseService.GetTransactionsAsync(from, to);
            var allLots = await _databaseService.GetAllLotsAsync();
            _allLots = allLots
                .Where(l => l.ReceivedAt >= from && l.ReceivedAt <= to)
                .ToList();

            await BuildAllEntriesAsync();
            CalculateStats();
            ApplyFilter();
            GenerateCharts();
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Error", ex.Message, "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task<Dictionary<int, bool>> LoadProductDeletedStatusAsync()
    {
        var allProducts = await _databaseService.GetAllProductsAsync();
        return allProducts.ToDictionary(p => p.Id, p => p.IsDeleted);
    }

    private async Task BuildAllEntriesAsync()
    {
        _productDeletedStatus = await LoadProductDeletedStatusAsync();

        _allEntries = new List<ProfitReportEntry>();

        foreach (var t in _allTransactions)
        {
            _allEntries.Add(new ProfitReportEntry
            {
                Id = t.TransactionId,
                Description = $"Transaction {t.TransactionId}",
                Timestamp = t.Timestamp,
                IncomeAmount = t.GrandTotal,
                ExpenseAmount = 0,
                EntryType = "Income",
                IsDeleted = false
            });
        }

        foreach (var l in _allLots)
        {
            var isProductDeleted = _productDeletedStatus.TryGetValue(l.ProductId, out var del) && del;
            var expense = l.CostPrice * l.Quantity;
            _allEntries.Add(new ProfitReportEntry
            {
                Id = l.LotId,
                Description = $"Lot {l.LotId} (Product #{l.ProductId})",
                Timestamp = l.ReceivedAt,
                IncomeAmount = 0,
                ExpenseAmount = expense,
                EntryType = "Expense",
                IsDeleted = isProductDeleted || l.IsDeleted
            });
        }

        _allEntries = _allEntries.OrderByDescending(e => e.Timestamp).ToList();
    }

    private void ApplyFilter()
    {
        var filtered = _allEntries.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var s = SearchText.Trim().ToLowerInvariant();
            filtered = filtered.Where(e =>
                e.Description.ToLowerInvariant().Contains(s) ||
                e.Id.ToLowerInvariant().Contains(s));
        }

        if (ActiveFilter == "Income")
            filtered = filtered.Where(e => e.EntryType == "Income");
        else if (ActiveFilter == "Expense")
            filtered = filtered.Where(e => e.EntryType == "Expense");

        ReportEntries.Clear();
        foreach (var e in filtered)
            ReportEntries.Add(e);
    }

    private void CalculateStats()
    {
        TotalIncome = _allTransactions.Sum(t => t.GrandTotal);
        TotalExpense = _allLots.Sum(l => l.CostPrice * l.Quantity);
        TotalProfit = TotalIncome - TotalExpense;
    }

    private void GenerateCharts()
    {
        var days = (ToDate.Date - FromDate.Date).Days;
        List<(string Label, decimal Income, decimal Expense)> buckets;

        if (days <= 14)
            buckets = GroupByDay(_allLots);
        else if (days <= 60)
            buckets = GroupByWeek(_allLots);
        else
            buckets = GroupByMonth(_allLots);

        if (buckets.Count == 0)
        {
            IncomeChart = new LineChart
            {
                Entries = new[] { new ChartEntry(0) { Label = "No data", Color = SKColor.Parse("#CCCCCC") } },
                BackgroundColor = SKColors.Transparent,
                LabelTextSize = 24f,
                ValueLabelTextSize = 22f,
            };
            ExpenseChart = null;
            return;
        }

        float maxVal = 0;
        foreach (var b in buckets)
        {
            if ((float)b.Income > maxVal) maxVal = (float)b.Income;
            if ((float)b.Expense > maxVal) maxVal = (float)b.Expense;
        }

        var incomeColor = SKColor.Parse("#4ECDC4");
        var expenseColor = SKColor.Parse("#E74C3C");
        float labelSize = buckets.Count > 10 ? 20f : 28f;
        float valueSize = buckets.Count > 10 ? 0f : 22f;
        float pointSize = buckets.Count > 10 ? 8f : 12f;

        float chartMax = maxVal > 0 ? maxVal * 1.15f : 1f;

        var incomeEntries = buckets.Select(b => new ChartEntry((float)b.Income)
        {
            Label = b.Label,
            ValueLabel = "",
            Color = incomeColor,
            TextColor = SKColor.Parse("#3c3d3c"),
            ValueLabelColor = incomeColor,
        }).ToList();

        IncomeChart = new LineChart
        {
            Entries = incomeEntries,
            BackgroundColor = SKColors.Transparent,
            LineMode = LineMode.Spline,
            LineSize = 3f,
            LineAreaAlpha = 20,
            PointMode = PointMode.Circle,
            PointSize = pointSize,
            LabelTextSize = labelSize,
            ValueLabelTextSize = valueSize,
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal,
            IsAnimated = false,
            MinValue = 0,
            MaxValue = chartMax,
        };

        var expenseEntries = buckets.Select(b => new ChartEntry((float)b.Expense)
        {
            Label = b.Label,
            ValueLabel = "",
            Color = expenseColor,
            TextColor = SKColors.Transparent,
            ValueLabelColor = expenseColor,
        }).ToList();

        ExpenseChart = new LineChart
        {
            Entries = expenseEntries,
            BackgroundColor = SKColors.Transparent,
            LineMode = LineMode.Spline,
            LineSize = 3f,
            LineAreaAlpha = 20,
            PointMode = PointMode.Circle,
            PointSize = pointSize,
            LabelTextSize = labelSize,
            ValueLabelTextSize = valueSize,
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal,
            IsAnimated = false,
            MinValue = 0,
            MaxValue = chartMax,
        };
    }

    private List<(string Label, decimal Income, decimal Expense)> GroupByDay(List<ProductLot> activeLots)
    {
        var result = new List<(string, decimal, decimal)>();
        var days = (ToDate.Date - FromDate.Date).Days + 1;
        for (int i = 0; i < days; i++)
        {
            var date = FromDate.Date.AddDays(i);
            var income = _allTransactions
                .Where(t => t.Timestamp.Date == date)
                .Sum(t => t.GrandTotal);
            var expense = activeLots
                .Where(l => l.ReceivedAt.Date == date)
                .Sum(l => l.CostPrice * l.Quantity);
            result.Add((date.ToString("dd/MM"), income, expense));
        }
        return result;
    }

    private List<(string Label, decimal Income, decimal Expense)> GroupByWeek(List<ProductLot> activeLots)
    {
        var result = new List<(string, decimal, decimal)>();
        var start = FromDate.Date;
        var end = ToDate.Date;
        var weekStart = start;
        int weekNum = 1;

        while (weekStart <= end)
        {
            var weekEnd = weekStart.AddDays(6);
            if (weekEnd > end) weekEnd = end;
            var income = _allTransactions
                .Where(t => t.Timestamp.Date >= weekStart && t.Timestamp.Date <= weekEnd)
                .Sum(t => t.GrandTotal);
            var expense = activeLots
                .Where(l => l.ReceivedAt.Date >= weekStart && l.ReceivedAt.Date <= weekEnd)
                .Sum(l => l.CostPrice * l.Quantity);
            result.Add(($"W{weekNum}", income, expense));
            weekStart = weekEnd.AddDays(1);
            weekNum++;
        }
        return result;
    }

    private List<(string Label, decimal Income, decimal Expense)> GroupByMonth(List<ProductLot> activeLots)
    {
        var result = new List<(string, decimal, decimal)>();
        var start = FromDate.Date;
        var end = ToDate.Date;
        var cursor = new DateTime(start.Year, start.Month, 1);

        while (cursor <= end)
        {
            var monthEnd = cursor.AddMonths(1).AddDays(-1);
            if (monthEnd > end) monthEnd = end;
            var income = _allTransactions
                .Where(t => t.Timestamp.Date >= cursor && t.Timestamp.Date <= monthEnd)
                .Sum(t => t.GrandTotal);
            var expense = activeLots
                .Where(l => l.ReceivedAt.Date >= cursor && l.ReceivedAt.Date <= monthEnd)
                .Sum(l => l.CostPrice * l.Quantity);
            result.Add((cursor.ToString("MMM"), income, expense));
            cursor = cursor.AddMonths(1);
        }
        return result;
    }

    private void ClearPeriodIfNotMatching()
    {
        if (string.IsNullOrEmpty(ActivePeriod)) return;

        var today = DateTime.Today;
        bool stillMatches = ActivePeriod switch
        {
            "ThisWeek" => FromDate.Date == today.AddDays(-((7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7))
                       && ToDate.Date == FromDate.Date.AddDays(6),
            "ThisMonth" => FromDate.Date == new DateTime(today.Year, today.Month, 1)
                        && ToDate.Date == new DateTime(today.Year, today.Month, 1).AddMonths(1).AddDays(-1),
            "ThisYear" => FromDate.Date == new DateTime(today.Year, 1, 1)
                       && ToDate.Date == new DateTime(today.Year, 12, 31),
            _ => false
        };

        if (!stillMatches)
        {
            ActivePeriod = string.Empty;
            OnPropertyChanged(nameof(IsThisWeekActive));
            OnPropertyChanged(nameof(IsThisMonthActive));
            OnPropertyChanged(nameof(IsThisYearActive));
        }
    }

    [RelayCommand]
    private async Task SetPeriod(string period)
    {
        if (ActivePeriod == period)
        {
            ActivePeriod = string.Empty;
            OnPropertyChanged(nameof(IsThisWeekActive));
            OnPropertyChanged(nameof(IsThisMonthActive));
            OnPropertyChanged(nameof(IsThisYearActive));
            return;
        }

        var today = DateTime.Today;

        switch (period)
        {
            case "ThisWeek":
                int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                FromDate = today.AddDays(-diff);
                ToDate = FromDate.AddDays(6);
                break;
            case "ThisMonth":
                FromDate = new DateTime(today.Year, today.Month, 1);
                ToDate = FromDate.AddMonths(1).AddDays(-1);
                break;
            case "ThisYear":
                FromDate = new DateTime(today.Year, 1, 1);
                ToDate = new DateTime(today.Year, 12, 31);
                break;
        }

        ActivePeriod = period;
        OnPropertyChanged(nameof(IsThisWeekActive));
        OnPropertyChanged(nameof(IsThisMonthActive));
        OnPropertyChanged(nameof(IsThisYearActive));

        await LoadReportCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private void SetFilter(string filter)
    {
        ActiveFilter = filter;
    }

    [RelayCommand]
    private async Task ExportExcelAsync()
    {
        if (_allEntries.Count == 0)
        {
            await Application.Current.MainPage.DisplayAlert(
                "No Data", "No profit data to export.", "OK");
            return;
        }

        try
        {
            var backupService = ServiceHelper.GetService<BackupService>();
            var filePath = await backupService
                .ExportProfitReportAsync(_allEntries, TotalIncome, TotalExpense,
                                         TotalProfit, FromDate, ToDate);

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Export Profit Report",
                File = new ShareFile(filePath)
            });
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Export Failed", ex.Message, "OK");
        }
    }
}
