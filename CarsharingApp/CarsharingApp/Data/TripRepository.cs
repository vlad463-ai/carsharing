using CarsharingApp.Models;
using Npgsql;

namespace CarsharingApp.Data;

public class TripRepository(DatabaseManager db)
{
    public List<Trip> GetByUser(int userId)
    {
        using var conn = db.Connect();
        using var cmd = new NpgsqlCommand("SELECT id,user_id,car_id,started_at,finished_at,distance,total_cost FROM trips WHERE user_id=@user ORDER BY started_at DESC", conn);
        cmd.Parameters.AddWithValue("user", userId);
        using var reader = cmd.ExecuteReader();
        var trips = new List<Trip>();
        while (reader.Read())
            trips.Add(new Trip { Id = reader.GetInt32(0), UserId = reader.GetInt32(1), CarId = reader.GetInt32(2), StartedAt = reader.GetDateTime(3), FinishedAt = reader.IsDBNull(4) ? null : reader.GetDateTime(4), Distance = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5), TotalCost = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6) });
        return trips;
    }

    public bool StartTrip(int bookingId, int userId, int carId)
    {
        using var conn = db.Connect();
        using var tx = conn.BeginTransaction();
        using var booking = new NpgsqlCommand("SELECT id FROM bookings WHERE id=@id AND user_id=@user AND car_id=@car AND status='активна' AND start_time<=LOCALTIMESTAMP AND end_time>LOCALTIMESTAMP FOR UPDATE", conn, tx);
        booking.Parameters.AddWithValue("id", bookingId);
        booking.Parameters.AddWithValue("user", userId);
        booking.Parameters.AddWithValue("car", carId);
        if (booking.ExecuteScalar() == null) return false;
        using var existing = new NpgsqlCommand("SELECT COUNT(*) FROM trips WHERE booking_id=@id OR (user_id=@user AND finished_at IS NULL)", conn, tx);
        existing.Parameters.AddWithValue("id", bookingId);
        existing.Parameters.AddWithValue("user", userId);
        if (Convert.ToInt64(existing.ExecuteScalar()) > 0) return false;
        using var car = new NpgsqlCommand("UPDATE cars SET status_id=3 WHERE id=@car AND status_id=2", conn, tx);
        car.Parameters.AddWithValue("car", carId);
        if (car.ExecuteNonQuery() != 1) return false;
        using var insert = new NpgsqlCommand("INSERT INTO trips(booking_id,user_id,car_id,started_at) VALUES(@id,@user,@car,LOCALTIMESTAMP)", conn, tx);
        insert.Parameters.AddWithValue("id", bookingId);
        insert.Parameters.AddWithValue("user", userId);
        insert.Parameters.AddWithValue("car", carId);
        insert.ExecuteNonQuery();
        tx.Commit();
        return true;
    }

    public static decimal CalculateCost(DateTime startedAt, DateTime finishedAt, decimal pricePerMin)
    {
        if (finishedAt < startedAt) throw new ArgumentOutOfRangeException(nameof(finishedAt));
        if (pricePerMin <= 0) throw new ArgumentOutOfRangeException(nameof(pricePerMin));
        return decimal.Ceiling((decimal)(finishedAt - startedAt).TotalMinutes) * pricePerMin;
    }

    public bool FinishTrip(int tripId, int userId, out decimal totalCost)
    {
        totalCost = 0;
        using var conn = db.Connect();
        using var tx = conn.BeginTransaction();
        // Lock the trip before charging; a retry must not create a second payment.
        using var get = new NpgsqlCommand(@"SELECT t.started_at,t.car_id,t.booking_id,c.price_per_min,LOCALTIMESTAMP
            FROM trips t JOIN cars c ON c.id=t.car_id
            WHERE t.id=@id AND t.user_id=@user AND t.finished_at IS NULL FOR UPDATE OF t", conn, tx);
        get.Parameters.AddWithValue("id", tripId);
        get.Parameters.AddWithValue("user", userId);
        DateTime startedAt, finishedAt;
        int carId;
        int? bookingId;
        decimal rate;
        using (var reader = get.ExecuteReader())
        {
            if (!reader.Read()) return false;
            startedAt = reader.GetDateTime(0);
            carId = reader.GetInt32(1);
            bookingId = reader.IsDBNull(2) ? null : reader.GetInt32(2);
            rate = reader.GetDecimal(3);
            finishedAt = reader.GetDateTime(4);
        }
        var cost = CalculateCost(startedAt, finishedAt, rate);
        using var update = new NpgsqlCommand("UPDATE trips SET finished_at=@finished,total_cost=@cost WHERE id=@id; INSERT INTO payments(trip_id,amount,status) VALUES(@id,@cost,'успешно'); UPDATE cars SET status_id=1 WHERE id=@car", conn, tx);
        update.Parameters.AddWithValue("finished", finishedAt);
        update.Parameters.AddWithValue("cost", cost);
        update.Parameters.AddWithValue("id", tripId);
        update.Parameters.AddWithValue("car", carId);
        update.ExecuteNonQuery();
        if (bookingId.HasValue)
        {
            using var close = new NpgsqlCommand("UPDATE bookings SET status='завершена' WHERE id=@id", conn, tx);
            close.Parameters.AddWithValue("id", bookingId.Value);
            close.ExecuteNonQuery();
        }
        tx.Commit();
        totalCost = cost;
        return true;
    }

    public int GetActiveTripId(int userId) => Convert.ToInt32(db.SelectScalar(
        "SELECT id FROM trips WHERE user_id=@user AND finished_at IS NULL ORDER BY started_at DESC LIMIT 1",
        new NpgsqlParameter("user", userId)) ?? -1);
}
