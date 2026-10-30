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
    public class FrmHopDong : Form
    {
        private sealed class PhongItem
        {
            public string Ma; public decimal Gia; public string Text;
            public override string ToString() { return Text; }
        }

        private readonly ComboBox cboKhachHang, cboNVQL, cboLoc;
        private readonly DateTimePicker dtBatDau;
        private readonly NumericUpDown numSoThang;
        private readonly CheckedListBox clbPhong;
        private readonly Label lblTongThang, lblTraTruoc, lblKetThuc;
        private readonly DataGridView dgvHopDong, dgvChiTiet;
        private readonly int _traTruoc;

        public FrmHopDong()
        {
            Text = "Lập hợp đồng thuê văn phòng";
            ClientSize = new Size(1220, 660);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Font = Ui.FontChu;
            _traTruoc = (int)Db.ThamSo("SO_THANG_TRA_TRUOC");

            var gb = new GroupBox { Text = "Thông tin hợp đồng", Left = 10, Top = 10, Width = 480, Height = 640 };
            Controls.Add(gb);

            Ui.Lbl(gb, "Khách hàng:", 15, 30);
            cboKhachHang = Ui.Cbo(gb, "cboKhachHang", 135, 27, 325);
            Ui.Lbl(gb, "Người lập:", 15, 65);
            cboNVQL = Ui.Cbo(gb, "cboNVQL", 135, 62, 325);
            Ui.Lbl(gb, "Ngày bắt đầu:", 15, 100);
            dtBatDau = Ui.Dtp(gb, "dtBatDau", 135, 97, 150);
            Ui.Lbl(gb, "Số tháng thuê:", 15, 135);
            numSoThang = Ui.Num(gb, "numSoThang", 135, 132, 90, 6, 120, 12);
            Ui.Lbl(gb, "(tối thiểu 6 tháng)", 235, 135, 150);

            Ui.Lbl(gb, "Chọn phòng trống (có thể chọn nhiều):", 15, 170, 400);
            clbPhong = new CheckedListBox { Name = "clbPhong", Left = 15, Top = 197, Width = 445, Height = 230, CheckOnClick = true };
            gb.Controls.Add(clbPhong);

            lblTongThang = Ui.Lbl(gb, "", 15, 440, 445, "lblTongThang");
            lblTraTruoc = Ui.Lbl(gb, "", 15, 470, 445, "lblTraTruoc");
            lblKetThuc = Ui.Lbl(gb, "", 15, 500, 445, "lblKetThuc");
            Ui.Btn(gb, "btnLap", "Lập hợp đồng", 15, 545, 200, (o, ev) => Ui.Run(Lap));
            Ui.Btn(gb, "btnLamMoi", "Làm mới", 230, 545, 120, (o, ev) => Ui.Run(TaiDuLieu));

            // ---- Danh sách hợp đồng ----
            Ui.Lbl(this, "Danh sách hợp đồng - lọc:", 505, 12, 160);
            cboLoc = Ui.Cbo(this, "cboLoc", 670, 10, 160, "(Tất cả)", "Chờ thanh toán", "Hiệu lực", "Hết hạn", "Huỷ");
            Ui.Btn(this, "btnHuyHD", "Huỷ hợp đồng chờ thanh toán", 900, 8, 300, (o, ev) => Ui.Run(Huy));
            dgvHopDong = Ui.Grid(this, "dgvHopDong", 505, 45, 705, 340);
            dgvChiTiet = Ui.Grid(this, "dgvChiTiet", 505, 420, 705, 230);
            Ui.Lbl(this, "Các phòng của hợp đồng đang chọn:", 505, 392, 300);

            cboLoc.SelectedIndexChanged += (o, ev) => Ui.Run(TaiHopDong);
            dgvHopDong.SelectionChanged += (o, ev) => Ui.Run(TaiChiTiet);
            clbPhong.ItemCheck += (o, ev) => BeginInvoke(new Action(TinhTruoc));
            numSoThang.ValueChanged += (o, ev) => TinhTruoc();
            dtBatDau.ValueChanged += (o, ev) => TinhTruoc();

            Load += (o, ev) => Ui.Run(TaiDuLieu);
        }

        private void TaiDuLieu()
        {
            Ui.Bind(cboKhachHang, DanhMucService.LayKhachHang(), "TenCongTy", "MaKH");
            Ui.Bind(cboNVQL, DanhMucService.LayNVQL(), "HoTen", "MaNVQL");
            clbPhong.Items.Clear();
            foreach (DataRow r in HopDongService.PhongTrong().Rows)
            {
                decimal gia = Convert.ToDecimal(r["GiaThue"]);
                clbPhong.Items.Add(new PhongItem
                {
                    Ma = (string)r["MaPhong"], Gia = gia,
                    Text = r["MaPhong"] + " - Tầng " + r["DoCao"] + " - " + Convert.ToDecimal(r["DienTich"]).ToString("0.##") +
                           " m² - " + r["Huong"] + " - " + Ui.Money(gia) + "/tháng"
                });
            }
            TinhTruoc();
            TaiHopDong();
        }

        private List<PhongItem> PhongDaChon() { return clbPhong.CheckedItems.Cast<PhongItem>().ToList(); }

        private void TinhTruoc()
        {
            decimal tong = PhongDaChon().Sum(p => p.Gia);
            lblTongThang.Text = "Tiền thuê mỗi tháng: " + Ui.Money(tong);
            lblTraTruoc.Text = "Phải trả trước " + _traTruoc + " tháng đầu: " + Ui.Money(tong * _traTruoc);
            lblKetThuc.Text = "Ngày kết thúc dự kiến: " +
                dtBatDau.Value.Date.AddMonths((int)numSoThang.Value).AddDays(-1).ToString("dd/MM/yyyy");
        }

        private void Lap()
        {
            List<string> ds = PhongDaChon().Select(p => p.Ma).ToList();
            string so = HopDongService.LapHopDong(Ui.Val(cboKhachHang), Ui.Val(cboNVQL), dtBatDau.Value,
                                                  (int)numSoThang.Value, ds);
            Ui.Info("Đã lập hợp đồng " + so + ". Khách hàng cần thanh toán tiền trả trước " + _traTruoc +
                    " tháng đầu để hợp đồng có hiệu lực.");
            TaiDuLieu();
        }

        private void Huy()
        {
            string so = Ui.Cell(dgvHopDong, "SoHD");
            if (so == "") throw new NghiepVuException("Chưa chọn hợp đồng.");
            if (!Ui.Confirm("Huỷ hợp đồng " + so + "?")) return;
            HopDongService.HuyHopDong(so);
            Ui.Info("Đã huỷ hợp đồng, các phòng trở về trạng thái trống.");
            TaiDuLieu();
        }

        private void TaiHopDong()
        {
            string tt = cboLoc.SelectedIndex <= 0 ? null : cboLoc.SelectedItem.ToString();
            dgvHopDong.DataSource = HopDongService.LayHopDong(tt);
            Ui.DinhDang(dgvHopDong, "GiaThueThang", "N0");
            TaiChiTiet();
        }

        private void TaiChiTiet()
        {
            string so = Ui.Cell(dgvHopDong, "SoHD");
            dgvChiTiet.DataSource = so == "" ? null : HopDongService.LayChiTiet(so);
            Ui.DinhDang(dgvChiTiet, "GiaThue", "N0");
        }
    }
}
