using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using QuanLyNhanSu.Services;

namespace QuanLyNhanSu.Forms
{
    public partial class FrmChamCong : Form
    {
        private readonly ChamCongService service = new ChamCongService();
        private readonly DanhMucService danhMuc = new DanhMucService();
        private DataTable chiTietPT;
        public FrmChamCong() { InitializeComponent(); }

        private void FrmChamCong_Load(object sender, EventArgs e)
        {
            foreach (ComboBox c in new[] { cboPhongBanKH, cboPhongBanCC, cboPhongBanPT })
            { c.DataSource = danhMuc.LayBang("PhongBan"); c.DisplayMember = "TenPB"; c.ValueMember = "MaPB"; }
            cboNhanVienPT.DataSource = danhMuc.LayBang("NhanVien"); cboNhanVienPT.DisplayMember = "HoTen"; cboNhanVienPT.ValueMember = "MaNV";
            cboHinhThuc.Items.AddRange(new object[] { "Tăng ca", "Ngày lễ", "Ngày nghỉ" }); cboHinhThuc.SelectedIndex = 0;
            chiTietPT = new DataTable();
            chiTietPT.Columns.Add("MaNV", typeof(string)); chiTietPT.Columns.Add("SoGio", typeof(decimal));
            dgvChiTietPT.DataSource = chiTietPT;
            numThang.Value = DateTime.Today.Month; numNam.Value = DateTime.Today.Year;
            dtTuNgay.Value = DateTime.Today; dtDenNgay.Value = DateTime.Today.AddDays(6);
            dgvPhanCong.Columns.Add("colMaNV", "MaNV"); dgvPhanCong.Columns.Add("colNgay", "Ngay (yyyy-MM-dd)"); dgvPhanCong.Columns.Add("colMaCa", "MaCa");
            dgvPhanCong.Columns.Add(new DataGridViewCheckBoxColumn { Name = "colPhuTroi", HeaderText = "CoPhuTroi" });
            dgvChamCong.Columns.Add("ccMaNV", "MaNV"); dgvChamCong.Columns.Add("ccNgay", "Ngay (yyyy-MM-dd)"); dgvChamCong.Columns.Add("ccMaCa", "MaCa");
            dgvChamCong.Columns.Add("ccTrangThai", "TrangThai (Đi làm / Nghỉ phép / Vắng)"); dgvChamCong.Columns.Add("ccSoGio", "SoGioLam");
        }

        private void Hien(KetQuaXuLy kq)
        {
            MessageBox.Show(kq.ThongBao, kq.ThanhCong ? "Thông báo" : "Lỗi", MessageBoxButtons.OK, kq.ThanhCong ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }

        // ---- Tab Phụ trội ----
        private void btnThemNVPT_Click(object sender, EventArgs e)
        {
            string ma = Convert.ToString(cboNhanVienPT.SelectedValue);
            foreach (DataRow r in chiTietPT.Rows) if (Convert.ToString(r["MaNV"]) == ma) { MessageBox.Show("Nhân viên đã có trong phiếu."); return; }
            chiTietPT.Rows.Add(ma, numSoGio.Value);
        }

        private void btnBoNVPT_Click(object sender, EventArgs e)
        {
            DataRowView r = dgvChiTietPT.CurrentRow == null ? null : dgvChiTietPT.CurrentRow.DataBoundItem as DataRowView;
            if (r != null) r.Row.Delete();
        }

        private void btnLuuPhieu_Click(object sender, EventArgs e)
        {
            PhuTroiDto d = new PhuTroiDto
            {
                SoPhieu = txtSoPhieu.Text.Trim(), NgayPhuTroi = dtNgayPhuTroi.Value, HinhThuc = Convert.ToString(cboHinhThuc.SelectedItem),
                MaPB = Convert.ToString(cboPhongBanPT.SelectedValue), NguoiLap = "Ban chấm công"
            };
            foreach (DataRow r in chiTietPT.Rows)
                if (r.RowState != DataRowState.Deleted) d.ChiTiet.Add(new PhuTroiChiTietDto { MaNV = Convert.ToString(r["MaNV"]), SoGio = Convert.ToDecimal(r["SoGio"]) });
            KetQuaXuLy kq = service.LapPhieuPhuTroi(d);
            Hien(kq);
            if (kq.ThanhCong) { chiTietPT.Rows.Clear(); txtSoPhieu.Clear(); }
        }

        // ---- Tab Chấm công ----
        private void btnChotCC_Click(object sender, EventArgs e)
        {
            Hien(service.ChotChamCong((int)numThang.Value, (int)numNam.Value, Convert.ToString(cboPhongBanCC.SelectedValue)));
        }

        private void btnLuuCC_Click(object sender, EventArgs e)
        {
            List<ChamCongDto> ds = new List<ChamCongDto>();
            foreach (DataGridViewRow r in dgvChamCong.Rows)
            {
                if (r.IsNewRow) continue;
                DateTime ngay; decimal gio;
                if (!DateTime.TryParse(Convert.ToString(r.Cells["ccNgay"].Value), out ngay)) { MessageBox.Show("Ngày không hợp lệ ở dòng " + (r.Index + 1)); return; }
                if (!decimal.TryParse(Convert.ToString(r.Cells["ccSoGio"].Value), out gio)) gio = 8;
                ds.Add(new ChamCongDto { MaNV = Convert.ToString(r.Cells["ccMaNV"].Value), Ngay = ngay, MaCa = Convert.ToString(r.Cells["ccMaCa"].Value),
                                         TrangThai = Convert.ToString(r.Cells["ccTrangThai"].Value), SoGioLam = gio });
            }
            Hien(service.LuuChamCong((int)numThang.Value, (int)numNam.Value, Convert.ToString(cboPhongBanCC.SelectedValue), "Ban chấm công", ds));
        }

        private void btnLapKH_Click(object sender, EventArgs e)
        {
            List<PhanCongDto> ds = new List<PhanCongDto>();
            foreach (DataGridViewRow r in dgvPhanCong.Rows)
            {
                if (r.IsNewRow) continue;
                DateTime ngay;
                if (!DateTime.TryParse(Convert.ToString(r.Cells["colNgay"].Value), out ngay)) { MessageBox.Show("Ngày không hợp lệ ở dòng " + (r.Index + 1)); return; }
                ds.Add(new PhanCongDto { MaNV = Convert.ToString(r.Cells["colMaNV"].Value), Ngay = ngay, MaCa = Convert.ToString(r.Cells["colMaCa"].Value),
                                         CoPhuTroi = r.Cells["colPhuTroi"].Value != null && Convert.ToBoolean(r.Cells["colPhuTroi"].Value) });
            }
            Hien(service.LapKeHoachTuan(Convert.ToString(cboPhongBanKH.SelectedValue), dtTuNgay.Value, dtDenNgay.Value, "Trưởng phòng", ds));
        }

        private void btnDong_Click(object sender, EventArgs e) { Close(); }
    }
}
