using System.Data;
using System.Drawing;
using System.Windows.Forms;
using static BenhVien.DataAccess.DatabaseHelper;

namespace BenhVien.Forms;

/// <summary>UC-07 / UC-08 Đặt lịch hẹn khám định kỳ (usp_DatLichDinhKy) và quản lý lịch hẹn (BR05, BR06).</summary>
public class FrmLichHen : FrmBase
{
    readonly TextBox txtMaHS = Ui.Txt(), txtTen = Ui.Txt(readOnly: true), txtHan = Ui.Txt(readOnly: true), txtTT = Ui.Txt(readOnly: true), txtQuy = Ui.Txt(readOnly: true);
    readonly DateTimePicker dtpHen = Ui.Date(true);
    readonly DataGridView grid = Ui.Grid();
    string _maHS;

    public FrmLichHen() : base("Đặt lịch & quản lý lịch hẹn khám định kỳ", "UC-07, UC-08 · Nhân viên CSKH")
    {
        dtpHen.Value = DateTime.Today.AddDays(3).AddHours(9);
        dtpHen.ValueChanged += (_, _) => CapNhatQuy();
        CapNhatQuy();

        var btnKT = Ui.Btn("Kiểm tra hiệu lực", width: 170);
        btnKT.Click += (_, _) => TimHoSo();

        var g1 = Ui.Group("1. Kiểm tra hồ sơ", Ui.SearchBar("Mã hồ sơ / SĐT / họ tên:", txtMaHS, btnKT));
        var g2 = Ui.Group("Thông tin hồ sơ", Ui.Fields(3, ("Bệnh nhân", txtTen), ("Hạn hồ sơ", txtHan), ("Hiệu lực", txtTT)));
        var g3 = Ui.Group("2. Chọn ngày giờ hẹn (mỗi quý một lịch – BR06, miễn phí – BR03)",
            Ui.Fields(2, ("Ngày giờ hẹn", dtpHen), ("Thuộc quý", txtQuy)));
        Stack(Ui.Group("Danh sách lịch hẹn", grid, DockStyle.Fill), g1, g2, g3);

        var btnDat = Ui.Btn("Đặt lịch", width: 120);
        var btnDoi = Ui.Btn("Đổi lịch", Ui.Blue, 120);
        var btnHuy = Ui.Btn("Hủy lịch", Ui.Red, 120);
        AddActions(btnDat, btnDoi, btnHuy);

        btnDat.Click += (_, _) =>
        {
            if (_maHS == null) { Ui.Alert("Hãy kiểm tra hiệu lực hồ sơ trước."); return; }
            var pOut = POut("@maLH", SqlDbType.VarChar, 12);
            if (!Ui.TryDb(() => ExecuteProc("dbo.usp_DatLichDinhKy",
                    P("@maHS", _maHS), PDateTime("@ngayGioHen", dtpHen.Value), P("@maNV", Session.MaNV), pOut))) return;
            Ui.Msg("Đã đặt lịch " + pOut.Value + " lúc " + dtpHen.Text + ".");
            LoadData();
        };

        btnDoi.Click += (_, _) =>
        {
            var r = Ui.Sel(grid);
            if (r == null) { Ui.Alert("Hãy chọn lịch hẹn cần đổi."); return; }
            if (dtpHen.Value <= DateTime.Now) { Ui.Alert("Ngày giờ hẹn mới phải ở tương lai."); return; }
            int n = 0;
            if (!Ui.TryDb(() => n = ExecuteNonQuery(@"
                    UPDATE dbo.LichHen
                    SET ngayGioHen = @d, quy = DATEPART(QUARTER, @d), nam = YEAR(@d)
                    WHERE maLH = @m AND trangThai = N'Đã hẹn'", PDateTime("@d", dtpHen.Value), P("@m", r["maLH"])))) return;
            if (n == 0) Ui.Alert("Chỉ đổi được lịch ở trạng thái \"Đã hẹn\".");
            else { Ui.Msg("Đã đổi lịch hẹn sang " + dtpHen.Text + "."); LoadData(); }
        };

        btnHuy.Click += (_, _) =>
        {
            var r = Ui.Sel(grid);
            if (r == null) { Ui.Alert("Hãy chọn lịch hẹn cần hủy."); return; }
            if (!Ui.Confirm("Hủy lịch hẹn " + r["maLH"] + "?")) return;
            int n = 0;
            if (!Ui.TryDb(() => n = ExecuteNonQuery("UPDATE dbo.LichHen SET trangThai = N'Hủy' WHERE maLH = @m AND trangThai = N'Đã hẹn'", P("@m", r["maLH"])))) return;
            if (n == 0) Ui.Alert("Chỉ hủy được lịch ở trạng thái \"Đã hẹn\".");
            else LoadData();
        };

        LoadData();
    }

    void CapNhatQuy() => txtQuy.Text = "Quý " + ((dtpHen.Value.Month - 1) / 3 + 1) + "/" + dtpHen.Value.Year;

    void TimHoSo()
    {
        var key = txtMaHS.Text.Trim();
        if (key == "") { Ui.Alert("Vui lòng nhập mã hồ sơ."); return; }
        Ui.TryDb(() =>
        {
            var dt = ExecuteProcTable("dbo.usp_TraCuuHoSo", P("@tuKhoa", key));
            if (dt.Rows.Count == 0) { _maHS = null; Ui.Alert("Không tìm thấy hồ sơ."); return; }
            if (dt.Rows.Count > 1) { _maHS = null; Ui.Alert("Có " + dt.Rows.Count + " kết quả, hãy nhập chính xác mã hồ sơ."); return; }
            var r = dt.Rows[0];
            _maHS = Ui.S(r["maHS"]);
            txtMaHS.Text = _maHS;
            txtTen.Text = Ui.S(r["hoTen"]);
            txtHan.Text = Ui.D(r["ngayHetHan"]);
            txtTT.Text = Ui.S(r["hieuLuc"]);
            txtTT.ForeColor = Ui.S(r["hieuLuc"]) == "Hiệu lực" ? Ui.Primary : Ui.Red;
        });
        LoadData();
    }

    void LoadData()
    {
        Ui.TryDb(() => Ui.Bind(grid, ExecuteQuery(@"
            SELECT TOP 200 lh.maLH, lh.maHS, bn.hoTen, lh.ngayGioHen, lh.quy, lh.nam, lh.trangThai
            FROM dbo.LichHen lh
            JOIN dbo.HoSoBenhAn hs ON hs.maHS = lh.maHS
            JOIN dbo.BenhNhan bn ON bn.maBN = hs.maBN
            WHERE @m = '' OR lh.maHS = @m
            ORDER BY lh.ngayGioHen DESC", P("@m", _maHS ?? "")),
            "Mã lịch hẹn", "Mã hồ sơ", "Bệnh nhân", "Ngày giờ hẹn", "Quý", "Năm", "Trạng thái"));
    }
}
