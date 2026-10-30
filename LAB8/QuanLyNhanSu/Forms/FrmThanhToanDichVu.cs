using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using QuanLyNhanSu.Data;

namespace QuanLyNhanSu.Forms
{
    public partial class FrmThanhToanDichVu : Form
    {
        public FrmThanhToanDichVu()
        {
            InitializeComponent();
        }

        private void btnLapHoaDonDV_Click(object sender, EventArgs e)
        {
            string maHD = cboHopDong.SelectedValue.ToString();
            string thangNam = txtThangNam.Text.Trim(); // VD: 10/2026
            string maHDDV = Db.TaoMa("HDDV");

            int chiSoDienCu = (int)numDienCu.Value;
            int chiSoDienMoi = (int)numDienMoi.Value;
            decimal donGiaDien = 3500; // Đơn giá điện chuẩn

            int chiSoNuocCu = (int)numNuocCu.Value;
            int chiSoNuocMoi = (int)numNuocMoi.Value;
            decimal donGiaNuoc = 12000; // Đơn giá nước chuẩn

            // QD01: Công thức tính tiền tiêu thụ
            decimal tienDien = (chiSoDienMoi - chiSoDienCu) * donGiaDien;
            decimal tienNuoc = (chiSoNuocMoi - chiSoNuocCu) * donGiaNuoc;
            decimal tongTien = tienDien + tienNuoc;

            using (SqlConnection cn = Db.OpenConnection())
            using (SqlTransaction tx = cn.BeginTransaction())
            {
                try
                {
                    // 1. Tạo Hóa đơn dịch vụ
                    string sqlHDDV = @"INSERT INTO HoaDonDichVu(MaHDDV, MaHD, ThangNam, NgayLap, NgayHetHan, TongTien, TrangThaiTT)
                                       VALUES(@MaHDDV, @MaHD, @ThangNam, GETDATE(), GETDATE() + 5, @TongTien, N'Chưa thanh toán')";
                    Db.Exec(cn, tx, sqlHDDV,
                        Db.P("@MaHDDV", maHDDV), Db.P("@MaHD", maHD),
                        Db.P("@ThangNam", thangNam), Db.P("@TongTien", tongTien));

                    // 2. Chi tiết điện
                    string sqlCTDien = @"INSERT INTO ChiTietHoaDonDichVu(MaHDDV, MaDV, ChiSoCu, ChiSoMoi, SoLuong, ThanhTien)
                                         VALUES(@MaHDDV, 'DV_DIEN', @Cu, @Moi, @SoLuong, @ThanhTien)";
                    Db.Exec(cn, tx, sqlCTDien,
                        Db.P("@MaHDDV", maHDDV), Db.P("@Cu", chiSoDienCu), Db.P("@Moi", chiSoDienMoi),
                        Db.P("@SoLuong", chiSoDienMoi - chiSoDienCu), Db.P("@ThanhTien", tienDien));

                    // 3. Chi tiết nước
                    string sqlCTNuoc = @"INSERT INTO ChiTietHoaDonDichVu(MaHDDV, MaDV, ChiSoCu, ChiSoMoi, SoLuong, ThanhTien)
                                         VALUES(@MaHDDV, 'DV_NUOC', @Cu, @Moi, @SoLuong, @ThanhTien)";
                    Db.Exec(cn, tx, sqlCTNuoc,
                        Db.P("@MaHDDV", maHDDV), Db.P("@Cu", chiSoNuocCu), Db.P("@Moi", chiSoNuocMoi),
                        Db.P("@SoLuong", chiSoNuocMoi - chiSoNuocCu), Db.P("@ThanhTien", tienNuoc));

                    tx.Commit();
                    MessageBox.Show($"Lập hóa đơn dịch vụ thành công! Tổng tiền: {tongTien:N0} VNĐ", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}