using CarsharingApp.Models;
using Npgsql;

namespace CarsharingApp.Data;

public class DocumentRepository(DatabaseManager db)
{
    public bool IsStaff(int userId) => Convert.ToInt64(db.SelectScalar(
        "SELECT COUNT(*) FROM staff_users WHERE user_id=@user", new NpgsqlParameter("user",userId))) > 0;

    public Document? GetByUser(int userId) => db.Select(
        "SELECT id,user_id,COALESCE(passport_photo,''),COALESCE(license_photo,''),status,revision FROM documents WHERE user_id=@user",
        ReadDocument, new NpgsqlParameter("user",userId)).SingleOrDefault();

    public string GetStatus(int userId) => GetByUser(userId)?.Status ?? "не загружены";

    public void Upload(int userId, string passportPhoto, string licensePhoto, int? expectedRevision = null)
    {
        if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
        if (string.IsNullOrWhiteSpace(passportPhoto) || string.IsNullOrWhiteSpace(licensePhoto))
            throw new ArgumentException("Выберите фото паспорта и водительских прав.");
        var rows = db.Insert(@"INSERT INTO documents(user_id,passport_photo,license_photo,status,revision)
            VALUES(@user,@passport,@license,'на проверке',1)
            ON CONFLICT(user_id) DO UPDATE SET passport_photo=EXCLUDED.passport_photo,
            license_photo=EXCLUDED.license_photo,status='на проверке',revision=documents.revision+1
            WHERE @expected IS NULL OR documents.revision=@expected",
            new NpgsqlParameter("user",userId), new NpgsqlParameter("passport",passportPhoto), new NpgsqlParameter("license",licensePhoto),
            new NpgsqlParameter("expected", NpgsqlTypes.NpgsqlDbType.Integer) { Value = (object?)expectedRevision ?? DBNull.Value });
        if (rows != 1) throw new InvalidOperationException("Комплект изменился в другом окне. Обновите статус и повторите отправку.");
    }

    public List<Document> GetPending(int staffUserId)
    {
        RequireStaff(staffUserId);
        return db.Select(@"SELECT d.id,d.user_id,COALESCE(d.passport_photo,''),COALESCE(d.license_photo,''),d.status,d.revision,u.full_name
            FROM documents d JOIN users u ON u.id=d.user_id WHERE d.status='на проверке' AND d.user_id<>@staff ORDER BY d.id",
            r => { var d=ReadDocument(r); d.UserName=r.GetString(6); return d; },
            new NpgsqlParameter("staff",staffUserId));
    }

    public bool ChangeStatus(int staffUserId, int documentId, int expectedRevision, string status)
    {
        if (status != Document.Approved && status != Document.Rejected)
            throw new ArgumentException("Допустимые решения: одобрено или отклонено.",nameof(status));
        using var conn=db.Connect();
        using var tx=conn.BeginTransaction();
        // Hold the role row until commit, so role revocation cannot race a decision.
        using var role=new NpgsqlCommand("SELECT user_id FROM staff_users WHERE user_id=@staff FOR SHARE",conn,tx);
        role.Parameters.AddWithValue("staff",staffUserId);
        if (role.ExecuteScalar()==null) throw new UnauthorizedAccessException("Проверка документов доступна только сотруднику.");
        using var update=new NpgsqlCommand(@"UPDATE documents SET status=@status,revision=revision+1
            WHERE id=@id AND revision=@revision AND status='на проверке' AND user_id<>@staff RETURNING user_id",conn,tx);
        update.Parameters.AddWithValue("status",status);
        update.Parameters.AddWithValue("id",documentId);
        update.Parameters.AddWithValue("revision",expectedRevision);
        update.Parameters.AddWithValue("staff",staffUserId);
        if (update.ExecuteScalar() is not int userId) return false;
        using var notification=new NpgsqlCommand(@"INSERT INTO notifications(user_id,type,message,is_read,created_at)
            VALUES(@user,'Документы',@message,false,LOCALTIMESTAMP)",conn,tx);
        notification.Parameters.AddWithValue("user",userId);
        notification.Parameters.AddWithValue("message",$"Статус ваших документов изменён: {status}.");
        notification.ExecuteNonQuery();
        tx.Commit();
        return true;
    }

    private void RequireStaff(int userId)
    {
        if (!IsStaff(userId)) throw new UnauthorizedAccessException("Проверка документов доступна только сотруднику.");
    }

    private static Document ReadDocument(NpgsqlDataReader r) => new()
    {
        Id=r.GetInt32(0), UserId=r.GetInt32(1), PassportPhoto=r.GetString(2),
        LicensePhoto=r.GetString(3), Status=r.GetString(4), Revision=r.GetInt32(5)
    };
}
