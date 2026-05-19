using POS_moblie_project.Services;
using POS_moblie_project.ViewModels;

namespace POS_moblie_project.Views.Reports;

public partial class SalesReportPage : ContentPage
{
    private readonly SalesReportViewModel _viewModel;

    public SalesReportPage(SalesReportViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (Preferences.Get("admin_lock_sales_report", false) &&
            !PageLockService.ConsumeAuthorization(PageLockService.RouteReports))
        {
            await Shell.Current.GoToAsync($"AdminPasswordPage?target={Uri.EscapeDataString(PageLockService.RouteReports)}");
            return;
        }

        await _viewModel.LoadReportCommand.ExecuteAsync(null);
    }
}