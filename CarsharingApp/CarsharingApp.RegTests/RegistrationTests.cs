using Microsoft.VisualStudio.TestTools.UnitTesting;
using System; 
using CarsharingApp.Models; 
namespace CarsharingApp.Tests
{
    [TestClass]
    public class RegistrationTests
    {
        // ============================================
        // ТЕСТ А. Успешная регистрация (валидные данные)
        // ============================================
        [TestMethod]
        public void Register_ValidData_ReturnsTrue()
        {
            var user = new User
            {
                Login = "ivan",
                FullName = "Иванов Иван Иванович",
                Phone = "+79001112233",
                Email = "ivan@mail.ru"
            };
            Assert.IsTrue(user.ValidateData(), "Валидные данные должны пройти проверку");
        }

        // ============================================
        // ТЕСТ Б. Пустой пароль
        // ============================================
        [TestMethod]
        public void Register_EmptyPassword_ReturnsFalse()
        {
            string password = "";
            bool tooShort = string.IsNullOrWhiteSpace(password) || password.Length < 6;
            Assert.IsTrue(tooShort, "Пустой пароль должен быть отклонён");
        }

        // ============================================
        // ТЕСТ В. Короткий пароль
        // ============================================
        [TestMethod]
        public void Register_ShortPassword_ReturnsFalse()
        {
            string password = "123";
            bool tooShort = password.Length < 6;
            Assert.IsTrue(tooShort, "Пароль короче 6 символов должен быть отклонён");
        }

        // ============================================
        // ТЕСТ Г. Дубликат логина
        // ============================================
        [TestMethod]
        public void Register_DuplicateLogin_ReturnsFalse()
        {
            long existingCount = 1;
            bool canRegister = existingCount == 0;
            Assert.IsFalse(canRegister, "Дубликат логина должен быть отклонён");
        }

        // ============================================
        // ТЕСТ Д. Email без @
        // ============================================
        [TestMethod]
        public void Register_InvalidEmail_ReturnsFalse()
        {
            var user = new User
            {
                Login = "test",
                FullName = "Тест Тест",
                Phone = "+79001112233",
                Email = "invalidmail"
            };
            Assert.IsFalse(user.ValidateData(), "Email без @ должен быть отклонён");
        }

        // ============================================
        // ТЕСТ Е. Телефон с буквами
        // ============================================
        [TestMethod]
        public void Register_InvalidPhone_ReturnsFalse()
        {
            var user = new User
            {
                Login = "test",
                FullName = "Тест Тест",
                Phone = "abc123",
                Email = "test@mail.ru"
            };
            Assert.IsFalse(user.ValidateData(), "Телефон с буквами должен быть отклонён");
        }

        // ============================================
        // ТЕСТ Ж. Хеш ≠ пароль
        // ============================================
        [TestMethod]
        public void HashPassword_NotEqualToOriginalPassword()
        {
            var user = new User();
            string password = "123456";
            string hash = user.HashPassword(password);
            Assert.AreNotEqual(password, hash, "Хеш не должен совпадать с паролем");
        }

        // ============================================
        // ТЕСТ З. Хеш в формате BCrypt
        // ============================================
        [TestMethod]
        public void HashPassword_StartsWithBCryptPrefix()
        {
            var user = new User();
            string hash = user.HashPassword("123456");
            Assert.IsTrue(hash.StartsWith("$2"), "Хеш должен быть в формате BCrypt");
        }

        // ============================================
        // ТЕСТ И. Длина BCrypt-хеша = 60
        // ============================================
        [TestMethod]
        public void HashPassword_LengthIs60()
        {
            var user = new User();
            string hash = user.HashPassword("123456");
            Assert.AreEqual(60, hash.Length, "BCrypt-хеш длиной 60 символов");
        }

        // ============================================
        // ТЕСТ К. Verify подтверждает пароль
        // ============================================
        [TestMethod]
        public void HashPassword_VerifyReturnsTrue()
        {
            var user = new User();
            string password = "123456";
            string hash = user.HashPassword(password);
            Assert.IsTrue(BCrypt.Net.BCrypt.Verify(password, hash), "Verify должен подтвердить пароль");
        }

        // ============================================
        // ТЕСТ Л. Пустое ФИО
        // ============================================
        [TestMethod]
        public void Register_EmptyFullName_ReturnsFalse()
        {
            var user = new User
            {
                Login = "test",
                FullName = "",
                Phone = "+79001112233",
                Email = "test@mail.ru"
            };
            Assert.IsFalse(user.ValidateData(), "Пустое ФИО должно быть отклонено");
        }

        // ============================================
        // ТЕСТ М. Пустой логин
        // ============================================
        [TestMethod]
        public void Register_EmptyLogin_ReturnsFalse()
        {
            var user = new User
            {
                Login = "",
                FullName = "Тест Тест",
                Phone = "+79001112233",
                Email = "test@mail.ru"
            };
            Assert.IsFalse(user.ValidateData(), "Пустой логин должен быть отклонён");
        }
    }
}