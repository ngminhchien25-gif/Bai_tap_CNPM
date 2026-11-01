using System;
using System.Windows.Forms;
using QuanLyKhachSanUI.Repositories;

namespace QuanLyKhachSanUI.Forms
{
    public class frmPhieuDatPhong : Form
    {
        private PhieuDatPhongRepository _repo = new PhieuDatPhongRepository();
        private KhachHangRepository _khRepo = new KhachHangRepository();
        private NhanVienRepository _nvRepo = new NhanVienRepository();

        private DataGridView dgv = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, ReadOnly = true };
        private TextBox txtSoPhieu = new TextBox(), txtTienCoc = new TextBox { Text = "0" };
        private ComboBox cboKhach = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList }, cboNV = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList }, cboKenh = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private DateTimePicker dtpNgayNhan = new DateTimePicker { Format = DateTimePickerFormat.Short }, dtpNgayTra = new DateTimePicker { Format = DateTimePickerFormat.Short };
        private Button btnLapPhieu = new Button { Text = "Lập Phiếu Đặt" };

        public frmPhieuDatPhong()
        {
            this.Text = "Lập Phiếu Đặt Phòng";
            this.Size = new System.Drawing.Size(850, 550);
            InitCustomControls();
            LoadDropdowns();
            LoadData();
        }

        private void InitCustomControls()
        {
            TableLayoutPanel panel = new TableLayoutPanel { Dock = DockStyle.Top, RowCount = 4, ColumnCount = 4, Height = 160, Padding = new Padding(10) };
            panel.Controls.Add(new Label { Text = "Số Phiếu:" }, 0, 0); panel.Controls.Add(txtSoPhieu, 1, 0);
            panel.Controls.Add(new Label { Text = "Khách Hàng:" }, 2, 0); panel.Controls.Add(cboKhach, 3, 0);
            panel.Controls.Add(new Label { Text = "NV Lễ Tân:" }, 0, 1); panel.Controls.Add(cboNV, 1, 1);
            panel.Controls.Add(new Label { Text = "Tiền Cọc:" }, 2, 1); panel.Controls.Add(txtTienCoc, 3, 1);
            panel.Controls.Add(new Label { Text = "Ngày Nhận:" }, 0, 2); panel.Controls.Add(dtpNgayNhan, 1, 2);
            panel.Controls.Add(new Label { Text = "Ngày Trả DK:" }, 2, 2); panel.Controls.Add(dtpNgayTra, 3, 2);
            panel.Controls.Add(new Label { Text = "Kênh Đặt:" }, 0, 3); panel.Controls.Add(cboKenh, 1, 3);

            cboKenh.Items.AddRange(new string[] { "Trực tiếp", "Điện thoại", "Website", "Agoda/Booking" });
            cboKenh.SelectedIndex = 0;
            btnLapPhieu.Dock = DockStyle.Top;

            this.Controls.Add(dgv);
            this.Controls.Add(btnLapPhieu);
            this.Controls.Add(panel);

            btnLapPhieu.Click += (s, e) => {
                if (string.IsNullOrEmpty(txtSoPhieu.Text) || cboKhach.SelectedValue == null || cboNV.SelectedValue == null)
                {
                    MessageBox.Show("Vui lòng điền đầy đủ thông tin!");
                    return;
                }
                decimal coc = decimal.TryParse(txtTienCoc.Text, out var res) ? res : 0;
                _repo.InsertPhieuDatPhong(txtSoPhieu.Text, cboKhach.SelectedValue.ToString(), cboNV.SelectedValue.ToString(), dtpNgayNhan.Value, dtpNgayTra.Value, coc, cboKenh.Text);
                LoadData();
                MessageBox.Show("Lập phiếu thành công!");
            };
        }

        private void LoadDropdowns()
        {
            cboKhach.DataSource = _khRepo.GetAllKhachHang();
            cboKhach.DisplayMember = "HoTen";
            cboKhach.ValueMember = "MaKhach";

            cboNV.DataSource = _nvRepo.GetAllNhanVien();
            cboNV.DisplayMember = "HoTen";
            cboNV.ValueMember = "MaNV";
        }

        private void LoadData() => dgv.DataSource = _repo.GetAllPhieuDatPhong();
    }
}