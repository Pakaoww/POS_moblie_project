using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microcharts;
using POS_moblie_project.Services;
using SkiaSharp;

namespace POS_moblie_project.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;

    [ObservableProperty] 
    private int totalProducts;
    [ObservableProperty] 
    private int lowStockItems;
    [ObservableProperty] 
    private int outOfStockItems;
    [ObservableProperty] 
    private decimal todaySales;
    [ObservableProperty] 
    private int todayTransactions;
    [ObservableProperty] 
    private Chart? weeklyBarChart;           // ← nullable
    [ObservableProperty] 
    private string dashboardTimeframeDisplay = "Last 7 days";
    [ObservableProperty] 
    private string _legendCurrentText = "Today";
    [ObservableProperty] 
    private string _legendOtherText = "Other";

    [ObservableProperty] 
    private Chart? profitIncomeChart;
    [ObservableProperty] 
    private Chart? profitExpenseChart;
    [ObservableProperty] 
    private string profitTimeframeDisplay = "Last 7 days";

    public HomeViewModel()
    {
        _databaseService = ServiceHelper.GetService<DatabaseService>();
    }

    [RelayCommand]
    public async Task LoadDashboardAsync()
    {
        try
        {
            WeeklyBarChart = null;

            var products = await _databaseService.GetAllProductsAsync();
            TotalProducts = products.Count;

            int lowCount = 0;
            int outCount = 0;
            foreach (var p in products)
            {
                var stock = await _databaseService.GetTotalStockAsync(p.Id);
                if (stock <= 0) outCount++;
                else if (stock <= 5) lowCount++;
            }
            LowStockItems = lowCount;
            OutOfStockItems = outCount;

            var today = DateTime.Now.Date;
            var tomorrow = today.AddDays(1);
            var todayTx = await _databaseService.GetTransactionsAsync(today, tomorrow);
            TodayTransactions = todayTx.Count;
            TodaySales = todayTx.Sum(t => t.GrandTotal);

            var tf = await _databaseService.GetSettingAsync("dashboard_timeframe");
            if (string.IsNullOrWhiteSpace(tf)) tf = "weekly";
            await GenerateSalesChartAsync(tf);
            await GenerateProfitChartAsync(tf);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");  // ← Shell
        }
    }

    private async Task GenerateSalesChartAsync(string timeframe)
    {
        try
        {
            var today = DateTime.Now.Date;
            DateTime from, to;

            switch (timeframe)
            {
                case "weekly":
                    from = today.AddDays(-(int)today.DayOfWeek);
                    to = from.AddDays(7);
                    break;
                case "monthly":
                    from = new DateTime(today.Year, today.Month, 1);
                    to = from.AddMonths(1);
                    break;
                case "quarterly":
                    int quarter = (today.Month - 1) / 3;
                    from = new DateTime(today.Year, quarter * 3 + 1, 1);
                    to = from.AddMonths(3);
                    break;
                default:
                    from = new DateTime(today.Year, 1, 1);
                    to = from.AddYears(1);
                    break;
            }

            var txns = await _databaseService.GetTransactionsAsync(from, to);
            var txnIds = txns.Select(t => t.Id).ToHashSet();
            var allItems = await _databaseService.GetAllTransactionItemsAsync();
            var filteredItems = allItems.Where(i => txnIds.Contains(i.TransactionId)).ToList();

            var productSales = filteredItems
                .GroupBy(i => i.ProductName)
                .Select(g => new { Product = g.Key, Count = g.Sum(i => i.Quantity) })
                .OrderByDescending(x => x.Count)
                .ToList();

            var entries = new List<ChartEntry>();
            var colors = new[]
            {
                SKColor.Parse("#FF6384"), SKColor.Parse("#36A2EB"),
                SKColor.Parse("#FFCE56"), SKColor.Parse("#4BC0C0"),
                SKColor.Parse("#9966FF"), SKColor.Parse("#FF9F40"),
                SKColor.Parse("#C9CBCF"), SKColor.Parse("#7BC8A4"),
                SKColor.Parse("#E7E9ED"), SKColor.Parse("#F7464A"),
                SKColor.Parse("#949FB1"), SKColor.Parse("#4D5360"),
            };

            int totalCount = productSales.Sum(x => x.Count);
            if (totalCount == 0)
            {
                WeeklyBarChart = new PieChart
                {
                    Entries = new[] { new ChartEntry(1) { Label = "No data", Color = SKColor.Parse("#CCCCCC") } },
                    BackgroundColor = SKColors.Transparent,
                    LabelTextSize = 28f,
                };
                return;
            }

            int colorIdx = 0;
            foreach (var ps in productSales)
            {
                var color = colors[colorIdx % colors.Length];
                entries.Add(new ChartEntry(ps.Count)
                {
                    Label = ps.Product,
                    ValueLabel = $"{ps.Count} units",
                    Color = color,
                });
                colorIdx++;
            }

            WeeklyBarChart = new PieChart
            {
                Entries = entries,
                BackgroundColor = SKColors.Transparent,
                LabelTextSize = 28f,
                IsAnimated = false,
                HoleRadius = 0.4f,
            };
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", $"Failed to generate chart: {ex.Message}", "OK");
        }
    }

    private async Task GenerateProfitChartAsync(string timeframe)
    {
        try
        {
            var today = DateTime.Now.Date;
            DateTime from, to;

            switch (timeframe)
            {
                case "weekly":
                    var weekStart = today.AddDays(-(int)today.DayOfWeek);
                    from = weekStart;
                    to = weekStart.AddDays(7);
                    ProfitTimeframeDisplay = "This week";
                    break;
                case "monthly":
                    from = new DateTime(today.Year, today.Month, 1);
                    to = from.AddMonths(1);
                    ProfitTimeframeDisplay = "This month";
                    break;
                case "quarterly":
                    int quarter = (today.Month - 1) / 3;
                    from = new DateTime(today.Year, quarter * 3 + 1, 1);
                    to = from.AddMonths(3);
                    ProfitTimeframeDisplay = "This quarter";
                    break;
                default:
                    from = new DateTime(today.Year, 1, 1);
                    to = from.AddYears(1);
                    ProfitTimeframeDisplay = "This year";
                    break;
            }

            var transactions = await _databaseService.GetTransactionsAsync(from, to);
            var allLots = await _databaseService.GetAllLotsAsync();
            var lotsInRange = allLots
                .Where(l => l.ReceivedAt >= from && l.ReceivedAt < to)
                .ToList();

            List<(string Label, decimal Income, decimal Expense)> buckets;

            if (timeframe == "weekly")
            {
                buckets = new List<(string, decimal, decimal)>();
                string[] dayLabels = { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
                for (int i = 0; i < 7; i++)
                {
                    var date = from.AddDays(i);
                    var income = transactions.Where(t => t.Timestamp.Date == date).Sum(t => t.GrandTotal);
                    var expense = lotsInRange.Where(l => l.ReceivedAt.Date == date).Sum(l => l.CostPrice * l.Quantity);
                    buckets.Add((dayLabels[i], income, expense));
                }
            }
            else if (timeframe == "monthly")
            {
                buckets = new List<(string, decimal, decimal)>();
                int daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
                int weekCount = (daysInMonth - 1) / 7 + 1;
                for (int w = 1; w <= weekCount; w++)
                {
                    int wStart = (w - 1) * 7 + 1;
                    int wEnd = Math.Min(w * 7, daysInMonth);
                    var startDate = new DateTime(today.Year, today.Month, wStart);
                    var endDate = new DateTime(today.Year, today.Month, wEnd);
                    var income = transactions.Where(t => t.Timestamp.Date >= startDate && t.Timestamp.Date <= endDate).Sum(t => t.GrandTotal);
                    var expense = lotsInRange.Where(l => l.ReceivedAt.Date >= startDate && l.ReceivedAt.Date <= endDate).Sum(l => l.CostPrice * l.Quantity);
                    buckets.Add(($"W{w}", income, expense));
                }
            }
            else if (timeframe == "quarterly")
            {
                buckets = new List<(string, decimal, decimal)>();
                int q = (today.Month - 1) / 3;
                for (int m = q * 3 + 1; m <= q * 3 + 3; m++)
                {
                    var income = transactions.Where(t => t.Timestamp.Month == m).Sum(t => t.GrandTotal);
                    var expense = lotsInRange.Where(l => l.ReceivedAt.Month == m).Sum(l => l.CostPrice * l.Quantity);
                    buckets.Add(($"M{m}", income, expense));
                }
            }
            else
            {
                buckets = new List<(string, decimal, decimal)>();
                for (int m = 1; m <= 12; m++)
                {
                    var income = transactions.Where(t => t.Timestamp.Month == m).Sum(t => t.GrandTotal);
                    var expense = lotsInRange.Where(l => l.ReceivedAt.Month == m).Sum(l => l.CostPrice * l.Quantity);
                    buckets.Add(($"M{m}", income, expense));
                }
            }

            bool noData = buckets.All(b => b.Income == 0 && b.Expense == 0);
            if (noData)
            {
                ProfitIncomeChart = new LineChart
                {
                    Entries = new[] { new ChartEntry(0) { Label = "No data", Color = SKColor.Parse("#CCCCCC") } },
                    BackgroundColor = SKColors.Transparent,
                    LabelTextSize = 24f,
                    ValueLabelTextSize = 22f,
                };
                ProfitExpenseChart = null;
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
            int entryCount = buckets.Count;
            float labelSize = entryCount > 10 ? 20f : 28f;
            float pointSize = entryCount > 10 ? 8f : 12f;
            float chartMax = maxVal > 0 ? maxVal * 1.15f : 1f;

            var incomeEntries = buckets.Select(b => new ChartEntry((float)b.Income)
            {
                Label = b.Label,
                ValueLabel = "",
                Color = incomeColor,
                TextColor = SKColor.Parse("#3c3d3c"),
                ValueLabelColor = incomeColor,
            }).ToList();

            ProfitIncomeChart = new LineChart
            {
                Entries = incomeEntries,
                BackgroundColor = SKColors.Transparent,
                LineMode = LineMode.Spline,
                LineSize = 3f,
                LineAreaAlpha = 20,
                PointMode = PointMode.Circle,
                PointSize = pointSize,
                LabelTextSize = labelSize,
                ValueLabelTextSize = 0f,
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

            ProfitExpenseChart = new LineChart
            {
                Entries = expenseEntries,
                BackgroundColor = SKColors.Transparent,
                LineMode = LineMode.Spline,
                LineSize = 3f,
                LineAreaAlpha = 20,
                PointMode = PointMode.Circle,
                PointSize = pointSize,
                LabelTextSize = 0f,
                ValueLabelTextSize = 0f,
                LabelOrientation = Orientation.Horizontal,
                ValueLabelOrientation = Orientation.Horizontal,
                IsAnimated = false,
                MinValue = 0,
                MaxValue = chartMax,
            };
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", $"Failed to generate profit chart: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task GoToPosAsync()
        => await Shell.Current.GoToAsync("///pos");

    [RelayCommand]
    private async Task GoToStockAsync()
        => await Shell.Current.GoToAsync("///stock");

    [RelayCommand]
    private async Task GoToReportsAsync()
        => await Shell.Current.GoToAsync("///reports");

    [RelayCommand]
    private async Task GoToTransactionsAsync()
        => await Shell.Current.GoToAsync("///transactions");

    [RelayCommand]
    private async Task GoToProfitReportAsync()
        => await Shell.Current.GoToAsync("///profitreport");
}