using System;
using System.Collections.Generic;
using System.Linq;
using CarsharingApp.Data;
using CarsharingApp.Models;

namespace CarsharingApp.Pages;

public partial class CarsPage : ContentPage
{
    private readonly CarRepository _carRepo;
    private readonly int _currentUserId;
    private List<Car> _allCars = new();

    public CarsPage(int userId)
    {
        _currentUserId = userId;
        InitializeComponent();
        var db = DatabaseSettings.CreateDatabase();
        _carRepo = new CarRepository(db);

    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadCars();
    }

    private async void OnBookClicked(object sender, EventArgs e)
    {
        if (sender is Button { BindingContext: Car car })
            await Navigation.PushAsync(new BookingPage(_currentUserId, car.Id));
    }

    private async void OnReviewsClicked(object sender, EventArgs e)
    {
        if (sender is Button { BindingContext: Car car })
            await Navigation.PushAsync(new ReviewsPage(_currentUserId, car.Id));
    }

    private async void OnProfileClicked(object sender, EventArgs e) =>
        await Navigation.PushAsync(new ProfilePage(_currentUserId));

    private async void OnHistoryClicked(object sender, EventArgs e) =>
        await Navigation.PushAsync(new TripsPage(_currentUserId));

    private async void OnTripClicked(object sender, EventArgs e) =>
        await Navigation.PushAsync(new TripPage(_currentUserId));

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        Navigation.InsertPageBefore(new LoginPage(), this);
        await Navigation.PopAsync();
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
        if (CarsList == null || ResultLabel == null) return;
        ApplyFilter();
    }

    private void OnPageSizeChanged(object sender, EventArgs e)
    {
        if (CarsLayout == null || FiltersGrid == null) return;
        CarsLayout.Span = Width >= 980 ? 2 : 1;
        var narrow = Width < 760;
        FiltersGrid.ColumnDefinitions[1].Width = narrow ? new GridLength(1, GridUnitType.Star) : new GridLength(220);
        Grid.SetColumnSpan(SearchField, narrow ? 3 : 1);
        Grid.SetRow(PriceField, narrow ? 1 : 0);
        Grid.SetColumn(PriceField, narrow ? 0 : 1);
        Grid.SetColumnSpan(PriceField, narrow ? 2 : 1);
        Grid.SetRow(RefreshButton, narrow ? 1 : 0);
    }

    private void OnRefreshClicked(object sender, EventArgs e)
    {
        SearchEntry.Text = "";
        MaxPriceEntry.Text = "";
        LoadCars();
    }
}
