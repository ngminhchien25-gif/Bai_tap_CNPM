# Hệ Thống Quản Lý Công Ty Du Lịch

Bài lab 5[cite: 75, 148]. Ứng dụng WinForms nội bộ cho nhân viên công ty du lịch Văn Hóa Việt: quản lý tour và hành trình, mở lịch chuyến khách lẻ, lập phiếu đăng ký đoàn, phân công hướng dẫn viên, thanh toán kinh phí sau tour, khảo sát ý kiến và tính lương[cite: 76, 77]. Khách hàng xem tour trên website, không đăng nhập vào hệ thống quản lý — mọi thao tác đều do nhân viên nghiệp vụ (bán vé, kinh doanh, điều hành, kế toán, CSKH) thực hiện[cite: 77, 104].

Không phải sản phẩm thương mại, đây là một công ty du lịch được mô hình hóa đủ để chạy hết quy trình từ lúc mở tour, đăng ký vé, đi tour cho đến khi kết thúc, khảo sát và quyết toán lương[cite: 76, 97].

## Công nghệ

C# WinForms trên .NET Framework 4.7.2, kết nối SQL Server qua `Microsoft.Data.SqlClient`[cite: 76, 120]. Không dùng Entity Framework hay ORM — mọi câu lệnh SQL được viết tay trong lớp `Db.cs` (`Data/Db.cs`), tham số hóa bằng `SqlParameter` để tránh SQL injection[cite: 78, 120, 121]. Giao diện gồm `FrmMain` đóng vai trò trang chủ điều hướng, mở các form con dạng modal dialog (`ShowDialog()`)[cite: 103, 104, 132].

## Cấu trúc thư mục

QuanLyCongTyDuLich/
├── Database/
│   └── QuanLyCongTyDuLich.sql
├── Data/
│   └── Db.cs
├── Services/
│   ├── Models.cs
│   ├── DanhMucService.cs
│   ├── TourService.cs
│   ├── ChuyenLeService.cs
│   ├── DangKyLeService.cs
│   ├── DangKyDoanService.cs
│   ├── PhanCongService.cs
│   ├── KetThucService.cs
│   └── ThongKeService.cs
├── Forms/
│   ├── FormHelper.cs
│   ├── FrmMain.cs
│   ├── FrmDanhMuc.cs
│   ├── FrmTour.cs
│   ├── FrmChuyenLe.cs
│   ├── FrmDangKyLe.cs
│   ├── FrmDangKyDoan.cs
│   ├── FrmPhanCongHDV.cs
│   ├── FrmKetThucKhaoSat.cs
│   └── FrmLuongThongKe.cs
└── App.config
## Cơ sở dữ liệu

11 bảng, khóa chính/khóa ngoại đầy đủ, cài đặt chặt chẽ các ràng buộc CHECK và UNIQUE quan trọng[cite: 99, 100, 101, 102]:

- `TourDiemDung` có `CHECK`: nếu `CoKhachSan = 0` thì `HangSaoKhachSan IS NULL`, nếu `CoKhachSan = 1` thì hạng sao bắt buộc từ 2 đến 5[cite: 100].
- `DangKyLe` có `CHECK(SoNguoi BETWEEN 1 AND 11)` và `CHECK(DaThanhToan = 1)` — khách lẻ phải dưới 12 người và bắt buộc mua/thanh toán vé ngay[cite: 78, 99, 101].
- `DangKyDoan` có `CHECK(SoNguoi > 12)` và `CHECK(TienCoc > 0)` — khách đoàn phải trên 12 người và bắt buộc đặt cọc trước[cite: 78, 99, 101].
- `PhanCongHDV` có `UX_PC_ChuyenLe` (UNIQUE INDEX) — chặn mỗi chuyến khách lẻ chỉ được phân công đúng một hướng dẫn viên[cite: 99, 102].
- `KhaoSat` có `UX_KS_Le` và `UX_KS_Doan` — mỗi đăng ký (lẻ hoặc đoàn) chỉ nhận tối đa một phiếu khảo sát[cite: 99, 102].

File script nằm ở `Database/QuanLyCongTyDuLich.sql`, chạy trực tiếp trên SQL Server hoặc LocalDB là xong, có sẵn dữ liệu mẫu để test nhanh[cite: 100, 102, 103].

## Cài đặt

1. Chạy `Database/QuanLyCongTyDuLich.sql` trên SQL Server Management Studio hoặc LocalDB (`(localdb)\MSSQLLocalDB`)[cite: 100, 120].
2. Mở `App.config`, sửa chuỗi kết nối `QuanLyCongTyDuLichDB` cho khớp với server máy bạn[cite: 120].
3. Build solution bằng Visual Studio 2022, target .NET Framework 4.7.2[cite: 76, 119, 147].
4. Chạy project — `FrmMain` tự mở làm trang chủ điều hướng đến các chức năng con[cite: 103, 104, 132].

Không cần cấu hình gì thêm. Dữ liệu lỗi hoặc sai tham số sẽ được các lớp `Service` bắt lỗi và trả về thông báo thông qua `KetQuaXuLy`[cite: 78, 121].

## Các module

**Danh mục** — quản lý phương tiện di chuyển, điểm bán vé, hướng dẫn viên và địa điểm tham quan[cite: 77, 104, 122, 123]. Đây là dữ liệu nền cho toàn hệ thống[cite: 99].

**Tour & Hành trình** — cấu hình thông tin tour (mọi tour xuất phát và kết thúc tại TP.HCM), danh sách điểm dừng chân theo thứ tự, gán phương tiện theo từng chặng và gắn các điểm tham quan[cite: 76, 77, 78, 100, 101, 123, 124].

**Lịch chuyến & Đăng ký lẻ** — mở lịch chuyến cho khách lẻ, tự động tính ngày về ($Ngày\ về = Ngày\ đi + Số\ ngày - 1$), cho phép đóng đăng ký[cite: 77, 78, 124, 125]. Khách lẻ dưới 12 người đăng ký theo chuyến và thanh toán vé 100% ngay tại điểm bán vé[cite: 76, 77, 78, 125, 126].

**Đăng ký theo đoàn** — lập phiếu cho đoàn trên 12 người, chọn ngày đi bất kỳ, thu tiền cọc[cite: 77, 78, 126, 127]. Nếu đoàn mua bảo hiểm, hệ thống bắt buộc nhập đủ danh sách thành viên cùng đi[cite: 77, 78, 126, 127]. Hỗ trợ hủy phiếu (mất cọc) và tự động gỡ lịch phân công HDV trong một Transaction[cite: 77, 78, 127].

**Phân công HDV** — phân công HDV theo chuyến lẻ hoặc theo đoàn[cite: 77, 78, 128]. Tự động kiểm tra chống trùng lịch công tác của HDV[cite: 77, 78, 128].

**Kết thúc tour & Khảo sát** — thanh toán kinh phí còn lại cho đoàn (chỉ cho phép sau ngày kết thúc tour)[cite: 77, 78, 129]; gửi phiếu khảo sát ý kiến (sau ngày về) và ghi nhận điểm đánh giá (1–5 sao), góp ý[cite: 77, 78, 130].

**Lương & Thống kê** — tự động tính bảng lương tháng cho HDV ($Lương\ căn\ bản + Tổng\ thù\ lao\ các\ tour\ kết\ thúc\ trong\ tháng$) và thống kê tổng hợp số lượng/doanh thu theo khoảng ngày[cite: 77, 78, 131].

## Một vài quyết định thiết kế đáng nói

Trường hợp số người đăng ký đúng 12 người do đề không quy định thuộc nhóm lẻ hay đoàn, hệ thống cài đặt chặn ở cả hai phía (`Check` constraint & `Service` logic) và báo người dùng ra quyết định nghiệp vụ[cite: 78, 100, 125, 126].

Trạng thái ngày về của khách lẻ và ngày kết thúc dự kiến của khách đoàn được tính tự động theo công thức nghiệp vụ ($Ngày\ đi + Số\ ngày - 1$), không để nhân viên tự nhập tay[cite: 77, 78, 125, 127].

Toàn bộ thao tác hủy phiếu đoàn (đổi trạng thái phiếu + gỡ phân công HDV) và lập phiếu đoàn kèm bảo hiểm được bao bọc trong một SQL Transaction để đảm bảo tính toàn vẹn dữ liệu[cite: 78, 85, 127].

Thù lao từng tour của HDV do người phân công nhập linh hoạt khi lập lịch, vì đề bài không cho mức thù lao cố định theo tour[cite: 78, 128, 129].

## Hạn chế hiện tại

Trạng thái đi tour của đoàn/chuyến (Đang đi tour, Chờ thanh toán) hiện đang được suy ra trực tiếp từ ngày hệ thống so với ngày khởi hành/kết thúc chứ CSDL chưa lưu cứng trạng thái này[cite: 83, 84].

Chưa phân quyền chi tiết cho từng loại nhân viên trên giao diện: hiện tại mọi nút trên `FrmMain` đều mở được mà chưa chặn theo tài khoản đăng nhập[cite: 77, 104, 132].

## Ghi chú

Bài làm phục vụ môn học, chưa audit bảo mật kỹ và chưa viết unit test[cite: 76, 145]. Để đưa vào sử dụng thực tế cần bổ sung màn hình đăng nhập, phân quyền người dùng và mã hóa kết nối[cite: 77, 120].
