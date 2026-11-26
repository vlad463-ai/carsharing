using System;

namespace CarsharingApp.Models
{
    public class Car
    {
        public int Id { get; set; }
        public string Brand { get; set; } = "";
        public string Model { get; set; } = "";
        public string PlateNumber { get; set; } = "";
        public decimal PricePerMin { get; set; }
        public int StatusId { get; set; }
        public decimal Rating { get; set; }

        public string DisplayName => $"{Brand} {Model}";
        public string DisplayPrice => $"{PricePerMin:0.##} ₽";
        public string DisplayPlateNumber => PlateNumberFormatter.Format(PlateNumber);
    }
}
