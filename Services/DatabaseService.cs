using Npgsql;
using System;
using System.Data;

namespace shoppro.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService()
        {
            _connectionString = Environment.GetEnvironmentVariable("SHOPPRO_DB_CONNECTION")
                                ?? throw new InvalidOperationException("Database connection string not found in environment variables.");
        }

        public NpgsqlConnection GetConnection()
        {
            var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            return conn;
        }

        public DataTable ExecuteQuery(string query)
        {
            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(query, conn);
            using var adapter = new NpgsqlDataAdapter(cmd);
            var dt = new DataTable();
            adapter.Fill(dt);
            return dt;
        }

        public int ExecuteNonQuery(string query)
        {
            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(query, conn);
            return cmd.ExecuteNonQuery();
        }
    }
}