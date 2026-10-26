using System;
using System.Data;
using System.Data.SqlClient;
using QuanLyNhanSu.Data;

namespace QuanLyNhanSu.Services
{
    public class QuyetDinhService
    {
        // Sơ đồ trạng thái QuyetDinh: Nháp → Chờ duyệt → Đã ký / Từ chối; Từ chối → Nháp; Nháp → Huỷ
        private static readonly string[][] ChuyenHopLe =
        {
            new[] { "Nháp", "Chờ duyệt" }, new[] { "Chờ duyệt", "Đã ký" }, new[] { "Chờ duyệt", "Từ chối" },
            new[] { "Từ chối", "Nháp" }, new[] { "Nháp", "Huỷ" }
        };

        public DataTable LayDanhSach(string loaiQD)
        {
            return Db.Query("SELECT SoQD, LoaiQD, NgayLap, NgayHieuLuc, NguoiKy, TrangThai FROM QuyetDinh WHERE LoaiQD=@l ORDER BY NgayLap DESC", Db.P("@l", loaiQD));
        }

        // BR04: mỗi thời điểm một QĐ lương hiệu lực = QĐ đã ký có ngày hiệu lực mới nhất; không trùng ngày hiệu lực
        public KetQuaXuLy LapQDLuong(QDLuongDto d)
        {
            if (d == null || string.IsNullOrWhiteSpace(d.MaNV)) return KetQuaXuLy.Loi("Vui lòng chọn nhân viên.");
            if (d.LuongCoBan <= 0) return KetQuaXuLy.Loi("Lương cơ bản phải lớn hơn 0.");
            if (d.PhuCapLuong < 0 || d.PhuCapDocHai < 0) return KetQuaXuLy.Loi("Phụ cấp không được âm.");
            if (Convert.ToString(Db.Scalar("SELECT TrangThai FROM NhanVien WHERE MaNV=@m", Db.P("@m", d.MaNV))) != "Chính thức")
                return KetQuaXuLy.Loi("Chỉ nhân viên chính thức mới có quyết định lương.");
            int trung = Convert.ToInt32(Db.Scalar(@"SELECT COUNT(*) FROM QDLuong l JOIN QuyetDinh q ON q.SoQD=l.SoQD
                          WHERE l.MaNV=@m AND q.NgayHieuLuc=@h AND q.TrangThai<>N'Huỷ'", Db.P("@m", d.MaNV), Db.P("@h", d.NgayHieuLuc.Date)));
            if (trung > 0) return KetQuaXuLy.Loi("Nhân viên đã có quyết định lương cùng ngày hiệu lực.");
            using (SqlConnection cn = Db.OpenConnection())
            using (SqlTransaction tx = cn.BeginTransaction())
            {
                try
                {
                    string so = Db.TaoMa("QL");
                    Db.ThemQuyetDinh(cn, tx, so, "Luong", d.NgayHieuLuc, "Quyết định lương");
                    Db.Exec(cn, tx, "INSERT QDLuong(SoQD,MaNV,LuongCoBan,PhuCapLuong,PhuCapDocHai) VALUES(@s,@m,@l,@p,@h)",
                        Db.P("@s", so), Db.P("@m", d.MaNV), Db.P("@l", d.LuongCoBan), Db.P("@p", d.PhuCapLuong), Db.P("@h", d.PhuCapDocHai));
                    tx.Commit();
                    return KetQuaXuLy.Ok("Đã lập quyết định lương " + so + " (Nháp).", so);
                }
                catch (Exception ex) { try { tx.Rollback(); } catch { } return KetQuaXuLy.Loi("Không thể lập quyết định: " + ex.Message); }
            }
        }

        public KetQuaXuLy LapQDChucVu(QDChucVuDto d)
        {
            if (d == null || string.IsNullOrWhiteSpace(d.MaNV) || string.IsNullOrWhiteSpace(d.MaCV)) return KetQuaXuLy.Loi("Vui lòng chọn nhân viên và chức vụ.");
            if (d.PhuCapChucVu < 0) return KetQuaXuLy.Loi("Phụ cấp không được âm.");
            if (Convert.ToString(Db.Scalar("SELECT TrangThai FROM NhanVien WHERE MaNV=@m", Db.P("@m", d.MaNV))) != "Chính thức")
                return KetQuaXuLy.Loi("Chỉ nhân viên chính thức mới có quyết định chức vụ.");
            int trung = Convert.ToInt32(Db.Scalar(@"SELECT COUNT(*) FROM QDChucVu c JOIN QuyetDinh q ON q.SoQD=c.SoQD
                          WHERE c.MaNV=@m AND q.NgayHieuLuc=@h AND q.TrangThai<>N'Huỷ'", Db.P("@m", d.MaNV), Db.P("@h", d.NgayHieuLuc.Date)));
            if (trung > 0) return KetQuaXuLy.Loi("Nhân viên đã có quyết định chức vụ cùng ngày hiệu lực.");
            using (SqlConnection cn = Db.OpenConnection())
            using (SqlTransaction tx = cn.BeginTransaction())
            {
                try
                {
                    string so = Db.TaoMa("CV");
                    Db.ThemQuyetDinh(cn, tx, so, "ChucVu", d.NgayHieuLuc, "Quyết định chức vụ");
                    Db.Exec(cn, tx, "INSERT QDChucVu(SoQD,MaNV,MaCV,PhuCapChucVu) VALUES(@s,@m,@c,@p)",
                        Db.P("@s", so), Db.P("@m", d.MaNV), Db.P("@c", d.MaCV), Db.P("@p", d.PhuCapChucVu));
                    tx.Commit();
                    return KetQuaXuLy.Ok("Đã lập quyết định chức vụ " + so + " (Nháp).", so);
                }
                catch (Exception ex) { try { tx.Rollback(); } catch { } return KetQuaXuLy.Loi("Không thể lập quyết định: " + ex.Message); }
            }
        }

        // Chuyển trạng thái; khi Giám đốc ký sẽ sinh hệ quả theo loại quyết định (BR07, BR08)
        public KetQuaXuLy ChuyenTrangThai(string soQD, string trangThaiMoi, string nguoiKy)
        {
            if (string.IsNullOrWhiteSpace(soQD)) return KetQuaXuLy.Loi("Vui lòng chọn quyết định.");
            if (trangThaiMoi == "Đã ký" && string.IsNullOrWhiteSpace(nguoiKy)) return KetQuaXuLy.Loi("Phải ghi tên người ký.");
            using (SqlConnection cn = Db.OpenConnection())
            using (SqlTransaction tx = cn.BeginTransaction())
            {
                try
                {
                    DataTable t = new DataTable();
                    using (SqlCommand c = new SqlCommand("SELECT LoaiQD, TrangThai FROM QuyetDinh WHERE SoQD=@s", cn, tx))
                    {
                        c.Parameters.AddWithValue("@s", soQD);
                        new SqlDataAdapter(c).Fill(t);
                    }
                    if (t.Rows.Count == 0) { tx.Rollback(); return KetQuaXuLy.Loi("Không tìm thấy quyết định."); }
                    string loai = Convert.ToString(t.Rows[0]["LoaiQD"]), cu = Convert.ToString(t.Rows[0]["TrangThai"]);
                    bool hopLe = false;
                    foreach (string[] cc in ChuyenHopLe) if (cc[0] == cu && cc[1] == trangThaiMoi) hopLe = true;
                    if (!hopLe) { tx.Rollback(); return KetQuaXuLy.Loi("Không thể chuyển từ \"" + cu + "\" sang \"" + trangThaiMoi + "\"."); }

                    Db.Exec(cn, tx, "UPDATE QuyetDinh SET TrangThai=@m, NguoiKy=CASE WHEN @m=N'Đã ký' THEN @nk ELSE NguoiKy END WHERE SoQD=@s",
                        Db.P("@m", trangThaiMoi), Db.P("@nk", nguoiKy), Db.P("@s", soQD));

                    if (trangThaiMoi == "Đã ký")
                    {
                        if (loai == "TuyenDung")
                            Db.Exec(cn, tx, "UPDATE NhanVien SET TrangThai=N'Thử việc', NgayVaoLam=(SELECT NgayBatDauTV FROM QDTuyenDung WHERE SoQD=@s) WHERE MaNV=(SELECT MaNV FROM QDTuyenDung WHERE SoQD=@s)", Db.P("@s", soQD));
                        else if (loai == "KhenThuong")
                            Db.Exec(cn, tx, @"INSERT PhieuChiKhenThuong(SoPhieuChi,SoQD,SoTien,NgayChi)
                                SELECT @p, k.SoQD, k.SoTien*(SELECT COUNT(*) FROM ChiTietDoiTuongQD d WHERE d.SoQD=k.SoQD), CAST(GETDATE() AS date)
                                FROM QDKhenThuong k WHERE k.SoQD=@s", Db.P("@p", Db.TaoMa("PC")), Db.P("@s", soQD));
                        else if (loai == "KyLuat")
                            Db.Exec(cn, tx, @"INSERT PhieuThuNopPhat(SoPhieuThu,SoQD,SoTien,NgayThu,HinhThuc)
                                SELECT @p, k.SoQD, k.SoTienPhat*(SELECT COUNT(*) FROM ChiTietDoiTuongQD d WHERE d.SoQD=k.SoQD), CAST(GETDATE() AS date), k.CachThu
                                FROM QDKyLuat k WHERE k.SoQD=@s AND k.SoTienPhat>0", Db.P("@p", Db.TaoMa("PT")), Db.P("@s", soQD));
                    }
                    tx.Commit();
                    return KetQuaXuLy.Ok("Quyết định " + soQD + " chuyển sang \"" + trangThaiMoi + "\".");
                }
                catch (Exception ex) { try { tx.Rollback(); } catch { } return KetQuaXuLy.Loi("Không thể xử lý: " + ex.Message); }
            }
        }
    }
}
