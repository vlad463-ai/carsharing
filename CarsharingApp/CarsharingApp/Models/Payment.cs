using System;

namespace CarsharingApp.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int TripId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaidAt { get; set; } = DateTime.Now;
        public string Status { get; set; } = "успешно";
    }
}