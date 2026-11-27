using CarsharingApp.Data;
using CarsharingApp.Models;

namespace CarsharingApp.Pages;

public partial class NotificationsPage : ContentPage
{
    private readonly int _userId;
    private readonly NotificationRepository _repo;
    public NotificationsPage(int userId)
    {
        InitializeComponent();
        _userId=userId;
        _repo=new NotificationRepository(DatabaseSettings.CreateDatabase());
    }
    protected override void OnAppearing() { base.OnAppearing(); LoadNotifications(); }
    private void LoadNotifications()
    {
        try
        {
            NotificationsList.ItemsSource=_repo.GetByUser(_userId);
            SummaryLabel.Text=$"Непрочитанных: {_repo.GetUnreadCount(_userId)}";
        }
        catch(Exception ex) { SummaryLabel.Text="Не удалось загрузить уведомления: "+ex.Message; }
    }
    private void OnRefreshClicked(object sender,EventArgs e)=>LoadNotifications();
    private void OnSelectionChanged(object sender,SelectionChangedEventArgs e)
    {
        if(e.CurrentSelection.FirstOrDefault() is not Notification item) return;
        NotificationsList.SelectedItem=null;
        try { if(!item.IsRead) { _repo.MarkAsRead(item.Id,_userId); LoadNotifications(); } }
        catch(Exception ex) { SummaryLabel.Text="Не удалось отметить уведомление: "+ex.Message; }
    }
}
