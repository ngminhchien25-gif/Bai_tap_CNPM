using System;
using System.Data;
using System.Windows.Forms;
using QuanLyNhanSu.Services;

namespace QuanLyNhanSu.Forms
{
    public partial class FrmKhenThuong : Form
    {
        private readonly KhenThuongService service = new KhenThuongService();
        private readonly QuyetDinhService quyetDinh = new QuyetDinhService();
        private readonly DanhMucService danhMuc = new DanhMucService();
        public FrmKhenThuong() { InitializeComponent(); }

        private void FrmKhenThuong_Load(object sender, EventArgs e)
        {
            cboLoaiKT.Items.AddRange(new object[] { "Thường", "Lễ", "Cuối năm", "Lương tháng 13" }); cboLoaiKT.SelectedIndex = 0;
            cboCachThu.Items.AddRange(new object[] { "Nộp phạt", "Trừ lương" }); cboCachThu.SelectedIndex = 0;
            rdoCaNhan.Checked = true;
            NapDoiTuong(true);
            dgvKhenThuong.DataSource = quyetDinh.LayDanhSach("KhenThuong");
            dgvKyLuat.DataSource = quyetDinh.LayDanhSach("KyLuat");
        }

        // Mục hiển thị trong CheckedListBox: ToString() trả về tên để hiện lên danh sách
        private class MucDoiTuong
        {
            public string Ma;
            public string Ten;
            public override string ToString() { return Ten; }
        }

        // Cá nhân → nạp danh sách nhân viên; tập thể → nạp danh sách phòng ban (BR06)
        private void NapDoiTuong(bool caNhan)
        {
            clbDoiTuong.Items.Clear();
            DataTable dt = danhMuc.LayBang(caNhan ? "NhanVien" : "PhongBan");
            foreach (DataRow r in dt.Rows)
            {
                string ma = Convert.ToString(r[caNhan ? "MaNV" : "MaPB"]);
                string ten = caNhan ? Convert.ToString(r["HoTen"]) + " (" + ma + ")" : Convert.ToString(r["TenPB"]) + " (" + ma + ")";
                clbDoiTuong.Items.Add(new MucDoiTuong { Ma = ma, Ten = ten });
            }
        }

        private void rdoCaNhan_CheckedChanged(object sender, EventArgs e) { NapDoiTuong(rdoCaNhan.Checked); }

        private System.Collections.Generic.List<DoiTuongDto> LayDoiTuong()
        {
            var ds = new System.Collections.Generic.List<DoiTuongDto>();
            foreach (object item in clbDoiTuong.CheckedItems)
            {
                MucDoiTuong m = (MucDoiTuong)item;
                ds.Add(rdoCaNhan.Checked ? new DoiTuongDto { MaNV = m.Ma } : new DoiTuongDto { MaPB = m.Ma });
            }
            return ds;
        }

        private void Hien(KetQuaXuLy kq)
        {
            MessageBox.Show(kq.ThongBao, kq.ThanhCong ? "Thông báo" : "Lỗi", MessageBoxButtons.OK, kq.ThanhCong ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            dgvKhenThuong.DataSource = quyetDinh.LayDanhSach("KhenThuong");
            dgvKyLuat.DataSource = quyetDinh.LayDanhSach("KyLuat");
        }

        private void btnLapKT_Click(object sender, EventArgs e)
        {
            KhenThuongDto d = new KhenThuongDto
            {
                LoaiDoiTuong = rdoCaNhan.Checked ? "CaNhan" : "TapThe", LoaiKhenThuong = Convert.ToString(cboLoaiKT.SelectedItem),
                HinhThuc = txtHinhThuc.Text.Trim(), SoTien = numSoTien.Value, LyDo = txtLyDo.Text.Trim(), NgayHieuLuc = dtHieuLuc.Value
            };
            d.DoiTuong.AddRange(LayDoiTuong());
            Hien(service.LapKhenThuong(d));
        }

        private void btnLapKL_Click(object sender, EventArgs e)
        {
            KyLuatDto d = new KyLuatDto
            {
                LoaiDoiTuong = rdoCaNhan.Checked ? "CaNhan" : "TapThe", HinhThuc = txtHinhThucKL.Text.Trim(), SoTienPhat = numTienPhat.Value,
                CachThu = Convert.ToString(cboCachThu.SelectedItem), LyDo = txtLyDoKL.Text.Trim(), NgayHieuLuc = dtHieuLuc.Value
            };
            d.DoiTuong.AddRange(LayDoiTuong());
            Hien(service.LapKyLuat(d));
        }

        private string SoQDDangChon()
        {
            DataGridView g = tabs.SelectedTab == tabKhenThuong ? dgvKhenThuong : dgvKyLuat;
            return g.CurrentRow == null ? "" : Convert.ToString(g.CurrentRow.Cells["SoQD"].Value);
        }

        private void btnTrinhKy_Click(object sender, EventArgs e) { Hien(quyetDinh.ChuyenTrangThai(SoQDDangChon(), "Chờ duyệt", null)); }
        private void btnGiamDocKy_Click(object sender, EventArgs e) { Hien(quyetDinh.ChuyenTrangThai(SoQDDangChon(), "Đã ký", "Giám đốc")); }
        private void btnDong_Click(object sender, EventArgs e) { Close(); }
    }
}