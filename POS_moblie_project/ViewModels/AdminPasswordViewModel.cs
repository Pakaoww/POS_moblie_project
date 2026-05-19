using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS_moblie_project.Services;

namespace POS_moblie_project.ViewModels;

public partial class AdminPasswordViewModel : ObservableObject
{
    [ObservableProperty]
    private string _statusMessage = "Enter Admin Password";

    [ObservableProperty]
    private bool _isError;

    [ObservableProperty]
    private string _enteredPin = string.Empty;

    private string _targetRoute = "//settings/AdminPanelPage";

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

    public void SetTarget(string target)
    {
        _targetRoute = string.IsNullOrWhiteSpace(target)
            ? "//settings/AdminPanelPage"
            : target;
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

        bool isValid = await PageLockService.VerifyAdminPinAsync(pin);

        if (!isValid)
        {
            IsError = true;
            StatusMessage = "Incorrect admin password";
            return;
        }

        if (_targetRoute != "//settings/AdminPanelPage")
        {
            PageLockService.Authorize(_targetRoute);
            // Pop กลับไปหน้าที่ถูก lock (TransactionHistoryPage ฯลฯ)
            // ตอนนี้ OnAppearing ของหน้านั้นจะ ConsumeAuthorization แล้วโหลดตามปกติ
            await Shell.Current.GoToAsync("..");
            return;
        }

        await Shell.Current.GoToAsync("//settings/AdminPanelPage");
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        EnteredPin = string.Empty;
        // ถ้ามี target แสดงว่ามาจาก Locked Page → กลับ Dashboard ทันที
        // เพื่อไม่ให้ loop redirect กลับมาที่นี่อีก
        if (_targetRoute != "//settings/AdminPanelPage")
            await Shell.Current.GoToAsync("///home");
        else
            await Shell.Current.GoToAsync("..");
    }

}