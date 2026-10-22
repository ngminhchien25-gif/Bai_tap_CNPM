using System.Data;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;
using BenhVien.DataAccess;

namespace BenhVien;

/// <summary>Thông tin người đăng nhập hiện tại.</summary>
public static class Session
{
    public static string TenDangNhap { get; set; } = "";
    public static string HoTen { get; set; } = "";
    public static string VaiTro { get; set; } = "";   // một trong các hằng số của Roles
    public static string MaNV { get; set; }           // null nếu là bác sĩ
    public static string MaBS { get; set; }           // null nếu là nhân viên
}

public static class Auth
{
    /// <summary>Mật khẩu lưu dạng SHA-512 của "tendangnhap:matkhau" (muối = tên đăng nhập). Không lưu mật khẩu thuần.</summary>
    public static byte[] Hash(string user, string pass) =>
        SHA512.HashData(Encoding.UTF8.GetBytes(user.Trim().ToLowerInvariant() + ":" + pass));

    static string MapRole(string dbRole) => dbRole switch
    {
        "Tiếp nhận" => Roles.TiepNhan,
        "CSKH" => Roles.CSKH,
        "Thu ngân" => Roles.ThuNgan,
        "Xét nghiệm" => Roles.XetNghiem,
        "Quản trị" => Roles.QuanTri,
        "Bác sĩ" => Roles.BacSi,
        _ => dbRole
    };

    /// <summary>Trả về true nếu đúng tài khoản; khi đó Session đã được điền.</summary>
    public static bool Login(string user, string pass)
    {
        var dt = DatabaseHelper.ExecuteQuery(@"
            SELECT tk.vaiTro, tk.maNV, tk.maBS, COALESCE(nv.hoTen, bs.hoTen) AS hoTen
            FROM dbo.TaiKhoan tk
            LEFT JOIN dbo.NhanVien nv ON nv.maNV = tk.maNV
            LEFT JOIN dbo.BacSi bs ON bs.maBS = tk.maBS
            WHERE tk.tenDangNhap = @u AND tk.matKhauHash = @h",
            new SqlParameter("@u", SqlDbType.VarChar, 50) { Value = user.Trim() },
            new SqlParameter("@h", SqlDbType.VarBinary, 64) { Value = Hash(user, pass) });

        if (dt.Rows.Count == 0) return false;
        var r = dt.Rows[0];
        Session.TenDangNhap = user.Trim();
        Session.HoTen = r["hoTen"].ToString();
        Session.VaiTro = MapRole(r["vaiTro"].ToString());
        Session.MaNV = r["maNV"] as string;
        Session.MaBS = r["maBS"] as string;
        return true;
    }
}
