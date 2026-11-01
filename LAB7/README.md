# Bài 7 – Hệ thống Quản lý nhân sự công ty sơn Đông Á

Bài thực hành Phân tích thiết kế hướng đối tượng: phân tích – thiết kế bằng UML, thiết kế CSDL SQL Server và hiện thực ứng dụng C# WinForms cho nghiệp vụ quản lý nhân sự, từ tuyển dụng đến tính lương, khen thưởng và kỷ luật.

| | |
| --- | --- |
| Sinh viên | Nguyễn Minh Chiến |
| MSSV | 1250080024 |
| Lớp | 12_CNPM1 |

---

## 1. Giới thiệu

Công ty sơn Đông Á cần tin học hóa toàn bộ quy trình nhân sự:

```
Tuyển dụng → Thử việc → Ký hợp đồng → QĐ chức vụ & lương
          → Phân công, chấm công, phụ trội → Tính lương
          (song song) Khen thưởng / Kỷ luật
```

Sản phẩm của bài gồm: bộ UML, CSDL SQL Server, ứng dụng C# WinForms, bộ test case và bảng truy vết yêu cầu.

## 2. Chức năng chính

| Nhóm chức năng | Form | Service |
| --- | --- | --- |
| Danh mục (phòng ban, chức vụ, ca làm việc), nhân viên | `FrmDanhMuc` | `DanhMucService` |
| QĐ tuyển dụng – thử việc, hợp đồng, bảo hiểm BHYT/BHXH | `FrmTuyenDung` | `TuyenDungService` |
| QĐ chức vụ, QĐ lương, trình ký / ký quyết định | `FrmQuyetDinh` | `QuyetDinhService` |
| Kế hoạch tuần, chấm công, phiếu phụ trội | `FrmChamCong` | `ChamCongService` |
| Khen thưởng, kỷ luật (cá nhân / tập thể) | `FrmKhenThuong` | `KhenThuongService` |
| Tính, xem, chốt bảng lương tháng | `FrmBangLuong` | `LuongService` |
| Điều hướng chính | `FrmMain` | – |

**Tác nhân:** HR, Trưởng phòng/ban/phân xưởng, Ban chấm công, Kế toán, Giám đốc, Nhân viên (6 actor, 17 use case).

## 3. Quy tắc nghiệp vụ nổi bật

- **BR02** – Chỉ ký hợp đồng chính thức sau khi hết thời gian thử việc.
- **BR03** – Chỉ hợp đồng dài hạn mới có hồ sơ BHYT/BHXH và bị trích trừ trong bảng lương.
- **BR05** – Giờ phụ trội tính bằng 1,5 lần giờ bình thường.
- **QD01** – Công thức lương:

  ```
  Thực lĩnh = LươngCB/26 × ngày công + phụ cấp chức vụ + phụ cấp lương + phụ cấp độc hại
              + tiền phụ trội + thưởng − bảo hiểm − phạt trừ lương
  Tiền phụ trội = giờ phụ trội × (LươngCB/26/8) × 1,5
  ```

- **QD03** – BHXH 8% và BHYT 1,5% trên lương cơ bản (phần người lao động), lưu ở bảng `ThamSo`.
- **QD05** – Chỉ khen thưởng/kỷ luật cá nhân được đưa vào bảng lương; phạt "Trừ lương" chỉ áp dụng cho cá nhân.
- **QD06** – QĐ lương/chức vụ đang hiệu lực là quyết định đã ký có ngày hiệu lực mới nhất; không cho hai quyết định cùng ngày hiệu lực của một nhân viên.
- **QD07** – Chỉ tính lương khi mọi bảng chấm công của tháng đã chốt; bảng lương đã chốt không được tính lại hay sửa.
- **QD10** – Trạng thái nhân viên: `Chờ tuyển dụng → Thử việc → Chính thức → Nghỉ việc`; CCCD gồm 12 chữ số.

Danh sách đầy đủ (BR01–BR09, QD01–QD10) xem trong báo cáo Word, mục 1.2.3.

## 4. Công nghệ

| Thành phần | Công nghệ |
| --- | --- |
| Ngôn ngữ / giao diện | C# WinForms, .NET Framework 4.7.2 |
| CSDL | Microsoft SQL Server |
| Mô hình hóa | StarUML (use case, class, state, sequence, activity); ERD |
| Kiến trúc | 3 lớp: **Form (UI) → Service → Db (truy cập dữ liệu)** |

Nguyên tắc cài đặt:

- Form không viết SQL, mọi truy cập dữ liệu đi qua Service và lớp `Db` (đóng vai trò Repository).
- Câu lệnh SQL luôn dùng `SqlParameter`, không nối chuỗi dữ liệu người dùng.
- Các thao tác lập quyết định, ký hợp đồng, lập phiếu phụ trội, kế hoạch tuần và tính lương chạy trong **transaction**, lỗi thì Rollback.
- Lỗi CSDL (ví dụ mã 547 – vi phạm khóa ngoại) được đổi thành thông báo dễ hiểu, không hiện lỗi SQL thô.
- Ứng dụng không có đăng nhập/phân quyền (QD08); vai trò thể hiện ở mục "Actor chính" của từng form.

## 5. Cơ sở dữ liệu

Gồm **24 bảng**, chia theo nhóm:

- **Danh mục:** `PhongBan`, `ChucVu`, `CaLamViec`, `ThamSo`
- **Nhân sự:** `NhanVien`
- **Quyết định** (bảng cha `QuyetDinh` + 5 bảng con): `QDTuyenDung`, `QDChucVu`, `QDLuong`, `QDKhenThuong`, `QDKyLuat`, kèm `ChiTietDoiTuongQD`
- **Hợp đồng – bảo hiểm:** `HopDong`, `HoSoBaoHiem`
- **Phân công – chấm công – phụ trội:** `KeHoachTuan`, `ChiTietPhanCong`, `BangChamCong`, `ChiTietChamCong`, `PhieuPhuTroi`, `ChiTietPhuTroi`
- **Lương:** `BangLuong`, `ChiTietBangLuong`
- **Thu – chi:** `PhieuChiKhenThuong`, `PhieuThuNopPhat`

Ràng buộc quan trọng: `CHECK` trạng thái nhân viên và quyết định, `CCCD UNIQUE`, `CHECK` lương/số tiền/số giờ > 0, `CK_DoiTuong` (mỗi dòng đối tượng chỉ có `MaNV` **hoặc** `MaPB`), `UNIQUE (Thang, Nam, MaPB)` cho `BangChamCong`, `UNIQUE (Thang, Nam)` cho `BangLuong`, mỗi quyết định tối đa một phiếu chi/thu.

Cuối script SQL có sẵn **dữ liệu mẫu**: 3 phòng ban, 3 nhân viên (NV001 chờ tuyển dụng, NV002 và NV003 chính thức), chấm công phòng SX tháng 9/2026 (đã chốt) và 10/2026 (chưa chốt), phiếu phụ trội PT001, một quyết định kỷ luật phạt trừ lương NV002.

## 6. Hướng dẫn cài đặt và chạy

**Yêu cầu:** Windows, Visual Studio (khuyến nghị 2022) có workload *.NET desktop development*, .NET Framework 4.7.2, SQL Server (Express/Developer) và SSMS.

1. **Tạo CSDL:** mở SSMS, chạy file script SQL của bài (tạo bảng, ràng buộc và dữ liệu mẫu).
2. **Cấu hình kết nối:** mở file cấu hình của dự án (`App.config`) và sửa chuỗi kết nối cho đúng máy, ví dụ:

   ```xml
   <connectionStrings>
     <add name="QuanLyNhanSu"
          connectionString="Server=.\SQLEXPRESS;Database=QuanLyNhanSu;Integrated Security=True;"
          providerName="System.Data.SqlClient" />
   </connectionStrings>
   ```

3. **Mở solution** bằng Visual Studio, chọn *Restore NuGet Packages* nếu có.
4. **Build và chạy** (`F5`). Ứng dụng mở `FrmMain`, từ đó vào các form chức năng.

> Tên CSDL, tên file script và tên chuỗi kết nối ở trên chỉ là ví dụ, cần đổi cho khớp với dự án thực tế.

## 7. Kiểm thử

Có **18 test case (TC01–TC18)** chạy trên dữ liệu mẫu, bao phủ: thêm nhân viên và trùng CCCD, QĐ tuyển dụng, ký hợp đồng (trước/sau hết thử việc), hợp đồng ngắn hạn không lập bảo hiểm, QĐ lương trùng ngày hiệu lực, phiếu phụ trội, tính và chốt lương, khen thưởng/kỷ luật cá nhân và tập thể, chuyển trạng thái quyết định.

Một số kết quả mẫu để đối chiếu:

| TC | Kịch bản | Kết quả mong đợi |
| --- | --- | --- |
| TC10 | Lương CB 10.400.000, 4 giờ phụ trội | Tiền phụ trội = 4 × 50.000 × 1,5 = **300.000** |
| TC11 | Tính lương 9/2026, NV002 | 10.400.000 + 500.000 + 300.000 − 988.000 − 200.000 = **10.012.000** |
| TC11 | Tính lương 9/2026, NV003 | 7.384.615 + 346.154 = **7.730.769** |
| TC12 | Tính lương 10/2026 (chấm công chưa chốt) | Từ chối theo QD07 |
| TC13 | Tính lại lương đã chốt | Từ chối |

Chi tiết dữ liệu và kết quả mong đợi của từng test case xem trong báo cáo, mục 6.

## 8. Cấu trúc tài liệu và mã nguồn

Mục lục báo cáo Word:

1. Giới thiệu hệ thống (nghiệp vụ, yêu cầu, quy tắc, thuật ngữ)
2. Pha phân tích: use case tổng quát (Hình P.1), phân rã use case (P.2), biểu đồ lớp (P.3, P.4), biểu đồ trạng thái (P.5–P.7)
3. Pha thiết kế: biểu đồ tuần tự (P.8–P.10), thiết kế từng chức năng, biểu đồ hoạt động (P.12–P.13)
4. Thiết kế CSDL: ERD (P.14, P.15), danh sách bảng, ràng buộc
5. Thiết kế giao diện (P.17–P.23)
6. Kiểm thử tổng thể

Cấu trúc thư mục đề xuất (điều chỉnh theo dự án thực tế):

```
Bai7_QuanLyNhanSu/
├── README.md
├── docs/
│   └── Bai7_QuanLyNhanSu.docx      # báo cáo
├── uml/                             # file StarUML
├── database/
│   └── script.sql                   # tạo bảng, ràng buộc, dữ liệu mẫu
└── src/
    ├── Forms/                       # FrmMain, FrmDanhMuc, FrmTuyenDung, ...
    ├── Services/                    # *Service
    ├── Data/                        # lớp Db
    └── App.config
```

## 9. Lưu ý

- Các mức lương thử việc, phụ cấp, thưởng, phạt không do hệ thống tự đặt mà nhập từ quyết định (QD09). Số liệu trong dữ liệu mẫu và test case là số liệu giả định của nhóm.
- Bảng lương đã chốt không thể tính lại hoặc chỉnh sửa; muốn thay đổi cần xử lý theo quy trình ngoài phạm vi ứng dụng.
