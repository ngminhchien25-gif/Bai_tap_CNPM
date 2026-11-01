using System;
using System.Windows.Forms;
using QuanLyKhachSanUI.Repositories;

namespace QuanLyKhachSanUI.Forms
{
    public class frmNhanVien : Form
    {
        private NhanVienRepository _repo = new NhanVienRepository();
        private DataGridView dgv = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, ReadOnly = true };
        private TextBox txtMa = new TextBox(), txtTen = new TextBox(), txtSdt = new TextBox();
        private ComboBox cboVaiTro = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private Button btnThem = new Button { Text = "Thêm" }, btnSua = new Button { Text = "Sửa" }, btnXoa = new Button { Text = "Xóa" };

        public frmNhanVien()
        {
            this.Text = "Quản Lý Nhân Viên";
            this.Size = new System.Drawing.Size(800, 500);
            InitCustomControls();
            LoadData();
        }

        private void InitCustomControls()
        {
            TableLayoutPanel panel = new TableLayoutPanel { Dock = DockStyle.Top, RowCount = 2, ColumnCount = 4, Height = 90, Padding = new Padding(10) };
            panel.Controls.Add(new Label { Text = "Mã NV:" }, 0, 0); panel.Controls.Add(txtMa, 1, 0);
            panel.Controls.Add(new Label { Text = "Họ Tên:" }, 2, 0); panel.Controls.Add(txtTen, 3, 0);
            panel.Controls.Add(new Label { Text = "Vai Trò:" }, 0, 1); panel.Controls.Add(cboVaiTro, 1, 1);
            panel.Controls.Add(new Label { Text = "Số ĐT:" }, 2, 1); panel.Controls.Add(txtSdt, 3, 1);

            cboVaiTro.Items.AddRange(new string[] { "Lễ tân", "Phục vụ phòng", "Thanh toán", "Quản lý" });
            if (cboVaiTro.Items.Count > 0) cboVaiTro.SelectedIndex = 0;

            FlowLayoutPanel pnlButtons = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40 };
            pnlButtons.Controls.AddRange(new Control[] { btnThem, btnSua, btnXoa });

            this.Controls.Add(dgv);
            this.Controls.Add(pnlButtons);
            this.Controls.Add(panel);

            // Bắt sự kiện Click dòng trên Bảng để đổ dữ liệu lên Form
            dgv.CellClick += (s, e) => {
                if (e.RowIndex >= 0 && dgv.Rows[e.RowIndex].Cells[0].Value != null)
                {
                    DataGridViewRow row = dgv.Rows[e.RowIndex];
                    txtMa.Text = row.Cells["MaNV"].Value?.ToString();
                    txtMa.ReadOnly = true; // Khóa Mã NV khi Sửa/Xóa
                    txtTen.Text = row.Cells["HoTen"].Value?.ToString();
                    cboVaiTro.Text = row.Cells["VaiTro"].Value?.ToString();
                    txtSdt.Text = row.Cells["SoDienThoai"].Value?.ToString();
                }
            };

            btnThem.Click += (s, e) => {
                txtMa.ReadOnly = false;
                _repo.InsertNhanVien(txtMa.Text, txtTen.Text, cboVaiTro.Text, txtSdt.Text);
                LoadData();
                ClearForm();
            };

            btnSua.Click += (s, e) => {
                if (string.IsNullOrEmpty(txtMa.Text)) return;
                _repo.UpdateNhanVien(txtMa.Text, txtTen.Text, cboVaiTro.Text, txtSdt.Text);
                LoadData();
                ClearForm();
            };

            btnXoa.Click += (s, e) => {
                if (string.IsNullOrEmpty(txtMa.Text)) return;
                if (MessageBox.Show("Xác nhận xóa nhân viên này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    try
                    {
                        _repo.DeleteNhanVien(txtMa.Text);
                        LoadData();
                        ClearForm();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Không thể xóa do nhân viên đã có lịch sử giao dịch/phiếu đặt: " + ex.Message);
                    }
                }
            };
        }

        private void ClearForm()
        {
            txtMa.ReadOnly = false;
            txtMa.Clear();
            txtTen.Clear();
            txtSdt.Clear();
            if (cboVaiTro.Items.Count > 0) cboVaiTro.SelectedIndex = 0;
        }

        private void LoadData() => dgv.DataSource = _repo.GetAllNhanVien();
    }
}