using System;
using System.Windows.Forms;
using QuanLyNhanSu.Services;

namespace QuanLyNhanSu.Forms
{
    public partial class FrmHopDongThue : Form
    {
        private HopDongService hopDongService;

        public FrmHopDongThue()
        {
            InitializeComponent();
            hopDongService = new HopDongService();
        }

        private void btnLapHopDong_Click(object sender, EventArgs e)
        {
            string maKH = cboKhachHang.SelectedValue?.ToString();
            string maPhong = cboPhongTrong.SelectedValue?.ToString();
            int thoiHan = (int)numThoiHanThue.Value;
            DateTime ngayBD = dtpNgayBatDau.Value;
            decimal donGia = Convert.ToDecimal(txtDonGiaThue.Text);

            var result = hopDongService.LapHopDong(maKH, maPhong, thoiHan, ngayBD, donGia);

            if (result.Success)
                MessageBox.Show(result.Message, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show(result.Message, "Lỗi nghiệp vụ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}