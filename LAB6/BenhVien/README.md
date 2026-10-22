# Lab06 – Hệ thống quản lý hồ sơ bệnh án (WinForms, C# / .NET 9)

Giao diện WinForms cho bài Lab06, viết bằng **Visual Studio Code**. Mỗi form bám theo use case (UC-xx) trong báo cáo.
Ứng dụng nối với SQL Server (CSDL `QLHoSoBenhAn`) qua `DataAccess/DatabaseHelper.cs` và gọi các stored procedure `usp_*`.

## Yêu cầu
- Windows 10/11 (WinForms chỉ chạy trên Windows)
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- VS Code + extension **C# Dev Kit** (hoặc **C#**)

## Chuẩn bị CSDL
1. Chạy script tạo CSDL `QLHoSoBenhAn` (bảng, stored procedure, dữ liệu mẫu) trong SSMS / Azure Data Studio.
2. Chạy `Database/02_TaiKhoan_Mau.sql` để tạo tài khoản đăng nhập.
3. Nếu SQL Server không phải instance mặc định, sửa `ConnectionString` trong `DataAccess/DatabaseHelper.cs`
   (ví dụ `Server=.\SQLEXPRESS;...`).

Tài khoản mẫu (mật khẩu `123456`, riêng `admin` là `admin123`):
`tiepnhan`, `cskh`, `thungan`, `xetnghiem`, `bs.leminh`, `bs.tranha`, `admin`.

## Chạy
```bash
cd BenhVien
dotnet run
```
Hoặc mở thư mục `BenhVien` trong VS Code → nhấn **F5** (đã có sẵn `.vscode/launch.json`, `tasks.json`).

Vai trò lấy từ bảng `TaiKhoan` khi đăng nhập; trang chính chỉ mở các ô chức năng đúng vai trò.
Mật khẩu lưu SHA-512 của `tendangnhap:matkhau` (xem `Auth.cs`).

## Cấu trúc
| File | Vai trò |
|---|---|
| `Program.cs` | Điểm vào: Đăng nhập → Trang chính (Đăng xuất quay lại đăng nhập) |
| `Ui.cs` | Hàm dựng control dùng chung (nút, lưới nhập liệu, bảng, tab...) |
| `Forms/FrmBase.cs` | Form nền: thanh tiêu đề, vùng nội dung, thanh nút + nút Đóng |
| `Forms/FrmLogin.cs` | UC-01 Đăng nhập |
| `Forms/FrmMain.cs` | **Trang chính** – các ô chức năng dẫn đến form khác |
| `Forms/FrmBenhNhan.cs` | UC-02 Quản lý bệnh nhân |
| `Forms/FrmDangKyKham.cs` | UC-03 Đăng ký khám bệnh |
| `Forms/FrmCapHoSo.cs` | UC-04 Lập phiếu đăng ký & cấp hồ sơ |
| `Forms/FrmLePhi.cs` | UC-05 Thu lệ phí / gia hạn |
| `Forms/FrmTraCuu.cs` | UC-06 Tra cứu hồ sơ bệnh án |
| `Forms/FrmLichHen.cs` | UC-07, UC-08 Đặt & quản lý lịch hẹn |
| `Forms/FrmPhieuKham.cs` | UC-09, 10, 11, 13 Phiếu khám, chỉ định & xem xét nghiệm |
| `Forms/FrmXetNghiem.cs` | UC-12 Thực hiện xét nghiệm |
| `Forms/FrmToaThuoc.cs` | UC-14, UC-15 Lập toa thuốc, phản ứng phụ |
| `Forms/FrmGhiNhanHoSo.cs` | UC-16 Ghi nhận phiếu khám vào hồ sơ |
| `Forms/FrmThanhToan.cs` | UC-17, UC-18 Thanh toán viện phí, in hóa đơn |
| `Forms/FrmDanhMuc.cs` | UC-19 → UC-22 Danh mục, tài khoản, phân quyền |
| `Forms/FrmThongKe.cs` | UC-23 Thống kê – báo cáo |

## Stored procedure được gọi
| Form | Proc |
|---|---|
| Cấp hồ sơ | `usp_DangKyLanDau` |
| Thu lệ phí / gia hạn | `usp_GiaHanHoSo` |
| Tra cứu, đăng ký khám, lịch hẹn | `usp_TraCuuHoSo`, `usp_LayLichSuKham`, `usp_TaoPhieuKham`, `usp_DatLichDinhKy` |
| Phiếu khám, xét nghiệm | `usp_ChiDinhXetNghiem`, `usp_NhapKetQuaXetNghiem` |
| Toa thuốc | `usp_LapToaThuoc` (tham số bảng `dbo.ChiTietToaType`) |
| Thanh toán, ghi nhận hồ sơ | `usp_ThanhToanVienPhi`, `usp_GhiNhanVaoHoSo` |

Các proc kiểm tra `chucVu` của nhân viên, nên phải đăng nhập đúng vai trò mới thao tác được.

## Luồng thử nghiệm nhanh
1. `tiepnhan`: Lập phiếu đăng ký & cấp hồ sơ → Đăng ký khám (chọn khoa, bác sĩ → tạo phiếu khám).
2. `bs.leminh`: Phiếu khám → thêm triệu chứng, chỉ định xét nghiệm.
3. `xetnghiem`: nhập kết quả xét nghiệm.
4. `bs.leminh`: Lập toa thuốc → lưu toa.
5. `thungan`: Thanh toán viện phí. 6. `cskh`: Ghi nhận phiếu khám vào hồ sơ.

> VS Code không có trình kéo-thả (Designer) cho WinForms, nên toàn bộ giao diện được dựng bằng code.
