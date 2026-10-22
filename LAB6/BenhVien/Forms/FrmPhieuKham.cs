using System.Data;
using System.Drawing;
using System.Windows.Forms;
using static BenhVien.DataAccess.DatabaseHelper;

namespace BenhVien.Forms;

/// <summary>UC-09, 10, 11, 13 Phiếu khám của bác sĩ: triệu chứng, kết luận, chỉ định/xem xét nghiệm, lịch sử bệnh án.</summary>
public class FrmPhieuKham : FrmBase
{
    readonly ComboBox cboPhieu = Ui.Cbo();
    readonly TextBox txtMaHS = Ui.Txt(readOnly: true), txtBN = Ui.Txt(readOnly: true), txtKhoa = Ui.Txt(readOnly: true), txtBS = Ui.Txt(readOnly: true);
    readonly TextBox txtLoai = Ui.Txt(readOnly: true), txtTT = Ui.Txt(readOnly: true), txtNgay = Ui.Txt(readOnly: true);
    readonly TextBox txtGiaTri = Ui.Txt(), txtDonVi = Ui.Txt(), txtKetLuan = Ui.Txt(multiline: true);
    readonly ComboBox cboTC = new() { Font = Ui.Normal };
    readonly ComboBox cboXN = Ui.Cbo();
    readonly DataGridView gridTC = Ui.Grid(), gridXN = Ui.Grid(), gridLS = Ui.Grid(), gridPU = Ui.Grid();
    string _maPK, _maHS;
    bool _loading;

    public FrmPhieuKham() : base("Phiếu khám bệnh", "UC-09, UC-10, UC-11, UC-13 · Bác sĩ", 1100, 760)
    {
        cboTC.Items.AddRange(SampleData.TrieuChung);
        cboTC.SelectedIndex = 0;
        Ui.TryDb(() => Ui.Fill(cboXN, ExecuteQuery("SELECT maDM, tenXN FROM dbo.DanhMucXetNghiem ORDER BY tenXN"), "tenXN", "maDM"));

        // ----- Tab 1: triệu chứng & chỉ số -----
        cboTC.Width = 200; txtGiaTri.Width = 120; txtDonVi.Width = 80;
        var btnThemTC = Ui.Btn("Thêm", Ui.Blue, 90);
        var btnXoaTC = Ui.Btn("Xóa dòng", Ui.Red, 100);
        btnThemTC.Click += (_, _) =>
        {
            if (!CoPhieu()) return;
            if (cboTC.Text.Trim() == "") { Ui.Alert("Nhập tên triệu chứng / chỉ số."); return; }
            if (Ui.TryDb(() => ExecuteNonQuery("INSERT dbo.ChiTietTrieuChung (maPK, ten, giaTri, donVi) VALUES (@pk, @t, NULLIF(@g, N''), NULLIF(@d, N''))",
                    P("@pk", _maPK), P("@t", cboTC.Text.Trim()), P("@g", txtGiaTri.Text.Trim()), P("@d", txtDonVi.Text.Trim()))))
            {
                txtGiaTri.Clear(); txtDonVi.Clear(); LoadTC();
            }
        };
        btnXoaTC.Click += (_, _) =>
        {
            var r = Ui.Sel(gridTC);
            if (r != null && Ui.TryDb(() => ExecuteNonQuery("DELETE dbo.ChiTietTrieuChung WHERE maTC = @m", P("@m", r["maTC"])))) LoadTC();
        };
        var tabTC = Ui.Stacked(gridTC, Ui.Row(Ui.Lbl("Triệu chứng / chỉ số:"), cboTC, Ui.Lbl("Giá trị:"), txtGiaTri, Ui.Lbl("Đơn vị:"), txtDonVi, btnThemTC, btnXoaTC));

        // ----- Tab 2: chẩn đoán -----
        var tabKL = Ui.Stacked(txtKetLuan, Ui.Lbl("Chẩn đoán / kết luận của bác sĩ:", true));

        // ----- Tab 3: xét nghiệm -----
        cboXN.Width = 260;
        var btnChiDinh = Ui.Btn("Chỉ định", Ui.Blue, 100);
        var btnXemKQ = Ui.Btn("Xem kết quả", Ui.Gray, 130);
        btnChiDinh.Click += (_, _) =>
        {
            if (!CoPhieu() || cboXN.SelectedValue == null) return;
            if (Ui.TryDb(() => ExecuteProc("dbo.usp_ChiDinhXetNghiem", P("@maPK", _maPK), P("@maDM", cboXN.SelectedValue))))
            {
                Ui.Msg("Đã chỉ định xét nghiệm \"" + cboXN.Text + "\". Nhân viên xét nghiệm sẽ nhận yêu cầu.");
                LoadXN();
            }
        };
        btnXemKQ.Click += (_, _) =>
        {
            var r = Ui.Sel(gridXN);
            if (r == null) { Ui.Alert("Hãy chọn một xét nghiệm."); return; }
            if (Ui.S(r["trangThai"]) != "Đã có kết quả") Ui.Alert("Xét nghiệm này chưa có kết quả.");
            else Ui.Msg(Ui.S(r["tenXN"]) + ":\n" + Ui.S(r["ketQua"]));
        };
        var tabXN = Ui.Stacked(gridXN, Ui.Row(Ui.Lbl("Xét nghiệm:"), cboXN, btnChiDinh, btnXemKQ));

        // ----- Tab 4: lịch sử bệnh án -----
        gridLS.Dock = DockStyle.Top; gridLS.Height = 170;
        var tabLS = Ui.Stacked(gridPU, Ui.Lbl("Lịch sử khám:", true), gridLS, Ui.Lbl("Phản ứng thuốc đã ghi nhận:", true));

        var tabs = Ui.Tabs(("Triệu chứng & chỉ số", tabTC), ("Chẩn đoán", tabKL), ("Xét nghiệm", tabXN), ("Lịch sử bệnh án", tabLS));

        var info = Ui.Group("Thông tin lần khám (mỗi phiếu thuộc một hồ sơ, một bác sĩ, một khoa – BR09)",
            Ui.Fields(3,
                ("Phiếu khám", cboPhieu), ("Mã hồ sơ", txtMaHS), ("Ngày giờ khám", txtNgay),
                ("Bệnh nhân", txtBN), ("Khoa", txtKhoa), ("Bác sĩ", txtBS),
                ("Loại khám", txtLoai), ("Trạng thái", txtTT)));
        Stack(tabs, info);

        cboPhieu.SelectedIndexChanged += (_, _) => { if (!_loading) LoadPhieu(); };

        var btnLamMoi = Ui.Btn("Làm mới", Ui.Gray, 110);
        var btnLuu = Ui.Btn("Lưu kết luận", Ui.Blue, 150);
        var btnToa = Ui.Btn("Lập toa thuốc", width: 150);
        var btnXong = Ui.Btn("Hoàn tất khám", Ui.Orange, 150);
        AddActions(btnLamMoi, btnLuu, btnToa, btnXong);

        btnLamMoi.Click += (_, _) => LoadList(_maPK);
        btnLuu.Click += (_, _) => { if (LuuKetLuan()) Ui.Msg("Đã lưu kết luận của phiếu " + _maPK + "."); };
        btnToa.Click += (_, _) =>
        {
            if (!CoPhieu()) return;
            using var f = new FrmToaThuoc(_maPK);
            f.ShowDialog(this);
        };
        btnXong.Click += (_, _) =>
        {
            if (!CoPhieu()) return;
            if (txtKetLuan.Text.Trim() == "") { Ui.Alert("Hãy nhập chẩn đoán / kết luận trước khi hoàn tất."); return; }
            if (LuuKetLuan())
                Ui.Msg("Hoàn tất khám. Chuyển bệnh nhân đến thu ngân để thanh toán viện phí (phiếu vẫn ghi tạm – BR04).");
        };

        LoadList();
    }

    bool CoPhieu()
    {
        if (_maPK == null) { Ui.Alert("Chưa có phiếu khám nào được chọn."); return false; }
        return true;
    }

    bool LuuKetLuan()
    {
        if (!CoPhieu()) return false;
        int n = 0;
        if (!Ui.TryDb(() => n = ExecuteNonQuery("UPDATE dbo.PhieuKhamBenh SET ketLuan = NULLIF(@k, N'') WHERE maPK = @m AND trangThai = N'Ghi tạm'",
                P("@k", txtKetLuan.Text.Trim()), P("@m", _maPK)))) return false;
        if (n == 0) { Ui.Alert("Phiếu đã được ghi nhận vào hồ sơ, không thể sửa."); return false; }
        return true;
    }

    void LoadList(string keep = null)
    {
        _loading = true;
        Ui.TryDb(() =>
        {
            var dt = ExecuteQuery(@"
                SELECT pk.maPK, pk.maPK + N' – ' + bn.hoTen + N' (' + pk.loaiKham + N')' AS ten
                FROM dbo.PhieuKhamBenh pk
                JOIN dbo.HoSoBenhAn hs ON hs.maHS = pk.maHS
                JOIN dbo.BenhNhan bn ON bn.maBN = hs.maBN
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
        if (_maPK == null)
        {
            _maHS = null;
            foreach (var t in new[] { txtMaHS, txtBN, txtKhoa, txtBS, txtLoai, txtTT, txtNgay, txtKetLuan }) t.Clear();
            foreach (var g in new[] { gridTC, gridXN, gridLS, gridPU }) g.DataSource = null;
            return;
        }
        Ui.TryDb(() =>
        {
            var h = ExecuteQuery(@"
                SELECT pk.maHS, bn.hoTen, pk.ngayGioKham, k.tenKhoa, bs.hoTen AS bacSi, pk.loaiKham, pk.trangThai, pk.ketLuan
                FROM dbo.PhieuKhamBenh pk
                JOIN dbo.HoSoBenhAn hs ON hs.maHS = pk.maHS JOIN dbo.BenhNhan bn ON bn.maBN = hs.maBN
                JOIN dbo.Khoa k ON k.maKhoa = pk.maKhoa JOIN dbo.BacSi bs ON bs.maBS = pk.maBS
                WHERE pk.maPK = @m", P("@m", _maPK));
            var r = h.Rows[0];
            _maHS = Ui.S(r["maHS"]);
            txtMaHS.Text = _maHS; txtBN.Text = Ui.S(r["hoTen"]); txtNgay.Text = Ui.D(r["ngayGioKham"], "dd/MM/yyyy HH:mm");
            txtKhoa.Text = Ui.S(r["tenKhoa"]); txtBS.Text = Ui.S(r["bacSi"]); txtLoai.Text = Ui.S(r["loaiKham"]);
            txtTT.Text = Ui.S(r["trangThai"]); txtKetLuan.Text = Ui.S(r["ketLuan"]);
            LoadTC(); LoadXN();

            Ui.Bind(gridLS, ExecuteProcTable("dbo.usp_LayLichSuKham", P("@maHS", _maHS)),
                "Mã phiếu", "Ngày khám", "Loại khám", "Kết luận", "Trạng thái", "Bác sĩ", "Khoa", "Số XN", "Số thuốc");
            Ui.Bind(gridPU, ExecuteQuery(@"
                SELECT pu.ngayGhiNhan, t.tenThuoc, pu.moTa, pu.mucDo
                FROM dbo.PhanUngThuoc pu
                JOIN dbo.ChiTietToaThuoc ct ON ct.maCT = pu.maCT JOIN dbo.Thuoc t ON t.maThuoc = ct.maThuoc
                JOIN dbo.ToaThuoc tt ON tt.maToa = ct.maToa JOIN dbo.PhieuKhamBenh pk ON pk.maPK = tt.maPK
                WHERE pk.maHS = @m ORDER BY pu.ngayGhiNhan DESC", P("@m", _maHS)),
                "Ngày ghi nhận", "Thuốc", "Phản ứng", "Mức độ");
        });
    }

    void LoadTC() => Ui.TryDb(() => Ui.Bind(gridTC,
        ExecuteQuery("SELECT maTC, ten, giaTri, donVi FROM dbo.ChiTietTrieuChung WHERE maPK = @m ORDER BY maTC", P("@m", _maPK)),
        "", "Triệu chứng / chỉ số", "Giá trị", "Đơn vị"));

    void LoadXN() => Ui.TryDb(() => Ui.Bind(gridXN,
        ExecuteQuery(@"SELECT x.maXN, dm.tenXN, x.ngayChiDinh, x.ketQua, x.trangThai
                       FROM dbo.XetNghiem x JOIN dbo.DanhMucXetNghiem dm ON dm.maDM = x.maDM
                       WHERE x.maPK = @m ORDER BY x.ngayChiDinh", P("@m", _maPK)),
        "Mã XN", "Tên xét nghiệm", "Ngày chỉ định", "Kết quả", "Trạng thái"));
}
