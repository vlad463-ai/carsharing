using CarsharingApp.Data;
using CarsharingApp.Models;

namespace CarsharingApp.Pages;

public partial class ReviewsPage : ContentPage
{
    private readonly ReviewRepository _reviewRepo;
    private readonly int _currentCarId;
    private readonly int _currentUserId;

    public ReviewsPage(int userId, int carId)
    {
        InitializeComponent();
        _currentUserId = userId; _currentCarId = carId;

        var db = DatabaseSettings.CreateDatabase();
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
