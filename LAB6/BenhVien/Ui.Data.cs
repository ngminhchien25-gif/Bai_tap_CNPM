using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace BenhVien;

/// <summary>Hàm hỗ trợ làm việc với dữ liệu từ SQL Server.</summary>
public static partial class Ui
{
    /// <summary>Chạy một thao tác CSDL, bắt lỗi và hiện thông báo thân thiện. Trả về true nếu thành công.</summary>
    public static bool TryDb(Action action)
    {
        try { action(); return true; }
        catch (SqlException ex)
        {
            Alert(ex.Number switch
            {
                2601 or 2627 => "Dữ liệu bị trùng với bản ghi đã có.",
                547 => "Dữ liệu không hợp lệ hoặc đang được bản ghi khác sử dụng.",
                >= 50000 => ex.Message,                    // lỗi nghiệp vụ do THROW trong stored procedure
                _ => "Không thao tác được với CSDL: " + ex.Message
            });
            return false;
        }
        catch (Exception ex)
        {
            Alert("Lỗi: " + ex.Message);
            return false;
        }
    }

    /// <summary>Gắn DataTable vào lưới. headers theo thứ tự cột; chuỗi rỗng = ẩn cột (dùng cho cột khóa).</summary>
    public static void Bind(DataGridView g, DataTable dt, params string[] headers)
    {
        g.DataSource = null;
        g.Columns.Clear();
        g.AutoGenerateColumns = true;
        g.DataSource = dt;
        for (int i = 0; i < g.Columns.Count && i < headers.Length; i++)
        {
            var c = g.Columns[i];
            if (headers[i] == "") { c.Visible = false; continue; }
            c.HeaderText = headers[i];
            int idx = i;
            if (c.ValueType == typeof(DateTime))
            {
                bool hasTime = dt.Rows.Cast<DataRow>().Any(r => !r.IsNull(idx) && ((DateTime)r[idx]).TimeOfDay != TimeSpan.Zero);
                c.DefaultCellStyle.Format = hasTime ? "dd/MM/yyyy HH:mm" : "dd/MM/yyyy";
            }
            else if (c.ValueType == typeof(decimal))
            {
                c.DefaultCellStyle.Format = "N0";
                c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
        }
    }

    /// <summary>Đổ DataTable vào ComboBox (DisplayMember/ValueMember phải đặt trước DataSource).</summary>
    public static void Fill(ComboBox c, DataTable dt, string display, string value)
    {
        c.DataSource = null;
        c.DisplayMember = display;
        c.ValueMember = value;
        c.DataSource = dt;
        if (dt.Rows.Count > 0) c.SelectedIndex = 0;
    }

    /// <summary>Dòng dữ liệu đang chọn trong lưới đã gắn DataTable (null nếu chưa chọn).</summary>
    public static DataRowView Sel(DataGridView g) => g.CurrentRow?.DataBoundItem as DataRowView;

    public static string S(object v) => v == null || v is DBNull ? "" : v.ToString();

    public static string D(object v, string fmt = "dd/MM/yyyy") => v is DateTime d ? d.ToString(fmt) : S(v);

    public static byte[] ImageBytes(Image img)
    {
        if (img == null) return null;
        using var ms = new MemoryStream();
        img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
        return ms.ToArray();
    }

    public static Image BytesImage(object v) =>
        v is byte[] b && b.Length > 0 ? Image.FromStream(new MemoryStream(b)) : null;

    /// <summary>Hộp nhập một dòng văn bản. Trả về null nếu người dùng bỏ qua.</summary>
    public static string Prompt(string title, string label)
    {
        using var f = new Form
        {
            Text = title, ClientSize = new Size(440, 150), FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent, MaximizeBox = false, MinimizeBox = false, Font = Normal
        };
        var l = new Label { Text = label, AutoSize = false, Location = new Point(14, 12), Size = new Size(412, 40) };
        var t = new TextBox { Location = new Point(14, 56), Width = 412 };
        var ok = Btn("Xác nhận", Primary, 120); ok.Location = new Point(190, 98); ok.DialogResult = DialogResult.OK;
        var cancel = Btn("Bỏ qua", Gray, 100); cancel.Location = new Point(326, 98); cancel.DialogResult = DialogResult.Cancel;
        f.Controls.AddRange(new Control[] { l, t, ok, cancel });
        f.AcceptButton = ok; f.CancelButton = cancel;
        return f.ShowDialog() == DialogResult.OK ? t.Text.Trim() : null;
    }

    // ---------- Đọc / ghi giá trị control cho form danh mục ----------
    public static object GetValue(Control c) => c switch
    {
        NumericUpDown n => n.Value,
        CheckBox cb => cb.Checked,
        DateTimePicker d => d.Value,
        ComboBox { DataSource: not null } cbo => cbo.SelectedValue,
        _ => c.Text.Trim()
    };

    public static void SetValue(Control c, object v)
    {
        switch (c)
        {
            case NumericUpDown n:
                n.Value = Math.Min(n.Maximum, Math.Max(n.Minimum, v is DBNull ? 0 : Convert.ToDecimal(v)));
                break;
            case CheckBox cb:
                cb.Checked = v is not DBNull && Convert.ToBoolean(v);
                break;
            case DateTimePicker d:
                if (v is DateTime dt) d.Value = dt;
                break;
            case ComboBox { DataSource: not null } cbo:
                if (v is not DBNull && v != null) cbo.SelectedValue = v;
                break;
            default:
                c.Text = S(v);
                break;
        }
    }
}
