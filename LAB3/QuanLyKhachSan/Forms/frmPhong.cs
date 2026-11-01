using System;
using System.Data;
using System.Windows.Forms;
using QuanLyKhachSanUI.Repositories;

namespace QuanLyKhachSanUI
{
    public class frmPhong : Form
    {
        private PhongRepository _repo = new PhongRepository();
        private DataGridView dgv = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
        private TextBox txtSoPhong = new TextBox(), txtSoNguoi = new TextBox(), txtDonGia = new TextBox();
        // Khóa DropDownList để không thể gõ tay giá trị ngoài danh mục (tránh vi phạm FK/CHECK khi lưu)
        private ComboBox cboKhuVuc = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private ComboBox cboTrangThai = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private Button btnThem = new Button { Text = "Thêm" }, btnSua = new Button { Text = "Sửa" }, btnXoa = new Button { Text = "Xóa" }, btnLamMoi = new Button { Text = "Làm Mới" };

        public frmPhong()
        {
            this.Text = "Quản Lý Phòng & Khu Vực";
            this.Size = new System.Drawing.Size(800, 500);
            InitLayout();
            LoadKhuVuc();
            LoadData();
        }

        private void InitLayout()
        {
            TableLayoutPanel panel = new TableLayoutPanel { Dock = DockStyle.Top, RowCount = 3, ColumnCount = 4, Height = 120, Padding = new Padding(10) };
            panel.Controls.Add(new Label { Text = "Số Phòng:" }, 0, 0); panel.Controls.Add(txtSoPhong, 1, 0);
            panel.Controls.Add(new Label { Text = "Khu Vực:" }, 2, 0); panel.Controls.Add(cboKhuVuc, 3, 0);
            panel.Controls.Add(new Label { Text = "Số Người Tối Đa:" }, 0, 1); panel.Controls.Add(txtSoNguoi, 1, 1);
            panel.Controls.Add(new Label { Text = "Đơn Giá Ngày:" }, 2, 1); panel.Controls.Add(txtDonGia, 3, 1);
            panel.Controls.Add(new Label { Text = "Trạng Thái:" }, 0, 2); panel.Controls.Add(cboTrangThai, 1, 2);

            // Khớp đúng danh sách cho phép theo ràng buộc CK_Phong_TrangThai trong CSDL
            cboTrangThai.Items.AddRange(new string[] { "Trống", "Đã đặt", "Đang ở", "Bảo trì" });
            cboTrangThai.SelectedIndex = 0;

            FlowLayoutPanel pnlBtn = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40 };
            pnlBtn.Controls.AddRange(new Control[] { btnThem, btnSua, btnXoa, btnLamMoi });

            this.Controls.Add(dgv);
            this.Controls.Add(pnlBtn);
            this.Controls.Add(panel);

            btnThem.Click += (s, e) =>
            {
                if (!KiemTraHopLe(out string soPhong, out int soNguoi, out decimal donGia)) return;
                try
                {
                    _repo.InsertPhong(soPhong, cboKhuVuc.SelectedValue.ToString(), soNguoi, donGia, cboTrangThai.Text);
                    LoadData();
                    LamMoiForm();
                }
                catch (Exception ex) { MessageBox.Show("Lỗi khi thêm phòng: " + ex.Message); }
            };

            btnSua.Click += (s, e) =>
            {
                if (!KiemTraHopLe(out string soPhong, out int soNguoi, out decimal donGia)) return;
                try
                {
                    _repo.UpdatePhong(soPhong, cboKhuVuc.SelectedValue.ToString(), soNguoi, donGia, cboTrangThai.Text);
                    LoadData();
                    LamMoiForm();
                }
                catch (Exception ex) { MessageBox.Show("Lỗi khi sửa phòng: " + ex.Message); }
            };

            btnXoa.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtSoPhong.Text))
                {
                    MessageBox.Show("Vui lòng chọn phòng cần xóa!");
                    return;
                }
                var xacNhan = MessageBox.Show($"Xác nhận xóa phòng \"{txtSoPhong.Text}\"?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (xacNhan != DialogResult.Yes) return;
                try
                {
                    _repo.DeletePhong(txtSoPhong.Text.Trim());
                    LoadData();
                    LamMoiForm();
                }
                catch (Exception ex)
                {
                    // FK sẽ chặn xóa nếu phòng đã phát sinh nghiệp vụ (đặt phòng, lắp đặt, dịch vụ...)
                    MessageBox.Show("Không thể xóa phòng này (có thể đã phát sinh dữ liệu liên quan): " + ex.Message);
                }
            };

            btnLamMoi.Click += (s, e) => LamMoiForm();

            dgv.CellClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                DataGridViewRow row = dgv.Rows[e.RowIndex];
                txtSoPhong.Text = row.Cells["SoPhong"].Value?.ToString();
                txtSoNguoi.Text = row.Cells["SoNguoiToiDa"].Value?.ToString();
                txtDonGia.Text = row.Cells["DonGiaNgay"].Value?.ToString();
                cboTrangThai.Text = row.Cells["TrangThai"].Value?.ToString();

                // Chọn đúng khu vực theo mã (MaKhuVuc), không dựa vào so khớp chuỗi TenKhuVuc
                if (dgv.Columns.Contains("MaKhuVuc"))
                    cboKhuVuc.SelectedValue = row.Cells["MaKhuVuc"].Value;
                else
                    cboKhuVuc.Text = row.Cells["TenKhuVuc"].Value?.ToString();

                // SoPhong là khóa chính: khóa lại khi đang sửa để tránh đổi nhầm khóa của dòng khác
                txtSoPhong.ReadOnly = true;
            };
        }

        private bool KiemTraHopLe(out string soPhong, out int soNguoi, out decimal donGia)
        {
            soPhong = txtSoPhong.Text.Trim();
            soNguoi = 0;
            donGia = 0;

            if (string.IsNullOrWhiteSpace(soPhong))
            {
                MessageBox.Show("Vui lòng nhập Số Phòng!");
                return false;
            }
            if (cboKhuVuc.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn Khu Vực!");
                return false;
            }
            if (!int.TryParse(txtSoNguoi.Text.Trim(), out soNguoi) || soNguoi <= 0)
            {
                MessageBox.Show("Số Người Tối Đa phải là số nguyên dương!");
                return false;
            }
            if (!decimal.TryParse(txtDonGia.Text.Trim(), out donGia) || donGia < 0)
            {
                MessageBox.Show("Đơn Giá Ngày phải là số hợp lệ và không âm!");
                return false;
            }
            return true;
        }

        private void LamMoiForm()
        {
            txtSoPhong.Clear();
            txtSoPhong.ReadOnly = false;
            txtSoNguoi.Clear();
            txtDonGia.Clear();
            cboTrangThai.SelectedIndex = 0;
            if (cboKhuVuc.Items.Count > 0) cboKhuVuc.SelectedIndex = 0;
            dgv.ClearSelection();
        }

        private void LoadKhuVuc()
        {
            cboKhuVuc.DataSource = _repo.GetAllKhuVuc();
            cboKhuVuc.DisplayMember = "TenKhuVuc";
            cboKhuVuc.ValueMember = "MaKhuVuc";
        }

        private void LoadData() => dgv.DataSource = _repo.GetAllPhong();
    }
}