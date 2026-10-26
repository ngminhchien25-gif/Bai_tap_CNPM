using System.Data;
using QuanLyThuVien.DataAccess;

namespace QuanLyThuVien.Repositories
{
    public class DanhMucRepository
    {
        public DataTable GetAll() =>
            DatabaseHelper.ExecuteQuery("SELECT * FROM DanhMuc ORDER BY TenDanhMuc");
    }
}