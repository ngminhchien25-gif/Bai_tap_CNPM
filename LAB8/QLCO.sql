-- 1. Tạo Cơ sở dữ liệu
CREATE DATABASE QL_CaoOcVanPhong;
GO

USE QL_CaoOcVanPhong;
GO

-- 2. Tạo Bảng Tang (Danh mục tầng)
CREATE TABLE Tang (
    MaTang VARCHAR(10) PRIMARY KEY,
    TenTang NVARCHAR(50) NOT NULL,
    ViTriTang INT NOT NULL
);

-- 3. Tạo Bảng Phong (Văn phòng cho thuê)
CREATE TABLE Phong (
    MaPhong VARCHAR(10) PRIMARY KEY,
    TenPhong NVARCHAR(50) NOT NULL,
    DienTich DECIMAL(10,2) NOT NULL,
    HuongPhong NVARCHAR(20),
    DoCao DECIMAL(10,2),
    DonGiaThue DECIMAL(18,2) NOT NULL,
    TrangThai NVARCHAR(20) DEFAULT N'Trống', -- Trống, Đã thuê, Bảo trì
    MaTang VARCHAR(10) NOT NULL,
    CONSTRAINT FK_Phong_Tang FOREIGN KEY (MaTang) REFERENCES Tang(MaTang)
);

-- 4. Tạo Bảng KhachHang (Doanh nghiệp/Khách thuê)
CREATE TABLE KhachHang (
    MaKH VARCHAR(10) PRIMARY KEY,
    TenKH NVARCHAR(100) NOT NULL,
    SoCMND_CCCD VARCHAR(12) UNIQUE NOT NULL,
    SoDienThoai VARCHAR(15),
    Email VARCHAR(50),
    TenCongTy NVARCHAR(100)
);

-- 5. Tạo Bảng HopDongThue (Hợp đồng thuê mặt bằng)
CREATE TABLE HopDongThue (
    MaHD VARCHAR(10) PRIMARY KEY,
    MaKH VARCHAR(10) NOT NULL,
    NgayLap DATETIME DEFAULT GETDATE(),
    NgayBatDau DATETIME NOT NULL,
    NgayKetThuc DATETIME NOT NULL,
    ThoiHanThueThang INT NOT NULL, -- BR01: >= 6 tháng
    TienCoc DECIMAL(18,2) NOT NULL,
    TrangThaiHD NVARCHAR(20) DEFAULT N'Hiệu lực',
    CONSTRAINT FK_HopDong_KhachHang FOREIGN KEY (MaKH) REFERENCES KhachHang(MaKH),
    CONSTRAINT CHK_ThoiHanThue CHECK (ThoiHanThueThang >= 6) -- Ràng buộc BR01
);

-- 6. Tạo Bảng ChiTietHopDong (Danh sách phòng theo hợp đồng)
CREATE TABLE ChiTietHopDong (
    MaHD VARCHAR(10) NOT NULL,
    MaPhong VARCHAR(10) NOT NULL,
    DonGiaThueTaiThoiDiem DECIMAL(18,2) NOT NULL,
    PRIMARY KEY (MaHD, MaPhong),
    CONSTRAINT FK_CTHD_HopDong FOREIGN KEY (MaHD) REFERENCES HopDongThue(MaHD),
    CONSTRAINT FK_CTHD_Phong FOREIGN KEY (MaPhong) REFERENCES Phong(MaPhong)
);

-- 7. Tạo Bảng PhieuThanhToanHopDong (Thu tiền mặt bằng 6 tháng đầu & các tháng sau)
CREATE TABLE PhieuThanhToanHopDong (
    MaPhieuTT VARCHAR(10) PRIMARY KEY,
    MaHD VARCHAR(10) NOT NULL,
    KyThanhToan INT NOT NULL,
    NgayThanhToan DATETIME,
    SoTienTT DECIMAL(18,2) NOT NULL,
    TrangThai NVARCHAR(20) DEFAULT N'Chưa thanh toán', -- Đã thanh toán, Chưa thanh toán
    CONSTRAINT FK_PhieuTT_HopDong FOREIGN KEY (MaHD) REFERENCES HopDongThue(MaHD)
);

-- 8. Tạo Bảng DichVu (Danh mục dịch vụ)
CREATE TABLE DichVu (
    MaDV VARCHAR(10) PRIMARY KEY,
    TenDV NVARCHAR(50) NOT NULL,
    DonGia DECIMAL(18,2) NOT NULL,
    DonViTinh NVARCHAR(20) NOT NULL
);

-- 9. Tạo Bảng HoaDonDichVu (Hóa đơn dịch vụ hàng tháng)
CREATE TABLE HoaDonDichVu (
    MaHDDV VARCHAR(10) PRIMARY KEY,
    MaHD VARCHAR(10) NOT NULL,
    ThangNam VARCHAR(7) NOT NULL, -- Định dạng: MM/yyyy
    NgayLap DATETIME DEFAULT GETDATE(),
    NgayHetHan DATETIME NOT NULL,
    TongTien DECIMAL(18,2) DEFAULT 0,
    TrangThaiTT NVARCHAR(20) DEFAULT N'Chưa thanh toán',
    CONSTRAINT FK_HDDV_HopDong FOREIGN KEY (MaHD) REFERENCES HopDongThue(MaHD)
);

-- 10. Tạo Bảng ChiTietHoaDonDichVu
CREATE TABLE ChiTietHoaDonDichVu (
    MaHDDV VARCHAR(10) NOT NULL,
    MaDV VARCHAR(10) NOT NULL,
    ChiSoCu INT DEFAULT 0,
    ChiSoMoi INT DEFAULT 0,
    SoLuong INT NOT NULL,
    ThanhTien DECIMAL(18,2) NOT NULL,
    PRIMARY KEY (MaHDDV, MaDV),
    CONSTRAINT FK_CTHDDV_HoaDon FOREIGN KEY (MaHDDV) REFERENCES HoaDonDichVu(MaHDDV),
    CONSTRAINT FK_CTHDDV_DichVu FOREIGN KEY (MaDV) REFERENCES DichVu(MaDV)
);

-- 11. Tạo Bảng NhanVienCongTy (Nhân viên làm việc tại cao ốc)
CREATE TABLE NhanVienCongTy (
    MaNV VARCHAR(10) PRIMARY KEY,
    MaKH VARCHAR(10) NOT NULL,
    MaPhong VARCHAR(10) NOT NULL,
    HoTen NVARCHAR(100) NOT NULL,
    SoCMND VARCHAR(12) UNIQUE NOT NULL, -- BR05
    ChucVu NVARCHAR(50),
    HinhAnh NVARCHAR(200),
    CONSTRAINT FK_NV_KhachHang FOREIGN KEY (MaKH) REFERENCES KhachHang(MaKH),
    CONSTRAINT FK_NV_Phong FOREIGN KEY (MaPhong) REFERENCES Phong(MaPhong)
);

-- 12. Tạo Bảng TheGiuXe (Cấp cho nhân viên)
CREATE TABLE TheGiuXe (
    MaThe VARCHAR(10) PRIMARY KEY,
    MaNV VARCHAR(10) NOT NULL,
    BienSoXe VARCHAR(20) NOT NULL,
    LoaiXe NVARCHAR(20) NOT NULL, -- Xe máy, Ô tô
    NgayCap DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_TheXe_NhanVien FOREIGN KEY (MaNV) REFERENCES NhanVienCongTy(MaNV)
);

-- 13. Tạo Bảng PhieuKiemTraPhong (Kiểm tra định kỳ 1 năm/lần)
CREATE TABLE PhieuKiemTraPhong (
    MaPKT VARCHAR(10) PRIMARY KEY,
    MaPhong VARCHAR(10) NOT NULL,
    NgayKiemTra DATETIME DEFAULT GETDATE(),
    NguoiKiemTra NVARCHAR(100) NOT NULL,
    TinhTrang NVARCHAR(200),
    PhatHienSua BIT DEFAULT 0, -- 1: Tự ý sửa kết cấu, 0: Bình thường
    MucPhat DECIMAL(18,2) DEFAULT 0,
    CONSTRAINT FK_PKT_Phong FOREIGN KEY (MaPhong) REFERENCES Phong(MaPhong)
);

-- 14. Tạo Bảng PhieuGiaHan (Phiếu gia hạn hợp đồng)
CREATE TABLE PhieuGiaHan (
    MaPGH VARCHAR(10) PRIMARY KEY,
    MaHD VARCHAR(10) NOT NULL,
    NgayLap DATETIME DEFAULT GETDATE(),
    ThoiGianGiaHanThang INT NOT NULL,
    NgayKetThucMoi DATETIME NOT NULL,
    LyDo NVARCHAR(200),
    CONSTRAINT FK_PGH_HopDong FOREIGN KEY (MaHD) REFERENCES HopDongThue(MaHD)
);
GO

-- 15. Stored Procedure Thực hiện Gia hạn Hợp đồng (Transaction)
CREATE PROCEDURE sp_GiaHanHopDong
    @MaPGH VARCHAR(10),
    @MaHD VARCHAR(10),
    @SoThangGiaHan INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        DECLARE @NgayKetThucCu DATETIME;
        SELECT @NgayKetThucCu = NgayKetThuc FROM HopDongThue WHERE MaHD = @MaHD;
        
        DECLARE @NgayKetThucMoi DATETIME = DATEADD(month, @SoThangGiaHan, @NgayKetThucCu);

        -- Thêm ghi nhận phiếu gia hạn
        INSERT INTO PhieuGiaHan(MaPGH, MaHD, NgayLap, ThoiGianGiaHanThang, NgayKetThucMoi, LyDo)
        VALUES(@MaPGH, @MaHD, GETDATE(), @SoThangGiaHan, @NgayKetThucMoi, N'Gia hạn hợp đồng định kỳ');

        -- Cập nhật ngày kết thúc mới cho hợp đồng gốc
        UPDATE HopDongThue
        SET NgayKetThuc = @NgayKetThucMoi
        WHERE MaHD = @MaHD;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO