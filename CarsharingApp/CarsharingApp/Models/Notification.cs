namespace CarsharingApp.Models;

public class Notification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Type { get; set; } = "";
    public string Message { get; set; } = "";
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public string DisplayDate => CreatedAt.ToString("dd.MM.yyyy HH:mm");
    public string ReadStatus => IsRead ? "Прочитано" : "Новое";
}
