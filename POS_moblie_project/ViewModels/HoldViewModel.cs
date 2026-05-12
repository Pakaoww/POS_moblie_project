using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS_moblie_project.Models;
using System.Collections.ObjectModel;

namespace POS_moblie_project.ViewModels;

public partial class HoldViewModel : ObservableObject
{
    private readonly POSViewModel _posViewModel;

    // ใช้ HoldItems ตัวเดียวกับ POS
    public ObservableCollection<HoldItem> HoldItems
        => _posViewModel.HoldItems;

    public int HoldCount
        => _posViewModel.HoldCount;

    [ObservableProperty]
    private decimal grandTotal;

    public HoldViewModel()
    {
        _posViewModel = ServiceHelper.GetService<POSViewModel>();

        RecalculateTotal();
    }

    // =========================
    // TOTAL
    // =========================

    private void RecalculateTotal()
    {
        GrandTotal = HoldItems.Sum(x => x.Subtotal);
    }

    // =========================
    // REMOVE
    // =========================

    [RelayCommand]
    private void RemoveItem(HoldItem item)
    {
        if (item == null)
            return;

        HoldItems.Remove(item);

        _posViewModel.HoldCount =
            HoldItems.Sum(x => x.Quantity);

        RecalculateTotal();
    }

    // =========================
    // CLEAR
    // =========================

    [RelayCommand]
    private void ClearHold()
    {
        HoldItems.Clear();

        _posViewModel.HoldCount = 0;

        RecalculateTotal();
    }

    // =========================
    // RESUME
    // =========================

    [RelayCommand]
    private async Task ResumeOrderAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    // =========================
    // CHECKOUT
    // =========================

    [RelayCommand]
    private async Task CheckoutAsync()
    {
        if (HoldItems.Count == 0)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Empty",
                "No hold items.",
                "OK");

            return;
        }

        // =========================
        // MOVE HOLD -> CART
        // =========================

        _posViewModel.CartItems.Clear();

        foreach (var item in HoldItems)
        {
            _posViewModel.CartItems.Add(
                new CartItem(
                    item.Product,
                    item.Quantity));
        }

        // Update cart count
        _posViewModel.CartCount =
            _posViewModel.CartItems.Sum(x => x.Quantity);

        // =========================
        // GO TO CART PAGE
        // =========================

        await Shell.Current.GoToAsync("CartPage");
    }
}