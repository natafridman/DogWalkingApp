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

        tabClients = new TabPage();
        pnlClientsToolbar = new Panel();
        txtSearchClients = new TextBox();
        btnRefreshClients = new Button();
        dgvClients = new DataGridView();

        tabWalks = new TabPage();
        pnlWalksToolbar = new Panel();
        cmbWalkStatus = new ComboBox();
        btnNewWalk = new Button();
        dgvWalks = new DataGridView();

        tabWalkers = new TabPage();
        pnlWalkersToolbar = new Panel();
        btnRefreshWalkers = new Button();
        dgvWalkers = new DataGridView();

        tabUsers = new TabPage();
        pnlUsersToolbar = new Panel();
        btnRefreshUsers = new Button();
        dgvAllUsers = new DataGridView();

        pnlTopBar.SuspendLayout();
        tabClients.SuspendLayout();
        tabWalks.SuspendLayout();
        tabWalkers.SuspendLayout();
        tabUsers.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvClients).BeginInit();
        ((System.ComponentModel.ISupportInitialize)dgvWalks).BeginInit();
        ((System.ComponentModel.ISupportInitialize)dgvWalkers).BeginInit();
        ((System.ComponentModel.ISupportInitialize)dgvAllUsers).BeginInit();
        SuspendLayout();

        // ── pnlTopBar ─────────────────────────────────
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

        // ── tabs ───────────────────────────────────────
        tabs.Controls.Add(tabClients);
        tabs.Controls.Add(tabWalks);
        tabs.Controls.Add(tabWalkers);
        tabs.Controls.Add(tabUsers);
        tabs.Dock = DockStyle.Fill;
        tabs.Font = new Font("Segoe UI", 9F);
        tabs.Location = new Point(0, 46);
        tabs.Name = "tabs";
        tabs.Size = new Size(1050, 634);

        // ═══ ADMIN TAB: Clients & Dogs ═══════════════
        tabClients.BackColor = Color.WhiteSmoke;
        tabClients.Controls.Add(dgvClients);
        tabClients.Controls.Add(pnlClientsToolbar);
        tabClients.Name = "tabClients";
        tabClients.Padding = new Padding(3);
        tabClients.Text = "\U0001f415 Clients & Dogs";
        tabClients.Enter += TabClients_Enter;

        pnlClientsToolbar.Controls.Add(txtSearchClients);
        pnlClientsToolbar.Controls.Add(btnRefreshClients);
        pnlClientsToolbar.Location = new Point(10, 8);
        pnlClientsToolbar.Name = "pnlClientsToolbar";
        pnlClientsToolbar.Size = new Size(1000, 38);

        txtSearchClients.Location = new Point(0, 7);
        txtSearchClients.Name = "txtSearchClients";
        txtSearchClients.PlaceholderText = "Search name or email\u2026";
        txtSearchClients.Size = new Size(300, 23);
        txtSearchClients.TextChanged += TxtSearchClients_TextChanged;

        btnRefreshClients.AutoSize = true;
        btnRefreshClients.BackColor = Color.FromArgb(30, 70, 150);
        btnRefreshClients.FlatStyle = FlatStyle.Flat;
        btnRefreshClients.Font = new Font("Segoe UI", 9F);
        btnRefreshClients.ForeColor = Color.White;
        btnRefreshClients.Location = new Point(320, 5);
        btnRefreshClients.Name = "btnRefreshClients";
        btnRefreshClients.Size = new Size(90, 28);
        btnRefreshClients.Text = "\u21bb Refresh";
        btnRefreshClients.UseVisualStyleBackColor = false;
        btnRefreshClients.Click += BtnRefreshClients_Click;

        dgvClients.AllowUserToAddRows = false;
        dgvClients.AllowUserToDeleteRows = false;
        dgvClients.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvClients.BackgroundColor = Color.White;
        dgvClients.BorderStyle = BorderStyle.None;
        dgvClients.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgvClients.Font = new Font("Segoe UI", 9F);
        dgvClients.Location = new Point(10, 56);
        dgvClients.Name = "dgvClients";
        dgvClients.ReadOnly = true;
        dgvClients.RowHeadersVisible = false;
        dgvClients.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvClients.Size = new Size(1005, 525);

        // ═══ ADMIN TAB: Walk Events ══════════════════
        tabWalks.BackColor = Color.WhiteSmoke;
        tabWalks.Controls.Add(dgvWalks);
        tabWalks.Controls.Add(pnlWalksToolbar);
        tabWalks.Name = "tabWalks";
        tabWalks.Padding = new Padding(3);
        tabWalks.Text = "Walk Events";
        tabWalks.Enter += TabWalks_Enter;

        pnlWalksToolbar.Controls.Add(cmbWalkStatus);
        pnlWalksToolbar.Controls.Add(btnNewWalk);
        pnlWalksToolbar.Location = new Point(10, 8);
        pnlWalksToolbar.Name = "pnlWalksToolbar";
        pnlWalksToolbar.Size = new Size(1000, 38);

        cmbWalkStatus.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbWalkStatus.Location = new Point(0, 7);
        cmbWalkStatus.Name = "cmbWalkStatus";
        cmbWalkStatus.Size = new Size(160, 23);
        cmbWalkStatus.SelectedIndexChanged += CmbWalkStatus_SelectedIndexChanged;

        btnNewWalk.AutoSize = true;
        btnNewWalk.BackColor = Color.FromArgb(30, 70, 150);
        btnNewWalk.FlatStyle = FlatStyle.Flat;
        btnNewWalk.Font = new Font("Segoe UI", 9F);
        btnNewWalk.ForeColor = Color.White;
        btnNewWalk.Location = new Point(180, 5);
        btnNewWalk.Name = "btnNewWalk";
        btnNewWalk.Size = new Size(100, 28);
        btnNewWalk.Text = "+ New Walk";
        btnNewWalk.UseVisualStyleBackColor = false;
        btnNewWalk.Click += BtnNewWalk_Click;

        dgvWalks.AllowUserToAddRows = false;
        dgvWalks.AllowUserToDeleteRows = false;
        dgvWalks.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvWalks.BackgroundColor = Color.White;
        dgvWalks.BorderStyle = BorderStyle.None;
        dgvWalks.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgvWalks.Font = new Font("Segoe UI", 9F);
        dgvWalks.Location = new Point(10, 56);
        dgvWalks.Name = "dgvWalks";
        dgvWalks.ReadOnly = true;
        dgvWalks.RowHeadersVisible = false;
        dgvWalks.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvWalks.Size = new Size(1005, 525);

        // ═══ ADMIN TAB: Walkers ══════════════════════
        tabWalkers.BackColor = Color.WhiteSmoke;
        tabWalkers.Controls.Add(dgvWalkers);
        tabWalkers.Controls.Add(pnlWalkersToolbar);
        tabWalkers.Name = "tabWalkers";
        tabWalkers.Padding = new Padding(3);
        tabWalkers.Text = "\U0001f6b6 Walkers";
        tabWalkers.Enter += TabWalkers_Enter;

        pnlWalkersToolbar.Controls.Add(btnRefreshWalkers);
        pnlWalkersToolbar.Location = new Point(10, 8);
        pnlWalkersToolbar.Name = "pnlWalkersToolbar";
        pnlWalkersToolbar.Size = new Size(1000, 38);

        btnRefreshWalkers.AutoSize = true;
        btnRefreshWalkers.BackColor = Color.FromArgb(30, 70, 150);
        btnRefreshWalkers.FlatStyle = FlatStyle.Flat;
        btnRefreshWalkers.Font = new Font("Segoe UI", 9F);
        btnRefreshWalkers.ForeColor = Color.White;
        btnRefreshWalkers.Location = new Point(0, 5);
        btnRefreshWalkers.Name = "btnRefreshWalkers";
        btnRefreshWalkers.Size = new Size(90, 28);
        btnRefreshWalkers.Text = "\u21bb Refresh";
        btnRefreshWalkers.UseVisualStyleBackColor = false;
        btnRefreshWalkers.Click += BtnRefreshWalkers_Click;

        dgvWalkers.AllowUserToAddRows = false;
        dgvWalkers.AllowUserToDeleteRows = false;
        dgvWalkers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvWalkers.BackgroundColor = Color.White;
        dgvWalkers.BorderStyle = BorderStyle.None;
        dgvWalkers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgvWalkers.Font = new Font("Segoe UI", 9F);
        dgvWalkers.Location = new Point(10, 56);
        dgvWalkers.Name = "dgvWalkers";
        dgvWalkers.ReadOnly = true;
        dgvWalkers.RowHeadersVisible = false;
        dgvWalkers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvWalkers.Size = new Size(1005, 525);

        // ═══ ADMIN TAB: Users ═════════════════════════
        tabUsers.BackColor = Color.WhiteSmoke;
        tabUsers.Controls.Add(dgvAllUsers);
        tabUsers.Controls.Add(pnlUsersToolbar);
        tabUsers.Name = "tabUsers";
        tabUsers.Padding = new Padding(3);
        tabUsers.Text = "\U0001f465 Users";
        tabUsers.Enter += TabUsers_Enter;

        pnlUsersToolbar.Controls.Add(btnRefreshUsers);
        pnlUsersToolbar.Location = new Point(10, 8);
        pnlUsersToolbar.Name = "pnlUsersToolbar";
        pnlUsersToolbar.Size = new Size(1000, 38);

        btnRefreshUsers.AutoSize = true;
        btnRefreshUsers.BackColor = Color.FromArgb(30, 70, 150);
        btnRefreshUsers.FlatStyle = FlatStyle.Flat;
        btnRefreshUsers.Font = new Font("Segoe UI", 9F);
        btnRefreshUsers.ForeColor = Color.White;
        btnRefreshUsers.Location = new Point(0, 5);
        btnRefreshUsers.Name = "btnRefreshUsers";
        btnRefreshUsers.Size = new Size(90, 28);
        btnRefreshUsers.Text = "\u21bb Refresh";
        btnRefreshUsers.UseVisualStyleBackColor = false;
        btnRefreshUsers.Click += BtnRefreshUsers_Click;

        dgvAllUsers.AllowUserToAddRows = false;
        dgvAllUsers.AllowUserToDeleteRows = false;
        dgvAllUsers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvAllUsers.BackgroundColor = Color.White;
        dgvAllUsers.BorderStyle = BorderStyle.None;
        dgvAllUsers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgvAllUsers.Font = new Font("Segoe UI", 9F);
        dgvAllUsers.Location = new Point(10, 56);
        dgvAllUsers.Name = "dgvAllUsers";
        dgvAllUsers.ReadOnly = true;
        dgvAllUsers.RowHeadersVisible = false;
        dgvAllUsers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvAllUsers.Size = new Size(1005, 525);

        // ── MainForm ──────────────────────────────────
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
        tabClients.ResumeLayout(false);
        tabWalks.ResumeLayout(false);
        tabWalkers.ResumeLayout(false);
        tabUsers.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvClients).EndInit();
        ((System.ComponentModel.ISupportInitialize)dgvWalks).EndInit();
        ((System.ComponentModel.ISupportInitialize)dgvWalkers).EndInit();
        ((System.ComponentModel.ISupportInitialize)dgvAllUsers).EndInit();
        ResumeLayout(false);
    }

    #endregion

    // Shell
    private Panel pnlTopBar;
    private Label lblAppTitle;
    private Label lblSession;
    private Button btnLogout;
    private TabControl tabs;

    // Admin: Clients & Dogs
    private TabPage tabClients;
    private Panel pnlClientsToolbar;
    private TextBox txtSearchClients;
    private Button btnRefreshClients;
    private DataGridView dgvClients;

    // Admin: Walk Events
    private TabPage tabWalks;
    private Panel pnlWalksToolbar;
    private ComboBox cmbWalkStatus;
    private Button btnNewWalk;
    private DataGridView dgvWalks;

    // Admin: Walkers
    private TabPage tabWalkers;
    private Panel pnlWalkersToolbar;
    private Button btnRefreshWalkers;
    private DataGridView dgvWalkers;

    // Admin: Users
    private TabPage tabUsers;
    private Panel pnlUsersToolbar;
    private Button btnRefreshUsers;
    private DataGridView dgvAllUsers;
}