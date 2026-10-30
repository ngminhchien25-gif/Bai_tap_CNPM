using System;
using System.Data;
using System.Windows.Forms;
using QuanLyNhanSu.Data;

namespace QuanLyNhanSu.Forms
{
    public partial class FrmDanhMucPhong : Form
    {
        public FrmDanhMucPhong()
        {
            InitializeComponent();
        }

        private void FrmDanhMucPhong_Load(object sender, EventArgs e)
        {
            LoadDataPhong();
        }

        private void LoadDataPhong()
        {
            string sql = @"SELECT p.MaPhong, p.TenPhong, p.DienTich, p.HuongPhong, p.DoCao, p.DonGiaThue, p.TrangThai, t.TenTang 
                           FROM Phong p INNER JOIN Tang t ON p.MaTang = t.MaTang";
            dgvPhong.DataSource = Db.Query(sql);
        }

        private void btnThemPhong_Click(object sender, EventArgs e)
        {
            string maPhong = Db.TaoMa("P");
            string sql = @"INSERT INTO Phong(MaPhong, TenPhong, DienTich, HuongPhong, DoCao, DonGiaThue, TrangThai, MaTang)
                           VALUES(@Ma, @Ten, @DT, @Huong, @Cao, @Gia, N'Trống', @Tang)";

            int result = Db.Execute(sql,
                Db.P("@Ma", maPhong),
                Db.P("@Ten", txtTenPhong.Text.Trim()),
                Db.P("@DT", Convert.ToDecimal(numDienTich.Value)),
                Db.P("@Huong", txtHuong.Text.Trim()),
                Db.P("@Cao", Convert.ToDecimal(numDoCao.Value)),
                Db.P("@Gia", Convert.ToDecimal(numDonGia.Value)),
                Db.P("@Tang", cboTang.SelectedValue));

            if (result > 0)
            {
                MessageBox.Show("Thêm văn phòng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadDataPhong();
            }
        }
    }
}