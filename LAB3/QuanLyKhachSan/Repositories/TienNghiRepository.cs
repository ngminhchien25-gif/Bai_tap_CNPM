using Microsoft.Data.SqlClient;
using System.Data;

namespace QuanLyKhachSanUI.Repositories
{
    public class TienNghiRepository
    {
        // Lấy danh sách tất cả loại tiện nghi
        public DataTable GetAllLoaiTienNghi()
        {
            return DatabaseHelper.ExecuteQuery("SELECT * FROM LoaiTienNghi");
        }

        // Lấy danh sách thiết bị đầy đủ kèm loại thiết bị
        public DataTable GetAllTienNghi()
        {
            string sql = @"SELECT tn.MaTienNghi, ltn.TenLoaiTN, tn.SoThuTu, tn.TinhTrangHienTai 
                           FROM TienNghi tn 
                           JOIN LoaiTienNghi ltn ON tn.MaLoaiTN = ltn.MaLoaiTN";
            return DatabaseHelper.ExecuteQuery(sql);
        }

        // Thêm loại tiện nghi mới
        public int InsertLoaiTienNghi(string maLoai, string tenLoai)
        {
            string sql = "INSERT INTO LoaiTienNghi(MaLoaiTN, TenLoaiTN) VALUES(@MaLoai, @TenLoai)";
            return DatabaseHelper.ExecuteNonQuery(sql,
                new SqlParameter("@MaLoai", maLoai),
                new SqlParameter("@TenLoai", tenLoai));
        }

        // Thêm thiết bị tiện nghi mới
        public int InsertTienNghi(string maTienNghi, string maLoaiTN, int soThuTu, string tinhTrang)
        {
            string sql = @"INSERT INTO TienNghi(MaTienNghi, MaLoaiTN, SoThuTu, TinhTrangHienTai) 
                           VALUES(@MaTienNghi, @MaLoaiTN, @SoThuTu, @TinhTrang)";
            return DatabaseHelper.ExecuteNonQuery(sql,
                new SqlParameter("@MaTienNghi", maTienNghi),
                new SqlParameter("@MaLoaiTN", maLoaiTN),
                new SqlParameter("@SoThuTu", soThuTu),
                new SqlParameter("@TinhTrang", tinhTrang));
        }
    }
}