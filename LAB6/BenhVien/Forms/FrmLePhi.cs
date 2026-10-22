using System.Data;
using System.Drawing;
using System.Windows.Forms;
using static BenhVien.DataAccess.DatabaseHelper;

namespace BenhVien.Forms;

/// <summary>UC-05 Thu lệ phí hằng năm / gia hạn: gọi usp_GiaHanHoSo (còn hạn thì cộng dồn thêm 1 năm – BR05).</summary>
public class FrmLePhi : FrmBase
{
    readonly TextBox txtMaHS = Ui.Txt(), txtTen = Ui.Txt(readOnly: true), txtTrangThai = Ui.Txt(readOnly: true), txtHanCu = Ui.Txt(readOnly: true);
    readonly NumericUpDown numTien = Ui.Num(0, 100000000, 500000);
    readonly DateTimePicker dtpDong = Ui.Date(), dtpHetHan = Ui.Date();
    readonly DataGridView grid = Ui.Grid();
    string _maHS;

    public FrmLePhi() : base("Thu lệ phí hằng năm / Gia hạn hồ sơ", "UC-05 · Nhân viên tiếp nhận")
    {
        dtpDong.Enabled = false; dtpHetHan.Enabled = false;

        var btnTim = Ui.Btn("Kiểm tra hồ sơ", width: 150);
        btnTim.Click += (_, _) => TimHoSo();

        var g1 = Ui.Group("1. Tìm hồ sơ cần gia hạn", Ui.SearchBar("Mã hồ sơ / SĐT / họ tên:", txtMaHS, btnTim));
        var g2 = Ui.Group("2. Tình trạng hồ sơ", Ui.Fields(3, ("Bệnh nhân", txtTen), ("Hạn hiện tại", txtHanCu), ("Trạng thái", txtTrangThai)));
        var g3 = Ui.Group("3. Thu lệ phí năm mới (hiệu lực 1 năm – BR05)",
            Ui.Fields(3, ("Số tiền (VNĐ)", numTien), ("Ngày đóng", dtpDong), ("Ngày hết hạn mới", dtpHetHan)));
        Stack(Ui.Group("4. Lịch sử đóng lệ phí", grid, DockStyle.Fill), g1, g2, g3);

        var btnThu = Ui.Btn("Thu phí & gia hạn", width: 170);
        var btnBL = Ui.Btn("In biên lai", Ui.Gray, 120);
        AddActions(btnThu, btnBL);

        btnThu.Click += (_, _) =>
        {
            if (_maHS == null) { Ui.Alert("Hãy kiểm tra hồ sơ trước khi thu phí."); return; }
            if (numTien.Value <= 0) { Ui.Alert("Số tiền lệ phí phải lớn hơn 0."); return; }
            if (!Ui.TryDb(() => ExecuteProc("dbo.usp_GiaHanHoSo", P("@maHS", _maHS), PMoney("@soTienLePhi", numTien.Value), P("@maNV", Session.MaNV)))) return;
            Ui.Msg("Đã gia hạn hồ sơ " + _maHS + ".");
            TimHoSo(_maHS);
        };

        btnBL.Click += (_, _) =>
        {
            if (_maHS == null) Ui.Alert("Chưa có hồ sơ.");
            else Ui.Msg("Đang in biên lai thu lệ phí...");
        };
    }

    void TimHoSo(string key = null)
    {
        key ??= txtMaHS.Text.Trim();
        if (key == "") { Ui.Alert("Vui lòng nhập mã hồ sơ."); return; }
        Ui.TryDb(() =>
        {
            var dt = ExecuteProcTable("dbo.usp_TraCuuHoSo", P("@tuKhoa", key));
            if (dt.Rows.Count == 0) { _maHS = null; Ui.Alert("Không tìm thấy hồ sơ."); return; }
            if (dt.Rows.Count > 1) { _maHS = null; Ui.Alert("Có " + dt.Rows.Count + " kết quả, hãy nhập chính xác mã hồ sơ."); return; }

            var r = dt.Rows[0];
            _maHS = Ui.S(r["maHS"]);
            txtMaHS.Text = _maHS;
            txtTen.Text = Ui.S(r["hoTen"]);
            var han = (DateTime)r["ngayHetHan"];
            txtHanCu.Text = han.ToString("dd/MM/yyyy");
            bool conHan = Ui.S(r["hieuLuc"]) == "Hiệu lực";
            txtTrangThai.Text = conHan ? "Còn hiệu lực" : "Hết hạn – cần gia hạn";
            txtTrangThai.ForeColor = conHan ? Ui.Primary : Ui.Red;

            dtpDong.Value = DateTime.Today;
            dtpHetHan.Value = (han >= DateTime.Today ? han : DateTime.Today).AddYears(1);   // xem trước, DB tính lại khi lưu

            Ui.Bind(grid, ExecuteQuery(@"SELECT maLePhi, namApDung, soTien, ngayDong, ngayHetHan
                                         FROM dbo.LePhi WHERE maHS = @m ORDER BY ngayDong DESC, maLePhi DESC", P("@m", _maHS)),
                "Mã lệ phí", "Năm áp dụng", "Số tiền (VNĐ)", "Ngày đóng", "Ngày hết hạn");
        });
    }
}
