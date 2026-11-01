using QuanLyKhachSanUI.Forms;

namespace QuanLyKhachSanUI
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new Frmmain());
        }
    }
}