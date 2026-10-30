using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using QuanLyCaoOc.Data;

namespace QuanLyCaoOc.Services
{
    /// <summary>Kiểm tra định kỳ tình trạng phòng (1 năm/lần) và xử phạt tự ý thay đổi kết cấu.</summary>
    public static class VanHanhService
    {
        private static readonly string[] TinhTrangHopLe = { "Tốt", "Cần tu sửa", "Hư hỏng nặng" };

        public static DataTable LayPhongKiemTra()
        {
            return Db.Query(
                @"SELECT v.MaPhong, v.TrangThai, v.NgayKiemTraGanNhat,
                         CASE WHEN v.DenHan = 1 THEN N'Đến hạn kiểm tra' ELSE N'Chưa đến hạn' END AS TinhTrang
                  FROM vw_PhongKiemTra v ORDER BY v.DenHan DESC, v.MaPhong");
        }

        public static string LapPhieuKiemTra(string maNVQL, DateTime ngay, string ketLuan, IList<DongKT> ds)
        {
            if (string.IsNullOrWhiteSpace(maNVQL)) throw new NghiepVuException("Chưa chọn người kiểm tra.");
            if (ngay.Date > DateTime.Today) throw new NghiepVuException("Ngày kiểm tra không được ở tương lai.");
            if (ds.Count == 0) throw new NghiepVuException("Phiếu kiểm tra phải có ít nhất một phòng.");
            if (ds.GroupBy(d => d.MaPhong).Any(g => g.Count() > 1)) throw new NghiepVuException("Mỗi phòng chỉ ghi một dòng.");
            if (ds.Any(d => !TinhTrangHopLe.Contains(d.TinhTrang))) throw new NghiepVuException("Tình trạng phòng không hợp lệ.");

            using (var tx = Db.BeginTran())
            {
                string prefix = "KT" + ngay.Year + "-";
                int next = Convert.ToInt32(tx.Scalar(
                    "SELECT ISNULL(MAX(CAST(RIGHT(MaKT,3) AS INT)),0)+1 FROM PhieuKiemTra WHERE MaKT LIKE @p", ("@p", prefix + "%")));
                string ma = prefix + next.ToString("000");

                tx.Exec("INSERT INTO PhieuKiemTra(MaKT,NgayKT,MaNVQL,KetLuan) VALUES(@m,@n,@nv,@kl)",
                    ("@m", ma), ("@n", ngay.Date), ("@nv", maNVQL), ("@kl", ketLuan));
                foreach (var d in ds)
                    tx.Exec("INSERT INTO ChiTietKiemTra(MaKT,MaPhong,TinhTrang,CanTuSua,GhiChu) VALUES(@m,@p,@tt,@s,@g)",
                        ("@m", ma), ("@p", d.MaPhong), ("@tt", d.TinhTrang), ("@s", d.TinhTrang != "Tốt"), ("@g", d.GhiChu));
                tx.Commit();
                return ma;
            }
        }

        /// <summary>Phòng cần tu sửa theo lần kiểm tra gần nhất, làm cơ sở lập kế hoạch tu sửa.</summary>
        public static DataTable DanhSachCanTuSua()
        {
            return Db.Query(
                @"SELECT c.MaPhong, c.TinhTrang, c.GhiChu, k.NgayKT
                  FROM ChiTietKiemTra c JOIN PhieuKiemTra k ON k.MaKT = c.MaKT
                  WHERE c.CanTuSua = 1 AND k.NgayKT = (
                        SELECT MAX(k2.NgayKT) FROM ChiTietKiemTra c2 JOIN PhieuKiemTra k2 ON k2.MaKT = c2.MaKT
                        WHERE c2.MaPhong = c.MaPhong)
                  ORDER BY k.NgayKT, c.MaPhong");
        }

        // ---------- Xử phạt ----------
        public static DataTable LayPhat()
        {
            return Db.Query(
                @"SELECT MaPhat, SoHD, MaPhong, NgayLap, LyDo, SoTienPhat, DaNop, NgayNop,
                         CASE WHEN DaNop = 1 THEN N'Đã nộp' ELSE N'Chưa nộp' END AS TinhTrang
                  FROM PhieuPhat ORDER BY NgayLap DESC, MaPhat DESC");
        }

        public static string LapPhieuPhat(string soHD, string maPhong, string lyDo, decimal soTien)
        {
            if (string.IsNullOrWhiteSpace(soHD)) throw new NghiepVuException("Chưa chọn hợp đồng.");
            if (string.IsNullOrWhiteSpace(maPhong)) throw new NghiepVuException("Chưa chọn phòng vi phạm.");
            if (string.IsNullOrWhiteSpace(lyDo)) throw new NghiepVuException("Phải ghi rõ nội dung vi phạm.");
            if (soTien <= 0) throw new NghiepVuException("Số tiền phạt phải lớn hơn 0.");

            using (var tx = Db.BeginTran())
            {
                string tt = tx.Scalar("SELECT TrangThai FROM HopDong WHERE SoHD=@h", ("@h", soHD)) as string;
                if (tt == null) throw new NghiepVuException("Không tìm thấy hợp đồng.");
                if (tt == TT.HDHuy) throw new NghiepVuException("Hợp đồng đã huỷ.");
                if (Convert.ToInt32(tx.Scalar("SELECT COUNT(*) FROM ChiTietHopDong WHERE SoHD=@h AND MaPhong=@p",
                        ("@h", soHD), ("@p", maPhong))) == 0)
                    throw new NghiepVuException("Phòng không thuộc hợp đồng đã chọn.");

                string prefix = "PF" + DateTime.Today.Year + "-";
                int next = Convert.ToInt32(tx.Scalar(
                    "SELECT ISNULL(MAX(CAST(RIGHT(MaPhat,3) AS INT)),0)+1 FROM PhieuPhat WHERE MaPhat LIKE @p", ("@p", prefix + "%")));
                string ma = prefix + next.ToString("000");
                tx.Exec("INSERT INTO PhieuPhat(MaPhat,SoHD,MaPhong,NgayLap,LyDo,SoTienPhat) VALUES(@m,@h,@p,@n,@ld,@t)",
                    ("@m", ma), ("@h", soHD), ("@p", maPhong), ("@n", DateTime.Today), ("@ld", lyDo.Trim()), ("@t", soTien));
                tx.Commit();
                return ma;
            }
        }

        public static void NopPhat(string maPhat)
        {
            if (string.IsNullOrWhiteSpace(maPhat)) throw new NghiepVuException("Chưa chọn phiếu phạt.");
            int n = Db.Exec("UPDATE PhieuPhat SET DaNop=1, NgayNop=@n WHERE MaPhat=@m AND DaNop=0",
                ("@n", DateTime.Today), ("@m", maPhat));
            if (n == 0) throw new NghiepVuException("Phiếu phạt không tồn tại hoặc đã được nộp.");
        }
    }
}
