using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace POS_moblie_project.ViewModels.Settings;

public partial class SettingsViewModel : ObservableObject
{
    // ════════════════════════════════════════════════════════
    //  VAT
    // ════════════════════════════════════════════════════════

    [ObservableProperty]
    private bool _vatEnabled = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VatRateDisplay))]
    private int _vatRate = 7;

    public string VatRateDisplay => $"{VatRate} %";

    [RelayCommand]
    private void IncreaseVat()
    {
        if (VatRate < 30) VatRate++;
    }

    [RelayCommand]
    private void DecreaseVat()
    {
        if (VatRate > 0) VatRate--;
    }

    // ════════════════════════════════════════════════════════
    //  CHANGE PASSWORD (3 รอบด้วย PasswordPage)
    // ════════════════════════════════════════════════════════

    [RelayCommand]
    private async Task ChangePasswordAsync()
    {
        // ── รอบ 1: ยืนยัน PIN ปัจจุบัน ──────────────────
        var currentPin = await NavigateToPasswordPageAsync(
            statusMessage: "Enter current PIN",
            mode: PasswordMode.VerifyCurrent);

        if (currentPin is null) return; // กด Cancel

        bool isCurrentValid = await VerifyCurrentPinAsync(currentPin);
        if (!isCurrentValid)
        {
            await Shell.Current.DisplayAlert("Error", "Incorrect current PIN.", "OK");
            return;
        }

        // ── รอบ 2: ใส่ PIN ใหม่ ──────────────────────────
        var newPin = await NavigateToPasswordPageAsync(
            statusMessage: "Enter new PIN",
            mode: PasswordMode.EnterNew);

        if (newPin is null) return;

        // ── รอบ 3: ยืนยัน PIN ใหม่ ───────────────────────
        var confirmPin = await NavigateToPasswordPageAsync(
            statusMessage: "Confirm new PIN",
            mode: PasswordMode.ConfirmNew);

        if (confirmPin is null) return;

        if (newPin != confirmPin)
        {
            await Shell.Current.DisplayAlert("Error", "PINs do not match. Please try again.", "OK");
            return;
        }

        // ── บันทึก PIN ใหม่ ───────────────────────────────
        await SaveNewPinAsync(newPin);
        await Shell.Current.DisplayAlert("Success", "PIN changed successfully.", "OK");
    }

    // ════════════════════════════════════════════════════════
    //  HELPERS
    // ════════════════════════════════════════════════════════

    /// <summary>
    /// Navigate ไปหน้า PasswordPage พร้อม parameter และรอรับ PIN ที่กด OK
    /// คืนค่า PIN string หรือ null ถ้ากด Cancel
    /// </summary>
    private static async Task<string?> NavigateToPasswordPageAsync(
        string statusMessage,
        PasswordMode mode)
    {
        var tcs = new TaskCompletionSource<string?>();

        var navParam = new Dictionary<string, object>
        {
            { "StatusMessage", statusMessage },
            { "Mode",          mode          },
            { "ResultCallback", (Action<string?>)(pin => tcs.TrySetResult(pin)) }
        };

        await Shell.Current.GoToAsync("passwordPage", navParam);

        return await tcs.Task;
    }

    /// <summary>ตรวจสอบ PIN ปัจจุบันกับที่เก็บไว้ — TODO: ใช้ SecureStorage จริง</summary>
    private static async Task<bool> VerifyCurrentPinAsync(string pin)
    {
        await Task.CompletedTask;
        var saved = await SecureStorage.GetAsync("user_pin");
        // ถ้ายังไม่มี PIN บันทึกไว้ให้ถือว่าผ่านเลย (first-time setup)
        return saved is null || saved == pin;
    }

    /// <summary>บันทึก PIN ใหม่ลง SecureStorage — TODO: hash ก่อนเก็บใน production</summary>
    private static async Task SaveNewPinAsync(string pin)
    {
        await SecureStorage.SetAsync("user_pin", pin);
    }
}

/// <summary>บอก PasswordPage ว่าอยู่ขั้นตอนไหน เพื่อแสดง UI ให้ถูกต้อง</summary>
public enum PasswordMode
{
    VerifyCurrent,
    EnterNew,
    ConfirmNew
}