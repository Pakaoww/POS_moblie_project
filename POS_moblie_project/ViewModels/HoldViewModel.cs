using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS_moblie_project.Models;
using System.Collections.ObjectModel;

namespace POS_moblie_project.ViewModels;

public partial class HoldViewModel : ObservableObject
{
    private readonly POSViewModel _posViewModel;

    // ════════════════════════════════════════════════════════
    //  SESSIONS (หลายตะกร้า)
    // ════════════════════════════════════════════════════════

    public ObservableCollection<HoldSession> Sessions { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedSession))]
    [NotifyPropertyChangedFor(nameof(SelectedItems))]
    [NotifyPropertyChangedFor(nameof(GrandTotal))]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    private HoldSession? _selectedSession;

    public bool HasSelectedSession => SelectedSession is not null;

    public ObservableCollection<HoldItem> SelectedItems
        => SelectedSession?.Items ?? new();

    public decimal GrandTotal
        => SelectedSession?.Total ?? 0;

    public bool IsEmpty => !Sessions.Any();

    // จำนวนตะกร้าทั้งหมด (แสดงบน badge)
    public int SessionCount => Sessions.Count;

    // ════════════════════════════════════════════════════════
    //  CONSTRUCTOR
    // ════════════════════════════════════════════════════════

    public HoldViewModel()
    {
        _posViewModel = ServiceHelper.GetService<POSViewModel>();
        Sessions.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(SessionCount));
            OnPropertyChanged(nameof(IsEmpty));
        };
    }

    // ════════════════════════════════════════════════════════
    //  ADD NEW SESSION (เรียกจาก POSViewModel ตอนกด Hold)
    // ════════════════════════════════════════════════════════

    public void AddSession(IEnumerable<HoldItem> items)
    {
        var session = new HoldSession();
        foreach (var item in items)
            session.Items.Add(item);

        Sessions.Add(session);

        // เลือก session ใหม่เป็น active
        SelectedSession = session;

        // อัป HoldCount ใน POS
        _posViewModel.HoldCount = Sessions.Sum(s => s.ItemCount);
    }

    // ════════════════════════════════════════════════════════
    //  SELECT SESSION
    // ════════════════════════════════════════════════════════

    [RelayCommand]
    private void SelectSession(HoldSession session)
    {
        SelectedSession = session;
        RefreshTotals();
    }

    // ════════════════════════════════════════════════════════
    //  ITEM COMMANDS (ใน selected session)
    // ════════════════════════════════════════════════════════

    [RelayCommand]
    private void IncreaseItem(HoldItem item)
    {
        if (item is null || SelectedSession is null) return;
        item.Quantity++;
        RefreshTotals();
    }

    [RelayCommand]
    private void DecreaseItem(HoldItem item)
    {
        if (item is null || SelectedSession is null) return;
        if (item.Quantity > 1)
        {
            item.Quantity--;
        }
        else
        {
            SelectedSession.Items.Remove(item);
            if (SelectedSession.Items.Count == 0)
                RemoveSelectedSession();
        }
        RefreshTotals();
    }

    [RelayCommand]
    private void RemoveItem(HoldItem item)
    {
        if (item is null || SelectedSession is null) return;
        SelectedSession.Items.Remove(item);
        if (SelectedSession.Items.Count == 0)
            RemoveSelectedSession();
        RefreshTotals();
    }

    // ════════════════════════════════════════════════════════
    //  SESSION COMMANDS
    // ════════════════════════════════════════════════════════

    [RelayCommand]
    private void ClearHold()
    {
        if (SelectedSession is null) return;
        RemoveSelectedSession();
        RefreshTotals();
    }

    [RelayCommand]
    private void ClearAllSessions()
    {
        Sessions.Clear();
        SelectedSession = null;
        _posViewModel.HoldCount = 0;
        RefreshTotals();
    }

    // ════════════════════════════════════════════════════════
    //  RESUME — โหลด session กลับไป POS
    // ════════════════════════════════════════════════════════

    [RelayCommand]
    private async Task ResumeOrderAsync()
    {
        if (SelectedSession is null) return;

        // โหลด items กลับไปที่ cart ของ POS
        _posViewModel.CartItems.Clear();
        foreach (var item in SelectedSession.Items)
        {
            _posViewModel.CartItems.Add(
                new CartItem(item.Product, item.Quantity));

            // อัป quantity ใน product list ด้วย
            var product = _posViewModel.Products
                .FirstOrDefault(p => p.ProductId == item.ProductId);
            if (product is not null)
                product.Quantity = item.Quantity;
        }
        _posViewModel.CartCount =
            _posViewModel.CartItems.Sum(x => x.Quantity);

        // ลบ session นี้ออก
        RemoveSelectedSession();
        RefreshTotals();

        await Shell.Current.GoToAsync("..");
    }

    // ════════════════════════════════════════════════════════
    //  CHECKOUT — ย้าย session ไปหน้า cart
    // ════════════════════════════════════════════════════════

    [RelayCommand]
    private async Task CheckoutAsync()
    {
        if (SelectedSession is null || !SelectedSession.Items.Any())
        {
            await Application.Current.MainPage.DisplayAlert(
                "Empty", "No items in this session.", "OK");
            return;
        }

        // ย้าย items ของ session นี้ไป cart
        _posViewModel.CartItems.Clear();
        foreach (var item in SelectedSession.Items)
        {
            _posViewModel.CartItems.Add(
                new CartItem(item.Product, item.Quantity));
        }
        _posViewModel.CartCount =
            _posViewModel.CartItems.Sum(x => x.Quantity);

        // ลบ session นี้ออก (hold หาย แต่ cart ยังอยู่)
        RemoveSelectedSession();
        RefreshTotals();

        await Shell.Current.GoToAsync("CartPage");
    }

    // ════════════════════════════════════════════════════════
    //  HELPERS
    // ════════════════════════════════════════════════════════

    private void RemoveSelectedSession()
    {
        if (SelectedSession is null) return;
        Sessions.Remove(SelectedSession);
        SelectedSession = Sessions.LastOrDefault();
        _posViewModel.HoldCount = Sessions.Sum(s => s.ItemCount);
    }

    private void RefreshTotals()
    {
        OnPropertyChanged(nameof(GrandTotal));
        OnPropertyChanged(nameof(SelectedItems));
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(SessionCount));
        if (SelectedSession is not null)
        {
            OnPropertyChanged(nameof(SelectedSession));
        }
    }
}