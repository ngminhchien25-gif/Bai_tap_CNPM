using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using QuanLyCaoOc.Data;

namespace QuanLyCaoOc.Services
{
    /// <summary>Thu tiền thuê (trả trước 6 tháng, hàng tháng từ tháng 7) và hóa đơn dịch vụ cuối tháng.</summary>
    public static class ThanhToanService
    {
        // ---------- Tiền thuê ----------
        public static DataTable LayKyThu(string soHD)
        {
            return Db.Query(
                @"SELECT MaKy, LoaiKy, TuThang, DenThang, SoTien, HanThanhToan, DaThu, NgayThu, HinhThuc, NguoiThu,
                         CASE WHEN DaThu = 1 THEN N'Đã thu'
                              WHEN HanThanhToan < @t THEN N'Quá hạn'
                              WHEN HanThanhToan = @t THEN N'Đến hạn hôm nay'
                              ELSE N'Chưa đến hạn' END AS TinhTrang
                  FROM KyThanhToanThue WHERE SoHD = @h ORDER BY TuThang, MaKy",
                ("@h", soHD), ("@t", DateTime.Today));
        }

        /// <summary>Nợ tiền thuê (kỳ đã đến hạn chưa thu) và nợ dịch vụ (hóa đơn chưa thanh toán) của một hợp đồng.</summary>
        public static (decimal NoThue, decimal NoDichVu) CongNo(DbSession s, string soHD)
        {
            decimal noThue = Convert.ToDecimal(s.Scalar(
                "SELECT ISNULL(SUM(SoTien),0) FROM KyThanhToanThue WHERE SoHD=@h AND DaThu=0 AND HanThanhToan<=@t",
                ("@h", soHD), ("@t", DateTime.Today)));
            decimal noDV = Convert.ToDecimal(s.Scalar(
                "SELECT ISNULL(SUM(TongTien),0) FROM HoaDonDichVu WHERE SoHD=@h AND DaThanhToan=0", ("@h", soHD)));
            return (noThue, noDV);
        }

        public static (decimal NoThue, decimal NoDichVu) CongNo(string soHD)
        {
            using (var s = new DbSession()) { return CongNo(s, soHD); }
        }

        public static (decimal SoTien, int SoNgayTre) ThuTienThue(int maKy, string hinhThuc, string nguoiThu)
        {
            if (string.IsNullOrWhiteSpace(hinhThuc)) throw new NghiepVuException("Chưa chọn hình thức thanh toán.");
            if (string.IsNullOrWhiteSpace(nguoiThu)) throw new NghiepVuException("Chưa nhập người thu tiền.");

            using (var tx = Db.BeginTran())
            {
                DataTable dt = tx.Query(
                    @"SELECT k.SoHD, k.TuThang, k.SoTien, k.DaThu, k.LoaiKy, k.HanThanhToan, h.TrangThai
                      FROM KyThanhToanThue k WITH (UPDLOCK) JOIN HopDong h ON h.SoHD = k.SoHD WHERE k.MaKy=@k", ("@k", maKy));
                if (dt.Rows.Count == 0) throw new NghiepVuException("Không tìm thấy kỳ thu tiền.");
                DataRow r = dt.Rows[0];
                string soHD = (string)r["SoHD"];
                if ((string)r["TrangThai"] == TT.HDHuy) throw new NghiepVuException("Hợp đồng đã huỷ, không thu tiền.");
                if ((bool)r["DaThu"]) throw new NghiepVuException("Kỳ này đã được thu tiền.");

                int truoc = Convert.ToInt32(tx.Scalar(
                    "SELECT COUNT(*) FROM KyThanhToanThue WHERE SoHD=@h AND TuThang<@tu AND DaThu=0 AND MaKy<>@k",
                    ("@h", soHD), ("@tu", (int)r["TuThang"]), ("@k", maKy)));
                if (truoc > 0) throw new NghiepVuException("Còn kỳ thu trước đó chưa thanh toán. Hãy thu theo thứ tự các kỳ.");

                tx.Exec("UPDATE KyThanhToanThue SET DaThu=1, NgayThu=@n, HinhThuc=@ht, NguoiThu=@nt WHERE MaKy=@k",
                    ("@n", DateTime.Today), ("@ht", hinhThuc), ("@nt", nguoiThu.Trim()), ("@k", maKy));

                if ((string)r["LoaiKy"] == TT.KyTraTruoc)   // trả trước xong -> hợp đồng có hiệu lực
                    tx.Exec("UPDATE HopDong SET TrangThai=@hl WHERE SoHD=@h AND TrangThai=@cho",
                        ("@hl", TT.HDHieuLuc), ("@h", soHD), ("@cho", TT.HDCho));

                tx.Commit();
                int tre = Math.Max(0, (DateTime.Today - (DateTime)r["HanThanhToan"]).Days);
                return (Convert.ToDecimal(r["SoTien"]), tre);
            }
        }

        // ---------- Dịch vụ ----------
        public static DataTable LayHoaDonDV(string soHD)
        {
            return Db.Query(
                @"SELECT SoHDDV, Thang, Nam, NgayLap, HanThanhToan, TongTien, DaThanhToan, NgayThanhToan,
                         CASE WHEN DaThanhToan = 1 THEN N'Đã thanh toán' ELSE N'Chưa thanh toán' END AS TinhTrang
                  FROM HoaDonDichVu WHERE SoHD=@h ORDER BY Nam DESC, Thang DESC", ("@h", soHD));
        }

        public static DataTable LayChiTietHoaDonDV(string soHDDV)
        {
            return Db.Query(
                @"SELECT c.MaPhong, d.TenDV, d.DonViTinh, c.SoLuong, c.DonGia, c.ThanhTien
                  FROM ChiTietHoaDonDV c JOIN DichVu d ON d.MaDV=c.MaDV WHERE c.SoHDDV=@s ORDER BY c.MaPhong, c.MaDV",
                ("@s", soHDDV));
        }

        public static string LapHoaDonDichVu(string soHD, int thang, int nam, IList<DongDV> dong)
        {
            if (string.IsNullOrWhiteSpace(soHD)) throw new NghiepVuException("Chưa chọn hợp đồng.");
            if (thang < 1 || thang > 12 || nam < 2000) throw new NghiepVuException("Tháng/năm không hợp lệ.");
            if (dong.Count == 0) throw new NghiepVuException("Hóa đơn phải có ít nhất một dòng dịch vụ.");
            if (dong.Any(d => d.SoLuong < 0)) throw new NghiepVuException("Số lượng sử dụng không được âm.");

            using (var tx = Db.BeginTran())
            {
                DataTable hd = tx.Query("SELECT NgayBatDau, NgayKetThuc, TrangThai FROM HopDong WHERE SoHD=@h", ("@h", soHD));
                if (hd.Rows.Count == 0) throw new NghiepVuException("Không tìm thấy hợp đồng.");
                string tt = (string)hd.Rows[0]["TrangThai"];
                if (tt != TT.HDHieuLuc && tt != TT.HDHetHan)
                    throw new NghiepVuException("Chỉ lập hóa đơn dịch vụ cho hợp đồng đã thanh toán tiền trả trước.");

                var dauThang = new DateTime(nam, thang, 1);
                DateTime cuoiThang = dauThang.AddMonths(1).AddDays(-1);
                if (cuoiThang < (DateTime)hd.Rows[0]["NgayBatDau"] || dauThang > (DateTime)hd.Rows[0]["NgayKetThuc"])
                    throw new NghiepVuException("Tháng lập hóa đơn nằm ngoài thời gian thuê của hợp đồng.");

                if (Convert.ToInt32(tx.Scalar("SELECT COUNT(*) FROM HoaDonDichVu WHERE SoHD=@h AND Thang=@t AND Nam=@n",
                        ("@h", soHD), ("@t", thang), ("@n", nam))) > 0)
                    throw new NghiepVuException("Hóa đơn dịch vụ tháng " + thang + "/" + nam + " của hợp đồng này đã được lập.");

                string prefix = "DV" + nam + "-" + thang.ToString("00") + "-";
                int next = Convert.ToInt32(tx.Scalar(
                    "SELECT ISNULL(MAX(CAST(RIGHT(SoHDDV,3) AS INT)),0)+1 FROM HoaDonDichVu WHERE SoHDDV LIKE @p", ("@p", prefix + "%")));
                string so = prefix + next.ToString("000");

                tx.Exec("INSERT INTO HoaDonDichVu(SoHDDV,SoHD,Thang,Nam,NgayLap,HanThanhToan) VALUES(@s,@h,@t,@n,@nl,@han)",
                    ("@s", so), ("@h", soHD), ("@t", thang), ("@n", nam), ("@nl", DateTime.Today), ("@han", cuoiThang));

                foreach (var g in dong.GroupBy(d => new { d.MaPhong, d.MaDV }))
                {
                    if (Convert.ToInt32(tx.Scalar("SELECT COUNT(*) FROM ChiTietHopDong WHERE SoHD=@h AND MaPhong=@p",
                            ("@h", soHD), ("@p", g.Key.MaPhong))) == 0)
                        throw new NghiepVuException("Phòng " + g.Key.MaPhong + " không thuộc hợp đồng " + soHD + ".");
                    object dg = tx.Scalar("SELECT DonGia FROM DichVu WHERE MaDV=@d", ("@d", g.Key.MaDV));
                    if (dg == null) throw new NghiepVuException("Dịch vụ " + g.Key.MaDV + " không tồn tại.");
                    tx.Exec("INSERT INTO ChiTietHoaDonDV(SoHDDV,MaPhong,MaDV,SoLuong,DonGia) VALUES(@s,@p,@d,@sl,@dg)",
                        ("@s", so), ("@p", g.Key.MaPhong), ("@d", g.Key.MaDV), ("@sl", g.Sum(x => x.SoLuong)), ("@dg", Convert.ToDecimal(dg)));
                }
                tx.Exec("UPDATE HoaDonDichVu SET TongTien=(SELECT SUM(ThanhTien) FROM ChiTietHoaDonDV WHERE SoHDDV=@s) WHERE SoHDDV=@s",
                    ("@s", so));
                tx.Commit();
                return so;
            }
        }

        public static void ThanhToanHoaDonDV(string soHDDV)
        {
            if (string.IsNullOrWhiteSpace(soHDDV)) throw new NghiepVuException("Chưa chọn hóa đơn dịch vụ.");
            int n = Db.Exec("UPDATE HoaDonDichVu SET DaThanhToan=1, NgayThanhToan=@n WHERE SoHDDV=@s AND DaThanhToan=0",
                ("@n", DateTime.Today), ("@s", soHDDV));
            if (n == 0) throw new NghiepVuException("Hóa đơn không tồn tại hoặc đã được thanh toán.");
        }
    }
}
