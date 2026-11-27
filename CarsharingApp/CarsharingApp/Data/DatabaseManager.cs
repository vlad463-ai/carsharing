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
            try
            {
                conn.Open();
                return conn;
            }
            catch
            {
                conn.Dispose();
                throw;
            }
        }

        public int Insert(string sql, params NpgsqlParameter[] parameters)
        {
            using var conn = Connect();
            using var cmd = new NpgsqlCommand(sql, conn);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteNonQuery();
        }

        public object? SelectScalar(string sql, params NpgsqlParameter[] parameters)
        {
            using var conn = Connect();
            using var cmd = new NpgsqlCommand(sql, conn);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteScalar();
        }

        public int Update(string sql, params NpgsqlParameter[] parameters) => Insert(sql, parameters);

        public List<T> Select<T>(string sql, Func<NpgsqlDataReader, T> map, params NpgsqlParameter[] parameters)
        {
            using var conn = Connect();
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddRange(parameters);
            using var reader = cmd.ExecuteReader();
            var rows = new List<T>();
            while (reader.Read()) rows.Add(map(reader));
            return rows;
        }
    }
}
