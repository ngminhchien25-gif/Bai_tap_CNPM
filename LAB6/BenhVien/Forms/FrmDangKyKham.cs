using System.Data;
using System.Drawing;
using System.Windows.Forms;
using static BenhVien.DataAccess.DatabaseHelper;

namespace BenhVien.Forms;

/// <summary>UC-03 Đăng ký khám bệnh: tra cứu hồ sơ, chọn khoa + bác sĩ, tạo phiếu khám bằng usp_TaoPhieuKham (BR06, BR09).</summary>
public class FrmDangKyKham : FrmBase
{
    readonly TextBox txtTim = Ui.Txt(), txtTen = Ui.Txt(readOnly: true), txtHan = Ui.Txt(readOnly: true), txtTT = Ui.Txt(readOnly: true);
    readonly TextBox txtTrieuChung = Ui.Txt(multiline: true);
    readonly ComboBox cboKhoa = Ui.Cbo(), cboBS = Ui.Cbo(), cboLich = Ui.Cbo();
    readonly ComboBox cboLoai = Ui.Cbo("Khám bệnh", "Khám sức khỏe định kỳ");
    readonly DataGridView grid = Ui.Grid();
    string _maHS;

    public FrmDangKyKham() : base("Đăng ký khám bệnh", "UC-03 · Nhân viên tiếp nhận", 1050, 740)
    {
        cboLich.Enabled = false;
        cboLoai.SelectedIndexChanged += (_, _) => cboLich.Enabled = cboLoai.SelectedIndex == 1;   // BR06
        cboKhoa.SelectedIndexChanged += (_, _) => LoadBS();

        Ui.TryDb(() => Ui.Fill(cboKhoa, ExecuteQuery("SELECT maKhoa, tenKhoa FROM dbo.Khoa ORDER BY tenKhoa"), "tenKhoa", "maKhoa"));
        LoadBS();
        LoadHangCho();

        var btnTim = Ui.Btn("Tra cứu", width: 120);
        btnTim.Click += (_, _) => TimHoSo();

        var g1 = Ui.Group("1. Tra cứu hồ sơ bệnh án", Ui.SearchBar("Mã hồ sơ / họ tên / SĐT:", txtTim, btnTim));
        var g2 = Ui.Group("Thông tin hồ sơ", Ui.Fields(3, ("Bệnh nhân", txtTen), ("Hạn hồ sơ", txtHan), ("Hiệu lực", txtTT)));
        var g3 = Ui.Group("2. Chọn khoa & bác sĩ khám",
            Ui.Fields(2, ("Khoa", cboKhoa), ("Bác sĩ", cboBS), ("Loại khám", cboLoai), ("Lịch hẹn định kỳ", cboLich),
                         ("Triệu chứng khai báo", txtTrieuChung)));
        Stack(Ui.Group("Hàng chờ khám hôm nay", grid, DockStyle.Fill), g1, g2, g3);

        var btnTao = Ui.Btn("Tạo phiếu khám", width: 170);
        var btnGH = Ui.Btn("Gia hạn hồ sơ", Ui.Orange, 150);
        AddActions(btnTao, btnGH);

        btnGH.Click += (_, _) => { using var f = new FrmLePhi(); f.ShowDialog(this); };

        btnTao.Click += (_, _) =>
        {
            if (_maHS == null) { Ui.Alert("Hãy tra cứu hồ sơ bệnh nhân trước."); return; }
            if (cboBS.SelectedValue == null) { Ui.Alert("Khoa này chưa có bác sĩ."); return; }
            object maLH = cboLoai.SelectedIndex == 1 ? cboLich.SelectedValue : null;
            var pOut = POut("@maPK", SqlDbType.VarChar, 12);

            bool ok = Ui.TryDb(() =>
            {
                ExecuteProc("dbo.usp_TaoPhieuKham",
                    P("@maHS", _maHS), P("@maBS", cboBS.SelectedValue), P("@maKhoa", cboKhoa.SelectedValue),
                    P("@loaiKham", cboLoai.Text), P("@maLH", maLH), pOut);
                if (txtTrieuChung.Text.Trim() != "")
                    ExecuteNonQuery("INSERT dbo.ChiTietTrieuChung (maPK, ten, giaTri) VALUES (@pk, N'Triệu chứng khai báo', @gt)",
                        P("@pk", pOut.Value), P("@gt", txtTrieuChung.Text.Trim()));
            });
            if (!ok) return;
            Ui.Msg("Đã tạo phiếu khám " + pOut.Value + " (ghi tạm) cho bác sĩ " + cboBS.Text + ".");
            txtTrieuChung.Clear();
            TimHoSo(_maHS);       // làm mới danh sách lịch hẹn (lịch đã dùng sẽ biến mất)
            LoadHangCho();
        };
    }

    void LoadBS()
    {
        Ui.TryDb(() =>
        {
            var dt = cboKhoa.SelectedValue is string k
                ? ExecuteQuery("SELECT maBS, hoTen FROM dbo.BacSi WHERE maKhoa = @k ORDER BY hoTen", P("@k", k))
                : new DataTable { Columns = { "maBS", "hoTen" } };
            Ui.Fill(cboBS, dt, "hoTen", "maBS");
        });
    }

    void TimHoSo(string key = null)
    {
        key ??= txtTim.Text.Trim();
        if (key == "") { Ui.Alert("Vui lòng nhập mã hồ sơ hoặc thông tin tìm kiếm."); return; }
        Ui.TryDb(() =>
        {
            var dt = ExecuteProcTable("dbo.usp_TraCuuHoSo", P("@tuKhoa", key));
            if (dt.Rows.Count == 0) { _maHS = null; Ui.Alert("Không tìm thấy hồ sơ."); return; }
            if (dt.Rows.Count > 1) { _maHS = null; Ui.Alert("Có " + dt.Rows.Count + " kết quả, hãy nhập chính xác mã hồ sơ."); return; }

            var r = dt.Rows[0];
            _maHS = Ui.S(r["maHS"]);
            txtTim.Text = _maHS;
            txtTen.Text = Ui.S(r["hoTen"]);
            txtHan.Text = Ui.D(r["ngayHetHan"]);
            bool con = Ui.S(r["hieuLuc"]) == "Hiệu lực";
            txtTT.Text = Ui.S(r["hieuLuc"]);
            txtTT.ForeColor = con ? Ui.Primary : Ui.Red;
            if (!con) Ui.Alert("Hồ sơ đã hết hạn. Cần gia hạn mới được khám sức khỏe định kỳ (BR05); vẫn có thể đăng ký khám bệnh thường.");

            Ui.Fill(cboLich, ExecuteQuery(@"
                SELECT lh.maLH,
                       N'Q' + CAST(lh.quy AS NVARCHAR(1)) + N'/' + CAST(lh.nam AS NVARCHAR(4)) + N' – ' + CONVERT(NVARCHAR(16), lh.ngayGioHen, 120) AS ten
                FROM dbo.LichHen lh
                WHERE lh.maHS = @m AND lh.trangThai = N'Đã hẹn'
                  AND NOT EXISTS (SELECT 1 FROM dbo.PhieuKhamBenh p WHERE p.maLH = lh.maLH)
                ORDER BY lh.ngayGioHen", P("@m", _maHS)), "ten", "maLH");
        });
    }

    void LoadHangCho()
    {
        Ui.TryDb(() => Ui.Bind(grid, ExecuteQuery(@"
            SELECT pk.maPK, bn.hoTen, k.tenKhoa, bs.hoTen AS bacSi, pk.loaiKham, pk.ngayGioKham, pk.trangThai
            FROM dbo.PhieuKhamBenh pk
            JOIN dbo.HoSoBenhAn hs ON hs.maHS = pk.maHS
            JOIN dbo.BenhNhan bn ON bn.maBN = hs.maBN
            JOIN dbo.Khoa k ON k.maKhoa = pk.maKhoa
            JOIN dbo.BacSi bs ON bs.maBS = pk.maBS
            WHERE CAST(pk.ngayGioKham AS DATE) = CAST(SYSDATETIME() AS DATE)
            ORDER BY pk.ngayGioKham"),
            "Mã phiếu khám", "Bệnh nhân", "Khoa", "Bác sĩ", "Loại khám", "Giờ khám", "Trạng thái"));
    }
}
