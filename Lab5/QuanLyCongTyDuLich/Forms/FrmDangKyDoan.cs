using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QuanLyCongTyDuLich.Services;

namespace QuanLyCongTyDuLich.Forms
{
    public class FrmDangKyDoan : Form
    {
        private readonly DangKyDoanService svc = new DangKyDoanService();
        private readonly BindingList<ThanhVienDoanItem> thanhVienList = new BindingList<ThanhVienDoanItem>();

        private TextBox txtMaDoan, txtTenCQ, txtDiaChi, txtDT, txtDaiDien, txtSo, txtDon;
        private ComboBox cboTour;
        private DateTimePicker dtDi;
        private NumericUpDown numNguoi, numCoc;
        private CheckBox chkBH;
        private Label lblKetThuc, lblTong;
        private DataGridView dgvTV, dgvPhieu;
        private Button btnDangKy, btnHuy;

        public FrmDangKyDoan()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Phiếu Đăng ký theo Đoàn";
            this.Size = new Size(920, 650);
            this.StartPosition = FormStartPosition.CenterParent;

            TableLayoutPanel pnlTop = new TableLayoutPanel { Dock = DockStyle.Top, Height = 180, ColumnCount = 2 };
            pnlTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            pnlTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            // Group 1: Thông tin đoàn
            GroupBox grpDoan = new GroupBox { Text = "Thông tin đoàn khách", Dock = DockStyle.Fill };
            txtMaDoan = new TextBox { Location = new Point(80, 20), Size = new Size(100, 25) };
            txtTenCQ = new TextBox { Location = new Point(80, 50), Size = new Size(330, 25) };
            txtDiaChi = new TextBox { Location = new Point(80, 80), Size = new Size(330, 25) };
            txtDT = new TextBox { Location = new Point(80, 110), Size = new Size(110, 25) };
            txtDaiDien = new TextBox { Location = new Point(270, 110), Size = new Size(140, 25) };
            grpDoan.Controls.AddRange(new Control[] { new Label { Text = "Mã đoàn:", Location = new Point(10, 23) }, txtMaDoan, new Label { Text = "Cơ quan:", Location = new Point(10, 53) }, txtTenCQ, new Label { Text = "Địa chỉ:", Location = new Point(10, 83) }, txtDiaChi, new Label { Text = "SĐT:", Location = new Point(10, 113) }, txtDT, new Label { Text = "Đại diện:", Location = new Point(200, 113) }, txtDaiDien });

            // Group 2: Thông tin phiếu
            GroupBox grpPhieu = new GroupBox { Text = "Đăng ký Tour", Dock = DockStyle.Fill };
            txtSo = new TextBox { Location = new Point(70, 20), Size = new Size(90, 25) };
            cboTour = new ComboBox { Location = new Point(210, 20), Size = new Size(200, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cboTour.SelectedIndexChanged += TinhTong;

            dtDi = new DateTimePicker { Location = new Point(70, 50), Size = new Size(110, 25), Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(14) };
            dtDi.ValueChanged += TinhTong;

            numNguoi = new NumericUpDown { Location = new Point(250, 50), Size = new Size(50, 25), Minimum = 13, Value = 15 };
            numNguoi.ValueChanged += TinhTong;

            txtDon = new TextBox { Location = new Point(70, 80), Size = new Size(340, 25) };
            numCoc = new NumericUpDown { Location = new Point(70, 110), Size = new Size(110, 25), Maximum = 500000000, Increment = 1000000, Value = 10000000 };
            chkBH = new CheckBox { Text = "Mua bảo hiểm", Location = new Point(190, 112), AutoSize = true };
            chkBH.CheckedChanged += (s, e) => dgvTV.Enabled = chkBH.Checked;

            lblKetThuc = new Label { Text = "-", Location = new Point(70, 145), AutoSize = true };
            lblTong = new Label { Text = "0 đ", Location = new Point(250, 145), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.DarkBlue };

            grpPhieu.Controls.AddRange(new Control[] { new Label { Text = "Số phiếu:", Location = new Point(10, 23) }, txtSo, new Label { Text = "Tour:", Location = new Point(170, 23) }, cboTour, new Label { Text = "Ngày đi:", Location = new Point(10, 53) }, dtDi, new Label { Text = "Số người:", Location = new Point(190, 53) }, numNguoi, new Label { Text = "Nơi đón:", Location = new Point(10, 83) }, txtDon, new Label { Text = "Tiền cọc:", Location = new Point(10, 113) }, numCoc, chkBH, new Label { Text = "Kết thúc:", Location = new Point(10, 145) }, lblKetThuc, new Label { Text = "Tổng tiền:", Location = new Point(190, 145) }, lblTong });

            pnlTop.Controls.Add(grpDoan, 0, 0);
            pnlTop.Controls.Add(grpPhieu, 1, 0);

            // Mid Panel: Danh sách thành viên (bảo hiểm)
            Panel pnlMid = new Panel { Dock = DockStyle.Top, Height = 140 };
            pnlMid.Controls.Add(new Label { Text = "Danh sách người cùng đi (bắt buộc nhập đủ nếu Mua bảo hiểm):", Location = new Point(10, 5), AutoSize = true });
            dgvTV = new DataGridView { Location = new Point(10, 25), Size = new Size(740, 105), Enabled = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            dgvTV.DataSource = thanhVienList;

            btnDangKy = new Button { Text = "Lập phiếu đăng ký", Location = new Point(760, 25), Size = new Size(130, 40) };
            btnDangKy.Click += BtnDangKy_Click;

            btnHuy = new Button { Text = "Hủy phiếu (mất cọc)", Location = new Point(760, 80), Size = new Size(130, 40), BackColor = Color.MistyRose };
            btnHuy.Click += BtnHuy_Click;

            pnlMid.Controls.AddRange(new Control[] { dgvTV, btnDangKy, btnHuy });

            dgvPhieu = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };

            this.Controls.Add(dgvPhieu);
            this.Controls.Add(pnlMid);
            this.Controls.Add(pnlTop);

            this.Load += FrmDangKyDoan_Load;
        }

        private void FrmDangKyDoan_Load(object sender, EventArgs e)
        {
            FormHelper.Nap(cboTour, new TourService().LayTourMoBan(), "HienThi", "MaTour");
            TinhTong(sender, e);
            TaiData();
        }

        private void TinhTong(object sender, EventArgs e)
        {
            var r = cboTour.SelectedItem as System.Data.DataRowView;
            if (r == null) return;
            lblTong.Text = (Convert.ToDecimal(r["DonGiaKhach"]) * numNguoi.Value).ToString("N0") + " đ";
            lblKetThuc.Text = dtDi.Value.Date.AddDays(Convert.ToInt32(r["SoNgay"]) - 1).ToString("dd/MM/yyyy");
        }

        private void BtnDangKy_Click(object sender, EventArgs e)
        {
            dgvTV.EndEdit();
            var ds = thanhVienList.Where(x => x != null && !string.IsNullOrWhiteSpace(x.HoTen)).ToList();
            if (FormHelper.Bao(svc.DangKy(txtSo.Text, txtMaDoan.Text, txtTenCQ.Text, txtDiaChi.Text, txtDT.Text, txtDaiDien.Text, FormHelper.Gia(cboTour), dtDi.Value, (int)numNguoi.Value, txtDon.Text, chkBH.Checked, numCoc.Value, ds)))
            {
                thanhVienList.Clear();
                TaiData();
            }
        }

        private void BtnHuy_Click(object sender, EventArgs e)
        {
            string so = FormHelper.O(dgvPhieu, "SoDKDoan");
            if (string.IsNullOrEmpty(so)) return;
            if (MessageBox.Show("Hủy phiếu đoàn sẽ mất tiền cọc. Xác nhận hủy phiếu " + so + "?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                if (FormHelper.Bao(svc.HuyDangKy(so))) TaiData();
            }
        }

        private void TaiData()
        {
            dgvPhieu.DataSource = svc.LayDanhSach();
        }
    }
}