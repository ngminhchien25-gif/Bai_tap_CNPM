using System.Data;

namespace QuanLyKhachSanUI.Repositories
{
    public class TrangThietBiRepository
    {
        public DataTable GetAllTrangThietBi()
        {
            string sql = "SELECT MaTB, TenTB, DonGia FROM TrangThietBi";
            return DatabaseHelper.ExecuteQuery(sql);
        }
    }
}