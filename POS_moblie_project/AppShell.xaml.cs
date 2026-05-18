using POS_moblie_project.Views.POS;
using POS_moblie_project.Views.Reports;
using POS_moblie_project.Views.Settings;
using POS_moblie_project.Views.Stock;
using POS_moblie_project.Views.Transactions;

namespace POS_moblie_project
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("ProductDetailPage", typeof(ProductDetailPage));
            Routing.RegisterRoute("AddStockPage", typeof(AddStockPage));        // ← ใหม่
            Routing.RegisterRoute("CartPage", typeof(CartPage));
            Routing.RegisterRoute("HoldPage", typeof(HoldPage));
            Routing.RegisterRoute("TransactionDetailPage", typeof(TransactionDetailPage));
            Routing.RegisterRoute("ProductSalesDetailPage", typeof(ProductSalesDetailPage));
            Routing.RegisterRoute("ManageCategoryPage", typeof(ManageCategoryPage));
            Routing.RegisterRoute("managePasswordPage", typeof(ManagePasswordPage));
            Routing.RegisterRoute("AdminPanelPage", typeof(AdminPanelPage));
            Routing.RegisterRoute(
                    nameof(AdminManagePasswordPage),
                    typeof(AdminManagePasswordPage));
        }
    }
}