using System;
using System.Data;
using System.Data.SqlClient;
using QuanLyNhanSu.Data;

namespace QuanLyNhanSu.Services
{
    public class TuyenDungService
    {
        // BR01: quyết định tuyển dụng ghi phòng ban, thời gian và lương thử việc, điều khoản hai bên
        public KetQuaXuLy LapQDTuyenDung(TuyenDungDto d)
        {
            if (d == null || string.IsNullOrWhiteSpace(d.MaNV) || string.IsNullOrWhiteSpace(d.MaPB))
                return KetQuaXuLy.Loi("Vui lòng chọn nhân viên và phòng ban tuyển dụng.");
            if (d.NgayKetThucTV.Date <= d.NgayBatDauTV.Date) return KetQuaXuLy.Loi("Ngày kết thúc thử việc phải sau ngày bắt đầu.");
            if (d.LuongThuViec <= 0) return KetQuaXuLy.Loi("Lương thử việc phải lớn hơn 0.");
            if (string.IsNullOrWhiteSpace(d.DieuKhoanBenA) || string.IsNullOrWhiteSpace(d.DieuKhoanBenB))
                return KetQuaXuLy.Loi("Phải ghi rõ điều khoản của hai bên.");
            object tt = Db.Scalar("SELECT TrangThai FROM NhanVien WHERE MaNV=@m", Db.P("@m", d.MaNV));
            if (tt == null) return KetQuaXuLy.Loi("Không tìm thấy nhân viên.");
            if (Convert.ToString(tt) != "Chờ tuyển dụng") return KetQuaXuLy.Loi("Nhân viên đã được tuyển dụng hoặc đã nghỉ việc.");

            using (SqlConnection cn = Db.OpenConnection())
            using (SqlTransaction tx = cn.BeginTransaction())
            {
                try
                {
                    string so = Db.TaoMa("TD");
                    Db.ThemQuyetDinh(cn, tx, so, "TuyenDung", d.NgayBatDauTV, "Quyết định tuyển dụng");
                    Db.Exec(cn, tx, @"INSERT QDTuyenDung(SoQD,MaNV,MaPB,NgayBatDauTV,NgayKetThucTV,LuongThuViec,DieuKhoanBenA,DieuKhoanBenB)
                                      VALUES(@s,@nv,@pb,@bd,@kt,@l,@a,@b)",
                        Db.P("@s", so), Db.P("@nv", d.MaNV), Db.P("@pb", d.MaPB), Db.P("@bd", d.NgayBatDauTV.Date),
                        Db.P("@kt", d.NgayKetThucTV.Date), Db.P("@l", d.LuongThuViec), Db.P("@a", d.DieuKhoanBenA), Db.P("@b", d.DieuKhoanBenB));
                    tx.Commit();
                    return KetQuaXuLy.Ok("Đã lập quyết định tuyển dụng " + so + " (Nháp).", so);
                }
                catch (Exception ex) { try { tx.Rollback(); } catch { } return KetQuaXuLy.Loi("Không thể lập quyết định: " + ex.Message); }
            }
        }

        // BR02, BR03: chỉ ký hợp đồng sau thử việc; hợp đồng có 2 loại
        public KetQuaXuLy KyHopDong(HopDongDto d)
        {
            if (d == null || string.IsNullOrWhiteSpace(d.MaNV)) return KetQuaXuLy.Loi("Vui lòng chọn nhân viên.");
            if (d.LoaiHD != "DaiHan" && d.LoaiHD != "NganHan") return KetQuaXuLy.Loi("Vui lòng chọn loại hợp đồng.");
            if (!d.GiamDocDaKy) return KetQuaXuLy.Loi("Giám đốc chưa ký hợp đồng.");
            if (d.LoaiHD == "NganHan" && (!d.NgayKetThuc.HasValue || d.NgayKetThuc.Value.Date <= d.NgayBatDau.Date))
                return KetQuaXuLy.Loi("Hợp đồng ngắn hạn phải có ngày kết thúc sau ngày bắt đầu.");
            if (Convert.ToString(Db.Scalar("SELECT TrangThai FROM NhanVien WHERE MaNV=@m", Db.P("@m", d.MaNV))) != "Thử việc")
                return KetQuaXuLy.Loi("Chỉ nhân viên đang thử việc mới được ký hợp đồng chính thức.");

            DataTable td = Db.Query(@"SELECT TOP 1 t.SoQD, t.NgayKetThucTV FROM QDTuyenDung t JOIN QuyetDinh q ON q.SoQD=t.SoQD
                                      WHERE t.MaNV=@m AND q.TrangThai=N'Đã ký' ORDER BY t.NgayKetThucTV DESC", Db.P("@m", d.MaNV));
            if (td.Rows.Count == 0) return KetQuaXuLy.Loi("Nhân viên chưa có quyết định tuyển dụng đã ký.");
            DateTime ketThucTV = Convert.ToDateTime(td.Rows[0]["NgayKetThucTV"]);
            if (d.NgayBatDau.Date <= ketThucTV.Date)
                return KetQuaXuLy.Loi("Chưa hết thời gian thử việc (đến " + ketThucTV.ToString("dd/MM/yyyy") + "), chưa thể ký hợp đồng.");

            using (SqlConnection cn = Db.OpenConnection())
            using (SqlTransaction tx = cn.BeginTransaction())
            {
                try
                {
                    string soHD = Db.TaoMa("HD");
                    Db.Exec(cn, tx, "INSERT HopDong(SoHD,MaNV,SoQDTuyenDung,LoaiHD,NgayBatDau,NgayKetThuc) VALUES(@h,@nv,@qd,@l,@bd,@kt)",
                        Db.P("@h", soHD), Db.P("@nv", d.MaNV), Db.P("@qd", td.Rows[0]["SoQD"]), Db.P("@l", d.LoaiHD),
                        Db.P("@bd", d.NgayBatDau.Date), Db.P("@kt", d.NgayKetThuc.HasValue ? (object)d.NgayKetThuc.Value.Date : null));
                    Db.Exec(cn, tx, "UPDATE NhanVien SET TrangThai=N'Chính thức' WHERE MaNV=@nv", Db.P("@nv", d.MaNV));
                    tx.Commit();
                    string tb = "Đã ký hợp đồng " + soHD + ".";
                    if (d.LoaiHD == "DaiHan") tb += " Hợp đồng dài hạn: cần lập hồ sơ BHYT/BHXH.";
                    return KetQuaXuLy.Ok(tb, soHD);
                }
                catch (Exception ex) { try { tx.Rollback(); } catch { } return KetQuaXuLy.Loi("Không thể ký hợp đồng: " + ex.Message); }
            }
        }

        // BR03: chỉ hợp đồng dài hạn mới có hồ sơ bảo hiểm
        public KetQuaXuLy LapHoSoBaoHiem(string soHD, string soBHYT, string soBHXH, DateTime ngayThamGia)
        {
            if (string.IsNullOrWhiteSpace(soHD)) return KetQuaXuLy.Loi("Vui lòng chọn hợp đồng.");
            DataTable hd = Db.Query("SELECT MaNV, LoaiHD FROM HopDong WHERE SoHD=@h", Db.P("@h", soHD));
            if (hd.Rows.Count == 0) return KetQuaXuLy.Loi("Không tìm thấy hợp đồng.");
            if (Convert.ToString(hd.Rows[0]["LoaiHD"]) != "DaiHan") return KetQuaXuLy.Loi("Hợp đồng ngắn hạn không làm hồ sơ BHYT/BHXH.");
            if (string.IsNullOrWhiteSpace(soBHYT) || string.IsNullOrWhiteSpace(soBHXH)) return KetQuaXuLy.Loi("Vui lòng nhập số BHYT và BHXH.");
            object luong = Db.Scalar(@"SELECT TOP 1 l.LuongCoBan FROM QDLuong l JOIN QuyetDinh q ON q.SoQD=l.SoQD
                                       WHERE l.MaNV=@m AND q.TrangThai=N'Đã ký' ORDER BY q.NgayHieuLuc DESC", Db.P("@m", hd.Rows[0]["MaNV"]));
            if (luong == null) return KetQuaXuLy.Loi("Cần có quyết định lương đã ký trước khi lập hồ sơ bảo hiểm.");
            decimal ty = Convert.ToDecimal(Db.Scalar("SELECT SUM(GiaTri) FROM ThamSo WHERE MaThamSo IN ('TyLeBHXH','TyLeBHYT')"));
            decimal muc = Math.Round(Convert.ToDecimal(luong) * ty);
            try
            {
                Db.Execute("INSERT HoSoBaoHiem(MaHSBH,SoHD,SoBHYT,SoBHXH,NgayThamGia,MucDong) VALUES(@m,@h,@y,@x,@n,@d)",
                    Db.P("@m", Db.TaoMa("BH")), Db.P("@h", soHD), Db.P("@y", soBHYT), Db.P("@x", soBHXH), Db.P("@n", ngayThamGia.Date), Db.P("@d", muc));
                return KetQuaXuLy.Ok("Đã lập hồ sơ bảo hiểm, mức đóng hằng tháng " + muc.ToString("N0") + " đ.");
            }
            catch (SqlException ex) { return KetQuaXuLy.Loi(ex.Number == 2627 || ex.Number == 2601 ? "Hợp đồng này đã có hồ sơ bảo hiểm." : "Lỗi cơ sở dữ liệu: " + ex.Message); }
        }
    }
}
