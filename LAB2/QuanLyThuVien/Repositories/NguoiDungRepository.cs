using Microsoft.Data.SqlClient;
using System.Data;
using QuanLyThuVien.DataAccess;

namespace QuanLyThuVien.Repositories
{
    public class NguoiDungRepository
    {
        public DataTable GetAll() =>
            DatabaseHelper.ExecuteQuery("SELECT * FROM NguoiDung ORDER BY MaND");

        public DataTable Search(string keyword) =>
            DatabaseHelper.ExecuteQuery(
                "SELECT * FROM NguoiDung WHERE HoTen LIKE @kw OR TenDangNhap LIKE @kw",
                new SqlParameter("@kw", $"%{keyword}%"));

        public void Insert(string tdn, string mk, string ht, string email, string sdt, string vt) =>
            DatabaseHelper.ExecuteNonQuery(
                @"INSERT INTO NguoiDung (TenDangNhap, MatKhau, HoTen, Email, SoDienThoai, VaiTro)
                  VALUES (@tdn, @mk, @ht, @email, @sdt, @vt)",
                new SqlParameter("@tdn", tdn),
                new SqlParameter("@mk", mk),
                new SqlParameter("@ht", ht),
                new SqlParameter("@email", email),
                new SqlParameter("@sdt", (object)sdt ?? DBNull.Value),
                new SqlParameter("@vt", vt));

        public void Update(int id, string ht, string email, string sdt, string vt) =>
            DatabaseHelper.ExecuteNonQuery(
                "UPDATE NguoiDung SET HoTen=@ht, Email=@email, SoDienThoai=@sdt, VaiTro=@vt WHERE MaND=@id",
                new SqlParameter("@ht", ht),
                new SqlParameter("@email", email),
                new SqlParameter("@sdt", (object)sdt ?? DBNull.Value),
                new SqlParameter("@vt", vt),
                new SqlParameter("@id", id));

        public void Delete(int id) =>
            DatabaseHelper.ExecuteNonQuery("DELETE FROM NguoiDung WHERE MaND=@id",
                new SqlParameter("@id", id));
    }
}