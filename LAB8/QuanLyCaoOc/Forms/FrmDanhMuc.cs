using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QuanLyCaoOc.Services;

namespace QuanLyCaoOc.Forms
{
    public class FrmDanhMuc : Form
    {
        public FrmDanhMuc()
        {
            Text = "Danh mục";
            ClientSize = new Size(1100, 620);
            StartPosition = FormStartPosition.CenterScreen;
            Font = Ui.FontChu;

            var tab = new TabControl { Name = "tabMain", Dock = DockStyle.Fill };
            Controls.Add(tab);

            // ---- Tầng ----
            Them(tab, "tabTang", "Tầng", new CrudPanel(
                new List<Field>
                {
                    new Field("MaTang", "Mã tầng") { IsKey = true },
                    new Field("TenTang", "Tên tầng"),
                    new Field("DoCao", "Độ cao (số tầng)", Kind.Number) { Min = 1, Max = 200 },
                    new Field("DonGiaM2", "Đơn giá /m²/tháng", Kind.Number)
                },
                DanhMucService.LayTang,
                v => DanhMucService.LuuTang(S(v, "MaTang"), S(v, "TenTang"), (int)D(v, "DoCao"), D(v, "DonGiaM2")),
                ma => DanhMucService.Xoa("Tang", "MaTang", ma)));

            // ---- Phòng ----
            Them(tab, "tabPhong", "Phòng", new CrudPanel(
                new List<Field>
                {
                    new Field("MaPhong", "Mã phòng") { IsKey = true },
                    new Field("MaTang", "Tầng", Kind.Combo) { Source = DanhMucService.LayTang, Display = "TenTang", Value = "MaTang" },
                    new Field("DienTich", "Diện tích (m²)", Kind.Number) { Min = 1, Decimals = 2 },
                    new Field("Huong", "Hướng", Kind.Combo) { Items = new[] { "Đông", "Tây", "Nam", "Bắc", "Đông Nam", "Đông Bắc", "Tây Nam", "Tây Bắc" } },
                    new Field("TrangThai", "Trạng thái", Kind.Combo) { Items = new[] { "Trống", "Đang thuê", "Bảo trì" } }
                },
                DanhMucService.LayPhong,
                v => DanhMucService.LuuPhong(S(v, "MaPhong"), S(v, "MaTang"), D(v, "DienTich"), S(v, "Huong"), S(v, "TrangThai")),
                ma => DanhMucService.Xoa("Phong", "MaPhong", ma)));

            // ---- Tra cứu phòng trống cho khách tham khảo ----
            tab.TabPages.Add(TaoTraCuu());

            // ---- Khách hàng ----
            Them(tab, "tabKhachHang", "Khách hàng", new CrudPanel(
                new List<Field>
                {
                    new Field("MaKH", "Mã khách hàng") { IsKey = true },
                    new Field("TenCongTy", "Tên công ty"),
                    new Field("MaSoThue", "Mã số thuế"),
                    new Field("NguoiDaiDien", "Người đại diện"),
                    new Field("DiaChi", "Địa chỉ"),
                    new Field("SDT", "Điện thoại"),
                    new Field("Email", "Email")
                },
                DanhMucService.LayKhachHang,
                v => DanhMucService.LuuKhachHang(S(v, "MaKH"), S(v, "TenCongTy"), S(v, "MaSoThue"), S(v, "NguoiDaiDien"),
                                                 S(v, "DiaChi"), S(v, "SDT"), S(v, "Email")),
                ma => DanhMucService.Xoa("KhachHang", "MaKH", ma)));

            // ---- Dịch vụ ----
            Them(tab, "tabDichVu", "Dịch vụ", new CrudPanel(
                new List<Field>
                {
                    new Field("MaDV", "Mã dịch vụ") { IsKey = true },
                    new Field("TenDV", "Tên dịch vụ"),
                    new Field("DonViTinh", "Đơn vị tính"),
                    new Field("DonGia", "Đơn giá", Kind.Number)
                },
                DanhMucService.LayDichVu,
                v => DanhMucService.LuuDichVu(S(v, "MaDV"), S(v, "TenDV"), S(v, "DonViTinh"), D(v, "DonGia")),
                ma => DanhMucService.Xoa("DichVu", "MaDV", ma)));

            // ---- Nhân viên quản lý ----
            Them(tab, "tabNVQL", "Nhân viên quản lý", new CrudPanel(
                new List<Field>
                {
                    new Field("MaNVQL", "Mã nhân viên") { IsKey = true },
                    new Field("HoTen", "Họ tên"),
                    new Field("ChucVu", "Chức vụ"),
                    new Field("SDT", "Điện thoại")
                },
                DanhMucService.LayNVQL,
                v => DanhMucService.LuuNVQL(S(v, "MaNVQL"), S(v, "HoTen"), S(v, "ChucVu"), S(v, "SDT")),
                ma => DanhMucService.Xoa("NhanVienQL", "MaNVQL", ma)));
        }

        private static string S(Dictionary<string, object> v, string k) { return v[k] == null ? "" : v[k].ToString(); }
        private static decimal D(Dictionary<string, object> v, string k) { return v[k] == null ? 0m : Convert.ToDecimal(v[k]); }

        private static void Them(TabControl tab, string name, string text, Control c)
        {
            var p = new TabPage(text) { Name = name };
            p.Controls.Add(c);
            tab.TabPages.Add(p);
        }

        private TabPage TaoTraCuu()
        {
            var p = new TabPage("Tra cứu phòng trống") { Name = "tabTraCuu" };
            Ui.Lbl(p, "Diện tích tối thiểu (m²):", 15, 15, 150);
            NumericUpDown numDT = Ui.Num(p, "numDienTich", 170, 12, 90, 0, 10000, 0, 0);
            Ui.Lbl(p, "Giá tối đa (đ/tháng, 0 = bỏ qua):", 280, 15, 210);
            NumericUpDown numGia = Ui.Num(p, "numGiaToiDa", 495, 12, 140, 0, 1000000000000m, 0, 0);
            Ui.Lbl(p, "Tầng:", 655, 15, 45);
            ComboBox cboTang = Ui.Cbo(p, "cboTang", 700, 12, 140);
            DataTable dt = DanhMucService.LayTang();
            DataRow all = dt.NewRow(); all["MaTang"] = ""; all["TenTang"] = "(Tất cả)"; dt.Rows.InsertAt(all, 0);
            Ui.Bind(cboTang, dt, "TenTang", "MaTang");
            DataGridView dgv = Ui.Grid(p, "dgvTraCuu", 15, 55, 1050, 480);
            Ui.Btn(p, "btnTim", "Tìm phòng trống", 860, 9, 150, (o, ev) => Ui.Run(() =>
            {
                string tang = Ui.Val(cboTang);
                dgv.DataSource = DanhMucService.TraCuuPhongTrong(numDT.Value, numGia.Value, tang == "" ? null : tang);
                Ui.DinhDang(dgv, "GiaThue", "N0");
            }));
            return p;
        }
    }
}
