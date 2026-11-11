using Microsoft.Maui.Hosting;

namespace CarsharingApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new Pages.TripPage());
            //MainPage = new NavigationPage(new Pages.BookingPage());
            //MainPage = new NavigationPage(new Pages.RegistrationPage());
        }
    }
}
