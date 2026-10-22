namespace BenhVien;

/// <summary>Dữ liệu mẫu để minh hoạ giao diện (chưa nối cơ sở dữ liệu).</summary>
public static class SampleData
{
    public static readonly string[] Khoa =
        { "Nội tổng quát", "Ngoại tổng quát", "Nhi", "Sản", "Tim mạch", "Da liễu" };

    public static readonly Dictionary<string, string[]> BacSiTheoKhoa = new()
    {
        ["Nội tổng quát"] = new[] { "BS. Trần Văn An", "BS. Lê Thị Bình" },
        ["Ngoại tổng quát"] = new[] { "BS. Phạm Quốc Cường", "BS. Võ Minh Đức" },
        ["Nhi"] = new[] { "BS. Nguyễn Thu Hà", "BS. Đặng Gia Huy" },
        ["Sản"] = new[] { "BS. Hoàng Mai Lan", "BS. Bùi Ngọc Anh" },
        ["Tim mạch"] = new[] { "BS. Lý Thanh Sơn", "BS. Trương Hải Yến" },
        ["Da liễu"] = new[] { "BS. Phan Khánh Linh" },
    };

    public static readonly string[] XetNghiem =
    {
        "Công thức máu", "Đường huyết", "Chức năng gan", "Chức năng thận",
        "Nước tiểu 10 thông số", "X-quang ngực"
    };

    public static readonly (string Ten, string HoatChat, string Dvt, string Cct)[] Thuoc =
    {
        ("Paracetamol 500mg", "Paracetamol", "Viên", "Suy gan nặng"),
        ("Amoxicillin 500mg", "Penicillin", "Viên", "Dị ứng Penicillin"),
        ("Ibuprofen 400mg", "Ibuprofen", "Viên", "Loét dạ dày – tá tràng"),
        ("Cetirizine 10mg", "Cetirizine", "Viên", "Suy thận nặng"),
        ("Omeprazole 20mg", "Omeprazole", "Viên", "Quá mẫn với thuốc"),
    };

    /// <summary>Hoạt chất bệnh nhân mẫu đã từng có phản ứng (dùng để minh hoạ BR08).</summary>
    public static readonly string[] DiUng = { "Penicillin" };

    public static readonly string[] TrieuChung =
        { "Nhức đầu", "Sốt", "Ho", "Thân nhiệt", "Huyết áp", "Nhịp tim", "Cân nặng" };

    public static readonly string[] HinhThucTT = { "Tiền mặt", "Thẻ ngân hàng", "Chuyển khoản" };
}
