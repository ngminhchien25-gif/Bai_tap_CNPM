/* Chạy SAU script tạo CSDL (QLHoSoBenhAn) và phần dữ liệu mẫu.
   Mật khẩu lưu = SHA-512 của 'tendangnhap:matkhau' (khớp Auth.Hash trong ứng dụng).
   Tài khoản mẫu: mật khẩu 123456 (riêng admin: admin123). */
USE QLHoSoBenhAn;
GO
SET NOCOUNT ON;

INSERT dbo.TaiKhoan (tenDangNhap, matKhauHash, vaiTro, maNV)
SELECT 'tiepnhan', HASHBYTES('SHA2_512', 'tiepnhan:123456'), N'Tiếp nhận', maNV FROM dbo.NhanVien WHERE chucVu = N'Tiếp nhận' AND NOT EXISTS (SELECT 1 FROM dbo.TaiKhoan WHERE tenDangNhap = 'tiepnhan') AND maNV = (SELECT TOP 1 maNV FROM dbo.NhanVien WHERE chucVu = N'Tiếp nhận' ORDER BY maNV);

INSERT dbo.TaiKhoan (tenDangNhap, matKhauHash, vaiTro, maNV)
SELECT 'cskh', HASHBYTES('SHA2_512', 'cskh:123456'), N'CSKH', maNV FROM dbo.NhanVien WHERE maNV = (SELECT TOP 1 maNV FROM dbo.NhanVien WHERE chucVu = N'CSKH' ORDER BY maNV) AND NOT EXISTS (SELECT 1 FROM dbo.TaiKhoan WHERE tenDangNhap = 'cskh');

INSERT dbo.TaiKhoan (tenDangNhap, matKhauHash, vaiTro, maNV)
SELECT 'thungan', HASHBYTES('SHA2_512', 'thungan:123456'), N'Thu ngân', maNV FROM dbo.NhanVien WHERE maNV = (SELECT TOP 1 maNV FROM dbo.NhanVien WHERE chucVu = N'Thu ngân' ORDER BY maNV) AND NOT EXISTS (SELECT 1 FROM dbo.TaiKhoan WHERE tenDangNhap = 'thungan');

INSERT dbo.TaiKhoan (tenDangNhap, matKhauHash, vaiTro, maNV)
SELECT 'xetnghiem', HASHBYTES('SHA2_512', 'xetnghiem:123456'), N'Xét nghiệm', maNV FROM dbo.NhanVien WHERE maNV = (SELECT TOP 1 maNV FROM dbo.NhanVien WHERE chucVu = N'Xét nghiệm' ORDER BY maNV) AND NOT EXISTS (SELECT 1 FROM dbo.TaiKhoan WHERE tenDangNhap = 'xetnghiem');

INSERT dbo.TaiKhoan (tenDangNhap, matKhauHash, vaiTro, maNV)
SELECT 'admin', HASHBYTES('SHA2_512', 'admin:admin123'), N'Quản trị', maNV FROM dbo.NhanVien WHERE maNV = (SELECT TOP 1 maNV FROM dbo.NhanVien WHERE chucVu = N'Quản trị' ORDER BY maNV) AND NOT EXISTS (SELECT 1 FROM dbo.TaiKhoan WHERE tenDangNhap = 'admin');

INSERT dbo.TaiKhoan (tenDangNhap, matKhauHash, vaiTro, maBS)
SELECT 'bs.leminh', HASHBYTES('SHA2_512', 'bs.leminh:123456'), N'Bác sĩ', maBS FROM dbo.BacSi WHERE hoTen = N'BS. Lê Minh' AND NOT EXISTS (SELECT 1 FROM dbo.TaiKhoan WHERE tenDangNhap = 'bs.leminh');

INSERT dbo.TaiKhoan (tenDangNhap, matKhauHash, vaiTro, maBS)
SELECT 'bs.tranha', HASHBYTES('SHA2_512', 'bs.tranha:123456'), N'Bác sĩ', maBS FROM dbo.BacSi WHERE hoTen = N'BS. Trần Hà' AND NOT EXISTS (SELECT 1 FROM dbo.TaiKhoan WHERE tenDangNhap = 'bs.tranha');

SELECT tenDangNhap, vaiTro FROM dbo.TaiKhoan ORDER BY tenDangNhap;
GO
