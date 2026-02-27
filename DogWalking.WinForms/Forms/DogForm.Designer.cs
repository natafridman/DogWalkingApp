namespace DogWalking.WinForms.Forms;

partial class DogForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        lblClient = new Label();
        btnAddDog = new Button();
        dgvDogs = new DataGridView();
        ctxDogs = new ContextMenuStrip();
        tsmiEdit = new ToolStripMenuItem();
        tsmiDelete = new ToolStripMenuItem();
        ((System.ComponentModel.ISupportInitialize)dgvDogs).BeginInit();
        ctxDogs.SuspendLayout();
        SuspendLayout();

        // lblClient
        lblClient.AutoSize = true;
        lblClient.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        lblClient.ForeColor = Color.FromArgb(30, 70, 150);
        lblClient.Location = new Point(12, 12);
        lblClient.Name = "lblClient";
        lblClient.Text = "Dogs for:";

        // btnAddDog
        btnAddDog.AutoSize = true;
        btnAddDog.BackColor = Color.FromArgb(30, 70, 150);
        btnAddDog.FlatStyle = FlatStyle.Flat;
        btnAddDog.Font = new Font("Segoe UI", 9F);
        btnAddDog.ForeColor = Color.White;
        btnAddDog.Location = new Point(12, 46);
        btnAddDog.Name = "btnAddDog";
        btnAddDog.Size = new Size(100, 30);
        btnAddDog.Text = "+ Add Dog";
        btnAddDog.UseVisualStyleBackColor = false;
        btnAddDog.Click += BtnAddDog_Click;

        // dgvDogs
        dgvDogs.AllowUserToAddRows = false;
        dgvDogs.AllowUserToDeleteRows = false;
        dgvDogs.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvDogs.BackgroundColor = Color.White;
        dgvDogs.ContextMenuStrip = ctxDogs;
        dgvDogs.Font = new Font("Segoe UI", 9F);
        dgvDogs.Location = new Point(12, 86);
        dgvDogs.Name = "dgvDogs";
        dgvDogs.ReadOnly = true;
        dgvDogs.RowHeadersVisible = false;
        dgvDogs.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvDogs.Size = new Size(640, 350);

        // ctxDogs
        ctxDogs.Items.AddRange(new ToolStripItem[] { tsmiEdit, tsmiDelete });
        ctxDogs.Name = "ctxDogs";

        // tsmiEdit
        tsmiEdit.Name = "tsmiEdit";
        tsmiEdit.Text = "Edit";
        tsmiEdit.Click += TsmiEdit_Click;

        // tsmiDelete
        tsmiDelete.Name = "tsmiDelete";
        tsmiDelete.Text = "Delete";
        tsmiDelete.Click += TsmiDelete_Click;

        // DogForm
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.WhiteSmoke;
        ClientSize = new Size(668, 448);
        Controls.Add(lblClient);
        Controls.Add(btnAddDog);
        Controls.Add(dgvDogs);
        Name = "DogForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Manage Dogs";
        ((System.ComponentModel.ISupportInitialize)dgvDogs).EndInit();
        ctxDogs.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label lblClient;
    private Button btnAddDog;
    private DataGridView dgvDogs;
    private ContextMenuStrip ctxDogs;
    private ToolStripMenuItem tsmiEdit;
    private ToolStripMenuItem tsmiDelete;
}
