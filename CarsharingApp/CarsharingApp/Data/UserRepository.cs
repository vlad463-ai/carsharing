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
            if (!reader.Read()) return null; // логина нет в БД

            string hash = reader.GetString(2);

            // Проверка пароля по хешу
            if (!BCrypt.Net.BCrypt.Verify(password, hash))
                return null;

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
                  VALUES (@login, @hash, @fio, @phone, @email, 0)",
                new NpgsqlParameter("@login", user.Login),
                new NpgsqlParameter("@hash", user.PasswordHash),
                new NpgsqlParameter("@fio", user.FullName),
                new NpgsqlParameter("@phone", user.Phone),
                new NpgsqlParameter("@email", user.Email));

            return rows > 0;
        }
    }
}