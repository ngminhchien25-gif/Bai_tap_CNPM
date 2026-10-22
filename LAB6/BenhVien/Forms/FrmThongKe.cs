using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using static BenhVien.DataAccess.DatabaseHelper;

namespace BenhVien.Forms;

/// <summary>UC-23 Thống kê – báo cáo theo tháng: lượt khám, hồ sơ mới, doanh thu, phản ứng thuốc.</summary>
public class FrmThongKe : FrmBase
{
    readonly DateTimePicker dtpTu = Ui.Date(), dtpDen = Ui.Date();
    readonly ComboBox cboLoai = Ui.Cbo("Lượt khám", "Hồ sơ mới", "Doanh thu", "Phản ứng thuốc");
    readonly DataGridView grid = Ui.Grid();
    readonly Panel chart = new() { Height = 190, BackColor = Color.White };
    readonly Panel cTong, cKham, cThu, cHan;
    string[] _labels = Array.Empty<string>();
    double[] _values = Array.Empty<double>();

    public FrmThongKe() : base("Thống kê – báo cáo", "UC-23 · Quản trị hệ thống", 1100, 780)
    {
        dtpTu.Value = new DateTime(DateTime.Today.Year, 1, 1);
        dtpTu.Width = 130; dtpDen.Width = 130; cboLoai.Width = 170;

        var btnXem = Ui.Btn("Xem báo cáo", width: 140);
        var filter = Ui.Group("Bộ lọc báo cáo",
            Ui.Row(Ui.Lbl("Từ ngày:", true), dtpTu, Ui.Lbl("Đến ngày:", true), dtpDen, Ui.Lbl("Biểu đồ:", true), cboLoai, btnXem));

        cTong = Ui.Card("Tổng số bệnh nhân", "0", Ui.Primary);
        cKham = Ui.Card("Lượt khám trong kỳ", "0", Ui.Blue);
        cThu = Ui.Card("Doanh thu (triệu VNĐ)", "0", Ui.Orange);
        cHan = Ui.Card("Hồ sơ hết hạn trong 30 ngày", "0", Ui.Red);
        var cards = Ui.Row(cTong, cKham, cThu, cHan);

        chart.Paint += VeBieuDo;
        chart.Resize += (_, _) => chart.Invalidate();
        var chartGroup = Ui.Group("Biểu đồ theo tháng", chart);

        Stack(Ui.Group("Số liệu chi tiết theo tháng", grid, DockStyle.Fill), filter, cards, chartGroup);

        btnXem.Click += (_, _) => Xem();
        cboLoai.SelectedIndexChanged += (_, _) => { if (grid.DataSource != null) CapNhatBieuDo(); };

        var btnXuat = Ui.Btn("Xuất CSV", Ui.Blue, 130);
        AddActions(btnXuat);
        btnXuat.Click += (_, _) => XuatCsv();

        Xem();
    }

    void Xem()
    {
        if (dtpTu.Value.Date > dtpDen.Value.Date) { Ui.Alert("Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc."); return; }
        Ui.TryDb(() =>
        {
            var dt = ExecuteQuery(@"
                WITH m AS (
                    SELECT DATEFROMPARTS(YEAR(@tu), MONTH(@tu), 1) AS d
                    UNION ALL
                    SELECT DATEADD(MONTH, 1, d) FROM m WHERE DATEADD(MONTH, 1, d) <= @den)
                SELECT FORMAT(m.d, 'MM/yyyy') AS thang,
                  (SELECT COUNT(*) FROM dbo.PhieuKhamBenh pk WHERE pk.ngayGioKham >= m.d AND pk.ngayGioKham < DATEADD(MONTH, 1, m.d)) AS luotKham,
                  (SELECT COUNT(*) FROM dbo.HoSoBenhAn hs WHERE hs.ngayCap >= m.d AND hs.ngayCap < DATEADD(MONTH, 1, m.d)) AS hoSoMoi,
                  (SELECT ISNULL(SUM(hd.tongTien), 0) FROM dbo.HoaDonVienPhi hd
                     WHERE hd.trangThai = N'Đã thanh toán' AND hd.ngayLap >= m.d AND hd.ngayLap < DATEADD(MONTH, 1, m.d)) AS doanhThu,
                  (SELECT COUNT(*) FROM dbo.PhanUngThuoc pu WHERE pu.ngayGhiNhan >= m.d AND pu.ngayGhiNhan < DATEADD(MONTH, 1, m.d)) AS phanUng
                FROM m ORDER BY m.d OPTION (MAXRECURSION 600)",
                PDate("@tu", dtpTu.Value), PDate("@den", dtpDen.Value));
            Ui.Bind(grid, dt, "Tháng", "Lượt khám", "Hồ sơ mới", "Doanh thu (VNĐ)", "Phản ứng thuốc");

            SetCard(cTong, Convert.ToInt32(ExecuteScalar("SELECT COUNT(*) FROM dbo.BenhNhan")).ToString("N0"));
            SetCard(cKham, dt.Rows.Cast<DataRow>().Sum(r => Convert.ToInt32(r["luotKham"])).ToString("N0"));
            SetCard(cThu, (dt.Rows.Cast<DataRow>().Sum(r => Convert.ToDecimal(r["doanhThu"])) / 1_000_000m).ToString("N1"));
            SetCard(cHan, Convert.ToInt32(ExecuteScalar(@"
                SELECT COUNT(*) FROM dbo.HoSoBenhAn
                WHERE ngayHetHan BETWEEN CAST(SYSDATETIME() AS DATE) AND DATEADD(DAY, 30, CAST(SYSDATETIME() AS DATE))")).ToString("N0"));
            CapNhatBieuDo();
        });
    }

    static void SetCard(Panel card, string text) => ((Label)card.Controls["value"]).Text = text;

    void CapNhatBieuDo()
    {
        var dt = (DataTable)grid.DataSource;
        string col = new[] { "luotKham", "hoSoMoi", "doanhThu", "phanUng" }[Math.Max(0, cboLoai.SelectedIndex)];
        var rows = dt.Rows.Cast<DataRow>().TakeLast(12).ToList();
        _labels = rows.Select(r => Ui.S(r["thang"])).ToArray();
        _values = rows.Select(r => Convert.ToDouble(r[col])).ToArray();
        chart.Invalidate();
    }

    void VeBieuDo(object sender, PaintEventArgs e)
    {
        if (_values.Length == 0) return;
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        bool tien = cboLoai.SelectedIndex == 2;
        int n = _values.Length, w = chart.Width - 60, h = chart.Height - 50;
        int bw = Math.Max(1, w / Math.Max(n, 1));
        double max = Math.Max(_values.Max(), 1);
        using var brush = new SolidBrush(Ui.Primary);
        using var small = new Font("Segoe UI", 8.5F);
        for (int i = 0; i < n; i++)
        {
            int bh = (int)(_values[i] / max * h);
            var r = new Rectangle(40 + i * bw + 6, 15 + h - bh, Math.Max(1, bw - 12), bh);
            g.FillRectangle(brush, r);
            var txt = tien ? (_values[i] / 1_000_000).ToString("0.#") + "tr" : _values[i].ToString("0");
            g.DrawString(txt, small, Brushes.Black, r.X, r.Y - 16);
            g.DrawString(_labels[i], small, Brushes.Gray, r.X, 15 + h + 4);
        }
    }

    void XuatCsv()
    {
        if (grid.DataSource is not DataTable dt || dt.Rows.Count == 0) { Ui.Alert("Chưa có dữ liệu để xuất."); return; }
        using var dlg = new SaveFileDialog { Filter = "CSV|*.csv", FileName = "thong_ke.csv" };
        if (dlg.ShowDialog() != DialogResult.OK) return;
        var lines = new List<string> { string.Join(",", dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName)) };
        lines.AddRange(dt.Rows.Cast<DataRow>().Select(r => string.Join(",", r.ItemArray.Select(v => "\"" + Ui.S(v).Replace("\"", "\"\"") + "\""))));
        File.WriteAllLines(dlg.FileName, lines, new System.Text.UTF8Encoding(true));
        Ui.Msg("Đã xuất " + dlg.FileName);
    }
}
