using Microsoft.Data.SqlClient;
using System.Data;

namespace QuanLyKhachSanUI.Repositories
{
    public class KhachHangRepository
    {
        public DataTable GetAllKhachHang()
        {
            return DatabaseHelper.ExecuteQuery("SELECT * FROM KhachHang");
        }

        public int InsertKhachHang(string maKhach, string hoTen, string soCMND, string quocTich, string soDienThoai)
        {
            string sql = @"INSERT INTO KhachHang(MaKhach, HoTen, SoCMND, QuocTich, SoDienThoai) 
                           VALUES(@MaKhach, @HoTen, @SoCMND, @QuocTich, @SoDienThoai)";
            return DatabaseHelper.ExecuteNonQuery(sql,
                new SqlParameter("@MaKhach", maKhach),
                new SqlParameter("@HoTen", hoTen),
                new SqlParameter("@SoCMND", soCMND),
                new SqlParameter("@QuocTich", quocTich),
                new SqlParameter("@SoDienThoai", (object)soDienThoai ?? DBNull.Value));
        }

        public int UpdateKhachHang(string maKhach, string hoTen, string soCMND, string quocTich, string soDienThoai)
        {
            string sql = @"UPDATE KhachHang 
                           SET HoTen = @HoTen, SoCMND = @SoCMND, QuocTich = @QuocTich, SoDienThoai = @SoDienThoai 
                           WHERE MaKhach = @MaKhach";
            return DatabaseHelper.ExecuteNonQuery(sql,
                new SqlParameter("@MaKhach", maKhach),
                new SqlParameter("@HoTen", hoTen),
                new SqlParameter("@SoCMND", soCMND),
                new SqlParameter("@QuocTich", quocTich),
                new SqlParameter("@SoDienThoai", (object)soDienThoai ?? DBNull.Value));
        }

        public int DeleteKhachHang(string maKhach)
        {
            return DatabaseHelper.ExecuteNonQuery("DELETE FROM KhachHang WHERE MaKhach = @MaKhach", 
                new SqlParameter("@MaKhach", maKhach));
        }
    }
}