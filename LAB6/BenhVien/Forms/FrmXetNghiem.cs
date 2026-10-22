using System.Data;
using System.Drawing;
using System.Windows.Forms;
using static BenhVien.DataAccess.DatabaseHelper;

namespace BenhVien.Forms;

/// <summary>UC-12 Thực hiện xét nghiệm &amp; nhập kết quả: usp_NhapKetQuaXetNghiem.</summary>
public class FrmXetNghiem : FrmBase
{
    readonly DataGridView grid = Ui.Grid();
    readonly ComboBox cboLoc = Ui.Cbo("Tất cả", "Đã chỉ định", "Đã có kết quả");
    readonly TextBox txtMaXN = Ui.Txt(readOnly: true), txtBN = Ui.Txt(readOnly: true), txtTenXN = Ui.Txt(readOnly: true), txtKQ = Ui.Txt(multiline: true);
    string _maXN;

    public FrmXetNghiem() : base("Thực hiện xét nghiệm & nhập kết quả", "UC-12 · Nhân viên xét nghiệm", 1100, 700)
    {
        txtKQ.Height = 160;
        var detail = Ui.Group("Nhập kết quả xét nghiệm",
            Ui.Fields(1, ("Mã xét nghiệm", txtMaXN), ("Bệnh nhân", txtBN), ("Xét nghiệm", txtTenXN), ("Kết quả", txtKQ)));
        detail.AutoSize = false; detail.Dock = DockStyle.Right; detail.Width = 400;

        cboLoc.Width = 200;
        var filter = Ui.Group("Bộ lọc", Ui.Row(Ui.Lbl("Trạng thái:", true), cboLoc));

        Content.Controls.Add(Ui.Group("Danh sách xét nghiệm được chỉ định", grid, DockStyle.Fill));
        Content.Controls.Add(detail);
        Content.Controls.Add(filter);

        cboLoc.SelectedIndexChanged += (_, _) => LoadData();

        grid.SelectionChanged += (_, _) =>
        {
            var r = Ui.Sel(grid);
            if (r == null) return;
            _maXN = Ui.S(r["maXN"]);
            txtMaXN.Text = _maXN;
            txtBN.Text = Ui.S(r["hoTen"]);
            txtTenXN.Text = Ui.S(r["tenXN"]);
            txtKQ.Text = Ui.S(r["ketQua"]);
        };

        var btnLuu = Ui.Btn("Lưu kết quả", width: 150);
        AddActions(btnLuu);
        btnLuu.Click += (_, _) =>
        {
            if (_maXN == null) { Ui.Alert("Hãy chọn xét nghiệm cần nhập kết quả."); return; }
            if (txtKQ.Text.Trim() == "") { Ui.Alert("Vui lòng nhập kết quả."); return; }
            if (!Ui.TryDb(() => ExecuteProc("dbo.usp_NhapKetQuaXetNghiem",
                    P("@maXN", _maXN), P("@ketQua", txtKQ.Text.Trim()), P("@maNV", Session.MaNV)))) return;
            Ui.Msg("Đã lưu kết quả xét nghiệm " + _maXN + ".");
            LoadData();
        };

        LoadData();
    }

    void LoadData()
    {
        Ui.TryDb(() => Ui.Bind(grid, ExecuteQuery(@"
            SELECT x.maXN, x.maPK, bn.hoTen, dm.tenXN, x.ngayChiDinh, x.ketQua, x.trangThai
            FROM dbo.XetNghiem x
            JOIN dbo.PhieuKhamBenh pk ON pk.maPK = x.maPK
            JOIN dbo.HoSoBenhAn hs ON hs.maHS = pk.maHS
            JOIN dbo.BenhNhan bn ON bn.maBN = hs.maBN
            JOIN dbo.DanhMucXetNghiem dm ON dm.maDM = x.maDM
            WHERE @tt = N'' OR x.trangThai = @tt
            ORDER BY CASE x.trangThai WHEN N'Đã chỉ định' THEN 0 ELSE 1 END, x.ngayChiDinh DESC",
            P("@tt", cboLoc.SelectedIndex <= 0 ? "" : cboLoc.Text)),
            "", "Mã phiếu khám", "Bệnh nhân", "Xét nghiệm", "Ngày chỉ định", "Kết quả", "Trạng thái"));
    }
}
