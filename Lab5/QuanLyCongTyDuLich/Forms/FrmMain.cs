using System;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLyCongTyDuLich.Forms
{
    public class FrmMain : Form
    {
        private Button btnDanhMuc, btnTour, btnChuyenLe, btnDangKyLe;
        private Button btnDangKyDoan, btnPhanCong, btnKetThuc, btnThongKe, btnThoat;

        public FrmMain()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Quản lý công ty du lịch Văn Hóa Việt";
            this.Size = new Size(820, 560);
            this.MinimumSize = new Size(800, 520);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 240);
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);

            // 1. TIÊU ĐỀ TRÊN CÙNG
            Label lblTitle = new Label
            {
                Text = "CÔNG TY DU LỊCH VĂN HÓA VIỆT",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(10, 40, 100),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 80
            };

            // 2. KHU VỰC NÚT CHỨC NĂNG LỚN (8 Ô ĐẦY ĐẶN BỐ CỤC 2 CỘT)
            TableLayoutPanel pnlGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 4,
                Padding = new Padding(40, 10, 40, 10)
            };

            pnlGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            pnlGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            pnlGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            pnlGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            pnlGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            pnlGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));

            // Khởi tạo 8 nút chức năng chính theo đúng tài liệu
            btnDanhMuc = TaoButtonChucNang("Danh mục");
            btnTour = TaoButtonChucNang("Tour - hành trình");
            btnChuyenLe = TaoButtonChucNang("Lịch chuyến khách lẻ");
            btnDangKyLe = TaoButtonChucNang("Đăng ký khách lẻ");
            btnDangKyDoan = TaoButtonChucNang("Đăng ký theo đoàn");
            btnPhanCong = TaoButtonChucNang("Phân công hướng dẫn viên");
            btnKetThuc = TaoButtonChucNang("Kết thúc tour - khảo sát");
            btnThongKe = TaoButtonChucNang("Lương - thống kê");

            // Đưa các nút vào vị trí Grid 2x4
            pnlGrid.Controls.Add(btnDanhMuc, 0, 0);
            pnlGrid.Controls.Add(btnTour, 1, 0);
            pnlGrid.Controls.Add(btnChuyenLe, 0, 1);
            pnlGrid.Controls.Add(btnDangKyLe, 1, 1);
            pnlGrid.Controls.Add(btnDangKyDoan, 0, 2);
            pnlGrid.Controls.Add(btnPhanCong, 1, 2);
            pnlGrid.Controls.Add(btnKetThuc, 0, 3);
            pnlGrid.Controls.Add(btnThongKe, 1, 3);

            // 3. KHU VỰC NÚT THOÁT DƯỚI CÙNG
            Panel pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 90,
                Padding = new Padding(220, 15, 220, 20)
            };

            btnThoat = new Button
            {
                Text = "Thoát",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Regular),
                BackColor = Color.FromArgb(230, 230, 230),
                Cursor = Cursors.Hand
            };

            pnlBottom.Controls.Add(btnThoat);

            // 4. ĐĂNG KÝ SỰ KIỆN CHUYỂN FORM (SHOWDIALOG)
            btnDanhMuc.Click += (s, e) => MoForm(new FrmDanhMuc());
            btnTour.Click += (s, e) => MoForm(new FrmTour());
            btnChuyenLe.Click += (s, e) => MoForm(new FrmChuyenLe());
            btnDangKyLe.Click += (s, e) => MoForm(new FrmDangKyLe());
            btnDangKyDoan.Click += (s, e) => MoForm(new FrmDangKyDoan());
            btnPhanCong.Click += (s, e) => MoForm(new FrmPhanCongHDV());
            btnKetThuc.Click += (s, e) => MoForm(new FrmKetThucKhaoSat());
            btnThongKe.Click += (s, e) => MoForm(new FrmLuongThongKe());

            btnThoat.Click += (s, e) =>
            {
                if (MessageBox.Show("Bạn có muốn thoát ứng dụng?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Close();
                }
            };

            // Bố cục các Container vào Form
            this.Controls.Add(pnlGrid);
            this.Controls.Add(pnlBottom);
            this.Controls.Add(lblTitle);
        }

        // Hàm tạo nút chức năng khối xám đầy đặn đúng theo giao diện mẫu
        private Button TaoButtonChucNang(string text)
        {
            Button btn = new Button
            {
                Text = text,
                Dock = DockStyle.Fill,
                Margin = new Padding(12, 8, 12, 8),
                Font = new Font("Segoe UI", 10.5F, FontStyle.Regular),
                BackColor = Color.FromArgb(225, 225, 225),
                ForeColor = Color.Black,
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Standard
            };
            return btn;
        }

        private void MoForm(Form f)
        {
            using (f) f.ShowDialog(this);
        }
    }
}