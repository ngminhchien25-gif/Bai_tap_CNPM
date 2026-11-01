using System;
using System.Windows.Forms;
using QuanLyKhachSanUI.Repositories;

namespace QuanLyKhachSanUI.Forms
{
    public class frmHoaDonThanhToan : Form
    {
        private HoaDonThanhToanRepository _repo = new HoaDonThanhToanRepository();
        private PhieuDatPhongRepository _phieuRepo = new PhieuDatPhongRepository();
        private NhanVienRepository _nvRepo = new NhanVienRepository();

        private DataGridView dgv = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
        // Danh sách các lần thanh toán của hóa đơn đang chọn (1 hóa đơn có thể có nhiều giao dịch ThanhToan)
        private DataGridView dgvThanhToan = new DataGridView { Dock = DockStyle.Bottom, Height = 140, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect };

        private TextBox txtSoHD = new TextBox();
        private TextBox txtSoNgay = new TextBox { Text = "1" };
        // Tiền phòng / tiền dịch vụ do HỆ THỐNG tự tính, không cho nhập tay (đúng BR09 và tránh sai lệch với dữ liệu dịch vụ đã ghi nhận)
        private TextBox txtTienPhong = new TextBox { Text = "0", ReadOnly = true, BackColor = System.Drawing.SystemColors.Control };
        private TextBox txtTienDV = new TextBox { Text = "0", ReadOnly = true, BackColor = System.Drawing.SystemColors.Control };
        private TextBox txtTongTien = new TextBox { Text = "0", ReadOnly = true, BackColor = System.Drawing.SystemColors.Control, Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold) };
        private TextBox txtMaTT = new TextBox();
        private TextBox txtSoTienTT = new TextBox();

        private ComboBox cboPhieuDat = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private ComboBox cboNV = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private ComboBox cboHinhThuc = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private Label lblTrangThai = new Label { Text = "Chưa thanh toán", AutoSize = true, Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold) };

        private Button btnLapHoaDon = new Button { Text = "Lập Hóa Đơn" };
        private Button btnGhiNhanTT = new Button { Text = "Ghi Nhận Thanh Toán" };
        private Button btnTraPhong = new Button { Text = "Hoàn Tất Trả Phòng", Enabled = false };

        public frmHoaDonThanhToan()
        {
            this.Text = "Quản Lý Hóa Đơn & Thanh Toán";
            this.Size = new System.Drawing.Size(900, 680);
            InitCustomControls();
            LoadDropdowns();
            LoadData();
        }

        private void InitCustomControls()
        {
            TableLayoutPanel panel = new TableLayoutPanel { Dock = DockStyle.Top, RowCount = 6, ColumnCount = 4, Height = 210, Padding = new Padding(10) };
            panel.Controls.Add(new Label { Text = "Số Hóa Đơn:" }, 0, 0); panel.Controls.Add(txtSoHD, 1, 0);
            panel.Controls.Add(new Label { Text = "Số Phiếu Đặt:" }, 2, 0); panel.Controls.Add(cboPhieuDat, 3, 0);
            panel.Controls.Add(new Label { Text = "Nhân Viên Lập:" }, 0, 1); panel.Controls.Add(cboNV, 1, 1);
            panel.Controls.Add(new Label { Text = "Số Ngày Tính Tiền:" }, 2, 1); panel.Controls.Add(txtSoNgay, 3, 1);
            panel.Controls.Add(new Label { Text = "Tiền Phòng (tự tính):" }, 0, 2); panel.Controls.Add(txtTienPhong, 1, 2);
            panel.Controls.Add(new Label { Text = "Tiền Dịch Vụ (tự tính):" }, 2, 2); panel.Controls.Add(txtTienDV, 3, 2);
            panel.Controls.Add(new Label { Text = "Tổng Tiền Hóa Đơn:" }, 0, 3); panel.Controls.Add(txtTongTien, 1, 3);
            panel.Controls.Add(new Label { Text = "Trạng Thái HD:" }, 2, 3); panel.Controls.Add(lblTrangThai, 3, 3);
            panel.Controls.Add(new Label { Text = "Mã Thanh Toán:" }, 0, 4); panel.Controls.Add(txtMaTT, 1, 4);
            panel.Controls.Add(new Label { Text = "Hình Thức TT:" }, 2, 4); panel.Controls.Add(cboHinhThuc, 3, 4);
            panel.Controls.Add(new Label { Text = "Số Tiền Thanh Toán:" }, 0, 5); panel.Controls.Add(txtSoTienTT, 1, 5);

            // Khớp đúng 4 giá trị cho phép theo ràng buộc CK_ThanhToan_HinhThuc trong CSDL
            cboHinhThuc.Items.AddRange(new string[] { "Tiền mặt", "Chuyển khoản", "Thẻ", "Ví điện tử" });
            cboHinhThuc.SelectedIndex = 0;

            FlowLayoutPanel pnlBtn = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40 };
            pnlBtn.Controls.AddRange(new Control[] { btnLapHoaDon, btnGhiNhanTT, btnTraPhong });

            this.Controls.Add(dgv);
            this.Controls.Add(dgvThanhToan);
            this.Controls.Add(pnlBtn);
            this.Controls.Add(panel);

            cboPhieuDat.SelectedIndexChanged += (s, e) => CapNhatTienPhongDichVu();
            txtSoNgay.TextChanged += (s, e) => CapNhatTienPhongDichVu();

            dgv.SelectionChanged += (s, e) =>
            {
                if (dgv.CurrentRow == null) return;
                string soHD = dgv.CurrentRow.Cells["SoHoaDon"].Value?.ToString();
                string trangThai = dgv.CurrentRow.Cells["TrangThai"].Value?.ToString();
                txtMaTT.Tag = soHD; // ghi nhớ hóa đơn đang chọn để thanh toán / trả phòng
                lblTrangThai.Text = trangThai;
                btnTraPhong.Enabled = trangThai == "Đã thanh toán";
                dgvThanhToan.DataSource = _repo.GetThanhToanByHoaDon(soHD);
            };

            btnLapHoaDon.Click += (s, e) =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(txtSoHD.Text) || cboPhieuDat.SelectedValue == null || cboNV.SelectedValue == null)
                    {
                        MessageBox.Show("Vui lòng điền và chọn đầy đủ thông tin!");
                        return;
                    }
                    if (!int.TryParse(txtSoNgay.Text, out int soNgay) || soNgay <= 0)
                    {
                        MessageBox.Show("Số ngày tính tiền phải là số nguyên dương!");
                        return;
                    }

                    decimal tp = decimal.TryParse(txtTienPhong.Text, out var r1) ? r1 : 0;
                    decimal tdv = decimal.TryParse(txtTienDV.Text, out var r2) ? r2 : 0;

                    // Cần bổ sung trong HoaDonThanhToanRepository:
                    //   public void LapHoaDon(string soHD, string soPhieuDat, string maNV, int soNgay,
                    //                          decimal tienPhong, decimal tienDichVu)
                    //   -> INSERT vào HoaDon với TrangThai mặc định N'Chưa thanh toán' (không nhận tham số trạng thái từ UI)
                    _repo.LapHoaDon(txtSoHD.Text.Trim(), cboPhieuDat.SelectedValue.ToString(), cboNV.SelectedValue.ToString(), soNgay, tp, tdv);

                    LoadData();
                    MessageBox.Show("Lập hóa đơn thành công! Tiếp tục ghi nhận thanh toán bên dưới.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi: " + ex.Message);
                }
            };

            btnGhiNhanTT.Click += (s, e) =>
            {
                try
                {
                    string soHD = txtMaTT.Tag as string;
                    if (string.IsNullOrEmpty(soHD))
                    {
                        MessageBox.Show("Vui lòng chọn 1 hóa đơn trong danh sách trước khi thanh toán!");
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(txtMaTT.Text))
                    {
                        MessageBox.Show("Vui lòng nhập Mã Thanh Toán!");
                        return;
                    }
                    if (!decimal.TryParse(txtSoTienTT.Text, out decimal soTien) || soTien <= 0)
                    {
                        MessageBox.Show("Số tiền thanh toán không hợp lệ!");
                        return;
                    }

                    // Cần bổ sung trong HoaDonThanhToanRepository:
                    //   public void GhiNhanThanhToan(string maTT, string soHD, string hinhThuc, decimal soTien)
                    //   -> Trong transaction: kiểm tra (đã trả + soTien) so với TongTien hóa đơn,
                    //      từ chối nếu vượt; INSERT ThanhToan; nếu đủ tiền thì UPDATE HoaDon
                    //      SET TrangThai = N'Đã thanh toán'.
                    _repo.GhiNhanThanhToan(txtMaTT.Text.Trim(), soHD, cboHinhThuc.Text, soTien);

                    LoadData();
                    dgvThanhToan.DataSource = _repo.GetThanhToanByHoaDon(soHD);
                    MessageBox.Show("Ghi nhận thanh toán thành công!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi: " + ex.Message);
                }
            };

            btnTraPhong.Click += (s, e) =>
            {
                try
                {
                    string soHD = txtMaTT.Tag as string;
                    if (string.IsNullOrEmpty(soHD)) return;

                    var xacNhan = MessageBox.Show("Xác nhận hoàn tất trả phòng cho hóa đơn này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (xacNhan != DialogResult.Yes) return;

                    // Cần bổ sung trong HoaDonThanhToanRepository:
                    //   public void TraPhongTheoHoaDon(string soHoaDon)
                    //   -> Từ SoHoaDon suy ra SoPhieuDat, kiểm tra TrangThai = N'Đã thanh toán',
                    //      UPDATE PhieuDatPhong.TrangThai = N'Đã trả', UPDATE Phong.TrangThai = N'Trống'
                    //      cho các phòng thuộc phiếu đó.
                    _repo.TraPhongTheoHoaDon(soHD);

                    LoadData();
                    MessageBox.Show("Trả phòng thành công!");
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

            cboNV.DataSource = _nvRepo.GetAllNhanVien();
            cboNV.DisplayMember = "HoTen";
            cboNV.ValueMember = "MaNV";

            CapNhatTienPhongDichVu();
        }

        // Tự tính Tiền Phòng = tổng đơn giá/ngày của các phòng trong phiếu đặt x Số ngày tính tiền,
        // và Tiền Dịch Vụ = tổng cộng dồn từ các Phiếu Sử Dụng DV của phiếu đặt đó (đúng BR09).
        private void CapNhatTienPhongDichVu()
        {
            if (cboPhieuDat.SelectedValue == null)
            {
                txtTienPhong.Text = "0"; txtTienDV.Text = "0"; txtTongTien.Text = "0";
                return;
            }
            string soPhieuDat = cboPhieuDat.SelectedValue.ToString();
            int.TryParse(txtSoNgay.Text, out int soNgay);
            if (soNgay <= 0) soNgay = 1;

            // Cần bổ sung trong HoaDonThanhToanRepository:
            //   public decimal LayTongDonGiaPhong(string soPhieuDat)
            //     -> SELECT ISNULL(SUM(p.DonGiaNgay),0) FROM ChiTietDatPhong c
            //        JOIN Phong p ON c.SoPhong=p.SoPhong WHERE c.SoPhieuDat=@s
            //   public decimal LayTongTienDichVu(string soPhieuDat)
            //     -> SELECT ISNULL(SUM(c.ThanhTien),0) FROM PhieuSuDungDV h
            //        JOIN ChiTietPhieuSuDungDV c ON h.SoPhieuSDDV=c.SoPhieuSDDV WHERE h.SoPhieuDat=@s
            decimal donGiaPhong = _repo.LayTongDonGiaPhong(soPhieuDat);
            decimal tienDV = _repo.LayTongTienDichVu(soPhieuDat);

            decimal tienPhong = donGiaPhong * soNgay;
            txtTienPhong.Text = tienPhong.ToString();
            txtTienDV.Text = tienDV.ToString();
            txtTongTien.Text = (tienPhong + tienDV).ToString();
        }

        private void LoadData() => dgv.DataSource = _repo.GetAllHoaDon();
    }
}