using System;

namespace QuanLyCaoOc
{
    /// <summary>Một dòng hóa đơn dịch vụ: phòng - dịch vụ - số lượng sử dụng trong tháng.</summary>
    public class DongDV
    {
        public string MaPhong { get; private set; }
        public string MaDV { get; private set; }
        public decimal SoLuong { get; private set; }
        public DongDV(string maPhong, string maDV, decimal soLuong) { MaPhong = maPhong; MaDV = maDV; SoLuong = soLuong; }
    }

    /// <summary>Một dòng phiếu kiểm tra phòng.</summary>
    public class DongKT
    {
        public string MaPhong { get; private set; }
        public string TinhTrang { get; private set; }
        public string GhiChu { get; private set; }
        public DongKT(string maPhong, string tinhTrang, string ghiChu) { MaPhong = maPhong; TinhTrang = tinhTrang; GhiChu = ghiChu; }
    }

    /// <summary>Kết quả kiểm tra điều kiện gia hạn của một hợp đồng.</summary>
    public class DieuKienGiaHan
    {
        public bool Duoc { get; private set; }
        public string LyDo { get; private set; }
        public int SoThang { get; private set; }
        public DateTime NgayKetThuc { get; private set; }
        public decimal NoThue { get; private set; }
        public decimal NoDichVu { get; private set; }
        /// <summary>Mỗi lần gia hạn tối đa 1/2 thời gian thuê (tính theo số tháng nguyên).</summary>
        public int ToiDaThang { get { return SoThang / 2; } }

        public DieuKienGiaHan(bool duoc, string lyDo, int soThang, DateTime ngayKetThuc, decimal noThue, decimal noDichVu)
        {
            Duoc = duoc; LyDo = lyDo; SoThang = soThang; NgayKetThuc = ngayKetThuc; NoThue = noThue; NoDichVu = noDichVu;
        }
    }
}
