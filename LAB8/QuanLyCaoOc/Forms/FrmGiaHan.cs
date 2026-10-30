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
    /// <summary>Lập phiếu gia hạn: chỉ khi hợp đồng còn hiệu lực, không nợ tiền thuê, đã thanh toán dịch vụ,
    /// mỗi phòng gia hạn không quá 1/2 thời gian thuê.</summary>
    public class FrmGiaHan : Form
    {
        private readonly ComboBox cboHopDong, cboNVQL, cboPhong;
        private readonly NumericUpDown numSoThang;
        private readonly Label lblKetQua, lblToiDa;
        private readonly TextBox txtGhiChu;
        private readonly DataGridView dgvDong, dgvLichSu;
        private readonly DataTable _dong = new DataTable();
        private DataTable _phongHD = new DataTable();
        private DieuKienGiaHan _dk;

        public FrmGiaHan()
        {
            Text = "Lập phiếu gia hạn";
            ClientSize = new Size(1120, 680);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Font = Ui.FontChu;

            _dong.Columns.Add("MaPhong", typeof(string));
            _dong.Columns.Add("SoThangGiaHan", typeof(int));
            _dong.Columns.Add("NgayKetThucCu", typeof(DateTime));
            _dong.Columns.Add("NgayKetThucMoi", typeof(DateTime));

            Ui.Lbl(this, "Hợp đồng:", 15, 15, 80);
            cboHopDong = Ui.Cbo(this, "cboHopDong", 100, 12, 460);
            Ui.Btn(this, "btnKiemTra", "Kiểm tra điều kiện", 580, 9, 170, (o, ev) => Ui.Run(KiemTra));
            lblKetQua = Ui.Lbl(this, "", 15, 50, 1080, "lblKetQua");
            lblKetQua.Height = 48;

            Ui.Lbl(this, "Người lập:", 15, 110, 80);
            cboNVQL = Ui.Cbo(this, "cboNVQL", 100, 107, 220);
            Ui.Lbl(this, "Phòng:", 345, 110, 55);
            cboPhong = Ui.Cbo(this, "cboPhong", 400, 107, 110);
            Ui.Lbl(this, "Số tháng gia hạn:", 530, 110, 115);
            numSoThang = Ui.Num(this, "numSoThang", 650, 107, 70, 1, 60, 1);
            lblToiDa = Ui.Lbl(this, "", 735, 110, 380, "lblToiDa");
            Ui.Btn(this, "btnThemDong", "Thêm phòng vào phiếu", 100, 145, 190, (o, ev) => Ui.Run(ThemDong));
            Ui.Btn(this, "btnBoDong", "Bỏ dòng", 300, 145, 100, (o, ev) => Ui.Run(BoDong));

            dgvDong = Ui.Grid(this, "dgvDong", 15, 185, 1090, 170);
            dgvDong.DataSource = _dong;

            Ui.Lbl(this, "Ghi chú:", 15, 372, 80);
            txtGhiChu = Ui.Txt(this, "txtGhiChu", 100, 369, 640);
            Ui.Btn(this, "btnLap", "Lập phiếu gia hạn", 760, 365, 200, (o, ev) => Ui.Run(Lap));

            Ui.Lbl(this, "Lịch sử gia hạn của hợp đồng:", 15, 415, 300);
            dgvLichSu = Ui.Grid(this, "dgvLichSu", 15, 442, 1090, 225);

            cboHopDong.SelectedIndexChanged += (o, ev) => Ui.Run(DoiHopDong);
            Load += (o, ev) => Ui.Run(() =>
            {
                Ui.Bind(cboNVQL, DanhMucService.LayNVQL(), "HoTen", "MaNVQL");
                Ui.Bind(cboHopDong, HopDongService.DanhSachChon(TT.HDHieuLuc), "HienThi", "SoHD");
                DoiHopDong();
            });
        }

        private void DoiHopDong()
        {
            _dong.Rows.Clear();
            _dk = null;
            lblKetQua.Text = ""; lblToiDa.Text = "";
            string so = Ui.Val(cboHopDong);
            if (so == "") { dgvLichSu.DataSource = null; return; }
            _phongHD = HopDongService.LayChiTiet(so);
            Ui.Bind(cboPhong, HopDongService.PhongCuaHopDong(so), "MaPhong", "MaPhong");
            dgvLichSu.DataSource = GiaHanService.LayLichSu(so);
            KiemTra();
        }

        private void KiemTra()
        {
            string so = Ui.Val(cboHopDong);
            if (so == "") throw new NghiepVuException("Chưa chọn hợp đồng.");
            _dk = GiaHanService.KiemTra(so);
            lblToiDa.Text = "Tối đa " + _dk.ToiDaThang + " tháng/phòng (1/2 của " + _dk.SoThang + " tháng thuê)";
            numSoThang.Maximum = Math.Max(1, _dk.ToiDaThang);
            if (_dk.Duoc)
            {
                lblKetQua.ForeColor = Color.DarkGreen;
                lblKetQua.Text = "Đủ điều kiện gia hạn: hợp đồng còn hiệu lực đến " + _dk.NgayKetThuc.ToString("dd/MM/yyyy") +
                                 ", không nợ tiền thuê, đã thanh toán dịch vụ.";
            }
            else
            {
                lblKetQua.ForeColor = Color.Firebrick;
                lblKetQua.Text = "Chưa đủ điều kiện gia hạn: " + _dk.LyDo + ".";
            }
        }

        private void ThemDong()
        {
            if (cboPhong.SelectedIndex < 0) throw new NghiepVuException("Chưa chọn phòng cần gia hạn.");
            if (_dk == null) KiemTra();
            string mp = Ui.Val(cboPhong);
            int n = (int)numSoThang.Value;
            if (n * 2 > _dk.SoThang)
                throw new NghiepVuException("Gia hạn tối đa " + _dk.ToiDaThang + " tháng (không quá 1/2 thời gian thuê).");
            if (_dong.Rows.Cast<DataRow>().Any(r => (string)r["MaPhong"] == mp))
                throw new NghiepVuException("Phòng này đã có trong phiếu.");
            DateTime cu = (DateTime)_phongHD.Rows.Cast<DataRow>().First(r => (string)r["MaPhong"] == mp)["NgayKetThuc"];
            _dong.Rows.Add(mp, n, cu, cu.AddDays(1).AddMonths(n).AddDays(-1));
        }

        private void BoDong()
        {
            if (dgvDong.CurrentRow != null) _dong.Rows.RemoveAt(dgvDong.CurrentRow.Index);
        }

        private void Lap()
        {
            List<Tuple<string, int>> ds = _dong.Rows.Cast<DataRow>()
                .Select(r => Tuple.Create((string)r["MaPhong"], (int)r["SoThangGiaHan"])).ToList();
            string so = GiaHanService.LapPhieuGiaHan(Ui.Val(cboHopDong), Ui.Val(cboNVQL), ds, txtGhiChu.Text.Trim());
            Ui.Info("Đã lập phiếu gia hạn " + so + ". Các kỳ thu tiền thuê cho thời gian gia hạn đã được phát sinh.");
            txtGhiChu.Clear();
            DoiHopDong();
        }
    }
}
