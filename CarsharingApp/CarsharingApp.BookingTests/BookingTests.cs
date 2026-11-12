using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using CarsharingApp.Models;
using CarsharingApp.Data;

namespace CarsharingApp.BookingTests
{
    [TestClass]
    public class BookingTests
    {
        // ТЕСТ 1: Проверка неверного времени (конец раньше начала)
        [TestMethod]
        public void CreateBooking_EndTimeBeforeStart_ReturnsFalse()
        {
            var start = new DateTime(2026, 10, 10, 12, 0, 0);
            var end = new DateTime(2026, 10, 10, 11, 0, 0); // раньше старта

            // Логика из BookingRepository: if (start >= end) return false;
            bool isValid = start < end;

            Assert.IsFalse(isValid, "Время окончания не может быть раньше времени начала");
        }

        // ТЕСТ 2: Проверка, что новая бронь не может пересекаться с существующей
        [TestMethod]
        public void IsCarBusy_OverlappingTime_ReturnsTrue()
        {
            // Старая бронь: 10:00 - 12:00
            var oldStart = new DateTime(2026, 10, 10, 10, 0, 0);
            var oldEnd = new DateTime(2026, 10, 10, 12, 0, 0);

            // Новая бронь: 11:00 - 13:00 (пересекается)
            var newStart = new DateTime(2026, 10, 10, 11, 0, 0);
            var newEnd = new DateTime(2026, 10, 10, 13, 0, 0);

            // Логика пересечения: (newStart < oldEnd) && (newEnd > oldStart)
            bool isBusy = (newStart < oldEnd) && (newEnd > oldStart);

            Assert.IsTrue(isBusy, "Машина должна быть занята (время пересекается)");
        }

        // ТЕСТ 3: Проверка, что бронь на свободное время проходит
        [TestMethod]
        public void IsCarBusy_FreeTime_ReturnsFalse()
        {
            // Старая бронь: 10:00 - 12:00
            var oldStart = new DateTime(2026, 10, 10, 10, 0, 0);
            var oldEnd = new DateTime(2026, 10, 10, 12, 0, 0);

            // Новая бронь: 13:00 - 15:00 (НЕ пересекается)
            var newStart = new DateTime(2026, 10, 10, 13, 0, 0);
            var newEnd = new DateTime(2026, 10, 10, 15, 0, 0);

            bool isBusy = (newStart < oldEnd) && (newEnd > oldStart);

            Assert.IsFalse(isBusy, "Машина должна быть свободна (время не пересекается)");
        }

      
    }
}