using System;
using Npgsql;
using CarsharingApp.Models;

namespace CarsharingApp.Data
{
    public class UserRepository
    {
        private readonly DatabaseManager _db;

        public UserRepository(DatabaseManager db)
        {
            _db = db;
        }

        public User Authorize(string login, string password)
        {
            using var conn = _db.Connect();
            using var cmd = new NpgsqlCommand(
                "SELECT id, login, password_hash, full_name, phone, email, rating FROM users WHERE login = @login",
                conn);
            cmd.Parameters.AddWithValue("@login", login);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;

            string hash = reader.GetString(2);
            if (!BCrypt.Net.BCrypt.Verify(password, hash)) return null;

            return new User
            {
                Id = reader.GetInt32(0),
                Login = reader.GetString(1),
                PasswordHash = hash,
                FullName = reader.GetString(3),
                Phone = reader.IsDBNull(4) ? null : reader.GetString(4),
                Email = reader.IsDBNull(5) ? null : reader.GetString(5),
                Rating = reader.GetDecimal(6)
            };
        }
    }
}