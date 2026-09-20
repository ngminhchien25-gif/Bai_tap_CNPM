using System.Data;
using QuanLyThuVien.Repositories;

namespace QuanLyThuVien.Forms
{
    public class frmTheThuVien : Form
    {
        private readonly TheAdminRepository _repo = new();
        private string? _selectedMaThe = null;

        private ComboBox cboDocGia = null!;
        private DateTimePicker dtpNgayCap = null!, dtpHetHan = null!;
        private CheckBox chkLePhi = null!;
        private Label lblChon = null!;
        private Button btnCap = null!, btnGiaHan = null!, btnKhoa = null!, btnMoKhoa = null!, btnLamMoi = null!;
        private DataGridView dgv = null!;

        public frmTheThuVien()
        {
            Text = "Quản lý Thẻ thư viện";
            Width = 950;
            Height = 620;
            StartPosition = FormStartPosition.CenterParent;
            BuildUI();
            LoadDocGia();
            LoadData();
            ClearForm();
        }

        private void BuildUI()
        {
            var lblDG = UiHelper.Lbl("Độc giả", 20, 20);
            cboDocGia = new ComboBox { Left = 130, Top = 20, Width = 320, DropDownStyle = ComboBoxStyle.DropDownList };

            var lblCap = UiHelper.Lbl("Ngày cấp", 20, 55);
            dtpNgayCap = new DateTimePicker { Left = 130, Top = 55, Width = 140, Format = DateTimePickerFormat.Short };
            var lblHan = UiHelper.Lbl("Hạn sử dụng", 300, 55, 90);
            dtpHetHan = new DateTimePicker { Left = 395, Top = 55, Width = 140, Format = DateTimePickerFormat.Short };
            chkLePhi = new CheckBox { Text = "Đã đóng lệ phí năm", Left = 560, Top = 57, Width = 170 };

            btnCap = new Button { Text = "Cấp thẻ mới", Left = 20, Top = 95, Width = 120 };
            btnGiaHan = new Button { Text = "Gia hạn (thẻ đã chọn)", Left = 150, Top = 95, Width = 160 };
            btnKhoa = new Button { Text = "Khóa thẻ", Left = 320, Top = 95, Width = 90 };
            btnMoKhoa = new Button { Text = "Mở khóa", Left = 420, Top = 95, Width = 90 };
            btnLamMoi = new Button { Text = "Làm mới", Left = 520, Top = 95, Width = 90 };
            lblChon = new Label { Left = 630, Top = 100, Width = 280, Text = "" };

            btnCap.Click += BtnCap_Click;
            btnGiaHan.Click += BtnGiaHan_Click;
            btnKhoa.Click += BtnKhoa_Click;
            btnMoKhoa.Click += BtnMoKhoa_Click;
            btnLamMoi.Click += (s, e) => { LoadData(); ClearForm(); };

            dgv = UiHelper.NewGrid(20, 140, 890, 425);
            dgv.CellClick += Dgv_CellClick;

            Controls.AddRange(new Control[]
            {
                lblDG, cboDocGia, lblCap, dtpNgayCap, lblHan, dtpHetHan, chkLePhi,
                btnCap, btnGiaHan, btnKhoa, btnMoKhoa, btnLamMoi, lblChon, dgv
            });
        }

        private void LoadDocGia()
        {
            try
            {
                cboDocGia.DataSource = _repo.GetDocGia();
                cboDocGia.DisplayMember = "HienThi";
                cboDocGia.ValueMember = "MaND";
            }
            catch (Exception ex) { MessageBox.Show("Lỗi kết nối CSDL: " + ex.Message); }
        }

        private void LoadData()
        {
            try
            {
                _repo.CapNhatHetHan();         
                dgv.DataSource = _repo.GetAll();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi kết nối CSDL: " + ex.Message); }
        }

        private void BtnCap_Click(object? sender, EventArgs e)
        {
            if (cboDocGia.SelectedValue == null) { UiHelper.Warn("Chọn độc giả."); return; }
            int maND = Convert.ToInt32(cboDocGia.SelectedValue);
            string maThe = "";
            if (DbHelper.TryRun(() => maThe = _repo.CapMoi(maND, dtpNgayCap.Value, dtpHetHan.Value, chkLePhi.Checked)))
            {
                UiHelper.Info("Cấp thẻ thành công. Mã thẻ: " + maThe);
                LoadData();
                ClearForm();
            }
        }

        private void BtnGiaHan_Click(object? sender, EventArgs e)
        {
            if (_selectedMaThe == null) { UiHelper.Warn("Chọn thẻ cần gia hạn trong danh sách."); return; }
            string ma = _selectedMaThe;
            if (DbHelper.TryRun(() => _repo.GiaHan(ma, dtpHetHan.Value, chkLePhi.Checked)))
            {
                UiHelper.Info("Gia hạn thẻ " + ma + " thành công đến " + dtpHetHan.Value.ToString("dd/MM/yyyy") + ".");
                LoadData();
                ClearForm();
            }
        }

        private void BtnKhoa_Click(object? sender, EventArgs e)
        {
            if (_selectedMaThe == null) { UiHelper.Warn("Chọn thẻ cần khóa."); return; }
            if (!UiHelper.Confirm("Khóa thẻ " + _selectedMaThe + "?")) return;
            string ma = _selectedMaThe;
            if (DbHelper.TryRun(() => _repo.Khoa(ma))) { LoadData(); ClearForm(); }
        }

        private void BtnMoKhoa_Click(object? sender, EventArgs e)
        {
            if (_selectedMaThe == null) { UiHelper.Warn("Chọn thẻ cần mở khóa."); return; }
            string ma = _selectedMaThe;
            if (DbHelper.TryRun(() => _repo.MoKhoa(ma))) { LoadData(); ClearForm(); }
        }

        private void Dgv_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgv.Rows[e.RowIndex];
            _selectedMaThe = Convert.ToString(row.Cells["MaThe"].Value);
            cboDocGia.SelectedValue = Convert.ToInt32(row.Cells["MaND"].Value);
            dtpHetHan.Value = Convert.ToDateTime(row.Cells["NgayHetHan"].Value).AddYears(1); 
            chkLePhi.Checked = false;                                                      
            lblChon.Text = "Đang chọn: " + _selectedMaThe;
            btnCap.Enabled = false;
            btnGiaHan.Enabled = btnKhoa.Enabled = btnMoKhoa.Enabled = true;
        }

        private void ClearForm()
        {
            _selectedMaThe = null;
            dtpNgayCap.Value = DateTime.Today;
            dtpHetHan.Value = DateTime.Today.AddYears(1);
            chkLePhi.Checked = false;
            lblChon.Text = "(chưa chọn thẻ)";
            btnCap.Enabled = true;
            btnGiaHan.Enabled = btnKhoa.Enabled = btnMoKhoa.Enabled = false;
        }
    }
}