using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using static BenhVien.DataAccess.DatabaseHelper;

namespace BenhVien.Forms;

/// <summary>UC-19 → UC-22 Danh mục (thuốc, xét nghiệm, khoa, bác sĩ, nhân viên) và tài khoản &amp; phân quyền.</summary>
public class FrmDanhMuc : FrmBase
{
    /// <summary>Một ô nhập của tab danh mục; Param là tên tham số SQL tương ứng.</summary>
    sealed record Fld(string Label, string Param, Control Ctl);

    public FrmDanhMuc() : base("Danh mục & tài khoản hệ thống", "UC-19, UC-20, UC-21, UC-22 · Quản trị hệ thống", 1100, 740)
    {
        var chkNgung = new CheckBox { Text = "Ngừng sử dụng", AutoSize = true, Font = Ui.Normal };
        var thuoc = CatalogTab(
            "SELECT maThuoc, tenThuoc, hoatChat, donViTinh, chongChiDinh, ngungSuDung FROM dbo.Thuoc ORDER BY maThuoc",
            new[] { "Mã thuốc", "Tên thuốc", "Hoạt chất", "Đơn vị tính", "Chống chỉ định", "Ngừng dùng" },
            new[]
            {
                new Fld("Mã thuốc", "@ma", Ui.Txt(readOnly: true)), new Fld("Tên thuốc (*)", "@ten", Ui.Txt()),
                new Fld("Hoạt chất (*)", "@hc", Ui.Txt()), new Fld("Đơn vị tính (*)", "@dvt", Ui.Txt()),
                new Fld("Chống chỉ định", "@cct", Ui.Txt()), new Fld("Tình trạng", "@ngung", chkNgung)
            },
            "INSERT dbo.Thuoc (tenThuoc, hoatChat, donViTinh, chongChiDinh, ngungSuDung) VALUES (@ten, @hc, @dvt, NULLIF(@cct, N''), @ngung)",
            "UPDATE dbo.Thuoc SET tenThuoc=@ten, hoatChat=@hc, donViTinh=@dvt, chongChiDinh=NULLIF(@cct, N''), ngungSuDung=@ngung WHERE maThuoc=@ma",
            "DELETE dbo.Thuoc WHERE maThuoc=@ma");

        var xn = CatalogTab(
            "SELECT maDM, tenXN, donGia, moTa FROM dbo.DanhMucXetNghiem ORDER BY maDM",
            new[] { "Mã XN", "Tên xét nghiệm", "Đơn giá (VNĐ)", "Mô tả" },
            new[]
            {
                new Fld("Mã XN", "@ma", Ui.Txt(readOnly: true)), new Fld("Tên xét nghiệm (*)", "@ten", Ui.Txt()),
                new Fld("Đơn giá (VNĐ)", "@gia", Ui.Num(0, 100000000, 100000)), new Fld("Mô tả", "@mt", Ui.Txt())
            },
            "INSERT dbo.DanhMucXetNghiem (tenXN, donGia, moTa) VALUES (@ten, @gia, NULLIF(@mt, N''))",
            "UPDATE dbo.DanhMucXetNghiem SET tenXN=@ten, donGia=@gia, moTa=NULLIF(@mt, N'') WHERE maDM=@ma",
            "DELETE dbo.DanhMucXetNghiem WHERE maDM=@ma");

        var khoa = CatalogTab(
            "SELECT maKhoa, tenKhoa, moTa FROM dbo.Khoa ORDER BY maKhoa",
            new[] { "Mã khoa", "Tên khoa", "Mô tả" },
            new[]
            {
                new Fld("Mã khoa", "@ma", Ui.Txt(readOnly: true)), new Fld("Tên khoa (*)", "@ten", Ui.Txt()), new Fld("Mô tả", "@mt", Ui.Txt())
            },
            "INSERT dbo.Khoa (tenKhoa, moTa) VALUES (@ten, NULLIF(@mt, N''))",
            "UPDATE dbo.Khoa SET tenKhoa=@ten, moTa=NULLIF(@mt, N'') WHERE maKhoa=@ma",
            "DELETE dbo.Khoa WHERE maKhoa=@ma");

        var cboKhoa = Ui.Cbo();
        Ui.TryDb(() => Ui.Fill(cboKhoa, ExecuteQuery("SELECT maKhoa, tenKhoa FROM dbo.Khoa ORDER BY tenKhoa"), "tenKhoa", "maKhoa"));
        var bacsi = CatalogTab(
            "SELECT bs.maBS, bs.hoTen, bs.chuyenMon, bs.dienThoai, bs.maKhoa, k.tenKhoa FROM dbo.BacSi bs JOIN dbo.Khoa k ON k.maKhoa = bs.maKhoa ORDER BY bs.maBS",
            new[] { "Mã BS", "Họ tên", "Chuyên môn", "Điện thoại", "", "Khoa" },
            new[]
            {
                new Fld("Mã BS", "@ma", Ui.Txt(readOnly: true)), new Fld("Họ tên (*)", "@ten", Ui.Txt()),
                new Fld("Chuyên môn", "@cm", Ui.Txt()), new Fld("Điện thoại", "@dt", Ui.Txt()), new Fld("Khoa (*)", "@khoa", cboKhoa)
            },
            "INSERT dbo.BacSi (hoTen, chuyenMon, dienThoai, maKhoa) VALUES (@ten, NULLIF(@cm, N''), NULLIF(@dt, ''), @khoa)",
            "UPDATE dbo.BacSi SET hoTen=@ten, chuyenMon=NULLIF(@cm, N''), dienThoai=NULLIF(@dt, ''), maKhoa=@khoa WHERE maBS=@ma",
            "DELETE dbo.BacSi WHERE maBS=@ma");

        var nhanvien = CatalogTab(
            "SELECT maNV, hoTen, chucVu, dienThoai FROM dbo.NhanVien ORDER BY maNV",
            new[] { "Mã NV", "Họ tên", "Chức vụ", "Điện thoại" },
            new[]
            {
                new Fld("Mã NV", "@ma", Ui.Txt(readOnly: true)), new Fld("Họ tên (*)", "@ten", Ui.Txt()),
                new Fld("Chức vụ", "@cv", Ui.Cbo("Tiếp nhận", "CSKH", "Thu ngân", "Xét nghiệm", "Quản trị")), new Fld("Điện thoại", "@dt", Ui.Txt())
            },
            "INSERT dbo.NhanVien (hoTen, chucVu, dienThoai) VALUES (@ten, @cv, NULLIF(@dt, ''))",
            "UPDATE dbo.NhanVien SET hoTen=@ten, chucVu=@cv, dienThoai=NULLIF(@dt, '') WHERE maNV=@ma",
            "DELETE dbo.NhanVien WHERE maNV=@ma");

        Stack(Ui.Tabs(
            ("Thuốc", thuoc), ("Xét nghiệm", xn), ("Khoa", khoa), ("Bác sĩ", bacsi),
            ("Nhân viên", nhanvien), ("Tài khoản & phân quyền", AccountTab())));
    }

    /// <summary>Một tab danh mục: cột lưới thứ i ứng với ô nhập thứ i; ô đầu là khóa (chỉ đọc, do CSDL tự sinh).</summary>
    static Control CatalogTab(string select, string[] headers, Fld[] fields, string insert, string update, string delete)
    {
        var grid = Ui.Grid();
        void Reload() => Ui.TryDb(() => Ui.Bind(grid, ExecuteQuery(select), headers));

        SqlParameter[] Params(bool withKey)
        {
            return fields.Skip(withKey ? 0 : 1)
                         .Select(f => new SqlParameter(f.Param, Ui.GetValue(f.Ctl) ?? DBNull.Value))
                         .ToArray();
        }

        var btnThem = Ui.Btn("Thêm", width: 100);
        var btnSua = Ui.Btn("Cập nhật", Ui.Blue, 110);
        var btnXoa = Ui.Btn("Xóa", Ui.Red, 100);
        var btnMoi = Ui.Btn("Làm mới ô nhập", Ui.Gray, 150);

        btnThem.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(Ui.S(Ui.GetValue(fields[1].Ctl)))) { Ui.Alert("Vui lòng nhập các trường bắt buộc (*)."); return; }
            if (Ui.TryDb(() => ExecuteNonQuery(insert, Params(false)))) Reload();
        };
        btnSua.Click += (_, _) =>
        {
            if (Ui.S(Ui.GetValue(fields[0].Ctl)) == "") { Ui.Alert("Hãy chọn dòng cần cập nhật."); return; }
            if (Ui.TryDb(() => ExecuteNonQuery(update, Params(true)))) Reload();
        };
        btnXoa.Click += (_, _) =>
        {
            if (Ui.S(Ui.GetValue(fields[0].Ctl)) == "") { Ui.Alert("Hãy chọn dòng cần xóa."); return; }
            if (Ui.Confirm("Xóa dòng đang chọn?") && Ui.TryDb(() => ExecuteNonQuery(delete, Params(true)))) Reload();
        };
        btnMoi.Click += (_, _) =>
        {
            foreach (var f in fields) if (f.Ctl is TextBox) f.Ctl.Text = "";
        };
        grid.SelectionChanged += (_, _) =>
        {
            var r = Ui.Sel(grid);
            if (r == null) return;
            for (int i = 0; i < fields.Length; i++) Ui.SetValue(fields[i].Ctl, r[i]);
        };

        Reload();
        return Ui.Stacked(grid,
            Ui.Fields(2, fields.Select(f => (f.Label, f.Ctl)).ToArray()),
            Ui.Row(btnThem, btnSua, btnXoa, btnMoi));
    }

    /// <summary>UC-22: tạo tài khoản gắn với một nhân viên hoặc bác sĩ, đổi mật khẩu, xóa tài khoản.</summary>
    static Control AccountTab()
    {
        var grid = Ui.Grid();
        var txtUser = Ui.Txt();
        var txtPass = Ui.Txt(password: true);
        var cboNS = Ui.Cbo();

        void Reload() => Ui.TryDb(() => Ui.Bind(grid, ExecuteQuery(@"
            SELECT tk.maTK, tk.tenDangNhap, tk.vaiTro, COALESCE(nv.hoTen, bs.hoTen) AS hoTen
            FROM dbo.TaiKhoan tk
            LEFT JOIN dbo.NhanVien nv ON nv.maNV = tk.maNV
            LEFT JOIN dbo.BacSi bs ON bs.maBS = tk.maBS
            ORDER BY tk.tenDangNhap"), "", "Tên đăng nhập", "Vai trò", "Nhân sự"));

        Ui.TryDb(() => Ui.Fill(cboNS, ExecuteQuery(@"
            SELECT maNV AS ma, hoTen + N' – ' + chucVu AS ten, chucVu AS vaiTro FROM dbo.NhanVien
            UNION ALL
            SELECT maBS, hoTen + N' – Bác sĩ', N'Bác sĩ' FROM dbo.BacSi
            ORDER BY ten"), "ten", "ma"));

        var btnTao = Ui.Btn("Tạo tài khoản", width: 150);
        var btnDoiMK = Ui.Btn("Đổi mật khẩu", Ui.Blue, 150);
        var btnXoa = Ui.Btn("Xóa tài khoản", Ui.Red, 150);

        btnTao.Click += (_, _) =>
        {
            if (txtUser.Text.Trim() == "" || txtPass.Text.Length < 6) { Ui.Alert("Nhập tên đăng nhập và mật khẩu tối thiểu 6 ký tự."); return; }
            if (cboNS.SelectedValue is not string ma || cboNS.SelectedItem is not DataRowView item) { Ui.Alert("Hãy chọn nhân sự."); return; }
            bool laNV = ma.StartsWith("NV");
            if (Ui.TryDb(() => ExecuteNonQuery(
                    "INSERT dbo.TaiKhoan (tenDangNhap, matKhauHash, vaiTro, maNV, maBS) VALUES (@u, @h, @r, @nv, @bs)",
                    new SqlParameter("@u", SqlDbType.VarChar, 50) { Value = txtUser.Text.Trim() },
                    new SqlParameter("@h", SqlDbType.VarBinary, 64) { Value = Auth.Hash(txtUser.Text, txtPass.Text) },
                    P("@r", item["vaiTro"]),
                    new SqlParameter("@nv", SqlDbType.VarChar, 12) { Value = laNV ? ma : DBNull.Value },
                    new SqlParameter("@bs", SqlDbType.VarChar, 12) { Value = laNV ? DBNull.Value : ma })))
            {
                txtPass.Clear(); Reload();
            }
        };

        btnDoiMK.Click += (_, _) =>
        {
            var r = Ui.Sel(grid);
            if (r == null) { Ui.Alert("Hãy chọn tài khoản."); return; }
            if (txtPass.Text.Length < 6) { Ui.Alert("Mật khẩu mới tối thiểu 6 ký tự."); return; }
            var user = Ui.S(r["tenDangNhap"]);
            if (Ui.TryDb(() => ExecuteNonQuery("UPDATE dbo.TaiKhoan SET matKhauHash = @h WHERE maTK = @m",
                    new SqlParameter("@h", SqlDbType.VarBinary, 64) { Value = Auth.Hash(user, txtPass.Text) }, P("@m", r["maTK"]))))
            {
                txtPass.Clear(); Ui.Msg("Đã đổi mật khẩu của " + user + ".");
            }
        };

        btnXoa.Click += (_, _) =>
        {
            var r = Ui.Sel(grid);
            if (r == null) { Ui.Alert("Hãy chọn tài khoản."); return; }
            if (Ui.S(r["tenDangNhap"]) == Session.TenDangNhap) { Ui.Alert("Không thể xóa tài khoản đang đăng nhập."); return; }
            if (Ui.Confirm("Xóa tài khoản " + r["tenDangNhap"] + "?") && Ui.TryDb(() => ExecuteNonQuery("DELETE dbo.TaiKhoan WHERE maTK = @m", P("@m", r["maTK"])))) Reload();
        };

        grid.SelectionChanged += (_, _) =>
        {
            var r = Ui.Sel(grid);
            if (r != null) txtUser.Text = Ui.S(r["tenDangNhap"]);
        };

        Reload();
        return Ui.Stacked(grid,
            Ui.Fields(2, ("Tên đăng nhập", txtUser), ("Mật khẩu (≥ 6 ký tự)", txtPass), ("Nhân sự", cboNS)),
            Ui.Row(btnTao, btnDoiMK, btnXoa));
    }
}
