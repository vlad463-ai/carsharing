using System;
using System.Collections.Generic;
using Npgsql;
using CarsharingApp.Models;

namespace CarsharingApp.Data
{
    public class CarRepository
    {
        private readonly DatabaseManager _db;

        public CarRepository(DatabaseManager db)
        {
            _db = db;
        }

        public List<Car> GetAvailable()
        {
            var result = new List<Car>();

            using var conn = _db.Connect();
            using var cmd = new NpgsqlCommand(
                @"SELECT id, brand, model, plate_number, price_per_min, status_id, rating
          FROM cars
          WHERE status_id = 1
          ORDER BY brand",
                conn);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new Car
                {
                    Id = reader.GetInt32(0),
                    Brand = reader.GetString(1),
                    Model = reader.GetString(2),
                    PlateNumber = reader.GetString(3),
                    PricePerMin = reader.GetDecimal(4),
                    StatusId = reader.GetInt32(5),
                    Rating = reader.GetDecimal(6)
                });
            }

            return result;
        }
    }
}