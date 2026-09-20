create database ThuVien;
go
CREATE TABLE NguoiDung (
    MaND INT PRIMARY KEY IDENTITY(1,1),
    TenDangNhap VARCHAR(50) NOT NULL UNIQUE,
    MatKhau VARCHAR(255) NOT NULL,
    HoTen NVARCHAR(100) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    SoDienThoai VARCHAR(15),
    VaiTro NVARCHAR(20) DEFAULT N'DocGia' CHECK (VaiTro IN (N'DocGia', N'ThuThu')),
    NgayTao DATETIME DEFAULT GETDATE()
);
go
CREATE TABLE TheThuVien (
    MaThe VARCHAR(20) PRIMARY KEY,
    MaND INT NOT NULL UNIQUE,
    NgayCap DATE NOT NULL,
    NgayHetHan DATE NOT NULL,
    TrangThai NVARCHAR(20) DEFAULT N'HoatDong' CHECK (TrangThai IN (N'HoatDong', N'Khoa', N'HetHan')),
    FOREIGN KEY (MaND) REFERENCES NguoiDung(MaND) ON DELETE CASCADE
);
go
CREATE TABLE DanhMuc (
    MaDanhMuc INT PRIMARY KEY IDENTITY(1,1),
    TenDanhMuc NVARCHAR(100) NOT NULL UNIQUE,
    MoTa NVARCHAR(MAX)
);
go
CREATE TABLE TaiLieu (
    MaTaiLieu INT PRIMARY KEY IDENTITY(1,1),
    TenTaiLieu NVARCHAR(255) NOT NULL,
    TacGia NVARCHAR(150) NOT NULL,
    NamXuatBan INT,
    NhaXuatBan NVARCHAR(100),
    MaDanhMuc INT NOT NULL,
    LoaiTaiLieu NVARCHAR(20) NOT NULL CHECK (LoaiTaiLieu IN (N'SachIn', N'SachDienTu')),
    SoLuongTon INT DEFAULT 0,
    FilePath VARCHAR(255) NULL,
    NgayCapNhat DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (MaDanhMuc) REFERENCES DanhMuc(MaDanhMuc)
);
go
CREATE TABLE PhieuMuon (
    MaPhieu INT PRIMARY KEY IDENTITY(1,1),
    MaThe VARCHAR(20) NOT NULL,
    MaTaiLieu INT NOT NULL,
    NgayMuon DATETIME DEFAULT GETDATE(),
    HanTra DATE NOT NULL,
    NgayTra DATETIME NULL,
    TrangThai NVARCHAR(20) DEFAULT N'DangMuon' CHECK (TrangThai IN (N'DangMuon', N'DaTra', N'QuaHan')),
    FOREIGN KEY (MaThe) REFERENCES TheThuVien(MaThe),
    FOREIGN KEY (MaTaiLieu) REFERENCES TaiLieu(MaTaiLieu)
);
go
CREATE TABLE YeuCauNhapSach (
    MaYeuCau INT PRIMARY KEY IDENTITY(1,1),
    MaND INT NOT NULL,
    TenSach NVARCHAR(255) NOT NULL,
    TacGia NVARCHAR(150) NOT NULL,
    NamXuatBan INT,
    GhiChu NVARCHAR(MAX),
    TrangThai NVARCHAR(20) DEFAULT N'ChuaXuLy' CHECK (TrangThai IN (N'ChuaXuLy', N'DaChapNhan', N'TuChoi')),
    NgayYeuCau DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (MaND) REFERENCES NguoiDung(MaND)
);
go
INSERT INTO DanhMuc (TenDanhMuc, MoTa) VALUES
(N'Công nghệ thông tin', N'Sách về lập trình, cơ sở dữ liệu, mạng máy tính và trí tuệ nhân tạo'),
(N'Văn học', N'Tiểu thuyết, truyện ngắn, thơ ca trong và ngoài nước'),
(N'Kinh tế - Quản lý', N'Sách về tài chính, kinh doanh, marketing và quản trị doanh nghiệp'),
(N'Kỹ năng sống', N'Sách phát triển bản thân, tư duy tích cực và giao tiếp'),
(N'Lịch sử - Địa lý', N'Sách tìm hiểu về lịch sử các thời kỳ và địa lý thế giới'),
(N'Ngoại ngữ', N'Sách học tiếng Anh, Trung, Nhật, Hàn và các chứng chỉ quốc tế');
go
INSERT INTO NguoiDung (TenDangNhap, MatKhau, HoTen, Email)
VALUES ('test01', '123456', N'Nguyễn Văn A', 'test01@email.com');
INSERT INTO TheThuVien (MaThe, MaND, NgayCap, NgayHetHan, TrangThai)
VALUES ('THE001', SCOPE_IDENTITY(), GETDATE(), DATEADD(YEAR, 1, GETDATE()), N'HoatDong');
USE ThuVien;
GO
DECLARE @uq sysname, @sql nvarchar(400);
SELECT @uq = kc.name
FROM sys.key_constraints kc
JOIN sys.index_columns ic ON ic.object_id = kc.parent_object_id AND ic.index_id = kc.unique_index_id
JOIN sys.columns c        ON c.object_id = ic.object_id AND c.column_id = ic.column_id
WHERE kc.parent_object_id = OBJECT_ID('dbo.TheThuVien') AND kc.type = 'UQ' AND c.name = 'MaND';
IF @uq IS NOT NULL
BEGIN
    SET @sql = N'ALTER TABLE dbo.TheThuVien DROP CONSTRAINT ' + QUOTENAME(@uq);
    EXEC (@sql);
END
GO

IF COL_LENGTH('dbo.TheThuVien','DaDongLePhi') IS NULL
BEGIN
    ALTER TABLE dbo.TheThuVien ADD DaDongLePhi BIT NOT NULL CONSTRAINT DF_TheThuVien_LePhi DEFAULT (0);
    EXEC('UPDATE dbo.TheThuVien SET DaDongLePhi = 1');  
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_TheThuVien_Han')
    ALTER TABLE dbo.TheThuVien ADD CONSTRAINT CK_TheThuVien_Han CHECK (NgayHetHan >= NgayCap);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_The_MotTheHoatDong' AND object_id = OBJECT_ID('dbo.TheThuVien'))
    CREATE UNIQUE INDEX UX_The_MotTheHoatDong ON dbo.TheThuVien(MaND) WHERE TrangThai = N'HoatDong';
GO
IF COL_LENGTH('dbo.PhieuMuon','TinhTrangTra') IS NULL
    ALTER TABLE dbo.PhieuMuon ADD TinhTrangTra NVARCHAR(20) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_PhieuMuon_TinhTrang')
    ALTER TABLE dbo.PhieuMuon ADD CONSTRAINT CK_PhieuMuon_TinhTrang
        CHECK (TinhTrangTra IS NULL OR TinhTrangTra IN (N'BinhThuong', N'HuHong', N'Mat'));
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_PhieuMuon_Han')
    ALTER TABLE dbo.PhieuMuon ADD CONSTRAINT CK_PhieuMuon_Han CHECK (HanTra >= CAST(NgayMuon AS date));
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_PM_MotSachDangMuon' AND object_id = OBJECT_ID('dbo.PhieuMuon'))
    CREATE UNIQUE INDEX UX_PM_MotSachDangMuon ON dbo.PhieuMuon(MaThe, MaTaiLieu) WHERE NgayTra IS NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_TaiLieu_SoLuong')
    ALTER TABLE dbo.TaiLieu ADD CONSTRAINT CK_TaiLieu_SoLuong CHECK (SoLuongTon >= 0);
GO

/* ---------- 4. PhieuPhat (BR09, BR10) ---------------------------------
   Ngày phạt, lý do, phí phạt, nhân viên lập. Một phiếu mượn có tối đa
   một phiếu phạt; LyDo là tập con của: TreHan, HuHong, Mat.
*/
IF OBJECT_ID('dbo.PhieuPhat', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PhieuPhat (
        MaPhieuPhat INT IDENTITY(1,1) PRIMARY KEY,
        MaPhieu     INT           NOT NULL UNIQUE,
        MaNV        INT           NOT NULL,
        NgayPhat    DATE          NOT NULL CONSTRAINT DF_PhieuPhat_Ngay DEFAULT (CAST(GETDATE() AS date)),
        LyDo        NVARCHAR(50)  NOT NULL,
        PhiPhat     DECIMAL(18,0) NOT NULL CONSTRAINT CK_PhieuPhat_Phi CHECK (PhiPhat > 0),
        GhiChu      NVARCHAR(250) NULL,
        CONSTRAINT FK_PhieuPhat_PhieuMuon FOREIGN KEY (MaPhieu) REFERENCES dbo.PhieuMuon(MaPhieu),
        CONSTRAINT FK_PhieuPhat_NguoiDung FOREIGN KEY (MaNV)    REFERENCES dbo.NguoiDung(MaND)
    );
    CREATE INDEX IX_PhieuPhat_NgayPhat ON dbo.PhieuPhat(NgayPhat);
END
GO