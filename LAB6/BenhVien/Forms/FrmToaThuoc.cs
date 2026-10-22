using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using static BenhVien.DataAccess.DatabaseHelper;

namespace BenhVien.Forms;

/// <summary>UC-14 Lập toa thuốc (usp_LapToaThuoc, BR08) và UC-15 Ghi nhận phản ứng phụ của thuốc.</summary>
public class FrmToaThuoc : FrmBase
{
    readonly ComboBox cboPhieu = Ui.Cbo(), cboThuoc = Ui.Cbo(), cboCT = Ui.Cbo();
    readonly TextBox txtMaToa = Ui.Txt(readOnly: true), txtBN = Ui.Txt(readOnly: true);
    readonly TextBox txtLoiDan = Ui.Txt(multiline: true), txtLieu = Ui.Txt(), txtMoTaPU = Ui.Txt();
    readonly DateTimePicker dtpPU = Ui.Date();
    readonly ComboBox cboCach = Ui.Cbo("Sau ăn", "Trước ăn", "Trong bữa ăn", "Khi cần");
    readonly ComboBox cboMuc = Ui.Cbo("Nhẹ", "Vừa", "Nặng");
    readonly NumericUpDown numSL = Ui.Num(1, 1000, 10), numNgay = Ui.Num(1, 90, 5);
    readonly DataGridView gridToa = Ui.Grid(), gridPU = Ui.Grid();
    readonly Label banner = new()
    {
        Dock = DockStyle.Top, AutoSize = false, Height = 48, Padding = new Padding(10, 6, 10, 6), Font = Ui.Bold
    };

    // Các dòng thuốc đang soạn (chưa lưu). Cột maThuoc/soLuong/lieuDung/cachDung/soNgay/lyDoXacNhanKe khớp kiểu dbo.ChiTietToaType.
    readonly DataTable _ct = new();
    string _maPK, _maHS;
    bool _daCoToa, _loading;

    public FrmToaThuoc(string maPK = null) : base("Lập toa thuốc", "UC-14, UC-15 · Bác sĩ", 1100, 780)
    {
        _ct.Columns.Add("maThuoc", typeof(string)); _ct.Columns.Add("tenThuoc", typeof(string));
        _ct.Columns.Add("soLuong", typeof(int)); _ct.Columns.Add("lieuDung", typeof(string));
        _ct.Columns.Add("cachDung", typeof(string)); _ct.Columns.Add("soNgay", typeof(int));
        _ct.Columns.Add("lyDoXacNhanKe", typeof(string));
        Ui.Bind(gridToa, _ct, "", "Tên thuốc", "Số lượng", "Liều dùng", "Cách dùng", "Số ngày", "Lý do xác nhận kê");

        Ui.TryDb(() => Ui.Fill(cboThuoc, ExecuteQuery("SELECT maThuoc, tenThuoc FROM dbo.Thuoc WHERE ngungSuDung = 0 ORDER BY tenThuoc"), "tenThuoc", "maThuoc"));

        var header = Ui.Group("Thông tin toa thuốc",
            Ui.Fields(2, ("Phiếu khám", cboPhieu), ("Bệnh nhân", txtBN), ("Mã toa", txtMaToa), ("Lời dặn của bác sĩ", txtLoiDan)));

        // ----- Tab toa thuốc -----
        var btnThem = Ui.Btn("Thêm thuốc", Ui.Blue, 130);
        var btnXoa = Ui.Btn("Xóa dòng", Ui.Red, 110);
        var btnKT = Ui.Btn("Xem chống chỉ định", Ui.Orange, 200);
        btnThem.Click += (_, _) => ThemThuoc();
        btnXoa.Click += (_, _) =>
        {
            if (_daCoToa) { Ui.Alert("Phiếu này đã có toa thuốc."); return; }
            if (gridToa.CurrentRow?.DataBoundItem is DataRowView v) v.Row.Delete();
        };
        btnKT.Click += (_, _) => XemChongChiDinh();
        var tabToa = Ui.Stacked(gridToa,
            Ui.Fields(3, ("Thuốc", cboThuoc), ("Số lượng", numSL), ("Số ngày", numNgay), ("Liều dùng (*)", txtLieu), ("Cách dùng", cboCach)),
            Ui.Row(btnThem, btnXoa, btnKT));

        // ----- Tab phản ứng phụ (UC-15) -----
        var btnGhiPU = Ui.Btn("Ghi nhận phản ứng", Ui.Blue, 190);
        btnGhiPU.Click += (_, _) => GhiPhanUng();
        var tabPU = Ui.Stacked(gridPU,
            Ui.Fields(2, ("Thuốc đã kê trước đây", cboCT), ("Mức độ", cboMuc), ("Ngày ghi nhận", dtpPU), ("Mô tả phản ứng", txtMoTaPU)),
            Ui.Row(btnGhiPU));

        Stack(Ui.Tabs(("Toa thuốc", tabToa), ("Phản ứng phụ của thuốc", tabPU)), banner, header);

        var btnLuu = Ui.Btn("Lưu toa", width: 120);
        var btnIn = Ui.Btn("In toa", Ui.Gray, 110);
        AddActions(btnLuu, btnIn);
        btnLuu.Click += (_, _) => LuuToa();
        btnIn.Click += (_, _) =>
        {
            if (txtMaToa.Text == "" || txtMaToa.Text.StartsWith("(")) Ui.Alert("Chưa có toa đã lưu để in.");
            else Ui.Msg("Đang in toa thuốc " + txtMaToa.Text + "...");
        };

        cboPhieu.SelectedIndexChanged += (_, _) => { if (!_loading) LoadPhieu(); };
        LoadList(maPK);
    }

    // ---------------- nạp dữ liệu ----------------
    void LoadList(string keep)
    {
        _loading = true;
        Ui.TryDb(() =>
        {
            var dt = ExecuteQuery(@"
                SELECT pk.maPK, pk.maPK + N' – ' + bn.hoTen AS ten
                FROM dbo.PhieuKhamBenh pk
                JOIN dbo.HoSoBenhAn hs ON hs.maHS = pk.maHS JOIN dbo.BenhNhan bn ON bn.maBN = hs.maBN
                WHERE pk.trangThai = N'Ghi tạm' AND (@bs = '' OR pk.maBS = @bs)
                ORDER BY pk.ngayGioKham DESC", P("@bs", Session.MaBS ?? ""));
            Ui.Fill(cboPhieu, dt, "ten", "maPK");
            if (keep != null && dt.Rows.Cast<DataRow>().Any(r => (string)r["maPK"] == keep)) cboPhieu.SelectedValue = keep;
        });
        _loading = false;
        LoadPhieu();
    }

    void LoadPhieu()
    {
        _maPK = cboPhieu.SelectedValue as string;
        _ct.Clear();
        _daCoToa = false;
        txtMaToa.Text = "(chưa lập)"; txtBN.Clear(); txtLoiDan.Clear();
        if (_maPK == null) { _maHS = null; banner.Text = ""; return; }

        Ui.TryDb(() =>
        {
            var h = ExecuteQuery("SELECT pk.maHS, bn.hoTen FROM dbo.PhieuKhamBenh pk JOIN dbo.HoSoBenhAn hs ON hs.maHS = pk.maHS JOIN dbo.BenhNhan bn ON bn.maBN = hs.maBN WHERE pk.maPK = @m", P("@m", _maPK));
            _maHS = Ui.S(h.Rows[0]["maHS"]);
            txtBN.Text = Ui.S(h.Rows[0]["hoTen"]) + " (" + _maHS + ")";

            // toa đã có (mỗi phiếu tối đa 1 toa) -> hiển thị, không cho sửa
            var toa = ExecuteQuery("SELECT maToa, loiDan FROM dbo.ToaThuoc WHERE maPK = @m", P("@m", _maPK));
            if (toa.Rows.Count > 0)
            {
                _daCoToa = true;
                txtMaToa.Text = Ui.S(toa.Rows[0]["maToa"]);
                txtLoiDan.Text = Ui.S(toa.Rows[0]["loiDan"]);
                foreach (DataRow r in ExecuteQuery(@"SELECT c.maThuoc, t.tenThuoc, c.soLuong, c.lieuDung, c.cachDung, c.soNgay, c.lyDoXacNhanKe
                                                     FROM dbo.ChiTietToaThuoc c JOIN dbo.Thuoc t ON t.maThuoc = c.maThuoc WHERE c.maToa = @t",
                                                     P("@t", toa.Rows[0]["maToa"])).Rows)
                    _ct.Rows.Add(r.ItemArray);
            }

            LoadPhanUng();
        });
    }

    void LoadPhanUng()
    {
        Ui.TryDb(() =>
        {
            // cảnh báo đầu trang
            var pu = ExecuteQuery(@"
                SELECT t.tenThuoc, pu.mucDo, pu.ngayGhiNhan
                FROM dbo.PhanUngThuoc pu
                JOIN dbo.ChiTietToaThuoc ct ON ct.maCT = pu.maCT JOIN dbo.Thuoc t ON t.maThuoc = ct.maThuoc
                JOIN dbo.ToaThuoc tt ON tt.maToa = ct.maToa JOIN dbo.PhieuKhamBenh pk ON pk.maPK = tt.maPK
                WHERE pk.maHS = @m ORDER BY pu.ngayGhiNhan DESC", P("@m", _maHS));
            if (pu.Rows.Count == 0)
            {
                banner.Text = "Hồ sơ chưa ghi nhận phản ứng phụ của thuốc nào.";
                banner.BackColor = Color.FromArgb(232, 245, 233); banner.ForeColor = Color.FromArgb(27, 94, 32);
            }
            else
            {
                banner.Text = "⚠ Hồ sơ đã ghi nhận phản ứng: " + string.Join("; ",
                    pu.Rows.Cast<DataRow>().Take(3).Select(r => r["tenThuoc"] + " (" + r["mucDo"] + ", " + Ui.D(r["ngayGhiNhan"]) + ")"))
                    + ". Kê lại thuốc này phải ghi lý do xác nhận (BR08).";
                banner.BackColor = Color.FromArgb(255, 243, 224); banner.ForeColor = Color.FromArgb(191, 54, 12);
            }

            Ui.Bind(gridPU, ExecuteQuery(@"
                SELECT pu.ngayGhiNhan, t.tenThuoc, pu.moTa, pu.mucDo
                FROM dbo.PhanUngThuoc pu
                JOIN dbo.ChiTietToaThuoc ct ON ct.maCT = pu.maCT JOIN dbo.Thuoc t ON t.maThuoc = ct.maThuoc
                JOIN dbo.ToaThuoc tt ON tt.maToa = ct.maToa JOIN dbo.PhieuKhamBenh pk ON pk.maPK = tt.maPK
                WHERE pk.maHS = @m ORDER BY pu.ngayGhiNhan DESC", P("@m", _maHS)),
                "Ngày ghi nhận", "Thuốc", "Phản ứng", "Mức độ");

            // danh sách dòng thuốc đã kê trước đây để ghi nhận phản ứng
            Ui.Fill(cboCT, ExecuteQuery(@"
                SELECT ct.maCT, t.tenThuoc + N' – toa ' + CONVERT(NVARCHAR(10), tt.ngayLap, 103) AS ten
                FROM dbo.ChiTietToaThuoc ct JOIN dbo.Thuoc t ON t.maThuoc = ct.maThuoc
                JOIN dbo.ToaThuoc tt ON tt.maToa = ct.maToa JOIN dbo.PhieuKhamBenh pk ON pk.maPK = tt.maPK
                WHERE pk.maHS = @m ORDER BY tt.ngayLap DESC", P("@m", _maHS)), "ten", "maCT");
        });
    }

    // ---------------- thao tác ----------------
    void ThemThuoc()
    {
        if (_maPK == null) { Ui.Alert("Chưa có phiếu khám nào được chọn."); return; }
        if (_daCoToa) { Ui.Alert("Phiếu này đã có toa thuốc."); return; }
        if (cboThuoc.SelectedValue is not string ma) { Ui.Alert("Hãy chọn thuốc."); return; }
        if (txtLieu.Text.Trim() == "") { Ui.Alert("Vui lòng nhập liều dùng."); return; }
        if (_ct.Rows.Cast<DataRow>().Any(r => r.RowState != DataRowState.Deleted && (string)r["maThuoc"] == ma))
        { Ui.Alert("Thuốc này đã có trong toa."); return; }

        string lyDo = null;
        bool huy = false;
        Ui.TryDb(() =>
        {
            var cct = Ui.S(ExecuteScalar("SELECT chongChiDinh FROM dbo.Thuoc WHERE maThuoc = @t", P("@t", ma)));
            if (cct != "" && !Ui.Confirm("Chống chỉ định của " + cboThuoc.Text + ":\n" + cct + "\n\nVẫn thêm vào toa?")) { huy = true; return; }

            int tungPU = Convert.ToInt32(ExecuteScalar(@"
                SELECT COUNT(*) FROM dbo.PhanUngThuoc pu
                JOIN dbo.ChiTietToaThuoc ct ON ct.maCT = pu.maCT
                JOIN dbo.ToaThuoc tt ON tt.maToa = ct.maToa JOIN dbo.PhieuKhamBenh pk ON pk.maPK = tt.maPK
                WHERE pk.maHS = @h AND ct.maThuoc = @t", P("@h", _maHS), P("@t", ma)));
            if (tungPU > 0)
            {
                lyDo = Ui.Prompt("BR08 – Bệnh nhân từng có phản ứng với thuốc này",
                                 "Nhập lý do xác nhận vẫn kê " + cboThuoc.Text + " (bỏ trống = không kê):");
                if (string.IsNullOrWhiteSpace(lyDo)) { huy = true; Ui.Alert("Không thêm thuốc vào toa."); }
            }
        });
        if (huy) return;

        _ct.Rows.Add(ma, cboThuoc.Text, (int)numSL.Value, txtLieu.Text.Trim(), cboCach.Text, (int)numNgay.Value, (object)lyDo ?? DBNull.Value);
        txtLieu.Clear();
    }

    void XemChongChiDinh()
    {
        var lines = new List<string>();
        Ui.TryDb(() =>
        {
            foreach (DataRow r in _ct.Rows.Cast<DataRow>().Where(x => x.RowState != DataRowState.Deleted))
            {
                var cct = Ui.S(ExecuteScalar("SELECT chongChiDinh FROM dbo.Thuoc WHERE maThuoc = @t", P("@t", r["maThuoc"])));
                lines.Add("• " + r["tenThuoc"] + ": " + (cct == "" ? "không có ghi chú chống chỉ định" : cct)
                          + (Ui.S(r["lyDoXacNhanKe"]) != "" ? "\n   (đã xác nhận kê: " + r["lyDoXacNhanKe"] + ")" : ""));
            }
        });
        Ui.Msg(lines.Count == 0 ? "Toa chưa có thuốc nào." : string.Join("\n", lines));
    }

    void LuuToa()
    {
        if (_maPK == null) { Ui.Alert("Chưa có phiếu khám nào được chọn."); return; }
        if (_daCoToa) { Ui.Alert("Phiếu này đã có toa " + txtMaToa.Text + "."); return; }
        var rows = _ct.Rows.Cast<DataRow>().Where(r => r.RowState != DataRowState.Deleted).ToList();
        if (rows.Count == 0) { Ui.Alert("Toa thuốc phải có ít nhất một thuốc."); return; }

        var tvp = new DataTable();
        tvp.Columns.Add("maThuoc", typeof(string)); tvp.Columns.Add("soLuong", typeof(int));
        tvp.Columns.Add("lieuDung", typeof(string)); tvp.Columns.Add("cachDung", typeof(string));
        tvp.Columns.Add("soNgay", typeof(int)); tvp.Columns.Add("lyDoXacNhanKe", typeof(string));
        foreach (var r in rows)
            tvp.Rows.Add(r["maThuoc"], r["soLuong"], r["lieuDung"], r["cachDung"], r["soNgay"], r["lyDoXacNhanKe"]);

        var pTvp = new SqlParameter("@chiTiet", SqlDbType.Structured) { TypeName = "dbo.ChiTietToaType", Value = tvp };
        var pOut = POut("@maToa", SqlDbType.VarChar, 12);
        if (!Ui.TryDb(() => ExecuteProc("dbo.usp_LapToaThuoc",
                P("@maPK", _maPK),
                P("@loiDan", string.IsNullOrWhiteSpace(txtLoiDan.Text) ? null : txtLoiDan.Text.Trim()),
                pTvp, pOut))) return;

        Ui.Msg("Đã lưu toa " + pOut.Value + " gắn với phiếu khám " + _maPK + " (ghi tạm).");
        LoadPhieu();
    }

    void GhiPhanUng()
    {
        if (cboCT.SelectedValue == null) { Ui.Alert("Hồ sơ này chưa có thuốc nào đã kê để ghi nhận phản ứng."); return; }
        if (txtMoTaPU.Text.Trim() == "") { Ui.Alert("Hãy mô tả phản ứng của bệnh nhân."); return; }
        if (!Ui.TryDb(() => ExecuteNonQuery(
                "INSERT dbo.PhanUngThuoc (maCT, moTa, mucDo, ngayGhiNhan) VALUES (@ct, @mt, @md, @n)",
                P("@ct", cboCT.SelectedValue), P("@mt", txtMoTaPU.Text.Trim()), P("@md", cboMuc.Text), PDate("@n", dtpPU.Value)))) return;
        txtMoTaPU.Clear();
        Ui.Msg("Đã ghi nhận phản ứng phụ vào hồ sơ để theo dõi.");
        LoadPhanUng();
    }
}
