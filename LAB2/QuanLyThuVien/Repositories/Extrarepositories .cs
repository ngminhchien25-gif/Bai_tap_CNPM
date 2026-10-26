using System.Data;
using Microsoft.Data.SqlClient;   // nếu project đang dùng System.Data.SqlClient thì đổi using này

namespace QuanLyThuVien.Repositories
{
    /// <summary>Helper ADO.NET dùng chung cho các repository bổ sung.</summary>
    internal static class DbHelper
    {
        // Phải trùng với DatabaseHelper.connectionString của project.
        public static string ConnectionString =
            @"Server=.;Database=ThuVien;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;";

        public static SqlParameter P(string name, object? value)
            => new SqlParameter(name, value ?? DBNull.Value);

        public static SqlParameter PDate(string name, DateTime value)
            => new SqlParameter(name, SqlDbType.Date) { Value = value.Date };

        public static DataTable Query(string sql, params SqlParameter[] ps)
        {
            using var cn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddRange(ps);
            using var da = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public static int Execute(string sql, params SqlParameter[] ps)
        {
            using var cn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddRange(ps);
            cn.Open();
            return cmd.ExecuteNonQuery();
        }

        public static int ExecTx(SqlConnection cn, SqlTransaction tx, string sql, params SqlParameter[] ps)
        {
            using var cmd = new SqlCommand(sql, cn, tx);
            cmd.Parameters.AddRange(ps);
            return cmd.ExecuteNonQuery();
        }

        public static object? ScalarTx(SqlConnection cn, SqlTransaction tx, string sql, params SqlParameter[] ps)
        {
            using var cmd = new SqlCommand(sql, cn, tx);
            cmd.Parameters.AddRange(ps);
            return cmd.ExecuteScalar();
        }

        /// <summary>Đổi SqlException thô thành thông báo dễ hiểu (đề yêu cầu không hiện raw SQL error).</summary>
        public static string Friendly(SqlException ex, string? fkMessage = null)
        {
            if (ex.Message.Contains("UX_The_MotTheHoatDong")) return "Độc giả này đã có một thẻ đang hoạt động.";
            if (ex.Message.Contains("UX_PM_MotSachDangMuon")) return "Thẻ này đang mượn tài liệu đó và chưa trả.";
            switch (ex.Number)
            {
                case 2627:
                case 2601:
                    return "Dữ liệu bị trùng (mã hoặc giá trị duy nhất đã tồn tại).";
                case 547:
                    return fkMessage ?? "Thao tác vi phạm ràng buộc dữ liệu (đang được tham chiếu hoặc giá trị không hợp lệ).";
                default:
                    return "Lỗi cơ sở dữ liệu: " + ex.Message;
            }
        }

        /// <summary>Chạy action, hiện MessageBox thân thiện nếu lỗi. Trả về true nếu thành công.</summary>
        public static bool TryRun(Action action, string? fkMessage = null)
        {
            try { action(); return true; }
            catch (SqlException ex)
            {
                MessageBox.Show(Friendly(ex, fkMessage), "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Không thể thực hiện", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }
    }

    // ------------------------------------------------------------------
    public class DanhMucAdminRepository
    {
        public DataTable GetAll() =>
            DbHelper.Query("SELECT MaDanhMuc, TenDanhMuc, MoTa FROM DanhMuc ORDER BY TenDanhMuc");

        public void Insert(string ten, string? moTa) =>
            DbHelper.Execute("INSERT INTO DanhMuc(TenDanhMuc, MoTa) VALUES(@t, @m)",
                DbHelper.P("@t", ten), DbHelper.P("@m", string.IsNullOrWhiteSpace(moTa) ? null : moTa));

        public void Update(int ma, string ten, string? moTa) =>
            DbHelper.Execute("UPDATE DanhMuc SET TenDanhMuc=@t, MoTa=@m WHERE MaDanhMuc=@ma",
                DbHelper.P("@ma", ma), DbHelper.P("@t", ten),
                DbHelper.P("@m", string.IsNullOrWhiteSpace(moTa) ? null : moTa));

        public void Delete(int ma) =>
            DbHelper.Execute("DELETE FROM DanhMuc WHERE MaDanhMuc=@ma", DbHelper.P("@ma", ma));
    }

    // ------------------------------------------------------------------
    public class TheAdminRepository
    {
        /// <summary>Đánh dấu HetHan cho các thẻ HoatDong đã quá NgayHetHan.</summary>
        public void CapNhatHetHan() =>
            DbHelper.Execute("UPDATE TheThuVien SET TrangThai=N'HetHan' WHERE TrangThai=N'HoatDong' AND NgayHetHan < CAST(GETDATE() AS date)");

        public DataTable GetAll() => DbHelper.Query(@"
            SELECT t.MaThe, t.MaND, u.HoTen, t.NgayCap, t.NgayHetHan, t.DaDongLePhi, t.TrangThai,
                   CASE WHEN t.TrangThai = N'HoatDong' AND t.DaDongLePhi = 1
                             AND t.NgayHetHan >= CAST(GETDATE() AS date)
                        THEN N'Có giá trị' ELSE N'Không có giá trị' END AS HieuLuc
            FROM TheThuVien t
            JOIN NguoiDung u ON u.MaND = t.MaND
            ORDER BY t.NgayCap DESC, t.MaThe DESC");

        public DataTable GetDocGia() => DbHelper.Query(
            "SELECT MaND, HoTen + N' (' + TenDangNhap + N')' AS HienThi FROM NguoiDung WHERE VaiTro = N'DocGia' ORDER BY HoTen");

        /// <summary>Cấp thẻ mới (thẻ HoatDong cũ đã hết hạn được chuyển sang HetHan). Trả về mã thẻ.</summary>
        public string CapMoi(int maND, DateTime ngayCap, DateTime ngayHetHan, bool daDongLePhi)
        {
            if (ngayHetHan.Date < ngayCap.Date)
                throw new InvalidOperationException("Hạn sử dụng phải từ ngày cấp trở đi.");

            using var cn = new SqlConnection(DbHelper.ConnectionString);
            cn.Open();
            using var tx = cn.BeginTransaction(IsolationLevel.Serializable);
            try
            {
                int conGiaTri = Convert.ToInt32(DbHelper.ScalarTx(cn, tx,
                    @"SELECT COUNT(*) FROM TheThuVien WITH (UPDLOCK, HOLDLOCK)
                      WHERE MaND=@nd AND TrangThai=N'HoatDong' AND NgayHetHan >= @cap",
                    DbHelper.P("@nd", maND), DbHelper.PDate("@cap", ngayCap)));
                if (conGiaTri > 0)
                    throw new InvalidOperationException("Độc giả đang có một thẻ còn giá trị sử dụng, không thể cấp thẻ thứ hai.");

                DbHelper.ExecTx(cn, tx,
                    "UPDATE TheThuVien SET TrangThai=N'HetHan' WHERE MaND=@nd AND TrangThai=N'HoatDong'",
                    DbHelper.P("@nd", maND));

                int next = Convert.ToInt32(DbHelper.ScalarTx(cn, tx,
                    @"SELECT ISNULL(MAX(TRY_CAST(SUBSTRING(MaThe, 4, 17) AS INT)), 0) + 1
                      FROM TheThuVien WITH (UPDLOCK, HOLDLOCK) WHERE MaThe LIKE 'THE%'"));
                string maThe = "THE" + next.ToString("D3");

                DbHelper.ExecTx(cn, tx,
                    @"INSERT INTO TheThuVien(MaThe, MaND, NgayCap, NgayHetHan, DaDongLePhi, TrangThai)
                      VALUES(@m, @nd, @cap, @han, @lp, N'HoatDong')",
                    DbHelper.P("@m", maThe), DbHelper.P("@nd", maND),
                    DbHelper.PDate("@cap", ngayCap), DbHelper.PDate("@han", ngayHetHan),
                    DbHelper.P("@lp", daDongLePhi));

                tx.Commit();
                return maThe;
            }
            catch
            {
                try { tx.Rollback(); } catch { }
                throw;
            }
        }

        public void GiaHan(string maThe, DateTime hanMoi, bool daDongLePhi)
        {
            int n = DbHelper.Execute(
                @"UPDATE TheThuVien SET NgayHetHan=@h, DaDongLePhi=@lp, TrangThai=N'HoatDong'
                  WHERE MaThe=@m AND TrangThai <> N'Khoa' AND NgayHetHan < @h",
                DbHelper.P("@m", maThe), DbHelper.PDate("@h", hanMoi), DbHelper.P("@lp", daDongLePhi));
            if (n == 0)
                throw new InvalidOperationException("Không gia hạn được: thẻ đang bị khóa hoặc hạn mới không sau hạn hiện tại.");
        }

        public void Khoa(string maThe)
        {
            int n = DbHelper.Execute("UPDATE TheThuVien SET TrangThai=N'Khoa' WHERE MaThe=@m AND TrangThai <> N'Khoa'",
                DbHelper.P("@m", maThe));
            if (n == 0) throw new InvalidOperationException("Thẻ đã ở trạng thái khóa.");
        }

        public void MoKhoa(string maThe)
        {
            int n = DbHelper.Execute(
                @"UPDATE TheThuVien
                  SET TrangThai = CASE WHEN NgayHetHan < CAST(GETDATE() AS date) THEN N'HetHan' ELSE N'HoatDong' END
                  WHERE MaThe=@m AND TrangThai=N'Khoa'",
                DbHelper.P("@m", maThe));
            if (n == 0) throw new InvalidOperationException("Thẻ không ở trạng thái khóa.");
        }
    }

    // ------------------------------------------------------------------
    public class YeuCauRepository
    {
        public DataTable GetNguoiDung() => DbHelper.Query(
            "SELECT MaND, HoTen + N' (' + TenDangNhap + N')' AS HienThi FROM NguoiDung ORDER BY HoTen");

        public DataTable GetAll(string? trangThai) => DbHelper.Query(@"
            SELECT y.MaYeuCau, y.MaND, u.HoTen, y.TenSach, y.TacGia, y.NamXuatBan, y.GhiChu, y.TrangThai, y.NgayYeuCau
            FROM YeuCauNhapSach y
            JOIN NguoiDung u ON u.MaND = y.MaND
            WHERE (@tt IS NULL OR y.TrangThai = @tt)
            ORDER BY y.NgayYeuCau DESC", DbHelper.P("@tt", trangThai));

        public void Insert(int maND, string tenSach, string tacGia, int? nam, string? ghiChu) =>
            DbHelper.Execute(
                @"INSERT INTO YeuCauNhapSach(MaND, TenSach, TacGia, NamXuatBan, GhiChu)
                  VALUES(@nd, @ten, @tg, @nam, @gc)",
                DbHelper.P("@nd", maND), DbHelper.P("@ten", tenSach), DbHelper.P("@tg", tacGia),
                DbHelper.P("@nam", nam), DbHelper.P("@gc", string.IsNullOrWhiteSpace(ghiChu) ? null : ghiChu));

        public void SetTrangThai(int ma, string trangThai)
        {
            int n = DbHelper.Execute(
                "UPDATE YeuCauNhapSach SET TrangThai=@tt WHERE MaYeuCau=@m AND TrangThai=N'ChuaXuLy'",
                DbHelper.P("@m", ma), DbHelper.P("@tt", trangThai));
            if (n == 0) throw new InvalidOperationException("Yêu cầu này đã được xử lý trước đó.");
        }

        public void Delete(int ma)
        {
            int n = DbHelper.Execute("DELETE FROM YeuCauNhapSach WHERE MaYeuCau=@m AND TrangThai=N'ChuaXuLy'",
                DbHelper.P("@m", ma));
            if (n == 0) throw new InvalidOperationException("Chỉ xóa được yêu cầu chưa xử lý.");
        }
    }

    // ------------------------------------------------------------------
    public class PhieuPhatRepository
    {
        public DataTable GetPhieuMuon() => DbHelper.Query(@"
            SELECT p.MaPhieu, p.MaThe, p.MaTaiLieu, tl.TenTaiLieu, p.NgayMuon, p.HanTra, p.NgayTra,
                   p.TrangThai, pp.PhiPhat AS DaPhat
            FROM PhieuMuon p
            JOIN TaiLieu tl ON tl.MaTaiLieu = p.MaTaiLieu
            LEFT JOIN PhieuPhat pp ON pp.MaPhieu = p.MaPhieu
            ORDER BY p.MaPhieu DESC");

        public DataTable GetNhanVien() => DbHelper.Query(
            "SELECT MaND, HoTen FROM NguoiDung WHERE VaiTro = N'ThuThu' ORDER BY HoTen");

        public DataTable GetPhat(DateTime tu, DateTime den) => DbHelper.Query(@"
            SELECT pp.MaPhieuPhat, pp.MaPhieu, pm.MaThe, tl.TenTaiLieu, pp.NgayPhat, pp.LyDo, pp.PhiPhat,
                   u.HoTen AS NhanVienLap, pp.GhiChu
            FROM PhieuPhat pp
            JOIN PhieuMuon pm ON pm.MaPhieu = pp.MaPhieu
            JOIN TaiLieu tl   ON tl.MaTaiLieu = pm.MaTaiLieu
            JOIN NguoiDung u  ON u.MaND = pp.MaNV
            WHERE pp.NgayPhat BETWEEN @tu AND @den
            ORDER BY pp.NgayPhat DESC, pp.MaPhieuPhat DESC",
            DbHelper.PDate("@tu", tu), DbHelper.PDate("@den", den));

        /// <summary>
        /// Lập phiếu phạt cho một phiếu mượn.
        /// - Trễ hạn: chỉ hợp lệ nếu thực sự trễ.
        /// - Mất/Hư hỏng: phiếu đang mượn -> đóng phiếu, KHÔNG cộng tồn kho;
        ///   phiếu đã trả (đã cộng tồn kho) -> trừ lại 1 (chỉ với sách in).
        /// </summary>
        public void Lap(int maPhieu, int maNV, bool tre, bool hu, bool mat, decimal phi, string? ghiChu)
        {
            if (!tre && !hu && !mat) throw new InvalidOperationException("Chọn ít nhất một lý do phạt.");
            if (phi <= 0) throw new InvalidOperationException("Phí phạt phải lớn hơn 0.");

            using var cn = new SqlConnection(DbHelper.ConnectionString);
            cn.Open();
            using var tx = cn.BeginTransaction(IsolationLevel.Serializable);
            try
            {
                int maTL;
                DateTime hanTra;
                DateTime? ngayTra;
                using (var cmd = new SqlCommand(
                    "SELECT MaTaiLieu, HanTra, NgayTra FROM PhieuMuon WITH (UPDLOCK, HOLDLOCK) WHERE MaPhieu=@p", cn, tx))
                {
                    cmd.Parameters.AddWithValue("@p", maPhieu);
                    using var rd = cmd.ExecuteReader();
                    if (!rd.Read()) throw new InvalidOperationException("Không tìm thấy phiếu mượn.");
                    maTL = rd.GetInt32(0);
                    hanTra = rd.GetDateTime(1);
                    ngayTra = rd.IsDBNull(2) ? null : rd.GetDateTime(2);
                }

                if (tre && (ngayTra ?? DateTime.Today).Date <= hanTra.Date)
                    throw new InvalidOperationException("Phiếu này không trả trễ hạn.");

                int daPhat = Convert.ToInt32(DbHelper.ScalarTx(cn, tx,
                    "SELECT COUNT(*) FROM PhieuPhat WHERE MaPhieu=@p", DbHelper.P("@p", maPhieu)));
                if (daPhat > 0) throw new InvalidOperationException("Phiếu mượn này đã có phiếu phạt.");

                var lyDo = new List<string>();
                if (tre) lyDo.Add("TreHan");
                if (hu) lyDo.Add("HuHong");
                if (mat) lyDo.Add("Mat");

                DbHelper.ExecTx(cn, tx,
                    @"INSERT INTO PhieuPhat(MaPhieu, MaNV, NgayPhat, LyDo, PhiPhat, GhiChu)
                      VALUES(@p, @nv, CAST(GETDATE() AS date), @ld, @phi, @gc)",
                    DbHelper.P("@p", maPhieu), DbHelper.P("@nv", maNV), DbHelper.P("@ld", string.Join(", ", lyDo)),
                    DbHelper.P("@phi", phi), DbHelper.P("@gc", string.IsNullOrWhiteSpace(ghiChu) ? null : ghiChu));

                if (hu || mat)
                {
                    string tt = mat ? "Mat" : "HuHong";
                    if (ngayTra == null)
                    {
                        DbHelper.ExecTx(cn, tx,
                            "UPDATE PhieuMuon SET NgayTra=GETDATE(), TrangThai=N'DaTra', TinhTrangTra=@tt WHERE MaPhieu=@p",
                            DbHelper.P("@tt", tt), DbHelper.P("@p", maPhieu));
                    }
                    else
                    {
                        DbHelper.ExecTx(cn, tx,
                            "UPDATE PhieuMuon SET TinhTrangTra=@tt WHERE MaPhieu=@p",
                            DbHelper.P("@tt", tt), DbHelper.P("@p", maPhieu));
                        DbHelper.ExecTx(cn, tx,
                            "UPDATE TaiLieu SET SoLuongTon = SoLuongTon - 1 WHERE MaTaiLieu=@tl AND LoaiTaiLieu=N'SachIn' AND SoLuongTon > 0",
                            DbHelper.P("@tl", maTL));
                    }
                }

                tx.Commit();
            }
            catch
            {
                try { tx.Rollback(); } catch { }
                throw;
            }
        }
    }

    // ------------------------------------------------------------------
    public class ThongKeRepository
    {
        /// <summary>Trả về 1 dòng: LuotMuon, QuaHan, Mat, HuHong, TongPhat.</summary>
        public DataRow GetTongHop(DateTime tu, DateTime den)
        {
            if (den.Date < tu.Date) (tu, den) = (den, tu);
            var dt = DbHelper.Query(@"
                SELECT
                  (SELECT COUNT(*) FROM PhieuMuon
                    WHERE CAST(NgayMuon AS date) BETWEEN @tu AND @den) AS LuotMuon,
                  (SELECT COUNT(*) FROM PhieuMuon
                    WHERE HanTra BETWEEN @tu AND @den
                      AND CAST(ISNULL(NgayTra, GETDATE()) AS date) > HanTra) AS QuaHan,
                  (SELECT COUNT(*) FROM PhieuPhat
                    WHERE NgayPhat BETWEEN @tu AND @den AND LyDo LIKE N'%Mat%') AS Mat,
                  (SELECT COUNT(*) FROM PhieuPhat
                    WHERE NgayPhat BETWEEN @tu AND @den AND LyDo LIKE N'%HuHong%') AS HuHong,
                  (SELECT ISNULL(SUM(PhiPhat), 0) FROM PhieuPhat
                    WHERE NgayPhat BETWEEN @tu AND @den) AS TongPhat",
                DbHelper.PDate("@tu", tu), DbHelper.PDate("@den", den));
            return dt.Rows[0];
        }
    }
}