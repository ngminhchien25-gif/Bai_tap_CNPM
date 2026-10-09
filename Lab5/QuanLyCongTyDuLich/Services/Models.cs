using System;

namespace QuanLyCongTyDuLich.Services
{
    public class KetQuaXuLy
    {
        public bool ThanhCong { get; private set; }
        public string ThongBao { get; private set; }

        public static KetQuaXuLy Ok(string thongBao) { return new KetQuaXuLy { ThanhCong = true, ThongBao = thongBao }; }
        public static KetQuaXuLy Fail(string thongBao) { return new KetQuaXuLy { ThanhCong = false, ThongBao = thongBao }; }
    }

    public class ThanhVienDoanItem
    {
        public string HoTen { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string SoGiayTo { get; set; }
    }

    public static class QuyDinh
    {
        public const int MocKhachDoan = 12; // Đoàn: > 12 người; Lẻ: < 12 người[cite: 3, 4, 47]
        public const string Le = "LE";
        public const string Doan = "DOAN";
        public const string MoDangKy = "Mở đăng ký";
        public const string DongDangKy = "Đóng đăng ký";
        public const string DaDangKy = "Đã đăng ký";
        public const string HuyMatCoc = "Hủy - mất cọc";
        public const string HoanTatThanhToan = "Đã hoàn tất thanh toán";
    }
}