using System.Drawing;
using System.Windows.Forms;

namespace BenhVien.Forms;

/// <summary>Form nền: thanh tiêu đề màu, vùng nội dung, thanh nút phía dưới (có sẵn nút Đóng).</summary>
public class FrmBase : Form
{
    protected readonly Panel Content = new() { Dock = DockStyle.Fill, Padding = new Padding(14) };

    protected readonly FlowLayoutPanel Actions = new()
    {
        Dock = DockStyle.Bottom, Height = 56, FlowDirection = FlowDirection.RightToLeft,
        Padding = new Padding(10, 6, 10, 6), BackColor = Color.White
    };

    public FrmBase(string title, string subtitle, int width = 1000, int height = 700)
    {
        Text = title;
        Font = Ui.Normal;
        BackColor = Ui.Back;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(width, height);
        MinimumSize = new Size(Math.Min(width, 900), Math.Min(height, 620));

        var header = new Panel { Dock = DockStyle.Top, Height = 66, BackColor = Ui.Primary };
        header.Controls.Add(new Label
        {
            Text = subtitle, ForeColor = Color.FromArgb(178, 223, 219),
            Font = new Font("Segoe UI", 9F), Location = new Point(18, 40), AutoSize = true
        });
        header.Controls.Add(new Label
        {
            Text = title, ForeColor = Color.White, Font = Ui.H1, Location = new Point(16, 6), AutoSize = true
        });

        var close = Ui.Btn("Đóng", Ui.Gray, 100);
        close.Click += (_, _) => Close();
        Actions.Controls.Add(close);               // thêm đầu tiên => nằm ngoài cùng bên phải

        Controls.Add(Content);                     // Fill phải thêm trước
        Controls.Add(Actions);
        Controls.Add(header);
    }

    protected void Stack(Control fill, params Control[] tops) => Ui.StackInto(Content, fill, tops);

    /// <summary>Truyền các nút theo thứ tự muốn hiển thị từ trái sang phải (nút Đóng luôn ở ngoài cùng bên phải).</summary>
    protected void AddActions(params Button[] leftToRight)
    {
        for (int i = leftToRight.Length - 1; i >= 0; i--) Actions.Controls.Add(leftToRight[i]);
    }
}
