using System.Data;
using QuanLyThuVien.DataAccess;

namespace QuanLyThuVien.Repositories
{
    public class TheThuVienRepository
    {
        // Chỉ lấy thẻ đang hoạt động để tránh mượn bằng thẻ khóa/hết hạn
        public DataTable GetActiveCards() => DatabaseHelper.ExecuteQuery(
            @"SELECT MaThe, MaND FROM TheThuVien WHERE TrangThai = N'HoatDong' ORDER BY MaThe");
    }
}