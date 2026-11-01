using Microsoft.Data.SqlClient;
using System.Data;

namespace QuanLyKhachSanUI.Repositories
{
    public class NhanVienRepository
    {
        public DataTable GetAllNhanVien()
        {
            return DatabaseHelper.ExecuteQuery("SELECT * FROM NhanVien");
        }

        public int InsertNhanVien(string maNV, string hoTen, string vaiTro, string soDienThoai)
        {
            string sql = @"INSERT INTO NhanVien(MaNV, HoTen, VaiTro, SoDienThoai) 
                           VALUES(@MaNV, @HoTen, @VaiTro, @SoDienThoai)";
            return DatabaseHelper.ExecuteNonQuery(sql,
                new SqlParameter("@MaNV", maNV),
                new SqlParameter("@HoTen", hoTen),
                new SqlParameter("@VaiTro", vaiTro),
                new SqlParameter("@SoDienThoai", (object)soDienThoai ?? DBNull.Value));
        }

        public int UpdateNhanVien(string maNV, string hoTen, string vaiTro, string soDienThoai)
        {
            string sql = @"UPDATE NhanVien 
                           SET HoTen = @HoTen, VaiTro = @VaiTro, SoDienThoai = @SoDienThoai 
                           WHERE MaNV = @MaNV";
            return DatabaseHelper.ExecuteNonQuery(sql,
                new SqlParameter("@MaNV", maNV),
                new SqlParameter("@HoTen", hoTen),
                new SqlParameter("@VaiTro", vaiTro),
                new SqlParameter("@SoDienThoai", (object)soDienThoai ?? DBNull.Value));
        }

        public int DeleteNhanVien(string maNV)
        {
            return DatabaseHelper.ExecuteNonQuery("DELETE FROM NhanVien WHERE MaNV = @MaNV", 
                new SqlParameter("@MaNV", maNV));
        }
    }
}