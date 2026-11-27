using CarsharingApp.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CarsharingApp.Tests;

[TestClass]
public class DocumentFileStoreTests
{
    private string _root="";
    [TestInitialize] public void SetUp() { _root=Path.Combine(Path.GetTempPath(),"carsharing-file-tests-"+Guid.NewGuid().ToString("N")); }
    [TestCleanup] public void CleanUp() { if(Directory.Exists(_root)) Directory.Delete(_root,true); }
    [TestMethod]
    public async Task Save_UsesNewPrivateLinkAndKeepsCopiesIndependent()
    {
        var store=new DocumentFileStore(_root);
        var photo=Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+aN1sAAAAASUVORK5CYII=");
        var first=await store.SaveAsync(4,new MemoryStream(photo));
        var second=await store.SaveAsync(4,new MemoryStream(photo));
        Assert.AreNotEqual(first,second);
        CollectionAssert.AreEqual(photo,await File.ReadAllBytesAsync(first));
        Assert.IsTrue(store.CanOpen(4,first));
        Assert.IsFalse(store.CanOpen(5,first));
        store.DeleteOwned(5,first);
        Assert.IsTrue(File.Exists(first));
        store.DeleteOwned(4,first);
        Assert.IsFalse(File.Exists(first));
        Assert.IsTrue(File.Exists(second));
    }
    [TestMethod]
    public async Task InvalidAndOversizedFiles_DoNotCreateCopies()
    {
        var store=new DocumentFileStore(_root);
        await Assert.ThrowsExceptionAsync<ArgumentException>(()=>store.SaveAsync(4,new MemoryStream(new byte[]{1,2,3,4})));
        await Assert.ThrowsExceptionAsync<ArgumentException>(()=>store.SaveAsync(4,new MemoryStream(new byte[DocumentFileStore.MaxBytes+1])));
        Assert.IsFalse(Directory.Exists(_root));
    }
    [TestMethod]
    public void CannotOpenFileOutsideStorage()
    {
        var store=new DocumentFileStore(_root);
        Assert.IsFalse(store.CanOpen(4,Path.Combine(_root,"4","..","5","passport.png")));
    }
}
