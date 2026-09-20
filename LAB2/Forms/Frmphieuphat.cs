using System.Data;
using QuanLyThuVien.Repositories;

namespace QuanLyThuVien.Forms
{
    public class frmPhieuPhat : Form
    {
        private readonly PhieuPhatRepository _repo = new();
        private int? _maPhieu = null;

        private DataGridView dgvPhieu = null!, dgvPhat = null!;
        private Label lblChon = null!;
        private ComboBox cboNV = null!;
        private CheckBox chkTre = null!, chkHu = null!, chkMat = null!;
        private NumericUpDown numPhi = null!;
        private TextBox txtGhiChu = null!;
        private Button btnLap = null!, btnLamMoi = null!;

        public frmPhieuPhat()
        {
            Text = "Lập phiếu phạt";
            Width = 1000;
            Height = 730;
            StartPosition = FormStartPosition.CenterParent;
            BuildUI();
            LoadNhanVien();
            LoadData();
            ResetForm();
        }

        private void BuildUI()
        {
            var lblDs = new Label { Text = "1. Chọn phiếu mượn cần lập phạt", Left = 20, Top = 12, Width = 400, Font = new Font(Font, FontStyle.Bold) };
            dgvPhieu = UiHelper.NewGrid(20, 38, 940, 220);
            dgvPhieu.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dgvPhieu.CellClick += DgvPhieu_CellClick;

            var lblSel = UiHelper.Lbl("Phiếu đang chọn", 20, 275, 120);
            lblChon = new Label { Left = 145, Top = 278, Width = 600 };

            var lblNV = UiHelper.Lbl("Nhân viên lập", 20, 310, 120);
            cboNV = new ComboBox { Left = 145, Top = 310, Width = 230, DropDownStyle = ComboBoxStyle.DropDownList };
            chkTre = new CheckBox { Text = "Trả trễ hạn", Left = 410, Top = 312, Width = 110 };
            chkHu = new CheckBox { Text = "Rách/hư hỏng", Left = 530, Top = 312, Width = 120 };
            chkMat = new CheckBox { Text = "Mất sách", Left = 660, Top = 312, Width = 100 };

            var lblPhi = UiHelper.Lbl("Phí phạt (đ)", 20, 345, 120);
            numPhi = new NumericUpDown
            {
                Left = 145, Top = 345, Width = 140,
                Minimum = 0, Maximum = 1000000000, Increment = 1000, ThousandsSeparator = true
            };
            var lblGC = UiHelper.Lbl("Ghi chú", 410, 345, 70);
            txtGhiChu = new TextBox { Left = 485, Top = 345, Width = 320, MaxLength = 250 };

            btnLap = new Button { Text = "Lập phiếu phạt", Left = 20, Top = 385, Width = 140 };
            btnLamMoi = new Button { Text = "Làm mới", Left = 170, Top = 385, Width = 90 };
            btnLap.Click += BtnLap_Click;
            btnLamMoi.Click += (s, e) => { LoadData(); ResetForm(); };

            var lblDs2 = new Label { Text = "2. Các phiếu phạt đã lập", Left = 20, Top = 425, Width = 400, Font = new Font(Font, FontStyle.Bold) };
            dgvPhat = UiHelper.NewGrid(20, 450, 940, 230);

            Controls.AddRange(new Control[]
            {
                lblDs, dgvPhieu, lblSel, lblChon, lblNV, cboNV, chkTre, chkHu, chkMat,
                lblPhi, numPhi, lblGC, txtGhiChu, btnLap, btnLamMoi, lblDs2, dgvPhat
            });
        }

        private void LoadNhanVien()
        {
            try
            {
                cboNV.DataSource = _repo.GetNhanVien();
                cboNV.DisplayMember = "HoTen";
                cboNV.ValueMember = "MaND";
            }
            catch (Exception ex) { MessageBox.Show("Lỗi kết nối CSDL: " + ex.Message); }
        }

        private void LoadData()
        {
            try
            {
                dgvPhieu.DataSource = _repo.GetPhieuMuon();
                dgvPhat.DataSource = _repo.GetPhat(new DateTime(2000, 1, 1), new DateTime(2999, 12, 31));
            }
            catch (Exception ex) { MessageBox.Show("Lỗi kết nối CSDL: " + ex.Message); }
        }

        private void DgvPhieu_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgvPhieu.Rows[e.RowIndex];

            object? daPhat = row.Cells["DaPhat"].Value;
            if (daPhat != null && daPhat != DBNull.Value)
            {
                UiHelper.Warn("Phiếu mượn này đã có phiếu phạt.");
                ResetForm();
                return;
            }

            _maPhieu = Convert.ToInt32(row.Cells["MaPhieu"].Value);
            DateTime han = Convert.ToDateTime(row.Cells["HanTra"].Value);
            object? nt = row.Cells["NgayTra"].Value;
            DateTime moc = (nt == null || nt == DBNull.Value) ? DateTime.Today : Convert.ToDateTime(nt);

            chkTre.Checked = moc.Date > han.Date;  
            chkHu.Checked = false;
            chkMat.Checked = false;
            lblChon.Text = "#" + _maPhieu + " - " + Convert.ToString(row.Cells["TenTaiLieu"].Value)
                         + " (thẻ " + Convert.ToString(row.Cells["MaThe"].Value) + ")";
        }

        private void BtnLap_Click(object? sender, EventArgs e)
        {
            if (_maPhieu == null) { UiHelper.Warn("Chọn phiếu mượn cần lập phạt."); return; }
            if (cboNV.SelectedValue == null) { UiHelper.Warn("Chưa có nhân viên vai trò ThuThu để lập phiếu."); return; }
            if (!chkTre.Checked && !chkHu.Checked && !chkMat.Checked) { UiHelper.Warn("Chọn ít nhất một lý do phạt."); return; }
            if (numPhi.Value <= 0) { UiHelper.Warn("Phí phạt phải lớn hơn 0."); return; }
            if (!UiHelper.Confirm("Lập phiếu phạt " + numPhi.Value.ToString("N0") + " đ cho phiếu #" + _maPhieu + "?")) return;

            int maPhieu = _maPhieu.Value;
            int maNV = Convert.ToInt32(cboNV.SelectedValue);
            if (DbHelper.TryRun(() => _repo.Lap(maPhieu, maNV, chkTre.Checked, chkHu.Checked, chkMat.Checked,
                                                numPhi.Value, txtGhiChu.Text.Trim())))
            {
                UiHelper.Info("Đã lập phiếu phạt.");
                LoadData();
                ResetForm();
            }
        }

        private void ResetForm()
        {
            _maPhieu = null;
            lblChon.Text = "(chưa chọn phiếu)";
            chkTre.Checked = chkHu.Checked = chkMat.Checked = false;
            numPhi.Value = 0;
            txtGhiChu.Clear();
        }
    }
}