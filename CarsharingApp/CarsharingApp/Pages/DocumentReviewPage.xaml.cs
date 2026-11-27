using CarsharingApp.Data;
using CarsharingApp.Models;

namespace CarsharingApp.Pages;

public partial class DocumentReviewPage : ContentPage
{
    private readonly int _staffId;
    private readonly DocumentRepository _repo;
    private readonly DocumentFileStore _files=new();
    public DocumentReviewPage(int userId)
    {
        InitializeComponent(); _staffId=userId; _repo=new DocumentRepository(DatabaseSettings.CreateDatabase());
    }
    protected override void OnAppearing() { base.OnAppearing(); LoadDocuments(); }
    private void LoadDocuments()
    {
        try { DocumentsList.ItemsSource=_repo.GetPending(_staffId); }
        catch(Exception ex) { DocumentsList.ItemsSource=null; ResultLabel.Text=ex.Message; }
    }
    private void OnRefreshClicked(object sender,EventArgs e)=>LoadDocuments();
    private void OnApproveClicked(object sender,EventArgs e)=>Decide(sender,Document.Approved);
    private void OnRejectClicked(object sender,EventArgs e)=>Decide(sender,Document.Rejected);
    private void Decide(object sender,string status)
    {
        if(sender is not Button { BindingContext:Document document })return;
        try
        {
            ResultLabel.Text=_repo.ChangeStatus(_staffId,document.Id,document.Revision,status)
                ? "Решение сохранено. Пользователю отправлено уведомление."
                : "Комплект уже изменён. Обновите список и проверьте новые фото.";
            LoadDocuments();
        }
        catch(Exception ex) { ResultLabel.Text=ex.Message; }
    }
    private async void OnPassportClicked(object sender,EventArgs e)=>await ShowPhoto(sender,true);
    private async void OnLicenseClicked(object sender,EventArgs e)=>await ShowPhoto(sender,false);
    private async Task ShowPhoto(object sender,bool passport)
    {
        if(sender is not Button { BindingContext:Document document })return;
        try
        {
            if(!_repo.IsStaff(_staffId))throw new UnauthorizedAccessException("Доступ только для сотрудника.");
            var path=passport?document.PassportPhoto:document.LicensePhoto;
            if(!_files.CanOpen(document.UserId,path))throw new IOException("Файл недоступен. Проверьте доступ к общей папке документов.");
            await Navigation.PushAsync(new ContentPage {
                Title=passport?"Паспорт":"Водительские права",
                Content=new Image { Source=ImageSource.FromFile(path),Aspect=Aspect.AspectFit,Margin=new Thickness(24) }
            });
        }
        catch(Exception ex) { ResultLabel.Text=ex.Message; }
    }
}
