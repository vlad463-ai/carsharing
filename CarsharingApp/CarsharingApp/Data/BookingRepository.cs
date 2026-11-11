using System;
using System.Collections.Generic;
using Npgsql;
using CarsharingApp.Models;

namespace CarsharingApp.Data
{
    public class BookingRepository
    {
        private readonly DatabaseManager _db;

        public BookingRepository(DatabaseManager db)
        {
            _db = db;
        }

        // Проверка, занята ли машина в этот промежуток
        public bool IsCarBusy(int carId, DateTime start, DateTime end)
        {
            using var conn = _db.Connect();
            using var cmd = new NpgsqlCommand(
                @"SELECT COUNT(*) FROM bookings 
                  WHERE car_id = @carId 
                    AND status = 'активна'
                    AND (@start < end_time AND @end > start_time)",
                conn);

            cmd.Parameters.AddWithValue("@carId", carId);
            cmd.Parameters.AddWithValue("@start", start);
            cmd.Parameters.AddWithValue("@end", end);

            long count = Convert.ToInt64(cmd.ExecuteScalar());
            return count > 0;
        }

        // Проверка, есть ли у пользователя активная бронь
        public bool HasActiveBooking(int userId)
        {
            using var conn = _db.Connect();
            using var cmd = new NpgsqlCommand(
                "SELECT COUNT(*) FROM bookings WHERE user_id = @userId AND status = 'активна'",
                conn);
            cmd.Parameters.AddWithValue("@userId", userId);

            long count = Convert.ToInt64(cmd.ExecuteScalar());
            return count > 0;
        }

        // Создание брони
        public bool Create(int userId, int carId, DateTime start, DateTime end)
        {
            // 1. Проверки
            if (start >= end) return false; // неверное время
            if (IsCarBusy(carId, start, end)) return false; // машина занята
            if (HasActiveBooking(userId)) return false; // у юзера уже есть бронь

            using var conn = _db.Connect();
            using var transaction = conn.BeginTransaction(); // Транзакция!

            try
            {
                // 2. Создаем бронь
                using var cmdBooking = new NpgsqlCommand(
                    @"INSERT INTO bookings (user_id, car_id, start_time, end_time, status)
                      VALUES (@userId, @carId, @start, @end, 'активна')",
                    conn, transaction);
                cmdBooking.Parameters.AddWithValue("@userId", userId);
                cmdBooking.Parameters.AddWithValue("@carId", carId);
                cmdBooking.Parameters.AddWithValue("@start", start);
                cmdBooking.Parameters.AddWithValue("@end", end);
                cmdBooking.ExecuteNonQuery();

                // 3. Меняем статус машины на "Забронирован" (status_id = 2)
                using var cmdCar = new NpgsqlCommand(
                    "UPDATE cars SET status_id = 2 WHERE id = @carId",
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

        // Отмена брони
        public bool Cancel(int bookingId)
        {
            using var conn = _db.Connect();
            using var transaction = conn.BeginTransaction();

            try
            {
                // 1. Получаем car_id, чтобы вернуть статус
                using var cmdGet = new NpgsqlCommand("SELECT car_id FROM bookings WHERE id = @id", conn, transaction);
                cmdGet.Parameters.AddWithValue("@id", bookingId);
                var carIdObj = cmdGet.ExecuteScalar();

                if (carIdObj == null) return false;
                int carId = Convert.ToInt32(carIdObj);

                // 2. Удаляем (или ставим статус "отменена")
                using var cmdDel = new NpgsqlCommand("DELETE FROM bookings WHERE id = @id", conn, transaction);
                cmdDel.Parameters.AddWithValue("@id", bookingId);
                cmdDel.ExecuteNonQuery();

                // 3. Возвращаем машине статус "Доступен" (status_id = 1)
                using var cmdCar = new NpgsqlCommand("UPDATE cars SET status_id = 1 WHERE id = @carId", conn, transaction);
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
    }
}