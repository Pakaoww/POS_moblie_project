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
    [NotifyPropertyChangedFor(nameof(CheckoutButtonText))]
    private decimal grandTotal;

    [ObservableProperty]
    private bool isVatEnabled;

    public string CheckoutButtonText =>
        $"Checkout  {ServiceHelper.GetService<CurrencyService>().Format(GrandTotal)}";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DiscountTypeIsPercent))]
    [NotifyPropertyChangedFor(nameof(DiscountTypeIsAmount))]
    private string discountType = "none";

    public bool DiscountTypeIsPercent => DiscountType == "percent";
    public bool DiscountTypeIsAmount => DiscountType == "amount";

    [ObservableProperty]
    private string discountInput = string.Empty;

    [ObservableProperty]
    private decimal discountValue;

    [ObservableProperty]
    private decimal discountAmount;

    [ObservableProperty]
    private bool isDiscountSectionVisible = false;

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

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PrintButtonText))]
    [NotifyPropertyChangedFor(nameof(PrintButtonColor))]
    private bool isReceiptPrinted = false;

    public string PrintButtonText => IsReceiptPrinted ? "Receipt Printed" : "🖨️  Print Receipt";
    public Color PrintButtonColor => IsReceiptPrinted
        ? Color.FromArgb("#AAAAAA")
        : Color.FromArgb("#FF6B6B");

    public CartViewModel()
    {
        _databaseService = ServiceHelper.GetService<DatabaseService>();
        _posViewModel = ServiceHelper.GetService<POSViewModel>();

        ServiceHelper.GetService<CurrencyService>().SettingChanged += () =>
        {
            OnPropertyChanged(nameof(CheckoutButtonText));
        };
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        CurrentState = CartState.Cart;
        MoneyReceivedInput = string.Empty;
        MoneyReceived = 0;
        Change = 0;
        DiscountType = "none";
        DiscountInput = string.Empty;
        DiscountValue = 0;
        DiscountAmount = 0;
        IsDiscountSectionVisible = false;
        IsReceiptPrinted = false;

        IsVatEnabled = (await _databaseService.GetSettingAsync("vat_enabled")) == "true";
        var rateStr = await _databaseService.GetSettingAsync("vat_rate");
        VatRate = decimal.TryParse(rateStr, out var r) ? r : 0;

        RecalculateTotals();
    }

    private void RecalculateTotals()
    {
        Subtotal = CartItems.Sum(i => i.Subtotal);

        if (DiscountType == "percent")
            DiscountAmount = Math.Round(Subtotal * DiscountValue / 100, 2);
        else if (DiscountType == "amount")
            DiscountAmount = Math.Min(DiscountValue, Subtotal);
        else
            DiscountAmount = 0;

        var afterDiscount = Subtotal - DiscountAmount;
        VatAmount = IsVatEnabled ? Math.Round(afterDiscount * VatRate / 100, 2) : 0;
        GrandTotal = afterDiscount + VatAmount;
    }

    // ── Cart actions ──────────────────────────────────────

    [RelayCommand]
    private async Task IncreaseItemAsync(CartItem item)
    {
        if (item == null) return;

        // เช็ค stock จาก lots แทน product.Stock
        var totalStock = await _databaseService.GetTotalStockAsync(item.ProductId);
        if (item.Quantity >= totalStock)
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Stock Limit",
                $"Only {totalStock} unit(s) available in stock.",
                "OK");
            return;
        }

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

    // ── Discount actions ──────────────────────────────────

    [RelayCommand]
    private void ToggleDiscountSection()
    {
        IsDiscountSectionVisible = !IsDiscountSectionVisible;
        if (!IsDiscountSectionVisible)
        {
            DiscountType = "none";
            DiscountInput = string.Empty;
            DiscountValue = 0;
            RecalculateTotals();
        }
    }

    [RelayCommand]
    private void SetDiscountType(string type)
    {
        DiscountType = type;
        DiscountInput = string.Empty;
        DiscountValue = 0;
        RecalculateTotals();
    }

    [RelayCommand]
    private void ApplyDiscount()
    {
        if (decimal.TryParse(DiscountInput, out var val) && val >= 0)
        {
            if (DiscountType == "percent" && val > 100)
                val = 100;
            DiscountValue = val;
        }
        else
        {
            DiscountValue = 0;
        }
        RecalculateTotals();
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
            await Application.Current!.MainPage!.DisplayAlert(
                "Insufficient", "Money received is less than the total.", "OK");
            return;
        }

        try
        {
            var transactionId = await _databaseService.GenerateTransactionIdAsync();

            var transaction = new Transaction(
                transactionId,
                Subtotal,
                DiscountType,
                DiscountValue,
                DiscountAmount,
                IsVatEnabled ? VatRate : 0,
                VatAmount,
                GrandTotal,
                MoneyReceived,
                Change);

            // ← แก้: ใช้ constructor ใหม่ที่มี LotId และ UnitCost
            var items = CartItems.Select(c => new TransactionItem(
                0,
                c.ProductId,
                c.LotId,
                c.ProductName,
                c.UnitCost,
                c.UnitPrice,
                c.Quantity)).ToList();

            // CreateTransactionAsync จัดการลด lot.Remaining ให้แล้ว
            // ไม่ต้อง update product.Stock เองอีกต่อไป
            await _databaseService.CreateTransactionAsync(transaction, items);

            ReceiptTransactionId = transactionId;
            ReceiptTimestamp = transaction.Timestamp;
            CurrentState = CartState.Receipt;
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Error", $"Failed to save transaction: {ex.Message}", "OK");
        }
    }

    // ── Receipt ────────────────────────────────────────────

    public event Func<Task<bool>>? PrintReceiptRequested;

    [RelayCommand]
    private async Task PrintReceiptAsync()
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
    private async Task GoHomeAsync()
    {
        _posViewModel.ClearCart();
        await Shell.Current.GoToAsync("//pos");
    }
}