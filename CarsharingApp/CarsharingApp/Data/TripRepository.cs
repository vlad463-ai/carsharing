using System;
using System.Collections.Generic;
using Npgsql;
using CarsharingApp.Models;

namespace CarsharingApp.Data
{
    public class TripRepository
    {
        private readonly DatabaseManager _db;

        public TripRepository(DatabaseManager db)
        {
            _db = db;
        }

        // Начало поездки
        public bool StartTrip(int bookingId, int userId, int carId)
        {
            using var conn = _db.Connect();
            using var transaction = conn.BeginTransaction();

            try
            {
                // 1. Создаём поездку
                using var cmdTrip = new NpgsqlCommand(
                    @"INSERT INTO trips (booking_id, user_id, car_id, started_at)
                      VALUES (@bookingId, @userId, @carId, NOW())",
                    conn, transaction);
                cmdTrip.Parameters.AddWithValue("@bookingId", bookingId);
                cmdTrip.Parameters.AddWithValue("@userId", userId);
                cmdTrip.Parameters.AddWithValue("@carId", carId);
                cmdTrip.ExecuteNonQuery();

                // 2. Меняем статус авто на "В поездке" (status_id = 3)
                using var cmdCar = new NpgsqlCommand(
                    "UPDATE cars SET status_id = 3 WHERE id = @carId",
                    conn, transaction);
                cmdCar.Parameters.AddWithValue("@carId", carId);
                cmdCar.ExecuteNonQuery();

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }

        // Завершение поездки и расчёт стоимости
        public bool FinishTrip(int tripId, decimal pricePerMin, out decimal totalCost)
        {
            totalCost = 0;
            using var conn = _db.Connect();
            using var transaction = conn.BeginTransaction();

            try
            {
                // 1. Получаем время начала поездки и car_id
                using var cmdGet = new NpgsqlCommand(
                    "SELECT started_at, car_id FROM trips WHERE id = @tripId",
                    conn, transaction);
                cmdGet.Parameters.AddWithValue("@tripId", tripId);

                DateTime startedAt;
                int carId;

                using (var reader = cmdGet.ExecuteReader())
                {
                    if (!reader.Read()) return false;
                    startedAt = reader.GetDateTime(0);
                    carId = reader.GetInt32(1);
                }

                // 2. Считаем стоимость (минуты * цена)
                var now = DateTime.Now;
                var minutes = (int)Math.Ceiling((now - startedAt).TotalMinutes);
                totalCost = minutes * pricePerMin;

                // 3. Обновляем поездку
                using var cmdUpdate = new NpgsqlCommand(
                    @"UPDATE trips 
                      SET finished_at = NOW(), total_cost = @cost 
                      WHERE id = @tripId",
                    conn, transaction);
                cmdUpdate.Parameters.AddWithValue("@cost", totalCost);
                cmdUpdate.Parameters.AddWithValue("@tripId", tripId);
                cmdUpdate.ExecuteNonQuery();

                // 4. Создаём платёж
                using var cmdPay = new NpgsqlCommand(
                    @"INSERT INTO payments (trip_id, amount, status)
                      VALUES (@tripId, @amount, 'успешно')",
                    conn, transaction);
                cmdPay.Parameters.AddWithValue("@tripId", tripId);
                cmdPay.Parameters.AddWithValue("@amount", totalCost);
                cmdPay.ExecuteNonQuery();

                // 5. Возвращаем авто в "Доступен" (status_id = 1)
                using var cmdCar = new NpgsqlCommand(
                    "UPDATE cars SET status_id = 1 WHERE id = @carId",
                    conn, transaction);
                cmdCar.Parameters.AddWithValue("@carId", carId);
                cmdCar.ExecuteNonQuery();

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }

        // Получение ID активной поездки
        public int GetActiveTripId(int userId)
        {
            using var conn = _db.Connect();
            using var cmd = new NpgsqlCommand(
                "SELECT id FROM trips WHERE user_id = @userId AND finished_at IS NULL ORDER BY started_at DESC LIMIT 1",
                conn);
            cmd.Parameters.AddWithValue("@userId", userId);

            var result = cmd.ExecuteScalar();
            return result == null ? -1 : Convert.ToInt32(result);
        }
    }
}