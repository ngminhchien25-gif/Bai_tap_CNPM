using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using QuanLyCaoOc.Data;

namespace QuanLyCaoOc.Services
{
    /// <summary>Lập hợp đồng thuê nhiều phòng, sinh lịch thu tiền thuê, hủy hợp đồng, cập nhật hết hạn.</summary>
    public static class HopDongService
    {
        public static DataTable LayHopDong(string trangThai = null)
        {
            return Db.Query(
                @"SELECT h.SoHD, k.TenCongTy, h.NgayLap, h.NgayBatDau, h.NgayKetThuc, h.SoThang, h.TrangThai,
                         (SELECT SUM(c.GiaThue) FROM ChiTietHopDong c WHERE c.SoHD = h.SoHD) AS GiaThueThang
                  FROM HopDong h JOIN KhachHang k ON k.MaKH = h.MaKH
                  WHERE (@tt IS NULL OR h.TrangThai = @tt)
                  ORDER BY h.NgayLap DESC, h.SoHD DESC", ("@tt", trangThai));
        }

        /// <summary>Danh sách hợp đồng để chọn trong ComboBox (HienThi = số HĐ - công ty).</summary>
        public static DataTable DanhSachChon(params string[] trangThai)
        {
            string dk = trangThai.Length == 0
                ? "h.TrangThai <> N'Huỷ'"
                : "h.TrangThai IN (" + string.Join(",", trangThai.Select((t, i) => "@t" + i)) + ")";
            return Db.Query(
                "SELECT h.SoHD, h.SoHD + N' - ' + k.TenCongTy AS HienThi FROM HopDong h " +
                "JOIN KhachHang k ON k.MaKH = h.MaKH WHERE " + dk + " ORDER BY h.SoHD DESC",
                trangThai.Select((t, i) => ("@t" + i, (object)t)).ToArray());
        }

        public static DataTable LayChiTiet(string soHD)
        {
            return Db.Query(
                @"SELECT c.MaPhong, t.DoCao AS Tang, p.DienTich, c.GiaThue, c.NgayKetThuc
                  FROM ChiTietHopDong c JOIN Phong p ON p.MaPhong = c.MaPhong JOIN Tang t ON t.MaTang = p.MaTang
                  WHERE c.SoHD = @h ORDER BY c.MaPhong", ("@h", soHD));
        }

        public static DataTable PhongCuaHopDong(string soHD)
        {
            return Db.Query("SELECT MaPhong FROM ChiTietHopDong WHERE SoHD=@h ORDER BY MaPhong", ("@h", soHD));
        }

        public static DataTable PhongTrong()
        {
            return Db.Query("SELECT MaPhong, DoCao, DienTich, Huong, GiaThue FROM vw_PhongTrong ORDER BY DoCao, MaPhong");
        }

        public static string LapHopDong(string maKH, string maNVQL, DateTime ngayBatDau, int soThang, IList<string> dsPhong)
        {
            int toiThieu = (int)Db.ThamSo("SO_THANG_TOI_THIEU");
            int traTruoc = (int)Db.ThamSo("SO_THANG_TRA_TRUOC");

            if (string.IsNullOrWhiteSpace(maKH)) throw new NghiepVuException("Chưa chọn khách hàng.");
            if (string.IsNullOrWhiteSpace(maNVQL)) throw new NghiepVuException("Chưa chọn người lập hợp đồng.");
            if (dsPhong.Count == 0) throw new NghiepVuException("Phải chọn ít nhất một văn phòng để thuê.");
            if (soThang < toiThieu) throw new NghiepVuException("Thời gian thuê tối thiểu là " + toiThieu + " tháng.");

            ngayBatDau = ngayBatDau.Date;
            DateTime ngayKetThuc = ngayBatDau.AddMonths(soThang).AddDays(-1);

            using (var tx = Db.BeginTran())
            {
                // 1. Khóa và kiểm tra từng phòng còn trống, lấy giá chốt.
                var gia = new Dictionary<string, decimal>();
                foreach (string mp in dsPhong.Distinct())
                {
                    DataTable dt = tx.Query("SELECT GiaThue, TrangThai FROM Phong WITH (UPDLOCK, HOLDLOCK) WHERE MaPhong=@p", ("@p", mp));
                    if (dt.Rows.Count == 0) throw new NghiepVuException("Phòng " + mp + " không tồn tại.");
                    if ((string)dt.Rows[0]["TrangThai"] != TT.PhongTrong)
                        throw new NghiepVuException("Phòng " + mp + " không còn trống để cho thuê.");
                    gia[mp] = Convert.ToDecimal(dt.Rows[0]["GiaThue"]);
                }

                // 2. Sinh số hợp đồng.
                string prefix = "HD" + DateTime.Today.Year + "-";
                int next = Convert.ToInt32(tx.Scalar(
                    "SELECT ISNULL(MAX(CAST(RIGHT(SoHD,3) AS INT)),0)+1 FROM HopDong WHERE SoHD LIKE @pf", ("@pf", prefix + "%")));
                string soHD = prefix + next.ToString("000");

                // 3. Hợp đồng và chi tiết phòng.
                tx.Exec(@"INSERT INTO HopDong(SoHD,MaKH,MaNVQL,NgayLap,NgayBatDau,SoThang,NgayKetThuc,TrangThai)
                          VALUES(@h,@kh,@nv,@nl,@bd,@st,@kt,@tt)",
                    ("@h", soHD), ("@kh", maKH), ("@nv", maNVQL), ("@nl", DateTime.Today), ("@bd", ngayBatDau),
                    ("@st", soThang), ("@kt", ngayKetThuc), ("@tt", TT.HDCho));

                foreach (var kv in gia)
                {
                    tx.Exec("INSERT INTO ChiTietHopDong(SoHD,MaPhong,GiaThue,NgayKetThuc) VALUES(@h,@p,@g,@kt)",
                        ("@h", soHD), ("@p", kv.Key), ("@g", kv.Value), ("@kt", ngayKetThuc));
                    tx.Exec("UPDATE Phong SET TrangThai=@tt WHERE MaPhong=@p", ("@tt", TT.PhongDangThue), ("@p", kv.Key));
                }

                // 4. Lịch thu tiền: trả trước các tháng đầu, từ tháng kế tiếp trả vào đầu mỗi tháng.
                decimal tongThang = gia.Values.Sum();
                const string sqlKy = @"INSERT INTO KyThanhToanThue(SoHD,LoaiKy,TuThang,DenThang,SoTien,HanThanhToan)
                                       VALUES(@h,@l,@tu,@den,@t,@han)";
                tx.Exec(sqlKy, ("@h", soHD), ("@l", TT.KyTraTruoc), ("@tu", 1), ("@den", traTruoc),
                    ("@t", tongThang * traTruoc), ("@han", ngayBatDau));
                for (int k = traTruoc + 1; k <= soThang; k++)
                    tx.Exec(sqlKy, ("@h", soHD), ("@l", TT.KyHangThang), ("@tu", k), ("@den", k),
                        ("@t", tongThang), ("@han", ngayBatDau.AddMonths(k - 1)));

                tx.Commit();
                return soHD;
            }
        }

        /// <summary>Hủy hợp đồng chưa thanh toán tiền trả trước; trả phòng về trạng thái trống.</summary>
        public static void HuyHopDong(string soHD)
        {
            using (var tx = Db.BeginTran())
            {
                string tt = tx.Scalar("SELECT TrangThai FROM HopDong WHERE SoHD=@h", ("@h", soHD)) as string;
                if (tt == null) throw new NghiepVuException("Không tìm thấy hợp đồng.");
                if (tt != TT.HDCho) throw new NghiepVuException("Chỉ hủy được hợp đồng đang chờ thanh toán tiền trả trước.");
                tx.Exec("UPDATE HopDong SET TrangThai=@tt WHERE SoHD=@h", ("@tt", TT.HDHuy), ("@h", soHD));
                tx.Exec(@"UPDATE p SET TrangThai=@t FROM Phong p
                          JOIN ChiTietHopDong c ON c.MaPhong=p.MaPhong WHERE c.SoHD=@h", ("@t", TT.PhongTrong), ("@h", soHD));
                tx.Commit();
            }
        }

        /// <summary>Đánh dấu hợp đồng hết hạn và trả phòng về trống (gọi khi mở ứng dụng).</summary>
        public static void CapNhatHetHan()
        {
            using (var tx = Db.BeginTran())
            {
                tx.Exec("UPDATE HopDong SET TrangThai=@het WHERE TrangThai=@hl AND NgayKetThuc < @t",
                    ("@het", TT.HDHetHan), ("@hl", TT.HDHieuLuc), ("@t", DateTime.Today));
                tx.Exec(@"UPDATE Phong SET TrangThai=@trong
                          WHERE TrangThai=@thue AND NOT EXISTS (
                              SELECT 1 FROM ChiTietHopDong c JOIN HopDong h ON h.SoHD=c.SoHD
                              WHERE c.MaPhong=Phong.MaPhong AND h.TrangThai IN (@cho,@hl) AND c.NgayKetThuc >= @t)",
                    ("@trong", TT.PhongTrong), ("@thue", TT.PhongDangThue), ("@cho", TT.HDCho),
                    ("@hl", TT.HDHieuLuc), ("@t", DateTime.Today));
                tx.Commit();
            }
        }
    }
}
