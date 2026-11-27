using CarsharingApp.Data;
using CarsharingApp.Models;

namespace CarsharingApp.Pages;

public partial class DocumentsPage : ContentPage
{
    private readonly int _userId;
    private readonly DocumentRepository _repo;
    private readonly DocumentFileStore _files=new();
    private FileResult? _passport, _license;
    private Document? _current;
    public DocumentsPage(int userId)
    {
        InitializeComponent(); _userId=userId;
        _repo=new DocumentRepository(DatabaseSettings.CreateDatabase());
    }
    protected override void OnAppearing() { base.OnAppearing(); Refresh(); }
    private void Refresh()
    {
        try
        {
            _current=_repo.GetByUser(_userId);
            StatusLabel.Text="Статус: "+(_current?.Status ?? "не загружены");
            if(_passport==null) SetPreview(PassportPreview,PassportLabel,_current?.PassportPhoto);
            if(_license==null) SetPreview(LicensePreview,LicenseLabel,_current?.LicensePhoto);
        }
        catch(Exception ex) { ResultLabel.Text="Не удалось загрузить документы: "+ex.Message; }
    }
    private void SetPreview(Image image,Label label,string? path)
    {
        var available=path!=null && _files.CanOpen(_userId,path);
        image.IsVisible=available;
        image.Source=available?ImageSource.FromFile(path):null;
        label.Text=string.IsNullOrEmpty(path)?"Фото не выбрано":available?"Фото загружено":"Файл недоступен. Выберите фото повторно.";
    }
    private async Task<FileResult?> PickPhoto()
    {
        try { return await FilePicker.Default.PickAsync(new PickOptions { PickerTitle="Выберите фото документа",FileTypes=FilePickerFileType.Images }); }
        catch(Exception ex) { ResultLabel.Text="Не удалось выбрать файл: "+ex.Message; return null; }
    }
    private async void OnPickPassport(object sender,EventArgs e)
    {
        var picked=await PickPhoto(); if(picked==null)return;
        _passport=picked; PassportLabel.Text=picked.FileName; PassportPreview.IsVisible=false;
    }
    private async void OnPickLicense(object sender,EventArgs e)
    {
        var picked=await PickPhoto(); if(picked==null)return;
        _license=picked; LicenseLabel.Text=picked.FileName; LicensePreview.IsVisible=false;
    }
    private void OnRefreshClicked(object sender,EventArgs e)=>Refresh();
    private async void OnSaveClicked(object sender,EventArgs e)
    {
        if(!SaveButton.IsEnabled)return;
        if(_passport==null && _license==null) { ResultLabel.Text="Выберите новое фото для отправки."; return; }
        SaveButton.IsEnabled=false;
        var created=new List<string>();
        bool committed=false;
        try
        {
            // Read the latest links so replacing one photo does not undo another saved photo.
            var current=_repo.GetByUser(_userId);
            var passport=current?.PassportPhoto ?? "";
            var license=current?.LicensePhoto ?? "";
            if(_passport!=null) { using var stream=await _passport.OpenReadAsync(); passport=await _files.SaveAsync(_userId,stream); created.Add(passport); }
            if(_license!=null) { using var stream=await _license.OpenReadAsync(); license=await _files.SaveAsync(_userId,stream); created.Add(license); }
            if(!_files.CanOpen(_userId,passport) || !_files.CanOpen(_userId,license)) throw new ArgumentException("Выберите доступные фото паспорта и прав.");
            _repo.Upload(_userId,passport,license,current?.Revision ?? 0);
            committed=true; _passport=null; _license=null;
            ResultLabel.Text="Документы отправлены на проверку. Решение появится в уведомлениях.";
            Refresh();
        }
        catch(Exception ex) { ResultLabel.Text="Не удалось сохранить документы: "+ex.Message; }
        finally
        {
            if(!committed) foreach(var path in created) _files.DeleteOwned(_userId,path);
            SaveButton.IsEnabled=true;
        }
    }
}
