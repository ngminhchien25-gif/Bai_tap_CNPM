using System;
using System.Drawing;
using System.Windows.Forms;
using QuanLyCongTyDuLich.Services;

namespace QuanLyCongTyDuLich.Forms
{
    public class FrmPhanCongHDV : Form
    {
        private readonly PhanCongService svc = new PhanCongService();

        private TextBox txtMaPC;
        private ComboBox cboHDV, cboLoai, cboDoiTuong;
        private NumericUpDown numThuLao;
        private DataGridView dgv;
        private Button btnPhanCong;

        public FrmPhanCongHDV()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Phân công Hướng dẫn viên";
            this.Size = new Size(880, 500);
            this.StartPosition = FormStartPosition.CenterParent;

            Panel pnlTop = new Panel { Dock = DockStyle.Top, Height = 90 };

            txtMaPC = new TextBox { Location = new Point(80, 15), Size = new Size(90, 25) };
            cboHDV = new ComboBox { Location = new Point(270, 15), Size = new Size(200, 25), DropDownStyle = ComboBoxStyle.DropDownList };

            cboLoai = new ComboBox { Location = new Point(80, 50), Size = new Size(90, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cboLoai.Items.AddRange(new object[] { QuyDinh.Le, QuyDinh.Doan });
            cboLoai.SelectedIndexChanged += (s, e) => FormHelper.Nap(cboDoiTuong, svc.LayDoiTuong(cboLoai.Text), "HienThi", "Ma");

            cboDoiTuong = new ComboBox { Location = new Point(270, 50), Size = new Size(280, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            numThuLao = new NumericUpDown { Location = new Point(630, 50), Size = new Size(110, 25), Maximum = 50000000, Increment = 100000, Value = 1500000 };

            btnPhanCong = new Button { Text = "Phân công", Location = new Point(755, 48), Size = new Size(90, 30) };
            btnPhanCong.Click += (s, e) =>
            {
                if (FormHelper.Bao(svc.PhanCong(txtMaPC.Text, FormHelper.Gia(cboHDV), cboLoai.Text, FormHelper.Gia(cboDoiTuong), numThuLao.Value)))
                {
                    TaiData();
                    FormHelper.Nap(cboDoiTuong, svc.LayDoiTuong(cboLoai.Text), "HienThi", "Ma");
                }
            };

            pnlTop.Controls.AddRange(new Control[] { new Label { Text = "Mã PC:", Location = new Point(10, 18) }, txtMaPC, new Label { Text = "Chọn HDV:", Location = new Point(190, 18) }, cboHDV, new Label { Text = "Loại:", Location = new Point(10, 53) }, cboLoai, new Label { Text = "Chuyến/Đoàn:", Location = new Point(180, 53) }, cboDoiTuong, new Label { Text = "Thù lao:", Location = new Point(565, 53) }, numThuLao, btnPhanCong });

            dgv = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };

            this.Controls.Add(dgv);
            this.Controls.Add(pnlTop);

            this.Load += FrmPhanCongHDV_Load;
        }

        private void FrmPhanCongHDV_Load(object sender, EventArgs e)
        {
            FormHelper.Nap(cboHDV, svc.LayHDVDangLam(), "HienThi", "MaHDV");
            cboLoai.SelectedIndex = 0;
            TaiData();
        }

        private void TaiData()
        {
            dgv.DataSource = svc.LayDanhSach();
        }
    }
}