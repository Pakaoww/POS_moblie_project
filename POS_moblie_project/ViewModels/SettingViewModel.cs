using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS_moblie_project.Services;
using System.Collections.ObjectModel;

namespace POS_moblie_project.ViewModels.Settings;

public partial class SettingsViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;
    private readonly BackupService _backupService;
    private readonly CurrencyService _currencyService;

    public SettingsViewModel()
    {
        _databaseService = ServiceHelper.GetService<DatabaseService>();
        _backupService = ServiceHelper.GetService<BackupService>();
        _currencyService = ServiceHelper.GetService<CurrencyService>();
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

    // ════════════════════════════════════════════════════════
    //  CURRENCY
    // ════════════════════════════════════════════════════════

    [ObservableProperty]
    private bool _showCurrencySymbol = true;

    [ObservableProperty]
    private string _currencySymbol = "฿";

    [RelayCommand]
    public async Task LoadSettingsAsync()
    {
        VatEnabled = (await _databaseService.GetSettingAsync("vat_enabled")) == "true";
        var rateStr = await _databaseService.GetSettingAsync("vat_rate");
        VatRate = int.TryParse(rateStr, out var r) ? r : 7;

        ShowCurrencySymbol = (await _databaseService.GetSettingAsync("show_currency_symbol")) != "false";
        var sym = await _databaseService.GetSettingAsync("currency_symbol");
        CurrencySymbol = string.IsNullOrWhiteSpace(sym) ? "฿" : sym;
    }

    partial void OnVatEnabledChanged(bool value)
        => _ = _databaseService.SetSettingAsync("vat_enabled", value ? "true" : "false");

    partial void OnVatRateChanged(int value)
        => _ = _databaseService.SetSettingAsync("vat_rate", value.ToString());

    partial void OnShowCurrencySymbolChanged(bool value)
        => _ = _currencyService.SetShowSymbolAsync(value);

    partial void OnCurrencySymbolChanged(string value)
        => _ = _currencyService.SetSymbolAsync(value);

    // ════════════════════════════════════════════════════════
    //  VAT COMMANDS
    // ════════════════════════════════════════════════════════

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

    [RelayCommand]
    private void ToggleVat() => VatEnabled = !VatEnabled;

    [RelayCommand]
    private void ToggleShowCurrencySymbol() => ShowCurrencySymbol = !ShowCurrencySymbol;

    [RelayCommand]
    private async Task ResetDataAsync()
    {
        var confirm1 = await Shell.Current.DisplayAlert(
            "Reset All Data",
            "This will permanently delete ALL products, transactions, and stock lots.\n\nThis action cannot be undone.",
            "Yes, Reset", "Cancel");

        if (!confirm1) return;

        // ยืนยันอีกครั้ง
        var confirm2 = await Shell.Current.DisplayAlert(
            "Are you sure?",
            "All data will be deleted permanently.",
            "Delete Everything", "Cancel");

        if (!confirm2) return;

        try
        {
            IsBusy = true;
            await _databaseService.ResetAllDataAsync();
            await Shell.Current.DisplayAlert(
                "Done", "All data has been reset successfully.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ════════════════════════════════════════════════════════
    //  NAVIGATION
    // ════════════════════════════════════════════════════════

    [RelayCommand]
    private async Task ChangePasswordAsync()
        => await Shell.Current.GoToAsync("ManagePasswordPage");

    [RelayCommand]
    private async Task ManageCategoryAsync()
        => await Shell.Current.GoToAsync("ManageCategoryPage");

    [RelayCommand]
    private async Task OpenAdminPanelAsync()
    {
        // ครั้งแรกที่ยังไม่มี admin PIN → เข้าได้เลย
        bool hasPIN = await PageLockService.HasAdminPinAsync();

        if (!hasPIN)
        {
            await Shell.Current.GoToAsync("//settings/AdminPanelPage");
            return;
        }

        // มี PIN แล้ว → ต้องยืนยันก่อน
        await Shell.Current.GoToAsync("AdminPasswordPage");
    }


    // ════════════════════════════════════════════════════════
    //  BACKUP / IMPORT
    // ════════════════════════════════════════════════════════

    [ObservableProperty]
    private bool isBusy;

    [RelayCommand]
    private async Task BackupDatabaseAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var filePath = await _backupService.BackupDatabaseAsync();
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Backup POS Database",
                File = new ShareFile(filePath)
            });
        }
        catch (Exception ex)
        {
            await AppAlert.ShowErrorAsync("Backup Failed", ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ImportDatabaseAsync()
    {
        if (IsBusy) return;

        var confirm = await AppAlert.ConfirmAsync(
            "Import Database",
            "This will REPLACE all current data with the imported file. Continue?",
            "Yes, Import", "Cancel", isDanger: true);

        if (!confirm) return;

        IsBusy = true;
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select .db3 backup file",
                FileTypes = new FilePickerFileType(
                    new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.Android, new[] { "application/octet-stream" } },
                        { DevicePlatform.iOS,     new[] { "public.data"              } },
                    })
            });

            if (result == null) return;

            var success = await _backupService.ImportDatabaseAsync(result.FullPath);

            if (success)
            {
                await AppAlert.ShowSuccessAsync("Import Successful", "Database restored. The app will restart.");
                Application.Current.MainPage = new AppShell();
            }
            else
            {
                await AppAlert.ShowErrorAsync("Import Failed", "Could not restore database.");
            }
        }
        catch (Exception ex)
        {
            await AppAlert.ShowErrorAsync("Import Failed", ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }
}