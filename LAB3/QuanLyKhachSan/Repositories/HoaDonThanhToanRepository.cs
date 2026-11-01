using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace QuanLyKhachSanUI.Repositories
{
    public class HoaDonThanhToanRepository
    {
        public DataTable GetAllHoaDon()
        {
            string sql = @"SELECT hd.SoHoaDon, hd.SoPhieuDat, hd.NgayLap, nv.HoTen AS TenNV, 
                                  hd.SoNgayTinhTien, hd.TienPhong, hd.TienDichVu, hd.TongTien, hd.TrangThai
                           FROM HoaDon hd
                           JOIN NhanVien nv ON hd.MaNV = nv.MaNV";
            return DatabaseHelper.ExecuteQuery(sql);
        }

        // Tổng đơn giá/ngày của các phòng thuộc phiếu đặt (dùng để tự tính Tiền Phòng = tổng đơn giá x số ngày)
        public decimal LayTongDonGiaPhong(string soPhieuDat)
        {
            string sql = @"SELECT ISNULL(SUM(p.DonGiaNgay), 0) AS Tong
                           FROM ChiTietDatPhong c JOIN Phong p ON c.SoPhong = p.SoPhong
                           WHERE c.SoPhieuDat = @SoPhieuDat";
            DataTable dt = DatabaseHelper.ExecuteQuery(sql, new SqlParameter("@SoPhieuDat", soPhieuDat));
            return dt.Rows.Count > 0 ? Convert.ToDecimal(dt.Rows[0]["Tong"]) : 0m;
        }

        // Tổng tiền dịch vụ đã ghi nhận (đã cộng dồn theo ngày) của phiếu đặt
        public decimal LayTongTienDichVu(string soPhieuDat)
        {
            string sql = @"SELECT ISNULL(SUM(c.ThanhTien), 0) AS Tong
                           FROM PhieuSuDungDV h JOIN ChiTietPhieuSuDungDV c ON h.SoPhieuSDDV = c.SoPhieuSDDV
                           WHERE h.SoPhieuDat = @SoPhieuDat";
            DataTable dt = DatabaseHelper.ExecuteQuery(sql, new SqlParameter("@SoPhieuDat", soPhieuDat));
            return dt.Rows.Count > 0 ? Convert.ToDecimal(dt.Rows[0]["Tong"]) : 0m;
        }

        // Lập hóa đơn (chưa thanh toán). Tiền phòng/dịch vụ được tính sẵn ở Form trước khi gọi xuống đây.
        public void LapHoaDon(string soHD, string soPhieuDat, string maNV, int soNgay, decimal tienPhong, decimal tienDichVu)
        {
            string sql = @"INSERT INTO HoaDon(SoHoaDon, SoPhieuDat, NgayLap, MaNV, SoNgayTinhTien, TienPhong, TienDichVu, TrangThai) 
                           VALUES(@SoHD, @SoPhieuDat, GETDATE(), @MaNV, @SoNgay, @TienPhong, @TienDV, N'Chưa thanh toán')";
            DatabaseHelper.ExecuteNonQuery(sql,
                new SqlParameter("@SoHD", soHD),
                new SqlParameter("@SoPhieuDat", soPhieuDat),
                new SqlParameter("@MaNV", maNV),
                new SqlParameter("@SoNgay", soNgay),
                new SqlParameter("@TienPhong", tienPhong),
                new SqlParameter("@TienDV", tienDichVu));
        }

        // Ghi nhận 1 giao dịch thanh toán cho hóa đơn (1 hóa đơn có thể có nhiều giao dịch).
        // Từ chối nếu tổng đã trả + số tiền lần này vượt quá TongTien của hóa đơn.
        // Khi đủ tiền, tự cập nhật HoaDon.TrangThai = N'Đã thanh toán'.
        public void GhiNhanThanhToan(string maTT, string soHD, string hinhThuc, decimal soTien)
        {
            DatabaseHelper.ExecuteTransaction((conn, tran) =>
            {
                decimal tongTien;
                using (var cmdTong = new SqlCommand("SELECT TongTien FROM HoaDon WHERE SoHoaDon = @SoHD", conn, tran))
                {
                    cmdTong.Parameters.AddWithValue("@SoHD", soHD);
                    object oTong = cmdTong.ExecuteScalar();
                    if (oTong == null) throw new Exception("Không tìm thấy hóa đơn.");
                    tongTien = Convert.ToDecimal(oTong);
                }

                decimal daTra;
                using (var cmdDaTra = new SqlCommand("SELECT ISNULL(SUM(SoTien),0) FROM ThanhToan WHERE SoHoaDon = @SoHD", conn, tran))
                {
                    cmdDaTra.Parameters.AddWithValue("@SoHD", soHD);
                    daTra = Convert.ToDecimal(cmdDaTra.ExecuteScalar());
                }

                if (daTra + soTien > tongTien)
                    throw new Exception($"Số tiền thanh toán vượt số còn phải trả (còn lại {tongTien - daTra:N0} VNĐ).");

                using (var cmdInsert = new SqlCommand(
                    @"INSERT INTO ThanhToan(MaThanhToan, SoHoaDon, NgayThanhToan, HinhThuc, SoTien) 
                      VALUES(@MaTT, @SoHD, GETDATE(), @HinhThuc, @SoTien)", conn, tran))
                {
                    cmdInsert.Parameters.AddWithValue("@MaTT", maTT);
                    cmdInsert.Parameters.AddWithValue("@SoHD", soHD);
                    cmdInsert.Parameters.AddWithValue("@HinhThuc", hinhThuc);
                    cmdInsert.Parameters.AddWithValue("@SoTien", soTien);
                    cmdInsert.ExecuteNonQuery();
                }

                if (daTra + soTien == tongTien)
                {
                    using (var cmdUpdate = new SqlCommand(
                        "UPDATE HoaDon SET TrangThai = N'Đã thanh toán' WHERE SoHoaDon = @SoHD", conn, tran))
                    {
                        cmdUpdate.Parameters.AddWithValue("@SoHD", soHD);
                        cmdUpdate.ExecuteNonQuery();
                    }
                }
            });
        }

        public DataTable GetThanhToanByHoaDon(string soHoaDon)
        {
            string sql = @"SELECT MaThanhToan, NgayThanhToan, HinhThuc, SoTien 
                           FROM ThanhToan WHERE SoHoaDon = @SoHD ORDER BY NgayThanhToan";
            return DatabaseHelper.ExecuteQuery(sql, new SqlParameter("@SoHD", soHoaDon));
        }

        // Hoàn tất trả phòng cho hóa đơn: chỉ cho phép khi hóa đơn đã "Đã thanh toán".
        // Cập nhật PhieuDatPhong -> 'Đã trả' và Phong -> 'Trống' cho các phòng thuộc phiếu đặt đó.
        public void TraPhongTheoHoaDon(string soHoaDon)
        {
            DatabaseHelper.ExecuteTransaction((conn, tran) =>
            {
                string soPhieuDat; string trangThai;
                using (var cmd = new SqlCommand("SELECT SoPhieuDat, TrangThai FROM HoaDon WHERE SoHoaDon = @SoHD", conn, tran))
                {
                    cmd.Parameters.AddWithValue("@SoHD", soHoaDon);
                    using (var rd = cmd.ExecuteReader())
                    {
                        if (!rd.Read()) throw new Exception("Không tìm thấy hóa đơn.");
                        soPhieuDat = rd.GetString(0);
                        trangThai = rd.GetString(1);
                    }
                }

                if (trangThai != "Đã thanh toán")
                    throw new Exception("Hóa đơn chưa thanh toán đủ, không thể trả phòng.");

                using (var cmd1 = new SqlCommand(
                    "UPDATE PhieuDatPhong SET TrangThai = N'Đã trả', NgayTraThucTe = GETDATE() WHERE SoPhieuDat = @SoPhieuDat", conn, tran))
                {
                    cmd1.Parameters.AddWithValue("@SoPhieuDat", soPhieuDat);
                    cmd1.ExecuteNonQuery();
                }

                using (var cmd2 = new SqlCommand(
                    @"UPDATE Phong SET TrangThai = N'Trống' 
                      WHERE SoPhong IN (SELECT SoPhong FROM ChiTietDatPhong WHERE SoPhieuDat = @SoPhieuDat)", conn, tran))
                {
                    cmd2.Parameters.AddWithValue("@SoPhieuDat", soPhieuDat);
                    cmd2.ExecuteNonQuery();
                }
            });
        }
    }
}