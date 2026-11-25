using CarsharingApp.Data;

namespace CarsharingApp.Pages;

public partial class BookingPage : ContentPage
{
    private readonly BookingRepository _bookingRepo;
    private readonly int _currentUserId;
    private readonly int _currentCarId;

    public BookingPage(int userId, int carId)
    {
        InitializeComponent();
        _currentUserId = userId;
        _currentCarId = carId;

        var db = DatabaseSettings.CreateDatabase();
        _bookingRepo = new BookingRepository(db);
        var start = DateTime.Now;
        StartDatePicker.Date = start.Date;
        StartTimePicker.Time = start.TimeOfDay;
        EndDatePicker.Date = start.AddHours(1).Date;
        EndTimePicker.Time = start.AddHours(1).TimeOfDay;
    }

    private async void OnBookClicked(object sender, EventArgs e)
    {
        try
        {
            DateTime start = StartDatePicker.Date + StartTimePicker.Time;
            DateTime end = EndDatePicker.Date + EndTimePicker.Time;

            bool success = _bookingRepo.Create(_currentUserId, _currentCarId, start, end);

            if (success)
            {
                ResultLabel.TextColor = Colors.Green;
                ResultLabel.Text = "Бронь успешно создана!";
                BookButton.IsEnabled = false;
                await Navigation.PushAsync(new TripPage(_currentUserId));
            }
            else
            {
                ResultLabel.TextColor = Colors.Red;
                ResultLabel.Text = "Ошибка: машина занята или неверное время.";
            }
        }
        catch (Exception ex)
        {
            ResultLabel.TextColor = Colors.Red;
            ResultLabel.Text = "Ошибка БД: " + ex.Message;
        }
    }
}
