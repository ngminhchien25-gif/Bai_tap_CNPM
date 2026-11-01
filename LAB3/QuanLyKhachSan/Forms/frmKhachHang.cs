using System;
using System.Windows.Forms;
using QuanLyKhachSanUI.Repositories;

namespace QuanLyKhachSanUI.Forms
{
    public class frmKhachHang : Form
    {
        private KhachHangRepository _repo = new KhachHangRepository();
        private DataGridView dgv = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, ReadOnly = true };
        private TextBox txtMa = new TextBox(), txtTen = new TextBox(), txtCMND = new TextBox(), txtQuocTich = new TextBox(), txtSDT = new TextBox();
        private Button btnThem = new Button { Text = "Thêm" }, btnSua = new Button { Text = "Sửa" }, btnXoa = new Button { Text = "Xóa" };

        public frmKhachHang()
        {
            this.Text = "Quản Lý Khách Hàng";
            this.Size = new System.Drawing.Size(800, 520);
            InitCustomControls();
            LoadData();
        }

        private void InitCustomControls()
        {
            TableLayoutPanel panel = new TableLayoutPanel { Dock = DockStyle.Top, RowCount = 3, ColumnCount = 4, Height = 120, Padding = new Padding(10) };
            panel.Controls.Add(new Label { Text = "Mã Khách:" }, 0, 0); panel.Controls.Add(txtMa, 1, 0);
            panel.Controls.Add(new Label { Text = "Họ Tên:" }, 2, 0); panel.Controls.Add(txtTen, 3, 0);
            panel.Controls.Add(new Label { Text = "CMND/CCCD:" }, 0, 1); panel.Controls.Add(txtCMND, 1, 1);
            panel.Controls.Add(new Label { Text = "Quốc Tịch:" }, 2, 1); panel.Controls.Add(txtQuocTich, 3, 1);
            panel.Controls.Add(new Label { Text = "Số ĐT:" }, 0, 2); panel.Controls.Add(txtSDT, 1, 2);

            FlowLayoutPanel pnlBtn = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40 };
            pnlBtn.Controls.AddRange(new Control[] { btnThem, btnSua, btnXoa });

            this.Controls.Add(dgv);
            this.Controls.Add(pnlBtn);
            this.Controls.Add(panel);

            // Click bảng chọn dòng
            dgv.CellClick += (s, e) => {
                if (e.RowIndex >= 0 && dgv.Rows[e.RowIndex].Cells[0].Value != null)
                {
                    DataGridViewRow row = dgv.Rows[e.RowIndex];
                    txtMa.Text = row.Cells["MaKhach"].Value?.ToString();
                    txtMa.ReadOnly = true; // Khóa Mã Khách khi Sửa/Xóa
                    txtTen.Text = row.Cells["HoTen"].Value?.ToString();
                    txtCMND.Text = row.Cells["SoCMND"].Value?.ToString();
                    txtQuocTich.Text = row.Cells["QuocTich"].Value?.ToString();
                    txtSDT.Text = row.Cells["SoDienThoai"].Value?.ToString();
                }
            };

            btnThem.Click += (s, e) => {
                txtMa.ReadOnly = false;
                _repo.InsertKhachHang(txtMa.Text, txtTen.Text, txtCMND.Text, txtQuocTich.Text, txtSDT.Text);
                LoadData();
                ClearForm();
            };

            btnSua.Click += (s, e) => {
                if (string.IsNullOrEmpty(txtMa.Text)) return;
                _repo.UpdateKhachHang(txtMa.Text, txtTen.Text, txtCMND.Text, txtQuocTich.Text, txtSDT.Text);
                LoadData();
                ClearForm();
            };

            btnXoa.Click += (s, e) => {
                if (string.IsNullOrEmpty(txtMa.Text)) return;
                if (MessageBox.Show("Xác nhận xóa khách hàng này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    try
                    {
                        _repo.DeleteKhachHang(txtMa.Text);
                        LoadData();
                        ClearForm();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Không thể xóa khách hàng đã có lịch sử đặt phòng: " + ex.Message);
                    }
                }
            };
        }

        private void ClearForm()
        {
            txtMa.ReadOnly = false;
            txtMa.Clear();
            txtTen.Clear();
            txtCMND.Clear();
            txtQuocTich.Clear();
            txtSDT.Clear();
        }

        private void LoadData() => dgv.DataSource = _repo.GetAllKhachHang();
    }
}