using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using QuanLyCaoOc.Data;

namespace QuanLyCaoOc.Services
{
    /// <summary>Nghiệp vụ "Lập phiếu gia hạn": chỉ gia hạn khi còn hiệu lực, không nợ tiền thuê,
    /// đã thanh toán dịch vụ, mỗi phòng gia hạn không quá 1/2 thời gian thuê.</summary>
    public static class GiaHanService
    {
        private static string Tien(decimal v) { return v.ToString("N0", new CultureInfo("vi-VN")) + " đ"; }

        public static DieuKienGiaHan KiemTra(DbSession s, string soHD)
        {
            DataTable hd = s.Query("SELECT SoThang, NgayKetThuc, TrangThai FROM HopDong WHERE SoHD=@h", ("@h", soHD));
            if (hd.Rows.Count == 0) throw new NghiepVuException("Không tìm thấy hợp đồng.");
            int soThang = (int)hd.Rows[0]["SoThang"];
            DateTime kt = (DateTime)hd.Rows[0]["NgayKetThuc"];
            string tt = (string)hd.Rows[0]["TrangThai"];
            var no = ThanhToanService.CongNo(s, soHD);

            var ly = new List<string>();
            if (tt != TT.HDHieuLuc) ly.Add("hợp đồng đang ở trạng thái '" + tt + "', chỉ gia hạn khi hợp đồng còn hiệu lực");
            else if (DateTime.Today > kt) ly.Add("đã quá ngày kết thúc hợp đồng, không còn trong thời gian cho phép gia hạn");
            if (no.NoThue > 0) ly.Add("còn nợ tiền thuê " + Tien(no.NoThue));
            if (no.NoDichVu > 0) ly.Add("còn hóa đơn dịch vụ chưa thanh toán " + Tien(no.NoDichVu));

            return new DieuKienGiaHan(ly.Count == 0, string.Join("; ", ly), soThang, kt, no.NoThue, no.NoDichVu);
        }

        public static DieuKienGiaHan KiemTra(string soHD)
        {
            using (var s = new DbSession()) { return KiemTra(s, soHD); }
        }

        public static DataTable LayLichSu(string soHD)
        {
            return Db.Query(
                @"SELECT p.SoPG, p.NgayLap, c.MaPhong, c.SoThangGiaHan, c.NgayKetThucCu, c.NgayKetThucMoi, p.GhiChu
                  FROM PhieuGiaHan p JOIN ChiTietGiaHan c ON c.SoPG = p.SoPG
                  WHERE p.SoHD=@h ORDER BY p.NgayLap DESC, p.SoPG DESC, c.MaPhong", ("@h", soHD));
        }

        public static string LapPhieuGiaHan(string soHD, string maNVQL, IList<Tuple<string, int>> ds, string ghiChu)
        {
            if (string.IsNullOrWhiteSpace(soHD)) throw new NghiepVuException("Chưa chọn hợp đồng.");
            if (string.IsNullOrWhiteSpace(maNVQL)) throw new NghiepVuException("Chưa chọn người lập phiếu.");
            if (ds.Count == 0) throw new NghiepVuException("Phiếu gia hạn phải có ít nhất một phòng.");
            if (ds.GroupBy(x => x.Item1).Any(g => g.Count() > 1))
                throw new NghiepVuException("Mỗi phòng chỉ được ghi một dòng trong phiếu gia hạn.");

            using (var tx = Db.BeginTran())
            {
                DieuKienGiaHan dk = KiemTra(tx, soHD);
                if (!dk.Duoc) throw new NghiepVuException("Không thể gia hạn: " + dk.LyDo + ".");

                DateTime ngayBD = (DateTime)tx.Query("SELECT NgayBatDau FROM HopDong WITH (UPDLOCK) WHERE SoHD=@h", ("@h", soHD)).Rows[0][0];

                // Kiểm tra từng dòng trước khi ghi.
                foreach (var d in ds)
                {
                    if (d.Item2 <= 0) throw new NghiepVuException("Phòng " + d.Item1 + ": số tháng gia hạn phải lớn hơn 0.");
                    if (d.Item2 * 2 > dk.SoThang)
                        throw new NghiepVuException("Phòng " + d.Item1 + ": gia hạn tối đa " + dk.ToiDaThang +
                            " tháng (không quá 1/2 thời gian thuê " + dk.SoThang + " tháng).");
                }

                string prefix = "GH" + DateTime.Today.Year + "-";
                int next = Convert.ToInt32(tx.Scalar(
                    "SELECT ISNULL(MAX(CAST(RIGHT(SoPG,3) AS INT)),0)+1 FROM PhieuGiaHan WHERE SoPG LIKE @p", ("@p", prefix + "%")));
                string soPG = prefix + next.ToString("000");

                tx.Exec("INSERT INTO PhieuGiaHan(SoPG,SoHD,NgayLap,MaNVQL,GhiChu) VALUES(@s,@h,@n,@nv,@g)",
                    ("@s", soPG), ("@h", soHD), ("@n", DateTime.Today), ("@nv", maNVQL), ("@g", ghiChu));

                foreach (var d in ds)
                {
                    string mp = d.Item1; int n = d.Item2;
                    DataTable ct = tx.Query("SELECT GiaThue, NgayKetThuc FROM ChiTietHopDong WITH (UPDLOCK) WHERE SoHD=@h AND MaPhong=@p",
                        ("@h", soHD), ("@p", mp));
                    if (ct.Rows.Count == 0) throw new NghiepVuException("Phòng " + mp + " không thuộc hợp đồng " + soHD + ".");
                    decimal gia = Convert.ToDecimal(ct.Rows[0]["GiaThue"]);
                    DateTime cu = (DateTime)ct.Rows[0]["NgayKetThuc"];
                    DateTime moi = cu.AddDays(1).AddMonths(n).AddDays(-1);

                    tx.Exec("INSERT INTO ChiTietGiaHan(SoPG,MaPhong,SoThangGiaHan,NgayKetThucCu,NgayKetThucMoi) VALUES(@s,@p,@n,@cu,@moi)",
                        ("@s", soPG), ("@p", mp), ("@n", n), ("@cu", cu), ("@moi", moi));
                    tx.Exec("UPDATE ChiTietHopDong SET NgayKetThuc=@moi WHERE SoHD=@h AND MaPhong=@p",
                        ("@moi", moi), ("@h", soHD), ("@p", mp));

                    // Phát sinh các kỳ thu hàng tháng cho thời gian gia hạn (trả vào đầu mỗi tháng).
                    DateTime batDauGH = cu.AddDays(1);
                    int thangDaThue = (batDauGH.Year - ngayBD.Year) * 12 + batDauGH.Month - ngayBD.Month;
                    for (int k = 1; k <= n; k++)
                    {
                        int idx = thangDaThue + k;
                        DateTime han = ngayBD.AddMonths(idx - 1);
                        int capNhat = tx.Exec(
                            "UPDATE KyThanhToanThue SET SoTien=SoTien+@t WHERE SoHD=@h AND TuThang=@m AND LoaiKy=@l AND DaThu=0",
                            ("@t", gia), ("@h", soHD), ("@m", idx), ("@l", TT.KyHangThang));
                        if (capNhat == 0)
                            tx.Exec(@"INSERT INTO KyThanhToanThue(SoHD,LoaiKy,TuThang,DenThang,SoTien,HanThanhToan)
                                      VALUES(@h,@l,@m,@m,@t,@han)",
                                ("@h", soHD), ("@l", TT.KyHangThang), ("@m", idx), ("@t", gia), ("@han", han));
                    }
                }

                tx.Exec("UPDATE HopDong SET NgayKetThuc=(SELECT MAX(NgayKetThuc) FROM ChiTietHopDong WHERE SoHD=@h) WHERE SoHD=@h",
                    ("@h", soHD));
                tx.Commit();
                return soPG;
            }
        }
    }
}
