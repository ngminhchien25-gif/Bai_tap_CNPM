using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using QuanLyNhanSu.Data;

namespace QuanLyNhanSu.Services
{
    public class DanhMucService
    {
        private static readonly Dictionary<string, string> Khoa = new Dictionary<string, string>
        { {"PhongBan","MaPB"}, {"ChucVu","MaCV"}, {"CaLamViec","MaCa"}, {"NhanVien","MaNV"} };

        public DataTable LayBang(string bang)
        {
            if (!Khoa.ContainsKey(bang)) throw new ArgumentException("Bảng không hợp lệ.");
            return Db.Query("SELECT * FROM " + bang + " ORDER BY " + Khoa[bang]);
        }

        public KetQuaXuLy LuuNhanVien(NhanVien nv, bool capNhat)
        {
            if (nv == null || string.IsNullOrWhiteSpace(nv.MaNV) || string.IsNullOrWhiteSpace(nv.HoTen) ||
                string.IsNullOrWhiteSpace(nv.CCCD) || string.IsNullOrWhiteSpace(nv.MaPB))
                return KetQuaXuLy.Loi("Vui lòng nhập mã, họ tên, CCCD và phòng ban.");
            if (nv.CCCD.Trim().Length != 12) return KetQuaXuLy.Loi("CCCD phải gồm 12 chữ số.");
            try
            {
                string sql = capNhat
                    ? @"UPDATE NhanVien SET HoTen=@ten,NgaySinh=@ns,GioiTinh=@gt,CCCD=@cccd,DiaChi=@dc,SDT=@sdt,Email=@em,MaPB=@pb WHERE MaNV=@ma"
                    : @"INSERT NhanVien(MaNV,HoTen,NgaySinh,GioiTinh,CCCD,DiaChi,SDT,Email,MaPB) VALUES(@ma,@ten,@ns,@gt,@cccd,@dc,@sdt,@em,@pb)";
                int n = Db.Execute(sql, Db.P("@ma", nv.MaNV.Trim()), Db.P("@ten", nv.HoTen.Trim()), Db.P("@ns", nv.NgaySinh.Date),
                    Db.P("@gt", nv.GioiTinh), Db.P("@cccd", nv.CCCD.Trim()), Db.P("@dc", nv.DiaChi), Db.P("@sdt", nv.SDT),
                    Db.P("@em", nv.Email), Db.P("@pb", nv.MaPB));
                return n > 0 ? KetQuaXuLy.Ok(capNhat ? "Đã cập nhật nhân viên." : "Đã thêm nhân viên.") : KetQuaXuLy.Loi("Không có dữ liệu thay đổi.");
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601) return KetQuaXuLy.Loi("Mã nhân viên hoặc CCCD đã tồn tại.");
                return KetQuaXuLy.Loi("Lỗi cơ sở dữ liệu: " + ex.Message);
            }
        }

        public KetQuaXuLy LuuPhongBan(string ma, string ten, string moTa, bool capNhat)
        {
            if (string.IsNullOrWhiteSpace(ma) || string.IsNullOrWhiteSpace(ten)) return KetQuaXuLy.Loi("Mã và tên phòng ban không được trống.");
            try
            {
                string sql = capNhat ? "UPDATE PhongBan SET TenPB=@t,MoTa=@m WHERE MaPB=@ma" : "INSERT PhongBan(MaPB,TenPB,MoTa) VALUES(@ma,@t,@m)";
                Db.Execute(sql, Db.P("@ma", ma.Trim()), Db.P("@t", ten.Trim()), Db.P("@m", moTa));
                return KetQuaXuLy.Ok("Đã lưu phòng ban.");
            }
            catch (SqlException ex) { return KetQuaXuLy.Loi(ex.Number == 2627 ? "Mã phòng ban đã tồn tại." : "Lỗi cơ sở dữ liệu: " + ex.Message); }
        }

        public KetQuaXuLy LuuChucVu(string ma, string ten, decimal phuCap, bool capNhat)
        {
            if (string.IsNullOrWhiteSpace(ma) || string.IsNullOrWhiteSpace(ten)) return KetQuaXuLy.Loi("Mã và tên chức vụ không được trống.");
            if (phuCap < 0) return KetQuaXuLy.Loi("Phụ cấp không được âm.");
            try
            {
                string sql = capNhat ? "UPDATE ChucVu SET TenCV=@t,PhuCapMacDinh=@p WHERE MaCV=@ma" : "INSERT ChucVu(MaCV,TenCV,PhuCapMacDinh) VALUES(@ma,@t,@p)";
                Db.Execute(sql, Db.P("@ma", ma.Trim()), Db.P("@t", ten.Trim()), Db.P("@p", phuCap));
                return KetQuaXuLy.Ok("Đã lưu chức vụ.");
            }
            catch (SqlException ex) { return KetQuaXuLy.Loi(ex.Number == 2627 ? "Mã chức vụ đã tồn tại." : "Lỗi cơ sở dữ liệu: " + ex.Message); }
        }

        public KetQuaXuLy LuuCa(string ma, string ten, TimeSpan batDau, TimeSpan ketThuc, bool capNhat)
        {
            if (string.IsNullOrWhiteSpace(ma) || string.IsNullOrWhiteSpace(ten)) return KetQuaXuLy.Loi("Mã và tên ca không được trống.");
            try
            {
                string sql = capNhat ? "UPDATE CaLamViec SET TenCa=@t,GioBatDau=@b,GioKetThuc=@k WHERE MaCa=@ma" : "INSERT CaLamViec(MaCa,TenCa,GioBatDau,GioKetThuc) VALUES(@ma,@t,@b,@k)";
                Db.Execute(sql, Db.P("@ma", ma.Trim()), Db.P("@t", ten.Trim()), Db.P("@b", batDau), Db.P("@k", ketThuc));
                return KetQuaXuLy.Ok("Đã lưu ca làm việc.");
            }
            catch (SqlException ex) { return KetQuaXuLy.Loi(ex.Number == 2627 ? "Mã ca đã tồn tại." : "Lỗi cơ sở dữ liệu: " + ex.Message); }
        }

        public KetQuaXuLy Xoa(string bang, string ma)
        {
            if (!Khoa.ContainsKey(bang)) return KetQuaXuLy.Loi("Tham số xóa không hợp lệ.");
            try
            {
                int n = Db.Execute("DELETE FROM " + bang + " WHERE " + Khoa[bang] + "=@ma", Db.P("@ma", ma));
                return n > 0 ? KetQuaXuLy.Ok("Đã xóa.") : KetQuaXuLy.Loi("Không tìm thấy dữ liệu.");
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547) return KetQuaXuLy.Loi("Không thể xóa vì dữ liệu đang được sử dụng ở nghiệp vụ khác.");
                return KetQuaXuLy.Loi("Lỗi cơ sở dữ liệu: " + ex.Message);
            }
        }
    }
}
