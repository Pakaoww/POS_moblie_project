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

        // ── ลบ auto-confirm ออก ─────────────────────────────
        // ผู้ใช้ต้องกด OK เองเท่านั้น
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

        await Shell.Current.GoToAsync("//settings/AdminPanelPage");
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        EnteredPin = string.Empty;
        await Shell.Current.GoToAsync("..");
    }

    // ── Static helpers ────────────────────────────────────

    public static async Task<bool> VerifyAdminAsync(string pin)
    {
        var saved = await SecureStorage.GetAsync(AdminPinKey);
        return saved is null || saved == pin;
    }

    /// <summary>ตรวจสอบว่ามี Admin PIN ตั้งไว้แล้วหรือยัง</summary>
    public static async Task<bool> HasAdminPinAsync()
    {
        var saved = await SecureStorage.GetAsync(AdminPinKey);
        return !string.IsNullOrWhiteSpace(saved);
    }
}