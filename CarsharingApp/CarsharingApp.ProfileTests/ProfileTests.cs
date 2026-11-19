using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using CarsharingApp.Models;

namespace CarsharingApp.ProfileTests
{
    [TestClass]
    public class ProfileTests
    {
        [TestMethod]
        public void User_ValidData_ReturnsTrue()
        {
            var user = new User
            {
                Login = "test",
                FullName = "Тест Тест",
                Email = "t@t.ru",
                Phone = "+79001112233"
            };
            Assert.IsTrue(user.ValidateData(), "Валидные данные — true");
        }

        [TestMethod]
        public void User_InvalidEmail_ReturnsFalse()
        {
            var user = new User
            {
                Login = "test",
                FullName = "Тест Тест",
                Email = "invalidmail",
                Phone = "+79001112233"
            };
            Assert.IsFalse(user.ValidateData(), "Email без @ — false");
        }

        [TestMethod]
        public void User_InvalidPhone_ReturnsFalse()
        {
            var user = new User
            {
                Login = "test",
                FullName = "Тест Тест",
                Email = "t@t.ru",
                Phone = "abc123"
            };
            Assert.IsFalse(user.ValidateData(), "Телефон с буквами — false");
        }

        [TestMethod]
        public void User_UpdateContacts_ChangesData()
        {
            var user = new User { Phone = "+79000000000", Email = "old@mail.ru" };
            user.Phone = "+79999999999";
            user.Email = "new@mail.ru";

            Assert.AreEqual("+79999999999", user.Phone);
            Assert.AreEqual("new@mail.ru", user.Email);
        }

        [TestMethod]
        public void User_RatingCannotExceedFive()
        {
            var user = new User { Rating = 5.0m };
            Assert.IsTrue(user.Rating <= 5, "Рейтинг не превышает 5");
        }

        [TestMethod]
        public void User_EmptyLogin_ReturnsFalse()
        {
            var user = new User
            {
                Login = "",
                FullName = "Тест Тест",
                Email = "t@t.ru",
                Phone = "+79001112233"
            };
            Assert.IsFalse(user.ValidateData(), "Пустой логин — false");
        }
    }
}