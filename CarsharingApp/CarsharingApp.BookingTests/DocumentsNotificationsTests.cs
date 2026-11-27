using CarsharingApp.Data;
using CarsharingApp.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;

namespace CarsharingApp.BookingTests;

[TestClass]
[TestCategory("Database")]
public class DocumentsNotificationsTests
{
    private DatabaseManager _db=null!;
    private int _user, _other, _staff;
    private DocumentRepository Docs=>new(_db);
    private NotificationRepository Notifications=>new(_db);

    [TestInitialize]
    public void SetUp()
    {
        var cs=Environment.GetEnvironmentVariable("CARSHARING_TEST_CONNECTION_STRING");
        if(string.IsNullOrWhiteSpace(cs)) Assert.Inconclusive("Requires a disposable migrated PostgreSQL database.");
        _db=new DatabaseManager(cs!);
        _user=AddUser(); _other=AddUser(); _staff=AddUser();
        _db.Insert("INSERT INTO staff_users(user_id) VALUES(@id)",new NpgsqlParameter("id",_staff));
    }
    private int AddUser()=>Convert.ToInt32(_db.SelectScalar(
        "INSERT INTO users(login,password_hash,full_name) VALUES(@login,'test-only','Test document user') RETURNING id",
        new NpgsqlParameter("login","docs_"+Guid.NewGuid().ToString("N"))));
    [TestCleanup]
    public void CleanUp()
    {
        if(_db!=null) _db.Insert("DELETE FROM users WHERE id=ANY(@ids)",new NpgsqlParameter("ids",new[]{_user,_other,_staff}));
    }
    private int AddNotification(int user,string message,int minutes)=>Convert.ToInt32(_db.SelectScalar(
        "INSERT INTO notifications(user_id,type,message,created_at) VALUES(@user,'Test',@message,LOCALTIMESTAMP+@minutes*INTERVAL '1 minute') RETURNING id",
        new NpgsqlParameter("user",user),new NpgsqlParameter("message",message),new NpgsqlParameter("minutes",minutes)));

    [TestMethod]
    public void Notifications_ArePrivateOrderedAndCounted()
    {
        var older=AddNotification(_user,"older",-5);
        var newer=AddNotification(_user,"newer",0);
        AddNotification(_other,"private",1);
        var rows=Notifications.GetByUser(_user);
        CollectionAssert.AreEqual(new[]{newer,older},rows.Select(n=>n.Id).ToArray());
        Assert.AreEqual(2L,Notifications.GetUnreadCount(_user));
        Assert.IsTrue(rows.All(n=>!n.IsRead));
    }
    [TestMethod]
    public void MarkAsRead_ChecksOwnerAndIsIdempotent()
    {
        var id=AddNotification(_user,"message",0);
        Assert.IsFalse(Notifications.MarkAsRead(id,_other));
        Assert.AreEqual(1L,Notifications.GetUnreadCount(_user));
        Assert.IsTrue(Notifications.MarkAsRead(id,_user));
        Assert.IsTrue(Notifications.MarkAsRead(id,_user));
        Assert.AreEqual(0L,Notifications.GetUnreadCount(_user));
        Assert.IsTrue(Notifications.GetByUser(_user).Single().IsRead);
    }
    [TestMethod]
    public void Upload_CreatesOnePendingKitAndReplacementResetsStatus()
    {
        Assert.IsNull(Docs.GetByUser(_user));
        Docs.Upload(_user,"passport.jpg","license.png");
        var first=Docs.GetByUser(_user)!;
        Assert.AreEqual("passport.jpg",first.PassportPhoto);
        Assert.AreEqual(Document.Pending,first.Status);
        Assert.IsTrue(Docs.ChangeStatus(_staff,first.Id,first.Revision,Document.Approved));
        Docs.Upload(_user,"new.jpg","license.png");
        var changed=Docs.GetByUser(_user)!;
        Assert.AreEqual(first.Id,changed.Id);
        Assert.AreEqual("new.jpg",changed.PassportPhoto);
        Assert.AreEqual(Document.Pending,Docs.GetStatus(_user));
        Assert.IsTrue(changed.Revision>first.Revision);
        Assert.AreEqual(1L,Convert.ToInt64(_db.SelectScalar("SELECT COUNT(*) FROM documents WHERE user_id=@id",new NpgsqlParameter("id",_user))));
    }
    [TestMethod]
    public async Task ConcurrentUploads_PreserveOneKit()
    {
        await Task.WhenAll(Task.Run(()=>Docs.Upload(_user,"p1","l1")),Task.Run(()=>Docs.Upload(_user,"p2","l2")));
        Assert.AreEqual(1L,Convert.ToInt64(_db.SelectScalar("SELECT COUNT(*) FROM documents WHERE user_id=@id",new NpgsqlParameter("id",_user))));
        var kit=Docs.GetByUser(_user)!;
        Assert.AreEqual(kit.PassportPhoto[1],kit.LicensePhoto[1]);
    }
    [TestMethod]
    public void DocumentForeignKey_RejectsUnknownUser()
    {
        var ex=Assert.ThrowsException<PostgresException>(()=>Docs.Upload(int.MaxValue,"passport","license"));
        Assert.AreEqual(PostgresErrorCodes.ForeignKeyViolation,ex.SqlState);
    }
    [TestMethod]
    public void NonStaff_CannotReadQueueOrChangeStatus()
    {
        Docs.Upload(_user,"passport","license");
        var kit=Docs.GetByUser(_user)!;
        Assert.ThrowsException<UnauthorizedAccessException>(()=>Docs.GetPending(_other));
        Assert.ThrowsException<UnauthorizedAccessException>(()=>Docs.ChangeStatus(_user,kit.Id,kit.Revision,Document.Approved));
        Assert.AreEqual(Document.Pending,Docs.GetStatus(_user));
        Assert.AreEqual(0L,Notifications.GetUnreadCount(_user));
    }
    [TestMethod]
    public void StaffDecision_NotifiesOnlyOwnerExactlyOnce()
    {
        Docs.Upload(_user,"passport","license");
        var kit=Docs.GetPending(_staff).Single(d=>d.UserId==_user);
        Assert.IsTrue(Docs.ChangeStatus(_staff,kit.Id,kit.Revision,Document.Rejected));
        Assert.IsFalse(Docs.ChangeStatus(_staff,kit.Id,kit.Revision,Document.Approved));
        Assert.AreEqual(Document.Rejected,Docs.GetStatus(_user));
        var notification=Notifications.GetByUser(_user).Single();
        Assert.IsTrue(notification.Message.Contains(Document.Rejected));
        Assert.AreEqual("Документы",notification.Type);
        Assert.IsFalse(notification.IsRead);
        Assert.AreEqual(0,Notifications.GetByUser(_other).Count);
    }
    [TestMethod]
    public void ReplacedDocuments_CannotBeApprovedUsingStaleRevision()
    {
        Docs.Upload(_user,"passport","license");
        var old=Docs.GetByUser(_user)!;
        Docs.Upload(_user,"new-passport","new-license");
        Assert.IsFalse(Docs.ChangeStatus(_staff,old.Id,old.Revision,Document.Approved));
        Assert.AreEqual(Document.Pending,Docs.GetStatus(_user));
        Assert.AreEqual(0L,Notifications.GetUnreadCount(_user));
    }
    [TestMethod]
    public void RevokedStaff_CannotDecideAndStaffCannotApproveOwnKit()
    {
        Docs.Upload(_staff,"passport","license");
        var own=Docs.GetByUser(_staff)!;
        Assert.IsFalse(Docs.ChangeStatus(_staff,own.Id,own.Revision,Document.Approved));
        Docs.Upload(_user,"passport","license");
        var kit=Docs.GetByUser(_user)!;
        _db.Update("DELETE FROM staff_users WHERE user_id=@id",new NpgsqlParameter("id",_staff));
        Assert.ThrowsException<UnauthorizedAccessException>(()=>Docs.ChangeStatus(_staff,kit.Id,kit.Revision,Document.Approved));
    }
    [TestMethod]
    public void StaleUpload_DoesNotOverwriteAnotherReplacement()
    {
        Docs.Upload(_user,"passport","license");
        var old=Docs.GetByUser(_user)!;
        Docs.Upload(_user,"new-passport","new-license",old.Revision);
        Assert.ThrowsException<InvalidOperationException>(()=>Docs.Upload(_user,"old-passport","old-license",old.Revision));
        Assert.AreEqual("new-passport",Docs.GetByUser(_user)!.PassportPhoto);
    }

    [TestMethod]
    public void InvalidDecision_IsRejectedWithoutNotification()
    {
        Docs.Upload(_user,"passport","license");
        var kit=Docs.GetByUser(_user)!;
        Assert.ThrowsException<ArgumentException>(()=>Docs.ChangeStatus(_staff,kit.Id,kit.Revision,"unexpected"));
        Assert.AreEqual(Document.Pending,Docs.GetStatus(_user));
    }
}
