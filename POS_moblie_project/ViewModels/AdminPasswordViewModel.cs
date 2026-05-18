using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace POS_moblie_project.ViewModels;

public partial class AdminPasswordViewModel : ObservableObject
{
    private const string AdminPinKey = "admin_pin";

    [ObservableProperty]
    private string _statusMessage = "Enter Admin Password";

    [ObservableProperty]
    private bool _isError;

    [ObservableProperty]
    private string _enteredPin = string.Empty;

    // ════════════════════════════════════════════════════════
    //  INIT
    // ════════════════════════════════════════════════════════

    [RelayCommand]
    private void Initialize()
    {
        EnteredPin = string.Empty;
        IsError = false;
        StatusMessage = "Enter Admin Password";
    }

    // ════════════════════════════════════════════════════════
    //  KEYPAD
    // ════════════════════════════════════════════════════════

    [RelayCommand]
    private void DigitPressed(string digit)
    {
        if (EnteredPin.Length >= 6) return;
        IsError = false;
        StatusMessage = "Enter Admin Password";
        EnteredPin += digit;

        if (EnteredPin.Length == 6)
            ConfirmCommand.Execute(null);
    }

    [RelayCommand]
    private void Delete()
    {
        if (EnteredPin.Length == 0) return;
        IsError = false;
        EnteredPin = EnteredPin[..^1];
    }

    [RelayCommand]
    private async Task ConfirmAsync()
    {
        if (EnteredPin.Length < 6)
        {
            IsError = true;
            StatusMessage = "Please enter all 6 digits";
            return;
        }

        var pin = EnteredPin;
        EnteredPin = string.Empty;

        var saved = await SecureStorage.GetAsync(AdminPinKey);
        bool isValid = saved is null || saved == pin;

        if (!isValid)
        {
            IsError = true;
            StatusMessage = "Incorrect admin password";
            return;
        }

        // ── ใช้ absolute path เพื่อ clear navigation stack ──
        await Shell.Current.GoToAsync("//settings/AdminPanelPage");
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        EnteredPin = string.Empty;
        await Shell.Current.GoToAsync("..");
    }

    // ── Static helper ─────────────────────────────────────
    public static async Task<bool> VerifyAdminAsync(string pin)
    {
        var saved = await SecureStorage.GetAsync(AdminPinKey);
        return saved is null || saved == pin;
    }
}