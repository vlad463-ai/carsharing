using CarsharingApp.Data;
using CarsharingApp.Models;

namespace CarsharingApp.Pages;

public partial class CarsPage : ContentPage
{
    private readonly CarRepository _carRepo;

    public CarsPage()
    {
        InitializeComponent();

        var db = new DatabaseManager("Host=localhost;Port=5432;Database=Carsharing;Username=postgres;Password=123");
        _carRepo = new CarRepository(db);

        LoadCars();
    }

    private void LoadCars()
    {
        try
        {
            var cars = _carRepo.GetAvailable();
            CarsList.ItemsSource = cars;

            if (cars.Count == 0)
                ResultLabel.Text = "Нет доступных автомобилей";
            else
                ResultLabel.Text = $"Найдено: {cars.Count} авто";
        }
        catch (Exception ex)
        {
            ResultLabel.Text = "Ошибка БД: " + ex.Message;
        }
    }

    private void OnRefreshClicked(object sender, EventArgs e)
    {
        LoadCars();
    }
}