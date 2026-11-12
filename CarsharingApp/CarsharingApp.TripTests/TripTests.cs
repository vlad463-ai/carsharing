using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using CarsharingApp.Models;

namespace CarsharingApp.TripTests
{
    [TestClass]
    public class TripTests
    {
        // ============================================
        // ТЕСТ А. Проверка расчёта стоимости поездки
        // ============================================
        [TestMethod]
        public void CalculateCost_30Minutes_Returns300()
        {
            decimal pricePerMin = 10.00m;
            int minutes = 30;

            decimal expectedCost = minutes * pricePerMin;

            Assert.AreEqual(300.00m, expectedCost, "30 минут по 10 руб = 300 руб");
        }

        [TestMethod]
        public void CalculateCost_45Minutes_Returns450()
        {
            decimal pricePerMin = 10.00m;
            int minutes = 45;

            decimal expectedCost = minutes * pricePerMin;

            Assert.AreEqual(450.00m, expectedCost, "45 минут по 10 руб = 450 руб");
        }

        // ============================================
        // ТЕСТ Б. Проверка округления минут (в большую сторону)
        // ============================================
        [TestMethod]
        public void CalculateCost_PartialMinute_RoundsUp()
        {
            var startedAt = new DateTime(2026, 10, 10, 10, 0, 0);
            var finishedAt = new DateTime(2026, 10, 10, 10, 5, 1); // 5 минут 1 секунда

            var minutes = (int)Math.Ceiling((finishedAt - startedAt).TotalMinutes);

            Assert.AreEqual(6, minutes, "5 минут 1 секунда округляются до 6 минут");
        }

        // ============================================
        // ТЕСТ В. Модель Payment по умолчанию
        // ============================================
        [TestMethod]
        public void Payment_DefaultStatus_IsSuccess()
        {
            var payment = new Payment();
            Assert.AreEqual("успешно", payment.Status, "Статус платежа по умолчанию — успешно");
        }

        [TestMethod]
        public void Payment_AmountIsStored()
        {
            var payment = new Payment { Amount = 150.50m };
            Assert.AreEqual(150.50m, payment.Amount, "Сумма платежа сохраняется корректно");
        }

        // ============================================
        // ТЕСТ Г. Модель Trip
        // ============================================
        [TestMethod]
        public void Trip_FinishedAtCanBeNull()
        {
            var trip = new Trip();
            Assert.IsNull(trip.FinishedAt, "У новой поездки время окончания = null (поездка не завершена)");
        }

        [TestMethod]
        public void Trip_StartedAtIsSet()
        {
            var now = DateTime.Now;
            var trip = new Trip { StartedAt = now };
            Assert.AreEqual(now, trip.StartedAt, "Время начала поездки сохраняется");
        }
    }
}