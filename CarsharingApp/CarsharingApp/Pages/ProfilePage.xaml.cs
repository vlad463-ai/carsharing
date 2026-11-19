using CarsharingApp.Data;

namespace CarsharingApp.Pages;

public partial class ProfilePage : ContentPage
{
    private readonly UserRepository _userRepo;
    private readonly int _currentUserId = 6; // Твой ID из БД

    public ProfilePage()
    {
        InitializeComponent();
        var db = new DatabaseManager("Host=192.168.1.48;Port=5432;Database=Carsharing;Username=st53-5;Password=535");
        _userRepo = new UserRepository(db);
        LoadData();
    }

    private void LoadData()
    {
        try
        {
            var user = _userRepo.GetUserById(_currentUserId);
            if (user != null)
            {
                LoginLabel.Text = $"Логин: {user.Login}";
                FullNameLabel.Text = $"ФИО: {user.FullName}";
                RatingLabel.Text = $"Рейтинг: {user.Rating:F1} ?";
                PhoneEntry.Text = user.Phone;
                EmailEntry.Text = user.Email;

                var stats = _userRepo.GetUserStats(_currentUserId);
                StatsLabel.Text = $"Поездок: {stats.tripsCount} | Потрачено: {stats.totalSpent} ?";
            }
        }
        catch (Exception ex)
        {
            ResultLabel.Text = "Ошибка: " + ex.Message;
            ResultLabel.TextColor = Colors.Red;
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            string newPhone = PhoneEntry.Text?.Trim();
            string newEmail = EmailEntry.Text?.Trim();

            if (string.IsNullOrEmpty(newPhone) || string.IsNullOrEmpty(newEmail))
            {
                ResultLabel.Text = "Заполните поля";
                ResultLabel.TextColor = Colors.Red;
                return;
            }

            bool ok = _userRepo.UpdateContacts(_currentUserId, newPhone, newEmail);
            if (ok)
            {
                ResultLabel.Text = "Сохранено!";
                ResultLabel.TextColor = Colors.Green;
            }
            else
            {
                ResultLabel.Text = "Ошибка";
                ResultLabel.TextColor = Colors.Red;
            }
        }
        catch (Exception ex)
        {
            ResultLabel.Text = "Ошибка БД: " + ex.Message;
            ResultLabel.TextColor = Colors.Red;
        }
    }
}