using Microsoft.Data.SqlClient;
using System.Data;

namespace QuanLyThuVien.DataAccess
{
    public static class DatabaseHelper
    {
        private static readonly string connectionString =
            @"Server=.;Database=QuanLyCongTyDuLich;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;";

        public static SqlConnection GetConnection() => new SqlConnection(connectionString);

        public static DataTable ExecuteQuery(string sql, params SqlParameter[] parameters)
        {
            using var conn = GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            if (parameters != null) cmd.Parameters.AddRange(parameters);

            var dt = new DataTable();
            using var adapter = new SqlDataAdapter(cmd);
            adapter.Fill(dt);
            return dt;
        }

        public static int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            using var conn = GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            if (parameters != null) cmd.Parameters.AddRange(parameters);

            conn.Open();
            return cmd.ExecuteNonQuery();
        }
        public static void ExecuteTransaction(Action<SqlConnection, SqlTransaction> actions)
        {
            using var conn = GetConnection();
            conn.Open();
            using var tran = conn.BeginTransaction();
            try
            {
                actions(conn, tran);
                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }
    }
}