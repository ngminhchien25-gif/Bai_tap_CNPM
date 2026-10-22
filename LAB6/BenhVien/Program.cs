using BenhVien.Forms;

namespace BenhVien;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        while (true)
        {
            string role;
            using (var login = new FrmLogin())
            {
                if (login.ShowDialog() != DialogResult.OK) return;
                role = login.VaiTro;
            }

            var main = new FrmMain(role);
            Application.Run(main);
            if (!main.LoggedOut) return;   // đóng cửa sổ chính = thoát; "Đăng xuất" = quay lại đăng nhập
        }
    }
}
