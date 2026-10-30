using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using QuanLyCaoOc.Data;

namespace QuanLyCaoOc.Forms
{
    public enum Kind { Text, Number, Date, Combo }

    public class Field
    {
        public string Name { get; private set; }
        public string Label { get; private set; }
        public Kind Kind { get; private set; }
        public bool IsKey { get; set; }
        public decimal Min { get; set; }
        public decimal Max { get; set; }
        public int Decimals { get; set; }
        public Func<DataTable> Source { get; set; }      // combo lấy từ CSDL
        public string Display { get; set; }
        public string Value { get; set; }
        public string[] Items { get; set; }              // combo cố định

        public Field(string name, string label, Kind kind = Kind.Text)
        {
            Name = name; Label = label; Kind = kind;
            Min = 0; Max = 1000000000000m; Decimals = 0;
        }
    }

    /// <summary>Khung CRUD dùng chung: ô nhập bên trái, lưới danh sách bên phải, nút Thêm mới/Lưu/Xóa.</summary>
    public class CrudPanel : Panel
    {
        private readonly List<Field> _fields;
        private readonly Dictionary<string, Control> _ctl = new Dictionary<string, Control>();
        private readonly DataGridView _grid;
        private readonly Func<DataTable> _load;
        private readonly Action<Dictionary<string, object>> _save;
        private readonly Action<string> _delete;
        private readonly Field _key;

        public CrudPanel(List<Field> fields, Func<DataTable> load, Action<Dictionary<string, object>> save, Action<string> delete)
        {
            _fields = fields; _load = load; _save = save; _delete = delete;
            _key = fields.First(f => f.IsKey);
            Dock = DockStyle.Fill;

            var left = new Panel { Left = 0, Top = 0, Width = 360, Dock = DockStyle.Left };
            Controls.Add(left);

            int y = 15;
            foreach (Field f in fields)
            {
                Ui.Lbl(left, f.Label + ":", 10, y, 120);
                Control c;
                if (f.Kind == Kind.Number) c = Ui.Num(left, "num" + f.Name, 135, y, 200, f.Min, f.Max, f.Min, f.Decimals);
                else if (f.Kind == Kind.Date) c = Ui.Dtp(left, "dt" + f.Name, 135, y, 200);
                else if (f.Kind == Kind.Combo) c = TaoCombo(left, f, y);
                else c = Ui.Txt(left, "txt" + f.Name, 135, y, 200);
                _ctl[f.Name] = c;
                y += 34;
            }

            Ui.Btn(left, "btnThemMoi", "Thêm mới", 10, y + 10, 100, (o, ev) => LamMoi());
            Ui.Btn(left, "btnLuu", "Lưu", 120, y + 10, 100, (o, ev) => Ui.Run(Luu));
            Ui.Btn(left, "btnXoa", "Xóa", 230, y + 10, 100, (o, ev) => Ui.Run(Xoa));

            _grid = Ui.Grid(this, "dgv", 0, 0, 100, 100);
            _grid.Dock = DockStyle.Fill;
            _grid.SelectionChanged += (o, ev) => DoVaoO();
            Controls.SetChildIndex(_grid, 0);

            Ui.Run(TaiLai);
        }

        private ComboBox TaoCombo(Control p, Field f, int y)
        {
            ComboBox c = Ui.Cbo(p, "cbo" + f.Name, 135, y, 200);
            if (f.Items != null) { c.Items.AddRange(f.Items); c.SelectedIndex = 0; }
            else if (f.Source != null) Ui.Bind(c, f.Source(), f.Display, f.Value);
            return c;
        }

        public void TaiLai()
        {
            _grid.DataSource = _load();
            foreach (Field f in _fields.Where(x => x.Kind == Kind.Combo && x.Source != null))
                Ui.Bind((ComboBox)_ctl[f.Name], f.Source(), f.Display, f.Value);
            DoVaoO();
        }

        private void LamMoi()
        {
            _grid.ClearSelection();
            foreach (Field f in _fields)
            {
                Control c = _ctl[f.Name];
                if (c is TextBox) ((TextBox)c).Clear();
                else if (c is NumericUpDown) { var n = (NumericUpDown)c; n.Value = Ui.Clamp(0, n.Minimum, n.Maximum); }
                else if (c is DateTimePicker) ((DateTimePicker)c).Value = DateTime.Today;
                else if (c is ComboBox) { var cb = (ComboBox)c; if (cb.Items.Count > 0) cb.SelectedIndex = 0; }
            }
            _ctl[_key.Name].Enabled = true;
        }

        private void DoVaoO()
        {
            if (_grid.CurrentRow == null || !_grid.CurrentRow.Selected) return;
            DataRow row = ((DataRowView)_grid.CurrentRow.DataBoundItem).Row;
            foreach (Field f in _fields)
            {
                if (!row.Table.Columns.Contains(f.Name) || row[f.Name] == DBNull.Value) continue;
                object v = row[f.Name];
                Control c = _ctl[f.Name];
                if (c is TextBox) ((TextBox)c).Text = v.ToString();
                else if (c is NumericUpDown)
                {
                    var n = (NumericUpDown)c;
                    n.Value = Ui.Clamp(Convert.ToDecimal(v), n.Minimum, n.Maximum);
                }
                else if (c is DateTimePicker) ((DateTimePicker)c).Value = Convert.ToDateTime(v);
                else if (c is ComboBox)
                {
                    var cb = (ComboBox)c;
                    if (f.Items != null) cb.SelectedItem = v.ToString(); else cb.SelectedValue = v;
                }
            }
            _ctl[_key.Name].Enabled = false;      // không sửa khóa khi đang chỉnh một dòng
        }

        private Dictionary<string, object> ThuGiaTri()
        {
            var d = new Dictionary<string, object>();
            foreach (Field f in _fields)
            {
                Control c = _ctl[f.Name];
                if (c is TextBox) d[f.Name] = ((TextBox)c).Text.Trim();
                else if (c is NumericUpDown) d[f.Name] = ((NumericUpDown)c).Value;
                else if (c is DateTimePicker) d[f.Name] = ((DateTimePicker)c).Value.Date;
                else if (c is ComboBox)
                {
                    var cb = (ComboBox)c;
                    object v = f.Items != null ? cb.SelectedItem : cb.SelectedValue;
                    d[f.Name] = v == null ? null : v.ToString();
                }
            }
            return d;
        }

        private void Luu()
        {
            _save(ThuGiaTri());
            Ui.Info("Đã lưu dữ liệu.");
            object k = ThuGiaTri()[_key.Name];
            string ma = k == null ? "" : k.ToString();
            TaiLai();
            foreach (DataGridViewRow r in _grid.Rows)
            {
                object cell = r.Cells[_key.Name].Value;
                if (cell != null && cell.ToString() == ma) { r.Selected = true; _grid.CurrentCell = r.Cells[0]; break; }
            }
        }

        private void Xoa()
        {
            object k = ThuGiaTri()[_key.Name];
            string ma = k == null ? "" : k.ToString();
            if (string.IsNullOrWhiteSpace(ma)) throw new NghiepVuException("Chưa chọn dòng cần xóa.");
            if (!Ui.Confirm("Xóa '" + ma + "'?")) return;
            _delete(ma);
            Ui.Info("Đã xóa.");
            TaiLai();
            LamMoi();
        }
    }
}
