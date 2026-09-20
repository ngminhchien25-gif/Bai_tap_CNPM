using System.Data;
using QuanLyThuVien.Repositories;

namespace QuanLyThuVien.Forms
{
    public class frmYeuCauNhapSach : Form
    {
        private readonly YeuCauRepository _repo = new();
        private int? _selectedId = null;
        private string? _selectedTrangThai = null;

        private ComboBox cboNguoiDung = null!, cboLoc = null!;
        private TextBox txtTenSach = null!, txtTacGia = null!, txtNam = null!, txtGhiChu = null!;
        private Button btnGui = null!, btnChapNhan = null!, btnTuChoi = null!, btnXoa = null!, btnLamMoi = null!;
        private DataGridView dgv = null!;

        public frmYeuCauNhapSach()
        {
            Text = "Yêu cầu nhập sách";
            Width = 980;
            Height = 620;
            StartPosition = FormStartPosition.CenterParent;
            BuildUI();
            LoadNguoiDung();
            LoadData();
            ClearForm();
        }

        private void BuildUI()
        {
            var lblND = UiHelper.Lbl("Người yêu cầu", 20, 20);
            cboNguoiDung = new ComboBox { Left = 130, Top = 20, Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            var lblTen = UiHelper.Lbl("Tên sách", 20, 55);
            txtTenSach = new TextBox { Left = 130, Top = 55, Width = 250, MaxLength = 255 };
            var lblTG = UiHelper.Lbl("Tác giả", 20, 90);
            txtTacGia = new TextBox { Left = 130, Top = 90, Width = 250, MaxLength = 150 };
            var lblNam = UiHelper.Lbl("Năm XB", 20, 125);
            txtNam = new TextBox { Left = 130, Top = 125, Width = 100, MaxLength = 4 };

            var lblGC = UiHelper.Lbl("Ghi chú", 420, 20, 70);
            txtGhiChu = new TextBox { Left = 495, Top = 20, Width = 420, Height = 105, Multiline = true };

            btnGui = new Button { Text = "Gửi yêu cầu", Left = 20, Top = 165, Width = 110 };
            btnChapNhan = new Button { Text = "Chấp nhận", Left = 140, Top = 165, Width = 100 };
            btnTuChoi = new Button { Text = "Từ chối", Left = 250, Top = 165, Width = 100 };
            btnXoa = new Button { Text = "Xóa", Left = 360, Top = 165, Width = 80 };
            btnLamMoi = new Button { Text = "Làm mới", Left = 450, Top = 165, Width = 90 };

            var lblLoc = UiHelper.Lbl("Lọc trạng thái", 680, 165, 90);
            cboLoc = new ComboBox { Left = 775, Top = 165, Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
            cboLoc.Items.AddRange(new object[] { "Tất cả", "ChuaXuLy", "DaChapNhan", "TuChoi" });
            cboLoc.SelectedIndex = 0;
            cboLoc.SelectedIndexChanged += (s, e) => LoadData();

            btnGui.Click += BtnGui_Click;
            btnChapNhan.Click += (s, e) => XuLy("DaChapNhan", "Chấp nhận yêu cầu này?");
            btnTuChoi.Click += (s, e) => XuLy("TuChoi", "Từ chối yêu cầu này?");
            btnXoa.Click += BtnXoa_Click;
            btnLamMoi.Click += (s, e) => { LoadData(); ClearForm(); };

            dgv = UiHelper.NewGrid(20, 205, 920, 360);
            dgv.CellClick += Dgv_CellClick;

            Controls.AddRange(new Control[]
            {
                lblND, cboNguoiDung, lblTen, txtTenSach, lblTG, txtTacGia, lblNam, txtNam, lblGC, txtGhiChu,
                btnGui, btnChapNhan, btnTuChoi, btnXoa, btnLamMoi, lblLoc, cboLoc, dgv
            });
        }

        private void LoadNguoiDung()
        {
            try
            {
                cboNguoiDung.DataSource = _repo.GetNguoiDung();
                cboNguoiDung.DisplayMember = "HienThi";
                cboNguoiDung.ValueMember = "MaND";
            }
            catch (Exception ex) { MessageBox.Show("Lỗi kết nối CSDL: " + ex.Message); }
        }

        private void LoadData()
        {
            try
            {
                string? tt = cboLoc.Text == "Tất cả" ? null : cboLoc.Text;
                dgv.DataSource = _repo.GetAll(tt);
            }
            catch (Exception ex) { MessageBox.Show("Lỗi kết nối CSDL: " + ex.Message); }
        }

        private void BtnGui_Click(object? sender, EventArgs e)
        {
            if (cboNguoiDung.SelectedValue == null) { UiHelper.Warn("Chọn người yêu cầu."); return; }
            if (string.IsNullOrWhiteSpace(txtTenSach.Text) || string.IsNullOrWhiteSpace(txtTacGia.Text))
            {
                UiHelper.Warn("Nhập đầy đủ Tên sách và Tác giả.");
                return;
            }

            int? nam = null;
            if (!string.IsNullOrWhiteSpace(txtNam.Text))
            {
                if (!int.TryParse(txtNam.Text.Trim(), out int n) || n < 1000 || n > DateTime.Today.Year + 1)
                {
                    UiHelper.Warn("Năm xuất bản không hợp lệ.");
                    return;
                }
                nam = n;
            }

            int maND = Convert.ToInt32(cboNguoiDung.SelectedValue);
            if (DbHelper.TryRun(() => _repo.Insert(maND, txtTenSach.Text.Trim(), txtTacGia.Text.Trim(), nam, txtGhiChu.Text.Trim())))
            {
                UiHelper.Info("Đã ghi nhận yêu cầu nhập sách.");
                LoadData();
                ClearForm();
            }
        }

        private void XuLy(string trangThaiMoi, string cauHoi)
        {
            if (_selectedId == null) { UiHelper.Warn("Chọn yêu cầu trong danh sách."); return; }
            if (!UiHelper.Confirm(cauHoi)) return;
            int id = _selectedId.Value;
            if (DbHelper.TryRun(() => _repo.SetTrangThai(id, trangThaiMoi))) { LoadData(); ClearForm(); }
        }

        private void BtnXoa_Click(object? sender, EventArgs e)
        {
            if (_selectedId == null) { UiHelper.Warn("Chọn yêu cầu cần xóa."); return; }
            if (!UiHelper.Confirm("Xóa yêu cầu đang chọn?")) return;
            int id = _selectedId.Value;
            if (DbHelper.TryRun(() => _repo.Delete(id))) { LoadData(); ClearForm(); }
        }

        private void Dgv_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgv.Rows[e.RowIndex];
            _selectedId = Convert.ToInt32(row.Cells["MaYeuCau"].Value);
            _selectedTrangThai = Convert.ToString(row.Cells["TrangThai"].Value);
            cboNguoiDung.SelectedValue = Convert.ToInt32(row.Cells["MaND"].Value);
            txtTenSach.Text = Convert.ToString(row.Cells["TenSach"].Value) ?? "";
            txtTacGia.Text = Convert.ToString(row.Cells["TacGia"].Value) ?? "";
            txtNam.Text = Convert.ToString(row.Cells["NamXuatBan"].Value) ?? "";
            txtGhiChu.Text = Convert.ToString(row.Cells["GhiChu"].Value) ?? "";
            SetMode(selected: true);
        }

        private void ClearForm()
        {
            _selectedId = null;
            _selectedTrangThai = null;
            txtTenSach.Clear(); txtTacGia.Clear(); txtNam.Clear(); txtGhiChu.Clear();
            if (cboNguoiDung.Items.Count > 0) cboNguoiDung.SelectedIndex = 0;
            SetMode(selected: false);
        }

        private void SetMode(bool selected)
        {
            bool chuaXuLy = selected && _selectedTrangThai == "ChuaXuLy";
            btnGui.Enabled = !selected;
            btnChapNhan.Enabled = chuaXuLy;
            btnTuChoi.Enabled = chuaXuLy;
            btnXoa.Enabled = chuaXuLy;
        }
    }
}