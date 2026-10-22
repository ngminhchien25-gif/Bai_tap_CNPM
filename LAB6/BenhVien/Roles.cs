namespace BenhVien;

/// <summary>Các actor trong sơ đồ Usecase (Câu 3).</summary>
public static class Roles
{
    public const string TiepNhan = "Nhân viên tiếp nhận";
    public const string BenhNhan = "Bệnh nhân";
    public const string CSKH = "Nhân viên CSKH";
    public const string ThuNgan = "Nhân viên thu ngân";
    public const string BacSi = "Bác sĩ";
    public const string XetNghiem = "Nhân viên xét nghiệm";
    public const string QuanTri = "Quản trị hệ thống";

    public static readonly object[] All =
        { TiepNhan, BenhNhan, CSKH, ThuNgan, BacSi, XetNghiem, QuanTri };
}
