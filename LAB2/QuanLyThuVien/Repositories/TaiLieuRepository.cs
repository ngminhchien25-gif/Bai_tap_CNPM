using Microsoft.Data.SqlClient;
using System.Data;
using QuanLyThuVien.DataAccess;

namespace QuanLyThuVien.Repositories
{
    public class TaiLieuRepository
    {
        public DataTable GetAll() => DatabaseHelper.ExecuteQuery(
            @"SELECT tl.MaTaiLieu, tl.TenTaiLieu, tl.TacGia, tl.NamXuatBan, tl.NhaXuatBan,
                     dm.TenDanhMuc, tl.LoaiTaiLieu, tl.SoLuongTon, tl.FilePath
              FROM TaiLieu tl
              JOIN DanhMuc dm ON tl.MaDanhMuc = dm.MaDanhMuc
              ORDER BY tl.MaTaiLieu");

        public DataTable Search(string keyword) => DatabaseHelper.ExecuteQuery(
            @"SELECT tl.MaTaiLieu, tl.TenTaiLieu, tl.TacGia, tl.NamXuatBan, tl.NhaXuatBan,
                     dm.TenDanhMuc, tl.LoaiTaiLieu, tl.SoLuongTon, tl.FilePath
              FROM TaiLieu tl
              JOIN DanhMuc dm ON tl.MaDanhMuc = dm.MaDanhMuc
              WHERE tl.TenTaiLieu LIKE @kw OR tl.TacGia LIKE @kw",
            new SqlParameter("@kw", $"%{keyword}%"));

        public void Insert(string ten, string tacGia, int? nam, string nxb,
                            int maDanhMuc, string loai, int soLuong, string filePath) =>
            DatabaseHelper.ExecuteNonQuery(
                @"INSERT INTO TaiLieu (TenTaiLieu, TacGia, NamXuatBan, NhaXuatBan, MaDanhMuc, LoaiTaiLieu, SoLuongTon, FilePath)
                  VALUES (@ten, @tg, @nam, @nxb, @dm, @loai, @sl, @fp)",
                new SqlParameter("@ten", ten),
                new SqlParameter("@tg", tacGia),
                new SqlParameter("@nam", (object?)nam ?? DBNull.Value),
                new SqlParameter("@nxb", (object)nxb ?? DBNull.Value),
                new SqlParameter("@dm", maDanhMuc),
                new SqlParameter("@loai", loai),
                new SqlParameter("@sl", soLuong),
                new SqlParameter("@fp", (object)filePath ?? DBNull.Value));

        public void Update(int id, string ten, string tacGia, int? nam, string nxb,
                            int maDanhMuc, string loai, int soLuong, string filePath) =>
            DatabaseHelper.ExecuteNonQuery(
                @"UPDATE TaiLieu SET TenTaiLieu=@ten, TacGia=@tg, NamXuatBan=@nam, NhaXuatBan=@nxb,
                  MaDanhMuc=@dm, LoaiTaiLieu=@loai, SoLuongTon=@sl, FilePath=@fp, NgayCapNhat=GETDATE()
                  WHERE MaTaiLieu=@id",
                new SqlParameter("@ten", ten),
                new SqlParameter("@tg", tacGia),
                new SqlParameter("@nam", (object?)nam ?? DBNull.Value),
                new SqlParameter("@nxb", (object)nxb ?? DBNull.Value),
                new SqlParameter("@dm", maDanhMuc),
                new SqlParameter("@loai", loai),
                new SqlParameter("@sl", soLuong),
                new SqlParameter("@fp", (object)filePath ?? DBNull.Value),
                new SqlParameter("@id", id));

        public void Delete(int id) =>
            DatabaseHelper.ExecuteNonQuery("DELETE FROM TaiLieu WHERE MaTaiLieu=@id",
                new SqlParameter("@id", id));
    }
}