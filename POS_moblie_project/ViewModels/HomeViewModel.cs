using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS_moblie_project.Services;

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

    public HomeViewModel()
    {
        _databaseService = ServiceHelper.GetService<DatabaseService>();
    }

    [RelayCommand]
    public async Task LoadDashboardAsync()
    {
        try
        {
            var products = await _databaseService.GetAllProductsAsync();
            TotalProducts = products.Count;
            LowStockItems = products.Count(p => p.Stock > 0 && p.Stock <= 5);
            OutOfStockItems = products.Count(p => p.Stock <= 0);

            var today = DateTime.Now.Date;
            var tomorrow = today.AddDays(1);
            var todayTx = await _databaseService.GetTransactionsAsync(today, tomorrow);
            TodayTransactions = todayTx.Count;
            TodaySales = todayTx.Sum(t => t.GrandTotal);
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Error", ex.Message, "OK");
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
}