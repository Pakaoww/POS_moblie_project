using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microcharts;
using POS_moblie_project.Services;
using SkiaSharp;

namespace POS_moblie_project.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;

    // ── Stock Summary ────────────────────────────────────────────────
    [ObservableProperty]
    private int totalProducts;

    [ObservableProperty]
    private int lowStockItems;

    [ObservableProperty]
    private int outOfStockItems;

    // ── Today Sales ──────────────────────────────────────────────────
    [ObservableProperty]
    private decimal todaySales;

    [ObservableProperty]
    private int todayTransactions;

    // ── Weekly Bar Chart (Microcharts) ───────────────────────────────
    /// <summary>
    /// Binding ตรงเข้า microcharts:ChartView Chart="{Binding WeeklyBarChart}"
    /// </summary>
    [ObservableProperty]

    private Chart weeklyBarChart;

    public HomeViewModel()
    {
        _databaseService = ServiceHelper.GetService<DatabaseService>();
    }

    // ── Commands ─────────────────────────────────────────────────────
    [RelayCommand]
    public async Task LoadDashboardAsync()
    {
        try
        {
            // FIX: reset chart ก่อน เพื่อบังคับ View detach ของเก่า
            WeeklyBarChart = null;

            // Stock
            var products = await _databaseService.GetAllProductsAsync();
            TotalProducts = products.Count;
            LowStockItems = products.Count(p => p.Stock > 0 && p.Stock <= 5);
            OutOfStockItems = products.Count(p => p.Stock <= 0);

            // Today sales
            var today = DateTime.Now.Date;
            var tomorrow = today.AddDays(1);
            var todayTx = await _databaseService.GetTransactionsAsync(today, tomorrow);
            TodayTransactions = todayTx.Count;
            TodaySales = todayTx.Sum(t => t.GrandTotal);

            // Weekly bar chart
            await GenerateWeeklySalesChartAsync();
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Error", ex.Message, "OK");
        }
    }

    /// <summary>
    /// สร้าง BarChart ยอดขาย 7 วันล่าสุด (ตามสไตล์ Family Co Finance)
    /// </summary>
    private async Task GenerateWeeklySalesChartAsync()
    {
        try
        {
            var today = DateTime.Now.Date;
            var from = today.AddDays(-6);
            var to = today.AddDays(1);

            var transactions = await _databaseService.GetTransactionsAsync(from, to);

            var salesByDate = transactions
                .GroupBy(t => t.Timestamp.Date)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.GrandTotal));

            var lineColor = SKColor.Parse("#76c8f3");
            var todayColor = SKColor.Parse("#a9d888");

            // ---- Empty state ----
            decimal totalSales = salesByDate.Values.DefaultIfEmpty(0).Sum();
            if (totalSales == 0)
            {
                WeeklyBarChart = null;
                WeeklyBarChart = new LineChart
                {
                    Entries = new[]
                    {
                    new ChartEntry(0)
                    {
                        Label    = "No data",
                        Color    = SKColor.Parse("#CCCCCC"),
                    }
                },
                    BackgroundColor = SKColors.Transparent,
                    LabelTextSize = 30f,
                    ValueLabelTextSize = 28f,
                };
                return;
            }

            // ---- สร้าง ChartEntry ครบ 7 วัน ----
            var entries = new List<ChartEntry>();
            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var amount = salesByDate.TryGetValue(date, out var v) ? v : 0m;

                string label = i == 0 ? "Today" : date.ToString("ddd");
                var color = i == 0 ? todayColor : lineColor;
                string valueLabel = amount == 0 ? "" : FormatAmount(amount);

                entries.Add(new ChartEntry((float)amount)
                {
                    Label = label,
                    ValueLabel = valueLabel,
                    Color = color,              // สีจุดแต่ละวัน
                    TextColor = SKColor.Parse("#3c3d3c"),
                    ValueLabelColor = SKColor.Parse("#3c3d3c"),
                });
            }

            WeeklyBarChart = null;
            // ---- LineChart — smooth curve + จุดกลม ----
            WeeklyBarChart = new LineChart
            {
                Entries = entries,
                BackgroundColor = SKColors.Transparent,

                LineMode = LineMode.Spline,
                LineSize = 3f,
                LineAreaAlpha = 40,

                PointMode = PointMode.Circle,
                PointSize = 14f,

                LabelTextSize = 30f,
                ValueLabelTextSize = 28f,
                LabelOrientation = Orientation.Horizontal,
                ValueLabelOrientation = Orientation.Horizontal,

                IsAnimated = false,   // ← ปิด animation แก้ปัญหาเส้นซ้อน
            };
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Error", $"Failed to generate chart: {ex.Message}", "OK");
        }
    }

    /// <summary>ย่อตัวเลข เช่น 12500 → 12.5K</summary>
    private static string FormatAmount(decimal amount)
    {
        if (amount >= 1_000_000m) return $"{amount / 1_000_000m:0.#}M";
        if (amount >= 1_000m) return $"{amount / 1_000m:0.#}K";
        return $"{amount:0}";
    }

    // ── Navigation Commands ──────────────────────────────────────────
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
}