using CarsharingApp.Data;
using CarsharingApp.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace CarsharingApp.Pages;

public partial class RegistrationPage : ContentPage
{
    private readonly UserRepository _userRepo;


    public RegistrationPage()
    {
        InitializeComponent();

        var db = new DatabaseManager("Host=localhost;Port=5432;Database=Carsharing;Username=postgres;Password=123");
        _userRepo = new UserRepository(db);
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        try
        {
            var user = new User
            {
                Login = LoginEntry.Text,
                FullName = FullNameEntry.Text,
                Phone = PhoneEntry.Text,
                Email = EmailEntry.Text
            };

            bool ok = _userRepo.Register(user, PasswordEntry.Text);

            if (ok)
                ResultLabel.Text = "Регистрация успешна!";
            else
                ResultLabel.Text = "Ошибка регистрации. Проверьте данные.";
        }
        catch (Exception ex)
        {
            ResultLabel.Text = "Ошибка БД: " + ex.Message;
        }
    }
}