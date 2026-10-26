using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using QuanLyNhanSu.Data;

namespace QuanLyNhanSu.Services
{
    public class ChamCongService
    {
        // BR05: kế hoạch phân công tuần do trưởng phòng/ban/phân xưởng lập
        public KetQuaXuLy LapKeHoachTuan(string maPB, DateTime tuNgay, DateTime denNgay, string nguoiLap, List<PhanCongDto> ds)
        {
            if (string.IsNullOrWhiteSpace(maPB)) return KetQuaXuLy.Loi("Vui lòng chọn phòng ban.");
            if (denNgay.Date < tuNgay.Date || (denNgay.Date - tuNgay.Date).Days > 6) return KetQuaXuLy.Loi("Kế hoạch tuần gồm tối đa 7 ngày liên tiếp.");
            if (ds == null || ds.Count == 0) return KetQuaXuLy.Loi("Chưa phân công nhân viên nào.");
            HashSet<string> khoa = new HashSet<string>();
            foreach (PhanCongDto p in ds)
            {
                if (p.Ngay.Date < tuNgay.Date || p.Ngay.Date > denNgay.Date) return KetQuaXuLy.Loi("Có ngày phân công nằm ngoài tuần của kế hoạch.");
                if (!khoa.Add(p.MaNV + "|" + p.Ngay.ToString("yyyyMMdd"))) return KetQuaXuLy.Loi("Một nhân viên không thể phân công hai lần trong một ngày.");
            }
            using (SqlConnection cn = Db.OpenConnection())
            using (SqlTransaction tx = cn.BeginTransaction())
            {
                try
                {
                    string ma = Db.TaoMa("KH");
                    Db.Exec(cn, tx, "INSERT KeHoachTuan(MaKH,MaPB,TuNgay,DenNgay,NguoiLap) VALUES(@k,@p,@t,@d,@n)",
                        Db.P("@k", ma), Db.P("@p", maPB), Db.P("@t", tuNgay.Date), Db.P("@d", denNgay.Date), Db.P("@n", nguoiLap));
                    foreach (PhanCongDto p in ds)
                        Db.Exec(cn, tx, "INSERT ChiTietPhanCong(MaKH,MaNV,Ngay,MaCa,CoPhuTroi) VALUES(@k,@nv,@n,@c,@pt)",
                            Db.P("@k", ma), Db.P("@nv", p.MaNV), Db.P("@n", p.Ngay.Date), Db.P("@c", p.MaCa), Db.P("@pt", p.CoPhuTroi));
                    tx.Commit();
                    return KetQuaXuLy.Ok("Đã lập kế hoạch tuần " + ma + ".", ma);
                }
                catch (Exception ex) { try { tx.Rollback(); } catch { } return KetQuaXuLy.Loi("Không thể lập kế hoạch: " + ex.Message); }
            }
        }

        public KetQuaXuLy LuuChamCong(int thang, int nam, string maPB, string nguoiLap, List<ChamCongDto> ds)
        {
            if (ds == null || ds.Count == 0) return KetQuaXuLy.Loi("Chưa có dữ liệu chấm công.");
            using (SqlConnection cn = Db.OpenConnection())
            using (SqlTransaction tx = cn.BeginTransaction())
            {
                try
                {
                    object ma = Db.ScalarTx(cn, tx, "SELECT MaBCC FROM BangChamCong WHERE Thang=@t AND Nam=@n AND MaPB=@p", Db.P("@t", thang), Db.P("@n", nam), Db.P("@p", maPB));
                    string maBCC;
                    if (ma == null)
                    {
                        maBCC = "CC" + nam + thang.ToString("00") + maPB;
                        Db.Exec(cn, tx, "INSERT BangChamCong(MaBCC,Thang,Nam,MaPB,NguoiLap) VALUES(@m,@t,@n,@p,@l)",
                            Db.P("@m", maBCC), Db.P("@t", thang), Db.P("@n", nam), Db.P("@p", maPB), Db.P("@l", nguoiLap));
                    }
                    else
                    {
                        maBCC = Convert.ToString(ma);
                        if (Convert.ToBoolean(Db.ScalarTx(cn, tx, "SELECT DaChot FROM BangChamCong WHERE MaBCC=@m", Db.P("@m", maBCC))))
                        { tx.Rollback(); return KetQuaXuLy.Loi("Bảng chấm công đã chốt, không được sửa."); }
                        Db.Exec(cn, tx, "DELETE FROM ChiTietChamCong WHERE MaBCC=@m", Db.P("@m", maBCC));
                    }
                    foreach (ChamCongDto c in ds)
                        Db.Exec(cn, tx, "INSERT ChiTietChamCong(MaBCC,MaNV,Ngay,MaCa,TrangThai,SoGioLam) VALUES(@m,@nv,@n,@c,@tt,@g)",
                            Db.P("@m", maBCC), Db.P("@nv", c.MaNV), Db.P("@n", c.Ngay.Date), Db.P("@c", c.MaCa), Db.P("@tt", c.TrangThai), Db.P("@g", c.SoGioLam));
                    tx.Commit();
                    return KetQuaXuLy.Ok("Đã lưu bảng chấm công " + maBCC + ".");
                }
                catch (Exception ex) { try { tx.Rollback(); } catch { } return KetQuaXuLy.Loi("Không thể lưu chấm công: " + ex.Message); }
            }
        }

        public KetQuaXuLy ChotChamCong(int thang, int nam, string maPB)
        {
            int n = Db.Execute("UPDATE BangChamCong SET DaChot=1 WHERE Thang=@t AND Nam=@n AND MaPB=@p AND DaChot=0", Db.P("@t", thang), Db.P("@n", nam), Db.P("@p", maPB));
            return n > 0 ? KetQuaXuLy.Ok("Đã chốt bảng chấm công.") : KetQuaXuLy.Loi("Không có bảng chấm công chưa chốt để chốt.");
        }

        // BR05: phiếu phụ trội ghi rõ từng nhân viên; giờ phụ trội tính hệ số ở LuongService
        public KetQuaXuLy LapPhieuPhuTroi(PhuTroiDto d)
        {
            if (d == null || string.IsNullOrWhiteSpace(d.SoPhieu) || string.IsNullOrWhiteSpace(d.MaPB)) return KetQuaXuLy.Loi("Vui lòng nhập số phiếu và phòng ban.");
            if (d.HinhThuc != "Tăng ca" && d.HinhThuc != "Ngày lễ" && d.HinhThuc != "Ngày nghỉ") return KetQuaXuLy.Loi("Hình thức phụ trội phải là tăng ca, ngày lễ hoặc ngày nghỉ.");
            if (d.ChiTiet.Count == 0) return KetQuaXuLy.Loi("Phiếu phụ trội phải có ít nhất một nhân viên.");
            HashSet<string> da = new HashSet<string>();
            foreach (PhuTroiChiTietDto c in d.ChiTiet)
            {
                if (c.SoGio <= 0) return KetQuaXuLy.Loi("Số giờ phụ trội phải lớn hơn 0.");
                if (!da.Add(c.MaNV)) return KetQuaXuLy.Loi("Nhân viên " + c.MaNV + " bị trùng trong phiếu.");
            }
            using (SqlConnection cn = Db.OpenConnection())
            using (SqlTransaction tx = cn.BeginTransaction())
            {
                try
                {
                    Db.Exec(cn, tx, "INSERT PhieuPhuTroi(SoPhieu,NgayPhuTroi,HinhThuc,MaPB,NguoiLap) VALUES(@s,@n,@h,@p,@l)",
                        Db.P("@s", d.SoPhieu.Trim()), Db.P("@n", d.NgayPhuTroi.Date), Db.P("@h", d.HinhThuc), Db.P("@p", d.MaPB), Db.P("@l", d.NguoiLap));
                    foreach (PhuTroiChiTietDto c in d.ChiTiet)
                        Db.Exec(cn, tx, "INSERT ChiTietPhuTroi(SoPhieu,MaNV,SoGioPhuTroi) VALUES(@s,@nv,@g)", Db.P("@s", d.SoPhieu.Trim()), Db.P("@nv", c.MaNV), Db.P("@g", c.SoGio));
                    tx.Commit();
                    return KetQuaXuLy.Ok("Đã lập phiếu phụ trội " + d.SoPhieu + ".");
                }
                catch (SqlException ex) { try { tx.Rollback(); } catch { } return KetQuaXuLy.Loi(ex.Number == 2627 ? "Số phiếu đã tồn tại." : "Lỗi cơ sở dữ liệu: " + ex.Message); }
                catch (Exception ex) { try { tx.Rollback(); } catch { } return KetQuaXuLy.Loi("Không thể lập phiếu: " + ex.Message); }
            }
        }
    }
}
