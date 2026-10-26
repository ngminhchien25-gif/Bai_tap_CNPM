using System;
using System.Data;
using System.Windows.Forms;
using QuanLyNhanSu.Services;

namespace QuanLyNhanSu.Forms
{
    public partial class FrmDanhMuc : Form
    {
        private readonly DanhMucService service = new DanhMucService();
        public FrmDanhMuc() { InitializeComponent(); }

        private void FrmDanhMuc_Load(object sender, EventArgs e)
        {
            cboGioiTinh.Items.AddRange(new object[] { "Nam", "Nữ", "Khác" });
            cboGioiTinh.SelectedIndex = 0;
            TaiDuLieu(); LamMoiNV();
        }

        private void TaiDuLieu()
        {
            dgvPB.DataSource = service.LayBang("PhongBan");
            dgvCV.DataSource = service.LayBang("ChucVu");
            dgvCa.DataSource = service.LayBang("CaLamViec");
            dgvNV.DataSource = service.LayBang("NhanVien");
            cboPhongBan.DataSource = service.LayBang("PhongBan");
            cboPhongBan.DisplayMember = "TenPB"; cboPhongBan.ValueMember = "MaPB";
        }

        private NhanVien LayNV()
        {
            return new NhanVien
            {
                MaNV = txtMaNV.Text.Trim(), HoTen = txtHoTen.Text.Trim(), NgaySinh = dtNgaySinh.Value.Date,
                GioiTinh = Convert.ToString(cboGioiTinh.SelectedItem), CCCD = txtCCCD.Text.Trim(), DiaChi = txtDiaChi.Text.Trim(),
                SDT = txtSDT.Text.Trim(), Email = txtEmail.Text.Trim(),
                MaPB = cboPhongBan.SelectedValue == null ? "" : cboPhongBan.SelectedValue.ToString()
            };
        }

        private void Hien(KetQuaXuLy kq)
        {
            MessageBox.Show(kq.ThongBao, kq.ThanhCong ? "Thông báo" : "Lỗi", MessageBoxButtons.OK, kq.ThanhCong ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (kq.ThanhCong) { TaiDuLieu(); LamMoiNV(); }
        }

        private void btnThemNV_Click(object sender, EventArgs e) { Hien(service.LuuNhanVien(LayNV(), false)); }
        private void btnCapNhatNV_Click(object sender, EventArgs e) { Hien(service.LuuNhanVien(LayNV(), true)); }
        private void btnXoaNV_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Xóa nhân viên đang chọn?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                Hien(service.Xoa("NhanVien", txtMaNV.Text.Trim()));
        }
        private void btnLamMoiNV_Click(object sender, EventArgs e) { LamMoiNV(); }

        private void LamMoiNV()
        {
            txtMaNV.Clear(); txtHoTen.Clear(); txtCCCD.Clear(); txtDiaChi.Clear(); txtSDT.Clear(); txtEmail.Clear();
            dtNgaySinh.Value = DateTime.Today.AddYears(-25);
            txtMaNV.ReadOnly = false; btnThemNV.Enabled = true; btnCapNhatNV.Enabled = false; btnXoaNV.Enabled = false;
        }

        private void dgvNV_SelectionChanged(object sender, EventArgs e)
        {
            DataRowView r = dgvNV.CurrentRow == null ? null : dgvNV.CurrentRow.DataBoundItem as DataRowView;
            if (r == null) return;
            txtMaNV.Text = Convert.ToString(r["MaNV"]); txtHoTen.Text = Convert.ToString(r["HoTen"]);
            if (r["NgaySinh"] != DBNull.Value) dtNgaySinh.Value = Convert.ToDateTime(r["NgaySinh"]);
            cboGioiTinh.SelectedItem = Convert.ToString(r["GioiTinh"]); txtCCCD.Text = Convert.ToString(r["CCCD"]);
            txtDiaChi.Text = Convert.ToString(r["DiaChi"]); txtSDT.Text = Convert.ToString(r["SDT"]); txtEmail.Text = Convert.ToString(r["Email"]);
            cboPhongBan.SelectedValue = Convert.ToString(r["MaPB"]);
            txtMaNV.ReadOnly = true; btnThemNV.Enabled = false; btnCapNhatNV.Enabled = true; btnXoaNV.Enabled = true;   // không sửa khóa chính
        }

        // ---- Tab Phòng ban ----
        private void btnThemPB_Click(object sender, EventArgs e) { Hien(service.LuuPhongBan(txtMaPB.Text, txtTenPB.Text, txtMoTaPB.Text, false)); }
        private void btnCapNhatPB_Click(object sender, EventArgs e) { Hien(service.LuuPhongBan(txtMaPB.Text, txtTenPB.Text, txtMoTaPB.Text, true)); }
        private void btnXoaPB_Click(object sender, EventArgs e) { Hien(service.Xoa("PhongBan", txtMaPB.Text.Trim())); }
        private void dgvPB_SelectionChanged(object sender, EventArgs e)
        {
            DataRowView r = dgvPB.CurrentRow == null ? null : dgvPB.CurrentRow.DataBoundItem as DataRowView;
            if (r == null) return;
            txtMaPB.Text = Convert.ToString(r["MaPB"]); txtTenPB.Text = Convert.ToString(r["TenPB"]); txtMoTaPB.Text = Convert.ToString(r["MoTa"]);
        }

        // ---- Tab Chức vụ ----
        private void btnThemCV_Click(object sender, EventArgs e) { Hien(service.LuuChucVu(txtMaCV.Text, txtTenCV.Text, numPhuCapCV.Value, false)); }
        private void btnCapNhatCV_Click(object sender, EventArgs e) { Hien(service.LuuChucVu(txtMaCV.Text, txtTenCV.Text, numPhuCapCV.Value, true)); }
        private void btnXoaCV_Click(object sender, EventArgs e) { Hien(service.Xoa("ChucVu", txtMaCV.Text.Trim())); }
        private void dgvCV_SelectionChanged(object sender, EventArgs e)
        {
            DataRowView r = dgvCV.CurrentRow == null ? null : dgvCV.CurrentRow.DataBoundItem as DataRowView;
            if (r == null) return;
            txtMaCV.Text = Convert.ToString(r["MaCV"]); txtTenCV.Text = Convert.ToString(r["TenCV"]); numPhuCapCV.Value = Convert.ToDecimal(r["PhuCapMacDinh"]);
        }

        // ---- Tab Ca làm việc ----
        private void btnThemCa_Click(object sender, EventArgs e) { Hien(service.LuuCa(txtMaCa.Text, txtTenCa.Text, dtGioBD.Value.TimeOfDay, dtGioKT.Value.TimeOfDay, false)); }
        private void btnCapNhatCa_Click(object sender, EventArgs e) { Hien(service.LuuCa(txtMaCa.Text, txtTenCa.Text, dtGioBD.Value.TimeOfDay, dtGioKT.Value.TimeOfDay, true)); }
        private void btnXoaCa_Click(object sender, EventArgs e) { Hien(service.Xoa("CaLamViec", txtMaCa.Text.Trim())); }
        private void dgvCa_SelectionChanged(object sender, EventArgs e)
        {
            DataRowView r = dgvCa.CurrentRow == null ? null : dgvCa.CurrentRow.DataBoundItem as DataRowView;
            if (r == null) return;
            txtMaCa.Text = Convert.ToString(r["MaCa"]); txtTenCa.Text = Convert.ToString(r["TenCa"]);
            dtGioBD.Value = DateTime.Today + (TimeSpan)r["GioBatDau"]; dtGioKT.Value = DateTime.Today + (TimeSpan)r["GioKetThuc"];
        }

        private void btnDong_Click(object sender, EventArgs e) { Close(); }
    }
}
