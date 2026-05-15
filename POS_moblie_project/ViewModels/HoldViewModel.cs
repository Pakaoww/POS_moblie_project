using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS_moblie_project.Models;
using POS_moblie_project.Services;
using System.Collections.ObjectModel;

namespace POS_moblie_project.ViewModels;

public partial class HoldViewModel : ObservableObject
{
    private readonly POSViewModel _posViewModel;
    private readonly DatabaseService _databaseService;

    public ObservableCollection<HoldSession> Sessions { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedSession))]
    [NotifyPropertyChangedFor(nameof(SelectedItems))]
    [NotifyPropertyChangedFor(nameof(GrandTotal))]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    private HoldSession? _selectedSession;

    public bool HasSelectedSession => SelectedSession is not null;
    public ObservableCollection<HoldItem> SelectedItems => SelectedSession?.Items ?? new();
    public decimal GrandTotal => SelectedSession?.Total ?? 0;
    public bool IsEmpty => !Sessions.Any();
    public int SessionCount => Sessions.Count;

    public int TotalReserved(int productId)
        => Sessions.SelectMany(s => s.Items)
                   .Where(i => i.ProductId == productId)
                   .Sum(i => i.Quantity);

    public HoldViewModel()
    {
        _posViewModel = ServiceHelper.GetService<POSViewModel>();
        _databaseService = ServiceHelper.GetService<DatabaseService>();
        Sessions.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(SessionCount));
            OnPropertyChanged(nameof(IsEmpty));
        };
    }

    public void AddSession(IEnumerable<HoldItem> items)
    {
        var session = new HoldSession();
        foreach (var item in items)
            session.Items.Add(item);
        Sessions.Add(session);
        SelectedSession = session;
        _posViewModel.HoldCount = Sessions.Sum(s => s.ItemCount);
    }

    [RelayCommand]
    private void SelectSession(HoldSession session)
    {
        SelectedSession = session;
        RefreshTotals();
    }

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
            item.Quantity--;
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

    [RelayCommand]
    private async Task ResumeOrderAsync()
    {
        if (SelectedSession is null) return;
        foreach (var item in SelectedSession.Items)
        {
            // ดึง FIFO lot ก่อนสร้าง CartItem
            var lot = await _databaseService.GetFifoLotAsync(item.ProductId);
            if (lot == null) continue;

            var existing = _posViewModel.CartItems
                .FirstOrDefault(c => c.ProductId == item.ProductId);
            if (existing != null)
                existing.Quantity += item.Quantity;
            else
                _posViewModel.CartItems.Add(
                    new CartItem(item.Product, lot, item.Quantity));

            var posItem = _posViewModel.Products
                .FirstOrDefault(p => p.ProductId == item.ProductId);
            if (posItem is not null) posItem.Quantity = 0;
        }
        _posViewModel.CartCount = _posViewModel.CartItems.Sum(x => x.Quantity);
        RemoveSelectedSession();
        RefreshTotals();
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task CheckoutAsync()
    {
        if (SelectedSession is null || !SelectedSession.Items.Any())
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Empty", "No items in this session.", "OK");
            return;
        }
        foreach (var item in SelectedSession.Items)
        {
            // ดึง FIFO lot ก่อนสร้าง CartItem
            var lot = await _databaseService.GetFifoLotAsync(item.ProductId);
            if (lot == null) continue;

            var existing = _posViewModel.CartItems
                .FirstOrDefault(c => c.ProductId == item.ProductId);
            if (existing != null)
                existing.Quantity += item.Quantity;
            else
                _posViewModel.CartItems.Add(
                    new CartItem(item.Product, lot, item.Quantity));
        }
        _posViewModel.CartCount = _posViewModel.CartItems.Sum(x => x.Quantity);
        RemoveSelectedSession();
        RefreshTotals();
        await Shell.Current.GoToAsync("CartPage");
    }

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
        OnPropertyChanged(nameof(SelectedSession));
    }
}