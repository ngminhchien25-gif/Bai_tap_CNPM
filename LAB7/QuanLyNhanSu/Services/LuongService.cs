using System;
using System.Data;
using System.Data.SqlClient;
using QuanLyNhanSu.Data;

namespace QuanLyNhanSu.Services
{
    public class LuongService
    {
        // QD01: Thực lĩnh = lương ngày công + phụ cấp + tiền phụ trội + thưởng - bảo hiểm - phạt
        public static decimal TinhThucLinh(decimal luongCB, decimal phuCap, decimal ngayCong, decimal gioPhuTroi,
            decimal thuong, decimal phat, bool coBaoHiem, decimal ngayCongChuan = 26, decimal gioNgay = 8,
            decimal heSoPhuTroi = 1.5m, decimal tyLeBH = 0.095m)
        {
            decimal luongNgayCong = Math.Round(luongCB / ngayCongChuan * ngayCong);
            decimal tienPhuTroi = Math.Round(gioPhuTroi * (luongCB / ngayCongChuan / gioNgay) * heSoPhuTroi);
            decimal bh = coBaoHiem ? Math.Round(luongCB * tyLeBH) : 0;
            return luongNgayCong + phuCap + tienPhuTroi + thuong - bh - phat;
        }

        private decimal ThamSo(string ma)
        {
            return Convert.ToDecimal(Db.Scalar("SELECT GiaTri FROM ThamSo WHERE MaThamSo=@m", Db.P("@m", ma)));
        }

        // BR09: chỉ tính lương khi bảng chấm công của tháng đã chốt; bảng lương đã chốt không tính lại
        public KetQuaXuLy TinhLuong(int thang, int nam)
        {
            if (thang < 1 || thang > 12) return KetQuaXuLy.Loi("Tháng không hợp lệ.");
            int tong = Convert.ToInt32(Db.Scalar("SELECT COUNT(*) FROM BangChamCong WHERE Thang=@t AND Nam=@n", Db.P("@t", thang), Db.P("@n", nam)));
            int chuaChot = Convert.ToInt32(Db.Scalar("SELECT COUNT(*) FROM BangChamCong WHERE Thang=@t AND Nam=@n AND DaChot=0", Db.P("@t", thang), Db.P("@n", nam)));
            if (tong == 0) return KetQuaXuLy.Loi("Chưa có bảng chấm công của tháng " + thang + "/" + nam + ".");
            if (chuaChot > 0) return KetQuaXuLy.Loi("Còn bảng chấm công chưa chốt, chưa thể tính lương.");
            object daChot = Db.Scalar("SELECT DaChot FROM BangLuong WHERE Thang=@t AND Nam=@n", Db.P("@t", thang), Db.P("@n", nam));
            if (daChot != null && Convert.ToBoolean(daChot)) return KetQuaXuLy.Loi("Bảng lương đã chốt, không được tính lại.");

            decimal heSo = ThamSo("HeSoPhuTroi"), chuan = ThamSo("NgayCongChuan"), gio = ThamSo("GioMoiNgay");
            decimal tyBH = ThamSo("TyLeBHXH") + ThamSo("TyLeBHYT");
            DateTime cuoi = new DateTime(nam, thang, DateTime.DaysInMonth(nam, thang));

            DataTable dt = Db.Query(@"
SELECT nv.MaNV,
 ISNULL((SELECT TOP 1 l.LuongCoBan FROM QDLuong l JOIN QuyetDinh q ON q.SoQD=l.SoQD WHERE l.MaNV=nv.MaNV AND q.TrangThai=N'Đã ký' AND q.NgayHieuLuc<=@c ORDER BY q.NgayHieuLuc DESC),0) AS LuongCB,
 ISNULL((SELECT TOP 1 l.PhuCapLuong+l.PhuCapDocHai FROM QDLuong l JOIN QuyetDinh q ON q.SoQD=l.SoQD WHERE l.MaNV=nv.MaNV AND q.TrangThai=N'Đã ký' AND q.NgayHieuLuc<=@c ORDER BY q.NgayHieuLuc DESC),0) AS PCLuong,
 ISNULL((SELECT TOP 1 v.PhuCapChucVu FROM QDChucVu v JOIN QuyetDinh q ON q.SoQD=v.SoQD WHERE v.MaNV=nv.MaNV AND q.TrangThai=N'Đã ký' AND q.NgayHieuLuc<=@c ORDER BY q.NgayHieuLuc DESC),0) AS PCChucVu,
 (SELECT COUNT(*) FROM ChiTietChamCong cc JOIN BangChamCong b ON b.MaBCC=cc.MaBCC WHERE cc.MaNV=nv.MaNV AND b.Thang=@t AND b.Nam=@n AND cc.TrangThai IN (N'Đi làm',N'Nghỉ phép')) AS NgayCong,
 ISNULL((SELECT SUM(t.SoGioPhuTroi) FROM ChiTietPhuTroi t JOIN PhieuPhuTroi p ON p.SoPhieu=t.SoPhieu WHERE t.MaNV=nv.MaNV AND MONTH(p.NgayPhuTroi)=@t AND YEAR(p.NgayPhuTroi)=@n),0) AS GioPT,
 ISNULL((SELECT SUM(k.SoTien) FROM QDKhenThuong k JOIN QuyetDinh q ON q.SoQD=k.SoQD JOIN ChiTietDoiTuongQD d ON d.SoQD=q.SoQD WHERE d.MaNV=nv.MaNV AND q.TrangThai=N'Đã ký' AND MONTH(q.NgayHieuLuc)=@t AND YEAR(q.NgayHieuLuc)=@n),0) AS Thuong,
 ISNULL((SELECT SUM(k.SoTienPhat) FROM QDKyLuat k JOIN QuyetDinh q ON q.SoQD=k.SoQD JOIN ChiTietDoiTuongQD d ON d.SoQD=q.SoQD WHERE d.MaNV=nv.MaNV AND q.TrangThai=N'Đã ký' AND k.CachThu=N'Trừ lương' AND MONTH(q.NgayHieuLuc)=@t AND YEAR(q.NgayHieuLuc)=@n),0) AS Phat,
 (SELECT COUNT(*) FROM HopDong h JOIN HoSoBaoHiem b ON b.SoHD=h.SoHD WHERE h.MaNV=nv.MaNV AND h.TrangThai=N'Hiệu lực') AS CoBH
FROM NhanVien nv WHERE nv.TrangThai=N'Chính thức'", Db.P("@c", cuoi), Db.P("@t", thang), Db.P("@n", nam));

            using (SqlConnection cn = Db.OpenConnection())
            using (SqlTransaction tx = cn.BeginTransaction())
            {
                try
                {
                    string maBL = "BL" + nam + thang.ToString("00");
                    Db.Exec(cn, tx, "DELETE FROM ChiTietBangLuong WHERE MaBL=@m", Db.P("@m", maBL));
                    Db.Exec(cn, tx, "DELETE FROM BangLuong WHERE MaBL=@m", Db.P("@m", maBL));
                    Db.Exec(cn, tx, "INSERT BangLuong(MaBL,Thang,Nam) VALUES(@m,@t,@n)", Db.P("@m", maBL), Db.P("@t", thang), Db.P("@n", nam));
                    foreach (DataRow r in dt.Rows)
                    {
                        decimal lcb = Convert.ToDecimal(r["LuongCB"]), pc = Convert.ToDecimal(r["PCLuong"]) + Convert.ToDecimal(r["PCChucVu"]);
                        decimal cong = Convert.ToDecimal(r["NgayCong"]), gioPT = Convert.ToDecimal(r["GioPT"]);
                        decimal thuong = Convert.ToDecimal(r["Thuong"]), phat = Convert.ToDecimal(r["Phat"]);
                        bool coBH = Convert.ToInt32(r["CoBH"]) > 0;
                        decimal tienPT = Math.Round(gioPT * (lcb / chuan / gio) * heSo);
                        decimal bh = coBH ? Math.Round(lcb * tyBH) : 0;
                        decimal thucLinh = TinhThucLinh(lcb, pc, cong, gioPT, thuong, phat, coBH, chuan, gio, heSo, tyBH);
                        Db.Exec(cn, tx, @"INSERT ChiTietBangLuong(MaBL,MaNV,LuongCoBan,TongPhuCap,NgayCong,TienPhuTroi,TienThuong,KhauTruBH,KhauTruPhat,ThucLinh)
                                          VALUES(@m,@nv,@l,@pc,@c,@pt,@th,@bh,@ph,@tl)",
                            Db.P("@m", maBL), Db.P("@nv", r["MaNV"]), Db.P("@l", lcb), Db.P("@pc", pc), Db.P("@c", cong), Db.P("@pt", tienPT),
                            Db.P("@th", thuong), Db.P("@bh", bh), Db.P("@ph", phat), Db.P("@tl", thucLinh));
                    }
                    tx.Commit();
                    return KetQuaXuLy.Ok("Đã tính lương tháng " + thang + "/" + nam + " cho " + dt.Rows.Count + " nhân viên.", maBL);
                }
                catch (Exception ex) { try { tx.Rollback(); } catch { } return KetQuaXuLy.Loi("Không thể tính lương: " + ex.Message); }
            }
        }

        public DataTable LayBangLuong(int thang, int nam)
        {
            return Db.Query(@"SELECT c.MaNV, n.HoTen, c.LuongCoBan, c.TongPhuCap, c.NgayCong, c.TienPhuTroi, c.TienThuong, c.KhauTruBH, c.KhauTruPhat, c.ThucLinh
                              FROM ChiTietBangLuong c JOIN BangLuong b ON b.MaBL=c.MaBL JOIN NhanVien n ON n.MaNV=c.MaNV
                              WHERE b.Thang=@t AND b.Nam=@n ORDER BY c.MaNV", Db.P("@t", thang), Db.P("@n", nam));
        }

        public KetQuaXuLy ChotBangLuong(int thang, int nam)
        {
            int n = Db.Execute("UPDATE BangLuong SET DaChot=1 WHERE Thang=@t AND Nam=@n AND DaChot=0", Db.P("@t", thang), Db.P("@n", nam));
            return n > 0 ? KetQuaXuLy.Ok("Đã chốt bảng lương tháng " + thang + "/" + nam + ".") : KetQuaXuLy.Loi("Chưa có bảng lương chưa chốt để chốt.");
        }
    }
}
