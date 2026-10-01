using static System.Net.Mime.MediaTypeNames;

namespace CarsharingApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();
    }
}