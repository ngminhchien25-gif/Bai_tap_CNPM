using Microsoft.Data.SqlClient;
using System.Data;
using QuanLyThuVien.DataAccess;

namespace QuanLyThuVien.Repositories
{
    public class PhieuMuonRepository
    {
        public DataTable GetAll() => DatabaseHelper.ExecuteQuery(
            @"SELECT pm.MaPhieu, pm.MaThe, tl.TenTaiLieu, pm.NgayMuon, pm.HanTra, pm.NgayTra, pm.TrangThai, pm.MaTaiLieu
              FROM PhieuMuon pm
              JOIN TaiLieu tl ON pm.MaTaiLieu = tl.MaTaiLieu
              ORDER BY pm.MaPhieu DESC");

        // Mượn sách: kiểm tra tồn kho + trừ tồn kho + tạo phiếu, trong 1 transaction
        public void Muon(string maThe, int maTaiLieu, DateTime hanTra)
        {
            DatabaseHelper.ExecuteTransaction((conn, tran) =>
            {
                // Kiểm tra tồn kho, khóa dòng để tránh race condition
                var cmdCheck = new SqlCommand(
                    "SELECT SoLuongTon FROM TaiLieu WITH (UPDLOCK, ROWLOCK) WHERE MaTaiLieu=@id",
                    conn, tran);
                cmdCheck.Parameters.AddWithValue("@id", maTaiLieu);
                var tonKho = (int)cmdCheck.ExecuteScalar();

                if (tonKho <= 0)
                    throw new InvalidOperationException("Tài liệu đã hết, không thể mượn.");

                var cmdInsert = new SqlCommand(
                    @"INSERT INTO PhieuMuon (MaThe, MaTaiLieu, HanTra, TrangThai)
                      VALUES (@maThe, @maTL, @han, N'DangMuon')", conn, tran);
                cmdInsert.Parameters.AddWithValue("@maThe", maThe);
                cmdInsert.Parameters.AddWithValue("@maTL", maTaiLieu);
                cmdInsert.Parameters.AddWithValue("@han", hanTra);
                cmdInsert.ExecuteNonQuery();

                var cmdUpdate = new SqlCommand(
                    "UPDATE TaiLieu SET SoLuongTon = SoLuongTon - 1 WHERE MaTaiLieu=@id", conn, tran);
                cmdUpdate.Parameters.AddWithValue("@id", maTaiLieu);
                cmdUpdate.ExecuteNonQuery();
            });
        }

        // Trả sách: cộng lại tồn kho + cập nhật phiếu
        public void Tra(int maPhieu, int maTaiLieu)
        {
            DatabaseHelper.ExecuteTransaction((conn, tran) =>
            {
                var cmdUpdatePhieu = new SqlCommand(
                    @"UPDATE PhieuMuon SET NgayTra=GETDATE(), TrangThai=N'DaTra' WHERE MaPhieu=@id",
                    conn, tran);
                cmdUpdatePhieu.Parameters.AddWithValue("@id", maPhieu);
                cmdUpdatePhieu.ExecuteNonQuery();

                var cmdUpdateTL = new SqlCommand(
                    "UPDATE TaiLieu SET SoLuongTon = SoLuongTon + 1 WHERE MaTaiLieu=@id", conn, tran);
                cmdUpdateTL.Parameters.AddWithValue("@id", maTaiLieu);
                cmdUpdateTL.ExecuteNonQuery();
            });
        }
    }
}