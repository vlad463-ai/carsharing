using System;
using System.Collections.Generic;
using Npgsql;
using CarsharingApp.Models;

namespace CarsharingApp.Data
{
    public class ReviewRepository
    {
        private readonly DatabaseManager _db;

        public ReviewRepository(DatabaseManager db)
        {
            _db = db;
        }

        // Получить все отзывы по машине
        public List<Review> GetByCar(int carId)
        {
            var list = new List<Review>();

            using var conn = _db.Connect();
            using var cmd = new NpgsqlCommand(
                "SELECT id, user_id, car_id, rating, comment, created_at FROM reviews WHERE car_id = @carId ORDER BY created_at DESC",
                conn);
            cmd.Parameters.AddWithValue("@carId", carId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Review
                {
                    Id = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    CarId = reader.GetInt32(2),
                    Rating = reader.GetDecimal(3),
                    Comment = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    CreatedAt = reader.GetDateTime(5)
                });
            }

            return list;
        }

        // Добавить новый отзыв
        public bool Add(Review review)
        {
            if (review.UserId <= 0 || review.CarId <= 0 || review.Rating < 1 || review.Rating > 5) return false;
            using var conn = _db.Connect();
            using var transaction = conn.BeginTransaction();
            using var car = new NpgsqlCommand("SELECT id FROM cars WHERE id=@car FOR UPDATE", conn, transaction);
            car.Parameters.AddWithValue("@car", review.CarId);
            if (car.ExecuteScalar() == null) return false;
            using var cmd = new NpgsqlCommand(
                "INSERT INTO reviews (user_id, car_id, rating, comment) VALUES (@u, @c, @r, @com)",
                conn, transaction);
            cmd.Parameters.AddWithValue("@u", review.UserId);
            cmd.Parameters.AddWithValue("@c", review.CarId);
            cmd.Parameters.AddWithValue("@r", review.Rating);
            cmd.Parameters.AddWithValue("@com", review.Comment);

            cmd.ExecuteNonQuery();
            using var rating = new NpgsqlCommand("UPDATE cars SET rating=(SELECT AVG(rating) FROM reviews WHERE car_id=@car) WHERE id=@car", conn, transaction);
            rating.Parameters.AddWithValue("@car", review.CarId);
            rating.ExecuteNonQuery();
            transaction.Commit();
            return true;
        }

        // Пересчитать средний рейтинг машины
        public void RecalcCarRating(int carId)
        {
            using var conn = _db.Connect();
            using var cmd = new NpgsqlCommand(
                @"UPDATE cars 
                  SET rating = (SELECT COALESCE(AVG(rating), 0) FROM reviews WHERE car_id = @carId) 
                  WHERE id = @carId",
                conn);
            cmd.Parameters.AddWithValue("@carId", carId);
            cmd.ExecuteNonQuery();
        }
    }
}
