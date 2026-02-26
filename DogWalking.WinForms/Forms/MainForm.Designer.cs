namespace DogWalking.WinForms.Forms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        pnlTopBar = new Panel();
        lblAppTitle = new Label();
        lblSession = new Label();
        btnLogout = new Button();
        tabs = new TabControl();
        pnlTopBar.SuspendLayout();
        SuspendLayout();

        // pnlTopBar
        pnlTopBar.BackColor = Color.FromArgb(30, 70, 150);
        pnlTopBar.Controls.Add(lblAppTitle);
        pnlTopBar.Controls.Add(lblSession);
        pnlTopBar.Controls.Add(btnLogout);
        pnlTopBar.Dock = DockStyle.Top;
        pnlTopBar.Location = new Point(0, 0);
        pnlTopBar.Name = "pnlTopBar";
        pnlTopBar.Size = new Size(1050, 46);

        // lblAppTitle
        lblAppTitle.AutoSize = true;
        lblAppTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
        lblAppTitle.ForeColor = Color.White;
        lblAppTitle.Location = new Point(12, 11);
        lblAppTitle.Name = "lblAppTitle";
        lblAppTitle.Text = "\U0001f43e Dog Walking Manager";

        // lblSession
        lblSession.AutoSize = true;
        lblSession.Font = new Font("Segoe UI", 9F);
        lblSession.ForeColor = Color.LightCyan;
        lblSession.Location = new Point(680, 14);
        lblSession.Name = "lblSession";
        lblSession.Text = "";

        // btnLogout
        btnLogout.BackColor = Color.FromArgb(60, 60, 120);
        btnLogout.FlatAppearance.BorderSize = 0;
        btnLogout.FlatStyle = FlatStyle.Flat;
        btnLogout.Font = new Font("Segoe UI", 9F);
        btnLogout.ForeColor = Color.White;
        btnLogout.Location = new Point(940, 9);
        btnLogout.Name = "btnLogout";
        btnLogout.Size = new Size(90, 28);
        btnLogout.Text = "Logout";
        btnLogout.UseVisualStyleBackColor = false;
        btnLogout.Click += BtnLogout_Click;

        // tabs
        tabs.Dock = DockStyle.Fill;
        tabs.Font = new Font("Segoe UI", 9F);
        tabs.Location = new Point(0, 46);
        tabs.Name = "tabs";
        tabs.Size = new Size(1050, 634);

        // MainForm
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.WhiteSmoke;
        ClientSize = new Size(1050, 680);
        Controls.Add(tabs);
        Controls.Add(pnlTopBar);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Dog Walking Manager";
        pnlTopBar.ResumeLayout(false);
        pnlTopBar.PerformLayout();
        ResumeLayout(false);
    }

    #endregion

    private Panel pnlTopBar;
    private Label lblAppTitle;
    private Label lblSession;
    private Button btnLogout;
    private TabControl tabs;
}
