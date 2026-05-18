namespace POS_moblie_project.Views.POS;

public partial class ConfirmPaymentPage : ContentPage
{
    private readonly TaskCompletionSource<bool> _tcs = new();

    public bool IsConfirmed { get; private set; } = false;

    public ConfirmPaymentPage(string grandTotal, string moneyReceived, string change)
    {
        InitializeComponent();

        // ผูกค่าที่ส่งมาเข้า Label
        GrandTotalLabel.Text = grandTotal;
        MoneyReceivedLabel.Text = moneyReceived;
        ChangeLabel.Text = change;
    }

    // ── รอจนกว่าจะ dismiss (เรียกจาก CartViewModel) ──────
    public Task WaitForDismissAsync() => _tcs.Task;

    // ── Confirm ───────────────────────────────────────────
    private async void OnConfirmClicked(object sender, EventArgs e)
    {
        IsConfirmed = true;
        await Navigation.PopModalAsync(animated: true);
        _tcs.TrySetResult(true);
    }

    // ── Cancel ────────────────────────────────────────────
    private async void OnCancelClicked(object sender, EventArgs e)
    {
        IsConfirmed = false;
        await Navigation.PopModalAsync(animated: true);
        _tcs.TrySetResult(false);
    }

    // ── Tap นอก card ─────────────────────────────────────
    private async void OnBackgroundTapped(object sender, EventArgs e)
    {
        IsConfirmed = false;
        await Navigation.PopModalAsync(animated: true);
        _tcs.TrySetResult(false);
    }

    // ── กด Back button (Android) ──────────────────────────
    protected override bool OnBackButtonPressed()
    {
        IsConfirmed = false;
        Navigation.PopModalAsync(animated: true);
        _tcs.TrySetResult(false);
        return true;
    }
}