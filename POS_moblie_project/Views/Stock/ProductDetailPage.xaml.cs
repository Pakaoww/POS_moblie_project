using POS_moblie_project.ViewModels;

namespace POS_moblie_project.Views.Stock;

public partial class ProductDetailPage : ContentPage
{
    private readonly ProductDetailViewModel _viewModel;
    private bool _initialized = false;

    public ProductDetailPage(ProductDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Initialize เฉพาะครั้งแรกเท่านั้น
        // ถ้า initialize ทุกครั้ง จะ reset ProductCode ทับค่าที่ scanner ส่งมา
        if (!_initialized)
        {
            _initialized = true;
            await _viewModel.InitializeCommand.ExecuteAsync(null);
        }
    }
}