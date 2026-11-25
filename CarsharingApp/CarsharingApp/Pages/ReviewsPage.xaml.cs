using CarsharingApp.Data;
using CarsharingApp.Models;

namespace CarsharingApp.Pages;

public partial class ReviewsPage : ContentPage
{
    private readonly ReviewRepository _reviewRepo;
    private readonly int _currentCarId = 1; // ID машины (замени на нужный)
    private readonly int _currentUserId = 6; // ID юзера

    public ReviewsPage()
    {
        InitializeComponent();

        var db = new DatabaseManager("Host=192.168.1.48;Port=5432;Database=Carsharing;Username=st53-5;Password=535");
        _reviewRepo = new ReviewRepository(db);

        LoadReviews();
    }

    private void LoadReviews()
    {
        try
        {
            ReviewsList.ItemsSource = _reviewRepo.GetByCar(_currentCarId);
        }
        catch (Exception ex)
        {
            DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }

    private async void OnAddReviewClicked(object sender, EventArgs e)
    {
        try
        {
            if (RatingPicker.SelectedIndex == -1)
            {
                await DisplayAlert("Ошибка", "Выберите оценку", "OK");
                return;
            }

            // Picker возвращает индекс 0..4, а оценка нужна 5..1
            int rating = 5 - RatingPicker.SelectedIndex;

            var review = new Review
            {
                UserId = _currentUserId,
                CarId = _currentCarId,
                Rating = rating,
                Comment = CommentEditor.Text ?? ""
            };

            bool success = _reviewRepo.Add(review);

            if (success)
            {
                _reviewRepo.RecalcCarRating(_currentCarId);
                CommentEditor.Text = "";
                RatingPicker.SelectedIndex = -1;
                LoadReviews();
                await DisplayAlert("Успех", "Отзыв добавлен!", "OK");
            }
            else
            {
                await DisplayAlert("Ошибка", "Не удалось добавить отзыв", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка БД", ex.Message, "OK");
        }
    }
}