using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS_moblie_project.Models;
using POS_moblie_project.Services;
using System.Collections.ObjectModel;

namespace POS_moblie_project.ViewModels;

[QueryProperty(nameof(TransactionId), "id")]
public partial class TransactionDetailViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;

    [ObservableProperty] private string transactionId = string.Empty;
    [ObservableProperty] private DateTime timestamp;
    [ObservableProperty] private ObservableCollection<TransactionItem> items = new();
    [ObservableProperty] private decimal subtotal;
    [ObservableProperty] private decimal vatRate;
    [ObservableProperty] private decimal vatAmount;
    [ObservableProperty] private decimal grandTotal;
    [ObservableProperty] private bool isVatEnabled;
    [ObservableProperty] private bool isLoading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasDiscount))]
    [NotifyPropertyChangedFor(nameof(IsDiscountPercent))]
    [NotifyPropertyChangedFor(nameof(IsDiscountAmount))]
    private string discountType = "none";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasDiscount))]
    private decimal discountAmount;

    [ObservableProperty] private decimal discountValue;

    [ObservableProperty] private bool isReceiptPrinted = false;

    public Color ReprintButtonColor => IsReceiptPrinted ? Colors.Gray : Color.FromArgb("#FF6B6B");

    public bool HasDiscount => DiscountAmount > 0;
    public bool IsDiscountPercent => DiscountType == "percent";
    public bool IsDiscountAmount => DiscountType == "amount";

    public event Func<Task<bool>>? PrintReceiptRequested;

    public TransactionDetailViewModel()
    {
        _databaseService = ServiceHelper.GetService<DatabaseService>();
    }

    partial void OnTransactionIdChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
            _ = LoadTransactionAsync(value);
    }

    partial void OnIsReceiptPrintedChanged(bool value)
    {
        OnPropertyChanged(nameof(ReprintButtonColor));
    }

    private async Task LoadTransactionAsync(string id)
    {
        IsLoading = true;
        try
        {
            var (transaction, transactionItems) =
                await _databaseService.GetTransactionWithItemsAsync(id);

            if (transaction == null) return;

            Timestamp = transaction.Timestamp;
            Subtotal = transaction.TotalAmount;
            VatRate = transaction.VatRate;
            VatAmount = transaction.VatAmount;
            GrandTotal = transaction.GrandTotal;
            IsVatEnabled = transaction.VatRate > 0;
            DiscountType = transaction.DiscountType ?? "none";
            DiscountValue = transaction.DiscountValue;
            DiscountAmount = transaction.DiscountAmount;

            Items.Clear();
            foreach (var item in transactionItems)
                Items.Add(item);
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ReprintReceiptAsync()
    {
        if (IsReceiptPrinted)
        {
            await Shell.Current.DisplayAlert(
                "Already Printed",
                "The receipt has already been printed to your gallery successfully.",
                "OK");
            return;
        }

        if (PrintReceiptRequested != null)
        {
            bool success = await PrintReceiptRequested.Invoke();
            if (success)
                IsReceiptPrinted = true;
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}