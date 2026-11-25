using System;

namespace CarsharingApp.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CarId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; }

        public string DisplayPeriod => $"{StartTime:dd.MM.yyyy HH:mm} - {EndTime:HH:mm}";
    }
}