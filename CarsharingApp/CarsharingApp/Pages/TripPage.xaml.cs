using CarsharingApp.Data;

namespace CarsharingApp.Pages;

public partial class TripPage : ContentPage
{
    private readonly TripRepository _tripRepo;
    private readonly BookingRepository _bookingRepo;
    private readonly int _currentUserId;
    private int _activeTripId = -1;

    public TripPage(int userId)
    {
        InitializeComponent();
        _currentUserId = userId;
        var db = DatabaseSettings.CreateDatabase();
        _tripRepo = new TripRepository(db);
        _bookingRepo = new BookingRepository(db);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        try { RefreshState(); }
        catch (Exception ex) { ResultLabel.Text = "Ошибка БД: " + ex.Message; }
    }

    private void RefreshState()
    {
        _activeTripId = _tripRepo.GetActiveTripId(_currentUserId);
        var booking = _bookingRepo.GetActiveForUser(_currentUserId);
        StartTripBtn.IsEnabled = _activeTripId < 0 && booking != null;
        FinishTripBtn.IsEnabled = _activeTripId > 0;
        CancelBookingBtn.IsEnabled = _activeTripId < 0 && booking != null;
        StatusLabel.Text = _activeTripId > 0 ? "Поездка идёт" : booking == null ? "Нет активной брони" : $"Бронь: {booking.DisplayPeriod}";
    }

    private void OnStartTripClicked(object sender, EventArgs e)
    {
        try
        {
            var booking = _bookingRepo.GetActiveForUser(_currentUserId);
            ResultLabel.Text = booking != null && _tripRepo.StartTrip(booking.Id, _currentUserId, booking.CarId)
                ? "Поездка началась!" : "Не удалось начать поездку. Проверьте время и состояние брони.";
            RefreshState();
        }
        catch (Exception ex) { ResultLabel.Text = "Ошибка БД: " + ex.Message; }
    }

    private void OnFinishTripClicked(object sender, EventArgs e)
    {
        try
        {
            ResultLabel.Text = _activeTripId > 0 && _tripRepo.FinishTrip(_activeTripId, _currentUserId, out var cost)
                ? $"Поездка завершена. К оплате: {cost:F2} ₽" : "Не удалось завершить поездку.";
            RefreshState();
        }
        catch (Exception ex) { ResultLabel.Text = "Ошибка БД: " + ex.Message; }
    }

    private void OnCancelBookingClicked(object sender, EventArgs e)
    {
        try
        {
            var booking = _bookingRepo.GetActiveForUser(_currentUserId);
            ResultLabel.Text = booking != null && _bookingRepo.Cancel(booking.Id, _currentUserId)
                ? "Бронь отменена." : "Не удалось отменить бронь.";
            RefreshState();
        }
        catch (Exception ex) { ResultLabel.Text = "Ошибка БД: " + ex.Message; }
    }
}
