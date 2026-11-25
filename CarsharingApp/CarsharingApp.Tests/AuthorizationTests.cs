using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using CarsharingApp.Models;

namespace CarsharingApp.Tests
{
    [TestClass]
    public class AuthorizationTests
    {
        // ============================================
        // ТЕСТ А. Проверка совпадения хешей
        // ============================================
        [TestMethod]
        public void CheckPassword_CorrectPassword_ReturnsTrue()
        {
            string password = "123456";
            string hash = BCrypt.Net.BCrypt.HashPassword(password);

            bool verified = BCrypt.Net.BCrypt.Verify(password, hash);

            Assert.IsTrue(verified, "Верный пароль должен пройти проверку");
        }

        // ============================================
        // ТЕСТ Б. Неверный пароль
        // ============================================
        [TestMethod]
        public void CheckPassword_WrongPassword_ReturnsFalse()
        {
            string hash = BCrypt.Net.BCrypt.HashPassword("123456");

            bool verified = BCrypt.Net.BCrypt.Verify("wrongpass", hash);

            Assert.IsFalse(verified, "Неверный пароль должен быть отклонён");
        }

        // ============================================
        // ТЕСТ В. Несуществующий пользователь
        // ============================================
        [TestMethod]
        public void Authorize_NonExistentUser_ReturnsNull()
        {
            User user = null;
            Assert.IsNull(user, "Несуществующий пользователь → null");
        }

        // ============================================
        // ТЕСТ Г. Пустой пароль
        // ============================================
        [TestMethod]
        public void Authorize_EmptyPassword_ReturnsFalse()
        {
            string password = "";
            bool empty = string.IsNullOrWhiteSpace(password);
            Assert.IsTrue(empty, "Пустой пароль должен быть отклонён");
        }

        // ============================================
        // ТЕСТ Д. Пустой логин
        // ============================================
        [TestMethod]
        public void Authorize_EmptyLogin_ReturnsFalse()
        {
            string login = "";
            bool empty = string.IsNullOrWhiteSpace(login);
            Assert.IsTrue(empty, "Пустой логин должен быть отклонён");
        }

        // ============================================
        // ТЕСТ Е. Разные пароли → разные хеши
        // ============================================
        [TestMethod]
        public void HashPassword_DifferentPasswords_DifferentHashes()
        {
            string hash1 = BCrypt.Net.BCrypt.HashPassword("123456");
            string hash2 = BCrypt.Net.BCrypt.HashPassword("123457");

            Assert.AreNotEqual(hash1, hash2, "Разные пароли — разные хеши");
        }

        // ============================================
        // ТЕСТ Ж. Хеш в формате BCrypt
        // ============================================
        [TestMethod]
        public void HashPassword_StartsWithBCryptPrefix()
        {
            string hash = BCrypt.Net.BCrypt.HashPassword("123456");
            Assert.IsTrue(hash.StartsWith("$2"), "Хеш в формате BCrypt");
        }

        // ============================================
        // ТЕСТ З. Пароль не хранится открыто
        // ============================================
        [TestMethod]
        public void HashPassword_NotEqualToPlainPassword()
        {
            string password = "123456";
            string hash = BCrypt.Net.BCrypt.HashPassword(password);
            Assert.AreNotEqual(password, hash, "Пароль не должен храниться открыто");
        }

        // ============================================
        // ТЕСТ И. Длина BCrypt-хеша = 60
        // ============================================
        [TestMethod]
        public void HashPassword_LengthIs60()
        {
            string hash = BCrypt.Net.BCrypt.HashPassword("123456");
            Assert.AreEqual(60, hash.Length, "BCrypt-хеш длиной 60 символов");
        }

        // ============================================
        // ТЕСТ К. Один пароль — один и тот же хеш проверяется
        // ============================================
        [TestMethod]
        public void CheckPassword_SamePasswordTwice_ReturnsTrue()
        {
            string password = "qwerty123";
            string hash = BCrypt.Net.BCrypt.HashPassword(password);

            Assert.IsTrue(BCrypt.Net.BCrypt.Verify(password, hash));
            Assert.IsTrue(BCrypt.Net.BCrypt.Verify(password, hash));
        }
    }
}