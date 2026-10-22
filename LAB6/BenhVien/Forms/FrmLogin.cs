using System.Drawing;
using System.Windows.Forms;

namespace BenhVien.Forms;

/// <summary>UC-01 Đăng nhập: xác thực với bảng TaiKhoan, vai trò lấy từ CSDL.</summary>
public class FrmLogin : Form
{
    public string VaiTro { get; private set; } = "";

    readonly TextBox txtUser = Ui.Txt();
    readonly TextBox txtPass = Ui.Txt(password: true);

    public FrmLogin()
    {
        Text = "Đăng nhập – Hệ thống quản lý hồ sơ bệnh án";
        ClientSize = new Size(760, 420);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false; MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        Font = Ui.Normal; BackColor = Color.White;

        var left = new Panel { Dock = DockStyle.Left, Width = 300, BackColor = Ui.Primary };
        left.Controls.Add(new Label { Text = "🏥", Font = new Font("Segoe UI Emoji", 54F), ForeColor = Color.White, AutoSize = true, Location = new Point(80, 70) });
        left.Controls.Add(new Label { Text = "BỆNH VIỆN ĐA KHOA\nQUỐC TẾ", Font = new Font("Segoe UI", 15F, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(40, 210) });
        left.Controls.Add(new Label { Text = "Hệ thống quản lý hồ sơ bệnh án điện tử", Font = new Font("Segoe UI", 9.5F), ForeColor = Color.FromArgb(178, 223, 219), AutoSize = true, Location = new Point(40, 275) });

        var title = new Label { Text = "ĐĂNG NHẬP", Font = Ui.H1, ForeColor = Ui.Primary, AutoSize = true, Location = new Point(340, 50) };
        var lblUser = Ui.Lbl("Tên đăng nhập", true); lblUser.Location = new Point(340, 120);
        txtUser.Location = new Point(340, 148); txtUser.Width = 380;
        var lblPass = Ui.Lbl("Mật khẩu", true); lblPass.Location = new Point(340, 196);
        txtPass.Location = new Point(340, 224); txtPass.Width = 380;
        var hint = new Label
        {
            Text = "Tài khoản mẫu: xem file Database/02_TaiKhoan_Mau.sql",
            Font = new Font("Segoe UI", 9F), ForeColor = Ui.Gray, AutoSize = true, Location = new Point(340, 266)
        };

        var btnLogin = Ui.Btn("Đăng nhập", width: 180); btnLogin.Location = new Point(340, 336);
        var btnExit = Ui.Btn("Thoát", Ui.Gray, 180); btnExit.Location = new Point(540, 336);
        btnExit.DialogResult = DialogResult.Cancel;

        btnLogin.Click += (_, _) =>
        {
            if (txtUser.Text.Trim() == "" || txtPass.Text == "")
            {
                Ui.Alert("Vui lòng nhập tên đăng nhập và mật khẩu.");
                return;
            }
            bool ok = false;
            if (!Ui.TryDb(() => ok = Auth.Login(txtUser.Text, txtPass.Text))) return;
            if (!ok) { Ui.Alert("Sai tên đăng nhập hoặc mật khẩu."); return; }
            VaiTro = Session.VaiTro;
            DialogResult = DialogResult.OK;
        };

        AcceptButton = btnLogin;
        CancelButton = btnExit;
        Controls.AddRange(new Control[] { left, title, lblUser, txtUser, lblPass, txtPass, hint, btnLogin, btnExit });
    }
}
