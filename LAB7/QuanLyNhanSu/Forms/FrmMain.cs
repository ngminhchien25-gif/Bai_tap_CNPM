using System;
using System.Windows.Forms;

namespace QuanLyNhanSu.Forms
{
    public partial class FrmMain : Form
    {
        public FrmMain() { InitializeComponent(); }
        private void btnDanhMuc_Click(object sender, EventArgs e) { using (FrmDanhMuc f = new FrmDanhMuc()) f.ShowDialog(this); }
        private void btnTuyenDung_Click(object sender, EventArgs e) { using (FrmTuyenDung f = new FrmTuyenDung()) f.ShowDialog(this); }
        private void btnQuyetDinh_Click(object sender, EventArgs e) { using (FrmQuyetDinh f = new FrmQuyetDinh()) f.ShowDialog(this); }
        private void btnChamCong_Click(object sender, EventArgs e) { using (FrmChamCong f = new FrmChamCong()) f.ShowDialog(this); }
        private void btnKhenThuong_Click(object sender, EventArgs e) { using (FrmKhenThuong f = new FrmKhenThuong()) f.ShowDialog(this); }
        private void btnBangLuong_Click(object sender, EventArgs e) { using (FrmBangLuong f = new FrmBangLuong()) f.ShowDialog(this); }
        private void btnThoat_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Bạn có thực sự muốn thoát chương trình?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) Close();
        }
    }
}
