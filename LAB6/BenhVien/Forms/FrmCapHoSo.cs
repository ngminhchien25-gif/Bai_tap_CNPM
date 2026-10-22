using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using static BenhVien.DataAccess.DatabaseHelper;

namespace BenhVien.Forms;

/// <summary>UC-04 Lập phiếu đăng ký &amp; cấp hồ sơ: gọi usp_DangKyLanDau (BenhNhan + PhieuDangKy + HoSoBenhAn + LePhi trong 1 transaction).</summary>
public class FrmCapHoSo : FrmBase
{
    readonly TextBox txtMaPhieu = Ui.Txt(readOnly: true), txtTrangThaiPhieu = Ui.Txt(readOnly: true);
    readonly TextBox txtTen = Ui.Txt(), txtDiaChi = Ui.Txt(), txtDT = Ui.Txt();
    readonly TextBox txtMaHS = Ui.Txt(readOnly: true), txtTrangThaiHS = Ui.Txt(readOnly: true);
    readonly DateTimePicker dtpNgayDK = Ui.Date(), dtpNgaySinh = Ui.Date(), dtpNgayDong = Ui.Date(), dtpHetHan = Ui.Date(), dtpNgayCap = Ui.Date();
    readonly NumericUpDown numTien = Ui.Num(0, 100000000, 500000);
    readonly PictureBox pic;
    bool _done;

    public FrmCapHoSo() : base("Lập phiếu đăng ký & cấp hồ sơ bệnh án", "UC-04 · Nhân viên tiếp nhận", 1000, 720)
    {
        Content.AutoScroll = true;
        Reset();

        // Ngày đóng / hết hạn do hệ thống tính (hôm nay, +1 năm – BR05)
        dtpNgayDK.Enabled = dtpNgayDong.Enabled = dtpHetHan.Enabled = dtpNgayCap.Enabled = false;

        var g1 = Ui.Group("1. Phiếu đăng ký",
            Ui.Fields(3, ("Mã phiếu", txtMaPhieu), ("Ngày đăng ký", dtpNgayDK), ("Trạng thái", txtTrangThaiPhieu)));

        var f2 = Ui.Fields(2,
            ("Họ và tên (*)", txtTen), ("Ngày sinh", dtpNgaySinh),
            ("Địa chỉ (*)", txtDiaChi), ("Điện thoại (*)", txtDT));
        var g2 = Ui.Group("2. Thông tin bệnh nhân", f2);
        g2.AutoSize = false; g2.Height = 210;
        f2.Dock = DockStyle.Fill;
        g2.Controls.Add(Ui.PhotoBox(out pic));

        var g3 = Ui.Group("3. Lệ phí quản lý hằng năm (BR01)",
            Ui.Fields(3, ("Số tiền (VNĐ)", numTien), ("Ngày đóng", dtpNgayDong), ("Ngày hết hạn (+1 năm)", dtpHetHan)));

        var g4 = Ui.Group("4. Hồ sơ bệnh án được cấp",
            Ui.Fields(3, ("Mã hồ sơ", txtMaHS), ("Ngày cấp", dtpNgayCap), ("Trạng thái", txtTrangThaiHS)));

        Stack(null, g1, g2, g3, g4);

        var btnMoi = Ui.Btn("Phiếu mới", Ui.Blue, 130);
        var btnCap = Ui.Btn("Thu phí & cấp hồ sơ", width: 190);
        var btnBienLai = Ui.Btn("In biên lai", Ui.Gray, 120);
        AddActions(btnMoi, btnCap, btnBienLai);

        btnMoi.Click += (_, _) => Reset();

        btnCap.Click += (_, _) =>
        {
            if (_done) { Ui.Alert("Hồ sơ đã được cấp. Bấm \"Phiếu mới\" để đăng ký bệnh nhân khác."); return; }
            if (txtTen.Text.Trim() == "" || txtDT.Text.Trim() == "" || txtDiaChi.Text.Trim() == "")
            {
                Ui.Alert("Vui lòng nhập họ tên, địa chỉ và điện thoại của bệnh nhân.");
                return;
            }
            if (numTien.Value <= 0)
            {
                Ui.Alert("BR01: bệnh nhân mới bắt buộc đóng lệ phí quản lý hằng năm mới được cấp hồ sơ.");
                return;
            }

            var pOut = POut("@maHS", SqlDbType.VarChar, 12);
            bool ok = Ui.TryDb(() => ExecuteProc("dbo.usp_DangKyLanDau",
                P("@hoTen", txtTen.Text.Trim()), P("@diaChi", txtDiaChi.Text.Trim()),
                PDate("@ngaySinh", dtpNgaySinh.Value), P("@dienThoai", txtDT.Text.Trim()),
                PBytes("@anh3x4", Ui.ImageBytes(pic.Image)), PMoney("@soTienLePhi", numTien.Value),
                P("@maNV", Session.MaNV), pOut));
            if (!ok) return;

            var maHS = (string)pOut.Value;
            Ui.TryDb(() =>
            {
                var dt = ExecuteQuery(@"
                    SELECT hs.maPhieuDK, hs.ngayCap, hs.ngayHetHan, hs.trangThai, dk.trangThai AS ttPhieu, dk.ngayDangKy
                    FROM dbo.HoSoBenhAn hs JOIN dbo.PhieuDangKy dk ON dk.maPhieuDK = hs.maPhieuDK
                    WHERE hs.maHS = @m", P("@m", maHS));
                var r = dt.Rows[0];
                txtMaPhieu.Text = Ui.S(r["maPhieuDK"]);
                txtTrangThaiPhieu.Text = Ui.S(r["ttPhieu"]);
                dtpNgayDK.Value = (DateTime)r["ngayDangKy"];
                dtpNgayCap.Value = (DateTime)r["ngayCap"];
                dtpNgayDong.Value = (DateTime)r["ngayCap"];
                dtpHetHan.Value = (DateTime)r["ngayHetHan"];
                txtMaHS.Text = maHS;
                txtTrangThaiHS.Text = Ui.S(r["trangThai"]);
            });
            _done = true;
            Ui.Msg("Đã cấp hồ sơ " + maHS + ". Quyền lợi khám định kỳ có hiệu lực đến " + dtpHetHan.Text + ".");
        };

        btnBienLai.Click += (_, _) =>
        {
            if (!_done) Ui.Alert("Chưa thu phí / cấp hồ sơ nên chưa có biên lai.");
            else Ui.Msg("Đang in biên lai thu lệ phí " + numTien.Value.ToString("N0") + " VNĐ cho hồ sơ " + txtMaHS.Text + "...");
        };
    }

    void Reset()
    {
        _done = false;
        txtMaPhieu.Text = "(tự sinh)"; txtTrangThaiPhieu.Text = "Chờ xử lý";
        txtMaHS.Text = "(chưa cấp)"; txtTrangThaiHS.Text = "—";
        txtTen.Clear(); txtDiaChi.Clear(); txtDT.Clear();
        dtpNgaySinh.Value = DateTime.Today.AddYears(-30);
        dtpNgayDK.Value = dtpNgayDong.Value = dtpNgayCap.Value = DateTime.Today;
        dtpHetHan.Value = DateTime.Today.AddYears(1);
        numTien.Value = 500000;
        if (pic != null) pic.Image = null;
    }
}
