# HỆ THỐNG QUẢN LÝ HỒ SƠ BỆNH ÁN BỆNH NHÂN

Mô hình phân tích thiết kế hệ thống phần mềm quản lý hồ sơ bệnh án cho bệnh viện đa khoa quốc tế.

## 1. Giới thiệu bối cảnh
Hệ thống được xây dựng nhằm số hóa toàn bộ quy trình tiếp đón, khám chữa bệnh, theo dõi sức khỏe định kỳ, quản lý toa thuốc, xét nghiệm và thanh toán viện phí tại bệnh viện[cite: 8, 13]. Hệ thống giúp:
* Lưu trữ thông tin bệnh án xuyên suốt thông qua mã hồ sơ độc nhất.
* Hỗ trợ y bác sĩ tra cứu lịch sử khám bệnh nhanh chóng, theo dõi diễn biến bệnh và phản ứng phụ của thuốc.
* Tự động quản lý quyền lợi khám sức khỏe định kỳ theo quý cho bệnh nhân.

---

## 2. Các nghiệp vụ cốt lõi & Quy tắc nghiệp vụ (Business Rules)
* **Quản lý hồ sơ & Lệ phí:** Bệnh nhân khám lần đầu được lập phiếu đăng ký và đóng lệ phí quản lý hằng năm để cấp mã hồ sơ bệnh án (`maHSBA`).
* **Tra cứu & Tiếp khám:** Tái khám bằng cách tra cứu qua mã hồ sơ hoặc thông tin cá nhân. Bác sĩ khám lâm sàng, ghi nhận chỉ số (nhiệt độ, huyết áp) và chỉ định xét nghiệm.
* **Kê toa thuốc an toàn:** Lập toa thuốc có tích hợp kiểm tra chống chỉ định và lịch sử phản ứng phụ của thuốc.
* **Khám định kỳ:** Đặt lịch hẹn khám sức khỏe định kỳ theo quý không mất phí phát sinh (đã bao gồm trong lệ phí năm).
* **Thanh toán & Xuất viện:** Tính toán viện phí, thanh toán, in hóa đơn và cập nhật chính thức thông tin vào hồ sơ bệnh án điện tử.

---

## 3. Cấu trúc Mô hình Phân tích & Thiết kế (UML)
Tài liệu và mã nguồn mô hình bao gồm các thành phần sau:
1. **Sơ đồ Lớp (Class Diagram):** 
   * Tổng quan cấu trúc hệ thống gồm 16 lớp thực thể (`BenhNhan`, `HoSoBenhAn`, `PhieuKhamBenh`, `BacSi`, `ToaThuoc`, `Thuoc`, `HoaDonVienPhi`, v.v.).
   * Chi tiết thuộc tính, kiểu dữ liệu, định danh `ID`, phương thức và các mối quan hệ kết hợp, hợp thành, bội số.
2. **Sơ đồ Use Case (Use Case Diagram):**
   * **Tổng quát:** Phân định rõ 7 nhóm Actor chính (Bệnh nhân, NV Tiếp nhận, NV CSKH, Thu ngân, Bác sĩ, NV Xét nghiệm, Quản trị hệ thống) và 23 Use Case chức năng.
   * **Phân rã chi tiết:** Các cụm nghiệp vụ đăng ký khám, quy trình khám bệnh của bác sĩ, quy trình xét nghiệm và lập toa thuốc.
3. **Sơ đồ Hoạt động (Activity Diagram):**
   * Mô tả luồng hoạt động chi tiết từ lúc đăng ký, kiểm tra hồ sơ, khám lâm sàng, rẽ nhánh xét nghiệm đến kê toa thuốc.
4. **Sơ đồ Tuần tự (Sequence Diagram):**
   * Đặc tả chi tiết luồng thông điệp của nghiệp vụ trọng tâm **"Lập toa thuốc"** giữa các đối tượng: Bác sĩ, Giao diện khám, PhieuKhamBenh, Thuoc, ToaThuoc, ChiTietToaThuoc và Cơ sở dữ liệu.
