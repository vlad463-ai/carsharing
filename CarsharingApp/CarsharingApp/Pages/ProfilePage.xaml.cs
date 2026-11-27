using CarsharingApp.Data;

namespace CarsharingApp.Pages;

public partial class ProfilePage : ContentPage
{
    private readonly UserRepository _userRepo;
    private readonly int _currentUserId;

    public ProfilePage(int userId)
    {
        InitializeComponent();
        _currentUserId = userId;
        var db = DatabaseSettings.CreateDatabase();
        _userRepo = new UserRepository(db);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadData();
        try
        {
            var db = DatabaseSettings.CreateDatabase();
            var documents = new DocumentRepository(db);
            DocumentStatusLabel.Text = "Документы: " + documents.GetStatus(_currentUserId);
            ReviewDocumentsButton.IsVisible = documents.IsStaff(_currentUserId);
            var count = new NotificationRepository(db).GetUnreadCount(_currentUserId);
            NotificationsButton.Text = count > 0 ? $"Уведомления · новых: {count}" : "Уведомления";
        }
        catch (Exception ex) { DocumentStatusLabel.Text = "Не удалось обновить статус: " + ex.Message; }
    }

    private async void OnDocumentsClicked(object sender, EventArgs e) =>
        await Navigation.PushAsync(new DocumentsPage(_currentUserId));
    private async void OnNotificationsClicked(object sender, EventArgs e) =>
        await Navigation.PushAsync(new NotificationsPage(_currentUserId));
    private async void OnReviewDocumentsClicked(object sender, EventArgs e) =>
        await Navigation.PushAsync(new DocumentReviewPage(_currentUserId));

    private void LoadData()
    {
        try
        {
            var user = _userRepo.GetUserById(_currentUserId);
            if (user != null)
            {
                LoginLabel.Text = $"Логин: {user.Login}";
                FullNameLabel.Text = user.FullName;
                RatingLabel.Text = $"Рейтинг: {user.Rating:F1} ★";
                PhoneEntry.Text = user.Phone;
                EmailEntry.Text = user.Email;

                var stats = _userRepo.GetUserStats(_currentUserId);
                StatsLabel.Text = $"Поездок: {stats.tripsCount}   ·   Потрачено: {stats.totalSpent:0.##} ₽";
            }
        }
        catch (Exception ex)
        {
            ResultLabel.Text = "Ошибка: " + ex.Message;
            ResultLabel.TextColor = Colors.Red;
        }
    }

    private void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            string newPhone = PhoneEntry.Text?.Trim() ?? "";
            string newEmail = EmailEntry.Text?.Trim() ?? "";

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
