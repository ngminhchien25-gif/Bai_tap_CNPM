using System.Data;
using QuanLyThuVien.Repositories;

namespace QuanLyThuVien.Forms
{
    public class frmThongKe : Form
    {
        private readonly ThongKeRepository _repo = new();
        private readonly PhieuPhatRepository _phatRepo = new();

        private DateTimePicker dtpTu = null!, dtpDen = null!;
        private Button btnThongKe = null!;
        private Label lblMuon = null!, lblQuaHan = null!, lblMat = null!, lblHuHong = null!, lblPhiPhat = null!;
        private DataGridView dgvPhat = null!;

        public frmThongKe()
        {
            Text = "Thống kê";
            Width = 1000;
            Height = 600;
            StartPosition = FormStartPosition.CenterParent;
            BuildUI();

            dtpTu.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            dtpDen.Value = DateTime.Today;
            LoadData();
        }

        private void BuildUI()
        {
            var lblTu = UiHelper.Lbl("Từ ngày", 20, 20, 70);
            dtpTu = new DateTimePicker { Left = 95, Top = 20, Width = 130, Format = DateTimePickerFormat.Short };
            var lblDen = UiHelper.Lbl("Đến ngày", 250, 20, 70);
            dtpDen = new DateTimePicker { Left = 325, Top = 20, Width = 130, Format = DateTimePickerFormat.Short };
            btnThongKe = new Button { Text = "Thống kê", Left = 480, Top = 18, Width = 100 };
            btnThongKe.Click += (s, e) => LoadData();

            lblMuon = NewKpi(20);
            lblQuaHan = NewKpi(210);
            lblMat = NewKpi(400);
            lblHuHong = NewKpi(590);
            lblPhiPhat = NewKpi(780);

            var lblCT = new Label { Text = "Chi tiết phiếu phạt trong khoảng", Left = 20, Top = 125, Width = 400, Font = new Font(Font, FontStyle.Bold) };
            dgvPhat = UiHelper.NewGrid(20, 150, 940, 400);

            Controls.AddRange(new Control[]
            {
                lblTu, dtpTu, lblDen, dtpDen, btnThongKe,
                lblMuon, lblQuaHan, lblMat, lblHuHong, lblPhiPhat, lblCT, dgvPhat
            });
        }

        private static Label NewKpi(int left) => new Label
        {
            Left = left, Top = 65, Width = 180, Height = 45,
            BorderStyle = BorderStyle.FixedSingle,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };

        private void LoadData()
        {
            try
            {
                DateTime tu = dtpTu.Value.Date, den = dtpDen.Value.Date;
                if (den < tu) (tu, den) = (den, tu);

                DataRow r = _repo.GetTongHop(tu, den);
                lblMuon.Text = "Lượt mượn: " + Convert.ToInt32(r["LuotMuon"]);
                lblQuaHan.Text = "Quá hạn: " + Convert.ToInt32(r["QuaHan"]);
                lblMat.Text = "Sách mất: " + Convert.ToInt32(r["Mat"]);
                lblHuHong.Text = "Hư hỏng: " + Convert.ToInt32(r["HuHong"]);
                lblPhiPhat.Text = "Tổng phạt: " + Convert.ToDecimal(r["TongPhat"]).ToString("N0") + " đ";

                dgvPhat.DataSource = _phatRepo.GetPhat(tu, den);
            }
            catch (Exception ex) { MessageBox.Show("Lỗi kết nối CSDL: " + ex.Message); }
        }
    }
}