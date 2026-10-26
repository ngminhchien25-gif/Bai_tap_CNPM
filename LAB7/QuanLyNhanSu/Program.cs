using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using QuanLyNhanSu.Forms;

namespace QuanLyNhanSu
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) =>
            {
                SqlException sql = e.Exception as SqlException;
                string tb = sql != null
                    ? "Không kết nối được cơ sở dữ liệu QLNS_DongA (mã lỗi " + sql.Number + ").\n" +
                      "Kiểm tra: (1) đã chạy file QLNS_DongA.sql trên đúng SQL Server chưa; " +
                      "(2) chuỗi kết nối trong App.config (Server=. hoặc Server=.\\SQLEXPRESS).\n\nChi tiết: " + sql.Message
                    : "Đã xảy ra lỗi: " + e.Exception.Message;
                MessageBox.Show(tb, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };
            Application.Run(new FrmMain());
        }
    }
}