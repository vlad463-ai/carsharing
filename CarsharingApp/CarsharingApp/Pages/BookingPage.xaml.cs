using CarsharingApp.Data;

namespace CarsharingApp.Pages;

public partial class BookingPage : ContentPage
{
    private readonly BookingRepository _bookingRepo;
    private readonly int _currentUserId = 6; // ID юзера из БД
    private readonly int _currentCarId = 1;   // ID машины из БД

    public BookingPage()
    {
        InitializeComponent();

        var db = new DatabaseManager("Host=localhost;Port=5432;Database=Carsharing;Username=postgres;Password=123");
        _bookingRepo = new BookingRepository(db);
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