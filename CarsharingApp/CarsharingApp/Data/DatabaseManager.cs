using Npgsql;

namespace CarsharingApp.Data
{
    public class DatabaseManager
    {
        private readonly string _connectionString;

        public DatabaseManager(string connectionString)
        {
            _connectionString = connectionString;
        }

        public NpgsqlConnection Connect()
        {
            var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            return conn;
        }

        public int Insert(string sql, params NpgsqlParameter[] parameters)
        {
            using var conn = Connect();
            using var cmd = new NpgsqlCommand(sql, conn);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteNonQuery();
        }

        public object SelectScalar(string sql, params NpgsqlParameter[] parameters)
        {
            using var conn = Connect();
            using var cmd = new NpgsqlCommand(sql, conn);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteScalar();
        }
    }
}