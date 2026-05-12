using POS_moblie_project.ViewModels;

namespace POS_moblie_project.Views.POS;

public partial class HoldPage : ContentPage
{
    public HoldPage()
    {
        InitializeComponent();

        BindingContext = ServiceHelper.GetService<HoldViewModel>();
    }
}