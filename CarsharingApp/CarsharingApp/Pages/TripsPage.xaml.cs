using CarsharingApp.Data;
using CarsharingApp.Models;

namespace CarsharingApp.Pages;

public partial class TripsPage : ContentPage
{
    private readonly TripRepository _tripRepo;
    private readonly int _currentUserId = 6;

    public TripsPage()
    {
        InitializeComponent();

        var db = new DatabaseManager("Host=localhost;Port=5432;Database=Carsharing;Username=postgres;Password=123");
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