namespace QuanLyThuVien.Forms
{
    public class frmMain : Form
    {
        public frmMain()
        {
            Text = "Quản lý thư viện";
            ClientSize = new Size(640, 450);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BuildUI();
        }

        private void BuildUI()
        {
            var lblTitle = new Label
            {
                Text = "HỆ THỐNG QUẢN LÝ THƯ VIỆN",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Left = 0, Top = 20, Width = ClientSize.Width, Height = 50
            };
            Controls.Add(lblTitle);

            var items = new (string Text, Action Run)[]
            {
                ("Người dùng",         () => Open(new frmNguoiDung())),
                ("Tài liệu",           () => Open(new frmTaiLieu())),
                ("Thẻ thư viện",       () => Open(new frmTheThuVien())),
                ("Mượn - Trả",         () => Open(new frmPhieuMuon())),
                ("Phiếu phạt",         () => Open(new frmPhieuPhat())),
                ("Yêu cầu nhập sách",  () => Open(new frmYeuCauNhapSach())),
                ("Danh mục",           () => Open(new frmDanhMuc())),
                ("Thống kê",           () => Open(new frmThongKe())),
                ("Thoát",              ConfirmExit),
            };

            for (int i = 0; i < items.Length; i++)
            {
                var item = items[i];
                var btn = new Button
                {
                    Text = item.Text,
                    Width = 260, Height = 50,
                    Left = 40 + (i % 2) * 300,
                    Top = 90 + (i / 2) * 65
                };
                btn.Click += (s, e) => item.Run();
                Controls.Add(btn);
            }
        }

        private void Open(Form f)
        {
            using (f) f.ShowDialog(this);
        }

        private void ConfirmExit()
        {
            if (UiHelper.Confirm("Bạn có thực sự muốn thoát chương trình?")) Close();
        }
    }
}