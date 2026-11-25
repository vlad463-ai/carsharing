using System;

namespace CarsharingApp.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string PasswordHash { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public decimal Rating { get; set; }


        public bool ValidateData()
        {
            if (string.IsNullOrWhiteSpace(Login)) return false;
            if (string.IsNullOrWhiteSpace(FullName)) return false;
            if (string.IsNullOrWhiteSpace(Phone)) return false;
            if (string.IsNullOrWhiteSpace(Email)) return false;
            if (!Email.Contains("@")) return false;

            foreach (char c in Phone)
            {
                if (!char.IsDigit(c) && c != '+' && c != '-')
                    return false;
            }
            return true;
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}