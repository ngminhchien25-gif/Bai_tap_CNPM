using System.Data;
using QuanLyThuVien.Repositories;

namespace QuanLyThuVien.Forms
{
    public class frmPhieuMuon : Form
    {
        private readonly PhieuMuonRepository _repo = new();
        private readonly TaiLieuRepository _tlRepo = new();
        private readonly TheThuVienRepository _theRepo = new();   
        private ComboBox cboMaThe;
        private ComboBox cboTaiLieu;
        private DateTimePicker dtpHanTra;
        private Button btnMuon, btnTra, btnLamMoi;
        private DataGridView dgv;

        public frmPhieuMuon()
        {
            Text = "Quản lý Phiếu Mượn";
            Width = 950;
            Height = 600;
            BuildUI();
            LoadMaThe();
            LoadTaiLieu();
            LoadData();
        }
        private void LoadMaThe()
        {
            var dt = _theRepo.GetActiveCards();
            cboMaThe.DataSource = dt;
            cboMaThe.DisplayMember = "MaThe";
            cboMaThe.ValueMember = "MaThe";
        }
        private void BuildUI()
        {
            var lblThe = new Label { Text = "Mã thẻ TV", Left = 20, Top = 20, Width = 100 };
            cboMaThe = new ComboBox { Left = 130, Top = 20, Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };

            var lblTL = new Label { Text = "Tài liệu", Left = 300, Top = 20, Width = 80 };
            cboTaiLieu = new ComboBox { Left = 380, Top = 20, Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };

            var lblHan = new Label { Text = "Hạn trả", Left = 700, Top = 20, Width = 60 };
            dtpHanTra = new DateTimePicker { Left = 760, Top = 20, Width = 150, Value = DateTime.Now.AddDays(14) };

            btnMuon = new Button { Text = "Xác nhận mượn", Left = 20, Top = 60, Width = 130 };
            btnTra = new Button { Text = "Xác nhận trả (chọn dòng)", Left = 160, Top = 60, Width = 180 };
            btnLamMoi = new Button { Text = "Làm mới", Left = 350, Top = 60, Width = 90 };

            btnMuon.Click += BtnMuon_Click;
            btnTra.Click += BtnTra_Click;
            btnLamMoi.Click += (s, e) => LoadData();

            dgv = new DataGridView
            {
                Left = 20, Top = 110, Width = 890, Height = 420,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            Controls.AddRange(new Control[]
            {
                lblThe, cboMaThe, lblTL, cboTaiLieu, lblHan, dtpHanTra,
                btnMuon, btnTra, btnLamMoi, dgv
            });
        }

        private void LoadTaiLieu()
        {
            var dt = _tlRepo.GetAll();
            cboTaiLieu.DataSource = dt;
            cboTaiLieu.DisplayMember = "TenTaiLieu";
            cboTaiLieu.ValueMember = "MaTaiLieu";
        }

        private void LoadData()
        {
            try { dgv.DataSource = _repo.GetAll(); }
            catch (Exception ex) { MessageBox.Show("Lỗi kết nối CSDL: " + ex.Message); }
        }

        private void BtnMuon_Click(object? sender, EventArgs e)
        {
            if (cboMaThe.SelectedValue == null || cboTaiLieu.SelectedValue == null)
            {
                MessageBox.Show("Chọn Mã thẻ và Tài liệu.");
                return;
            }

            try
            {
                _repo.Muon(cboMaThe.SelectedValue.ToString(), (int)cboTaiLieu.SelectedValue!, dtpHanTra.Value);
                MessageBox.Show("Mượn sách thành công!");
                LoadTaiLieu(); // refresh tồn kho trong combobox
                LoadData();
                cboMaThe.SelectedIndex = -1;
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Không thể mượn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        private void BtnTra_Click(object? sender, EventArgs e)
        {
            if (dgv.CurrentRow == null) { MessageBox.Show("Chọn phiếu mượn cần trả."); return; }

            var row = dgv.CurrentRow;
            if (row.Cells["TrangThai"].Value.ToString() == "DaTra")
            {
                MessageBox.Show("Phiếu này đã trả rồi.");
                return;
            }

            int maPhieu = Convert.ToInt32(row.Cells["MaPhieu"].Value);
            int maTaiLieu = Convert.ToInt32(row.Cells["MaTaiLieu"].Value);

            _repo.Tra(maPhieu, maTaiLieu);
            MessageBox.Show("Trả sách thành công!");
            LoadTaiLieu();
            LoadData();
        }
    }
}