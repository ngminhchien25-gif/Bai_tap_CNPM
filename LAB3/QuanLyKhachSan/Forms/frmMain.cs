using System;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLyKhachSanUI.Forms
{
    public class Frmmain : Form
    {
        private MenuStrip menuStrip1;
        private ToolStripMenuItem mnuHeThong;
        private ToolStripMenuItem mnuNhanVien;
        private ToolStripMenuItem mnuKhachHang;
        private ToolStripMenuItem mnuDanhMuc;
        private ToolStripMenuItem mnuPhong;
        private ToolStripMenuItem mnuDichVu;
        private ToolStripMenuItem mnuNghiepVu;
        private ToolStripMenuItem mnuDatPhong;
        private ToolStripMenuItem mnuSuDungDV;
        private ToolStripMenuItem mnuDenBu;
        private ToolStripMenuItem mnuHoaDon;
        private ToolStripMenuItem mnuThongKe;

        public Frmmain()
        {
            InitCustomComponents();
        }

        private void InitCustomComponents()
        {
            this.menuStrip1 = new MenuStrip();
            this.mnuHeThong = new ToolStripMenuItem("Hệ thống");
            this.mnuNhanVien = new ToolStripMenuItem("Quản lý Nhân viên");
            this.mnuKhachHang = new ToolStripMenuItem("Quản lý Khách hàng");

            this.mnuDanhMuc = new ToolStripMenuItem("Danh mục");
            this.mnuPhong = new ToolStripMenuItem("Quản lý Phòng & Khu vực");
            this.mnuDichVu = new ToolStripMenuItem("Quản lý Dịch vụ");

            this.mnuNghiepVu = new ToolStripMenuItem("Nghiệp vụ");
            this.mnuDatPhong = new ToolStripMenuItem("Đặt phòng");
            this.mnuSuDungDV = new ToolStripMenuItem("Sử dụng Dịch vụ");
            this.mnuDenBu = new ToolStripMenuItem("Xử lý Hư hỏng / Đền bù");
            this.mnuHoaDon = new ToolStripMenuItem("Hóa đơn & Thanh toán");

            // Menu Thống kê: đặt ngang hàng các menu cha khác (dành cho vai trò Quản lý)
            this.mnuThongKe = new ToolStripMenuItem("Thống kê");

            // Thêm menu con vào menu cha
            this.mnuHeThong.DropDownItems.AddRange(new ToolStripItem[] { this.mnuNhanVien, this.mnuKhachHang });
            this.mnuDanhMuc.DropDownItems.AddRange(new ToolStripItem[] { this.mnuPhong, this.mnuDichVu });
            this.mnuNghiepVu.DropDownItems.AddRange(new ToolStripItem[] { this.mnuDatPhong, this.mnuSuDungDV, this.mnuDenBu, this.mnuHoaDon });

            this.menuStrip1.Items.AddRange(new ToolStripItem[] { this.mnuHeThong, this.mnuDanhMuc, this.mnuNghiepVu, this.mnuThongKe });

            // Cấu hình Form Chính
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Controls.Add(this.menuStrip1);
            this.Text = "Hệ Thống Quản Lý Khách Sạn";
            this.Size = new Size(1024, 768);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Đăng ký sự kiện Click cho các Menu
            this.mnuNhanVien.Click += (s, e) => OpenChildForm<frmNhanVien>();
            this.mnuKhachHang.Click += (s, e) => OpenChildForm<frmKhachHang>();
            this.mnuPhong.Click += (s, e) => OpenChildForm<frmPhong>();
            this.mnuDichVu.Click += (s, e) => OpenChildForm<frmDichVu>();
            this.mnuDatPhong.Click += (s, e) => OpenChildForm<frmPhieuDatPhong>();
            this.mnuSuDungDV.Click += (s, e) => OpenChildForm<frmSuDungDichVu>();
            this.mnuDenBu.Click += (s, e) => OpenChildForm<frmDenBu>();
            this.mnuHoaDon.Click += (s, e) => OpenChildForm<frmHoaDonThanhToan>();
            this.mnuThongKe.Click += (s, e) => OpenChildForm<frmThongKe>();
        }

        // Hàm mở Form con dạng MDI Child
        private void OpenChildForm<T>() where T : Form, new()
        {
            foreach (Form child in this.MdiChildren)
            {
                if (child is T)
                {
                    child.Activate();
                    return;
                }
            }
            T newForm = new T();
            newForm.MdiParent = this;
            newForm.Show();
        }
    }
}