using CarsharingApp.Models;

namespace CarsharingApp.Pages;

public partial class ProfilePage : ContentPage
{
    public ProfilePage()
    {
        InitializeComponent();
        LoadFakeData(); // Временная заглушка
    }

    private void LoadFakeData()
    {
        // Фейковые данные для проверки интерфейса
        LoginLabel.Text = "Логин: ivan";
        FullNameLabel.Text = "ФИО: Иванов Иван Иванович";
        RatingLabel.Text = "Рейтинг: 4.5 ?";
        PhoneEntry.Text = "+79001112233";
        EmailEntry.Text = "ivan@mail.ru";
        StatsLabel.Text = "Поездок: 3 | Потрачено: 2310 ?";
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Заглушка", "Сохранение работает (заглушка)", "OK");
    }
}