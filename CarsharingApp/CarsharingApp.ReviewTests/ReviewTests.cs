using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using CarsharingApp.Models;

namespace CarsharingApp.ReviewTests
{
    [TestClass]
    public class ReviewTests
    {
        [TestMethod]
        public void Review_DisplayStars_5Stars()
        {
            var review = new Review { Rating = 5 };
            Assert.AreEqual("★★★★★", review.DisplayStars, "5 звезд — 5 закрашенных");
        }

        [TestMethod]
        public void Review_DisplayStars_3Stars()
        {
            var review = new Review { Rating = 3 };
            Assert.AreEqual("★★★☆☆", review.DisplayStars, "3 звезды — 3 закрашенных, 2 пустых");
        }

        [TestMethod]
        public void Review_DisplayDate_Format()
        {
            var review = new Review { CreatedAt = new DateTime(2026, 10, 15) };
            Assert.AreEqual("15.10.2026", review.DisplayDate, "Формат даты дд.ММ.гггг");
        }

        [TestMethod]
        public void Review_DefaultComment_IsEmpty()
        {
            var review = new Review();
            Assert.AreEqual("", review.Comment, "По умолчанию комментарий пустой");
        }

        [TestMethod]
        public void Review_RatingStoredCorrectly()
        {
            var review = new Review { Rating = 4.5m };
            Assert.AreEqual(4.5m, review.Rating, "Рейтинг сохраняется корректно");
        }

        [TestMethod]
        public void Review_IdIsZeroByDefault()
        {
            var review = new Review();
            Assert.AreEqual(0, review.Id, "ID по умолчанию = 0");
        }
    }
}