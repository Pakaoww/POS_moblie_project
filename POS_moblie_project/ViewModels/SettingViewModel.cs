using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS_moblie_project.ViewModels;
using POS_moblie_project.Views.Settings;

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
    //  SECURITY
    // ════════════════════════════════════════════════════════

    [RelayCommand]
    private async Task ChangePasswordAsync()
    => await Shell.Current.GoToAsync("managePasswordPage");

    [RelayCommand]
    private async Task ChangePinAsync()
        => await Shell.Current.GoToAsync("managePasswordPage");

    // ════════════════════════════════════════════════════════
    //  HELPER
    // ════════════════════════════════════════════════════════

    //private static async Task NavigateToManagePasswordAsync(ManagePasswordMode mode)
    //{
        //if (Shell.Current.Handler?.MauiContext?.Services
           // .GetService(typeof(ManagePasswordPage)) is ManagePasswordPage page)
       // {
        //    page.SetMode(mode);
       // }

        // ใช้ absolute path ตาม AppShell.xaml
       // await Shell.Current.GoToAsync("//settings/ManagePasswordPage");
    //}
}