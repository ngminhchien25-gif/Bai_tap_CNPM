using System;
using System.Drawing;
using System.Windows.Forms;
using QuanLyCongTyDuLich.Services;

namespace QuanLyCongTyDuLich.Forms
{
    public class FrmDanhMuc : Form
    {
        private readonly DanhMucService svc = new DanhMucService();

        private DataGridView dgvPT, dgvDB, dgvHDV, dgvDTQ;
        private TextBox txtPTMa, txtPTTen, txtPTGhiChu;
        private TextBox txtDBMa, txtDBTen, txtDBDiaChi, txtDBDT;
        private TextBox txtHDVMa, txtHDVTen, txtHDVDT;
        private NumericUpDown numLuong;
        private TextBox txtDTQMa, txtDTQTen, txtDTQDiaDiem, txtDTQNoiDung, txtDTQYNghia;

        public FrmDanhMuc()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Quản lý Danh mục Hệ thống";
            this.Size = new Size(880, 560);
            this.StartPosition = FormStartPosition.CenterParent;

            TabControl tab = new TabControl { Dock = DockStyle.Fill };

            // 1. Tab Phương tiện
            TabPage tabPT = new TabPage("Phương tiện");
            dgvPT = TaoDgv();
            txtPTMa = new TextBox { Location = new Point(70, 360), Size = new Size(100, 25) };
            txtPTTen = new TextBox { Location = new Point(240, 360), Size = new Size(180, 25) };
            txtPTGhiChu = new TextBox { Location = new Point(500, 360), Size = new Size(200, 25) };
            Button btnThemPT = new Button { Text = "Thêm", Location = new Point(720, 358), Size = new Size(80, 28) };
            btnThemPT.Click += (s, e) => { if (FormHelper.Bao(svc.ThemPhuongTien(txtPTMa.Text, txtPTTen.Text, txtPTGhiChu.Text))) TaiData(); };

            tabPT.Controls.AddRange(new Control[] { dgvPT, new Label { Text = "Mã PT:", Location = new Point(15, 363) }, txtPTMa, new Label { Text = "Tên PT:", Location = new Point(180, 363) }, txtPTTen, new Label { Text = "Ghi chú:", Location = new Point(435, 363) }, txtPTGhiChu, btnThemPT });

            // 2. Tab Điểm bán vé
            TabPage tabDB = new TabPage("Điểm bán vé");
            dgvDB = TaoDgv();
            txtDBMa = new TextBox { Location = new Point(70, 360), Size = new Size(90, 25) };
            txtDBTen = new TextBox { Location = new Point(220, 360), Size = new Size(150, 25) };
            txtDBDiaChi = new TextBox { Location = new Point(430, 360), Size = new Size(180, 25) };
            txtDBDT = new TextBox { Location = new Point(660, 360), Size = new Size(100, 25) };
            Button btnThemDB = new Button { Text = "Thêm", Location = new Point(770, 358), Size = new Size(80, 28) };
            btnThemDB.Click += (s, e) => { if (FormHelper.Bao(svc.ThemDiemBan(txtDBMa.Text, txtDBTen.Text, txtDBDiaChi.Text, txtDBDT.Text))) TaiData(); };

            tabDB.Controls.AddRange(new Control[] { dgvDB, new Label { Text = "Mã DB:", Location = new Point(15, 363) }, txtDBMa, new Label { Text = "Tên DB:", Location = new Point(170, 363) }, txtDBTen, new Label { Text = "Địa chỉ:", Location = new Point(380, 363) }, txtDBDiaChi, new Label { Text = "SĐT:", Location = new Point(620, 363) }, txtDBDT, btnThemDB });

            // 3. Tab Hướng dẫn viên
            TabPage tabHDV = new TabPage("Hướng dẫn viên");
            dgvHDV = TaoDgv();
            txtHDVMa = new TextBox { Location = new Point(80, 360), Size = new Size(90, 25) };
            txtHDVTen = new TextBox { Location = new Point(240, 360), Size = new Size(150, 25) };
            txtHDVDT = new TextBox { Location = new Point(440, 360), Size = new Size(100, 25) };
            numLuong = new NumericUpDown { Location = new Point(630, 360), Size = new Size(120, 25), Maximum = 100000000, Increment = 500000, Value = 9000000 };
            Button btnThemHDV = new Button { Text = "Thêm", Location = new Point(760, 358), Size = new Size(80, 28) };
            btnThemHDV.Click += (s, e) => { if (FormHelper.Bao(svc.ThemHDV(txtHDVMa.Text, txtHDVTen.Text, txtHDVDT.Text, numLuong.Value))) TaiData(); };

            tabHDV.Controls.AddRange(new Control[] { dgvHDV, new Label { Text = "Mã HDV:", Location = new Point(15, 363) }, txtHDVMa, new Label { Text = "Họ tên:", Location = new Point(180, 363) }, txtHDVTen, new Label { Text = "SĐT:", Location = new Point(400, 363) }, txtHDVDT, new Label { Text = "Lương CB:", Location = new Point(555, 363) }, numLuong, btnThemHDV });

            // 4. Tab Điểm tham quan
            TabPage tabDTQ = new TabPage("Điểm tham quan");
            dgvDTQ = TaoDgv();
            txtDTQMa = new TextBox { Location = new Point(70, 360), Size = new Size(80, 25) };
            txtDTQTen = new TextBox { Location = new Point(210, 360), Size = new Size(140, 25) };
            txtDTQDiaDiem = new TextBox { Location = new Point(410, 360), Size = new Size(120, 25) };
            txtDTQNoiDung = new TextBox { Location = new Point(600, 360), Size = new Size(120, 25) };
            txtDTQYNghia = new TextBox { Location = new Point(70, 400), Size = new Size(650, 25) };
            Button btnThemDTQ = new Button { Text = "Thêm", Location = new Point(735, 398), Size = new Size(80, 28) };
            btnThemDTQ.Click += (s, e) => { if (FormHelper.Bao(svc.ThemDiemThamQuan(txtDTQMa.Text, txtDTQTen.Text, txtDTQDiaDiem.Text, txtDTQNoiDung.Text, txtDTQYNghia.Text))) TaiData(); };

            tabDTQ.Controls.AddRange(new Control[] { dgvDTQ, new Label { Text = "Mã TQ:", Location = new Point(15, 363) }, txtDTQMa, new Label { Text = "Tên TQ:", Location = new Point(155, 363) }, txtDTQTen, new Label { Text = "Địa điểm:", Location = new Point(355, 363) }, txtDTQDiaDiem, new Label { Text = "Nội dung:", Location = new Point(535, 363) }, txtDTQNoiDung, new Label { Text = "Ý nghĩa:", Location = new Point(15, 403) }, txtDTQYNghia, btnThemDTQ });

            tab.TabPages.AddRange(new TabPage[] { tabPT, tabDB, tabHDV, tabDTQ });
            this.Controls.Add(tab);
            this.Load += (s, e) => TaiData();
        }

        private DataGridView TaoDgv()
        {
            return new DataGridView { Location = new Point(10, 10), Size = new Size(840, 330), ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
        }

        private void TaiData()
        {
            dgvPT.DataSource = svc.LayPhuongTien();
            dgvDB.DataSource = svc.LayDiemBan();
            dgvHDV.DataSource = svc.LayHDV();
            dgvDTQ.DataSource = svc.LayDiemThamQuan();
        }
    }
}