using System.Data;
using System.Drawing;
using System.Windows.Forms;
using static BenhVien.DataAccess.DatabaseHelper;

namespace BenhVien.Forms;

/// <summary>UC-06 Tra cứu hồ sơ bệnh án: usp_TraCuuHoSo (mã hồ sơ / SĐT / họ tên – BR07) và usp_LayLichSuKham.</summary>
public class FrmTraCuu : FrmBase
{
    readonly TextBox txtTu = Ui.Txt();
    readonly DataGridView gridKQ = Ui.Grid(), gridLS = Ui.Grid(), gridPU = Ui.Grid();
    readonly SplitContainer split = new() { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal };

    public FrmTraCuu() : base("Tra cứu hồ sơ bệnh án", "UC-06 · Nhân viên tiếp nhận", 1050, 720)
    {
        txtTu.Width = 380;
        var btnTim = Ui.Btn("Tìm kiếm", width: 120);
        var search = Ui.Group("Tìm hồ sơ", Ui.Row(Ui.Lbl("Mã hồ sơ / SĐT / họ tên:", true), txtTu, btnTim));

        split.Panel1.Controls.Add(Ui.Group("Kết quả tìm kiếm", gridKQ, DockStyle.Fill));
        split.Panel2.Controls.Add(Ui.Tabs(("Lịch sử khám", gridLS), ("Phản ứng thuốc đã ghi nhận", gridPU)));
        Stack(split, search);
        Shown += (_, _) => split.SplitterDistance = Math.Max(120, split.Height / 2 - 40);

        btnTim.Click += (_, _) => Tim();
        AcceptButton = btnTim;

        gridKQ.SelectionChanged += (_, _) =>
        {
            var r = Ui.Sel(gridKQ);
            if (r == null) return;
            var maHS = Ui.S(r["maHS"]);
            Ui.TryDb(() =>
            {
                Ui.Bind(gridLS, ExecuteProcTable("dbo.usp_LayLichSuKham", P("@maHS", maHS)),
                    "Mã phiếu", "Ngày khám", "Loại khám", "Kết luận", "Trạng thái", "Bác sĩ", "Khoa", "Số XN", "Số thuốc");
                Ui.Bind(gridPU, ExecuteQuery(@"
                    SELECT pu.ngayGhiNhan, t.tenThuoc, pu.moTa, pu.mucDo
                    FROM dbo.PhanUngThuoc pu
                    JOIN dbo.ChiTietToaThuoc ct ON ct.maCT = pu.maCT
                    JOIN dbo.Thuoc t ON t.maThuoc = ct.maThuoc
                    JOIN dbo.ToaThuoc tt ON tt.maToa = ct.maToa
                    JOIN dbo.PhieuKhamBenh pk ON pk.maPK = tt.maPK
                    WHERE pk.maHS = @m ORDER BY pu.ngayGhiNhan DESC", P("@m", maHS)),
                    "Ngày ghi nhận", "Thuốc", "Phản ứng", "Mức độ");
            });
        };

        var btnDK = Ui.Btn("Đăng ký khám", Ui.Blue, 150);
        var btnGH = Ui.Btn("Gia hạn hồ sơ", Ui.Orange, 150);
        AddActions(btnDK, btnGH);
        btnDK.Click += (_, _) => { using var f = new FrmDangKyKham(); f.ShowDialog(this); };
        btnGH.Click += (_, _) => { using var f = new FrmLePhi(); f.ShowDialog(this); };
    }

    void Tim()
    {
        var kw = txtTu.Text.Trim();
        if (kw == "") { Ui.Alert("Vui lòng nhập mã hồ sơ, số điện thoại hoặc họ tên."); return; }
        Ui.TryDb(() =>
        {
            var dt = ExecuteProcTable("dbo.usp_TraCuuHoSo", P("@tuKhoa", kw));
            Ui.Bind(gridKQ, dt, "Mã hồ sơ", "Họ tên", "Ngày sinh", "Điện thoại", "Hạn hồ sơ", "Hiệu lực");
            if (dt.Rows.Count == 0)
            {
                gridLS.DataSource = null; gridPU.DataSource = null;
                Ui.Msg("Không tìm thấy hồ sơ phù hợp.");
            }
        });
    }
}
