using System;
using System.Windows.Forms;
using QuanLyKhachSanUI.Repositories;

namespace QuanLyKhachSanUI
{
    public class frmDichVu : Form
    {
        private DichVuRepository _repo = new DichVuRepository();
        private DataGridView dgv = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
        private TextBox txtMaDV = new TextBox(), txtTenDV = new TextBox(), txtDVT = new TextBox(), txtDonGia = new TextBox();
        private Button btnThem = new Button { Text = "Thêm" }, btnSua = new Button { Text = "Sửa" }, btnXoa = new Button { Text = "Xóa" };

        public frmDichVu()
        {
            this.Text = "Quản Lý Dịch Vụ";
            this.Size = new System.Drawing.Size(700, 450);
            InitLayout();
            LoadData();
        }

        private void InitLayout()
        {
            TableLayoutPanel panel = new TableLayoutPanel { Dock = DockStyle.Top, RowCount = 2, ColumnCount = 4, Height = 90, Padding = new Padding(10) };
            panel.Controls.Add(new Label { Text = "Mã DV:" }, 0, 0); panel.Controls.Add(txtMaDV, 1, 0);
            panel.Controls.Add(new Label { Text = "Tên DV:" }, 2, 0); panel.Controls.Add(txtTenDV, 3, 0);
            panel.Controls.Add(new Label { Text = "Đơn Vị Tính:" }, 0, 1); panel.Controls.Add(txtDVT, 1, 1);
            panel.Controls.Add(new Label { Text = "Đơn Giá:" }, 2, 1); panel.Controls.Add(txtDonGia, 3, 1);

            FlowLayoutPanel pnlBtn = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40 };
            pnlBtn.Controls.AddRange(new Control[] { btnThem, btnSua, btnXoa });

            this.Controls.Add(dgv);
            this.Controls.Add(pnlBtn);
            this.Controls.Add(panel);

            btnThem.Click += (s, e) => {
                _repo.InsertDichVu(txtMaDV.Text, txtTenDV.Text, txtDVT.Text, decimal.Parse(txtDonGia.Text));
                LoadData();
            };

            btnSua.Click += (s, e) => {
                _repo.UpdateDichVu(txtMaDV.Text, txtTenDV.Text, txtDVT.Text, decimal.Parse(txtDonGia.Text));
                LoadData();
            };

            btnXoa.Click += (s, e) => {
                _repo.DeleteDichVu(txtMaDV.Text);
                LoadData();
            };

            dgv.CellClick += (s, e) => {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgv.Rows[e.RowIndex];
                    txtMaDV.Text = row.Cells["MaDV"].Value?.ToString();
                    txtTenDV.Text = row.Cells["TenDV"].Value?.ToString();
                    txtDVT.Text = row.Cells["DonViTinh"].Value?.ToString();
                    txtDonGia.Text = row.Cells["DonGia"].Value?.ToString();
                }
            };
        }

        private void LoadData() => dgv.DataSource = _repo.GetAllDichVu();
    }
}