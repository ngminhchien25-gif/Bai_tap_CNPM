using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using QuanLyNhanSu.Data;

namespace QuanLyNhanSu.Services
{
    public class KhenThuongService
    {
        // BR06: mỗi dòng đối tượng chỉ là nhân viên hoặc phòng ban; cá nhân → nhân viên, tập thể → phòng ban
        private static KetQuaXuLy KiemTraDoiTuong(string loai, List<DoiTuongDto> ds)
        {
            if (loai != "CaNhan" && loai != "TapThe") return KetQuaXuLy.Loi("Vui lòng chọn loại đối tượng.");
            if (ds == null || ds.Count == 0) return KetQuaXuLy.Loi("Chưa chọn đối tượng.");
            HashSet<string> da = new HashSet<string>();
            foreach (DoiTuongDto o in ds)
            {
                bool coNV = !string.IsNullOrWhiteSpace(o.MaNV), coPB = !string.IsNullOrWhiteSpace(o.MaPB);
                if (coNV == coPB) return KetQuaXuLy.Loi("Mỗi đối tượng phải là nhân viên hoặc phòng ban, không đồng thời cả hai.");
                if (loai == "CaNhan" && !coNV) return KetQuaXuLy.Loi("Quyết định cá nhân chỉ chọn nhân viên.");
                if (loai == "TapThe" && !coPB) return KetQuaXuLy.Loi("Quyết định tập thể chỉ chọn phòng ban.");
                if (!da.Add(coNV ? o.MaNV : o.MaPB)) return KetQuaXuLy.Loi("Đối tượng bị trùng trong danh sách.");
            }
            return KetQuaXuLy.Ok("");
        }

        private static void LuuDoiTuong(SqlConnection cn, SqlTransaction tx, string so, List<DoiTuongDto> ds)
        {
            foreach (DoiTuongDto o in ds)
                Db.Exec(cn, tx, "INSERT ChiTietDoiTuongQD(SoQD,MaNV,MaPB) VALUES(@s,@nv,@pb)",
                    Db.P("@s", so), Db.P("@nv", string.IsNullOrWhiteSpace(o.MaNV) ? null : o.MaNV), Db.P("@pb", string.IsNullOrWhiteSpace(o.MaPB) ? null : o.MaPB));
        }

        public KetQuaXuLy LapKhenThuong(KhenThuongDto d)
        {
            if (d == null) return KetQuaXuLy.Loi("Dữ liệu không hợp lệ.");
            KetQuaXuLy kt = KiemTraDoiTuong(d.LoaiDoiTuong, d.DoiTuong);
            if (!kt.ThanhCong) return kt;
            if (d.SoTien <= 0) return KetQuaXuLy.Loi("Số tiền khen thưởng phải lớn hơn 0.");
            if (string.IsNullOrWhiteSpace(d.LoaiKhenThuong)) return KetQuaXuLy.Loi("Vui lòng chọn loại khen thưởng.");
            using (SqlConnection cn = Db.OpenConnection())
            using (SqlTransaction tx = cn.BeginTransaction())
            {
                try
                {
                    string so = Db.TaoMa("KT");
                    Db.ThemQuyetDinh(cn, tx, so, "KhenThuong", d.NgayHieuLuc, "Quyết định khen thưởng");
                    Db.Exec(cn, tx, "INSERT QDKhenThuong(SoQD,LoaiDoiTuong,LoaiKhenThuong,HinhThuc,SoTien,LyDo) VALUES(@s,@a,@b,@c,@t,@l)",
                        Db.P("@s", so), Db.P("@a", d.LoaiDoiTuong), Db.P("@b", d.LoaiKhenThuong), Db.P("@c", d.HinhThuc), Db.P("@t", d.SoTien), Db.P("@l", d.LyDo));
                    LuuDoiTuong(cn, tx, so, d.DoiTuong);
                    tx.Commit();
                    return KetQuaXuLy.Ok("Đã lập quyết định khen thưởng " + so + " (Nháp).", so);
                }
                catch (Exception ex) { try { tx.Rollback(); } catch { } return KetQuaXuLy.Loi("Không thể lập quyết định: " + ex.Message); }
            }
        }

        public KetQuaXuLy LapKyLuat(KyLuatDto d)
        {
            if (d == null) return KetQuaXuLy.Loi("Dữ liệu không hợp lệ.");
            KetQuaXuLy kt = KiemTraDoiTuong(d.LoaiDoiTuong, d.DoiTuong);
            if (!kt.ThanhCong) return kt;
            if (d.SoTienPhat < 0) return KetQuaXuLy.Loi("Tiền phạt không được âm.");
            if (d.CachThu != "Nộp phạt" && d.CachThu != "Trừ lương") return KetQuaXuLy.Loi("Vui lòng chọn cách thu tiền phạt.");
            if (d.CachThu == "Trừ lương" && d.LoaiDoiTuong == "TapThe") return KetQuaXuLy.Loi("Kỷ luật tập thể chỉ áp dụng hình thức nộp phạt.");
            using (SqlConnection cn = Db.OpenConnection())
            using (SqlTransaction tx = cn.BeginTransaction())
            {
                try
                {
                    string so = Db.TaoMa("KL");
                    Db.ThemQuyetDinh(cn, tx, so, "KyLuat", d.NgayHieuLuc, "Quyết định kỷ luật");
                    Db.Exec(cn, tx, "INSERT QDKyLuat(SoQD,LoaiDoiTuong,HinhThuc,SoTienPhat,CachThu,LyDo) VALUES(@s,@a,@c,@t,@h,@l)",
                        Db.P("@s", so), Db.P("@a", d.LoaiDoiTuong), Db.P("@c", d.HinhThuc), Db.P("@t", d.SoTienPhat), Db.P("@h", d.CachThu), Db.P("@l", d.LyDo));
                    LuuDoiTuong(cn, tx, so, d.DoiTuong);
                    tx.Commit();
                    return KetQuaXuLy.Ok("Đã lập quyết định kỷ luật " + so + " (Nháp).", so);
                }
                catch (Exception ex) { try { tx.Rollback(); } catch { } return KetQuaXuLy.Loi("Không thể lập quyết định: " + ex.Message); }
            }
        }
    }
}
