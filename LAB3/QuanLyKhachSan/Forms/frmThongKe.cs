using System;
using System.Windows.Forms;
using QuanLyKhachSanUI.Repositories;

namespace QuanLyKhachSanUI.Forms
{
    public class frmThongKe : Form
    {
        private ThongKeRepository _repo = new ThongKeRepository();

        private DateTimePicker dtTu = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Now.AddMonths(-1), Width = 120 };
        private DateTimePicker dtDen = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Now, Width = 120 };
        private Button btnThongKe = new Button { Text = "Thống Kê" };

        private Label lblTieuDeTongHop = new Label { Text = "Tổng hợp số liệu", AutoSize = true, Padding = new Padding(10, 8, 0, 4), Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold) };
        private DataGridView dgvTongHop = new DataGridView { Dock = DockStyle.Top, Height = 90, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };

        private Label lblTieuDeDV = new Label { Text = "Thống kê dịch vụ sử dụng", AutoSize = true, Padding = new Padding(10, 8, 0, 4), Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold) };
        private DataGridView dgvDichVu = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };

        public frmThongKe()
        {
            this.Text = "Thống Kê";
            this.Size = new System.Drawing.Size(850, 600);
            InitLayout();
            ThucHienThongKe();
        }

        private void InitLayout()
        {
            TableLayoutPanel panel = new TableLayoutPanel { Dock = DockStyle.Top, RowCount = 1, ColumnCount = 5, Height = 50, Padding = new Padding(10) };
            panel.Controls.Add(new Label { Text = "Từ ngày:", AutoSize = true, Anchor = AnchorStyles.Left, Padding = new Padding(0, 8, 5, 0) }, 0, 0);
            panel.Controls.Add(dtTu, 1, 0);
            panel.Controls.Add(new Label { Text = "Đến ngày:", AutoSize = true, Anchor = AnchorStyles.Left, Padding = new Padding(10, 8, 5, 0) }, 2, 0);
            panel.Controls.Add(dtDen, 3, 0);
            panel.Controls.Add(btnThongKe, 4, 0);

            btnThongKe.Click += (s, e) => ThucHienThongKe();

            // Thứ tự add quyết định vị trí khi cùng Dock = Top: control add SAU cùng sẽ nằm TRÊN CÙNG.
            // Vì vậy add theo thứ tự ngược để có layout từ trên xuống: panel -> lblTongHop -> dgvTongHop -> lblDV -> dgvDichVu(Fill)
            this.Controls.Add(dgvDichVu);
            this.Controls.Add(lblTieuDeDV);
            this.Controls.Add(dgvTongHop);
            this.Controls.Add(lblTieuDeTongHop);
            this.Controls.Add(panel);

            lblTieuDeTongHop.Dock = DockStyle.Top;
            lblTieuDeDV.Dock = DockStyle.Top;
        }

        private void ThucHienThongKe()
        {
            if (dtDen.Value.Date < dtTu.Value.Date)
            {
                MessageBox.Show("Đến ngày không được trước Từ ngày!");
                return;
            }
            try
            {
                dgvTongHop.DataSource = _repo.TongHop(dtTu.Value, dtDen.Value);
                dgvDichVu.DataSource = _repo.ThongKeDichVu(dtTu.Value, dtDen.Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }
    }
}