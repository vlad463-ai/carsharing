using System;
using System.Collections.Generic;
using System.Linq;
using CarsharingApp.Data;
using CarsharingApp.Models;

namespace CarsharingApp.Pages;

public partial class CarsPage : ContentPage
{
    private readonly CarRepository _carRepo;
    private List<Car> _allCars = new();

    public CarsPage()
    {
        InitializeComponent();

        //var db = new DatabaseManager("Host=localhost;Port=5432;Database=carsharing;Username=postgres;Password=123");
        var db = new DatabaseManager("Host=192.168.1.48;Port=5432;Database=Carsharing;Username=st53-5;Password=535");
        _carRepo = new CarRepository(db);

        LoadCars();
    }

    private void LoadCars()
    {
        try
        {
            _allCars = _carRepo.GetAvailable();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            ResultLabel.Text = "Ошибка: " + ex.Message;
        }
    }

    private void ApplyFilter()
    {
        var filtered = _allCars.AsEnumerable();

        // Фильтр по марке или модели
        string search = SearchEntry?.Text?.Trim() ?? "";
        if (!string.IsNullOrEmpty(search))
        {
            filtered = filtered.Where(c =>
                c.Brand.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                c.Model.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        // Фильтр по максимальной цене
        if (decimal.TryParse(MaxPriceEntry?.Text, out decimal maxPrice) && maxPrice > 0)
        {
            filtered = filtered.Where(c => c.PricePerMin <= maxPrice);
        }

        var result = filtered.ToList();
        CarsList.ItemsSource = result;

        if (result.Count == 0)
            ResultLabel.Text = "Ничего не найдено";
        else
            ResultLabel.Text = $"Найдено: {result.Count} авто";
    }

    private void OnFilterChanged(object sender, TextChangedEventArgs e)
    {
        ApplyFilter();
    }

    private void OnRefreshClicked(object sender, EventArgs e)
    {
        SearchEntry.Text = "";
        MaxPriceEntry.Text = "";
        LoadCars();
    }
}