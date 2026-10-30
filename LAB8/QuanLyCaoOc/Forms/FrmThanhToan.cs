using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QuanLyCaoOc.Data;
using QuanLyCaoOc.Services;

namespace QuanLyCaoOc.Forms
{
    public class FrmThanhToan : Form
    {
        // ---- Tab tiền thuê ----
        private ComboBox cboHopDong, cboHinhThuc;
        private TextBox txtNguoiThu;
        private DataGridView dgvKy;
        private Label lblCongNo;

        // ---- Tab dịch vụ ----
        private ComboBox cboHopDongDV, cboPhongDV, cboDichVu;
        private NumericUpDown numThang, numNam, numSoLuong;
        private DataGridView dgvDong, dgvHoaDon, dgvChiTietHD;
        private Label lblTong;
        private readonly DataTable _dong = new DataTable();

        public FrmThanhToan()
        {
            Text = "Thanh toán tiền thuê và dịch vụ";
            ClientSize = new Size(1180, 680);
            StartPosition = FormStartPosition.CenterScreen;
            Font = Ui.FontChu;

            _dong.Columns.Add("MaPhong", typeof(string));
            _dong.Columns.Add("MaDV", typeof(string));
            _dong.Columns.Add("TenDV", typeof(string));
            _dong.Columns.Add("SoLuong", typeof(decimal));
            _dong.Columns.Add("DonGia", typeof(decimal));
            _dong.Columns.Add("ThanhTien", typeof(decimal));

            var tab = new TabControl { Name = "tabMain", Dock = DockStyle.Fill };
            var tabThue = new TabPage("Tiền thuê") { Name = "tabThue" };
            var tabDV = new TabPage("Hóa đơn dịch vụ") { Name = "tabDV" };
            tab.TabPages.Add(tabThue);
            tab.TabPages.Add(tabDV);
            Controls.Add(tab);
            DungTabThue(tabThue);
            DungTabDichVu(tabDV);

            Load += (o, ev) => Ui.Run(TaiDanhSach);
        }

        // =================== TAB TIỀN THUÊ ===================
        private void DungTabThue(Control p)
        {
            Ui.Lbl(p, "Hợp đồng:", 15, 15, 80);
            cboHopDong = Ui.Cbo(p, "cboHopDong", 100, 12, 420);
            lblCongNo = Ui.Lbl(p, "", 540, 15, 600, "lblCongNo");
            lblCongNo.ForeColor = Color.Firebrick;

            dgvKy = Ui.Grid(p, "dgvKy", 15, 50, 1130, 430);

            Ui.Lbl(p, "Hình thức:", 15, 500, 80);
            cboHinhThuc = Ui.Cbo(p, "cboHinhThuc", 100, 497, 150, "Tiền mặt", "Chuyển khoản");
            Ui.Lbl(p, "Người thu:", 270, 500, 80);
            txtNguoiThu = Ui.Txt(p, "txtNguoiThu", 355, 497, 200);
            Ui.Btn(p, "btnThu", "Thu tiền kỳ đang chọn", 580, 494, 220, (o, ev) => Ui.Run(ThuTien));
            Label ghiChu = Ui.Lbl(p,
                "Quy tắc: kỳ 'Trả trước' thu khi ký hợp đồng; từ tháng thứ 7 thu vào đầu mỗi tháng; các kỳ phải thu theo thứ tự.",
                15, 545, 1100);
            ghiChu.ForeColor = Color.DimGray;

            cboHopDong.SelectedIndexChanged += (o, ev) => Ui.Run(TaiKyThu);
        }

        private void TaiKyThu()
        {
            string so = Ui.Val(cboHopDong);
            if (so == "") { dgvKy.DataSource = null; lblCongNo.Text = ""; return; }
            dgvKy.DataSource = ThanhToanService.LayKyThu(so);
            Ui.DinhDang(dgvKy, "SoTien", "N0");
            Ui.TuMauDong(dgvKy, "TinhTrang", "Quá hạn", Color.MistyRose);
            Ui.TuMauDong(dgvKy, "TinhTrang", "Đã thu", Color.Honeydew);
            var no = ThanhToanService.CongNo(so);
            lblCongNo.Text = (no.NoThue > 0 || no.NoDichVu > 0)
                ? "Công nợ: tiền thuê quá hạn " + Ui.Money(no.NoThue) + " | dịch vụ chưa thanh toán " + Ui.Money(no.NoDichVu)
                : "Hợp đồng không còn công nợ.";
        }

        private void ThuTien()
        {
            string ky = Ui.Cell(dgvKy, "MaKy");
            if (ky == "") throw new NghiepVuException("Chưa chọn kỳ thu tiền.");
            var kq = ThanhToanService.ThuTienThue(int.Parse(ky), cboHinhThuc.Text, txtNguoiThu.Text);
            Ui.Info("Đã thu " + Ui.Money(kq.SoTien) + (kq.SoNgayTre > 0 ? " (thanh toán trễ " + kq.SoNgayTre + " ngày)." : "."));
            TaiKyThu();
        }

        // =================== TAB DỊCH VỤ ===================
        private void DungTabDichVu(Control p)
        {
            Ui.Lbl(p, "Hợp đồng:", 15, 15, 80);
            cboHopDongDV = Ui.Cbo(p, "cboHopDongDV", 100, 12, 420);
            Ui.Lbl(p, "Tháng:", 545, 15, 50);
            numThang = Ui.Num(p, "numThang", 600, 12, 60, 1, 12, DateTime.Today.Month);
            Ui.Lbl(p, "Năm:", 680, 15, 40);
            numNam = Ui.Num(p, "numNam", 725, 12, 80, 2000, 2100, DateTime.Today.Year);
            numNam.ThousandsSeparator = false;

            Ui.Lbl(p, "Phòng:", 15, 52, 50);
            cboPhongDV = Ui.Cbo(p, "cboPhongDV", 70, 49, 110);
            Ui.Lbl(p, "Dịch vụ:", 195, 52, 60);
            cboDichVu = Ui.Cbo(p, "cboDichVu", 260, 49, 180);
            Ui.Lbl(p, "Số lượng:", 455, 52, 70);
            numSoLuong = Ui.Num(p, "numSoLuong", 530, 49, 110, 0, 1000000, 0, 2);
            Ui.Btn(p, "btnThemDong", "Thêm dòng", 660, 46, 120, (o, ev) => Ui.Run(ThemDong));
            Ui.Btn(p, "btnBoDong", "Bỏ dòng", 790, 46, 100, (o, ev) => Ui.Run(BoDong));

            dgvDong = Ui.Grid(p, "dgvDong", 15, 90, 1130, 150);
            dgvDong.DataSource = _dong;
            lblTong = Ui.Lbl(p, "Tổng hóa đơn: 0 đ", 15, 248, 400, "lblTong");
            Ui.Btn(p, "btnLapHoaDon", "Lập hóa đơn dịch vụ cuối tháng", 850, 244, 295, (o, ev) => Ui.Run(LapHoaDon));

            Ui.Lbl(p, "Hóa đơn dịch vụ của hợp đồng:", 15, 290, 300);
            dgvHoaDon = Ui.Grid(p, "dgvHoaDon", 15, 318, 620, 250);
            dgvChiTietHD = Ui.Grid(p, "dgvChiTietHD", 650, 318, 495, 250);
            Ui.Btn(p, "btnThanhToanDV", "Thanh toán hóa đơn đang chọn", 15, 580, 280, (o, ev) => Ui.Run(ThanhToanDV));

            cboHopDongDV.SelectedIndexChanged += (o, ev) => Ui.Run(DoiHopDongDV);
            dgvHoaDon.SelectionChanged += (o, ev) => Ui.Run(TaiChiTietHD);
        }

        private void DoiHopDongDV()
        {
            string so = Ui.Val(cboHopDongDV);
            _dong.Rows.Clear();
            CapNhatTong();
            if (so == "") { dgvHoaDon.DataSource = null; return; }
            Ui.Bind(cboPhongDV, HopDongService.PhongCuaHopDong(so), "MaPhong", "MaPhong");
            TaiHoaDon();
        }

        private void TaiHoaDon()
        {
            string so = Ui.Val(cboHopDongDV);
            dgvHoaDon.DataSource = so == "" ? null : ThanhToanService.LayHoaDonDV(so);
            Ui.DinhDang(dgvHoaDon, "TongTien", "N0");
            TaiChiTietHD();
        }

        private void TaiChiTietHD()
        {
            string so = Ui.Cell(dgvHoaDon, "SoHDDV");
            dgvChiTietHD.DataSource = so == "" ? null : ThanhToanService.LayChiTietHoaDonDV(so);
            Ui.DinhDang(dgvChiTietHD, "DonGia", "N0");
            Ui.DinhDang(dgvChiTietHD, "ThanhTien", "N0");
        }

        private void ThemDong()
        {
            if (cboPhongDV.SelectedIndex < 0 || cboDichVu.SelectedIndex < 0)
                throw new NghiepVuException("Chọn phòng và dịch vụ cần ghi nhận.");
            if (numSoLuong.Value <= 0) throw new NghiepVuException("Số lượng sử dụng phải lớn hơn 0.");
            var dv = (DataRowView)cboDichVu.SelectedItem;
            decimal gia = Convert.ToDecimal(dv["DonGia"]);
            string maPhong = Ui.Val(cboPhongDV), maDV = Ui.Val(cboDichVu);
            DataRow trung = _dong.Rows.Cast<DataRow>().FirstOrDefault(r => (string)r["MaPhong"] == maPhong && (string)r["MaDV"] == maDV);
            if (trung != null)
            {
                trung["SoLuong"] = (decimal)trung["SoLuong"] + numSoLuong.Value;
                trung["ThanhTien"] = Math.Round((decimal)trung["SoLuong"] * gia, 0);
            }
            else
                _dong.Rows.Add(maPhong, maDV, dv["TenDV"].ToString(), numSoLuong.Value, gia, Math.Round(numSoLuong.Value * gia, 0));
            CapNhatTong();
        }

        private void BoDong()
        {
            if (dgvDong.CurrentRow == null) return;
            _dong.Rows.RemoveAt(dgvDong.CurrentRow.Index);
            CapNhatTong();
        }

        private void CapNhatTong()
        {
            lblTong.Text = "Tổng hóa đơn: " + Ui.Money(_dong.Rows.Cast<DataRow>().Sum(r => (decimal)r["ThanhTien"]));
        }

        private void LapHoaDon()
        {
            List<DongDV> ds = _dong.Rows.Cast<DataRow>()
                .Select(r => new DongDV((string)r["MaPhong"], (string)r["MaDV"], (decimal)r["SoLuong"])).ToList();
            string so = ThanhToanService.LapHoaDonDichVu(Ui.Val(cboHopDongDV), (int)numThang.Value, (int)numNam.Value, ds);
            Ui.Info("Đã lập hóa đơn dịch vụ " + so + ". Hạn thanh toán: cuối tháng " + numThang.Value + "/" + numNam.Value + ".");
            _dong.Rows.Clear();
            CapNhatTong();
            TaiHoaDon();
        }

        private void ThanhToanDV()
        {
            string so = Ui.Cell(dgvHoaDon, "SoHDDV");
            if (so == "") throw new NghiepVuException("Chưa chọn hóa đơn dịch vụ.");
            ThanhToanService.ThanhToanHoaDonDV(so);
            Ui.Info("Đã ghi nhận thanh toán hóa đơn dịch vụ.");
            TaiHoaDon();
            TaiKyThu();
        }

        private void TaiDanhSach()
        {
            DataTable ds = HopDongService.DanhSachChon();
            Ui.Bind(cboHopDong, ds, "HienThi", "SoHD");
            Ui.Bind(cboHopDongDV, ds.Copy(), "HienThi", "SoHD");
            Ui.Bind(cboDichVu, DanhMucService.LayDichVu(), "TenDV", "MaDV");
            TaiKyThu();
            DoiHopDongDV();
        }
    }
}
