using System;

namespace CarsharingApp.Models
{
    public class Trip
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CarId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public decimal Distance { get; set; }
        public decimal TotalCost { get; set; }

        public string DisplayDate => StartedAt.ToString("dd.MM.yyyy HH:mm");
        public string DisplayCost => $"{TotalCost} ₽";
        public string DisplayDistance => $"{Distance} км";
    }
}