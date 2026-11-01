using Microsoft.Data.SqlClient;
using System.Data;

namespace QuanLyKhachSanUI.Repositories
{
    public class DichVuRepository
    {
        public DataTable GetAllDichVu() => DatabaseHelper.ExecuteQuery("SELECT * FROM DichVu");

        public int InsertDichVu(string maDV, string tenDV, string donViTinh, decimal donGia)
        {
            string sql = "INSERT INTO DichVu(MaDV, TenDV, DonViTinh, DonGia) VALUES(@MaDV, @TenDV, @DVT, @DonGia)";
            return DatabaseHelper.ExecuteNonQuery(sql,
                new SqlParameter("@MaDV", maDV),
                new SqlParameter("@TenDV", tenDV),
                new SqlParameter("@DVT", donViTinh),
                new SqlParameter("@DonGia", donGia));
        }

        public int UpdateDichVu(string maDV, string tenDV, string donViTinh, decimal donGia)
        {
            string sql = "UPDATE DichVu SET TenDV = @TenDV, DonViTinh = @DVT, DonGia = @DonGia WHERE MaDV = @MaDV";
            return DatabaseHelper.ExecuteNonQuery(sql,
                new SqlParameter("@MaDV", maDV),
                new SqlParameter("@TenDV", tenDV),
                new SqlParameter("@DVT", donViTinh),
                new SqlParameter("@DonGia", donGia));
        }

        public int DeleteDichVu(string maDV)
        {
            return DatabaseHelper.ExecuteNonQuery("DELETE FROM DichVu WHERE MaDV = @MaDV", new SqlParameter("@MaDV", maDV));
        }
    }
}