using CarsharingApp.Data;

namespace CarsharingApp.Pages;

public partial class TripPage : ContentPage
{
    private readonly TripRepository _tripRepo;
    private readonly int _currentUserId = 6;      // ID юзера (из БД)
    private readonly int _currentCarId = 1;       // ID машины (из БД)
    private readonly int _currentBookingId = 12;  // ID активной брони (замени на свой!)
    private readonly decimal _pricePerMin = 10.00m; // Цена за минуту

    private int _activeTripId = -1;

    public TripPage()
    {
        InitializeComponent();

        var db = new DatabaseManager("Host=localhost;Port=5432;Database=Carsharing;Username=postgres;Password=123");
        _tripRepo = new TripRepository(db);
    }

    private async void OnStartTripClicked(object sender, EventArgs e)
    {
        try
        {
            bool success = _tripRepo.StartTrip(_currentBookingId, _currentUserId, _currentCarId);

            if (success)
            {
                _activeTripId = _tripRepo.GetActiveTripId(_currentUserId);
                StatusLabel.Text = "Поездка началась!";
                ResultLabel.Text = "Время пошло...";
                StartTripBtn.IsEnabled = false;
                FinishTripBtn.IsEnabled = true;
            }
            else
            {
                ResultLabel.TextColor = Colors.Red;
                ResultLabel.Text = "Ошибка: не удалось начать поездку.";
            }
        }
        catch (Exception ex)
        {
            ResultLabel.TextColor = Colors.Red;
            ResultLabel.Text = "Ошибка БД: " + ex.Message;
        }
    }

    private async void OnFinishTripClicked(object sender, EventArgs e)
    {
        try
        {
            if (_activeTripId == -1)
            {
                ResultLabel.TextColor = Colors.Red;
                ResultLabel.Text = "Нет активной поездки.";
                return;
            }

            bool success = _tripRepo.FinishTrip(_activeTripId, _pricePerMin, out decimal totalCost);

            if (success)
            {
                StatusLabel.Text = "Поездка завершена.";
                ResultLabel.TextColor = Colors.Green;
                ResultLabel.Text = $"К оплате: {totalCost} ?";
                FinishTripBtn.IsEnabled = false;
            }
            else
            {
                ResultLabel.TextColor = Colors.Red;
                ResultLabel.Text = "Ошибка завершения.";
            }
        }
        catch (Exception ex)
        {
            ResultLabel.TextColor = Colors.Red;
            ResultLabel.Text = "Ошибка БД: " + ex.Message;
        }
    }
}