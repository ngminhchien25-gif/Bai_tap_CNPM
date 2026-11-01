using System;
using System.Data;
using System.Windows.Forms;
using QuanLyKhachSanUI.Repositories;

namespace QuanLyKhachSanUI.Forms
{
    public class frmDenBu : Form
    {
        private DenBuRepository _repo = new DenBuRepository();
        private PhieuDatPhongRepository _phieuRepo = new PhieuDatPhongRepository();
        private PhongRepository _phongRepo = new PhongRepository();
        private NhanVienRepository _nvRepo = new NhanVienRepository();
        private TienNghiRepository _tnRepo = new TienNghiRepository();

        private DataGridView dgv = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
        private TextBox txtSoPhieuDB = new TextBox();
        // Số tiền đền bù được HỆ THỐNG tự tra từ bảng QuyDinhDenBu, không cho gõ tay để tránh lệch quy định (BR08)
        private TextBox txtSoTien = new TextBox { Text = "0", ReadOnly = true, BackColor = System.Drawing.SystemColors.Control };
        private ComboBox cboPhieuDat = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private ComboBox cboPhong = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private ComboBox cboTienNghi = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private ComboBox cboMucDo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private ComboBox cboNV = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private Label lblGhiChuMuc = new Label { AutoSize = true, ForeColor = System.Drawing.Color.DarkRed, Text = "" };
        private Button btnLuu = new Button { Text = "Lập Phiếu Đền Bù" };

        public frmDenBu()
        {
            this.Text = "Quản Lý Hư Hỏng & Đền Bù";
            this.Size = new System.Drawing.Size(850, 560);
            InitCustomControls();
            LoadDropdowns();
            LoadData();
        }

        private void InitCustomControls()
        {
            TableLayoutPanel panel = new TableLayoutPanel { Dock = DockStyle.Top, RowCount = 5, ColumnCount = 4, Height = 190, Padding = new Padding(10) };
            panel.Controls.Add(new Label { Text = "Số Phiếu Đền Bù:" }, 0, 0); panel.Controls.Add(txtSoPhieuDB, 1, 0);
            panel.Controls.Add(new Label { Text = "Số Phiếu Đặt:" }, 2, 0); panel.Controls.Add(cboPhieuDat, 3, 0);
            panel.Controls.Add(new Label { Text = "Số Phòng:" }, 0, 1); panel.Controls.Add(cboPhong, 1, 1);
            panel.Controls.Add(new Label { Text = "Tiện Nghi Hỏng:" }, 2, 1); panel.Controls.Add(cboTienNghi, 3, 1);
            panel.Controls.Add(new Label { Text = "Mức Độ Thiệt Hại:" }, 0, 2); panel.Controls.Add(cboMucDo, 1, 2);
            panel.Controls.Add(new Label { Text = "Số Tiền Đền Bù (tự động):" }, 2, 2); panel.Controls.Add(txtSoTien, 3, 2);
            panel.Controls.Add(new Label { Text = "NV Ghi Nhận:" }, 0, 3); panel.Controls.Add(cboNV, 1, 3);
            panel.Controls.Add(lblGhiChuMuc, 0, 4); panel.SetColumnSpan(lblGhiChuMuc, 4);

            // Mức độ thiệt hại phải khớp với dữ liệu cấu hình trong bảng QuyDinhDenBu
            // (seed: "Hư hỏng nhẹ", "Mất") để tra được đúng mức đền bù.
            cboMucDo.Items.AddRange(new string[] { "Hư hỏng nhẹ", "Mất" });
            cboMucDo.SelectedIndex = 0;

            // Chọn Phòng -> tải các tiện nghi đã lắp đặt tại phòng đó
            cboPhong.SelectedIndexChanged += (s, e) => { LoadTienNghiByPhong(); CapNhatSoTienDenBu(); };
            // Chọn tiện nghi hoặc đổi mức độ -> tự tra lại số tiền đền bù theo quy định
            cboTienNghi.SelectedIndexChanged += (s, e) => CapNhatSoTienDenBu();
            cboMucDo.SelectedIndexChanged += (s, e) => CapNhatSoTienDenBu();

            btnLuu.Dock = DockStyle.Top;
            this.Controls.Add(dgv);
            this.Controls.Add(btnLuu);
            this.Controls.Add(panel);

            btnLuu.Click += (s, e) =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(txtSoPhieuDB.Text) ||
                        cboPhieuDat.SelectedValue == null ||
                        cboPhong.SelectedValue == null ||
                        cboTienNghi.SelectedValue == null ||
                        cboNV.SelectedValue == null)
                    {
                        MessageBox.Show("Vui lòng điền và chọn đầy đủ thông tin!");
                        return;
                    }

                    string soPhieuDB = txtSoPhieuDB.Text.Trim();
                    string soPhieuDat = cboPhieuDat.SelectedValue.ToString();
                    string soPhong = cboPhong.SelectedValue.ToString();
                    string maNV = cboNV.SelectedValue.ToString();
                    string maTienNghi = cboTienNghi.SelectedValue.ToString();
                    string mucDo = cboMucDo.SelectedItem?.ToString() ?? "";

                    // Số tiền lấy đúng theo giá trị hệ thống đã tự tra (không cho nhập tay)
                    decimal tien = decimal.TryParse(txtSoTien.Text, out var r) ? r : 0;
                    if (tien <= 0)
                    {
                        var xacNhan = MessageBox.Show(
                            "Chưa tìm thấy mức đền bù cấu hình sẵn cho tiện nghi/mức độ này (số tiền = 0).\n" +
                            "Bạn có muốn tiếp tục lập phiếu với số tiền 0 không?",
                            "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (xacNhan != DialogResult.Yes) return;
                    }

                    _repo.LapPhieuDenBuTransaction(soPhieuDB, soPhieuDat, soPhong, maNV, maTienNghi, mucDo, tien);

                    LoadData();
                    MessageBox.Show("Lập phiếu đền bù thành công!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi: " + ex.Message);
                }
            };
        }

        private void LoadDropdowns()
        {
            cboPhieuDat.DataSource = _phieuRepo.GetAllPhieuDatPhong();
            cboPhieuDat.DisplayMember = "SoPhieuDat";
            cboPhieuDat.ValueMember = "SoPhieuDat";

            cboPhong.DataSource = _phongRepo.GetAllPhong();
            cboPhong.DisplayMember = "SoPhong";
            cboPhong.ValueMember = "SoPhong";

            cboNV.DataSource = _nvRepo.GetAllNhanVien();
            cboNV.DisplayMember = "HoTen";
            cboNV.ValueMember = "MaNV";

            LoadTienNghiByPhong();
            CapNhatSoTienDenBu();
        }

        private void LoadTienNghiByPhong()
        {
            // GetTienNghiByPhong / GetAllTienNghi phải trả về cả cột MaLoaiTN (không chỉ TenLoaiTN)
            // để tra được đúng mức đền bù theo Loại tiện nghi.
            if (cboPhong.SelectedValue != null && cboPhong.SelectedValue is string soPhong)
            {
                DataTable dt = _repo.GetTienNghiByPhong(soPhong);
                if (dt != null && dt.Rows.Count > 0)
                {
                    cboTienNghi.DataSource = dt;
                    cboTienNghi.DisplayMember = "TenLoaiTN";
                    cboTienNghi.ValueMember = "MaTienNghi";
                    return;
                }
            }

            DataTable dtAll = _tnRepo.GetAllTienNghi();
            cboTienNghi.DataSource = dtAll;
            cboTienNghi.DisplayMember = "TenLoaiTN";
            cboTienNghi.ValueMember = "MaTienNghi";
        }

        // Tự động tra Số Tiền Đền Bù theo (Loại tiện nghi của thiết bị đang chọn + Mức độ thiệt hại)
        // dựa trên bảng cấu hình QuyDinhDenBu, thay vì để nhân viên gõ tay (dễ sai lệch quy định - BR08).
        private void CapNhatSoTienDenBu()
        {
            lblGhiChuMuc.Text = "";
            if (!(cboTienNghi.SelectedItem is DataRowView row) || cboMucDo.SelectedItem == null)
            {
                txtSoTien.Text = "0";
                return;
            }

            string maLoaiTN = row.Row.Table.Columns.Contains("MaLoaiTN") ? row["MaLoaiTN"]?.ToString() : null;
            string mucDo = cboMucDo.SelectedItem.ToString();

            if (string.IsNullOrEmpty(maLoaiTN))
            {
                txtSoTien.Text = "0";
                lblGhiChuMuc.Text = "Không xác định được loại tiện nghi để tra mức đền bù.";
                return;
            }

            // Cần bổ sung trong DenBuRepository:
            //   public decimal LayMucDenBuTheoQuyDinh(string maLoaiTN, string mucDoThietHai)
            //   {
            //       var dt = Db.Query(
            //           "SELECT MucDenBu FROM QuyDinhDenBu WHERE MaLoaiTN=@l AND MucDoThietHai=@m",
            //           new SqlParameter("@l", maLoaiTN), new SqlParameter("@m", mucDoThietHai));
            //       return dt.Rows.Count > 0 ? Convert.ToDecimal(dt.Rows[0]["MucDenBu"]) : 0m;
            //   }
            decimal muc = _repo.LayMucDenBuTheoQuyDinh(maLoaiTN, mucDo);
            txtSoTien.Text = muc.ToString();

            if (muc == 0)
                lblGhiChuMuc.Text = "Chưa có quy định đền bù cho loại tiện nghi + mức độ này trong QuyDinhDenBu.";
        }

        private void LoadData() => dgv.DataSource = _repo.GetAllPhieuDenBu();
    }
}