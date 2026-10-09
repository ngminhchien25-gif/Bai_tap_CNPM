using QuanLyCongTyDuLich.Forms;
using QuanLyThuVien.Forms;

namespace QuanLyThuVien
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new FrmMain());
        }
    }
}