using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using POS_moblie_project.Services;
using POS_moblie_project.ViewModels;
using POS_moblie_project.ViewModels.Settings;
using POS_moblie_project.Views.Auth;
using POS_moblie_project.Views.Settings;

namespace POS_moblie_project
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            // Initialize SQLite native bindings before anything else uses them
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
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // ============================================
            // DI - Register Services
            // ============================================
            builder.Services.AddSingleton<DatabaseService>();

            // ============================================
            // DI - Register ViewModels
            // ============================================
            // Transient: form pages that must start clean each time
            builder.Services.AddTransient<PasswordViewModel>();

            // Singleton ViewModels (added in later phases as built):
            // builder.Services.AddSingleton<HomeViewModel>();
            // builder.Services.AddSingleton<POSViewModel>();
            // builder.Services.AddSingleton<StockViewModel>();
            // builder.Services.AddSingleton<TransactionHistoryViewModel>();
            // builder.Services.AddSingleton<SalesReportViewModel>();
            // builder.Services.AddSingleton<SettingsViewModel>();
            // builder.Services.AddSingleton<ManageCategoryViewModel>();

            // Transient ViewModels (form pages):
            // builder.Services.AddTransient<ProductDetailViewModel>();
            // builder.Services.AddTransient<CartViewModel>();
            // builder.Services.AddTransient<TransactionDetailViewModel>();
            // builder.Services.AddTransient<ProductSalesDetailViewModel>();
            // builder.Services.AddTransient<ManagePasswordViewModel>();

            // ============================================
            // DI - Register Pages
            // ============================================
            builder.Services.AddTransient<PasswordPage>();

            // Pages (added in later phases as built):
            // builder.Services.AddSingleton<HomePage>();
            // builder.Services.AddSingleton<POSPage>();
            // builder.Services.AddSingleton<StockPage>();
            // builder.Services.AddSingleton<TransactionHistoryPage>();
            // builder.Services.AddSingleton<SalesReportPage>();
            // builder.Services.AddSingleton<SettingsPage>();
            // builder.Services.AddSingleton<ManageCategoryPage>();
            // builder.Services.AddTransient<CartPage>();
            // builder.Services.AddTransient<ProductDetailPage>();
            // builder.Services.AddTransient<TransactionDetailPage>();
            // builder.Services.AddTransient<ProductSalesDetailPage>();
            // builder.Services.AddTransient<ManagePasswordPage>();

#if DEBUG
            builder.Logging.AddDebug();
            builder.Services.AddTransient<SettingsViewModel>();
            builder.Services.AddTransient<SettingsPage>();
#endif

            return builder.Build();
        }
    }
}