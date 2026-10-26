using System.Windows.Forms;

namespace QuanLyNhanSu.Forms
{
    partial class FrmBangLuong
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
            this.lblBangLuong1 = new System.Windows.Forms.Label();
            this.numThang = new System.Windows.Forms.NumericUpDown();
            this.lblBangLuong2 = new System.Windows.Forms.Label();
            this.numNam = new System.Windows.Forms.NumericUpDown();
            this.btnTinhLuong = new System.Windows.Forms.Button();
            this.btnXem = new System.Windows.Forms.Button();
            this.btnChot = new System.Windows.Forms.Button();
            this.dgvBangLuong = new System.Windows.Forms.DataGridView();
            this.lblTong = new System.Windows.Forms.Label();
            this.btnDong = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numThang)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numNam)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBangLuong)).BeginInit();
            this.SuspendLayout();
            this.lblBangLuong1.Name = "lblBangLuong1";
            this.lblBangLuong1.Location = new System.Drawing.Point(20, 23);
            this.lblBangLuong1.AutoSize = true;
            this.lblBangLuong1.Text = "Tháng";
            this.numThang.Name = "numThang";
            this.numThang.Location = new System.Drawing.Point(145, 20);
            this.numThang.Size = new System.Drawing.Size(80, 23);
            this.numThang.Maximum = new decimal(new int[] { 12, 0, 0, 0 });
            this.numThang.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numThang.Increment = new decimal(new int[] { 1, 0, 0, 0 });
            this.lblBangLuong2.Name = "lblBangLuong2";
            this.lblBangLuong2.Location = new System.Drawing.Point(20, 58);
            this.lblBangLuong2.AutoSize = true;
            this.lblBangLuong2.Text = "Năm";
            this.numNam.Name = "numNam";
            this.numNam.Location = new System.Drawing.Point(145, 55);
            this.numNam.Size = new System.Drawing.Size(100, 23);
            this.numNam.Maximum = new decimal(new int[] { 2100, 0, 0, 0 });
            this.numNam.Minimum = new decimal(new int[] { 2000, 0, 0, 0 });
            this.numNam.Increment = new decimal(new int[] { 1, 0, 0, 0 });
            this.btnTinhLuong.Name = "btnTinhLuong";
            this.btnTinhLuong.Location = new System.Drawing.Point(380, 18);
            this.btnTinhLuong.Size = new System.Drawing.Size(120, 30);
            this.btnTinhLuong.Text = "Tính lương";
            this.btnTinhLuong.UseVisualStyleBackColor = true;
            this.btnTinhLuong.Click += new System.EventHandler(this.btnTinhLuong_Click);
            this.btnXem.Name = "btnXem";
            this.btnXem.Location = new System.Drawing.Point(510, 18);
            this.btnXem.Size = new System.Drawing.Size(100, 30);
            this.btnXem.Text = "Xem";
            this.btnXem.UseVisualStyleBackColor = true;
            this.btnXem.Click += new System.EventHandler(this.btnXem_Click);
            this.btnChot.Name = "btnChot";
            this.btnChot.Location = new System.Drawing.Point(620, 18);
            this.btnChot.Size = new System.Drawing.Size(150, 30);
            this.btnChot.Text = "Chốt bảng lương";
            this.btnChot.UseVisualStyleBackColor = true;
            this.btnChot.Click += new System.EventHandler(this.btnChot_Click);
            this.dgvBangLuong.Name = "dgvBangLuong";
            this.dgvBangLuong.Location = new System.Drawing.Point(20, 100);
            this.dgvBangLuong.Size = new System.Drawing.Size(1060, 450);
            this.dgvBangLuong.ReadOnly = true;
            this.dgvBangLuong.AllowUserToAddRows = false;
            this.dgvBangLuong.AllowUserToDeleteRows = false;
            this.dgvBangLuong.MultiSelect = false;
            this.dgvBangLuong.RowHeadersVisible = false;
            this.dgvBangLuong.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvBangLuong.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.lblTong.Name = "lblTong";
            this.lblTong.Location = new System.Drawing.Point(20, 565);
            this.lblTong.AutoSize = true;
            this.lblTong.Text = "Tổng thực lĩnh: 0 đ";
            this.btnDong.Name = "btnDong";
            this.btnDong.Location = new System.Drawing.Point(980, 590);
            this.btnDong.Size = new System.Drawing.Size(100, 35);
            this.btnDong.Text = "Đóng";
            this.btnDong.UseVisualStyleBackColor = true;
            this.btnDong.Click += new System.EventHandler(this.btnDong_Click);
            this.Controls.Add(this.lblBangLuong1);
            this.Controls.Add(this.numThang);
            this.Controls.Add(this.lblBangLuong2);
            this.Controls.Add(this.numNam);
            this.Controls.Add(this.btnTinhLuong);
            this.Controls.Add(this.btnXem);
            this.Controls.Add(this.btnChot);
            this.Controls.Add(this.dgvBangLuong);
            this.Controls.Add(this.lblTong);
            this.Controls.Add(this.btnDong);
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.ClientSize = new System.Drawing.Size(1100, 640);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmBangLuong";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Bảng lương hằng tháng";
            this.Load += new System.EventHandler(this.FrmBangLuong_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numThang)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numNam)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBangLuong)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblBangLuong1;
        private System.Windows.Forms.NumericUpDown numThang;
        private System.Windows.Forms.Label lblBangLuong2;
        private System.Windows.Forms.NumericUpDown numNam;
        private System.Windows.Forms.Button btnTinhLuong;
        private System.Windows.Forms.Button btnXem;
        private System.Windows.Forms.Button btnChot;
        private System.Windows.Forms.DataGridView dgvBangLuong;
        private System.Windows.Forms.Label lblTong;
        private System.Windows.Forms.Button btnDong;
    }
}
