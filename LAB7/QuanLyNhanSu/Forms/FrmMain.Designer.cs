using System.Windows.Forms;

namespace QuanLyNhanSu.Forms
{
    partial class FrmMain
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.lblTitle = new System.Windows.Forms.Label();
            this.btnDanhMuc = new System.Windows.Forms.Button();
            this.btnTuyenDung = new System.Windows.Forms.Button();
            this.btnQuyetDinh = new System.Windows.Forms.Button();
            this.btnChamCong = new System.Windows.Forms.Button();
            this.btnKhenThuong = new System.Windows.Forms.Button();
            this.btnBangLuong = new System.Windows.Forms.Button();
            this.btnThoat = new System.Windows.Forms.Button();
            this.SuspendLayout();
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Location = new System.Drawing.Point(0, 40);
            this.lblTitle.Size = new System.Drawing.Size(780, 50);
            this.lblTitle.Text = "HỆ THỐNG QUẢN LÝ NHÂN SỰ";
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold);
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblTitle.AutoSize = false;
            this.btnDanhMuc.Name = "btnDanhMuc";
            this.btnDanhMuc.Location = new System.Drawing.Point(100, 120);
            this.btnDanhMuc.Size = new System.Drawing.Size(240, 60);
            this.btnDanhMuc.Text = "Danh mục / Nhân viên";
            this.btnDanhMuc.UseVisualStyleBackColor = true;
            this.btnDanhMuc.Click += new System.EventHandler(this.btnDanhMuc_Click);
            this.btnTuyenDung.Name = "btnTuyenDung";
            this.btnTuyenDung.Location = new System.Drawing.Point(440, 120);
            this.btnTuyenDung.Size = new System.Drawing.Size(240, 60);
            this.btnTuyenDung.Text = "Tuyển dụng - Hợp đồng";
            this.btnTuyenDung.UseVisualStyleBackColor = true;
            this.btnTuyenDung.Click += new System.EventHandler(this.btnTuyenDung_Click);
            this.btnQuyetDinh.Name = "btnQuyetDinh";
            this.btnQuyetDinh.Location = new System.Drawing.Point(100, 195);
            this.btnQuyetDinh.Size = new System.Drawing.Size(240, 60);
            this.btnQuyetDinh.Text = "QĐ chức vụ - lương";
            this.btnQuyetDinh.UseVisualStyleBackColor = true;
            this.btnQuyetDinh.Click += new System.EventHandler(this.btnQuyetDinh_Click);
            this.btnChamCong.Name = "btnChamCong";
            this.btnChamCong.Location = new System.Drawing.Point(440, 195);
            this.btnChamCong.Size = new System.Drawing.Size(240, 60);
            this.btnChamCong.Text = "Phân công - Chấm công - Phụ trội";
            this.btnChamCong.UseVisualStyleBackColor = true;
            this.btnChamCong.Click += new System.EventHandler(this.btnChamCong_Click);
            this.btnKhenThuong.Name = "btnKhenThuong";
            this.btnKhenThuong.Location = new System.Drawing.Point(100, 270);
            this.btnKhenThuong.Size = new System.Drawing.Size(240, 60);
            this.btnKhenThuong.Text = "Khen thưởng - Kỷ luật";
            this.btnKhenThuong.UseVisualStyleBackColor = true;
            this.btnKhenThuong.Click += new System.EventHandler(this.btnKhenThuong_Click);
            this.btnBangLuong.Name = "btnBangLuong";
            this.btnBangLuong.Location = new System.Drawing.Point(440, 270);
            this.btnBangLuong.Size = new System.Drawing.Size(240, 60);
            this.btnBangLuong.Text = "Bảng lương";
            this.btnBangLuong.UseVisualStyleBackColor = true;
            this.btnBangLuong.Click += new System.EventHandler(this.btnBangLuong_Click);
            this.btnThoat.Name = "btnThoat";
            this.btnThoat.Location = new System.Drawing.Point(270, 350);
            this.btnThoat.Size = new System.Drawing.Size(240, 60);
            this.btnThoat.Text = "Thoát";
            this.btnThoat.UseVisualStyleBackColor = true;
            this.btnThoat.Click += new System.EventHandler(this.btnThoat_Click);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.btnDanhMuc);
            this.Controls.Add(this.btnTuyenDung);
            this.Controls.Add(this.btnQuyetDinh);
            this.Controls.Add(this.btnChamCong);
            this.Controls.Add(this.btnKhenThuong);
            this.Controls.Add(this.btnBangLuong);
            this.Controls.Add(this.btnThoat);
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.ClientSize = new System.Drawing.Size(780, 450);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Quản lý nhân sự";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnDanhMuc;
        private System.Windows.Forms.Button btnTuyenDung;
        private System.Windows.Forms.Button btnQuyetDinh;
        private System.Windows.Forms.Button btnChamCong;
        private System.Windows.Forms.Button btnKhenThuong;
        private System.Windows.Forms.Button btnBangLuong;
        private System.Windows.Forms.Button btnThoat;
    }
}