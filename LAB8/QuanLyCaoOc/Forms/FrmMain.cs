using System;
using System.Drawing;
using System.Windows.Forms;
using QuanLyCaoOc.Services;

namespace QuanLyCaoOc.Forms
{
    public class FrmMain : Form
    {
        public FrmMain()
        {
            Text = "Quản lý cho thuê văn phòng cao ốc";
            ClientSize = new Size(520, 470);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Font = Ui.FontChu;

            var lblTitle = new Label
            {
                Name = "lblTitle", Text = "QUẢN LÝ CHO THUÊ VĂN PHÒNG CAO ỐC", Dock = DockStyle.Top, Height = 70,
                TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 14f, FontStyle.Bold), ForeColor = Color.DarkBlue
            };
            Controls.Add(lblTitle);

            int y = 85;
            Mo("btnDanhMuc", "1. Danh mục (tầng, phòng, khách hàng, dịch vụ)", ref y, () => new FrmDanhMuc());
            Mo("btnHopDong", "2. Lập hợp đồng thuê văn phòng", ref y, () => new FrmHopDong());
            Mo("btnThanhToan", "3. Thanh toán tiền thuê và dịch vụ", ref y, () => new FrmThanhToan());
            Mo("btnGiaHan", "4. Lập phiếu gia hạn", ref y, () => new FrmGiaHan());
            Mo("btnNhanVien", "5. Nhân viên công ty thuê và thẻ giữ xe", ref y, () => new FrmNhanVien());
            Mo("btnKiemTra", "6. Kiểm tra phòng định kỳ và xử phạt", ref y, () => new FrmKiemTra());
            Ui.Btn(this, "btnThoat", "Thoát", 60, y + 5, 400, (o, ev) => { if (Ui.Confirm("Thoát chương trình?")) Close(); });

            // Cập nhật hợp đồng hết hạn và trả phòng về trống khi mở ứng dụng.
            Load += (o, ev) => Ui.Run(HopDongService.CapNhatHetHan);
        }

        private void Mo(string name, string text, ref int y, Func<Form> tao)
        {
            Button b = Ui.Btn(this, name, text, 60, y, 400, (o, ev) =>
            {
                using (Form f = tao()) { f.ShowDialog(this); }
            });
            b.Height = 40;
            y += 50;
        }
    }
}
