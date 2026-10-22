using System.Drawing;
using System.Windows.Forms;

namespace BenhVien.Forms;

/// <summary>Trang chính: các ô chức năng (tile) dẫn đến từng form, bật/tắt theo vai trò đăng nhập.</summary>
public class FrmMain : Form
{
    readonly string _role;
    readonly ToolTip _tip = new();
    readonly FlowLayoutPanel _flow = new()
    {
        Dock = DockStyle.Fill, AutoScroll = true,
        Padding = new Padding(24, 12, 24, 12), BackColor = Ui.Back
    };
    Control _last;

    public bool LoggedOut { get; private set; }

    public FrmMain(string role)
    {
        _role = role;
        Text = "Hệ thống quản lý hồ sơ bệnh án – Bệnh viện đa khoa quốc tế";
        Font = Ui.Normal; BackColor = Ui.Back;
        MinimumSize = new Size(1000, 640);
        WindowState = FormWindowState.Maximized;
        StartPosition = FormStartPosition.CenterScreen;

        // ----- Thanh tiêu đề -----
        var header = new Panel { Dock = DockStyle.Top, Height = 84, BackColor = Ui.Primary };
        header.Controls.Add(new Label { Text = "BỆNH VIỆN ĐA KHOA QUỐC TẾ", Font = new Font("Segoe UI", 18F, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(24, 12) });
        header.Controls.Add(new Label { Text = "Hệ thống quản lý hồ sơ bệnh án điện tử – chọn chức năng bên dưới để bắt đầu", Font = new Font("Segoe UI", 10F), ForeColor = Color.FromArgb(178, 223, 219), AutoSize = true, Location = new Point(26, 50) });

        var right = new FlowLayoutPanel { Dock = DockStyle.Right, Width = 400, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(8, 22, 18, 0) };
        var btnOut = Ui.Btn("Đăng xuất", Ui.PrimaryDark, 110);
        btnOut.Click += (_, _) => { LoggedOut = true; Close(); };
        right.Controls.Add(btnOut);
        right.Controls.Add(new Label { Text = "👤 " + Session.HoTen + " – " + role, ForeColor = Color.White, Font = Ui.Bold, AutoSize = true, Margin = new Padding(0, 10, 12, 0) });
        header.Controls.Add(right);

        // ----- Thanh trạng thái -----
        var status = new StatusStrip();
        status.Items.Add(new ToolStripStatusLabel("Vai trò: " + role));
        status.Items.Add(new ToolStripStatusLabel("|"));
        status.Items.Add(new ToolStripStatusLabel(DateTime.Now.ToString("dd/MM/yyyy")));

        // ----- Các ô chức năng (mã UC lấy từ Câu 3 của báo cáo) -----
        AddSection("TIẾP NHẬN BỆNH NHÂN");
        AddTile("👤", "Quản lý bệnh nhân", "UC-02", () => new FrmBenhNhan(), Roles.TiepNhan);
        AddTile("📝", "Lập phiếu đăng ký & cấp hồ sơ", "UC-04", () => new FrmCapHoSo(), Roles.TiepNhan);
        AddTile("💳", "Thu lệ phí / Gia hạn hồ sơ", "UC-05", () => new FrmLePhi(), Roles.TiepNhan);
        AddTile("🔍", "Tra cứu hồ sơ bệnh án", "UC-06", () => new FrmTraCuu(), Roles.TiepNhan);
        AddTile("🏥", "Đăng ký khám bệnh", "UC-03", () => new FrmDangKyKham(), Roles.TiepNhan);

        AddSection("LỊCH HẸN KHÁM ĐỊNH KỲ");
        AddTile("📅", "Đặt & quản lý lịch hẹn", "UC-07, UC-08", () => new FrmLichHen(), Roles.CSKH);
        AddTile("📂", "Ghi nhận phiếu khám vào hồ sơ", "UC-16", () => new FrmGhiNhanHoSo(), Roles.CSKH);

        AddSection("KHÁM & ĐIỀU TRỊ");
        AddTile("🩺", "Phiếu khám bệnh", "UC-09, 10, 11, 13", () => new FrmPhieuKham(), Roles.BacSi);
        AddTile("💊", "Lập toa thuốc", "UC-14, UC-15", () => new FrmToaThuoc(), Roles.BacSi);
        AddTile("🧪", "Thực hiện xét nghiệm", "UC-12", () => new FrmXetNghiem(), Roles.XetNghiem);

        AddSection("THANH TOÁN");
        AddTile("🧾", "Thanh toán viện phí & xuất viện", "UC-17, UC-18", () => new FrmThanhToan(), Roles.ThuNgan);

        AddSection("QUẢN TRỊ HỆ THỐNG");
        AddTile("🗂", "Danh mục & tài khoản", "UC-19 → UC-22", () => new FrmDanhMuc(), Roles.QuanTri);
        AddTile("📊", "Thống kê – báo cáo", "UC-23", () => new FrmThongKe(), Roles.QuanTri);

        Controls.Add(_flow);      // Fill thêm trước
        Controls.Add(status);
        Controls.Add(header);
    }

    void AddSection(string title)
    {
        if (_last != null) _flow.SetFlowBreak(_last, true);
        var l = new Label
        {
            Text = title, Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = Ui.PrimaryDark,
            AutoSize = true, Margin = new Padding(6, 14, 0, 4)
        };
        _flow.Controls.Add(l);
        _flow.SetFlowBreak(l, true);
        _last = l;
    }

    void AddTile(string icon, string title, string uc, Func<Form> open, params string[] roles)
    {
        bool ok = roles.Contains(_role);
        var b = new Button
        {
            Width = 270, Height = 96, FlatStyle = FlatStyle.Flat,
            BackColor = ok ? Color.White : Color.FromArgb(232, 236, 238),
            ForeColor = ok ? Ui.TextColor : Color.Gray,
            Font = Ui.Bold, TextAlign = ContentAlignment.MiddleLeft,
            Text = icon + "  " + title + "\n" + uc,
            Padding = new Padding(12, 0, 0, 0), Margin = new Padding(6),
            Cursor = ok ? Cursors.Hand : Cursors.Default, UseVisualStyleBackColor = false
        };
        b.FlatAppearance.BorderSize = 1;
        b.FlatAppearance.BorderColor = ok ? Ui.Primary : Color.Silver;
        b.FlatAppearance.MouseOverBackColor = Color.FromArgb(224, 242, 241);

        if (ok)
        {
            _tip.SetToolTip(b, "Mở: " + title);
            b.Click += (_, _) =>
            {
                using var f = open();
                f.ShowDialog(this);
            };
        }
        else
        {
            b.Click += (_, _) => Ui.Alert("Vai trò \"" + _role + "\" không có quyền dùng chức năng này.");
        }

        _flow.Controls.Add(b);
        _last = b;
    }
}
