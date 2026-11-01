using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace QuanLyKhachSanUI.Repositories
{
    public class SuDungDichVuRepository
    {
        public DataTable GetAllSuDungDichVu()
        {
            string sql = @"SELECT p.SoPhieuSDDV, p.SoPhieuDat, p.SoPhong, p.NgaySuDung, 
                                  dv.TenDV, ct.SoLuong, ct.DonGia, (ct.SoLuong * ct.DonGia) AS ThanhTien, nv.HoTen AS TenNhanVien
                           FROM PhieuSuDungDV p
                           LEFT JOIN ChiTietPhieuSuDungDV ct ON p.SoPhieuSDDV = ct.SoPhieuSDDV
                           LEFT JOIN DichVu dv ON ct.MaDV = dv.MaDV
                           LEFT JOIN NhanVien nv ON p.MaNV = nv.MaNV";
            return DatabaseHelper.ExecuteQuery(sql);
        }

        public void LapPhieuSuDungDichVuTransaction(string soPhieuSD, string soPhieuDat, string soPhong, string maNV, string maDV, int soLuong, decimal donGia)
        {
            DatabaseHelper.ExecuteTransaction((conn, tran) =>
            {
                // 1. Tự động kiểm tra và thêm vào ChiTietDatPhong nếu chưa có để tránh lỗi Foreign Key
                string sqlCheckCT = @"IF NOT EXISTS (SELECT 1 FROM ChiTietDatPhong WHERE SoPhieuDat = @SoPhieuDat AND SoPhong = @SoPhong)
                                      BEGIN
                                          INSERT INTO ChiTietDatPhong(SoPhieuDat, SoPhong, SoNguoi)
                                          VALUES(@SoPhieuDat, @SoPhong, 1)
                                      END";
                using (var cmd0 = new SqlCommand(sqlCheckCT, conn, tran))
                {
                    cmd0.Parameters.AddWithValue("@SoPhieuDat", soPhieuDat);
                    cmd0.Parameters.AddWithValue("@SoPhong", soPhong);
                    cmd0.ExecuteNonQuery();
                }

                // 2. Tạo phiếu SDDV nếu chưa có
                string sqlPhieu = @"IF NOT EXISTS(SELECT 1 FROM PhieuSuDungDV WHERE SoPhieuSDDV = @SoPhieuSD)
                                    BEGIN
                                        INSERT INTO PhieuSuDungDV(SoPhieuSDDV, SoPhieuDat, SoPhong, NgaySuDung, MaNV)
                                        VALUES(@SoPhieuSD, @SoPhieuDat, @SoPhong, GETDATE(), @MaNV)
                                    END";
                using (var cmd1 = new SqlCommand(sqlPhieu, conn, tran))
                {
                    cmd1.Parameters.AddWithValue("@SoPhieuSD", soPhieuSD);
                    cmd1.Parameters.AddWithValue("@SoPhieuDat", soPhieuDat);
                    cmd1.Parameters.AddWithValue("@SoPhong", soPhong);
                    cmd1.Parameters.AddWithValue("@MaNV", maNV);
                    cmd1.ExecuteNonQuery();
                }

                // 3. Chèn Chi tiết dịch vụ
                string sqlChiTiet = @"INSERT INTO ChiTietPhieuSuDungDV(SoPhieuSDDV, MaDV, SoLuong, DonGia) 
                                     VALUES(@SoPhieuSD, @MaDV, @SoLuong, @DonGia)";
                using (var cmd2 = new SqlCommand(sqlChiTiet, conn, tran))
                {
                    cmd2.Parameters.AddWithValue("@SoPhieuSD", soPhieuSD);
                    cmd2.Parameters.AddWithValue("@MaDV", maDV);
                    cmd2.Parameters.AddWithValue("@SoLuong", soLuong);
                    cmd2.Parameters.AddWithValue("@DonGia", donGia);
                    cmd2.ExecuteNonQuery();
                }
            });
        }
    }
}