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

    // Stats
    [ObservableProperty]
    private int totalTransactions;

    [ObservableProperty]
    private decimal averageRevenue;

    [ObservableProperty]
    private decimal totalRevenue;

    public TransactionHistoryViewModel()
    {
        _databaseService = ServiceHelper.GetService<DatabaseService>();
    }

    partial void OnSearchTextChanged(string value) => ApplySearch();

    [RelayCommand]
    public async Task LoadTransactionsAsync()
    {
        IsLoading = true;
        try
        {
            _allTransactions = await _databaseService
                .GetTransactionsAsync(FromDate, ToDate);

            ApplySearch();
            CalculateStats();
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
            await Application.Current.MainPage.DisplayAlert(
                "No Data", "No transactions to export.", "OK");
            return;
        }

        try
        {
            var backupService = ServiceHelper.GetService<BackupService>();
            var filePath = await backupService
                .ExportTransactionHistoryAsync(_allTransactions);

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Export Transaction History",
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