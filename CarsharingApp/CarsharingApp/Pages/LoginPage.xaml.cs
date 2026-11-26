using CarsharingApp.Data;
using CarsharingApp.Models;


namespace CarsharingApp.Pages;

public partial class LoginPage : ContentPage
{
    private readonly UserRepository _userRepo;

    public LoginPage()
    {
        InitializeComponent();

        var db = DatabaseSettings.CreateDatabase();
        _userRepo = new UserRepository(db);
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        try
        {
            string login = LoginEntry.Text;
            string password = PasswordEntry.Text;

            User? user = _userRepo.Authorize(login, password);

            if (user != null)
            {
                Navigation.InsertPageBefore(new CarsPage(user.Id), this);
                await Navigation.PopAsync();
            }
            else
                ResultLabel.Text = "Неверный логин или пароль";
        }
        catch (Exception ex)
        {
            ResultLabel.Text = "Ошибка БД: " + ex.Message;
        }
    }

    private async void OnRegisterClicked(object sender, EventArgs e) =>
        await Navigation.PushAsync(new RegistrationPage());
}
