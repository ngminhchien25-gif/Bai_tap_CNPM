using System.Data;
using QuanLyThuVien.Repositories;

namespace QuanLyThuVien.Forms
{
    public class frmTaiLieu : Form
    {
        private readonly TaiLieuRepository _repo = new();
        private readonly DanhMucRepository _dmRepo = new();
        private int? _selectedId = null;
        private string? _filePath = null;

        private TextBox txtTen, txtTacGia, txtNam, txtNXB, txtSoLuong, txtTimKiem, txtFile;
        private ComboBox cboDanhMuc, cboLoai;
        private Button btnThem, btnSua, btnXoa, btnLamMoi, btnTimKiem, btnChonFile;
        private DataGridView dgv;

        public frmTaiLieu()
        {
            Text = "Quản lý Tài liệu";
            Width = 950;
            Height = 650;
            BuildUI();
            LoadDanhMuc();
            LoadData();
        }

        private void BuildUI()
        {
            var lblTen = new Label { Text = "Tên tài liệu", Left = 20, Top = 20, Width = 100 };
            txtTen = new TextBox { Left = 130, Top = 20, Width = 220 };

            var lblTG = new Label { Text = "Tác giả", Left = 20, Top = 55, Width = 100 };
            txtTacGia = new TextBox { Left = 130, Top = 55, Width = 220 };

            var lblNam = new Label { Text = "Năm XB", Left = 20, Top = 90, Width = 100 };
            txtNam = new TextBox { Left = 130, Top = 90, Width = 100 };

            var lblNXB = new Label { Text = "Nhà XB", Left = 20, Top = 125, Width = 100 };
            txtNXB = new TextBox { Left = 130, Top = 125, Width = 220 };

            var lblDM = new Label { Text = "Danh mục", Left = 370, Top = 20, Width = 100 };
            cboDanhMuc = new ComboBox { Left = 470, Top = 20, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };

            var lblLoai = new Label { Text = "Loại tài liệu", Left = 370, Top = 55, Width = 100 };
            cboLoai = new ComboBox { Left = 470, Top = 55, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cboLoai.Items.AddRange(new[] { "SachIn", "SachDienTu" });
            cboLoai.SelectedIndexChanged += (s, e) =>
            {
                bool isDienTu = cboLoai.Text == "SachDienTu";
                btnChonFile.Enabled = isDienTu;
                txtFile.Enabled = isDienTu;
            };

            var lblSL = new Label { Text = "Số lượng tồn", Left = 370, Top = 90, Width = 100 };
            txtSoLuong = new TextBox { Left = 470, Top = 90, Width = 100 };

            var lblFile = new Label { Text = "File (ebook)", Left = 370, Top = 125, Width = 100 };
            txtFile = new TextBox { Left = 470, Top = 125, Width = 150, ReadOnly = true, Enabled = false };
            btnChonFile = new Button { Text = "Chọn...", Left = 630, Top = 124, Width = 60, Enabled = false };
            btnChonFile.Click += BtnChonFile_Click;

            btnThem = new Button { Text = "Thêm", Left = 20, Top = 170, Width = 80 };
            btnSua = new Button { Text = "Sửa", Left = 110, Top = 170, Width = 80 };
            btnXoa = new Button { Text = "Xóa", Left = 200, Top = 170, Width = 80 };
            btnLamMoi = new Button { Text = "Làm mới", Left = 290, Top = 170, Width = 80 };

            btnThem.Click += BtnThem_Click;
            btnSua.Click += BtnSua_Click;
            btnXoa.Click += BtnXoa_Click;
            btnLamMoi.Click += (s, e) => ClearForm();

            txtTimKiem = new TextBox { Left = 470, Top = 170, Width = 200 };
            btnTimKiem = new Button { Text = "Tìm kiếm", Left = 680, Top = 170, Width = 90 };
            btnTimKiem.Click += (s, e) => dgv.DataSource = _repo.Search(txtTimKiem.Text.Trim());

            dgv = new DataGridView
            {
                Left = 20, Top = 210, Width = 890, Height = 350,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgv.CellClick += Dgv_CellClick;

            Controls.AddRange(new Control[]
            {
                lblTen, txtTen, lblTG, txtTacGia, lblNam, txtNam, lblNXB, txtNXB,
                lblDM, cboDanhMuc, lblLoai, cboLoai, lblSL, txtSoLuong,
                lblFile, txtFile, btnChonFile,
                btnThem, btnSua, btnXoa, btnLamMoi, txtTimKiem, btnTimKiem, dgv
            });
        }

        private void LoadDanhMuc()
        {
            var dt = _dmRepo.GetAll();
            cboDanhMuc.DataSource = dt;
            cboDanhMuc.DisplayMember = "TenDanhMuc";
            cboDanhMuc.ValueMember = "MaDanhMuc";
        }

        private void LoadData()
        {
            try { dgv.DataSource = _repo.GetAll(); }
            catch (Exception ex) { MessageBox.Show("Lỗi kết nối CSDL: " + ex.Message); }
        }

        private void BtnChonFile_Click(object? sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog { Filter = "PDF/EPUB|*.pdf;*.epub|All files|*.*" };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                _filePath = dlg.FileName;
                txtFile.Text = Path.GetFileName(dlg.FileName);
            }
        }

        private void BtnThem_Click(object? sender, EventArgs e)
        {
            if (!ValidateInput()) return;
            _repo.Insert(txtTen.Text.Trim(), txtTacGia.Text.Trim(), ParseNam(),
                txtNXB.Text.Trim(), (int)cboDanhMuc.SelectedValue!, cboLoai.Text,
                int.Parse(txtSoLuong.Text), _filePath);
            LoadData();
            ClearForm();
        }

        private void BtnSua_Click(object? sender, EventArgs e)
        {
            if (_selectedId == null) { MessageBox.Show("Chọn tài liệu cần sửa."); return; }
            if (!ValidateInput()) return;
            _repo.Update(_selectedId.Value, txtTen.Text.Trim(), txtTacGia.Text.Trim(), ParseNam(),
                txtNXB.Text.Trim(), (int)cboDanhMuc.SelectedValue!, cboLoai.Text,
                int.Parse(txtSoLuong.Text), _filePath);
            LoadData();
            ClearForm();
        }

        private void BtnXoa_Click(object? sender, EventArgs e)
        {
            if (_selectedId == null) { MessageBox.Show("Chọn tài liệu cần xóa."); return; }
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
            _selectedId = Convert.ToInt32(row.Cells["MaTaiLieu"].Value);
            txtTen.Text = row.Cells["TenTaiLieu"].Value.ToString();
            txtTacGia.Text = row.Cells["TacGia"].Value.ToString();
            txtNam.Text = row.Cells["NamXuatBan"].Value?.ToString();
            txtNXB.Text = row.Cells["NhaXuatBan"].Value?.ToString();
            cboDanhMuc.Text = row.Cells["TenDanhMuc"].Value.ToString();
            cboLoai.Text = row.Cells["LoaiTaiLieu"].Value.ToString();
            txtSoLuong.Text = row.Cells["SoLuongTon"].Value.ToString();
            _filePath = row.Cells["FilePath"].Value?.ToString();
            txtFile.Text = _filePath != null ? Path.GetFileName(_filePath) : "";
        }

        private int? ParseNam() => int.TryParse(txtNam.Text.Trim(), out int n) ? n : null;

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtTen.Text) || string.IsNullOrWhiteSpace(txtTacGia.Text))
            {
                MessageBox.Show("Nhập đầy đủ Tên tài liệu và Tác giả.");
                return false;
            }
            if (cboDanhMuc.SelectedValue == null || string.IsNullOrEmpty(cboLoai.Text))
            {
                MessageBox.Show("Chọn Danh mục và Loại tài liệu.");
                return false;
            }
            if (!int.TryParse(txtSoLuong.Text.Trim(), out _))
            {
                MessageBox.Show("Số lượng tồn phải là số nguyên.");
                return false;
            }
            return true;
        }

        private void ClearForm()
        {
            _selectedId = null;
            _filePath = null;
            txtTen.Clear(); txtTacGia.Clear(); txtNam.Clear(); txtNXB.Clear();
            txtSoLuong.Clear(); txtFile.Clear();
            cboDanhMuc.SelectedIndex = -1;
            cboLoai.SelectedIndex = -1;
        }
    }
}