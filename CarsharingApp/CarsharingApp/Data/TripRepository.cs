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

        public List<Trip> GetByUser(int userId)
        {
            var result = new List<Trip>();

            using var conn = _db.Connect();
            using var cmd = new NpgsqlCommand(
                @"SELECT id, user_id, car_id, started_at, finished_at, distance, total_cost
                  FROM trips
                  WHERE user_id = @userId
                  ORDER BY started_at DESC",
                conn);
            cmd.Parameters.AddWithValue("@userId", userId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new Trip
                {
                    Id = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    CarId = reader.GetInt32(2),
                    StartedAt = reader.GetDateTime(3),
                    FinishedAt = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                    Distance = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5),
                    TotalCost = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6)
                });
            }

            return result;
        }
    }
}