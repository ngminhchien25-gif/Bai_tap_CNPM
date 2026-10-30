using System;
using System.Windows.Forms;
using QuanLyNhanSu.Data;

namespace QuanLyNhanSu.Forms
{
    public partial class FrmKhachHang : Form
    {
        public FrmKhachHang()
        {
            InitializeComponent();
        }

        private void FrmKhachHang_Load(object sender, EventArgs e)
        {
            LoadDataKhachHang();
        }

        private void LoadDataKhachHang()
        {
            dgvKhachHang.DataSource = Db.Query("SELECT * FROM KhachHang");
        }

        private void btnThemKhachHang_Click(object sender, EventArgs e)
        {
            string cccd = txtCCCD.Text.Trim();
            // Kiểm tra trùng CCCD/MST
            object checkExist = Db.Scalar("SELECT COUNT(*) FROM KhachHang WHERE SoCMND_CCCD = @CCCD", Db.P("@CCCD", cccd));
            if (Convert.ToInt32(checkExist) > 0)
            {
                MessageBox.Show("Số CCCD/MST này đã tồn tại trong hệ thống!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string maKH = Db.TaoMa("KH");
            string sql = @"INSERT INTO KhachHang(MaKH, TenKH, SoCMND_CCCD, SoDienThoai, Email, TenCongTy)
                           VALUES(@MaKH, @TenKH, @CCCD, @SDT, @Email, @TenCT)";

            Db.Execute(sql,
                Db.P("@MaKH", maKH),
                Db.P("@TenKH", txtTenKH.Text.Trim()),
                Db.P("@CCCD", cccd),
                Db.P("@SDT", txtSDT.Text.Trim()),
                Db.P("@Email", txtEmail.Text.Trim()),
                Db.P("@TenCT", txtTenCongTy.Text.Trim()));

            MessageBox.Show("Thêm khách hàng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadDataKhachHang();
        }
    }
}