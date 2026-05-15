using POS_moblie_project.ViewModels;

namespace POS_moblie_project.Views.Reports;

public partial class ProfitReportPage : ContentPage
{
    private readonly ProfitReportViewModel _viewModel;

    public ProfitReportPage(ProfitReportViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadReportCommand.ExecuteAsync(null);
    }
}
