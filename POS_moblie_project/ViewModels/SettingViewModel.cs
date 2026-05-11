using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace POS_moblie_project.ViewModels.Settings;

public partial class SettingsViewModel : ObservableObject
{
    private const string VatEnabledKey = "vat_enabled";
    private const string VatRateKey = "vat_rate";

    // ── Constructor: โหลดค่าที่บันทึกไว้ ──────────────────
    public SettingsViewModel()
    {
        _vatEnabled = Preferences.Get(VatEnabledKey, true);
        _vatRate = Preferences.Get(VatRateKey, 7);
    }

    // ════════════════════════════════════════════════════════
    //  VAT
    // ════════════════════════════════════════════════════════

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VatRateDisplay))]
    private bool _vatEnabled;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VatRateDisplay))]
    private int _vatRate;

    public string VatRateDisplay => $"{VatRate} %";

    // บันทึกทันทีเมื่อ VatEnabled เปลี่ยน
    partial void OnVatEnabledChanged(bool value)
        => Preferences.Set(VatEnabledKey, value);

    // บันทึกทันทีเมื่อ VatRate เปลี่ยน
    partial void OnVatRateChanged(int value)
        => Preferences.Set(VatRateKey, value);

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
    //  SECURITY
    // ════════════════════════════════════════════════════════

    [RelayCommand]
    private async Task ChangePasswordAsync()
        => await Shell.Current.GoToAsync("managePasswordPage");

    [RelayCommand]
    private async Task ChangePinAsync()
        => await Shell.Current.GoToAsync("ManageCategoryPage");
}