using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using QuanLyCaoOc.Data;

namespace QuanLyCaoOc
{
    /// <summary>Tiện ích dựng control bằng code (không cần Designer) và xử lý lỗi thống nhất.</summary>
    public static class Ui
    {
        public static readonly CultureInfo VN = new CultureInfo("vi-VN");
        public static readonly Font FontChu = new Font("Segoe UI", 9.5f);

        public static string Money(object v)
        {
            if (v == null || v == DBNull.Value) return "";
            return Convert.ToDecimal(v).ToString("N0", VN) + " đ";
        }

        public static decimal Clamp(decimal v, decimal min, decimal max)
        {
            return v < min ? min : (v > max ? max : v);
        }

        // ---------- Thông báo ----------
        public static void Run(Action a)
        {
            try { a(); }
            catch (NghiepVuException ex) { MessageBox.Show(ex.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            catch (SqlException ex) { MessageBox.Show(Db.ThongBaoLoi(ex), "Lỗi dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show(ex.ToString(), "Lỗi (DEBUG)", MessageBoxButtons.OK, MessageBoxIcon.Error);
#else
                MessageBox.Show("Đã xảy ra lỗi không mong muốn. Vui lòng thử lại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
#endif
            }
        }

        public static void Info(string m) { MessageBox.Show(m, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information); }

        public static bool Confirm(string m)
        {
            return MessageBox.Show(m, "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        // ---------- Dựng control ----------
        public static Label Lbl(Control p, string text, int x, int y, int w = 110, string name = null)
        {
            var l = new Label { Text = text, Left = x, Top = y + 3, Width = w, AutoSize = false, Height = 22 };
            if (name != null) l.Name = name;
            p.Controls.Add(l);
            return l;
        }

        public static TextBox Txt(Control p, string name, int x, int y, int w = 160)
        {
            var t = new TextBox { Name = name, Left = x, Top = y, Width = w };
            p.Controls.Add(t);
            return t;
        }

        public static ComboBox Cbo(Control p, string name, int x, int y, int w = 160, params string[] items)
        {
            var c = new ComboBox { Name = name, Left = x, Top = y, Width = w, DropDownStyle = ComboBoxStyle.DropDownList };
            if (items.Length > 0) { c.Items.AddRange(items); c.SelectedIndex = 0; }
            p.Controls.Add(c);
            return c;
        }

        public static NumericUpDown Num(Control p, string name, int x, int y, int w = 100,
            decimal min = 0, decimal max = 1000000000000m, decimal val = 0, int dec = 0)
        {
            var n = new NumericUpDown
            {
                Name = name, Left = x, Top = y, Width = w, Minimum = min, Maximum = max,
                DecimalPlaces = dec, ThousandsSeparator = true
            };
            n.Value = Clamp(val, min, max);
            p.Controls.Add(n);
            return n;
        }

        public static DateTimePicker Dtp(Control p, string name, int x, int y, int w = 130)
        {
            var d = new DateTimePicker { Name = name, Left = x, Top = y, Width = w, Format = DateTimePickerFormat.Short };
            p.Controls.Add(d);
            return d;
        }

        public static Button Btn(Control p, string name, string text, int x, int y, int w, EventHandler click)
        {
            var b = new Button { Name = name, Text = text, Left = x, Top = y, Width = w, Height = 30 };
            b.Click += click;
            p.Controls.Add(b);
            return b;
        }

        public static DataGridView Grid(Control p, string name, int x, int y, int w, int h)
        {
            var g = new DataGridView
            {
                Name = name, Left = x, Top = y, Width = w, Height = h,
                ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
                RowHeadersVisible = false, BackgroundColor = Color.White
            };
            p.Controls.Add(g);
            return g;
        }

        public static void Bind(ComboBox c, DataTable dt, string display, string value)
        {
            c.DataSource = null;
            c.DisplayMember = display;
            c.ValueMember = value;
            c.DataSource = dt;
            if (dt.Rows.Count == 0) c.SelectedIndex = -1;
        }

        public static string Val(ComboBox c)
        {
            return c.SelectedValue == null ? "" : c.SelectedValue.ToString();
        }

        public static string Cell(DataGridView g, string col)
        {
            if (g.CurrentRow == null) return "";
            object v = g.CurrentRow.Cells[col].Value;
            return v == null ? "" : v.ToString();
        }

        /// <summary>Tô màu các dòng có giá trị cột chỉ định bằng văn bản cho trước (ví dụ: Quá hạn).</summary>
        public static void TuMauDong(DataGridView g, string cot, string giaTri, Color mau)
        {
            foreach (DataGridViewRow r in g.Rows)
            {
                object v = r.Cells[cot].Value;
                if (v != null && v.ToString() == giaTri) r.DefaultCellStyle.BackColor = mau;
            }
        }

        public static void DinhDang(DataGridView g, string cot, string format)
        {
            if (g.Columns[cot] != null) g.Columns[cot].DefaultCellStyle.Format = format;
        }
    }
}
