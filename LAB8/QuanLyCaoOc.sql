/* =====================================================================
   BÀI 4 - QUẢN LÝ CHO THUÊ VĂN PHÒNG Ở CAO ỐC (SQL Server)
   Chạy toàn bộ script trong SSMS: tạo CSDL, bảng, ràng buộc, view, dữ liệu mẫu.
   Mọi số liệu mẫu là giả định của nhóm.
   ===================================================================== */
USE master;
GO
IF DB_ID(N'QuanLyCaoOc') IS NOT NULL
BEGIN
    ALTER DATABASE QuanLyCaoOc SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE QuanLyCaoOc;
END
GO
CREATE DATABASE QuanLyCaoOc COLLATE Vietnamese_CI_AS;
GO
USE QuanLyCaoOc;
GO

/* ---------------------------- DANH MỤC ---------------------------- */
CREATE TABLE ThamSo (
    MaThamSo VARCHAR(30)    NOT NULL PRIMARY KEY,
    GiaTri   DECIMAL(18,2)  NOT NULL,
    MoTa     NVARCHAR(200)  NULL
);

CREATE TABLE Tang (
    MaTang   VARCHAR(10)    NOT NULL PRIMARY KEY,
    TenTang  NVARCHAR(50)   NOT NULL,
    DoCao    INT            NOT NULL CONSTRAINT CK_Tang_DoCao CHECK (DoCao > 0),     -- vị trí/độ cao của tầng
    DonGiaM2 DECIMAL(18,0)  NOT NULL CONSTRAINT CK_Tang_DonGia CHECK (DonGiaM2 > 0) -- đơn giá thuê /m2/tháng theo độ cao
);

CREATE TABLE Phong (
    MaPhong   VARCHAR(10)    NOT NULL PRIMARY KEY,
    MaTang    VARCHAR(10)    NOT NULL CONSTRAINT FK_Phong_Tang REFERENCES Tang(MaTang),
    DienTich  DECIMAL(10,2)  NOT NULL CONSTRAINT CK_Phong_DienTich CHECK (DienTich > 0),
    Huong     NVARCHAR(20)   NOT NULL,
    GiaThue   DECIMAL(18,0)  NOT NULL CONSTRAINT CK_Phong_Gia CHECK (GiaThue > 0),   -- = DienTich x DonGiaM2 của tầng
    TrangThai NVARCHAR(20)   NOT NULL CONSTRAINT DF_Phong_TT DEFAULT N'Trống'
              CONSTRAINT CK_Phong_TT CHECK (TrangThai IN (N'Trống', N'Đang thuê', N'Bảo trì'))
);

CREATE TABLE KhachHang (
    MaKH          VARCHAR(10)   NOT NULL PRIMARY KEY,
    TenCongTy     NVARCHAR(100) NOT NULL,
    MaSoThue      VARCHAR(20)   NOT NULL CONSTRAINT UQ_KH_MST UNIQUE,
    NguoiDaiDien  NVARCHAR(100) NOT NULL,
    DiaChi        NVARCHAR(200) NULL,
    SDT           VARCHAR(15)   NULL,
    Email         VARCHAR(100)  NULL
);

CREATE TABLE NhanVienQL (          -- nhân viên bộ phận quản lý cao ốc
    MaNVQL  VARCHAR(10)   NOT NULL PRIMARY KEY,
    HoTen   NVARCHAR(100) NOT NULL,
    ChucVu  NVARCHAR(50)  NOT NULL,
    SDT     VARCHAR(15)   NULL
);

CREATE TABLE DichVu (
    MaDV      VARCHAR(10)   NOT NULL PRIMARY KEY,
    TenDV     NVARCHAR(50)  NOT NULL,
    DonViTinh NVARCHAR(20)  NOT NULL,
    DonGia    DECIMAL(18,0) NOT NULL CONSTRAINT CK_DV_DonGia CHECK (DonGia >= 0)
);

/* ----------------------- HỢP ĐỒNG - GIA HẠN ----------------------- */
CREATE TABLE HopDong (
    SoHD        VARCHAR(15)  NOT NULL PRIMARY KEY,
    MaKH        VARCHAR(10)  NOT NULL CONSTRAINT FK_HD_KH   REFERENCES KhachHang(MaKH),
    MaNVQL      VARCHAR(10)  NOT NULL CONSTRAINT FK_HD_NVQL REFERENCES NhanVienQL(MaNVQL),
    NgayLap     DATE         NOT NULL,
    NgayBatDau  DATE         NOT NULL,
    SoThang     INT          NOT NULL CONSTRAINT CK_HD_SoThang CHECK (SoThang >= 6),   -- thuê tối thiểu 6 tháng (BR03)
    NgayKetThuc DATE         NOT NULL,
    TrangThai   NVARCHAR(20) NOT NULL CONSTRAINT DF_HD_TT DEFAULT N'Chờ thanh toán'
                CONSTRAINT CK_HD_TT CHECK (TrangThai IN (N'Chờ thanh toán', N'Hiệu lực', N'Hết hạn', N'Huỷ')),
    CONSTRAINT CK_HD_Ngay CHECK (NgayKetThuc > NgayBatDau)
);

CREATE TABLE ChiTietHopDong (      -- khách thuê nhiều phòng trong một hợp đồng (BR02)
    SoHD        VARCHAR(15)   NOT NULL CONSTRAINT FK_CTHD_HD   REFERENCES HopDong(SoHD),
    MaPhong     VARCHAR(10)   NOT NULL CONSTRAINT FK_CTHD_Phong REFERENCES Phong(MaPhong),
    GiaThue     DECIMAL(18,0) NOT NULL CONSTRAINT CK_CTHD_Gia CHECK (GiaThue > 0),  -- giá chốt tại thời điểm ký
    NgayKetThuc DATE          NOT NULL,                                              -- thay đổi khi gia hạn
    CONSTRAINT PK_CTHD PRIMARY KEY (SoHD, MaPhong)
);

CREATE TABLE PhieuGiaHan (
    SoPG    VARCHAR(15)   NOT NULL PRIMARY KEY,
    SoHD    VARCHAR(15)   NOT NULL CONSTRAINT FK_PGH_HD   REFERENCES HopDong(SoHD),
    NgayLap DATE          NOT NULL,
    MaNVQL  VARCHAR(10)   NOT NULL CONSTRAINT FK_PGH_NVQL REFERENCES NhanVienQL(MaNVQL),
    GhiChu  NVARCHAR(300) NULL
);

CREATE TABLE ChiTietGiaHan (       -- phòng nào, gia hạn bao lâu (<= 1/2 thời gian thuê)
    SoPG            VARCHAR(15) NOT NULL CONSTRAINT FK_CTGH_PG REFERENCES PhieuGiaHan(SoPG),
    MaPhong         VARCHAR(10) NOT NULL CONSTRAINT FK_CTGH_Phong REFERENCES Phong(MaPhong),
    SoThangGiaHan   INT         NOT NULL CONSTRAINT CK_CTGH_Thang CHECK (SoThangGiaHan > 0),
    NgayKetThucCu   DATE        NOT NULL,
    NgayKetThucMoi  DATE        NOT NULL,
    CONSTRAINT PK_CTGH PRIMARY KEY (SoPG, MaPhong),
    CONSTRAINT CK_CTGH_Ngay CHECK (NgayKetThucMoi > NgayKetThucCu)
);

/* ---------------- THANH TOÁN TIỀN THUÊ & DỊCH VỤ ---------------- */
CREATE TABLE KyThanhToanThue (     -- lịch thu tiền thuê: 1 kỳ trả trước 6 tháng + các kỳ hàng tháng từ tháng 7
    MaKy         INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    SoHD         VARCHAR(15)   NOT NULL CONSTRAINT FK_KY_HD REFERENCES HopDong(SoHD),
    LoaiKy       NVARCHAR(20)  NOT NULL CONSTRAINT CK_KY_Loai CHECK (LoaiKy IN (N'Trả trước', N'Hàng tháng')),
    TuThang      INT           NOT NULL CONSTRAINT CK_KY_Tu  CHECK (TuThang >= 1),
    DenThang     INT           NOT NULL,
    SoTien       DECIMAL(18,0) NOT NULL CONSTRAINT CK_KY_Tien CHECK (SoTien > 0),
    HanThanhToan DATE          NOT NULL,
    DaThu        BIT           NOT NULL CONSTRAINT DF_KY_DaThu DEFAULT 0,
    NgayThu      DATE          NULL,
    HinhThuc     NVARCHAR(20)  NULL,
    NguoiThu     NVARCHAR(50)  NULL,
    CONSTRAINT CK_KY_Den  CHECK (DenThang >= TuThang),
    CONSTRAINT CK_KY_Thu  CHECK ((DaThu = 0 AND NgayThu IS NULL) OR (DaThu = 1 AND NgayThu IS NOT NULL))
);
CREATE INDEX IX_KY_HD ON KyThanhToanThue(SoHD, TuThang);

CREATE TABLE HoaDonDichVu (        -- hóa đơn dịch vụ (điện, nước, internet...) thanh toán cuối tháng
    SoHDDV        VARCHAR(20)   NOT NULL PRIMARY KEY,
    SoHD          VARCHAR(15)   NOT NULL CONSTRAINT FK_HDDV_HD REFERENCES HopDong(SoHD),
    Thang         INT           NOT NULL CONSTRAINT CK_HDDV_Thang CHECK (Thang BETWEEN 1 AND 12),
    Nam           INT           NOT NULL,
    NgayLap       DATE          NOT NULL,
    HanThanhToan  DATE          NOT NULL,                              -- cuối tháng
    TongTien      DECIMAL(18,0) NOT NULL CONSTRAINT DF_HDDV_Tong DEFAULT 0,
    DaThanhToan   BIT           NOT NULL CONSTRAINT DF_HDDV_DaTT DEFAULT 0,
    NgayThanhToan DATE          NULL,
    CONSTRAINT UQ_HDDV UNIQUE (SoHD, Thang, Nam),
    CONSTRAINT CK_HDDV_TT CHECK ((DaThanhToan = 0 AND NgayThanhToan IS NULL) OR (DaThanhToan = 1 AND NgayThanhToan IS NOT NULL))
);

CREATE TABLE ChiTietHoaDonDV (
    SoHDDV    VARCHAR(20)   NOT NULL CONSTRAINT FK_CTHDDV_HD REFERENCES HoaDonDichVu(SoHDDV),
    MaPhong   VARCHAR(10)   NOT NULL CONSTRAINT FK_CTHDDV_Phong REFERENCES Phong(MaPhong),
    MaDV      VARCHAR(10)   NOT NULL CONSTRAINT FK_CTHDDV_DV REFERENCES DichVu(MaDV),
    SoLuong   DECIMAL(18,2) NOT NULL CONSTRAINT CK_CTHDDV_SL CHECK (SoLuong >= 0),
    DonGia    DECIMAL(18,0) NOT NULL,
    ThanhTien AS (CONVERT(DECIMAL(18,0), SoLuong * DonGia)) PERSISTED,
    CONSTRAINT PK_CTHDDV PRIMARY KEY (SoHDDV, MaPhong, MaDV)
);

/* --------------- NHÂN VIÊN CÔNG TY THUÊ - THẺ GIỮ XE --------------- */
CREATE TABLE NhanVien (            -- nhân viên làm việc tại các văn phòng, công ty phải báo cáo cho cao ốc
    MaNV        VARCHAR(10)   NOT NULL PRIMARY KEY,
    HoTen       NVARCHAR(100) NOT NULL,
    NgaySinh    DATE          NULL,
    GioiTinh    NVARCHAR(5)   NOT NULL CONSTRAINT CK_NV_GT CHECK (GioiTinh IN (N'Nam', N'Nữ', N'Khác')),
    HinhAnh     NVARCHAR(260) NULL,                  -- đường dẫn tệp ảnh
    ChucVu      NVARCHAR(50)  NOT NULL,
    MaKH        VARCHAR(10)   NOT NULL CONSTRAINT FK_NV_KH    REFERENCES KhachHang(MaKH),   -- thuộc công ty nào
    MaPhong     VARCHAR(10)   NOT NULL CONSTRAINT FK_NV_Phong REFERENCES Phong(MaPhong),   -- làm việc ở phòng nào (suy ra tầng)
    NgayBaoCao  DATE          NOT NULL CONSTRAINT DF_NV_NBC DEFAULT CAST(GETDATE() AS DATE),
    TrangThai   NVARCHAR(20)  NOT NULL CONSTRAINT DF_NV_TT DEFAULT N'Đang làm'
                CONSTRAINT CK_NV_TT CHECK (TrangThai IN (N'Đang làm', N'Đã nghỉ'))
);

CREATE TABLE TheGiuXe (
    MaThe     VARCHAR(10)  NOT NULL PRIMARY KEY,
    MaNV      VARCHAR(10)  NOT NULL CONSTRAINT FK_The_NV REFERENCES NhanVien(MaNV),
    LoaiXe    NVARCHAR(20) NOT NULL CONSTRAINT CK_The_Loai CHECK (LoaiXe IN (N'Xe máy', N'Ô tô', N'Xe đạp')),
    BienSo    VARCHAR(15)  NOT NULL,
    HangXe    NVARCHAR(50) NULL,
    MauSac    NVARCHAR(30) NULL,
    NgayCap   DATE         NOT NULL CONSTRAINT DF_The_NC DEFAULT CAST(GETDATE() AS DATE),
    TrangThai NVARCHAR(20) NOT NULL CONSTRAINT DF_The_TT DEFAULT N'Đang dùng'
              CONSTRAINT CK_The_TT CHECK (TrangThai IN (N'Đang dùng', N'Đã huỷ'))
);
-- Một biển số chỉ có tối đa một thẻ đang dùng (thẻ đã huỷ không chặn việc cấp lại)
CREATE UNIQUE INDEX UX_The_BienSo_DangDung ON TheGiuXe(BienSo) WHERE TrangThai = N'Đang dùng';

/* ------------------- KIỂM TRA ĐỊNH KỲ - XỬ PHẠT ------------------- */
CREATE TABLE PhieuKiemTra (
    MaKT    VARCHAR(15)   NOT NULL PRIMARY KEY,
    NgayKT  DATE          NOT NULL,
    MaNVQL  VARCHAR(10)   NOT NULL CONSTRAINT FK_PKT_NVQL REFERENCES NhanVienQL(MaNVQL),
    KetLuan NVARCHAR(500) NULL
);

CREATE TABLE ChiTietKiemTra (
    MaKT      VARCHAR(15)   NOT NULL CONSTRAINT FK_CTKT_KT REFERENCES PhieuKiemTra(MaKT),
    MaPhong   VARCHAR(10)   NOT NULL CONSTRAINT FK_CTKT_Phong REFERENCES Phong(MaPhong),
    TinhTrang NVARCHAR(20)  NOT NULL CONSTRAINT CK_CTKT_TT CHECK (TinhTrang IN (N'Tốt', N'Cần tu sửa', N'Hư hỏng nặng')),
    CanTuSua  BIT           NOT NULL CONSTRAINT DF_CTKT_Sua DEFAULT 0,
    GhiChu    NVARCHAR(300) NULL,
    CONSTRAINT PK_CTKT PRIMARY KEY (MaKT, MaPhong)
);

CREATE TABLE PhieuPhat (           -- phạt tự ý thay đổi/chỉnh sửa kết cấu văn phòng
    MaPhat      VARCHAR(15)   NOT NULL PRIMARY KEY,
    SoHD        VARCHAR(15)   NOT NULL,
    MaPhong     VARCHAR(10)   NOT NULL,
    NgayLap     DATE          NOT NULL,
    LyDo        NVARCHAR(300) NOT NULL,
    SoTienPhat  DECIMAL(18,0) NOT NULL CONSTRAINT CK_PP_Tien CHECK (SoTienPhat > 0),
    DaNop       BIT           NOT NULL CONSTRAINT DF_PP_DaNop DEFAULT 0,
    NgayNop     DATE          NULL,
    CONSTRAINT FK_PP_CTHD FOREIGN KEY (SoHD, MaPhong) REFERENCES ChiTietHopDong(SoHD, MaPhong),
    CONSTRAINT CK_PP_Nop CHECK ((DaNop = 0 AND NgayNop IS NULL) OR (DaNop = 1 AND NgayNop IS NOT NULL))
);
GO

/* ------------------------------ VIEW ------------------------------ */
CREATE VIEW vw_PhongTrong AS
SELECT p.MaPhong, p.MaTang, t.TenTang, t.DoCao, p.DienTich, p.Huong, p.GiaThue
FROM Phong p JOIN Tang t ON t.MaTang = p.MaTang
WHERE p.TrangThai = N'Trống';
GO

CREATE VIEW vw_CongNoThue AS      -- kỳ thu tiền thuê đã đến hạn mà chưa thu
SELECT k.MaKy, k.SoHD, h.MaKH, k.LoaiKy, k.TuThang, k.DenThang, k.SoTien, k.HanThanhToan,
       DATEDIFF(DAY, k.HanThanhToan, CAST(GETDATE() AS DATE)) AS SoNgayTre
FROM KyThanhToanThue k JOIN HopDong h ON h.SoHD = k.SoHD
WHERE k.DaThu = 0 AND k.HanThanhToan <= CAST(GETDATE() AS DATE) AND h.TrangThai <> N'Huỷ';
GO

CREATE VIEW vw_CongNoDichVu AS    -- hóa đơn dịch vụ chưa thanh toán
SELECT d.SoHDDV, d.SoHD, h.MaKH, d.Thang, d.Nam, d.TongTien, d.HanThanhToan
FROM HoaDonDichVu d JOIN HopDong h ON h.SoHD = d.SoHD
WHERE d.DaThanhToan = 0;
GO

CREATE VIEW vw_PhongKiemTra AS    -- phòng đến hạn kiểm tra (chu kỳ lấy từ ThamSo, mặc định 12 tháng)
SELECT p.MaPhong, p.TrangThai, MAX(k.NgayKT) AS NgayKiemTraGanNhat,
       CASE WHEN MAX(k.NgayKT) IS NULL
              OR DATEADD(MONTH, (SELECT CAST(GiaTri AS INT) FROM ThamSo WHERE MaThamSo = 'CHU_KY_KIEM_TRA_THANG'), MAX(k.NgayKT)) <= CAST(GETDATE() AS DATE)
            THEN 1 ELSE 0 END AS DenHan
FROM Phong p
LEFT JOIN ChiTietKiemTra c ON c.MaPhong = p.MaPhong
LEFT JOIN PhieuKiemTra  k ON k.MaKT = c.MaKT
GROUP BY p.MaPhong, p.TrangThai;
GO

/* --------------------------- DỮ LIỆU MẪU --------------------------- */
INSERT ThamSo VALUES
 ('SO_THANG_TOI_THIEU', 6,   N'Thời gian thuê tối thiểu (tháng)'),
 ('SO_THANG_TRA_TRUOC', 6,   N'Số tháng đầu phải trả trước khi ký hợp đồng'),
 ('TY_LE_GIA_HAN_TOI_DA', 0.5, N'Mỗi lần gia hạn tối đa bằng 1/2 thời gian thuê'),
 ('CHU_KY_KIEM_TRA_THANG', 12, N'Chu kỳ kiểm tra tình trạng phòng (tháng)');

INSERT Tang VALUES
 ('T01', N'Tầng 1', 1, 200000), ('T02', N'Tầng 2', 2, 220000), ('T03', N'Tầng 3', 3, 240000),
 ('T04', N'Tầng 4', 4, 260000), ('T05', N'Tầng 5', 5, 300000);

INSERT Phong (MaPhong, MaTang, DienTich, Huong, GiaThue, TrangThai)
SELECT v.MaPhong, v.MaTang, v.DienTich, v.Huong, v.DienTich * t.DonGiaM2, v.TrangThai
FROM (VALUES
 ('P101','T01', 50.0, N'Đông', N'Đang thuê'), ('P102','T01', 80.0, N'Tây',  N'Trống'),
 ('P201','T02', 60.0, N'Nam',  N'Đang thuê'), ('P202','T02',100.0, N'Bắc',  N'Trống'),
 ('P301','T03', 75.0, N'Đông', N'Đang thuê'), ('P302','T03',120.0, N'Tây',  N'Đang thuê'),
 ('P401','T04', 90.0, N'Nam',  N'Trống'),     ('P402','T04',110.0, N'Bắc',  N'Bảo trì'),
 ('P501','T05',150.0, N'Đông', N'Trống')
) AS v(MaPhong, MaTang, DienTich, Huong, TrangThai)
JOIN Tang t ON t.MaTang = v.MaTang;

INSERT KhachHang VALUES
 ('KH001', N'Công ty TNHH Phần mềm ABC',   '0301234567', N'Nguyễn Văn An',  N'12 Nguyễn Huệ, Q.1, TP.HCM',  '0901111222', 'abc@example.com'),
 ('KH002', N'Công ty CP Thương mại XYZ',   '0307654321', N'Trần Thị Bình',  N'45 Lê Lợi, Q.1, TP.HCM',      '0902222333', 'xyz@example.com'),
 ('KH003', N'Công ty TNHH Tư vấn Đông Nam','0309988776', N'Lê Quốc Cường',  N'88 Hai Bà Trưng, Q.3, TP.HCM','0903333444', 'dongnam@example.com');

INSERT NhanVienQL VALUES
 ('QL01', N'Phạm Minh Tuấn', N'Trưởng ban quản lý', '0911000111'),
 ('QL02', N'Võ Thị Hoa',     N'Nhân viên kế toán',  '0911000222'),
 ('QL03', N'Đặng Văn Kiên',  N'Nhân viên kỹ thuật', '0911000333');

INSERT DichVu VALUES
 ('DV01', N'Điện',     N'kWh',   3500),
 ('DV02', N'Nước',     N'm3',    20000),
 ('DV03', N'Internet', N'tháng', 300000);

/* HD2026-001: KH001 thuê P101, 12 tháng từ 01/01/2026, đã thu đến tháng 10, còn nợ hóa đơn dịch vụ tháng 10 */
INSERT HopDong VALUES ('HD2026-001','KH001','QL01','2025-12-20','2026-01-01',12,'2026-12-31',N'Hiệu lực');
INSERT ChiTietHopDong VALUES ('HD2026-001','P101',10000000,'2026-12-31');
INSERT KyThanhToanThue (SoHD,LoaiKy,TuThang,DenThang,SoTien,HanThanhToan,DaThu,NgayThu,HinhThuc,NguoiThu)
VALUES ('HD2026-001',N'Trả trước',1,6,60000000,'2026-01-01',1,'2026-01-01',N'Chuyển khoản',N'Võ Thị Hoa');
;WITH n AS (SELECT 7 AS k UNION ALL SELECT k + 1 FROM n WHERE k < 12)
INSERT KyThanhToanThue (SoHD,LoaiKy,TuThang,DenThang,SoTien,HanThanhToan,DaThu,NgayThu,HinhThuc,NguoiThu)
SELECT 'HD2026-001', N'Hàng tháng', k, k, 10000000, DATEADD(MONTH, k - 1, '2026-01-01'),
       CASE WHEN k <= 10 THEN 1 ELSE 0 END,
       CASE WHEN k <= 10 THEN DATEADD(MONTH, k - 1, '2026-01-01') END,
       CASE WHEN k <= 10 THEN N'Chuyển khoản' END,
       CASE WHEN k <= 10 THEN N'Võ Thị Hoa' END
FROM n;

/* HD2026-002: KH002 thuê P201, 6 tháng từ 01/05/2026, không nợ -> đủ điều kiện gia hạn (tối đa 3 tháng) */
INSERT HopDong VALUES ('HD2026-002','KH002','QL01','2026-04-20','2026-05-01',6,'2026-10-31',N'Hiệu lực');
INSERT ChiTietHopDong VALUES ('HD2026-002','P201',13200000,'2026-10-31');
INSERT KyThanhToanThue (SoHD,LoaiKy,TuThang,DenThang,SoTien,HanThanhToan,DaThu,NgayThu,HinhThuc,NguoiThu)
VALUES ('HD2026-002',N'Trả trước',1,6,79200000,'2026-05-01',1,'2026-05-01',N'Tiền mặt',N'Võ Thị Hoa');

/* HD2026-003: KH003 thuê P301 + P302, 8 tháng từ 01/04/2026, quá hạn tiền thuê tháng 7 (đến hạn 01/10/2026 chưa thu) */
INSERT HopDong VALUES ('HD2026-003','KH003','QL01','2026-03-25','2026-04-01',8,'2026-11-30',N'Hiệu lực');
INSERT ChiTietHopDong VALUES ('HD2026-003','P301',18000000,'2026-11-30'), ('HD2026-003','P302',28800000,'2026-11-30');
INSERT KyThanhToanThue (SoHD,LoaiKy,TuThang,DenThang,SoTien,HanThanhToan,DaThu,NgayThu,HinhThuc,NguoiThu)
VALUES ('HD2026-003',N'Trả trước',1,6,280800000,'2026-04-01',1,'2026-04-01',N'Chuyển khoản',N'Võ Thị Hoa'),
       ('HD2026-003',N'Hàng tháng',7,7,46800000,'2026-10-01',0,NULL,NULL,NULL),
       ('HD2026-003',N'Hàng tháng',8,8,46800000,'2026-11-01',0,NULL,NULL,NULL);

/* Hóa đơn dịch vụ mẫu */
INSERT HoaDonDichVu (SoHDDV,SoHD,Thang,Nam,NgayLap,HanThanhToan,DaThanhToan,NgayThanhToan) VALUES
 ('DV2026-09-001','HD2026-001',9,2026,'2026-09-28','2026-09-30',1,'2026-09-30'),
 ('DV2026-10-001','HD2026-001',10,2026,'2026-10-28','2026-10-31',0,NULL),
 ('DV2026-09-002','HD2026-002',9,2026,'2026-09-28','2026-09-30',1,'2026-09-30');
INSERT ChiTietHoaDonDV (SoHDDV,MaPhong,MaDV,SoLuong,DonGia) VALUES
 ('DV2026-09-001','P101','DV01',1200,3500), ('DV2026-09-001','P101','DV02',30,20000), ('DV2026-09-001','P101','DV03',1,300000),
 ('DV2026-10-001','P101','DV01',1350,3500), ('DV2026-10-001','P101','DV02',35,20000), ('DV2026-10-001','P101','DV03',1,300000),
 ('DV2026-09-002','P201','DV01',800,3500),  ('DV2026-09-002','P201','DV02',20,20000), ('DV2026-09-002','P201','DV03',1,300000);
UPDATE HoaDonDichVu SET TongTien = (SELECT SUM(ThanhTien) FROM ChiTietHoaDonDV c WHERE c.SoHDDV = HoaDonDichVu.SoHDDV);

/* Nhân viên công ty thuê và thẻ giữ xe */
INSERT NhanVien (MaNV,HoTen,NgaySinh,GioiTinh,HinhAnh,ChucVu,MaKH,MaPhong,NgayBaoCao,TrangThai) VALUES
 ('NV0001', N'Hoàng Gia Bảo', '1995-03-12', N'Nam', NULL, N'Lập trình viên',  'KH001','P101','2026-01-05',N'Đang làm'),
 ('NV0002', N'Nguyễn Thu Hà', '1998-07-21', N'Nữ',  NULL, N'Kế toán',        'KH002','P201','2026-05-03',N'Đang làm'),
 ('NV0003', N'Trương Minh Đức','1990-11-02', N'Nam', NULL, N'Giám đốc kinh doanh','KH003','P302','2026-04-04',N'Đang làm');
INSERT TheGiuXe (MaThe,MaNV,LoaiXe,BienSo,HangXe,MauSac,NgayCap,TrangThai) VALUES
 ('TX001','NV0001',N'Xe máy','59A1-123.45',N'Honda',N'Đen','2026-01-06',N'Đang dùng'),
 ('TX002','NV0003',N'Ô tô',  '51H-678.90', N'Toyota',N'Trắng','2026-04-05',N'Đang dùng');

/* Kiểm tra định kỳ năm trước (đã quá 12 tháng -> đến hạn kiểm tra lại) và một phiếu phạt mẫu */
INSERT PhieuKiemTra VALUES ('KT2025-001','2025-10-15','QL03',N'Các phòng đang thuê hoạt động bình thường.');
INSERT ChiTietKiemTra VALUES ('KT2025-001','P101',N'Tốt',0,NULL), ('KT2025-001','P102',N'Tốt',0,NULL),
                             ('KT2025-001','P402',N'Cần tu sửa',1,N'Thấm trần, cần xử lý chống thấm');
INSERT PhieuPhat VALUES ('PF2026-001','HD2026-003','P302','2026-08-10',N'Tự ý dỡ vách ngăn phòng',5000000,0,NULL);
GO

/* --------- Truy vấn kiểm tra nhanh điều kiện gia hạn (BR07) --------- */
-- SELECT SoHD, (SELECT ISNULL(SUM(SoTien),0) FROM vw_CongNoThue c WHERE c.SoHD=h.SoHD) AS NoTienThue,
--               (SELECT ISNULL(SUM(TongTien),0) FROM vw_CongNoDichVu d WHERE d.SoHD=h.SoHD) AS NoDichVu
-- FROM HopDong h WHERE TrangThai = N'Hiệu lực';
