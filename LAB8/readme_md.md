# HỆ THỐNG QUẢN LÝ CHO THUÊ VĂN PHÒNG CAO ỐC (LAB 8)

Hệ thống quản lý quy trình vận hành toàn bộ vòng đời cho thuê diện tích mặt bằng thương mại và văn phòng làm việc tại tòa cao ốc: từ tiếp nhận khách hàng, lập hợp đồng thuê, quản lý nhân viên & thẻ xe, chốt chỉ số dịch vụ hàng tháng, kiểm tra kết cấu phòng đến gia hạn và thanh lý hợp đồng.

---

## Kiến trúc Hệ thống (3-Tier Architecture)

Dự án được xây dựng theo mô hình 3 tầng (3-Tier Architecture) đảm bảo phân tách rõ ràng giữa Giao diện (GUI), Xử lý Nghiệp vụ (BUS/Services) và Truy xuất Dữ liệu (DAL/Data):

- **Presentation Layer (`Forms/`)**: Giao diện người dùng Windows Forms (MDI Dashboard, Forms nhập liệu và tra cứu).
- **Business Logic Layer (`Services/`)**: Xử lý các quy tắc nghiệp vụ (Business Rules), tính toán tiền cọc, chi phí dịch vụ, kiểm tra điều kiện gia hạn hợp đồng.
- **Data Access Layer (`Data/Db.cs`)**: Quản lý kết nối CSDL SQL Server, hỗ trợ thực thi truy vấn an toàn bằng `SqlParameter` và quản lý giao dịch dữ liệu `SqlTransaction`.

---

## Cấu trúc Thư mục Project

```text
QuanLyNhanSu/
├── App.config               # Cấu hình chuỗi kết nối Database SQL Server
├── Models.cs                # Các lớp Data Transfer Object (DTO) / Entity
├── Program.cs               # Điểm khởi chạy chính của ứng dụng
├── README.md                # Tài liệu hướng dẫn dự án
├── QuanLyNhanSu.csproj      # Tệp cấu hình dự án .NET
│
├── Data/                    # Data Access Layer (DAL)
│   └── Db.cs                # Lớp truy xuất SQL Server & Transaction dùng chung
│
├── Services/                # Business Logic Layer (BUS)
│   ├── IdResult.cs          # Lớp trả về kết quả xử lý nghiệp vụ
│   ├── GiaHanService.cs     # Xử lý gia hạn hợp đồng (BR08, BR09)
│   ├── HopDongService.cs    # Xử lý lập hợp đồng & cọc 6 tháng (BR01, BR02)
│   ├── NhanVienService.cs   # Xử lý nhân viên & thẻ xe (BR05, BR06)
│   └── HoaDonService.cs     # Xử lý tính tiền dịch vụ (BR04, QD01)
│
└── Forms/                   # Graphical User Interface (GUI)
    ├── FrmMain.cs           # Màn hình chính (MDI Dashboard điều khiển)
    ├── FrmDanhMucPhong.cs   # Quản lý Danh mục Tầng & Phòng
    ├── FrmKhachHang.cs      # Quản lý Doanh nghiệp / Khách thuê
    ├── FrmHopDongThue.cs    # Lập Hợp đồng Thuê & Thu tiền cọc 6 tháng
    ├── FrmNhanVienXe.cs     # Đăng ký Nhân viên & Cấp Thẻ giữ xe
    ├── FrmThanhToanDichVu.cs# Chốt chỉ số & Lập hóa đơn Dịch vụ cuối tháng
    ├── FrmKiemTraPhong.cs   # Kiểm tra định kỳ kết cấu & Lập biên bản phạt
    └── FrmGiaHanHopDong.cs  # Kiểm tra công nợ & Lập Phiếu gia hạn HĐ
```

---

## Các Quy tắc Nghiệp vụ (Business Rules & Decisions)

| Mã Quy tắc | Nội dung Chi tiết |
| :--- | :--- |
| **BR01** | Thời hạn hợp đồng thuê văn phòng tối thiểu phải từ **6 tháng trở lên**. |
| **BR02** | Khách hàng phải **trả trước 6 tháng tiền mặt bằng** ngay khi ký hợp đồng thuê. |
| **BR03** | Từ tháng thứ 7 trở đi, tiền thuê mặt bằng thanh toán vào đầu mỗi tháng (trước ngày 5). |
| **BR04** | Chi phí dịch vụ phụ trợ (điện, nước, internet...) không tính trong giá thuê và phải thanh toán vào cuối tháng. |
| **BR05** | Doanh nghiệp thuê phải khai báo danh sách nhân viên kèm số CCCD (đảm bảo duy nhất). |
| **BR06** | Mỗi nhân viên làm việc được cấp thẻ giữ xe tòa nhà kèm biển số và loại xe tương ứng. |
| **BR07** | Định kỳ 1 năm kiểm tra tình trạng văn phòng. Lập biên bản xử phạt nếu tự ý thay đổi kết cấu. |
| **BR08** | Việc gia hạn hợp đồng chỉ thành công khi **hoàn tất toàn bộ công nợ** (tiền thuê + tiền dịch vụ). |
| **BR09** | Thời gian gia hạn ($T_{gh}$) tối đa không vượt quá **1/2 thời hạn hợp đồng gốc** ($T_{goc} / 2$). |
| **QD01** | Công thức tiền dịch vụ: Điện/Nước = (Chỉ số mới - Chỉ số cũ) × Đơn giá. |
| **QD03** | Toàn bộ nghiệp vụ tác động nhiều bảng (Ký HĐ, Gia hạn) phải chạy trong `SqlTransaction`. |

---

## 🛢️ Cài đặt Cơ sở Dữ liệu SQL Server

1. Mở **SQL Server Management Studio (SSMS)**.
2. Tạo CSDL mới có tên `QL_CaoOcVanPhong`.
3. Chạy toàn bộ kịch bản SQL khởi tạo các bảng (`Tang`, `Phong`, `KhachHang`, `HopDongThue`, `PhieuThanhToanHopDong`, `HoaDonDichVu`, `PhieuGiaHan`,...) và Stored Procedure `sp_GiaHanHopDong`.
4. Cập nhật chuỗi kết nối trong file `App.config`:

```xml
<connectionStrings>
    <add name="QLNS" 
         connectionString="Data Source=.;Initial Catalog=QL_CaoOcVanPhong;Integrated Security=True" 
         providerName="System.Data.SqlClient" />
</connectionStrings>
```

---

## Hướng dẫn Biên dịch và Chạy Ứng dụng

### 1. Yêu cầu Tiền đề
- **.NET SDK**: Phiên bản 4.8 hoặc .NET Core / .NET 6.0+ hỗ trợ C# WinForms.
- **SQL Server**: Phiên bản 2012 trở lên.

### 2. Lệnh thực thi từ Cửa sổ Dòng lệnh (Terminal)

Chuyển tới thư mục dự án và tiến hành kiểm tra biên dịch / khởi chạy:

```bash
# Clean dự án (nếu cần)
dotnet clean

# Biên dịch kiểm tra lỗi
dotnet build

# Chạy ứng dụng
dotnet run
```

---

## Màn hình Điều khiển & Luồng Sử dụng

Khi khởi chạy ứng dụng, màn hình chính **`FrmMain`** (MDI Parent) hiển thị thanh Menu điều hướng giúp truy cập nhanh các chức năng:

1. **Quản lý Danh mục**: Mở `FrmDanhMucPhong` và `FrmKhachHang` để thiết lập thông tin phòng trống và khách thuê.
2. **Nghiệp vụ Thuê & Dịch vụ**:
   - `FrmHopDongThue`: Lập HĐ (>= 6 tháng), tự động tính cọc 6 tháng và cập nhật phòng sang trạng thái *Đã thuê*.
   - `FrmNhanVienXe`: Khai báo nhân viên tòa nhà và cấp thẻ xe.
   - `FrmThanhToanDichVu`: Chốt chỉ số điện/nước cuối tháng.
3. **Kiểm tra & Gia hạn**:
   - `FrmKiemTraPhong`: Lập biên bản kiểm tra kết cấu định kỳ.
   - `FrmGiaHanHopDong`: Kiểm tra tự động công nợ (BR08) và khống chế hạn gia hạn (BR09) trước khi thực thi Transaction.