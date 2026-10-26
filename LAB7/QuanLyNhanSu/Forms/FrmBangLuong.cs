using System;
using System.Windows.Forms;
using QuanLyNhanSu.Services;

namespace QuanLyNhanSu.Forms
{
    public partial class FrmBangLuong : Form
    {
        private readonly LuongService service = new LuongService();
        public FrmBangLuong() { InitializeComponent(); }

        private void FrmBangLuong_Load(object sender, EventArgs e)
        {
            numThang.Value = DateTime.Today.Month; numNam.Value = DateTime.Today.Year;
        }

        private void TaiLai()
        {
            dgvBangLuong.DataSource = service.LayBangLuong((int)numThang.Value, (int)numNam.Value);
            decimal tong = 0;
            foreach (DataGridViewRow r in dgvBangLuong.Rows) if (r.Cells["ThucLinh"].Value != null) tong += Convert.ToDecimal(r.Cells["ThucLinh"].Value);
            lblTong.Text = "Tổng thực lĩnh: " + tong.ToString("N0") + " đ";
        }

        private void HienKetQua(KetQuaXuLy kq)
        {
            MessageBox.Show(kq.ThongBao, kq.ThanhCong ? "Thông báo" : "Lỗi", MessageBoxButtons.OK, kq.ThanhCong ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (kq.ThanhCong) TaiLai();
        }

        private void btnTinhLuong_Click(object sender, EventArgs e) { HienKetQua(service.TinhLuong((int)numThang.Value, (int)numNam.Value)); }

        private void btnChot_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Sau khi chốt không được tính lại. Tiếp tục?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                HienKetQua(service.ChotBangLuong((int)numThang.Value, (int)numNam.Value));
        }

        private void btnXem_Click(object sender, EventArgs e) { TaiLai(); }
        private void btnDong_Click(object sender, EventArgs e) { Close(); }
    }
}
