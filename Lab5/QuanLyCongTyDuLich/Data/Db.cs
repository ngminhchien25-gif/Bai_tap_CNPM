using System;
using System.Data;
using Microsoft.Data.SqlClient;
using QuanLyThuVien.DataAccess; 

namespace QuanLyCongTyDuLich.Data
{
    public static class Db
    {
        /// <summary>
        /// Lấy kết nối từ DatabaseHelper
        /// </summary>
        public static SqlConnection OpenConnection()
        {
            var cn = DatabaseHelper.GetConnection();
            cn.Open();
            return cn;
        }

        /// <summary>
        /// Thực thi câu lệnh SELECT và trả về kết quả dạng bảng (DataTable).
        /// </summary>
        public static DataTable Query(string sql, params SqlParameter[] ps)
        {
            using (var cn = OpenConnection())
            using (var cmd = new SqlCommand(sql, cn))
            using (var da = new SqlDataAdapter(cmd))
            {
                if (ps != null && ps.Length > 0) 
                {
                    cmd.Parameters.AddRange(ps);
                }
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        /// <summary>
        /// Thực thi các câu lệnh INSERT, UPDATE, DELETE và trả về số dòng bị ảnh hưởng.
        /// </summary>
        public static int Execute(string sql, params SqlParameter[] ps)
        {
            using (var cn = OpenConnection())
            using (var cmd = new SqlCommand(sql, cn))
            {
                if (ps != null && ps.Length > 0) 
                {
                    cmd.Parameters.AddRange(ps);
                }
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Thực thi câu lệnh SQL trả về một giá trị đơn duy nhất (ví dụ: COUNT, SUM, MAX hoặc SELECT cột đơn).
        /// </summary>
        public static object Scalar(string sql, params SqlParameter[] ps)
        {
            using (var cn = OpenConnection())
            using (var cmd = new SqlCommand(sql, cn))
            {
                if (ps != null && ps.Length > 0) 
                {
                    cmd.Parameters.AddRange(ps);
                }
                return cmd.ExecuteScalar();
            }
        }

        /// <summary>
        /// Tạo SqlParameter ngắn gọn. 
        /// Tự động chuyển đổi các giá trị null, chuỗi rỗng hoặc khoảng trắng về DBNull.Value trong SQL Server.
        /// </summary>
        public static SqlParameter P(string name, object value)
        {
            if (value == null) 
            {
                return new SqlParameter(name, DBNull.Value);
            }

            if (value is string s)
            {
                if (string.IsNullOrWhiteSpace(s))
                {
                    return new SqlParameter(name, DBNull.Value);
                }
                return new SqlParameter(name, s.Trim());
            }

            return new SqlParameter(name, value);
        }
    }
}