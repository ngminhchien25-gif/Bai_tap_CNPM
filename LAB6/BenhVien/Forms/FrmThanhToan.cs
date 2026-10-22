using System.Data;
using System.Drawing;
using System.Windows.Forms;
using static BenhVien.DataAccess.DatabaseHelper;

namespace BenhVien.Forms;

/// <summary>UC-17, UC-18 Thanh toán viện phí: usp_ThanhToanVienPhi (BR03: khám định kỳ miễn phí; BR04: ghi tạm đến khi thanh toán).</summary>
public class FrmThanhToan : FrmBase
{
    readonly TextBox txtMaPK = Ui.Txt(), txtBN = Ui.Txt(readOnly: true), txtHS = Ui.Txt(readOnly: true), txtKhoa = Ui.Txt(readOnly: true), txtBS = Ui.Txt(readOnly: true);
    readonly TextBox txtLoai = Ui.Txt(readOnly: true), txtTrangThai = Ui.Txt(readOnly: true), txtTong = Ui.Txt(readOnly: true), txtThua = Ui.Txt(readOnly: true);
    readonly ComboBox cboHT = Ui.Cbo(SampleData.HinhThucTT);
    readonly NumericUpDown numPhiKhac = Ui.Num(0, 1000000000, 0);
    readonly NumericUpDown numDua = Ui.Num(0, 1000000000, 0);
    readonly DataGridView grid = Ui.Grid();
    string _maPK;
    bool _dinhKy, _paid;
    decimal _tongXN;

    public FrmThanhToan() : base("Thanh toán viện phí & xuất viện", "UC-17, UC-18 · Nhân viên thu ngân", 1050, 780)
    {
        txtTrangThai.Text = "—"; txtTong.Text = "0"; txtThua.Text = "0";

        var btnTim = Ui.Btn("Tìm phiếu khám", width: 160);
        btnTim.Click += (_, _) => TimPhieu();

        numPhiKhac.ValueChanged += (_, _) => Recalc();
        numDua.ValueChanged += (_, _) => Recalc();

        var g1 = Ui.Group("1. Tìm phiếu khám cần thanh toán", Ui.SearchBar("Mã phiếu khám:", txtMaPK, btnTim));
        var g2 = Ui.Group("Thông tin lần khám",
            Ui.Fields(3, ("Bệnh nhân", txtBN), ("Mã hồ sơ", txtHS), ("Khoa", txtKhoa),
                         ("Bác sĩ", txtBS), ("Loại khám", txtLoai), ("Hóa đơn", txtTrangThai)));
        var bottom = Ui.Group("2. Thanh toán (BR03: khám sức khỏe định kỳ miễn viện phí)",
            Ui.Fields(2, ("Phí khám & thuốc (VNĐ)", numPhiKhac), ("Hình thức thanh toán", cboHT),
                         ("Tổng tiền (VNĐ)", txtTong), ("Tiền khách đưa", numDua), ("Tiền thừa", txtThua)));

        Stack(Ui.Group("Chi phí xét nghiệm đã chỉ định", grid, DockStyle.Fill), g1, g2);
        Content.Controls.Add(bottom);
        bottom.Dock = DockStyle.Bottom;

        var btnTT = Ui.Btn("Thanh toán & xuất viện", Ui.Orange, 230);
        var btnIn = Ui.Btn("In hóa đơn", Ui.Gray, 130);
        AddActions(btnTT, btnIn);

        btnTT.Click += (_, _) =>
        {
            if (_maPK == null) { Ui.Alert("Hãy tìm phiếu khám trước."); return; }
            if (_paid) { Ui.Alert("Phiếu này đã thanh toán."); return; }
            var tong = Tong();
            if (tong > 0 && cboHT.Text == "Tiền mặt" && numDua.Value < tong)
            {
                Ui.Alert("Tiền khách đưa chưa đủ để thanh toán.");
                return;
            }
            var pOut = POut("@maHD", SqlDbType.VarChar, 12);
            if (!Ui.TryDb(() => ExecuteProc("dbo.usp_ThanhToanVienPhi",
                    P("@maPK", _maPK), P("@maNV", Session.MaNV), PMoney("@tongTien", tong),
                    P("@hinhThucTT", _dinhKy ? "Miễn phí (định kỳ)" : cboHT.Text), pOut))) return;
            _paid = true;
            txtTrangThai.Text = "Đã thanh toán (" + pOut.Value + ")"; txtTrangThai.ForeColor = Ui.Primary;
            Ui.Msg("Thanh toán thành công " + tong.ToString("N0") + " VNĐ. Bệnh nhân hoàn tất xuất viện.\nChuyển phòng CSKH ghi nhận phiếu khám vào hồ sơ (BR04).");
        };

        btnIn.Click += (_, _) =>
        {
            if (!_paid) Ui.Alert("Chỉ in hóa đơn sau khi đã thanh toán.");
            else Ui.Msg("Đang in hóa đơn viện phí cho phiếu " + _maPK + "...");
        };
    }

    void TimPhieu()
    {
        var key = txtMaPK.Text.Trim();
        if (key == "") { Ui.Alert("Vui lòng nhập mã phiếu khám."); return; }
        Ui.TryDb(() =>
        {
            var h = ExecuteQuery(@"
                SELECT pk.maPK, pk.maHS, bn.hoTen, k.tenKhoa, bs.hoTen AS bacSi, pk.loaiKham, pk.trangThai, hd.maHD, hd.trangThai AS ttHD
                FROM dbo.PhieuKhamBenh pk
                JOIN dbo.HoSoBenhAn hs ON hs.maHS = pk.maHS JOIN dbo.BenhNhan bn ON bn.maBN = hs.maBN
                JOIN dbo.Khoa k ON k.maKhoa = pk.maKhoa JOIN dbo.BacSi bs ON bs.maBS = pk.maBS
                LEFT JOIN dbo.HoaDonVienPhi hd ON hd.maPK = pk.maPK
                WHERE pk.maPK = @m", P("@m", key));
            if (h.Rows.Count == 0) { _maPK = null; Ui.Alert("Không tìm thấy phiếu khám."); return; }

            var r = h.Rows[0];
            _maPK = Ui.S(r["maPK"]);
            txtMaPK.Text = _maPK;
            txtBN.Text = Ui.S(r["hoTen"]); txtHS.Text = Ui.S(r["maHS"]);
            txtKhoa.Text = Ui.S(r["tenKhoa"]); txtBS.Text = Ui.S(r["bacSi"]); txtLoai.Text = Ui.S(r["loaiKham"]);
            _dinhKy = Ui.S(r["loaiKham"]) == "Khám sức khỏe định kỳ";
            _paid = !(r["maHD"] is DBNull);
            txtTrangThai.Text = _paid ? "Đã thanh toán (" + r["maHD"] + ")" : "Chưa thanh toán";
            txtTrangThai.ForeColor = _paid ? Ui.Primary : Ui.Red;
            numPhiKhac.Enabled = cboHT.Enabled = numDua.Enabled = !_dinhKy;
            if (_dinhKy) { numPhiKhac.Value = 0; numDua.Value = 0; }

            var xn = ExecuteQuery(@"
                SELECT dm.tenXN, dm.donGia FROM dbo.XetNghiem x
                JOIN dbo.DanhMucXetNghiem dm ON dm.maDM = x.maDM WHERE x.maPK = @m", P("@m", _maPK));
            Ui.Bind(grid, xn, "Xét nghiệm", "Đơn giá (VNĐ)");
            _tongXN = xn.Rows.Cast<DataRow>().Sum(x => Convert.ToDecimal(x["donGia"]));
            Recalc();

            if (_paid) Ui.Msg("Phiếu này đã được thanh toán.");
            else if (Ui.S(r["trangThai"]) != "Ghi tạm") Ui.Alert("Phiếu này đã ghi nhận vào hồ sơ.");
        });
    }

    decimal Tong() => _dinhKy ? 0 : _tongXN + numPhiKhac.Value;

    void Recalc()
    {
        var t = Tong();
        txtTong.Text = t.ToString("N0");
        var thua = numDua.Value - t;
        txtThua.Text = (thua > 0 ? thua : 0).ToString("N0");
    }
}
