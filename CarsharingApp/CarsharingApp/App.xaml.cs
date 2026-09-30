namespace CarsharingApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new NavigationPage(new Pages.LoginPage()); // авторизация
        //MainPage = new NavigationPage(new Pages.RegistrationPage()); // регистрация
    }
}