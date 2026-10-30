using System;
using System.Data;
using QuanLyCaoOc.Data;

namespace QuanLyCaoOc.Services
{
    /// <summary>Nhân viên làm việc tại các văn phòng (công ty báo cáo cho cao ốc) và thẻ giữ xe.</summary>
    public static class NhanVienService
    {
        public static DataTable LayNhanVien()
        {
            return Db.Query(
                @"SELECT n.MaNV, n.HoTen, n.NgaySinh, n.GioiTinh, n.ChucVu, k.TenCongTy, n.MaPhong, t.DoCao AS Tang,
                         n.NgayBaoCao, n.TrangThai, n.HinhAnh
                  FROM NhanVien n
                  JOIN KhachHang k ON k.MaKH = n.MaKH
                  JOIN Phong p ON p.MaPhong = n.MaPhong
                  JOIN Tang t ON t.MaTang = p.MaTang
                  ORDER BY n.MaNV");
        }

        public static DataTable NhanVienDangLam()
        {
            return Db.Query(
                @"SELECT n.MaNV, n.MaNV + N' - ' + n.HoTen + N' (' + k.TenCongTy + N')' AS HienThi
                  FROM NhanVien n JOIN KhachHang k ON k.MaKH = n.MaKH WHERE n.TrangThai = N'Đang làm' ORDER BY n.MaNV");
        }

        /// <summary>Các phòng công ty đang thuê (hợp đồng còn hiệu lực).</summary>
        public static DataTable PhongDangThueCua(string maKH)
        {
            return Db.Query(
                @"SELECT DISTINCT c.MaPhong
                  FROM ChiTietHopDong c JOIN HopDong h ON h.SoHD = c.SoHD
                  WHERE h.MaKH=@k AND h.TrangThai=@hl AND c.NgayKetThuc >= @t ORDER BY c.MaPhong",
                ("@k", maKH), ("@hl", TT.HDHieuLuc), ("@t", DateTime.Today));
        }

        public static string BaoCaoNhanVienMoi(string hoTen, DateTime ngaySinh, string gioiTinh, string chucVu,
                                               string maKH, string maPhong, string hinhAnh)
        {
            if (string.IsNullOrWhiteSpace(hoTen)) throw new NghiepVuException("Họ tên nhân viên không được để trống.");
            if (string.IsNullOrWhiteSpace(chucVu)) throw new NghiepVuException("Chức vụ không được để trống.");
            if (string.IsNullOrWhiteSpace(maKH)) throw new NghiepVuException("Chưa chọn công ty.");
            if (string.IsNullOrWhiteSpace(maPhong)) throw new NghiepVuException("Chưa chọn phòng làm việc.");
            if (ngaySinh.Date >= DateTime.Today) throw new NghiepVuException("Ngày sinh không hợp lệ.");

            using (var tx = Db.BeginTran())
            {
                int hopLe = Convert.ToInt32(tx.Scalar(
                    @"SELECT COUNT(*) FROM ChiTietHopDong c JOIN HopDong h ON h.SoHD=c.SoHD
                      WHERE h.MaKH=@k AND c.MaPhong=@p AND h.TrangThai=@hl AND c.NgayKetThuc >= @t",
                    ("@k", maKH), ("@p", maPhong), ("@hl", TT.HDHieuLuc), ("@t", DateTime.Today)));
                if (hopLe == 0)
                    throw new NghiepVuException("Phòng này không thuộc hợp đồng đang hiệu lực của công ty đã chọn.");

                int next = Convert.ToInt32(tx.Scalar("SELECT ISNULL(MAX(CAST(SUBSTRING(MaNV,3,10) AS INT)),0)+1 FROM NhanVien"));
                string ma = "NV" + next.ToString("0000");
                tx.Exec(@"INSERT INTO NhanVien(MaNV,HoTen,NgaySinh,GioiTinh,HinhAnh,ChucVu,MaKH,MaPhong,NgayBaoCao,TrangThai)
                          VALUES(@m,@ht,@ns,@gt,@ha,@cv,@kh,@p,@nb,N'Đang làm')",
                    ("@m", ma), ("@ht", hoTen.Trim()), ("@ns", ngaySinh.Date), ("@gt", gioiTinh), ("@ha", hinhAnh),
                    ("@cv", chucVu.Trim()), ("@kh", maKH), ("@p", maPhong), ("@nb", DateTime.Today));
                tx.Commit();
                return ma;
            }
        }

        public static void NghiViec(string maNV)
        {
            if (string.IsNullOrWhiteSpace(maNV)) throw new NghiepVuException("Chưa chọn nhân viên.");
            using (var tx = Db.BeginTran())
            {
                int n = tx.Exec("UPDATE NhanVien SET TrangThai=N'Đã nghỉ' WHERE MaNV=@m AND TrangThai=N'Đang làm'", ("@m", maNV));
                if (n == 0) throw new NghiepVuException("Nhân viên không tồn tại hoặc đã nghỉ.");
                tx.Exec("UPDATE TheGiuXe SET TrangThai=N'Đã huỷ' WHERE MaNV=@m AND TrangThai=N'Đang dùng'", ("@m", maNV));
                tx.Commit();
            }
        }

        // ---------- Thẻ giữ xe ----------
        public static DataTable LayThe()
        {
            return Db.Query(
                @"SELECT x.MaThe, x.MaNV, n.HoTen, x.LoaiXe, x.BienSo, x.HangXe, x.MauSac, x.NgayCap, x.TrangThai
                  FROM TheGiuXe x JOIN NhanVien n ON n.MaNV = x.MaNV ORDER BY x.MaThe");
        }

        public static string CapThe(string maNV, string loaiXe, string bienSo, string hangXe, string mauSac)
        {
            if (string.IsNullOrWhiteSpace(maNV)) throw new NghiepVuException("Chưa chọn nhân viên gửi xe.");
            if (string.IsNullOrWhiteSpace(loaiXe)) throw new NghiepVuException("Chưa chọn loại xe.");
            if (string.IsNullOrWhiteSpace(bienSo)) throw new NghiepVuException("Biển số xe không được để trống.");
            bienSo = bienSo.Trim().ToUpperInvariant();

            using (var tx = Db.BeginTran())
            {
                string tt = tx.Scalar("SELECT TrangThai FROM NhanVien WHERE MaNV=@m", ("@m", maNV)) as string;
                if (tt != "Đang làm") throw new NghiepVuException("Chỉ cấp thẻ cho nhân viên đang làm việc tại cao ốc.");
                if (Convert.ToInt32(tx.Scalar("SELECT COUNT(*) FROM TheGiuXe WHERE BienSo=@b AND TrangThai=N'Đang dùng'", ("@b", bienSo))) > 0)
                    throw new NghiepVuException("Biển số này đã có thẻ giữ xe đang sử dụng.");

                int next = Convert.ToInt32(tx.Scalar("SELECT ISNULL(MAX(CAST(SUBSTRING(MaThe,3,10) AS INT)),0)+1 FROM TheGiuXe"));
                string ma = "TX" + next.ToString("000");
                tx.Exec(@"INSERT INTO TheGiuXe(MaThe,MaNV,LoaiXe,BienSo,HangXe,MauSac,NgayCap,TrangThai)
                          VALUES(@m,@nv,@l,@b,@h,@c,@n,N'Đang dùng')",
                    ("@m", ma), ("@nv", maNV), ("@l", loaiXe), ("@b", bienSo), ("@h", hangXe), ("@c", mauSac), ("@n", DateTime.Today));
                tx.Commit();
                return ma;
            }
        }

        public static void HuyThe(string maThe)
        {
            if (string.IsNullOrWhiteSpace(maThe)) throw new NghiepVuException("Chưa chọn thẻ giữ xe.");
            int n = Db.Exec("UPDATE TheGiuXe SET TrangThai=N'Đã huỷ' WHERE MaThe=@m AND TrangThai=N'Đang dùng'", ("@m", maThe));
            if (n == 0) throw new NghiepVuException("Thẻ không tồn tại hoặc đã huỷ.");
        }
    }
}
