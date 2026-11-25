using CarsharingApp.Data;
using CarsharingApp.Models;

namespace CarsharingApp.Pages;

public partial class TripsPage : ContentPage
{
    private readonly TripRepository _tripRepo;
    private readonly int _currentUserId;

    public TripsPage(int userId)
    {
        InitializeComponent();
        _currentUserId = userId;
        var db = DatabaseSettings.CreateDatabase();
        _tripRepo = new TripRepository(db);

        LoadTrips();
    }

    private void LoadTrips()
    {
        try
        {
            var trips = _tripRepo.GetByUser(_currentUserId);
            TripsList.ItemsSource = trips;

            if (trips.Count == 0)
                ResultLabel.Text = "У вас пока нет поездок";
            else
                ResultLabel.Text = $"Найдено: {trips.Count} поездок";
        }
        catch (Exception ex)
        {
            ResultLabel.Text = "Ошибка: " + ex.Message;
        }
    }

    private void OnRefreshClicked(object sender, EventArgs e)
    {
        LoadTrips();
    }
}
