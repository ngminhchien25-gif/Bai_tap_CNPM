using Microsoft.Data.SqlClient;
using System.Data;

namespace QuanLyKhachSanUI.Repositories
{
    public class PhongRepository
    {
        public DataTable GetAllPhong()
        {
            // Bổ sung p.MaKhuVuc để frmPhong có thể chọn đúng ComboBox Khu Vực theo mã
            // khi bấm vào 1 dòng trong lưới (thay vì so khớp theo chuỗi TenKhuVuc).
            string sql = @"SELECT p.SoPhong, p.MaKhuVuc, k.TenKhuVuc, p.SoNguoiToiDa, p.DonGiaNgay, p.TrangThai 
                           FROM Phong p JOIN KhuVuc k ON p.MaKhuVuc = k.MaKhuVuc";
            return DatabaseHelper.ExecuteQuery(sql);
        }

        public DataTable GetAllKhuVuc()
        {
            return DatabaseHelper.ExecuteQuery("SELECT * FROM KhuVuc");
        }

        public int InsertPhong(string soPhong, string maKhuVuc, int soNguoi, decimal donGia, string trangThai)
        {
            string sql = @"INSERT INTO Phong(SoPhong, MaKhuVuc, SoNguoiToiDa, DonGiaNgay, TrangThai) 
                           VALUES(@SoPhong, @MaKhuVuc, @SoNguoi, @DonGia, @TrangThai)";
            return DatabaseHelper.ExecuteNonQuery(sql,
                new SqlParameter("@SoPhong", soPhong),
                new SqlParameter("@MaKhuVuc", maKhuVuc),
                new SqlParameter("@SoNguoi", soNguoi),
                new SqlParameter("@DonGia", donGia),
                new SqlParameter("@TrangThai", trangThai));
        }

        public int UpdatePhong(string soPhong, string maKhuVuc, int soNguoi, decimal donGia, string trangThai)
        {
            string sql = @"UPDATE Phong 
                           SET MaKhuVuc = @MaKhuVuc, SoNguoiToiDa = @SoNguoi, DonGiaNgay = @DonGia, TrangThai = @TrangThai 
                           WHERE SoPhong = @SoPhong";
            return DatabaseHelper.ExecuteNonQuery(sql,
                new SqlParameter("@SoPhong", soPhong),
                new SqlParameter("@MaKhuVuc", maKhuVuc),
                new SqlParameter("@SoNguoi", soNguoi),
                new SqlParameter("@DonGia", donGia),
                new SqlParameter("@TrangThai", trangThai));
        }

        public int DeletePhong(string soPhong)
        {
            return DatabaseHelper.ExecuteNonQuery("DELETE FROM Phong WHERE SoPhong = @SoPhong", new SqlParameter("@SoPhong", soPhong));
        }
    }
}