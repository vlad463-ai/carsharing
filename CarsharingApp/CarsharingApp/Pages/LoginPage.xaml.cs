using CarsharingApp.Data;
using CarsharingApp.Models;

namespace CarsharingApp.Pages;

public partial class LoginPage : ContentPage
{
    private readonly UserRepository _userRepo;

    public LoginPage()
    {
        InitializeComponent();

        var db = new DatabaseManager("Host=localhost;Port=5432;Database=Carsharing;Username=postgres;Password=123");
        _userRepo = new UserRepository(db);
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        try
        {
            string login = LoginEntry.Text;
            string password = PasswordEntry.Text;

            User user = _userRepo.Authorize(login, password);

            if (user != null)
            {
                ResultLabel.Text = $"Добро пожаловать, {user.FullName}!";
            }
            else
            {
                ResultLabel.Text = "Неверный логин или пароль";
            }
        }
        catch (Exception ex)
        {
            ResultLabel.Text = "Ошибка БД: " + ex.Message;
        }
    }
}