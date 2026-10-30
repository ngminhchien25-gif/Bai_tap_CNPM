using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using QuanLyCaoOc.Data;
using QuanLyCaoOc.Services;

namespace QuanLyCaoOc.Forms
{
    /// <summary>Công ty báo cáo nhân viên mới cho bộ phận quản lý; cao ốc cấp thẻ giữ xe cho nhân viên.</summary>
    public class FrmNhanVien : Form
    {
        // ---- Tab nhân viên ----
        private ComboBox cboKhachHang, cboPhong, cboGioiTinh;
        private TextBox txtHoTen, txtChucVu, txtHinhAnh;
        private DateTimePicker dtNgaySinh;
        private PictureBox picHinh;
        private DataGridView dgvNV;

        // ---- Tab thẻ giữ xe ----
        private ComboBox cboNhanVien, cboLoaiXe;
        private TextBox txtBienSo, txtHangXe, txtMauSac;
        private DataGridView dgvThe;

        public FrmNhanVien()
        {
            Text = "Nhân viên công ty thuê và thẻ giữ xe";
            ClientSize = new Size(1180, 660);
            StartPosition = FormStartPosition.CenterScreen;
            Font = Ui.FontChu;

            var tab = new TabControl { Name = "tabMain", Dock = DockStyle.Fill };
            var tabNV = new TabPage("Nhân viên") { Name = "tabNV" };
            var tabThe = new TabPage("Thẻ giữ xe") { Name = "tabThe" };
            tab.TabPages.Add(tabNV);
            tab.TabPages.Add(tabThe);
            Controls.Add(tab);
            DungTabNhanVien(tabNV);
            DungTabThe(tabThe);

            Load += (o, ev) => Ui.Run(TaiTatCa);
        }

        // =================== NHÂN VIÊN ===================
        private void DungTabNhanVien(Control p)
        {
            var gb = new GroupBox { Text = "Báo cáo nhân viên mới", Left = 10, Top = 10, Width = 400, Height = 590 };
            p.Controls.Add(gb);

            Ui.Lbl(gb, "Công ty:", 15, 30, 80);
            cboKhachHang = Ui.Cbo(gb, "cboKhachHang", 100, 27, 280);
            Ui.Lbl(gb, "Phòng làm việc:", 15, 65, 85);
            cboPhong = Ui.Cbo(gb, "cboPhong", 100, 62, 120);
            Ui.Lbl(gb, "Họ tên:", 15, 100, 80);
            txtHoTen = Ui.Txt(gb, "txtHoTen", 100, 97, 280);
            Ui.Lbl(gb, "Ngày sinh:", 15, 135, 80);
            dtNgaySinh = Ui.Dtp(gb, "dtNgaySinh", 100, 132, 130);
            dtNgaySinh.Value = new DateTime(1995, 1, 1);
            Ui.Lbl(gb, "Giới tính:", 15, 170, 80);
            cboGioiTinh = Ui.Cbo(gb, "cboGioiTinh", 100, 167, 100, "Nam", "Nữ", "Khác");
            Ui.Lbl(gb, "Chức vụ:", 15, 205, 80);
            txtChucVu = Ui.Txt(gb, "txtChucVu", 100, 202, 280);

            Ui.Lbl(gb, "Hình ảnh:", 15, 240, 80);
            picHinh = new PictureBox
            {
                Name = "picHinh", Left = 100, Top = 240, Width = 130, Height = 160,
                BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom
            };
            gb.Controls.Add(picHinh);
            Ui.Btn(gb, "btnChonAnh", "Chọn ảnh...", 245, 240, 110, (o, ev) => Ui.Run(ChonAnh));
            txtHinhAnh = Ui.Txt(gb, "txtHinhAnh", 100, 410, 280);
            txtHinhAnh.ReadOnly = true;

            Ui.Btn(gb, "btnBaoCao", "Báo cáo nhân viên", 15, 455, 180, (o, ev) => Ui.Run(BaoCao));
            Ui.Btn(gb, "btnNghiViec", "Nhân viên nghỉ việc", 205, 455, 175, (o, ev) => Ui.Run(NghiViec));

            dgvNV = Ui.Grid(p, "dgvNV", 425, 10, 730, 590);
            dgvNV.SelectionChanged += (o, ev) => HienAnhDong();
            cboKhachHang.SelectedIndexChanged += (o, ev) => Ui.Run(() =>
                Ui.Bind(cboPhong, NhanVienService.PhongDangThueCua(Ui.Val(cboKhachHang)), "MaPhong", "MaPhong"));
        }

        private void ChonAnh()
        {
            using (var dlg = new OpenFileDialog { Filter = "Ảnh (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp" })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                txtHinhAnh.Text = dlg.FileName;
                HienAnh(dlg.FileName);
            }
        }

        private void HienAnh(string duongDan)
        {
            if (picHinh.Image != null) picHinh.Image.Dispose();
            picHinh.Image = null;
            if (string.IsNullOrWhiteSpace(duongDan) || !File.Exists(duongDan)) return;
            using (Image tmp = Image.FromFile(duongDan))
            {
                picHinh.Image = new Bitmap(tmp);      // sao chép để không khóa tệp
            }
        }

        private void HienAnhDong() { HienAnh(Ui.Cell(dgvNV, "HinhAnh")); }

        private void BaoCao()
        {
            string ma = NhanVienService.BaoCaoNhanVienMoi(txtHoTen.Text, dtNgaySinh.Value, cboGioiTinh.Text, txtChucVu.Text,
                Ui.Val(cboKhachHang), Ui.Val(cboPhong), string.IsNullOrWhiteSpace(txtHinhAnh.Text) ? null : txtHinhAnh.Text);
            Ui.Info("Đã tiếp nhận báo cáo nhân viên mới, mã nhân viên " + ma + ".");
            txtHoTen.Clear(); txtChucVu.Clear(); txtHinhAnh.Clear(); HienAnh(null);
            TaiTatCa();
        }

        private void NghiViec()
        {
            string ma = Ui.Cell(dgvNV, "MaNV");
            if (ma == "") throw new NghiepVuException("Chưa chọn nhân viên.");
            if (!Ui.Confirm("Xác nhận nhân viên " + ma + " nghỉ việc? Các thẻ giữ xe đang dùng sẽ bị huỷ.")) return;
            NhanVienService.NghiViec(ma);
            Ui.Info("Đã cập nhật nhân viên nghỉ việc và huỷ thẻ giữ xe.");
            TaiTatCa();
        }

        // =================== THẺ GIỮ XE ===================
        private void DungTabThe(Control p)
        {
            var gb = new GroupBox { Text = "Cấp thẻ giữ xe", Left = 10, Top = 10, Width = 400, Height = 330 };
            p.Controls.Add(gb);

            Ui.Lbl(gb, "Nhân viên:", 15, 30, 80);
            cboNhanVien = Ui.Cbo(gb, "cboNhanVien", 100, 27, 280);
            Ui.Lbl(gb, "Loại xe:", 15, 65, 80);
            cboLoaiXe = Ui.Cbo(gb, "cboLoaiXe", 100, 62, 130, "Xe máy", "Ô tô", "Xe đạp");
            Ui.Lbl(gb, "Biển số:", 15, 100, 80);
            txtBienSo = Ui.Txt(gb, "txtBienSo", 100, 97, 160);
            Ui.Lbl(gb, "Hãng xe:", 15, 135, 80);
            txtHangXe = Ui.Txt(gb, "txtHangXe", 100, 132, 200);
            Ui.Lbl(gb, "Màu sắc:", 15, 170, 80);
            txtMauSac = Ui.Txt(gb, "txtMauSac", 100, 167, 200);
            Ui.Btn(gb, "btnCapThe", "Cấp thẻ", 15, 215, 150, (o, ev) => Ui.Run(CapThe));
            Ui.Btn(gb, "btnHuyThe", "Huỷ thẻ đang chọn", 175, 215, 190, (o, ev) => Ui.Run(HuyThe));

            dgvThe = Ui.Grid(p, "dgvThe", 425, 10, 730, 590);
        }

        private void CapThe()
        {
            string ma = NhanVienService.CapThe(Ui.Val(cboNhanVien), cboLoaiXe.Text, txtBienSo.Text, txtHangXe.Text, txtMauSac.Text);
            Ui.Info("Đã cấp thẻ giữ xe " + ma + ".");
            txtBienSo.Clear(); txtHangXe.Clear(); txtMauSac.Clear();
            TaiThe();
        }

        private void HuyThe()
        {
            string ma = Ui.Cell(dgvThe, "MaThe");
            if (ma == "") throw new NghiepVuException("Chưa chọn thẻ.");
            if (!Ui.Confirm("Huỷ thẻ giữ xe " + ma + "?")) return;
            NhanVienService.HuyThe(ma);
            Ui.Info("Đã huỷ thẻ giữ xe.");
            TaiThe();
        }

        private void TaiThe() { dgvThe.DataSource = NhanVienService.LayThe(); }

        private void TaiTatCa()
        {
            Ui.Bind(cboKhachHang, DanhMucService.LayKhachHang(), "TenCongTy", "MaKH");
            Ui.Bind(cboNhanVien, NhanVienService.NhanVienDangLam(), "HienThi", "MaNV");
            dgvNV.DataSource = NhanVienService.LayNhanVien();
            if (dgvNV.Columns["HinhAnh"] != null) dgvNV.Columns["HinhAnh"].Visible = false;
            TaiThe();
        }
    }
}
