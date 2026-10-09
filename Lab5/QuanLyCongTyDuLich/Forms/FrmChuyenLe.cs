using System;
using System.Drawing;
using System.Windows.Forms;
using QuanLyCongTyDuLich.Services;

namespace QuanLyCongTyDuLich.Forms
{
    public class FrmChuyenLe : Form
    {
        private readonly ChuyenLeService svc = new ChuyenLeService();
        private readonly TourService tourSvc = new TourService();

        private TextBox txtMa, txtDon;
        private ComboBox cboTour;
        private DateTimePicker dtDi;
        private Label lblNgayVe;
        private DataGridView dgv;
        private Button btnThem, btnDongDK;

        public FrmChuyenLe()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Quản lý Lịch chuyến Khách lẻ";
            this.Size = new Size(850, 520);
            this.StartPosition = FormStartPosition.CenterParent;

            Panel pnlInput = new Panel { Dock = DockStyle.Top, Height = 120 };

            txtMa = new TextBox { Location = new Point(80, 15), Size = new Size(120, 25) };
            cboTour = new ComboBox { Location = new Point(270, 15), Size = new Size(250, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cboTour.SelectedIndexChanged += TinhNgayVe;

            dtDi = new DateTimePicker { Location = new Point(80, 50), Size = new Size(120, 25), Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(7) };
            dtDi.ValueChanged += TinhNgayVe;

            lblNgayVe = new Label { Text = "-", Location = new Point(270, 55), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            txtDon = new TextBox { Location = new Point(80, 85), Size = new Size(440, 25), Text = "Nhà Văn hóa Thanh Niên, Quận 1" };

            btnThem = new Button { Text = "Tạo chuyến", Location = new Point(540, 13), Size = new Size(100, 30) };
            btnThem.Click += (s, e) => { if (FormHelper.Bao(svc.ThemChuyen(txtMa.Text, FormHelper.Gia(cboTour), dtDi.Value, txtDon.Text))) TaiData(); };

            btnDongDK = new Button { Text = "Đóng đăng ký", Location = new Point(540, 48), Size = new Size(100, 30) };
            btnDongDK.Click += (s, e) => { if (FormHelper.Bao(svc.DongDangKy(FormHelper.O(dgv, "MaChuyen")))) TaiData(); };

            pnlInput.Controls.AddRange(new Control[] { new Label { Text = "Mã chuyến:", Location = new Point(10, 18) }, txtMa, new Label { Text = "Tour:", Location = new Point(220, 18) }, cboTour, new Label { Text = "Ngày đi:", Location = new Point(10, 53) }, dtDi, new Label { Text = "Ngày về:", Location = new Point(220, 53) }, lblNgayVe, new Label { Text = "Nơi đón:", Location = new Point(10, 88) }, txtDon, btnThem, btnDongDK });

            dgv = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };

            this.Controls.Add(dgv);
            this.Controls.Add(pnlInput);

            this.Load += FrmChuyenLe_Load;
        }

        private void FrmChuyenLe_Load(object sender, EventArgs e)
        {
            FormHelper.Nap(cboTour, tourSvc.LayTourMoBan(), "HienThi", "MaTour");
            TinhNgayVe(sender, e);
            TaiData();
        }

        private void TinhNgayVe(object sender, EventArgs e)
        {
            var r = cboTour.SelectedItem as System.Data.DataRowView;
            lblNgayVe.Text = r == null ? "-" : dtDi.Value.Date.AddDays(Convert.ToInt32(r["SoNgay"]) - 1).ToString("dd/MM/yyyy");
        }

        private void TaiData()
        {
            dgv.DataSource = svc.LayChuyen();
        }
    }
}