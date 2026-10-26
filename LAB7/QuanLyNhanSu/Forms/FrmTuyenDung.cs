using System;
using System.Windows.Forms;
using QuanLyNhanSu.Services;

namespace QuanLyNhanSu.Forms
{
    public partial class FrmTuyenDung : Form
    {
        private readonly TuyenDungService tuyenDung = new TuyenDungService();
        private readonly QuyetDinhService quyetDinh = new QuyetDinhService();
        private readonly DanhMucService danhMuc = new DanhMucService();
        public FrmTuyenDung() { InitializeComponent(); }

        private void FrmTuyenDung_Load(object sender, EventArgs e)
        {
            cboNhanVien.DataSource = danhMuc.LayBang("NhanVien"); cboNhanVien.DisplayMember = "HoTen"; cboNhanVien.ValueMember = "MaNV";
            cboPhongBan.DataSource = danhMuc.LayBang("PhongBan"); cboPhongBan.DisplayMember = "TenPB"; cboPhongBan.ValueMember = "MaPB";
            cboNhanVienHD.DataSource = danhMuc.LayBang("NhanVien"); cboNhanVienHD.DisplayMember = "HoTen"; cboNhanVienHD.ValueMember = "MaNV";
            rdoDaiHan.Checked = true; dtBatDauTV.Value = DateTime.Today; dtKetThucTV.Value = DateTime.Today.AddMonths(2);
            dgvQD.DataSource = quyetDinh.LayDanhSach("TuyenDung");
        }

        private void Hien(KetQuaXuLy kq)
        {
            MessageBox.Show(kq.ThongBao, kq.ThanhCong ? "Thông báo" : "Lỗi", MessageBoxButtons.OK, kq.ThanhCong ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            dgvQD.DataSource = quyetDinh.LayDanhSach("TuyenDung");
        }

        private void btnLuuNhap_Click(object sender, EventArgs e)
        {
            Hien(tuyenDung.LapQDTuyenDung(new TuyenDungDto
            {
                MaNV = Convert.ToString(cboNhanVien.SelectedValue), MaPB = Convert.ToString(cboPhongBan.SelectedValue),
                NgayBatDauTV = dtBatDauTV.Value, NgayKetThucTV = dtKetThucTV.Value, LuongThuViec = numLuongThuViec.Value,
                DieuKhoanBenA = txtDieuKhoanA.Text.Trim(), DieuKhoanBenB = txtDieuKhoanB.Text.Trim()
            }));
        }

        private string SoQDDangChon()
        {
            return dgvQD.CurrentRow == null ? "" : Convert.ToString(dgvQD.CurrentRow.Cells["SoQD"].Value);
        }

        private void btnTrinhKy_Click(object sender, EventArgs e) { Hien(quyetDinh.ChuyenTrangThai(SoQDDangChon(), "Chờ duyệt", null)); }
        private void btnGiamDocKy_Click(object sender, EventArgs e) { Hien(quyetDinh.ChuyenTrangThai(SoQDDangChon(), "Đã ký", "Giám đốc")); }

        private void btnKyHD_Click(object sender, EventArgs e)
        {
            Hien(tuyenDung.KyHopDong(new HopDongDto
            {
                MaNV = Convert.ToString(cboNhanVienHD.SelectedValue), LoaiHD = rdoDaiHan.Checked ? "DaiHan" : "NganHan",
                NgayBatDau = dtBatDauHD.Value, NgayKetThuc = rdoNganHan.Checked ? (DateTime?)dtKetThucHD.Value : null,
                GiamDocDaKy = chkGiamDocKy.Checked
            }));
        }

        private void btnLapBH_Click(object sender, EventArgs e)
        {
            Hien(tuyenDung.LapHoSoBaoHiem(txtSoHD.Text.Trim(), txtSoBHYT.Text.Trim(), txtSoBHXH.Text.Trim(), dtNgayThamGia.Value));
        }

        private void btnDong_Click(object sender, EventArgs e) { Close(); }
    }
}
