# Hệ Thống Quản Lý Công Ty Du Lịch

**Lab 5 - Bài tập Công nghệ phần mềm**

Ứng dụng WinForms nội bộ cho nhân viên công ty du lịch Văn Hóa Việt, hỗ trợ trọn quy trình từ mở tour, đăng ký vé, phân công hướng dẫn viên, kết thúc tour, khảo sát đến tính lương. Khách hàng xem tour trên website và không đăng nhập vào hệ thống này; mọi thao tác do nhân viên nghiệp vụ thực hiện.

## Công nghệ

- C# WinForms, .NET Framework 4.7.2
- SQL Server (`Microsoft.Data.SqlClient`), viết SQL thuần, tham số hóa bằng `SqlParameter`, không dùng ORM

## Chức năng chính

- **Danh mục:** phương tiện, điểm bán vé, hướng dẫn viên, địa điểm tham quan
- **Tour & Hành trình:** cấu hình tour, điểm dừng chân, phương tiện từng chặng
- **Lịch chuyến & Đăng ký lẻ:** mở lịch chuyến, đăng ký khách lẻ (dưới 12 người, thanh toán ngay)
- **Đăng ký đoàn:** lập phiếu đoàn (trên 12 người, đặt cọc), bảo hiểm, hủy phiếu
- **Phân công HDV:** phân công theo chuyến lẻ/đoàn, kiểm tra trùng lịch
- **Kết thúc tour & Khảo sát:** thanh toán kinh phí còn lại, thu thập đánh giá
- **Lương & Thống kê:** tính lương HDV theo tháng, thống kê số lượng và doanh thu

## Cài đặt & chạy

1. Chạy `Database/QuanLyCongTyDuLich.sql` trên SQL Server hoặc LocalDB (đã có sẵn dữ liệu mẫu).
2. Sửa chuỗi kết nối `QuanLyCongTyDuLich` trong `App.config` cho khớp với máy.
3. Mở solution bằng Visual Studio 2022, build và chạy. `FrmMain` là trang chủ điều hướng.

## Ghi chú

Đây là bài làm phục vụ môn học, chưa có phân quyền người dùng, chưa audit bảo mật và chưa có unit test.
