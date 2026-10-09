using System;
using System.Drawing;
using System.Windows.Forms;
using QuanLyCongTyDuLich.Services;

namespace QuanLyCongTyDuLich.Forms
{
    public class FrmTour : Form
    {
        private readonly TourService svc = new TourService();
        private readonly DanhMucService dm = new DanhMucService();

        private ComboBox cboTour;
        private DataGridView dgvTour, dgvDiemDung, dgvChang, dgvTQ;
        private TextBox txtMa, txtTen, txtMoTa, txtDiemDung, txtGhiChuDD, txtGhiChuPT;
        private NumericUpDown numNgay, numDem, numGia, numThuTu, numSao, numChang, numThuTuTQ;
        private CheckBox chkDoiPT, chkAn, chkKS;
        private ComboBox cboPT, cboDTQ;

        public FrmTour()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Quản lý Tour & Cấu hình Hành trình";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            Panel pnlTop = new Panel { Dock = DockStyle.Top, Height = 40 };
            pnlTop.Controls.Add(new Label { Text = "Tour đang chọn:", Location = new Point(15, 12), AutoSize = true });
            cboTour = new ComboBox { Location = new Point(120, 8), Size = new Size(300, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cboTour.SelectedIndexChanged += (s, e) => { if (cboTour.ValueMember == "MaTour") TaiChiTiet(); };
            pnlTop.Controls.Add(cboTour);

            TabControl tab = new TabControl { Dock = DockStyle.Fill };

            // Tab 1: Tour
            TabPage tabTour = new TabPage("Thông tin Tour");
            dgvTour = TaoDgv();
            txtMa = new TextBox { Location = new Point(70, 360), Size = new Size(90, 25) };
            txtTen = new TextBox { Location = new Point(230, 360), Size = new Size(180, 25) };
            numNgay = new NumericUpDown { Location = new Point(480, 360), Size = new Size(50, 25), Minimum = 1, Value = 3 };
            numDem = new NumericUpDown { Location = new Point(590, 360), Size = new Size(50, 25), Minimum = 0, Value = 2 };
            numGia = new NumericUpDown { Location = new Point(710, 360), Size = new Size(120, 25), Maximum = 100000000, Increment = 100000, Value = 2500000 };
            txtMoTa = new TextBox { Location = new Point(70, 400), Size = new Size(630, 25) };
            Button btnThemTour = new Button { Text = "Thêm Tour", Location = new Point(710, 398), Size = new Size(120, 28) };
            btnThemTour.Click += (s, e) => { if (FormHelper.Bao(svc.ThemTour(txtMa.Text, txtTen.Text, (int)numNgay.Value, (int)numDem.Value, numGia.Value, txtMoTa.Text))) TaiTour(); };

            tabTour.Controls.AddRange(new Control[] { dgvTour, new Label { Text = "Mã tour:", Location = new Point(10, 363) }, txtMa, new Label { Text = "Tên tour:", Location = new Point(170, 363) }, txtTen, new Label { Text = "Số ngày:", Location = new Point(420, 363) }, numNgay, new Label { Text = "Số đêm:", Location = new Point(540, 363) }, numDem, new Label { Text = "Đơn giá:", Location = new Point(650, 363) }, numGia, new Label { Text = "Mô tả:", Location = new Point(10, 403) }, txtMoTa, btnThemTour });

            // Tab 2: Điểm dừng
            TabPage tabDD = new TabPage("Điểm dừng");
            dgvDiemDung = TaoDgv();
            numThuTu = new NumericUpDown { Location = new Point(60, 360), Size = new Size(50, 25), Minimum = 1, Value = 1 };
            txtDiemDung = new TextBox { Location = new Point(180, 360), Size = new Size(150, 25) };
            chkDoiPT = new CheckBox { Text = "Đổi PT", Location = new Point(340, 362), AutoSize = true };
            chkAn = new CheckBox { Text = "Có nơi ăn", Location = new Point(420, 362), AutoSize = true };
            chkKS = new CheckBox { Text = "Có KS", Location = new Point(500, 362), AutoSize = true };
            numSao = new NumericUpDown { Location = new Point(620, 360), Size = new Size(50, 25), Minimum = 2, Maximum = 5, Value = 3, Enabled = false };
            chkKS.CheckedChanged += (s, e) => numSao.Enabled = chkKS.Checked;
            txtGhiChuDD = new TextBox { Location = new Point(60, 400), Size = new Size(610, 25) };
            Button btnThemDD = new Button { Text = "Thêm điểm dừng", Location = new Point(680, 398), Size = new Size(150, 28) };
            btnThemDD.Click += (s, e) => { if (FormHelper.Bao(svc.ThemDiemDung(FormHelper.Gia(cboTour), (int)numThuTu.Value, txtDiemDung.Text, chkDoiPT.Checked, chkAn.Checked, chkKS.Checked, (int)numSao.Value, txtGhiChuDD.Text))) TaiChiTiet(); };

            tabDD.Controls.AddRange(new Control[] { dgvDiemDung, new Label { Text = "Thứ tự:", Location = new Point(10, 363) }, numThuTu, new Label { Text = "Tên điểm:", Location = new Point(120, 363) }, txtDiemDung, chkDoiPT, chkAn, chkKS, new Label { Text = "Hạng sao:", Location = new Point(560, 363) }, numSao, new Label { Text = "Ghi chú:", Location = new Point(10, 403) }, txtGhiChuDD, btnThemDD });

            // Tab 3: Phương tiện chặng
            TabPage tabChang = new TabPage("Phương tiện theo chặng");
            dgvChang = TaoDgv();
            numChang = new NumericUpDown { Location = new Point(70, 360), Size = new Size(50, 25), Minimum = 1, Value = 1 };
            cboPT = new ComboBox { Location = new Point(210, 360), Size = new Size(180, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            txtGhiChuPT = new TextBox { Location = new Point(460, 360), Size = new Size(220, 25) };
            Button btnThemChang = new Button { Text = "Gắn phương tiện", Location = new Point(690, 358), Size = new Size(140, 28) };
            btnThemChang.Click += (s, e) => { if (FormHelper.Bao(svc.ThemChang(FormHelper.Gia(cboTour), (int)numChang.Value, FormHelper.Gia(cboPT), txtGhiChuPT.Text))) TaiChiTiet(); };

            tabChang.Controls.AddRange(new Control[] { dgvChang, new Label { Text = "Chặng thứ:", Location = new Point(10, 363) }, numChang, new Label { Text = "Phương tiện:", Location = new Point(130, 363) }, cboPT, new Label { Text = "Ghi chú:", Location = new Point(400, 363) }, txtGhiChuPT, btnThemChang });

            // Tab 4: Điểm tham quan
            TabPage tabTQ = new TabPage("Điểm tham quan");
            dgvTQ = TaoDgv();
            cboDTQ = new ComboBox { Location = new Point(110, 360), Size = new Size(220, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            numThuTuTQ = new NumericUpDown { Location = new Point(390, 360), Size = new Size(50, 25), Minimum = 1, Value = 1 };
            Button btnThemTQ = new Button { Text = "Gắn điểm tham quan", Location = new Point(460, 358), Size = new Size(160, 28) };
            btnThemTQ.Click += (s, e) => { if (FormHelper.Bao(svc.ThemDiemTQTour(FormHelper.Gia(cboTour), FormHelper.Gia(cboDTQ), (int)numThuTuTQ.Value))) TaiChiTiet(); };

            tabTQ.Controls.AddRange(new Control[] { dgvTQ, new Label { Text = "Điểm tham quan:", Location = new Point(10, 363) }, cboDTQ, new Label { Text = "Thứ tự:", Location = new Point(340, 363) }, numThuTuTQ, btnThemTQ });

            tab.TabPages.AddRange(new TabPage[] { tabTour, tabDD, tabChang, tabTQ });
            this.Controls.Add(tab);
            this.Controls.Add(pnlTop);

            this.Load += FrmTour_Load;
        }

        private DataGridView TaoDgv()
        {
            return new DataGridView { Location = new Point(10, 10), Size = new Size(860, 330), ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
        }

        private void FrmTour_Load(object sender, EventArgs e)
        {
            FormHelper.Nap(cboPT, dm.LayPhuongTien(), "TenPT", "MaPT");
            FormHelper.Nap(cboDTQ, dm.LayDiemThamQuan(), "TenDiemTQ", "MaDiemTQ");
            TaiTour();
        }

        private void TaiTour()
        {
            dgvTour.DataSource = svc.LayTour();
            FormHelper.Nap(cboTour, svc.LayTourMoBan(), "HienThi", "MaTour");
            TaiChiTiet();
        }

        private void TaiChiTiet()
        {
            string ma = FormHelper.Gia(cboTour);
            dgvDiemDung.DataSource = svc.LayDiemDung(ma);
            dgvChang.DataSource = svc.LayChang(ma);
            dgvTQ.DataSource = svc.LayDiemTQTour(ma);
        }
    }
}