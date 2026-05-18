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
            Routing.RegisterRoute("AddStockPage", typeof(AddStockPage));
            Routing.RegisterRoute("CartPage", typeof(CartPage));
            Routing.RegisterRoute("HoldPage", typeof(HoldPage));
            Routing.RegisterRoute("TransactionDetailPage", typeof(TransactionDetailPage));
            Routing.RegisterRoute("ProductSalesDetailPage", typeof(ProductSalesDetailPage));
            Routing.RegisterRoute("ManageCategoryPage", typeof(ManageCategoryPage));
            Routing.RegisterRoute("managePasswordPage", typeof(ManagePasswordPage));
            Routing.RegisterRoute("AdminPanelPage", typeof(AdminPanelPage));
            Routing.RegisterRoute("AdminManagePasswordPage", typeof(AdminManagePasswordPage));
            Routing.RegisterRoute("AdminPasswordPage", typeof(AdminPasswordPage));

            // ตั้งค่า visibility ครั้งแรกตาม Preferences
            RefreshFlyoutVisibility();
        }

        /// <summary>
        /// เรียกเมื่อ Admin เปลี่ยนค่า visibility ใน AdminPanelPage
        /// </summary>
        public void RefreshFlyoutVisibility()
        {
            bool showSales = Preferences.Get("admin_show_sales_report", true);
            bool showProfit = Preferences.Get("admin_show_profit_report", true);
            bool showTransaction = Preferences.Get("admin_show_transaction_history", true);

            // ค้นหา FlyoutItem ตาม Route แล้วซ่อน/แสดง
            foreach (var item in Items)
            {
                if (item is FlyoutItem flyout)
                {
                    switch (flyout.Route)
                    {
                        case "reports":
                            flyout.IsVisible = showSales;
                            break;
                        case "profitreport":
                            flyout.IsVisible = showProfit;
                            break;
                        case "transactions":
                            flyout.IsVisible = showTransaction;
                            break;
                    }
                }
            }
        }
    }
}