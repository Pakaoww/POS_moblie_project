using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS_moblie_project.Services;

namespace POS_moblie_project.ViewModels.Settings;

public partial class SettingsViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;

    public SettingsViewModel()
    {
        _databaseService = ServiceHelper.GetService<DatabaseService>();
    }

    // ════════════════════════════════════════════════════════
    //  VAT
    // ════════════════════════════════════════════════════════

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VatRateDisplay))]
    private bool _vatEnabled;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VatRateDisplay))]
    private int _vatRate = 7;

    public string VatRateDisplay => $"{VatRate} %";

    [RelayCommand]
    public async Task LoadSettingsAsync()
    {
        VatEnabled = (await _databaseService.GetSettingAsync("vat_enabled")) == "true";
        var rateStr = await _databaseService.GetSettingAsync("vat_rate");
        VatRate = int.TryParse(rateStr, out var r) ? r : 7;
    }

    partial void OnVatEnabledChanged(bool value)
    {
        _ = _databaseService.SetSettingAsync("vat_enabled", value ? "true" : "false");
    }

    partial void OnVatRateChanged(int value)
    {
        _ = _databaseService.SetSettingAsync("vat_rate", value.ToString());
    }

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
    //  SECURITY & NAVIGATION
    // ════════════════════════════════════════════════════════

    [RelayCommand]
    private async Task ChangePasswordAsync()
        => await Shell.Current.GoToAsync("managePasswordPage");

    [RelayCommand]
    private async Task ManageCategoryAsync()
        => await Shell.Current.GoToAsync("ManageCategoryPage");
}