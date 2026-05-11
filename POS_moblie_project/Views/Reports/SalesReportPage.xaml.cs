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
        await _viewModel.LoadReportCommand.ExecuteAsync(null);
    }
}