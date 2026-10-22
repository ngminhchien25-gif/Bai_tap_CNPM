using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace BenhVien;

/// <summary>Các hàm dựng control dùng chung để mọi form có cùng phong cách.</summary>
public static partial class Ui
{
    // ---------- Màu & font ----------
    public static readonly Color Primary = Color.FromArgb(0, 121, 107);
    public static readonly Color PrimaryDark = Color.FromArgb(0, 77, 64);
    public static readonly Color Back = Color.FromArgb(244, 247, 248);
    public static readonly Color Red = Color.FromArgb(198, 40, 40);
    public static readonly Color Orange = Color.FromArgb(239, 108, 0);
    public static readonly Color Blue = Color.FromArgb(25, 118, 210);
    public static readonly Color Gray = Color.FromArgb(96, 125, 139);
    public static readonly Color TextColor = Color.FromArgb(33, 43, 54);

    public static readonly Font Normal = new("Segoe UI", 10F);
    public static readonly Font Bold = new("Segoe UI", 10F, FontStyle.Bold);
    public static readonly Font H1 = new("Segoe UI", 16F, FontStyle.Bold);

    // ---------- Hộp thoại ----------
    public static void Msg(string text) =>
        MessageBox.Show(text, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

    public static void Alert(string text) =>
        MessageBox.Show(text, "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);

    public static bool Confirm(string text) =>
        MessageBox.Show(text, "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

    // ---------- Control đơn ----------
    public static Button Btn(string text, Color? color = null, int width = 130)
    {
        var b = new Button
        {
            Text = text, Width = width, Height = 38, FlatStyle = FlatStyle.Flat,
            BackColor = color ?? Primary, ForeColor = Color.White, Font = Bold,
            Cursor = Cursors.Hand, Margin = new Padding(6, 4, 0, 4), UseVisualStyleBackColor = false
        };
        b.FlatAppearance.BorderSize = 0;
        return b;
    }

    public static Label Lbl(string text, bool bold = false, Color? color = null) => new()
    {
        Text = text, AutoSize = true, Font = bold ? Bold : Normal, ForeColor = color ?? TextColor,
        Anchor = AnchorStyles.Left, Margin = new Padding(3, 6, 8, 3)
    };

    public static TextBox Txt(bool multiline = false, bool readOnly = false, bool password = false)
    {
        var t = new TextBox { Font = Normal, ReadOnly = readOnly, UseSystemPasswordChar = password };
        if (multiline) { t.Multiline = true; t.Height = 70; t.ScrollBars = ScrollBars.Vertical; }
        if (readOnly) t.BackColor = Color.FromArgb(235, 238, 240);
        return t;
    }

    public static ComboBox Cbo(params object[] items)
    {
        var c = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = Normal };
        c.Items.AddRange(items);
        if (items.Length > 0) c.SelectedIndex = 0;
        return c;
    }

    public static DateTimePicker Date(bool withTime = false) => new()
    {
        Format = DateTimePickerFormat.Custom,
        CustomFormat = withTime ? "dd/MM/yyyy HH:mm" : "dd/MM/yyyy",
        Font = Normal
    };

    public static NumericUpDown Num(decimal min = 0, decimal max = 1000000000, decimal value = 0,
                                    int decimals = 0, bool thousands = true) => new()
    {
        Minimum = min, Maximum = max, Value = value, DecimalPlaces = decimals,
        ThousandsSeparator = thousands, Font = Normal
    };

    public static DateTime ParseDate(string s) =>
        DateTime.TryParseExact(s, new[] { "dd/MM/yyyy", "dd/MM/yyyy HH:mm" }, CultureInfo.InvariantCulture,
                               DateTimeStyles.None, out var d) ? d : DateTime.Today;

    // ---------- Khung nhập liệu ----------
    /// <summary>Lưới nhãn + control. pairs = số cặp (nhãn, control) trên mỗi hàng. TextBox nhiều dòng chiếm trọn hàng.</summary>
    public static TableLayoutPanel Fields(int pairs, params (string label, Control ctl)[] items)
    {
        var t = new TableLayoutPanel
        {
            AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = pairs * 2, Dock = DockStyle.Top, Padding = new Padding(4)
        };
        for (int i = 0; i < pairs; i++)
        {
            t.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / pairs));
        }

        int row = 0, col = 0;
        t.RowCount = 1;
        t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        void NewRow() { row++; t.RowCount = row + 1; t.RowStyles.Add(new RowStyle(SizeType.AutoSize)); }

        foreach (var (label, ctl) in items)
        {
            bool wide = ctl is TextBox { Multiline: true };
            if (wide && col != 0) { col = 0; NewRow(); }

            var lb = Lbl(label);
            if (wide) lb.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            ctl.Dock = DockStyle.Fill;
            ctl.Margin = new Padding(3, 4, 14, 4);
            t.Controls.Add(lb, col * 2, row);
            t.Controls.Add(ctl, col * 2 + 1, row);

            if (wide)
            {
                t.SetColumnSpan(ctl, pairs * 2 - 1);
                col = 0; NewRow();
            }
            else
            {
                col++;
                if (col == pairs) { col = 0; NewRow(); }
            }
        }
        return t;
    }

    public static GroupBox Group(string text, Control inner, DockStyle dock = DockStyle.Top)
    {
        var g = new GroupBox
        {
            Text = text, Dock = dock, Font = Bold, BackColor = Color.White,
            Padding = new Padding(10, 6, 10, 10), Margin = new Padding(0, 0, 0, 10)
        };
        if (dock == DockStyle.Top) { g.AutoSize = true; g.AutoSizeMode = AutoSizeMode.GrowAndShrink; }
        inner.Font = Normal;
        inner.Dock = dock == DockStyle.Top ? DockStyle.Top : DockStyle.Fill;
        g.Controls.Add(inner);
        return g;
    }

    /// <summary>Dãy control xếp ngang (thanh tìm kiếm, nhóm nút...).</summary>
    public static FlowLayoutPanel Row(params Control[] ctrls)
    {
        var f = new FlowLayoutPanel
        {
            Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
            WrapContents = true, Margin = new Padding(0, 0, 0, 8)
        };
        f.Controls.AddRange(ctrls);
        return f;
    }

    public static FlowLayoutPanel SearchBar(string label, TextBox txt, Button btn)
    {
        txt.Width = 300;
        return Row(Lbl(label, true), txt, btn);
    }

    // ---------- Bảng dữ liệu ----------
    public static DataGridView Grid(params string[] cols)
    {
        var g = new DataGridView
        {
            Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
            RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White, BorderStyle = BorderStyle.None, EnableHeadersVisualStyles = false,
            Font = Normal, ColumnHeadersHeight = 36,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        };
        g.ColumnHeadersDefaultCellStyle.BackColor = Primary;
        g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Primary;
        g.ColumnHeadersDefaultCellStyle.Font = Bold;
        g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 248, 247);
        g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(178, 223, 219);
        g.DefaultCellStyle.SelectionForeColor = Color.Black;
        foreach (var c in cols) g.Columns.Add(c, c);
        return g;
    }

    public static void AddRows(DataGridView g, params object[][] rows)
    {
        foreach (var r in rows) g.Rows.Add(r);
    }

    public static void FilterGrid(DataGridView g, string keyword)
    {
        var kw = keyword.Trim().ToLowerInvariant();
        g.CurrentCell = null;
        foreach (DataGridViewRow r in g.Rows)
            r.Visible = kw == "" || r.Cells.Cast<DataGridViewCell>()
                .Any(c => (c.Value?.ToString() ?? "").ToLowerInvariant().Contains(kw));
    }

    // ---------- Bố cục ----------
    /// <summary>Control "fill" thêm trước, các control dock Top thêm sau theo thứ tự ngược (để hiển thị đúng thứ tự từ trên xuống).</summary>
    public static void StackInto(Control host, Control fill, params Control[] tops)
    {
        if (fill != null) { fill.Dock = DockStyle.Fill; host.Controls.Add(fill); }
        for (int i = tops.Length - 1; i >= 0; i--)
        {
            tops[i].Dock = DockStyle.Top;
            host.Controls.Add(tops[i]);
        }
    }

    public static Panel Stacked(Control fill, params Control[] tops)
    {
        var p = new Panel { Dock = DockStyle.Fill };
        StackInto(p, fill, tops);
        return p;
    }

    public static TabControl Tabs(params (string title, Control page)[] pages)
    {
        var tc = new TabControl { Dock = DockStyle.Fill, Font = Bold, Padding = new Point(18, 6) };
        foreach (var (title, c) in pages)
        {
            var tp = new TabPage(title) { BackColor = Color.White, Padding = new Padding(10), Font = Normal };
            c.Dock = DockStyle.Fill;
            tp.Controls.Add(c);
            tc.TabPages.Add(tp);
        }
        return tc;
    }

    // ---------- Thành phần đặc biệt ----------
    public static Panel PhotoBox() => PhotoBox(out _);

    public static Panel PhotoBox(out PictureBox picture)
    {
        var pic = new PictureBox
        {
            Dock = DockStyle.Top, Height = 170, BorderStyle = BorderStyle.FixedSingle,
            SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.White
        };
        pic.Paint += (_, e) =>
        {
            if (pic.Image == null)
                TextRenderer.DrawText(e.Graphics, "Ảnh 3x4", Normal, pic.ClientRectangle, Color.Gray,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        };
        var btn = Btn("Chọn ảnh 3x4", Gray, 140);
        btn.Dock = DockStyle.Top;
        btn.Click += (_, _) =>
        {
            using var dlg = new OpenFileDialog { Filter = "Ảnh|*.jpg;*.jpeg;*.png;*.bmp" };
            if (dlg.ShowDialog() == DialogResult.OK) pic.Image = Image.FromFile(dlg.FileName);
        };
        var p = new Panel { Dock = DockStyle.Right, Width = 160, Padding = new Padding(10, 0, 0, 0) };
        p.Controls.Add(btn);   // thêm trước => nằm dưới
        p.Controls.Add(pic);   // thêm sau  => nằm trên
        picture = pic;
        return p;
    }

    public static Panel Card(string title, string value, Color color)
    {
        var p = new Panel { Width = 230, Height = 84, BackColor = Color.White, Margin = new Padding(0, 0, 14, 10) };
        p.Controls.Add(new Panel { Dock = DockStyle.Left, Width = 6, BackColor = color });
        p.Controls.Add(new Label
        {
            Name = "value", Text = value, Font = new Font("Segoe UI", 18F, FontStyle.Bold),
            ForeColor = color, Location = new Point(18, 30), AutoSize = true
        });
        p.Controls.Add(new Label
        {
            Text = title, Font = Normal, ForeColor = Gray, Location = new Point(18, 8), AutoSize = true
        });
        return p;
    }

    // ---------- Đọc / ghi giá trị control (dùng cho form danh mục) ----------
    public static string GetText(Control c) => c switch
    {
        NumericUpDown n => n.Value.ToString("N0"),
        _ => c.Text
    };

    public static void SetText(Control c, string s)
    {
        switch (c)
        {
            case NumericUpDown n:
                if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.CurrentCulture, out var v)) n.Value = Math.Min(n.Maximum, Math.Max(n.Minimum, v));
                break;
            case DateTimePicker d:
                d.Value = ParseDate(s);
                break;
            default:
                c.Text = s;
                break;
        }
    }
}
