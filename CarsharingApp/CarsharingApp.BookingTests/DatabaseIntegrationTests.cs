using CarsharingApp.Data;
using CarsharingApp.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;

namespace CarsharingApp.BookingTests;

[TestClass]
[TestCategory("Database")]
public class DatabaseIntegrationTests
{
    private DatabaseManager _db = null!;
    private int _userId, _otherUserId, _carId, _otherCarId;
    private string _login = "";
    private string _phone = "";

    [TestInitialize]
    public void SetUp()
    {
        var connection = Environment.GetEnvironmentVariable("CARSHARING_TEST_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(connection))
            Assert.Inconclusive("Set CARSHARING_TEST_CONNECTION_STRING to a disposable database restored from the repository dump.");
        _db = new DatabaseManager(connection!);
        _db.Insert("INSERT INTO car_statuses(id,name) VALUES(1,'Доступен'),(2,'Забронирован'),(3,'В поездке') ON CONFLICT(id) DO NOTHING");
        _login = "test_" + Guid.NewGuid().ToString("N");
        _phone = "+" + DateTime.UtcNow.Ticks.ToString();
        var users = new UserRepository(_db);
        Assert.IsTrue(users.Register(NewUser(_login, _phone), "secret123"));
        _userId = users.Authorize(_login, "secret123")!.Id;
        Assert.IsTrue(users.Register(NewUser(_login + "b", _phone + "1"), "secret123"));
        _otherUserId = users.Authorize(_login + "b", "secret123")!.Id;
        _carId = AddCar();
        _otherCarId = AddCar();
    }

    private User NewUser(string login, string phone) => new() { Login = login, FullName = "Тестовый пользователь", Phone = phone, Email = login + "@example.test" };

    private int AddCar() => Convert.ToInt32(_db.SelectScalar(
        "INSERT INTO cars(brand,model,plate_number,price_per_min,status_id) VALUES('Test','Car',@plate,12.50,1) RETURNING id",
        new NpgsqlParameter("plate", Guid.NewGuid().ToString("N")[..12])));

    [TestCleanup]
    public void CleanUp()
    {
        if (_db == null) return;
        _db.Insert("DELETE FROM users WHERE id=@user OR id=@other", new NpgsqlParameter("user", _userId), new NpgsqlParameter("other", _otherUserId));
        _db.Insert("DELETE FROM cars WHERE id=@car OR id=@other", new NpgsqlParameter("car", _carId), new NpgsqlParameter("other", _otherCarId));
    }

    private Booking Book()
    {
        var repo = new BookingRepository(_db);
        Assert.IsTrue(repo.Create(_userId, _carId, DateTime.Now.AddMinutes(-1), DateTime.Now.AddHours(1)));
        return repo.GetActiveForUser(_userId)!;
    }

    [TestMethod]
    public void RegistrationAuthorizationAndProfile_UseSameUser()
    {
        var users = new UserRepository(_db);
        Assert.IsNull(users.Authorize(_login, "incorrect"));
        Assert.IsNull(users.Authorize("", ""));
        Assert.IsNull(users.Authorize("missing_" + _login, "secret123"));
        Assert.IsFalse(users.Register(NewUser(_login, _phone), "secret123"));
        Assert.IsTrue(users.UpdateContacts(_userId, _phone, _login + "@new.test"));
        Assert.AreEqual(_login + "@new.test", users.GetUserById(_userId)!.Email);
        Assert.AreEqual(0, users.GetUserStats(_userId).tripsCount);
    }

    [TestMethod]
    public void BookingTripPaymentHistory_CompleteOnceAndAllowNextBooking()
    {
        var booking = Book();
        var trips = new TripRepository(_db);
        Assert.IsFalse(trips.StartTrip(booking.Id, _otherUserId, _carId));
        Assert.IsTrue(trips.StartTrip(booking.Id, _userId, _carId));
        Assert.IsFalse(trips.StartTrip(booking.Id, _userId, _carId));
        Assert.IsFalse(new BookingRepository(_db).Cancel(booking.Id, _userId));
        var tripId = trips.GetActiveTripId(_userId);
        _db.Insert("UPDATE trips SET started_at=LOCALTIMESTAMP-INTERVAL '5 minutes 1 second' WHERE id=@id", new NpgsqlParameter("id", tripId));
        Assert.IsFalse(trips.FinishTrip(tripId, _otherUserId, out _));
        Assert.IsTrue(trips.FinishTrip(tripId, _userId, out var cost));
        Assert.AreEqual(75m, cost);
        Assert.IsFalse(trips.FinishTrip(tripId, _userId, out _));
        Assert.AreEqual(1L, Convert.ToInt64(_db.SelectScalar("SELECT COUNT(*) FROM payments WHERE trip_id=@id", new NpgsqlParameter("id", tripId))));
        Assert.AreEqual(cost, trips.GetByUser(_userId).Single().TotalCost);
        Assert.AreEqual(0, trips.GetByUser(_otherUserId).Count);
        Assert.AreEqual((1, cost), new UserRepository(_db).GetUserStats(_userId));
        Assert.IsTrue(new CarRepository(_db).GetAvailable().Any(c => c.Id == _carId));
        Assert.IsFalse(new BookingRepository(_db).HasActiveBooking(_userId));
        Book();
    }

    [TestMethod]
    public void Booking_RejectsInvalidAndOccupiedReservations()
    {
        var repo = new BookingRepository(_db);
        Assert.IsFalse(repo.Create(_userId, _carId, DateTime.Now, DateTime.Now.AddHours(-1)));
        var booking = Book();
        Assert.IsTrue(repo.IsCarBusy(_carId, DateTime.Now, DateTime.Now.AddHours(1)));
        Assert.IsFalse(repo.Create(_otherUserId, _carId, DateTime.Now, DateTime.Now.AddHours(1)));
        Assert.IsFalse(repo.Create(_userId, _otherCarId, DateTime.Now, DateTime.Now.AddHours(1)));
        Assert.IsFalse(repo.Cancel(booking.Id, _otherUserId));
        Assert.IsTrue(repo.Cancel(booking.Id, _userId));
        Assert.IsFalse(repo.Cancel(booking.Id, _userId));
        Assert.IsTrue(repo.Create(_otherUserId, _carId, DateTime.Now, DateTime.Now.AddHours(1)));
    }

    [TestMethod]
    public async Task ConcurrentBookings_OnlyOneUserCanReserveCar()
    {
        var start = DateTime.Now;
        var results = await Task.WhenAll(
            Task.Run(() => new BookingRepository(_db).Create(_userId, _carId, start, start.AddHours(1))),
            Task.Run(() => new BookingRepository(_db).Create(_otherUserId, _carId, start, start.AddHours(1))));
        Assert.AreEqual(1, results.Count(success => success));
    }

    [TestMethod]
    public async Task ConcurrentBookings_UserCannotReserveTwoCars()
    {
        var start = DateTime.Now;
        var results = await Task.WhenAll(
            Task.Run(() => new BookingRepository(_db).Create(_userId, _carId, start, start.AddHours(1))),
            Task.Run(() => new BookingRepository(_db).Create(_userId, _otherCarId, start, start.AddHours(1))));
        Assert.AreEqual(1, results.Count(success => success));
    }

    [TestMethod]
    public void Reviews_UpdateSelectedCarRating()
    {
        var reviews = new ReviewRepository(_db);
        Assert.IsFalse(reviews.Add(new Review { UserId = _userId, CarId = _carId, Rating = 6 }));
        Assert.IsTrue(reviews.Add(new Review { UserId = _userId, CarId = _carId, Rating = 5, Comment = "Отлично" }));
        Assert.IsTrue(reviews.Add(new Review { UserId = _otherUserId, CarId = _carId, Rating = 3 }));
        Assert.AreEqual(2, reviews.GetByCar(_carId).Count);
        Assert.AreEqual(4m, new CarRepository(_db).GetAvailable().Single(c => c.Id == _carId).Rating);
        Assert.AreEqual(0, reviews.GetByCar(_otherCarId).Count);
    }
}
