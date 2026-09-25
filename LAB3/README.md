# Hệ Thống Quản Lý Khách Sạn

Bài lab 3 môn Phân tích thiết kế hệ thống tại HCMUNRE. Ứng dụng WinForms nội bộ cho nhân viên khách sạn: đặt phòng, ghi nhận dịch vụ, xử lý hư hỏng, xuất hóa đơn. Khách không đăng nhập vào hệ thống — mọi thao tác đều do lễ tân hoặc nhân viên nhập giúp.

Không phải sản phẩm thương mại, chỉ là một khách sạn quy mô nhỏ được mô hình hóa đủ để chạy được hết vòng đời từ lúc khách đặt phòng tới lúc trả phòng và thanh toán.

## Công nghệ

C# WinForms trên .NET Framework, kết nối SQL Server qua `Microsoft.Data.SqlClient`. Không dùng Entity Framework hay ORM nào — mọi câu lệnh SQL viết tay trong tầng Repository, tham số hóa để tránh SQL injection. Giao diện dạng MDI: một `Frmmain` chứa menu, các form nghiệp vụ mở như cửa sổ con bên trong.

## Cấu trúc thư mục

```
QuanLyKhachSanUI/
├── Database/
│   └── QuanLyKhachSan.sql       
├── Repositories/
│   ├── PhongRepository.cs
│   ├── DenBuRepository.cs
│   ├── HoaDonThanhToanRepository.cs
│   ├── ThongKeRepository.cs
│   └── ...
├── Forms/
│   ├── Frmmain.cs                
│   ├── frmPhong.cs
│   ├── frmDenBu.cs
│   ├── frmHoaDonThanhToan.cs
│   ├── frmThongKe.cs
│   └── ...
└── DatabaseHelper.cs
```

## Cơ sở dữ liệu

18 bảng, khóa chính/khóa ngoại đầy đủ, một số ràng buộc CHECK và UNIQUE quan trọng:

- `TienNghi` có `UNIQUE(MaLoaiTN, SoThuTu)` — không trùng số thứ tự trong cùng loại tiện nghi.
- `PhieuLapDat` có `UNIQUE(MaTienNghi, NgayLap)` — một thiết bị chỉ lắp cho một phòng trong một ngày.
- `PhieuSuDungDV` có `UNIQUE(SoPhieuDat, SoPhong, NgaySuDung)` — dịch vụ dùng nhiều lần trong ngày được cộng dồn vào cùng một phiếu thay vì tạo phiếu mới.
- `HoaDon.TongTien` là cột tính toán (`TienPhong + TienDichVu`), không lưu trực tiếp.
- `ThanhToan.HinhThuc` chỉ nhận 4 giá trị: Tiền mặt, Chuyển khoản, Thẻ, Ví điện tử. Combobox trong form phải khớp chính xác chuỗi này, kể cả dấu.

File script nằm ở `Database/QuanLyKhachSan.sql`, chạy trực tiếp trên SQL Server hoặc LocalDB là xong, có sẵn vài dòng dữ liệu mẫu để test nhanh.

## Cài đặt

1. Chạy `Database/QuanLyKhachSan.sql` trên SQL Server Management Studio hoặc LocalDB.
2. Mở `App.config`, sửa `connectionString` cho khớp với server của máy bạn.
3. Build solution bằng Visual Studio 2022, target .NET Framework 4.7.2.
4. Chạy `Frmmain` — form chính tự mở với menu, các form còn lại mở qua đó.

Không cần cấu hình gì thêm. Nếu connection string sai, form sẽ báo lỗi ngay khi bấm nút load dữ liệu đầu tiên chứ không crash lúc khởi động.

## Các module

**Hệ thống** — quản lý nhân viên, quản lý khách hàng.

**Danh mục** — phòng và khu vực, dịch vụ. Đây là dữ liệu nền, các form nghiệp vụ phía sau đều tham chiếu tới.

**Nghiệp vụ** là phần nặng nhất: đặt phòng (kiểm tra trùng lịch, sức chứa), sử dụng dịch vụ (cộng dồn theo ngày), xử lý hư hỏng/đền bù (tự tra mức đền bù từ bảng `QuyDinhDenBu` theo loại tiện nghi và mức độ thiệt hại thay vì để nhân viên gõ tay), hóa đơn và thanh toán (tự tính tiền phòng/dịch vụ, cho phép nhiều lần thanh toán trên một hóa đơn, chỉ cho trả phòng khi hóa đơn đã thanh toán đủ).

**Thống kê** — tổng hợp số phiếu đặt, số phòng đang ở, doanh thu hóa đơn, tổng tiền đền bù theo khoảng ngày; kèm bảng thống kê dịch vụ sử dụng.

## Một vài quyết định thiết kế đáng nói

Số ngày tính tiền khi lập hóa đơn do nhân viên xác nhận thủ công, không tự động làm tròn theo giờ nhận/trả — đề bài gốc không quy định cách làm tròn nên để mở cho nhân viên quyết định tại thời điểm lập hóa đơn.

Mức đền bù không phải con số cố định của khách sạn mà lấy từ bảng cấu hình `QuyDinhDenBu`, đổi được mà không cần sửa code. Ngược lại, nếu ai đó xóa hết dữ liệu bảng này, form đền bù sẽ trả về 0 và cảnh báo, chứ không tự bịa ra số tiền.

Một hóa đơn có thể có nhiều giao dịch thanh toán — khách trả một phần bằng tiền mặt, phần còn lại chuyển khoản chẳng hạn. Đây là phần mở rộng so với yêu cầu gốc, đề chỉ nói "hỗ trợ nhiều phương thức" chứ không nói rõ có cho chia nhỏ hay không.

## Hạn chế hiện tại

ComboBox chọn nhân viên lễ tân, nhân viên phục vụ phòng... không lọc theo `VaiTro`, nên về lý thuyết một nhân viên thanh toán vẫn có thể được chọn làm lễ tân lập phiếu đặt. DB không chặn việc này, code cũng chưa chặn.

Trạng thái phòng cập nhật thủ công qua từng bước nghiệp vụ (đặt → đang ở → trả), chưa có job nào tự động dọn các phiếu đặt quá hạn chưa nhận phòng thành "No-show".

## Ghi chú

Làm cho môn học, không audit bảo mật kỹ, cũng chưa viết unit test. Nếu định dùng cho việc thật thì ít nhất phải thêm xác thực người dùng trước đã — hiện tại ai mở app cũng thao tác được hết mọi form.
