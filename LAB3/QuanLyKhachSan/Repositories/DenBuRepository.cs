using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace QuanLyKhachSanUI.Repositories
{
    public class DenBuRepository
    {
        public DataTable GetAllPhieuDenBu()
        {
            string sql = @"SELECT p.SoPhieuDenBu, p.SoPhieuDat, p.SoPhong, p.NgayLap, nv.HoTen AS TenNV, p.TongTien 
                           FROM PhieuDenBu p JOIN NhanVien nv ON p.MaNV = nv.MaNV";
            return DatabaseHelper.ExecuteQuery(sql);
        }

        public DataTable GetQuyDinhDenBu()
        {
            return DatabaseHelper.ExecuteQuery(@"SELECT qd.MaQuyDinh, qd.MaLoaiTN, ltn.TenLoaiTN, qd.MucDoThietHai, qd.MucDenBu 
                                                 FROM QuyDinhDenBu qd 
                                                 JOIN LoaiTienNghi ltn ON qd.MaLoaiTN = ltn.MaLoaiTN");
        }

        public DataTable GetTienNghiByPhong(string soPhong)
        {
            // Bổ sung ltn.MaLoaiTN để frmDenBu tra được đúng mức đền bù theo Loại tiện nghi
            // (không chỉ hiển thị tên loại tiện nghi TenLoaiTN).
            string sql = @"SELECT tn.MaTienNghi, ltn.MaLoaiTN, ltn.TenLoaiTN, tn.SoThuTu, tn.TinhTrangHienTai 
                           FROM PhieuLapDat pld 
                           JOIN TienNghi tn ON pld.MaTienNghi = tn.MaTienNghi 
                           JOIN LoaiTienNghi ltn ON tn.MaLoaiTN = ltn.MaLoaiTN 
                           WHERE pld.SoPhong = @SoPhong";
            return DatabaseHelper.ExecuteQuery(sql, new SqlParameter("@SoPhong", soPhong));
        }

        // Tự tra Số Tiền Đền Bù theo (Loại tiện nghi + Mức độ thiệt hại) từ bảng cấu hình QuyDinhDenBu.
        // Trả về 0 nếu chưa có quy định tương ứng — form sẽ cảnh báo cho nhân viên biết.
        public decimal LayMucDenBuTheoQuyDinh(string maLoaiTN, string mucDoThietHai)
        {
            string sql = "SELECT MucDenBu FROM QuyDinhDenBu WHERE MaLoaiTN = @MaLoaiTN AND MucDoThietHai = @MucDo";
            DataTable dt = DatabaseHelper.ExecuteQuery(sql,
                new SqlParameter("@MaLoaiTN", maLoaiTN),
                new SqlParameter("@MucDo", mucDoThietHai));
            return dt.Rows.Count > 0 ? Convert.ToDecimal(dt.Rows[0]["MucDenBu"]) : 0m;
        }

        public void LapPhieuDenBuTransaction(string soPhieuDB, string soPhieuDat, string soPhong, string maNV, string maTienNghi, string mucDo, decimal soTien)
        {
            DatabaseHelper.ExecuteTransaction((conn, tran) =>
            {
                // 1. Thêm phiếu đền bù
                string sqlPhieu = @"INSERT INTO PhieuDenBu(SoPhieuDenBu, SoPhieuDat, SoPhong, NgayLap, MaNV, TongTien) 
                                   VALUES(@SoPhieuDB, @SoPhieuDat, @SoPhong, GETDATE(), @MaNV, @SoTien)";
                using (var cmd1 = new SqlCommand(sqlPhieu, conn, tran))
                {
                    cmd1.Parameters.AddWithValue("@SoPhieuDB", soPhieuDB);
                    cmd1.Parameters.AddWithValue("@SoPhieuDat", soPhieuDat);
                    cmd1.Parameters.AddWithValue("@SoPhong", soPhong);
                    cmd1.Parameters.AddWithValue("@MaNV", maNV);
                    cmd1.Parameters.AddWithValue("@SoTien", soTien);
                    cmd1.ExecuteNonQuery();
                }

                // 2. Thêm chi tiết đền bù
                string sqlChiTiet = @"INSERT INTO ChiTietPhieuDenBu(SoPhieuDenBu, MaTienNghi, MucDoThietHai, SoTien) 
                                     VALUES(@SoPhieuDB, @MaTienNghi, @MucDo, @SoTien)";
                using (var cmd2 = new SqlCommand(sqlChiTiet, conn, tran))
                {
                    cmd2.Parameters.AddWithValue("@SoPhieuDB", soPhieuDB);
                    cmd2.Parameters.AddWithValue("@MaTienNghi", maTienNghi);
                    cmd2.Parameters.AddWithValue("@MucDo", mucDo);
                    cmd2.Parameters.AddWithValue("@SoTien", soTien);
                    cmd2.ExecuteNonQuery();
                }

                // 3. Cập nhật trạng thái tiện nghi
                string sqlUpdateTN = @"UPDATE TienNghi SET TinhTrangHienTai = @MucDo WHERE MaTienNghi = @MaTienNghi";
                using (var cmd3 = new SqlCommand(sqlUpdateTN, conn, tran))
                {
                    cmd3.Parameters.AddWithValue("@MaTienNghi", maTienNghi);
                    cmd3.Parameters.AddWithValue("@MucDo", mucDo);
                    cmd3.ExecuteNonQuery();
                }
            });
        }
    }
}