using CarsharingApp.Models;
using Npgsql;

namespace CarsharingApp.Data;

public class BookingRepository(DatabaseManager db)
{
    public bool IsCarBusy(int carId, DateTime start, DateTime end) => Convert.ToInt64(db.SelectScalar(
        "SELECT COUNT(*) FROM bookings WHERE car_id=@car AND status='активна' AND @start<end_time AND @end>start_time",
        new NpgsqlParameter("car", carId), new NpgsqlParameter("start", start), new NpgsqlParameter("end", end))) > 0;

    public bool HasActiveBooking(int userId) => GetActiveForUser(userId) != null;

    public Booking? GetActiveForUser(int userId)
    {
        using var conn = db.Connect();
        using var cmd = new NpgsqlCommand("SELECT id, car_id, start_time, end_time FROM bookings WHERE user_id=@user AND status='активна' ORDER BY id DESC LIMIT 1", conn);
        cmd.Parameters.AddWithValue("user", userId);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? new Booking { Id = reader.GetInt32(0), UserId = userId, CarId = reader.GetInt32(1), StartTime = reader.GetDateTime(2), EndTime = reader.GetDateTime(3), Status = "активна" } : null;
    }

    public bool Create(int userId, int carId, DateTime start, DateTime end)
    {
        if (userId <= 0 || carId <= 0 || start >= end) return false;
        using var conn = db.Connect();
        using var tx = conn.BeginTransaction();
        // Lock the user and car so concurrent requests cannot double-book either.
        using var user = new NpgsqlCommand("SELECT id FROM users WHERE id=@user FOR UPDATE", conn, tx);
        user.Parameters.AddWithValue("user", userId);
        if (user.ExecuteScalar() == null) return false;
        using var car = new NpgsqlCommand("SELECT status_id FROM cars WHERE id=@car FOR UPDATE", conn, tx);
        car.Parameters.AddWithValue("car", carId);
        if (car.ExecuteScalar() is not int status || status != 1) return false;
        using var existing = new NpgsqlCommand("SELECT COUNT(*) FROM bookings WHERE user_id=@user AND status='активна'", conn, tx);
        existing.Parameters.AddWithValue("user", userId);
        if (Convert.ToInt64(existing.ExecuteScalar()) > 0) return false;
        using var insert = new NpgsqlCommand("INSERT INTO bookings(user_id,car_id,start_time,end_time,status) VALUES(@user,@car,@start,@end,'активна')", conn, tx);
        insert.Parameters.AddWithValue("user", userId);
        insert.Parameters.AddWithValue("car", carId);
        insert.Parameters.AddWithValue("start", DateTime.SpecifyKind(start, DateTimeKind.Unspecified));
        insert.Parameters.AddWithValue("end", DateTime.SpecifyKind(end, DateTimeKind.Unspecified));
        insert.ExecuteNonQuery();
        using var update = new NpgsqlCommand("UPDATE cars SET status_id=2 WHERE id=@car", conn, tx);
        update.Parameters.AddWithValue("car", carId);
        update.ExecuteNonQuery();
        tx.Commit();
        return true;
    }

    public bool Cancel(int bookingId, int userId)
    {
        using var conn = db.Connect();
        using var tx = conn.BeginTransaction();
        using var booking = new NpgsqlCommand("SELECT car_id FROM bookings WHERE id=@id AND user_id=@user AND status='активна' FOR UPDATE", conn, tx);
        booking.Parameters.AddWithValue("id", bookingId);
        booking.Parameters.AddWithValue("user", userId);
        if (booking.ExecuteScalar() is not int carId) return false;
        using var active = new NpgsqlCommand("SELECT COUNT(*) FROM trips WHERE booking_id=@id", conn, tx);
        active.Parameters.AddWithValue("id", bookingId);
        if (Convert.ToInt64(active.ExecuteScalar()) > 0) return false;
        using var update = new NpgsqlCommand("UPDATE bookings SET status='отменена' WHERE id=@id; UPDATE cars SET status_id=1 WHERE id=@car AND status_id=2", conn, tx);
        update.Parameters.AddWithValue("id", bookingId);
        update.Parameters.AddWithValue("car", carId);
        update.ExecuteNonQuery();
        tx.Commit();
        return true;
    }
}
