namespace DogWalking.WinForms.Controls;

/// <summary>
/// Lightweight borderless popup that shows a notification message
/// at the bottom-right of the owner window and auto-closes after 5 seconds.
/// Click anywhere on it to dismiss early.
/// </summary>
public sealed class ToastNotification : Form
{
    private static int _activeCount;
    private readonly System.Windows.Forms.Timer _autoClose;

    public ToastNotification(Form owner, string title, string message, Color accentColor)
    {
        FormBorderStyle = FormBorderStyle.None;
        StartPosition   = FormStartPosition.Manual;
        Size            = new Size(360, 85);
        BackColor       = Color.White;
        ShowInTaskbar   = false;
        TopMost         = true;
        Opacity         = 0.95;

        // Position relative to the owner window
        var area = owner.DesktopBounds;
        Location = new Point(
            area.Right  - Width  - 12,
            area.Bottom - Height - 12 - (_activeCount * (Height + 6)));
        _activeCount++;

        var accent = new Panel
        {
            Dock = DockStyle.Left, Width = 5, BackColor = accentColor
        };
        var lblTitle = new Label
        {
            Text      = title,
            Font      = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 30, 30),
            Location  = new Point(16, 10),
            AutoSize  = true
        };
        var lblMsg = new Label
        {
            Text        = message,
            Font        = new Font("Segoe UI", 9),
            ForeColor   = Color.FromArgb(80, 80, 80),
            Location    = new Point(16, 35),
            MaximumSize = new Size(330, 40),
            AutoSize    = true
        };

        Controls.AddRange([accent, lblTitle, lblMsg]);

        // Click anywhere to dismiss
        Click          += (_, _) => Close();
        lblTitle.Click += (_, _) => Close();
        lblMsg.Click   += (_, _) => Close();

        _autoClose = new System.Windows.Forms.Timer { Interval = 5000 };
        _autoClose.Tick += (_, _) => Close();
        _autoClose.Start();

        FormClosed += (_, _) =>
        {
            _autoClose.Dispose();
            _activeCount = Math.Max(0, _activeCount - 1);
        };
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        ControlPaint.DrawBorder(e.Graphics, ClientRectangle,
            Color.FromArgb(200, 200, 200), ButtonBorderStyle.Solid);
    }
}
