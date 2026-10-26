using System;
using System.Windows.Forms;
using QuanLyNhanSu.Services;

namespace QuanLyNhanSu.Forms
{
    public partial class FrmQuyetDinh : Form
    {
        private readonly QuyetDinhService service = new QuyetDinhService();
        private readonly DanhMucService danhMuc = new DanhMucService();
        public FrmQuyetDinh() { InitializeComponent(); }

        private void FrmQuyetDinh_Load(object sender, EventArgs e)
        {
            foreach (ComboBox c in new[] { cboNhanVienLuong, cboNhanVienCV })
            { c.DataSource = danhMuc.LayBang("NhanVien"); c.DisplayMember = "HoTen"; c.ValueMember = "MaNV"; }
            cboChucVu.DataSource = danhMuc.LayBang("ChucVu"); cboChucVu.DisplayMember = "TenCV"; cboChucVu.ValueMember = "MaCV";
            dgvLuong.DataSource = service.LayDanhSach("Luong"); dgvChucVu.DataSource = service.LayDanhSach("ChucVu");
        }

        private void Hien(KetQuaXuLy kq)
        {
            MessageBox.Show(kq.ThongBao, kq.ThanhCong ? "Thông báo" : "Lỗi", MessageBoxButtons.OK, kq.ThanhCong ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            dgvLuong.DataSource = service.LayDanhSach("Luong"); dgvChucVu.DataSource = service.LayDanhSach("ChucVu");
        }

        private void btnLapLuong_Click(object sender, EventArgs e)
        {
            Hien(service.LapQDLuong(new QDLuongDto
            {
                MaNV = Convert.ToString(cboNhanVienLuong.SelectedValue), NgayHieuLuc = dtHieuLuc.Value,
                LuongCoBan = numLuongCB.Value, PhuCapLuong = numPhuCapLuong.Value, PhuCapDocHai = numPhuCapDocHai.Value
            }));
        }

        private void btnLapChucVu_Click(object sender, EventArgs e)
        {
            Hien(service.LapQDChucVu(new QDChucVuDto
            {
                MaNV = Convert.ToString(cboNhanVienCV.SelectedValue), MaCV = Convert.ToString(cboChucVu.SelectedValue),
                NgayHieuLuc = dtHieuLucCV.Value, PhuCapChucVu = numPhuCapCV.Value
            }));
        }

        private void btnTrinhKy_Click(object sender, EventArgs e) { Hien(service.ChuyenTrangThai(SoQDDangChon(), "Chờ duyệt", null)); }
        private void btnGiamDocKy_Click(object sender, EventArgs e) { Hien(service.ChuyenTrangThai(SoQDDangChon(), "Đã ký", "Giám đốc")); }

        private string SoQDDangChon()
        {
            DataGridView g = tabs.SelectedTab == tabLuong ? dgvLuong : dgvChucVu;
            return g.CurrentRow == null ? "" : Convert.ToString(g.CurrentRow.Cells["SoQD"].Value);
        }

        private void btnDong_Click(object sender, EventArgs e) { Close(); }
    }
}
