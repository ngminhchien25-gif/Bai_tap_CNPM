using System;
using System.Data.SqlClient;
using QuanLyNhanSu.Data;

namespace QuanLyNhanSu.Services
{
    public class HopDongService
    {
        public IdResult LapHopDong(string maKH, string maPhong, int thoiHanThue, DateTime ngayBatDau, decimal donGiaThue)
        {
            // BR01: Thời hạn hợp đồng tối thiểu 6 tháng
            if (thoiHanThue < 6)
            {
                return new IdResult { Success = false, Message = "LỖI (BR01): Thời hạn thuê tối thiểu phải từ 6 tháng trở lên!" };
            }

            // BR02: Tiền cọc 6 tháng đầu
            decimal tienCoc6Thang = donGiaThue * 6;
            string maHD = Db.TaoMa("HD");
            string maPhieuTT = Db.TaoMa("PTT");
            DateTime ngayKetThuc = ngayBatDau.AddMonths(thoiHanThue);

            using (SqlConnection cn = Db.OpenConnection())
            using (SqlTransaction tx = cn.BeginTransaction())
            {
                try
                {
                    // 1. Thêm hợp đồng thuê
                    string sqlHD = @"INSERT INTO HopDongThue(MaHD, MaKH, NgayLap, NgayBatDau, NgayKetThuc, ThoiHanThueThang, TienCoc, TrangThaiHD)
                                     VALUES(@MaHD, @MaKH, GETDATE(), @NBD, @NKT, @ThoiHan, @TienCoc, N'Hiệu lực')";
                    Db.Exec(cn, tx, sqlHD,
                        Db.P("@MaHD", maHD), Db.P("@MaKH", maKH),
                        Db.P("@NBD", ngayBatDau), Db.P("@NKT", ngayKetThuc),
                        Db.P("@ThoiHan", thoiHanThue), Db.P("@TienCoc", tienCoc6Thang));

                    // 2. Thêm chi tiết hợp đồng
                    string sqlCTHD = "INSERT INTO ChiTietHopDong(MaHD, MaPhong, DonGiaThueTaiThoiDiem) VALUES(@MaHD, @MaPhong, @Gia)";
                    Db.Exec(cn, tx, sqlCTHD, Db.P("@MaHD", maHD), Db.P("@MaPhong", maPhong), Db.P("@Gia", donGiaThue));

                    // 3. Đổi trạng thái phòng sang 'Đã thuê'
                    string sqlPhong = "UPDATE Phong SET TrangThai = N'Đã thuê' WHERE MaPhong = @MaPhong";
                    Db.Exec(cn, tx, sqlPhong, Db.P("@MaPhong", maPhong));

                    // 4. Lập phiếu thu tiền thuê 6 tháng đầu
                    string sqlPTT = @"INSERT INTO PhieuThanhToanHopDong(MaPhieuTT, MaHD, KyThanhToan, NgayThanhToan, SoTienTT, TrangThai)
                                      VALUES(@MaPTT, @MaHD, 1, GETDATE(), @SoTien, N'Đã thanh toán')";
                    Db.Exec(cn, tx, sqlPTT, Db.P("@MaPTT", maPhieuTT), Db.P("@MaHD", maHD), Db.P("@SoTien", tienCoc6Thang));

                    tx.Commit();
                    return new IdResult { Success = true, Message = $"Ký hợp đồng thành công! Mã HĐ: {maHD}. Tiền cọc 6 tháng đã thu: {tienCoc6Thang:N0} VNĐ" };
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    return new IdResult { Success = false, Message = "Lỗi khi lập hợp đồng: " + ex.Message };
                }
            }
        }
    }
}