using CarsharingApp.Models;
using Npgsql;

namespace CarsharingApp.Data;

public class NotificationRepository(DatabaseManager db)
{
    public List<Notification> GetByUser(int userId) => db.Select(
        "SELECT id,user_id,COALESCE(type,''),COALESCE(message,''),COALESCE(is_read,false),COALESCE(created_at,LOCALTIMESTAMP) FROM notifications WHERE user_id=@user ORDER BY created_at DESC NULLS LAST,id DESC",
        r => new Notification { Id=r.GetInt32(0), UserId=r.GetInt32(1), Type=r.GetString(2), Message=r.GetString(3), IsRead=r.GetBoolean(4), CreatedAt=r.GetDateTime(5) },
        new NpgsqlParameter("user",userId));

    public long GetUnreadCount(int userId) => Convert.ToInt64(db.SelectScalar(
        "SELECT COUNT(*) FROM notifications WHERE user_id=@user AND COALESCE(is_read,false)=false", new NpgsqlParameter("user",userId)));

    public bool MarkAsRead(int notificationId, int userId) => db.Update(
        "UPDATE notifications SET is_read=true WHERE id=@id AND user_id=@user",
        new NpgsqlParameter("id",notificationId),new NpgsqlParameter("user",userId)) == 1;
}
