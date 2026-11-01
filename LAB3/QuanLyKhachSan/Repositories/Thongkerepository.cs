using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace QuanLyKhachSanUI.Repositories
{
    public class ThongKeRepository
    {
        // Tổng hợp số liệu trong khoảng thời gian: số phiếu đặt, số đang ở hiện tại,
        // số hóa đơn, doanh thu hóa đơn, tổng tiền đền bù.
        public DataTable TongHop(DateTime tu, DateTime den)
        {
            string sql = @"SELECT 
                (SELECT COUNT(*) FROM PhieuDatPhong WHERE CAST(NgayLap AS date) BETWEEN @Tu AND @Den) AS SoPhieuDat,
                (SELECT COUNT(*) FROM PhieuDatPhong WHERE TrangThai = N'Đang ở') AS DangO,
                (SELECT COUNT(*) FROM HoaDon WHERE CAST(NgayLap AS date) BETWEEN @Tu AND @Den) AS SoHoaDon,
                (SELECT ISNULL(SUM(TongTien),0) FROM HoaDon WHERE CAST(NgayLap AS date) BETWEEN @Tu AND @Den) AS DoanhThuHoaDon,
                (SELECT ISNULL(SUM(TongTien),0) FROM PhieuDenBu WHERE CAST(NgayLap AS date) BETWEEN @Tu AND @Den) AS TongDenBu";
            return DatabaseHelper.ExecuteQuery(sql,
                new SqlParameter("@Tu", tu.Date),
                new SqlParameter("@Den", den.Date));
        }

        // Thống kê dịch vụ sử dụng theo khoảng thời gian: tổng số lượng, tổng tiền theo từng dịch vụ.
        public DataTable ThongKeDichVu(DateTime tu, DateTime den)
        {
            string sql = @"SELECT d.MaDV, d.TenDV, SUM(c.SoLuong) AS TongSoLuong, SUM(c.ThanhTien) AS TongTien
                           FROM PhieuSuDungDV p
                           JOIN ChiTietPhieuSuDungDV c ON p.SoPhieuSDDV = c.SoPhieuSDDV
                           JOIN DichVu d ON c.MaDV = d.MaDV
                           WHERE p.NgaySuDung BETWEEN @Tu AND @Den
                           GROUP BY d.MaDV, d.TenDV
                           ORDER BY TongTien DESC";
            return DatabaseHelper.ExecuteQuery(sql,
                new SqlParameter("@Tu", tu.Date),
                new SqlParameter("@Den", den.Date));
        }
    }
}