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
    /// <summary>Kiểm tra tình trạng phòng mỗi năm một lần và xử phạt phòng tự ý thay đổi kết cấu.</summary>
    public class FrmKiemTra : Form
    {
        // ---- Tab kiểm tra ----
        private ComboBox cboNVQL, cboTinhTrang;
        private DateTimePicker dtNgayKT;
        private TextBox txtKetLuan, txtGhiChu;
        private DataGridView dgvPhong, dgvDong, dgvTuSua;
        private readonly DataTable _dong = new DataTable();

        // ---- Tab xử phạt ----
        private ComboBox cboHopDong, cboPhongPhat;
        private TextBox txtLyDo;
        private NumericUpDown numTienPhat;
        private DataGridView dgvPhat;

        public FrmKiemTra()
        {
            Text = "Kiểm tra phòng định kỳ và xử phạt";
            ClientSize = new Size(1200, 700);
            StartPosition = FormStartPosition.CenterScreen;
            Font = Ui.FontChu;

            _dong.Columns.Add("MaPhong", typeof(string));
            _dong.Columns.Add("TinhTrang", typeof(string));
            _dong.Columns.Add("GhiChu", typeof(string));

            var tab = new TabControl { Name = "tabMain", Dock = DockStyle.Fill };
            var tabKT = new TabPage("Kiểm tra định kỳ") { Name = "tabKiemTra" };
            var tabPhat = new TabPage("Xử phạt vi phạm") { Name = "tabPhat" };
            tab.TabPages.Add(tabKT);
            tab.TabPages.Add(tabPhat);
            Controls.Add(tab);
            DungTabKiemTra(tabKT);
            DungTabPhat(tabPhat);

            Load += (o, ev) => Ui.Run(TaiTatCa);
        }

        // =================== KIỂM TRA ===================
        private void DungTabKiemTra(Control p)
        {
            Ui.Lbl(p, "Người kiểm tra:", 15, 15, 100);
            cboNVQL = Ui.Cbo(p, "cboNVQL", 120, 12, 220);
            Ui.Lbl(p, "Ngày kiểm tra:", 360, 15, 100);
            dtNgayKT = Ui.Dtp(p, "dtNgayKT", 465, 12, 130);
            Ui.Lbl(p, "Kết luận chung:", 615, 15, 100);
            txtKetLuan = Ui.Txt(p, "txtKetLuan", 720, 12, 450);

            Ui.Lbl(p, "Phòng và thời điểm kiểm tra gần nhất:", 15, 52, 300);
            dgvPhong = Ui.Grid(p, "dgvPhong", 15, 78, 520, 300);
            Ui.Btn(p, "btnChonDenHan", "Thêm tất cả phòng đến hạn", 15, 385, 230, (o, ev) => Ui.Run(ThemDenHan));

            Ui.Lbl(p, "Tình trạng:", 560, 82, 80);
            cboTinhTrang = Ui.Cbo(p, "cboTinhTrang", 645, 79, 130, "Tốt", "Cần tu sửa", "Hư hỏng nặng");
            Ui.Lbl(p, "Ghi chú:", 790, 82, 60);
            txtGhiChu = Ui.Txt(p, "txtGhiChu", 855, 79, 315);
            Ui.Btn(p, "btnThemDong", "Thêm phòng đang chọn vào phiếu", 560, 115, 260, (o, ev) => Ui.Run(ThemDong));
            Ui.Btn(p, "btnBoDong", "Bỏ dòng", 830, 115, 100, (o, ev) => Ui.Run(BoDong));
            dgvDong = Ui.Grid(p, "dgvDong", 560, 155, 610, 223);
            dgvDong.DataSource = _dong;
            Ui.Btn(p, "btnLapPhieu", "Lập phiếu kiểm tra", 940, 115, 230, (o, ev) => Ui.Run(LapPhieu));

            Ui.Lbl(p, "Phòng cần tu sửa (theo lần kiểm tra gần nhất):", 15, 430, 400);
            dgvTuSua = Ui.Grid(p, "dgvTuSua", 15, 458, 1155, 190);
        }

        private void ThemDong()
        {
            string mp = Ui.Cell(dgvPhong, "MaPhong");
            if (mp == "") throw new NghiepVuException("Chưa chọn phòng cần ghi nhận.");
            if (_dong.Rows.Cast<DataRow>().Any(r => (string)r["MaPhong"] == mp))
                throw new NghiepVuException("Phòng này đã có trong phiếu.");
            _dong.Rows.Add(mp, cboTinhTrang.Text, txtGhiChu.Text.Trim());
        }

        private void ThemDenHan()
        {
            foreach (DataRow r in ((DataTable)dgvPhong.DataSource).Rows)
            {
                if ((string)r["TinhTrang"] != "Đến hạn kiểm tra") continue;
                string mp = (string)r["MaPhong"];
                if (!_dong.Rows.Cast<DataRow>().Any(x => (string)x["MaPhong"] == mp)) _dong.Rows.Add(mp, "Tốt", "");
            }
        }

        private void BoDong()
        {
            if (dgvDong.CurrentRow != null) _dong.Rows.RemoveAt(dgvDong.CurrentRow.Index);
        }

        private void LapPhieu()
        {
            List<DongKT> ds = _dong.Rows.Cast<DataRow>()
                .Select(r => new DongKT((string)r["MaPhong"], (string)r["TinhTrang"], r["GhiChu"].ToString())).ToList();
            string ma = VanHanhService.LapPhieuKiemTra(Ui.Val(cboNVQL), dtNgayKT.Value, txtKetLuan.Text.Trim(), ds);
            Ui.Info("Đã lập phiếu kiểm tra " + ma + ".");
            _dong.Rows.Clear();
            txtKetLuan.Clear();
            TaiKiemTra();
        }

        private void TaiKiemTra()
        {
            dgvPhong.DataSource = VanHanhService.LayPhongKiemTra();
            Ui.TuMauDong(dgvPhong, "TinhTrang", "Đến hạn kiểm tra", Color.MistyRose);
            dgvTuSua.DataSource = VanHanhService.DanhSachCanTuSua();
        }

        // =================== XỬ PHẠT ===================
        private void DungTabPhat(Control p)
        {
            Ui.Lbl(p, "Hợp đồng:", 15, 15, 80);
            cboHopDong = Ui.Cbo(p, "cboHopDong", 100, 12, 420);
            Ui.Lbl(p, "Phòng vi phạm:", 540, 15, 100);
            cboPhongPhat = Ui.Cbo(p, "cboPhongPhat", 645, 12, 110);
            Ui.Lbl(p, "Nội dung vi phạm:", 15, 52, 120);
            txtLyDo = Ui.Txt(p, "txtLyDo", 140, 49, 615);
            Ui.Lbl(p, "Số tiền phạt:", 15, 87, 100);
            numTienPhat = Ui.Num(p, "numTienPhat", 140, 84, 160, 0, 1000000000000m, 0, 0);
            Ui.Btn(p, "btnLapPhat", "Lập phiếu phạt", 330, 80, 170, (o, ev) => Ui.Run(LapPhat));
            Ui.Btn(p, "btnNopPhat", "Ghi nhận nộp phạt", 510, 80, 180, (o, ev) => Ui.Run(NopPhat));
            dgvPhat = Ui.Grid(p, "dgvPhat", 15, 130, 1155, 510);

            cboHopDong.SelectedIndexChanged += (o, ev) => Ui.Run(() =>
            {
                string so = Ui.Val(cboHopDong);
                Ui.Bind(cboPhongPhat, so == "" ? new DataTable() : HopDongService.PhongCuaHopDong(so), "MaPhong", "MaPhong");
            });
        }

        private void LapPhat()
        {
            string ma = VanHanhService.LapPhieuPhat(Ui.Val(cboHopDong), Ui.Val(cboPhongPhat), txtLyDo.Text, numTienPhat.Value);
            Ui.Info("Đã lập phiếu phạt " + ma + ".");
            txtLyDo.Clear(); numTienPhat.Value = 0;
            TaiPhat();
        }

        private void NopPhat()
        {
            string ma = Ui.Cell(dgvPhat, "MaPhat");
            if (ma == "") throw new NghiepVuException("Chưa chọn phiếu phạt.");
            VanHanhService.NopPhat(ma);
            Ui.Info("Đã ghi nhận nộp phạt.");
            TaiPhat();
        }

        private void TaiPhat()
        {
            dgvPhat.DataSource = VanHanhService.LayPhat();
            Ui.DinhDang(dgvPhat, "SoTienPhat", "N0");
        }

        private void TaiTatCa()
        {
            Ui.Bind(cboNVQL, DanhMucService.LayNVQL(), "HoTen", "MaNVQL");
            Ui.Bind(cboHopDong, HopDongService.DanhSachChon(), "HienThi", "SoHD");
            TaiKiemTra();
            TaiPhat();
        }
    }
}
