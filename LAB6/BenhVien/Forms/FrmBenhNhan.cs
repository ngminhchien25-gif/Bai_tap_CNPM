using System.Drawing;
using System.Windows.Forms;
using static BenhVien.DataAccess.DatabaseHelper;

namespace BenhVien.Forms;

/// <summary>UC-02 Quản lý thông tin bệnh nhân (bảng BenhNhan).</summary>
public class FrmBenhNhan : FrmBase
{
    readonly TextBox txtMa = Ui.Txt(readOnly: true), txtTen = Ui.Txt(), txtDiaChi = Ui.Txt(), txtDT = Ui.Txt(), txtTim = Ui.Txt();
    readonly DateTimePicker dtpNgaySinh = Ui.Date();
    readonly DataGridView grid = Ui.Grid();
    readonly PictureBox pic;
    string _ma = "";   // rỗng = đang nhập bệnh nhân mới

    public FrmBenhNhan() : base("Quản lý thông tin bệnh nhân", "UC-02 · Nhân viên tiếp nhận")
    {
        var btnTim = Ui.Btn("Tìm kiếm", width: 110);
        btnTim.Click += (_, _) => LoadData();
        var search = Ui.Group("Tìm kiếm bệnh nhân", Ui.SearchBar("Mã BN / họ tên / SĐT / mã hồ sơ:", txtTim, btnTim));

        var fields = Ui.Fields(2,
            ("Mã bệnh nhân", txtMa), ("Họ và tên (*)", txtTen),
            ("Ngày sinh", dtpNgaySinh), ("Điện thoại (*)", txtDT),
            ("Địa chỉ (*)", txtDiaChi));
        var info = Ui.Group("Thông tin bệnh nhân", fields);
        info.AutoSize = false; info.Height = 210;
        fields.Dock = DockStyle.Fill;
        info.Controls.Add(Ui.PhotoBox(out pic));

        Stack(Ui.Group("Danh sách bệnh nhân", grid, DockStyle.Fill), search, info);

        var btnMoi = Ui.Btn("Thêm mới", Ui.Blue, 120);
        var btnLuu = Ui.Btn("Lưu", width: 100);
        var btnXoa = Ui.Btn("Xóa", Ui.Red, 100);
        AddActions(btnMoi, btnLuu, btnXoa);

        btnMoi.Click += (_, _) =>
        {
            _ma = ""; txtMa.Text = "(tự sinh)";
            txtTen.Clear(); txtDT.Clear(); txtDiaChi.Clear(); pic.Image = null;
            dtpNgaySinh.Value = DateTime.Today.AddYears(-30);
            txtTen.Focus();
        };

        btnLuu.Click += (_, _) =>
        {
            if (txtTen.Text.Trim() == "" || txtDT.Text.Trim() == "" || txtDiaChi.Text.Trim() == "")
            {
                Ui.Alert("Vui lòng nhập họ tên, điện thoại và địa chỉ.");
                return;
            }
            bool ok = Ui.TryDb(() =>
            {
                var photo = Ui.ImageBytes(pic.Image);
                if (_ma == "")
                    ExecuteNonQuery(@"INSERT dbo.BenhNhan (hoTen, diaChi, ngaySinh, dienThoai, anh3x4)
                                      VALUES (@t, @d, @n, @s, @a)",
                        P("@t", txtTen.Text.Trim()), P("@d", txtDiaChi.Text.Trim()), PDate("@n", dtpNgaySinh.Value),
                        P("@s", txtDT.Text.Trim()), PBytes("@a", photo));
                else
                    ExecuteNonQuery(@"UPDATE dbo.BenhNhan SET hoTen=@t, diaChi=@d, ngaySinh=@n, dienThoai=@s, anh3x4=@a WHERE maBN=@m",
                        P("@t", txtTen.Text.Trim()), P("@d", txtDiaChi.Text.Trim()), PDate("@n", dtpNgaySinh.Value),
                        P("@s", txtDT.Text.Trim()), PBytes("@a", photo), P("@m", _ma));
            });
            if (!ok) return;
            Ui.Msg("Đã lưu thông tin bệnh nhân.");
            _ma = "";
            LoadData();
        };

        btnXoa.Click += (_, _) =>
        {
            if (_ma == "") { Ui.Alert("Hãy chọn bệnh nhân cần xóa."); return; }
            if (!Ui.Confirm("Xóa bệnh nhân " + txtTen.Text + "?")) return;
            if (Ui.TryDb(() => ExecuteNonQuery("DELETE dbo.BenhNhan WHERE maBN = @m", P("@m", _ma))))
            {
                _ma = ""; LoadData();
            }
        };

        grid.SelectionChanged += (_, _) =>
        {
            var r = Ui.Sel(grid);
            if (r == null) return;
            _ma = Ui.S(r["maBN"]);
            txtMa.Text = _ma;
            txtTen.Text = Ui.S(r["hoTen"]);
            if (r["ngaySinh"] is DateTime d) dtpNgaySinh.Value = d;
            txtDiaChi.Text = Ui.S(r["diaChi"]);
            txtDT.Text = Ui.S(r["dienThoai"]);
            Ui.TryDb(() =>
            {
                pic.Image?.Dispose();
                pic.Image = Ui.BytesImage(ExecuteScalar("SELECT anh3x4 FROM dbo.BenhNhan WHERE maBN = @m", P("@m", _ma)));
            });
        };

        LoadData();
    }

    void LoadData()
    {
        Ui.TryDb(() =>
        {
            var dt = ExecuteQuery(@"
                SELECT bn.maBN, bn.hoTen, bn.ngaySinh, bn.diaChi, bn.dienThoai, hs.maHS
                FROM dbo.BenhNhan bn LEFT JOIN dbo.HoSoBenhAn hs ON hs.maBN = bn.maBN
                WHERE @kw = N'' OR bn.maBN = @kw OR hs.maHS = @kw
                   OR bn.hoTen LIKE N'%' + @kw + N'%' OR bn.dienThoai LIKE N'%' + @kw + N'%'
                ORDER BY bn.maBN DESC", P("@kw", txtTim.Text.Trim()));
            Ui.Bind(grid, dt, "Mã BN", "Họ tên", "Ngày sinh", "Địa chỉ", "Điện thoại", "Mã hồ sơ");
        });
    }
}
