using System.Data;
using QuanLyThuVien.Repositories;

namespace QuanLyThuVien.Forms
{
    public class frmDanhMuc : Form
    {
        private readonly DanhMucAdminRepository _repo = new();
        private int? _selectedId = null;

        private TextBox txtTen = null!, txtMoTa = null!;
        private Button btnThem = null!, btnSua = null!, btnXoa = null!, btnLamMoi = null!;
        private DataGridView dgv = null!;

        public frmDanhMuc()
        {
            Text = "Quản lý Danh mục";
            Width = 720;
            Height = 520;
            StartPosition = FormStartPosition.CenterParent;
            BuildUI();
            LoadData();
            ClearForm();
        }

        private void BuildUI()
        {
            var lblTen = UiHelper.Lbl("Tên danh mục", 20, 20);
            txtTen = new TextBox { Left = 140, Top = 20, Width = 300, MaxLength = 100 };
            var lblMoTa = UiHelper.Lbl("Mô tả", 20, 55);
            txtMoTa = new TextBox { Left = 140, Top = 55, Width = 300, Height = 60, Multiline = true };

            btnThem = new Button { Text = "Thêm", Left = 20, Top = 130, Width = 80 };
            btnSua = new Button { Text = "Sửa", Left = 110, Top = 130, Width = 80 };
            btnXoa = new Button { Text = "Xóa", Left = 200, Top = 130, Width = 80 };
            btnLamMoi = new Button { Text = "Làm mới", Left = 290, Top = 130, Width = 80 };
            btnThem.Click += BtnThem_Click;
            btnSua.Click += BtnSua_Click;
            btnXoa.Click += BtnXoa_Click;
            btnLamMoi.Click += (s, e) => ClearForm();

            dgv = UiHelper.NewGrid(20, 175, 660, 290);
            dgv.CellClick += Dgv_CellClick;

            Controls.AddRange(new Control[] { lblTen, txtTen, lblMoTa, txtMoTa, btnThem, btnSua, btnXoa, btnLamMoi, dgv });
        }

        private void LoadData()
        {
            try { dgv.DataSource = _repo.GetAll(); }
            catch (Exception ex) { MessageBox.Show("Lỗi kết nối CSDL: " + ex.Message); }
        }

        private void BtnThem_Click(object? sender, EventArgs e)
        {
            if (!ValidateInput()) return;
            if (DbHelper.TryRun(() => _repo.Insert(txtTen.Text.Trim(), txtMoTa.Text.Trim())))
            {
                LoadData();
                ClearForm();
            }
        }

        private void BtnSua_Click(object? sender, EventArgs e)
        {
            if (_selectedId == null) { UiHelper.Warn("Chọn dòng cần sửa."); return; }
            if (!ValidateInput()) return;
            int id = _selectedId.Value;
            if (DbHelper.TryRun(() => _repo.Update(id, txtTen.Text.Trim(), txtMoTa.Text.Trim())))
            {
                LoadData();
                ClearForm();
            }
        }

        private void BtnXoa_Click(object? sender, EventArgs e)
        {
            if (_selectedId == null) { UiHelper.Warn("Chọn dòng cần xóa."); return; }
            if (!UiHelper.Confirm("Xóa danh mục đang chọn?")) return;
            int id = _selectedId.Value;
            if (DbHelper.TryRun(() => _repo.Delete(id), "Không thể xóa: danh mục đang được dùng bởi tài liệu."))
            {
                LoadData();
                ClearForm();
            }
        }

        private void Dgv_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgv.Rows[e.RowIndex];
            _selectedId = Convert.ToInt32(row.Cells["MaDanhMuc"].Value);
            txtTen.Text = Convert.ToString(row.Cells["TenDanhMuc"].Value) ?? "";
            txtMoTa.Text = Convert.ToString(row.Cells["MoTa"].Value) ?? "";
            SetMode(editing: true);
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtTen.Text))
            {
                UiHelper.Warn("Nhập tên danh mục.");
                return false;
            }
            return true;
        }

        private void ClearForm()
        {
            _selectedId = null;
            txtTen.Clear();
            txtMoTa.Clear();
            SetMode(editing: false);
            txtTen.Focus();
        }

        private void SetMode(bool editing)
        {
            btnThem.Enabled = !editing;
            btnSua.Enabled = editing;
            btnXoa.Enabled = editing;
        }
    }
}