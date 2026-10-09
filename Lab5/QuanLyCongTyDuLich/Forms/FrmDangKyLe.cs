using System;
using System.Drawing;
using System.Windows.Forms;
using QuanLyCongTyDuLich.Services;

namespace QuanLyCongTyDuLich.Forms
{
    public class FrmDangKyLe : Form
    {
        private readonly DangKyLeService svc = new DangKyLeService();

        private TextBox txtSo, txtTen, txtDT;
        private ComboBox cboChuyen, cboDiemBan;
        private NumericUpDown numNguoi;
        private Label lblThanhTien;
        private DataGridView dgv;
        private Button btnDangKy;

        public FrmDangKyLe()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Đăng ký Khách lẻ theo Chuyến";
            this.Size = new Size(880, 520);
            this.StartPosition = FormStartPosition.CenterParent;

            Panel pnlTop = new Panel { Dock = DockStyle.Top, Height = 100 };

            txtSo = new TextBox { Location = new Point(70, 15), Size = new Size(100, 25) };
            cboChuyen = new ComboBox { Location = new Point(230, 15), Size = new Size(250, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cboChuyen.SelectedIndexChanged += TinhTien;

            cboDiemBan = new ComboBox { Location = new Point(570, 15), Size = new Size(180, 25), DropDownStyle = ComboBoxStyle.DropDownList };

            txtTen = new TextBox { Location = new Point(70, 50), Size = new Size(150, 25) };
            txtDT = new TextBox { Location = new Point(260, 50), Size = new Size(110, 25) };
            numNguoi = new NumericUpDown { Location = new Point(440, 50), Size = new Size(50, 25), Minimum = 1, Maximum = 11, Value = 2 };
            numNguoi.ValueChanged += TinhTien;

            lblThanhTien = new Label { Text = "0 đ", Location = new Point(570, 53), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.DarkGreen };

            btnDangKy = new Button { Text = "Đăng ký & Thanh toán", Location = new Point(700, 48), Size = new Size(150, 30) };
            btnDangKy.Click += (s, e) => { if (FormHelper.Bao(svc.DangKy(txtSo.Text, FormHelper.Gia(cboChuyen), FormHelper.Gia(cboDiemBan), txtTen.Text, txtDT.Text, (int)numNguoi.Value))) TaiData(); };

            pnlTop.Controls.AddRange(new Control[] { new Label { Text = "Số DK:", Location = new Point(10, 18) }, txtSo, new Label { Text = "Chuyến:", Location = new Point(180, 18) }, cboChuyen, new Label { Text = "Điểm bán:", Location = new Point(495, 18) }, cboDiemBan, new Label { Text = "Họ tên:", Location = new Point(10, 53) }, txtTen, new Label { Text = "SĐT:", Location = new Point(225, 53) }, txtDT, new Label { Text = "Số người:", Location = new Point(380, 53) }, numNguoi, new Label { Text = "Thành tiền:", Location = new Point(500, 53) }, lblThanhTien, btnDangKy });

            dgv = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };

            this.Controls.Add(dgv);
            this.Controls.Add(pnlTop);

            this.Load += FrmDangKyLe_Load;
        }

        private void FrmDangKyLe_Load(object sender, EventArgs e)
        {
            FormHelper.Nap(cboChuyen, new ChuyenLeService().LayChuyenMo(), "HienThi", "MaChuyen");
            FormHelper.Nap(cboDiemBan, new DanhMucService().LayDiemBan(), "TenDiemBan", "MaDiemBan");
            TinhTien(sender, e);
            TaiData();
        }

        private void TinhTien(object sender, EventArgs e)
        {
            var r = cboChuyen.SelectedItem as System.Data.DataRowView;
            lblThanhTien.Text = r == null ? "0 đ" : (Convert.ToDecimal(r["DonGiaKhach"]) * numNguoi.Value).ToString("N0") + " đ";
        }

        private void TaiData()
        {
            dgv.DataSource = svc.LayDanhSach();
        }
    }
}