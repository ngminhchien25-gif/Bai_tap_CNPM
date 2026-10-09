using System;
using System.Drawing;
using System.Windows.Forms;
using QuanLyCongTyDuLich.Services;

namespace QuanLyCongTyDuLich.Forms
{
    public class FrmLuongThongKe : Form
    {
        private readonly ThongKeService svc = new ThongKeService();

        private NumericUpDown numThang, numNam;
        private DateTimePicker dtTu, dtDen;
        private DataGridView dgvLuong, dgvTongHop;
        private Button btnLuong, btnThongKe, btnDong;

        public FrmLuongThongKe()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Lương Hướng dẫn viên & Thống kê Tổng hợp";
            this.Size = new Size(900, 560);
            this.StartPosition = FormStartPosition.CenterParent;

            TabControl tab = new TabControl { Dock = DockStyle.Fill };

            // ==========================================
            // TAB 1: LƯƠNG HƯỚNG DẪN VIÊN
            // ==========================================
            TabPage tabLuong = new TabPage("Lương Hướng dẫn viên");
            Panel pnlTopL = new Panel { Dock = DockStyle.Top, Height = 60 };

            numThang = new NumericUpDown { Location = new Point(65, 16), Size = new Size(55, 25), Minimum = 1, Maximum = 12, Value = DateTime.Today.Month };
            numNam = new NumericUpDown { Location = new Point(175, 16), Size = new Size(75, 25), Minimum = 2020, Maximum = 2030, Value = DateTime.Today.Year };

            btnLuong = new Button { Text = "Tính lương", Location = new Point(270, 14), Size = new Size(110, 30), BackColor = Color.LightSkyBlue };
            btnLuong.Click += (s, e) => TinhLuong();

            Label lblCongThuc = new Label
            {
                Text = "(*) Lương = Lương căn bản + Tổng thù lao các tour kết thúc trong tháng",
                Location = new Point(400, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.DarkSlateGray
            };

            pnlTopL.Controls.AddRange(new Control[] {
                new Label { Text = "Tháng:", Location = new Point(15, 20), AutoSize = true }, numThang,
                new Label { Text = "Năm:", Location = new Point(135, 20), AutoSize = true }, numNam,
                btnLuong, lblCongThuc
            });

            dgvLuong = TaoDataGridView();
            tabLuong.Controls.Add(dgvLuong);
            tabLuong.Controls.Add(pnlTopL);

            // ==========================================
            // TAB 2: THỐNG KÊ TỔNG HỢP
            // ==========================================
            TabPage tabTK = new TabPage("Thống kê Tổng hợp");
            Panel pnlTopTK = new Panel { Dock = DockStyle.Top, Height = 60 };

            dtTu = new DateTimePicker { Location = new Point(75, 16), Size = new Size(120, 25), Format = DateTimePickerFormat.Short, Value = new DateTime(DateTime.Today.Year, 1, 1) };
            dtDen = new DateTimePicker { Location = new Point(285, 16), Size = new Size(120, 25), Format = DateTimePickerFormat.Short, Value = DateTime.Today };

            btnThongKe = new Button { Text = "Xem báo cáo", Location = new Point(420, 14), Size = new Size(110, 30), BackColor = Color.LightGreen };
            btnThongKe.Click += (s, e) => XemBaoCao();

            pnlTopTK.Controls.AddRange(new Control[] {
                new Label { Text = "Từ ngày:", Location = new Point(15, 20), AutoSize = true }, dtTu,
                new Label { Text = "Đến ngày:", Location = new Point(215, 20), AutoSize = true }, dtDen,
                btnThongKe
            });

            dgvTongHop = TaoDataGridView();
            tabTK.Controls.Add(dgvTongHop);
            tabTK.Controls.Add(pnlTopTK);

            // Panel Bottom: Nút đóng
            Panel pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 45 };
            btnDong = new Button { Text = "Đóng", Location = new Point(780, 8), Size = new Size(90, 30) };
            btnDong.Click += (s, e) => Close();
            pnlBottom.Controls.Add(btnDong);

            tab.TabPages.AddRange(new TabPage[] { tabLuong, tabTK });
            this.Controls.Add(tab);
            this.Controls.Add(pnlBottom);

            this.Load += (s, e) => { TinhLuong(); XemBaoCao(); };
        }

        private DataGridView TaoDataGridView()
        {
            return new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
        }

        private void TinhLuong()
        {
            dgvLuong.DataSource = svc.LuongHDV((int)numThang.Value, (int)numNam.Value);
            DinhDangLuoiLuong();
        }

        private void XemBaoCao()
        {
            if (dtDen.Value.Date < dtTu.Value.Date)
            {
                MessageBox.Show("Ngày kết thúc ('Đến ngày') không được trước ngày bắt đầu ('Từ ngày').", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            dgvTongHop.DataSource = svc.TongHop(dtTu.Value, dtDen.Value);
            DinhDangLuoiThongKe();
        }

        private void DinhDangLuoiLuong()
        {
            if (dgvLuong.Columns.Count == 0) return;

            dgvLuong.Columns["MaHDV"].HeaderText = "Mã HDV";
            dgvLuong.Columns["HoTen"].HeaderText = "Họ và Tên";
            dgvLuong.Columns["LuongCoBan"].HeaderText = "Lương Căn Bản";
            dgvLuong.Columns["SoTour"].HeaderText = "Số Tour";
            dgvLuong.Columns["LuongTheoTour"].HeaderText = "Thù Lao Tour";
            dgvLuong.Columns["TongLuong"].HeaderText = "Tổng Lương Tốt Nhận";

            // Định dạng tiền tệ
            dgvLuong.Columns["LuongCoBan"].DefaultCellStyle.Format = "N0";
            dgvLuong.Columns["LuongTheoTour"].DefaultCellStyle.Format = "N0";
            dgvLuong.Columns["TongLuong"].DefaultCellStyle.Format = "N0";

            dgvLuong.Columns["SoTour"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvLuong.Columns["LuongCoBan"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvLuong.Columns["LuongTheoTour"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvLuong.Columns["TongLuong"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }

        private void DinhDangLuoiThongKe()
        {
            if (dgvTongHop.Columns.Count == 0) return;

            dgvTongHop.Columns["ChiSo"].HeaderText = "Chỉ Số Thống Kê Kinh Doanh";
            dgvTongHop.Columns["SoLuong"].HeaderText = "Số Lượng";
            dgvTongHop.Columns["GiaTri"].HeaderText = "Tổng Giá Trị (VNĐ / Điểm TB)";

            dgvTongHop.Columns["SoLuong"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvTongHop.Columns["GiaTri"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvTongHop.Columns["GiaTri"].DefaultCellStyle.Format = "N2";
        }
    }
}