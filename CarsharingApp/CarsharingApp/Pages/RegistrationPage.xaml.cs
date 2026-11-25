using CarsharingApp.Data;
using CarsharingApp.Models;


namespace CarsharingApp.Pages;

public partial class RegistrationPage : ContentPage
{
    private readonly UserRepository _userRepo;


    public RegistrationPage()
    {
        InitializeComponent();
        var db = DatabaseSettings.CreateDatabase();
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
                { await DisplayAlert("Регистрация", "Регистрация успешна! Теперь войдите в аккаунт.", "OK"); await Navigation.PopAsync(); }
            else
                ResultLabel.Text = "Ошибка регистрации. Проверьте данные.";
        }
        catch (Exception ex)
        {
            ResultLabel.Text = "Ошибка БД: " + ex.Message;
        }
    }
}
