using System;

namespace CarsharingApp.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CarId { get; set; }
        public decimal Rating { get; set; }
        public string Comment { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Для отображения в списке
        public string DisplayStars => new string('★', (int)Rating) + new string('☆', 5 - (int)Rating);
        public string DisplayDate => CreatedAt.ToString("dd.MM.yyyy");
    }
}