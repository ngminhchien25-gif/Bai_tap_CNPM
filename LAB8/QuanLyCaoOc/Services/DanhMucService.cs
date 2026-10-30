using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using QuanLyCaoOc.Data;

namespace QuanLyCaoOc.Services
{
    /// <summary>Danh mục: tầng, phòng, khách hàng, dịch vụ, nhân viên quản lý.</summary>
    public static class DanhMucService
    {
        private static readonly HashSet<string> BangChoPhepXoa =
            new HashSet<string> { "Tang", "Phong", "KhachHang", "DichVu", "NhanVienQL" };

        // ---------- Tiện ích dùng chung ----------
        private static void Upsert(string table, string key, Dictionary<string, object> v)
        {
            var cols = v.Keys.ToList();
            string upd = string.Join(", ", cols.Where(c => c != key).Select(c => c + "=@" + c));
            string ins = "INSERT INTO " + table + "(" + string.Join(",", cols) + ") VALUES(" +
                         string.Join(",", cols.Select(c => "@" + c)) + ")";
            string sql = "IF EXISTS (SELECT 1 FROM " + table + " WHERE " + key + "=@" + key + ") UPDATE " + table +
                         " SET " + upd + " WHERE " + key + "=@" + key + " ELSE " + ins;
            Db.Exec(sql, v.Select(kv => ("@" + kv.Key, kv.Value)).ToArray());
        }

        private static void Bat(string giaTri, string ten)
        {
            if (string.IsNullOrWhiteSpace(giaTri)) throw new NghiepVuException(ten + " không được để trống.");
        }

        public static void Xoa(string bang, string khoa, string giaTri)
        {
            if (!BangChoPhepXoa.Contains(bang)) throw new NghiepVuException("Không được phép xóa dữ liệu bảng này.");
            Bat(giaTri, "Mã cần xóa");
            Db.Exec("DELETE FROM " + bang + " WHERE " + khoa + "=@k", ("@k", giaTri));
        }

        // ---------- Tầng ----------
        public static DataTable LayTang()
        {
            return Db.Query("SELECT MaTang, TenTang, DoCao, DonGiaM2 FROM Tang ORDER BY DoCao");
        }

        public static void LuuTang(string ma, string ten, int doCao, decimal donGia)
        {
            Bat(ma, "Mã tầng"); Bat(ten, "Tên tầng");
            if (doCao <= 0) throw new NghiepVuException("Độ cao (số tầng) phải lớn hơn 0.");
            if (donGia <= 0) throw new NghiepVuException("Đơn giá /m² phải lớn hơn 0.");
            Upsert("Tang", "MaTang", new Dictionary<string, object>
                { { "MaTang", ma }, { "TenTang", ten }, { "DoCao", doCao }, { "DonGiaM2", donGia } });
            // Đổi đơn giá chỉ áp dụng cho phòng trống; hợp đồng đang chạy giữ giá đã chốt trong ChiTietHopDong.
            Db.Exec("UPDATE Phong SET GiaThue = DienTich * @g WHERE MaTang=@t AND TrangThai=@tt",
                ("@g", donGia), ("@t", ma), ("@tt", TT.PhongTrong));
        }

        // ---------- Phòng ----------
        public static DataTable LayPhong()
        {
            return Db.Query(
                @"SELECT p.MaPhong, p.MaTang, t.DoCao AS Tang, p.DienTich, p.Huong, p.GiaThue, p.TrangThai
                  FROM Phong p JOIN Tang t ON t.MaTang = p.MaTang ORDER BY t.DoCao, p.MaPhong");
        }

        public static void LuuPhong(string ma, string maTang, decimal dienTich, string huong, string trangThai)
        {
            Bat(ma, "Mã phòng"); Bat(maTang, "Tầng"); Bat(huong, "Hướng");
            if (dienTich <= 0) throw new NghiepVuException("Diện tích phải lớn hơn 0.");
            if (trangThai != TT.PhongTrong && trangThai != TT.PhongBaoTri && trangThai != TT.PhongDangThue)
                throw new NghiepVuException("Trạng thái phòng không hợp lệ.");

            string cu = Db.Scalar("SELECT TrangThai FROM Phong WHERE MaPhong=@p", ("@p", ma)) as string;
            if (cu == TT.PhongDangThue && trangThai != TT.PhongDangThue)
                throw new NghiepVuException("Phòng đang được thuê, không thể đổi trạng thái thủ công.");
            if (cu != TT.PhongDangThue && trangThai == TT.PhongDangThue)
                throw new NghiepVuException("Trạng thái 'Đang thuê' chỉ được đặt khi lập hợp đồng.");

            object dg = Db.Scalar("SELECT DonGiaM2 FROM Tang WHERE MaTang=@t", ("@t", maTang));
            if (dg == null) throw new NghiepVuException("Tầng không tồn tại.");
            decimal gia = Math.Round(dienTich * Convert.ToDecimal(dg), 0);   // giá ấn định theo độ cao và diện tích

            Upsert("Phong", "MaPhong", new Dictionary<string, object>
            {
                { "MaPhong", ma }, { "MaTang", maTang }, { "DienTich", dienTich }, { "Huong", huong },
                { "GiaThue", gia }, { "TrangThai", trangThai }
            });
        }

        /// <summary>Khách tham khảo vị trí, diện tích, giá trước khi thuê.</summary>
        public static DataTable TraCuuPhongTrong(decimal dienTichToiThieu, decimal giaToiDa, string maTang)
        {
            return Db.Query(
                @"SELECT p.MaPhong, p.TenTang, p.DoCao AS Tang, p.DienTich, p.Huong, p.GiaThue
                  FROM vw_PhongTrong p
                  WHERE p.DienTich >= @dt AND (@gia = 0 OR p.GiaThue <= @gia) AND (@tang IS NULL OR p.MaTang = @tang)
                  ORDER BY p.DoCao, p.MaPhong",
                ("@dt", dienTichToiThieu), ("@gia", giaToiDa), ("@tang", maTang));
        }

        // ---------- Khách hàng ----------
        public static DataTable LayKhachHang()
        {
            return Db.Query("SELECT MaKH, TenCongTy, MaSoThue, NguoiDaiDien, DiaChi, SDT, Email FROM KhachHang ORDER BY MaKH");
        }

        public static void LuuKhachHang(string ma, string ten, string mst, string daiDien, string diaChi, string sdt, string email)
        {
            Bat(ma, "Mã khách hàng"); Bat(ten, "Tên công ty"); Bat(mst, "Mã số thuế"); Bat(daiDien, "Người đại diện");
            Upsert("KhachHang", "MaKH", new Dictionary<string, object>
            {
                { "MaKH", ma }, { "TenCongTy", ten }, { "MaSoThue", mst }, { "NguoiDaiDien", daiDien },
                { "DiaChi", diaChi }, { "SDT", sdt }, { "Email", email }
            });
        }

        // ---------- Dịch vụ ----------
        public static DataTable LayDichVu()
        {
            return Db.Query("SELECT MaDV, TenDV, DonViTinh, DonGia FROM DichVu ORDER BY MaDV");
        }

        public static void LuuDichVu(string ma, string ten, string dvt, decimal donGia)
        {
            Bat(ma, "Mã dịch vụ"); Bat(ten, "Tên dịch vụ"); Bat(dvt, "Đơn vị tính");
            if (donGia < 0) throw new NghiepVuException("Đơn giá không được âm.");
            Upsert("DichVu", "MaDV", new Dictionary<string, object>
                { { "MaDV", ma }, { "TenDV", ten }, { "DonViTinh", dvt }, { "DonGia", donGia } });
        }

        // ---------- Nhân viên quản lý ----------
        public static DataTable LayNVQL()
        {
            return Db.Query("SELECT MaNVQL, HoTen, ChucVu, SDT FROM NhanVienQL ORDER BY MaNVQL");
        }

        public static void LuuNVQL(string ma, string hoTen, string chucVu, string sdt)
        {
            Bat(ma, "Mã nhân viên"); Bat(hoTen, "Họ tên"); Bat(chucVu, "Chức vụ");
            Upsert("NhanVienQL", "MaNVQL", new Dictionary<string, object>
                { { "MaNVQL", ma }, { "HoTen", hoTen }, { "ChucVu", chucVu }, { "SDT", sdt } });
        }
    }
}
