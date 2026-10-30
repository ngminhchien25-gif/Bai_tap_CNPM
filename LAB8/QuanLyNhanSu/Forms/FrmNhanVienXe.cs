using System;
using System.Windows.Forms;
using QuanLyNhanSu.Data;

namespace QuanLyNhanSu.Forms
{
    public partial class FrmNhanVienXe : Form
    {
        public FrmNhanVienXe()
        {
            InitializeComponent();
        }

        private void btnThemNhanVien_Click(object sender, EventArgs e)
        {
            string cccd = txtSoCCCD.Text.Trim();
            // BR05: Kiểm tra duy nhất CCCD
            object checkCCCD = Db.Scalar("SELECT COUNT(*) FROM NhanVienCongTy WHERE SoCMND = @CCCD", Db.P("@CCCD", cccd));
            if (Convert.ToInt32(checkCCCD) > 0)
            {
                MessageBox.Show("LỖI (BR05): CCCD nhân viên này đã tồn tại trong hệ thống!", "Vi phạm", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string maNV = Db.TaoMa("NV");
            string sqlNV = @"INSERT INTO NhanVienCongTy(MaNV, MaKH, MaPhong, HoTen, SoCMND, ChucVu)
                             VALUES(@MaNV, @MaKH, @MaPhong, @HoTen, @CCCD, @ChucVu)";

            Db.Execute(sqlNV,
                Db.P("@MaNV", maNV),
                Db.P("@MaKH", cboKhachHang.SelectedValue),
                Db.P("@MaPhong", cboPhong.SelectedValue),
                Db.P("@HoTen", txtHoTenNV.Text.Trim()),
                Db.P("@CCCD", cccd),
                Db.P("@ChucVu", txtChucVu.Text.Trim()));

            // BR06: Cấp thẻ giữ xe tự động
            if (chkCapTheXe.Checked)
            {
                string maThe = Db.TaoMa("TX");
                string sqlThe = "INSERT INTO TheGiuXe(MaThe, MaNV, BienSoXe, LoaiXe) VALUES(@MaThe, @MaNV, @BienSo, @Loai)";
                Db.Execute(sqlThe,
                    Db.P("@MaThe", maThe),
                    Db.P("@MaNV", maNV),
                    Db.P("@BienSo", txtBienSoXe.Text.Trim()),
                    Db.P("@Loai", cboLoaiXe.SelectedItem.ToString()));
            }

            MessageBox.Show("Thêm nhân viên và cấp thẻ thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}