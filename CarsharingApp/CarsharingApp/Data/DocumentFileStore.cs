namespace CarsharingApp.Data;

public class DocumentFileStore
{
    public const int MaxBytes = 10 * 1024 * 1024;
    private readonly string _root;
    public DocumentFileStore(string? root = null)
    {
        _root=Path.GetFullPath(root ?? Environment.GetEnvironmentVariable("CARSHARING_DOCUMENTS_DIRECTORY")
            ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Carsharing","Documents"));
    }

    public async Task<string> SaveAsync(int userId, Stream input)
    {
        if(userId<=0) throw new ArgumentOutOfRangeException(nameof(userId));
        using var data=new MemoryStream();
        var buffer=new byte[81920];
        int length;
        while((length=await input.ReadAsync(buffer))>0)
        {
            if(data.Length+length>MaxBytes) throw new ArgumentException("Фото должно быть не больше 10 МБ.");
            await data.WriteAsync(buffer.AsMemory(0,length));
        }
        var bytes=data.ToArray();
        var png=bytes.Length>=8 && bytes.AsSpan(0,8).SequenceEqual(new byte[]{137,80,78,71,13,10,26,10});
        var jpeg=bytes.Length>=3 && bytes[0]==255 && bytes[1]==216 && bytes[2]==255;
        if(!png && !jpeg) throw new ArgumentException("Выберите фото в формате JPEG или PNG.");
        var dir=Path.Combine(_root,userId.ToString());
        Directory.CreateDirectory(dir);
        var path=Path.Combine(dir,Guid.NewGuid().ToString("N")+(png?".png":".jpg"));
        try { await File.WriteAllBytesAsync(path,bytes); }
        catch { if(File.Exists(path)) File.Delete(path); throw; }
        return path;
    }

    public bool CanOpen(int userId,string path)
    {
        if(string.IsNullOrWhiteSpace(path)) return false;
        var folder=Path.GetFullPath(Path.Combine(_root,userId.ToString()))+Path.DirectorySeparatorChar;
        var fullPath=Path.GetFullPath(path);
        return fullPath.StartsWith(folder,OperatingSystem.IsWindows()?StringComparison.OrdinalIgnoreCase:StringComparison.Ordinal)
            && File.Exists(fullPath);
    }

    public void DeleteOwned(int userId,string path)
    {
        try { if(CanOpen(userId,path)) File.Delete(path); }
        catch(IOException) { }
        catch(UnauthorizedAccessException) { }
    }
}
