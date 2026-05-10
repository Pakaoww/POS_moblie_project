using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace POS_moblie_project.ViewModels.Settings;

public partial class SettingsViewModel : ObservableObject
{
    // ── VAT ────────────────────────────────────────────────

    [ObservableProperty]
    private bool _vatEnabled = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VatRateDisplay))]
    private int _vatRate = 7;

    /// <summary>Text shown in the stepper e.g. "7 %"</summary>
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

    // ── Security ───────────────────────────────────────────

    [RelayCommand]
    private async Task ChangePasswordAsync()
    {
        var current = await Shell.Current.DisplayPromptAsync(
            "Change Password",
            "Enter your current password");

        if (current is null) return;

        var newPass = await Shell.Current.DisplayPromptAsync(
            "Change Password",
            "Enter new password (min 8 characters)");

        if (newPass is null || newPass.Length < 8)
        {
            await Shell.Current.DisplayAlert("Error", "Password must be at least 8 characters.", "OK");
            return;
        }

        // TODO: call auth service
        await Shell.Current.DisplayAlert("Success", "Password updated successfully.", "OK");
    }

    [RelayCommand]
    private async Task ChangePinAsync()
    {
        var pin = await Shell.Current.DisplayPromptAsync(
            "Set PIN Code",
            "Enter a 4-digit PIN",
            keyboard: Keyboard.Numeric);

        if (pin is null) return;

        if (pin.Length != 4 || !pin.All(char.IsDigit))
        {
            await Shell.Current.DisplayAlert("Error", "PIN must be exactly 4 digits.", "OK");
            return;
        }

        // TODO: persist PIN securely
        await Shell.Current.DisplayAlert("Success", "PIN updated successfully.", "OK");
    }
}