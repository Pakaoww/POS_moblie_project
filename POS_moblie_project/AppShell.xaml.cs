namespace POS_moblie_project
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            // Register routes ที่ไม่ได้อยู่ใน FlyoutItem
            //Routing.RegisterRoute(nameof(PasswordPage), typeof(PasswordPage));
        }
    }
}
