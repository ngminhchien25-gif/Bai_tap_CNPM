using System;
using System.Windows.Forms;
using QuanLyKhachSanUI.Repositories;

namespace QuanLyKhachSanUI.Forms
{
    public class frmSuDungDichVu : Form
    {
        private SuDungDichVuRepository _repo = new SuDungDichVuRepository();
        private DichVuRepository _dvRepo = new DichVuRepository();
        private PhongRepository _phongRepo = new PhongRepository();
        private NhanVienRepository _nvRepo = new NhanVienRepository();
        private PhieuDatPhongRepository _phieuRepo = new PhieuDatPhongRepository();

        private DataGridView dgv = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true };
        private TextBox txtSoPhieuSD = new TextBox(), txtSoLuong = new TextBox { Text = "1" }, txtDonGia = new TextBox { Text = "0" };
        private ComboBox cboPhieuDat = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList }, cboPhong = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList }, cboDV = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList }, cboNV = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private Button btnGhiNhan = new Button { Text = "Ghi Nhận Sử Dụng Dịch Vụ" };

        public frmSuDungDichVu()
        {
            this.Text = "Quản Lý Sử Dụng Dịch Vụ";
            this.Size = new System.Drawing.Size(800, 550);
            InitCustomControls();
            LoadDropdowns();
            LoadData();
        }

        private void InitCustomControls()
        {
            TableLayoutPanel panel = new TableLayoutPanel { Dock = DockStyle.Top, RowCount = 4, ColumnCount = 4, Height = 150, Padding = new Padding(10) };
            panel.Controls.Add(new Label { Text = "Số Phiếu SDDV:" }, 0, 0); panel.Controls.Add(txtSoPhieuSD, 1, 0);
            panel.Controls.Add(new Label { Text = "Phiếu Đặt:" }, 2, 0); panel.Controls.Add(cboPhieuDat, 3, 0);
            panel.Controls.Add(new Label { Text = "Số Phòng:" }, 0, 1); panel.Controls.Add(cboPhong, 1, 1);
            panel.Controls.Add(new Label { Text = "Nhân Viên:" }, 2, 1); panel.Controls.Add(cboNV, 3, 1);
            panel.Controls.Add(new Label { Text = "Dịch Vụ:" }, 0, 2); panel.Controls.Add(cboDV, 1, 2);
            panel.Controls.Add(new Label { Text = "Số Lượng:" }, 2, 2); panel.Controls.Add(txtSoLuong, 3, 2);
            panel.Controls.Add(new Label { Text = "Đơn Giá:" }, 0, 3); panel.Controls.Add(txtDonGia, 1, 3);

            btnGhiNhan.Dock = DockStyle.Top;
            this.Controls.Add(dgv);
            this.Controls.Add(btnGhiNhan);
            this.Controls.Add(panel);

            cboDV.SelectedIndexChanged += (s, e) => {
                if (cboDV.SelectedItem is System.Data.DataRowView row)
                {
                    txtDonGia.Text = row["DonGia"].ToString();
                }
            };

            btnGhiNhan.Click += (s, e) => {
                try
                {
                    _repo.LapPhieuSuDungDichVuTransaction(
                        txtSoPhieuSD.Text,
                        cboPhieuDat.SelectedValue.ToString(),
                        cboPhong.SelectedValue.ToString(),
                        cboNV.SelectedValue.ToString(),
                        cboDV.SelectedValue.ToString(),
                        int.Parse(txtSoLuong.Text),
                        decimal.Parse(txtDonGia.Text));
                    LoadData();
                    MessageBox.Show("Ghi nhận dịch vụ thành công!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi: " + ex.Message);
                }
            };
        }

        private void LoadDropdowns()
        {
            cboPhieuDat.DataSource = _phieuRepo.GetAllPhieuDatPhong();
            cboPhieuDat.DisplayMember = "SoPhieuDat";
            cboPhieuDat.ValueMember = "SoPhieuDat";

            cboPhong.DataSource = _phongRepo.GetAllPhong();
            cboPhong.DisplayMember = "SoPhong";
            cboPhong.ValueMember = "SoPhong";

            cboNV.DataSource = _nvRepo.GetAllNhanVien();
            cboNV.DisplayMember = "HoTen";
            cboNV.ValueMember = "MaNV";

            cboDV.DataSource = _dvRepo.GetAllDichVu();
            cboDV.DisplayMember = "TenDV";
            cboDV.ValueMember = "MaDV";
        }

        private void LoadData() => dgv.DataSource = _repo.GetAllSuDungDichVu();
    }
}