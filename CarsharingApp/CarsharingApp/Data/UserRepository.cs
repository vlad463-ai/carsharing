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

        // ============================================
        // РЕГИСТРАЦИЯ (из твоей первой задачи)
        // ============================================
        public bool Register(User user, string password)
        {
            if (!user.ValidateData()) return false;
            if (string.IsNullOrWhiteSpace(password) || password.Length < 6) return false;

            var existing = _db.SelectScalar(
                "SELECT COUNT(*) FROM users WHERE login = @login",
                new NpgsqlParameter("@login", user.Login));
            if (Convert.ToInt64(existing) > 0) return false;

            user.PasswordHash = user.HashPassword(password);

            int rows = _db.Insert(
                @"INSERT INTO users (login, password_hash, full_name, phone, email, rating)
                  VALUES (@login, @hash, @fio, @phone, @email, 0) ON CONFLICT DO NOTHING",
                new NpgsqlParameter("@login", user.Login),
                new NpgsqlParameter("@hash", user.PasswordHash),
                new NpgsqlParameter("@fio", user.FullName),
                new NpgsqlParameter("@phone", user.Phone),
                new NpgsqlParameter("@email", user.Email));

            return rows > 0;
        }

        // ============================================
        // АВТОРИЗАЦИЯ (из задачи друга)
        // ============================================
        public User? Authorize(string? login, string? password)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrEmpty(password)) return null;
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
                Phone = reader.IsDBNull(4) ? "" : reader.GetString(4),
                Email = reader.IsDBNull(5) ? "" : reader.GetString(5),
                Rating = reader.GetDecimal(6)
            };
        }

        // ============================================
        // ЛИЧНЫЙ КАБИНЕТ (новые методы)
        // ============================================
        public User? GetUserById(int id)
        {
            using var conn = _db.Connect();
            using var cmd = new NpgsqlCommand(
                "SELECT id, login, full_name, phone, email, rating FROM users WHERE id = @id",
                conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new User
                {
                    Id = reader.GetInt32(0),
                    Login = reader.GetString(1),
                    FullName = reader.GetString(2),
                    Phone = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    Email = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    Rating = reader.GetDecimal(5)
                };
            }
            return null;
        }

        public bool UpdateContacts(int userId, string phone, string email)
        {
            using var conn = _db.Connect();
            using var cmd = new NpgsqlCommand(
                "UPDATE users SET phone = @phone, email = @email WHERE id = @id",
                conn);
            cmd.Parameters.AddWithValue("@phone", phone);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@id", userId);

            return cmd.ExecuteNonQuery() > 0;
        }

        public (int tripsCount, decimal totalSpent) GetUserStats(int userId)
        {
            using var conn = _db.Connect();
            using var cmd = new NpgsqlCommand(
                "SELECT COUNT(*), COALESCE(SUM(total_cost), 0) FROM trips WHERE user_id = @userId",
                conn);
            cmd.Parameters.AddWithValue("@userId", userId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return ((int)reader.GetInt64(0), reader.GetDecimal(1));
            }
            return (0, 0);
        }
    }
}
