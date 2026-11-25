namespace CarsharingApp.Data;

public static class DatabaseSettings
{
    public static DatabaseManager CreateDatabase() => new(
        Environment.GetEnvironmentVariable("CARSHARING_CONNECTION_STRING")
        ?? "Host=localhost;Port=5432;Database=Carsharing;Username=postgres;Timeout=5");
}
