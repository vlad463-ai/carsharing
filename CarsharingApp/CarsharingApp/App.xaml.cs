namespace CarsharingApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            UserAppTheme = AppTheme.Light;

            MainPage = new NavigationPage(new Pages.LoginPage());
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = base.CreateWindow(activationState);
            window.Title = "Каршеринг";
            window.Width = 1180;
            window.Height = 820;
            window.MinimumWidth = 480;
            window.MinimumHeight = 600;
            return window;
        }
    }
}
