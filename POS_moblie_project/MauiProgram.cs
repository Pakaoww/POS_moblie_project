using CommunityToolkit.Maui;
using Microcharts.Maui;
using Microsoft.Extensions.Logging;
using POS_moblie_project.Services;
using POS_moblie_project.ViewModels;
using POS_moblie_project.ViewModels.Settings;
using POS_moblie_project.Views.Auth;
using POS_moblie_project.Views.Home;
using POS_moblie_project.Views.POS;
using POS_moblie_project.Views.Reports;
using POS_moblie_project.Views.Settings;
using POS_moblie_project.Views.Splash;
using POS_moblie_project.Views.Stock;
using POS_moblie_project.Views.Transactions;
using ZXing.Net.Maui.Controls;

namespace POS_moblie_project
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            try
            {
                SQLitePCL.Batteries.Init();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SQLite initialization error: {ex.Message}");
            }

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseBarcodeReader()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Poppins-Bold.ttf", "PoppinsBold");
                    fonts.AddFont("Poppins-Medium.ttf", "PoppinsMedium");
                    fonts.AddFont("Poppins-Light.ttf", "PoppinsLight");
                });

            // ============================================
            // DI - Services
            // ============================================
            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddSingleton<CurrencyService>();
            builder.Services.AddSingleton<ProductAiService>();

            // Backup service (for export):
            builder.Services.AddSingleton<BackupService>();

            // ============================================
            // DI - ViewModels
            // ============================================
            builder.Services.AddTransient<PasswordViewModel>();
            builder.Services.AddTransient<SettingsViewModel>();
            builder.Services.AddTransient<ManagePasswordViewModel>();
            builder.Services.AddSingleton<ManageCategoryViewModel>();
            builder.Services.AddSingleton<StockViewModel>();
            builder.Services.AddTransient<ProductDetailViewModel>();
            builder.Services.AddTransient<AddStockViewModel>();
            builder.Services.AddTransient<AdminPanelViewModel>();
            builder.Services.AddTransient<AdminPanelPage>();

            // Later phases (uncomment as built):
            builder.Services.AddSingleton<HomeViewModel>();
            builder.Services.AddSingleton<POSViewModel>();
            builder.Services.AddSingleton<HoldViewModel>();
            builder.Services.AddTransient<CartViewModel>();
            builder.Services.AddSingleton<TransactionHistoryViewModel>();
            builder.Services.AddTransient<TransactionDetailViewModel>();
            builder.Services.AddSingleton<SalesReportViewModel>();
            builder.Services.AddTransient<ProductSalesDetailViewModel>();
            builder.Services.AddSingleton<ProfitReportViewModel>();

            // ============================================
            // DI - Pages
            // ============================================
            builder.Services.AddTransient<SplashPage>();
            builder.Services.AddTransient<PasswordPage>();
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<ManagePasswordPage>();
            builder.Services.AddSingleton<ManageCategoryPage>();
            builder.Services.AddSingleton<StockPage>();
            builder.Services.AddTransient<ProductDetailPage>();
            builder.Services.AddTransient<AddStockPage>();
            builder.Services.AddTransient<AdminManagePasswordPage>();
            builder.Services.AddTransient<AdminManagePasswordViewModel>();

            // Later phases (uncomment as built):
            builder.Services.AddSingleton<HomePage>();
            builder.Services.AddSingleton<POSPage>();
            builder.Services.AddTransient<CartPage>();
            builder.Services.AddSingleton<TransactionHistoryPage>();
            builder.Services.AddTransient<TransactionDetailPage>();
            builder.Services.AddSingleton<SalesReportPage>();
            builder.Services.AddTransient<ProductSalesDetailPage>();
            builder.Services.AddSingleton<ProfitReportPage>();

            builder.UseMicrocharts();


#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}