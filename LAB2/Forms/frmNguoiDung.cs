using System.Data;
using QuanLyThuVien.Repositories;

namespace QuanLyThuVien.Forms
{
    public class frmNguoiDung : Form
    {
        private readonly NguoiDungRepository _repo = new();
        private int? _selectedId = null;

        private TextBox txtTenDangNhap, txtMatKhau, txtHoTen, txtEmail, txtSoDienThoai, txtTimKiem;
        private ComboBox cboVaiTro;
        private Button btnThem, btnSua, btnXoa, btnLamMoi, btnTimKiem;
        private DataGridView dgv;

        public frmNguoiDung()
        {
            Text = "Quản lý Người dùng";
            Width = 900;
            Height = 600;
            BuildUI();
            LoadData();
        }

        private void BuildUI()
        {
            var lblTDN = new Label { Text = "Tên đăng nhập", Left = 20, Top = 20, Width = 120 };
            txtTenDangNhap = new TextBox { Left = 150, Top = 20, Width = 200 };

            var lblMK = new Label { Text = "Mật khẩu", Left = 20, Top = 55, Width = 120 };
            txtMatKhau = new TextBox { Left = 150, Top = 55, Width = 200, PasswordChar = '*' };

            var lblHT = new Label { Text = "Họ tên", Left = 20, Top = 90, Width = 120 };
            txtHoTen = new TextBox { Left = 150, Top = 90, Width = 200 };

            var lblEmail = new Label { Text = "Email", Left = 20, Top = 125, Width = 120 };
            txtEmail = new TextBox { Left = 150, Top = 125, Width = 200 };

            var lblSDT = new Label { Text = "Số điện thoại", Left = 20, Top = 160, Width = 120 };
            txtSoDienThoai = new TextBox { Left = 150, Top = 160, Width = 200 };

            var lblVT = new Label { Text = "Vai trò", Left = 20, Top = 195, Width = 120 };
            cboVaiTro = new ComboBox { Left = 150, Top = 195, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cboVaiTro.Items.AddRange(new[] { "DocGia", "ThuThu" });

            btnThem = new Button { Text = "Thêm", Left = 20, Top = 240, Width = 80 };
            btnSua = new Button { Text = "Sửa", Left = 110, Top = 240, Width = 80 };
            btnXoa = new Button { Text = "Xóa", Left = 200, Top = 240, Width = 80 };
            btnLamMoi = new Button { Text = "Làm mới", Left = 290, Top = 240, Width = 80 };

            btnThem.Click += BtnThem_Click;
            btnSua.Click += BtnSua_Click;
            btnXoa.Click += BtnXoa_Click;
            btnLamMoi.Click += (s, e) => ClearForm();

            txtTimKiem = new TextBox { Left = 400, Top = 20, Width = 200 };
            btnTimKiem = new Button { Text = "Tìm kiếm", Left = 610, Top = 20, Width = 90 };
            btnTimKiem.Click += (s, e) => dgv.DataSource = _repo.Search(txtTimKiem.Text.Trim());

            dgv = new DataGridView
            {
                Left = 20, Top = 290, Width = 840, Height = 260,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgv.CellClick += Dgv_CellClick;

            Controls.AddRange(new Control[]
            {
                lblTDN, txtTenDangNhap, lblMK, txtMatKhau, lblHT, txtHoTen,
                lblEmail, txtEmail, lblSDT, txtSoDienThoai, lblVT, cboVaiTro,
                btnThem, btnSua, btnXoa, btnLamMoi,
                txtTimKiem, btnTimKiem, dgv
            });
        }

        private void LoadData()
        {
            try { dgv.DataSource = _repo.GetAll(); }
            catch (Exception ex) { MessageBox.Show("Lỗi kết nối CSDL: " + ex.Message); }
        }

        private void BtnThem_Click(object? sender, EventArgs e)
        {
            if (!ValidateInput()) return;
            _repo.Insert(txtTenDangNhap.Text.Trim(), txtMatKhau.Text.Trim(), txtHoTen.Text.Trim(),
                txtEmail.Text.Trim(), txtSoDienThoai.Text.Trim(), cboVaiTro.Text);
            LoadData();
            ClearForm();
        }

        private void BtnSua_Click(object? sender, EventArgs e)
        {
            if (_selectedId == null) { MessageBox.Show("Chọn dòng cần sửa."); return; }
            if (!ValidateInput()) return;
            _repo.Update(_selectedId.Value, txtHoTen.Text.Trim(), txtEmail.Text.Trim(),
                txtSoDienThoai.Text.Trim(), cboVaiTro.Text);
            LoadData();
            ClearForm();
        }

        private void BtnXoa_Click(object? sender, EventArgs e)
        {
            if (_selectedId == null) { MessageBox.Show("Chọn dòng cần xóa."); return; }
            if (MessageBox.Show("Xác nhận xóa?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _repo.Delete(_selectedId.Value);
                LoadData();
                ClearForm();
            }
        }

        private void Dgv_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgv.Rows[e.RowIndex];
            _selectedId = Convert.ToInt32(row.Cells["MaND"].Value);
            txtTenDangNhap.Text = row.Cells["TenDangNhap"].Value.ToString();
            txtHoTen.Text = row.Cells["HoTen"].Value.ToString();
            txtEmail.Text = row.Cells["Email"].Value.ToString();
            txtSoDienThoai.Text = row.Cells["SoDienThoai"].Value?.ToString();
            cboVaiTro.Text = row.Cells["VaiTro"].Value.ToString();
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtHoTen.Text) || string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Nhập đầy đủ Họ tên và Email.");
                return false;
            }
            return true;
        }

        private void ClearForm()
        {
            _selectedId = null;
            txtTenDangNhap.Clear();
            txtMatKhau.Clear();
            txtHoTen.Clear();
            txtEmail.Clear();
            txtSoDienThoai.Clear();
            cboVaiTro.SelectedIndex = -1;
        }
    }
}