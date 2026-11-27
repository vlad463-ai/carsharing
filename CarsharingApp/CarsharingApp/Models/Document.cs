namespace CarsharingApp.Models;

public class Document
{
    public const string Pending = "на проверке";
    public const string Approved = "одобрено";
    public const string Rejected = "отклонено";
    public int Id { get; set; }
    public int UserId { get; set; }
    public string PassportPhoto { get; set; } = "";
    public string LicensePhoto { get; set; } = "";
    public string Status { get; set; } = Pending;
    public int Revision { get; set; }
    public string UserName { get; set; } = "";
    public string DisplayStatus => $"Статус: {Status}";
}
