using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using static BenhVien.DataAccess.DatabaseHelper;

namespace BenhVien.Forms;

/// <summary>UC-16 Ghi nhận phiếu khám vào hồ sơ sau khi thanh toán &amp; xuất viện: usp_GhiNhanVaoHoSo (BR02, BR04).</summary>
public class FrmGhiNhanHoSo : FrmBase
{
    readonly DataGridView grid = Ui.Grid();
    readonly TextBox txtChiTiet = Ui.Txt(multiline: true, readOnly: true);

    public FrmGhiNhanHoSo() : base("Ghi nhận phiếu khám vào hồ sơ", "UC-16 · Nhân viên CSKH", 1100, 700)
    {
        txtChiTiet.Dock = DockStyle.Fill;
        var detail = Ui.Group("Chi tiết phiếu khám", txtChiTiet, DockStyle.Fill);
        detail.Dock = DockStyle.Right; detail.Width = 420;

        var note = new Label
        {
            Text = "BR04: chỉ ghi nhận chính thức vào hồ sơ sau khi bệnh nhân hoàn tất thanh toán và xuất viện. Trước đó phiếu khám / toa thuốc chỉ ở trạng thái ghi tạm.",
            Dock = DockStyle.Top, AutoSize = false, Height = 46, Padding = new Padding(10, 6, 10, 6),
            BackColor = Color.FromArgb(255, 243, 224), ForeColor = Color.FromArgb(191, 54, 12), Font = Ui.Normal
        };

        Content.Controls.Add(Ui.Group("Phiếu khám đang ghi tạm", grid, DockStyle.Fill));
        Content.Controls.Add(detail);
        Content.Controls.Add(note);

        grid.SelectionChanged += (_, _) =>
        {
            var r = Ui.Sel(grid);
            if (r == null) { txtChiTiet.Clear(); return; }
            Ui.TryDb(() => txtChiTiet.Text = ChiTiet(Ui.S(r["maPK"])));
        };

        var btnGhi = Ui.Btn("Ghi nhận vào hồ sơ", width: 200);
        AddActions(btnGhi);
        btnGhi.Click += (_, _) =>
        {
            var r = Ui.Sel(grid);
            if (r == null) { Ui.Alert("Hãy chọn phiếu khám cần ghi nhận."); return; }
            var maPK = Ui.S(r["maPK"]);
            if (!Ui.TryDb(() => ExecuteProc("dbo.usp_GhiNhanVaoHoSo", P("@maPK", maPK), P("@maNV", Session.MaNV)))) return;
            Ui.Msg("Đã ghi nhận phiếu " + maPK + " vào trang chi tiết của hồ sơ " + r["maHS"] + " (BR02).");
            LoadData();
        };

        LoadData();
    }

    void LoadData()
    {
        Ui.TryDb(() => Ui.Bind(grid, ExecuteQuery(@"
            SELECT pk.maPK, pk.maHS, bn.hoTen, pk.ngayGioKham, pk.loaiKham,
                   ISNULL(hd.trangThai, N'Chưa thanh toán') AS thanhToan, pk.trangThai
            FROM dbo.PhieuKhamBenh pk
            JOIN dbo.HoSoBenhAn hs ON hs.maHS = pk.maHS
            JOIN dbo.BenhNhan bn ON bn.maBN = hs.maBN
            LEFT JOIN dbo.HoaDonVienPhi hd ON hd.maPK = pk.maPK
            WHERE pk.trangThai = N'Ghi tạm'
            ORDER BY pk.ngayGioKham DESC"),
            "Mã phiếu khám", "Mã hồ sơ", "Bệnh nhân", "Ngày khám", "Loại khám", "Thanh toán", "Trạng thái"));
    }

    static string ChiTiet(string maPK)
    {
        var sb = new StringBuilder();
        var h = ExecuteQuery(@"
            SELECT pk.maPK, pk.maHS, bn.hoTen, pk.ngayGioKham, pk.loaiKham, pk.ketLuan, k.tenKhoa, bs.hoTen AS bacSi
            FROM dbo.PhieuKhamBenh pk
            JOIN dbo.HoSoBenhAn hs ON hs.maHS = pk.maHS JOIN dbo.BenhNhan bn ON bn.maBN = hs.maBN
            JOIN dbo.Khoa k ON k.maKhoa = pk.maKhoa JOIN dbo.BacSi bs ON bs.maBS = pk.maBS
            WHERE pk.maPK = @m", P("@m", maPK));
        if (h.Rows.Count == 0) return "";
        var r = h.Rows[0];
        sb.AppendLine("Phiếu khám: " + r["maPK"] + "   Hồ sơ: " + r["maHS"]);
        sb.AppendLine("Bệnh nhân: " + r["hoTen"]);
        sb.AppendLine("Ngày khám: " + Ui.D(r["ngayGioKham"], "dd/MM/yyyy HH:mm") + " – " + r["loaiKham"]);
        sb.AppendLine("Khoa: " + r["tenKhoa"] + " – " + r["bacSi"]);
        sb.AppendLine("Kết luận: " + Ui.S(r["ketLuan"]));
        sb.AppendLine();

        sb.AppendLine("Triệu chứng / chỉ số:");
        foreach (DataRow t in ExecuteQuery("SELECT ten, giaTri, donVi FROM dbo.ChiTietTrieuChung WHERE maPK = @m ORDER BY maTC", P("@m", maPK)).Rows)
            sb.AppendLine("  - " + t["ten"] + ": " + Ui.S(t["giaTri"]) + " " + Ui.S(t["donVi"]));

        sb.AppendLine("Xét nghiệm:");
        foreach (DataRow t in ExecuteQuery(@"SELECT dm.tenXN, x.ketQua, x.trangThai FROM dbo.XetNghiem x
                                            JOIN dbo.DanhMucXetNghiem dm ON dm.maDM = x.maDM WHERE x.maPK = @m", P("@m", maPK)).Rows)
            sb.AppendLine("  - " + t["tenXN"] + ": " + (Ui.S(t["ketQua"]) == "" ? Ui.S(t["trangThai"]) : Ui.S(t["ketQua"])));

        sb.AppendLine("Toa thuốc:");
        foreach (DataRow t in ExecuteQuery(@"SELECT th.tenThuoc, c.soLuong, c.lieuDung, c.soNgay FROM dbo.ToaThuoc tt
                                            JOIN dbo.ChiTietToaThuoc c ON c.maToa = tt.maToa
                                            JOIN dbo.Thuoc th ON th.maThuoc = c.maThuoc WHERE tt.maPK = @m", P("@m", maPK)).Rows)
            sb.AppendLine("  - " + t["tenThuoc"] + " x" + t["soLuong"] + ", " + t["lieuDung"] + ", " + t["soNgay"] + " ngày");

        var hd = ExecuteQuery("SELECT tongTien, hinhThucTT, trangThai FROM dbo.HoaDonVienPhi WHERE maPK = @m", P("@m", maPK));
        sb.AppendLine();
        sb.AppendLine(hd.Rows.Count == 0
            ? "Hóa đơn: CHƯA thanh toán"
            : "Hóa đơn: " + Convert.ToDecimal(hd.Rows[0]["tongTien"]).ToString("N0") + " VNĐ – " + hd.Rows[0]["hinhThucTT"] + " (" + hd.Rows[0]["trangThai"] + ")");
        return sb.ToString();
    }
}
