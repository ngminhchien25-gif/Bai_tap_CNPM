using System;
using System.Drawing;
using System.Windows.Forms;
using QuanLyCongTyDuLich.Services;

namespace QuanLyCongTyDuLich.Forms
{
    public class FrmKetThucKhaoSat : Form
    {
        private readonly KetThucService svc = new KetThucService();

        private DataGridView dgvDoan, dgvKS;
        private TextBox txtSoTT, txtSoDKDoan, txtGhiChuTT;
        private DateTimePicker dtTT, dtGui, dtPH;
        private NumericUpDown numTien, numDiem;
        private ComboBox cboLoaiKS, cboDangKy;
        private TextBox txtMaKS, txtKSChon, txtGopY;

        public FrmKetThucKhaoSat()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Kết thúc Tour - Thanh toán & Khảo sát";
            this.Size = new Size(900, 580);
            this.StartPosition = FormStartPosition.CenterParent;

            TabControl tab = new TabControl { Dock = DockStyle.Fill };

            // Tab 1: Thanh toán đoàn sau tour
            TabPage tabTT = new TabPage("Thanh toán sau tour (Đoàn)");
            dgvDoan = new DataGridView { Location = new Point(10, 10), Size = new Size(860, 320), ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            dgvDoan.SelectionChanged += DgvDoan_SelectionChanged;

            txtSoTT = new TextBox { Location = new Point(70, 350), Size = new Size(90, 25) };
            txtSoDKDoan = new TextBox { Location = new Point(230, 350), Size = new Size(100, 25), ReadOnly = true };
            dtTT = new DateTimePicker { Location = new Point(410, 350), Size = new Size(110, 25), Format = DateTimePickerFormat.Short };
            numTien = new NumericUpDown { Location = new Point(590, 350), Size = new Size(120, 25), Maximum = 500000000, Increment = 1000000 };
            txtGhiChuTT = new TextBox { Location = new Point(70, 390), Size = new Size(640, 25) };

            Button btnThanhToan = new Button { Text = "Ghi nhận thanh toán", Location = new Point(720, 388), Size = new Size(150, 28) };
            btnThanhToan.Click += (s, e) => { if (FormHelper.Bao(svc.ThanhToanDoan(txtSoTT.Text, txtSoDKDoan.Text, dtTT.Value, numTien.Value, txtGhiChuTT.Text))) TaiTT(); };

            tabTT.Controls.AddRange(new Control[] { dgvDoan, new Label { Text = "Số TT:", Location = new Point(10, 353) }, txtSoTT, new Label { Text = "Số phiếu:", Location = new Point(170, 353) }, txtSoDKDoan, new Label { Text = "Ngày TT:", Location = new Point(345, 353) }, dtTT, new Label { Text = "Số tiền:", Location = new Point(530, 353) }, numTien, new Label { Text = "Ghi chú:", Location = new Point(10, 393) }, txtGhiChuTT, btnThanhToan });

            // Tab 2: Khảo sát khách hàng
            TabPage tabKS = new TabPage("Khảo sát khách hàng");
            cboLoaiKS = new ComboBox { Location = new Point(80, 15), Size = new Size(90, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cboLoaiKS.Items.AddRange(new object[] { QuyDinh.Le, QuyDinh.Doan });
            cboLoaiKS.SelectedIndexChanged += (s, e) => FormHelper.Nap(cboDangKy, svc.LayDangKyChoKhaoSat(cboLoaiKS.Text), "HienThi", "Ma");

            cboDangKy = new ComboBox { Location = new Point(280, 15), Size = new Size(220, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            txtMaKS = new TextBox { Location = new Point(560, 15), Size = new Size(80, 25) };
            dtGui = new DateTimePicker { Location = new Point(700, 15), Size = new Size(100, 25), Format = DateTimePickerFormat.Short };

            Button btnGui = new Button { Text = "Gửi phiếu", Location = new Point(810, 13), Size = new Size(70, 28) };
            btnGui.Click += (s, e) => { if (FormHelper.Bao(svc.GuiKhaoSat(txtMaKS.Text, cboLoaiKS.Text, FormHelper.Gia(cboDangKy), dtGui.Value))) TaiKS(); };

            dgvKS = new DataGridView { Location = new Point(10, 50), Size = new Size(860, 280), ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            dgvKS.SelectionChanged += (s, e) => txtKSChon.Text = FormHelper.O(dgvKS, "MaKhaoSat");

            txtKSChon = new TextBox { Location = new Point(80, 350), Size = new Size(80, 25), ReadOnly = true };
            dtPH = new DateTimePicker { Location = new Point(230, 350), Size = new Size(110, 25), Format = DateTimePickerFormat.Short };
            numDiem = new NumericUpDown { Location = new Point(410, 350), Size = new Size(50, 25), Minimum = 1, Maximum = 5, Value = 5 };
            txtGopY = new TextBox { Location = new Point(80, 390), Size = new Size(640, 25) };

            Button btnGhiPH = new Button { Text = "Ghi nhận góp ý", Location = new Point(730, 388), Size = new Size(140, 28) };
            btnGhiPH.Click += (s, e) => { if (FormHelper.Bao(svc.GhiPhanHoi(txtKSChon.Text, dtPH.Value, (int)numDiem.Value, txtGopY.Text))) TaiKS(); };

            tabKS.Controls.AddRange(new Control[] { new Label { Text = "Loại khách:", Location = new Point(10, 18) }, cboLoaiKS, new Label { Text = "Đăng ký:", Location = new Point(180, 18) }, cboDangKy, new Label { Text = "Mã KS:", Location = new Point(510, 18) }, txtMaKS, new Label { Text = "Ngày gửi:", Location = new Point(645, 18) }, dtGui, btnGui, dgvKS, new Label { Text = "Phiếu chọn:", Location = new Point(10, 353) }, txtKSChon, new Label { Text = "Ngày phản hồi:", Location = new Point(170, 353) }, dtPH, new Label { Text = "Điểm (1-5):", Location = new Point(350, 353) }, numDiem, new Label { Text = "Góp ý:", Location = new Point(10, 393) }, txtGopY, btnGhiPH });

            tab.TabPages.AddRange(new TabPage[] { tabTT, tabKS });
            this.Controls.Add(tab);

            this.Load += FrmKetThucKhaoSat_Load;
        }

        private void FrmKetThucKhaoSat_Load(object sender, EventArgs e)
        {
            cboLoaiKS.SelectedIndex = 0;
            TaiTT();
            TaiKS();
        }

        private void DgvDoan_SelectionChanged(object sender, EventArgs e)
        {
            txtSoDKDoan.Text = FormHelper.O(dgvDoan, "SoDKDoan");
            string con = FormHelper.O(dgvDoan, "ConLai");
            if (!string.IsNullOrEmpty(con)) numTien.Value = Math.Max(0, Math.Min(numTien.Maximum, Convert.ToDecimal(con)));
        }

        private void TaiTT() { dgvDoan.DataSource = svc.DoanCanThanhToan(); }
        private void TaiKS() { dgvKS.DataSource = svc.LayKhaoSat(); }
    }
}