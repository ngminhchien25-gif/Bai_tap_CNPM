using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace QuanLyCaoOc.Data
{
    public class NghiepVuException : Exception
    {
        public NghiepVuException(string message) : base(message) { }
    }

    public static class TT
    {
        public const string PhongTrong = "Trống", PhongDangThue = "Đang thuê", PhongBaoTri = "Bảo trì";
        public const string HDCho = "Chờ thanh toán", HDHieuLuc = "Hiệu lực", HDHetHan = "Hết hạn", HDHuy = "Huỷ";
        public const string KyTraTruoc = "Trả trước", KyHangThang = "Hàng tháng";
    }

    public static class Db
    {
        public static string ConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["QLCaoOc"].ConnectionString; }
        }

        public static DataTable Query(string sql, params (string, object)[] p)
        { using (var s = new DbSession()) { return s.Query(sql, p); } }

        public static object Scalar(string sql, params (string, object)[] p)
        { using (var s = new DbSession()) { return s.Scalar(sql, p); } }

        public static int Exec(string sql, params (string, object)[] p)
        { using (var s = new DbSession()) { return s.Exec(sql, p); } }

        public static DbSession BeginTran() { return new DbSession(true); }

        public static decimal ThamSo(string ma)
        {
            object v = Scalar("SELECT GiaTri FROM ThamSo WHERE MaThamSo=@m", ("@m", ma));
            if (v == null) throw new NghiepVuException("Thiếu tham số hệ thống " + ma + ".");
            return Convert.ToDecimal(v);
        }

        public static string ThongBaoLoi(SqlException ex)
        {
            switch (ex.Number)
            {
                case 2627:
                case 2601: return "Dữ liệu bị trùng (mã hoặc giá trị duy nhất đã tồn tại).";
                case 547: return "Thao tác vi phạm ràng buộc dữ liệu (đang được tham chiếu hoặc giá trị không hợp lệ).";
                case 515: return "Thiếu dữ liệu bắt buộc.";
                case -1:
                case 53:
                case 4060: return "Không kết nối được cơ sở dữ liệu. Hãy kiểm tra chuỗi kết nối trong App.config.";
                default: return "Lỗi cơ sở dữ liệu. Vui lòng thử lại.";
            }
        }
    }

    public sealed class DbSession : IDisposable
    {
        private readonly SqlConnection _cn;
        private SqlTransaction _tx;

        public DbSession(bool transaction = false)
        {
            _cn = new SqlConnection(Db.ConnectionString);
            _cn.Open();
            if (transaction) _tx = _cn.BeginTransaction();
        }

        private SqlCommand Cmd(string sql, (string, object)[] p)
        {
            var c = _cn.CreateCommand();
            c.Transaction = _tx;
            c.CommandText = sql;
            foreach (var item in p)
                c.Parameters.AddWithValue(item.Item1, item.Item2 ?? DBNull.Value);
            return c;
        }

        public DataTable Query(string sql, params (string, object)[] p)
        {
            var dt = new DataTable();
            using (var c = Cmd(sql, p))
            using (var da = new SqlDataAdapter(c))
            {
                da.Fill(dt);
            }
            return dt;
        }

        public object Scalar(string sql, params (string, object)[] p)
        {
            using (var c = Cmd(sql, p))
            {
                object r = c.ExecuteScalar();
                return r == DBNull.Value ? null : r;
            }
        }

        public int Exec(string sql, params (string, object)[] p)
        {
            using (var c = Cmd(sql, p)) { return c.ExecuteNonQuery(); }
        }

        public void Commit() { if (_tx != null) { _tx.Commit(); _tx = null; } }

        public void Dispose()
        {
            try { if (_tx != null) _tx.Rollback(); } catch { /* đã đóng */ }
            _cn.Dispose();
        }
    }
}
