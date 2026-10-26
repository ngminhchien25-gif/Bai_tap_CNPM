using System;
using System.Collections.Generic;

namespace QuanLyNhanSu
{
    public class KetQuaXuLy
    {
        public bool ThanhCong { get; private set; }
        public string ThongBao { get; private set; }
        public object DuLieu { get; private set; }
        public static KetQuaXuLy Ok(string tb, object dl = null) { return new KetQuaXuLy { ThanhCong = true, ThongBao = tb, DuLieu = dl }; }
        public static KetQuaXuLy Loi(string tb) { return new KetQuaXuLy { ThanhCong = false, ThongBao = tb }; }
    }

    public class NhanVien
    {
        public string MaNV { get; set; }
        public string HoTen { get; set; }
        public DateTime NgaySinh { get; set; }
        public string GioiTinh { get; set; }
        public string CCCD { get; set; }
        public string DiaChi { get; set; }
        public string SDT { get; set; }
        public string Email { get; set; }
        public string MaPB { get; set; }
    }

    public class TuyenDungDto
    {
        public string MaNV { get; set; }
        public string MaPB { get; set; }
        public DateTime NgayBatDauTV { get; set; }
        public DateTime NgayKetThucTV { get; set; }
        public decimal LuongThuViec { get; set; }
        public string DieuKhoanBenA { get; set; }
        public string DieuKhoanBenB { get; set; }
    }

    public class HopDongDto
    {
        public string MaNV { get; set; }
        public string LoaiHD { get; set; }          // DaiHan | NganHan
        public DateTime NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public bool GiamDocDaKy { get; set; }
    }

    public class QDLuongDto
    {
        public string MaNV { get; set; }
        public DateTime NgayHieuLuc { get; set; }
        public decimal LuongCoBan { get; set; }
        public decimal PhuCapLuong { get; set; }
        public decimal PhuCapDocHai { get; set; }
    }

    public class QDChucVuDto
    {
        public string MaNV { get; set; }
        public string MaCV { get; set; }
        public DateTime NgayHieuLuc { get; set; }
        public decimal PhuCapChucVu { get; set; }
    }

    public class PhanCongDto { public string MaNV; public DateTime Ngay; public string MaCa; public bool CoPhuTroi; }
    public class ChamCongDto { public string MaNV; public DateTime Ngay; public string MaCa; public string TrangThai; public decimal SoGioLam; }
    public class PhuTroiChiTietDto { public string MaNV; public decimal SoGio; }

    public class PhuTroiDto
    {
        public string SoPhieu { get; set; }
        public DateTime NgayPhuTroi { get; set; }
        public string HinhThuc { get; set; }        // Tăng ca | Ngày lễ | Ngày nghỉ
        public string MaPB { get; set; }
        public string NguoiLap { get; set; }
        public List<PhuTroiChiTietDto> ChiTiet = new List<PhuTroiChiTietDto>();
    }

    public class DoiTuongDto { public string MaNV; public string MaPB; }

    public class KhenThuongDto
    {
        public string LoaiDoiTuong { get; set; }    // CaNhan | TapThe
        public string LoaiKhenThuong { get; set; }  // Thường | Lễ | Cuối năm | Lương tháng 13
        public string HinhThuc { get; set; }
        public decimal SoTien { get; set; }
        public string LyDo { get; set; }
        public DateTime NgayHieuLuc { get; set; }
        public List<DoiTuongDto> DoiTuong = new List<DoiTuongDto>();
    }

    public class KyLuatDto
    {
        public string LoaiDoiTuong { get; set; }
        public string HinhThuc { get; set; }
        public decimal SoTienPhat { get; set; }
        public string CachThu { get; set; }         // Nộp phạt | Trừ lương
        public string LyDo { get; set; }
        public DateTime NgayHieuLuc { get; set; }
        public List<DoiTuongDto> DoiTuong = new List<DoiTuongDto>();
    }
}
