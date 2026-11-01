using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace QuanLyKhachSanUI.Repositories
{
    public class PhieuDatPhongRepository
    {
        public DataTable GetAllPhieuDatPhong()
        {
            string sql = @"SELECT p.SoPhieuDat, kh.HoTen AS TenKhach, nv.HoTen AS TenNVLeTan, 
                                  p.NgayLap, p.NgayNhan, p.NgayTraDuKien, p.TienCoc, p.KenhDat, p.TrangThai
                           FROM PhieuDatPhong p
                           JOIN KhachHang kh ON p.MaKhach = kh.MaKhach
                           JOIN NhanVien nv ON p.MaNVLeTan = nv.MaNV";
            return DatabaseHelper.ExecuteQuery(sql);
        }

        public int InsertPhieuDatPhong(string soPhieu, string maKhach, string maNV, DateTime ngayNhan, DateTime ngayTra, decimal tienCoc, string kenhDat)
        {
            string sql = @"INSERT INTO PhieuDatPhong(SoPhieuDat, MaKhach, MaNVLeTan, NgayLap, NgayNhan, NgayTraDuKien, TienCoc, KenhDat, TrangThai) 
                           VALUES(@SoPhieu, @MaKhach, @MaNV, GETDATE(), @NgayNhan, @NgayTra, @TienCoc, @Kenh, N'Đã đặt')";
            return DatabaseHelper.ExecuteNonQuery(sql,
                new SqlParameter("@SoPhieu", soPhieu),
                new SqlParameter("@MaKhach", maKhach),
                new SqlParameter("@MaNV", maNV),
                new SqlParameter("@NgayNhan", ngayNhan),
                new SqlParameter("@NgayTra", ngayTra),
                new SqlParameter("@TienCoc", tienCoc),
                new SqlParameter("@Kenh", kenhDat));
        }

        public int UpdateTrangThaiPhieu(string soPhieu, string trangThai)
        {
            string sql = "UPDATE PhieuDatPhong SET TrangThai = @TrangThai WHERE SoPhieuDat = @SoPhieu";
            return DatabaseHelper.ExecuteNonQuery(sql,
                new SqlParameter("@SoPhieu", soPhieu),
                new SqlParameter("@TrangThai", trangThai));
        }

        public int AddChiTietDatPhong(string soPhieu, string soPhong, int soNguoi)
        {
            string sql = "INSERT INTO ChiTietDatPhong(SoPhieuDat, SoPhong, SoNguoi) VALUES(@SoPhieu, @SoPhong, @SoNguoi)";
            return DatabaseHelper.ExecuteNonQuery(sql,
                new SqlParameter("@SoPhieu", soPhieu),
                new SqlParameter("@SoPhong", soPhong),
                new SqlParameter("@SoNguoi", soNguoi));
        }
    }
}