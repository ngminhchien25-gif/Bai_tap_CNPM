using Microsoft.Data.SqlClient;
using System.Data;

namespace BenhVien.DataAccess;

public static class DatabaseHelper
{
    // Đổi Server nếu dùng SQL Express: @"Server=.\SQLEXPRESS;..."
    public static string ConnectionString { get; set; } =
        @"Server=.;Database=QLHoSoBenhAn;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;";

    public static SqlConnection GetConnection() => new SqlConnection(ConnectionString);

    // ---------- Truy vấn thường ----------
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

    public static object ExecuteScalar(string sql, params SqlParameter[] parameters)
    {
        using var conn = GetConnection();
        using var cmd = new SqlCommand(sql, conn);
        if (parameters != null) cmd.Parameters.AddRange(parameters);

        conn.Open();
        return cmd.ExecuteScalar();
    }

    // ---------- Stored procedure ----------
    /// <summary>Gọi proc có trả về bảng (SELECT).</summary>
    public static DataTable ExecuteProcTable(string proc, params SqlParameter[] parameters)
    {
        using var conn = GetConnection();
        using var cmd = new SqlCommand(proc, conn) { CommandType = CommandType.StoredProcedure };
        if (parameters != null) cmd.Parameters.AddRange(parameters);

        var dt = new DataTable();
        using var adapter = new SqlDataAdapter(cmd);
        adapter.Fill(dt);
        return dt;
    }

    /// <summary>Gọi proc không trả bảng. Tham số OUTPUT đọc lại qua chính đối tượng SqlParameter đã truyền vào.</summary>
    public static void ExecuteProc(string proc, params SqlParameter[] parameters)
    {
        using var conn = GetConnection();
        using var cmd = new SqlCommand(proc, conn) { CommandType = CommandType.StoredProcedure };
        if (parameters != null) cmd.Parameters.AddRange(parameters);

        conn.Open();
        cmd.ExecuteNonQuery();
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

    // ---------- Tạo tham số ----------
    public static SqlParameter P(string name, object value) => new SqlParameter(name, value ?? DBNull.Value);

    public static SqlParameter PDate(string name, DateTime? v) =>
        new SqlParameter(name, SqlDbType.Date) { Value = v.HasValue ? (object)v.Value.Date : DBNull.Value };

    public static SqlParameter PDateTime(string name, DateTime v) =>
        new SqlParameter(name, SqlDbType.DateTime2) { Value = v };

    /// <summary>Cột VARBINARY: NULL phải khai báo đúng kiểu, nếu không SQL Server báo lỗi chuyển kiểu.</summary>
    public static SqlParameter PBytes(string name, byte[] data) =>
        new SqlParameter(name, SqlDbType.VarBinary, -1) { Value = (object)data ?? DBNull.Value };

    public static SqlParameter PMoney(string name, decimal v) =>
        new SqlParameter(name, SqlDbType.Decimal) { Precision = 12, Scale = 0, Value = v };

    public static SqlParameter POut(string name, SqlDbType type, int size) =>
        new SqlParameter(name, type, size) { Direction = ParameterDirection.Output };
}
