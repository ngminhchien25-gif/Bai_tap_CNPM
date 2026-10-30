using System;
using System.Windows.Forms;
using QuanLyCaoOc.Forms;

namespace QuanLyCaoOc
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FrmMain());
        }
    }
}
