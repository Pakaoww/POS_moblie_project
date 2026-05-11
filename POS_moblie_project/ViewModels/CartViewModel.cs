using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS_moblie_project.Models;
using POS_moblie_project.Services;
using System.Collections.ObjectModel;

namespace POS_moblie_project.ViewModels;

public enum CartState
{
    Cart,
    Confirm,
    Charge,
    Receipt
}

public partial class CartViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;
    private readonly POSViewModel _posViewModel;

    public ObservableCollection<CartItem> CartItems => _posViewModel.CartItems;

    [ObservableProperty]
    private CartState currentState = CartState.Cart;

    [ObservableProperty]
    private decimal subtotal;

    [ObservableProperty]
    private decimal vatRate;

    [ObservableProperty]
    private decimal vatAmount;

    [ObservableProperty]
    private decimal grandTotal;

    [ObservableProperty]
    private bool isVatEnabled;

    [ObservableProperty]
    private string moneyReceivedInput = string.Empty;

    [ObservableProperty]
    private decimal moneyReceived;

    [ObservableProperty]
    private decimal change;

    [ObservableProperty]
    private string receiptTransactionId = string.Empty;

    [ObservableProperty]
    private DateTime receiptTimestamp;

    public CartViewModel()
    {
        _databaseService = ServiceHelper.GetService<DatabaseService>();
        _posViewModel = ServiceHelper.GetService<POSViewModel>();
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        CurrentState = CartState.Cart;
        MoneyReceivedInput = string.Empty;
        MoneyReceived = 0;
        Change = 0;

        IsVatEnabled = (await _databaseService.GetSettingAsync("vat_enabled")) == "true";
        var rateStr = await _databaseService.GetSettingAsync("vat_rate");
        VatRate = decimal.TryParse(rateStr, out var r) ? r : 0;

        RecalculateTotals();
    }

    private void RecalculateTotals()
    {
        Subtotal = CartItems.Sum(i => i.Subtotal);
        VatAmount = IsVatEnabled ? Math.Round(Subtotal * VatRate / 100, 2) : 0;
        GrandTotal = Subtotal + VatAmount;
    }

    // ── Cart actions ──────────────────────────────────────

    [RelayCommand]
    private void IncreaseItem(CartItem item)
    {
        if (item == null) return;
        item.Quantity++;
        RecalculateTotals();
        SyncToPos(item);
    }

    [RelayCommand]
    private void DecreaseItem(CartItem item)
    {
        if (item == null) return;
        if (item.Quantity > 1)
            item.Quantity--;
        else
        {
            CartItems.Remove(item);
            var posItem = _posViewModel.Products
                .FirstOrDefault(p => p.ProductId == item.ProductId);
            if (posItem != null) posItem.Quantity = 0;
        }
        RecalculateTotals();
        _posViewModel.CartCount = CartItems.Sum(c => c.Quantity);
    }

    [RelayCommand]
    private void RemoveItem(CartItem item)
    {
        if (item == null) return;
        CartItems.Remove(item);
        var posItem = _posViewModel.Products
            .FirstOrDefault(p => p.ProductId == item.ProductId);
        if (posItem != null) posItem.Quantity = 0;
        RecalculateTotals();
        _posViewModel.CartCount = CartItems.Sum(c => c.Quantity);
    }

    private void SyncToPos(CartItem item)
    {
        var posItem = _posViewModel.Products
            .FirstOrDefault(p => p.ProductId == item.ProductId);
        if (posItem != null) posItem.Quantity = item.Quantity;
        _posViewModel.CartCount = CartItems.Sum(c => c.Quantity);
    }

    // ── Checkout flow ──────────────────────────────────────

    [RelayCommand]
    private void Checkout()
    {
        RecalculateTotals();
        CurrentState = CartState.Confirm;
    }

    [RelayCommand]
    private void BackToCart() => CurrentState = CartState.Cart;

    [RelayCommand]
    private void GoToCharge()
    {
        MoneyReceivedInput = string.Empty;
        MoneyReceived = 0;
        Change = 0;
        CurrentState = CartState.Charge;
    }

    // ── Charge keypad ──────────────────────────────────────

    [RelayCommand]
    private void ChargeDigitPressed(string digit)
    {
        if (MoneyReceivedInput.Length >= 10) return;
        if (digit == "." && MoneyReceivedInput.Contains(".")) return;
        if (digit == "." && MoneyReceivedInput.Length == 0)
            MoneyReceivedInput = "0.";
        else
            MoneyReceivedInput += digit;

        if (decimal.TryParse(MoneyReceivedInput, out var amount))
        {
            MoneyReceived = amount;
            Change = MoneyReceived - GrandTotal;
        }
    }

    [RelayCommand]
    private void ChargeDelete()
    {
        if (MoneyReceivedInput.Length == 0) return;
        MoneyReceivedInput = MoneyReceivedInput[..^1];

        if (decimal.TryParse(MoneyReceivedInput, out var amount))
        {
            MoneyReceived = amount;
            Change = MoneyReceived - GrandTotal;
        }
        else
        {
            MoneyReceived = 0;
            Change = -GrandTotal;
        }
    }

    [RelayCommand]
    private void BackToConfirm() => CurrentState = CartState.Confirm;

    [RelayCommand]
    private async Task ConfirmChargeAsync()
    {
        if (MoneyReceived < GrandTotal)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Insufficient", "Money received is less than the total.", "OK");
            return;
        }

        try
        {
            var transactionId = await _databaseService.GenerateTransactionIdAsync();

            var transaction = new Transaction(
                transactionId,
                Subtotal,
                IsVatEnabled ? VatRate : 0,
                VatAmount,
                GrandTotal,
                MoneyReceived,
                Change);

            var items = CartItems.Select(c => new TransactionItem(
                0,
                c.ProductId,
                c.ProductName,
                c.UnitPrice,
                c.Quantity)).ToList();

            await _databaseService.CreateTransactionAsync(transaction, items);

            ReceiptTransactionId = transactionId;
            ReceiptTimestamp = transaction.Timestamp;
            CurrentState = CartState.Receipt;
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Error", $"Failed to save transaction: {ex.Message}", "OK");
        }
    }

    // ── Receipt ────────────────────────────────────────────

    [RelayCommand]
    private async Task GoHomeAsync()
    {
        _posViewModel.ClearCart();
        await Shell.Current.GoToAsync("//pos");
    }
}