using System;
using System.Windows.Forms;
using QuanLyNhanSu.Data;

namespace QuanLyNhanSu.Forms
{
    public partial class FrmKiemTraPhong : Form
    {
        public FrmKiemTraPhong()
        {
            InitializeComponent();
        }

        private void btnLapBienBan_Click(object sender, EventArgs e)
        {
            string maPKT = Db.TaoMa("PKT");
            string maPhong = cboPhong.SelectedValue.ToString();
            bool isViPham = chkViPhamKetCau.Checked; // BR07
            decimal mucPhat = isViPham ? Convert.ToDecimal(numMucPhat.Value) : 0;

            string sql = @"INSERT INTO PhieuKiemTraPhong(MaPKT, MaPhong, NgayKiemTra, NguoiKiemTra, TinhTrang, PhatHienSua, MucPhat)
                           VALUES(@MaPKT, @MaPhong, GETDATE(), @NguoiKT, @TinhTrang, @ViPham, @MucPhat)";

            Db.Execute(sql,
                Db.P("@MaPKT", maPKT),
                Db.P("@MaPhong", maPhong),
                Db.P("@NguoiKT", txtNguoiKiemTra.Text.Trim()),
                Db.P("@TinhTrang", txtTinhTrang.Text.Trim()),
                Db.P("@ViPham", isViPham ? 1 : 0),
                Db.P("@MucPhat", mucPhat));

            if (isViPham)
            {
                MessageBox.Show($"Đã lập biên bản vi phạm tự ý sửa kết cấu phòng! Mức phạt: {mucPhat:N0} VNĐ", "Cảnh báo vi phạm BR07", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                MessageBox.Show("Ghi nhận kết quả kiểm tra phòng bình thường!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}