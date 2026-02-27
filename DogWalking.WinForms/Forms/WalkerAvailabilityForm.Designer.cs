namespace DogWalking.WinForms.Forms;

partial class WalkerAvailabilityForm
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
        lblError = new Label();
        lblHeader = new Label();
        gridSlots = new DataGridView();
        colId = new DataGridViewTextBoxColumn();
        colDay = new DataGridViewTextBoxColumn();
        colStart = new DataGridViewTextBoxColumn();
        colEnd = new DataGridViewTextBoxColumn();
        colDuration = new DataGridViewTextBoxColumn();
        colZone = new DataGridViewTextBoxColumn();
        pnlAddSlot = new Panel();
        lblDay = new Label();
        cmbDay = new ComboBox();
        lblFrom = new Label();
        dtpStart = new DateTimePicker();
        lblUntil = new Label();
        dtpEnd = new DateTimePicker();
        lblZone = new Label();
        cmbZone = new ComboBox();
        btnAddSlot = new Button();
        btnDelSlot = new Button();
        btnClose = new Button();
        ((System.ComponentModel.ISupportInitialize)gridSlots).BeginInit();
        pnlAddSlot.SuspendLayout();
        SuspendLayout();

        // lblError
        lblError.ForeColor = Color.Crimson;
        lblError.Location = new Point(12, 12);
        lblError.Name = "lblError";
        lblError.Size = new Size(640, 20);
        lblError.Visible = false;

        // lblHeader
        lblHeader.AutoSize = true;
        lblHeader.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblHeader.ForeColor = Color.FromArgb(30, 70, 150);
        lblHeader.Location = new Point(12, 36);
        lblHeader.Name = "lblHeader";
        lblHeader.Text = "Availability Windows";

        // gridSlots
        gridSlots.AllowUserToAddRows = false;
        gridSlots.BackgroundColor = Color.White;
        gridSlots.BorderStyle = BorderStyle.FixedSingle;
        gridSlots.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        gridSlots.Columns.AddRange(new DataGridViewColumn[] { colId, colDay, colStart, colEnd, colDuration, colZone });
        gridSlots.Location = new Point(12, 58);
        gridSlots.Name = "gridSlots";
        gridSlots.ReadOnly = true;
        gridSlots.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        gridSlots.Size = new Size(646, 180);

        // colId
        colId.HeaderText = "Id";
        colId.Name = "Id";
        colId.Visible = false;

        // colDay
        colDay.HeaderText = "Day";
        colDay.Name = "Day";
        colDay.Width = 110;

        // colStart
        colStart.HeaderText = "From";
        colStart.Name = "Start";
        colStart.Width = 80;

        // colEnd
        colEnd.HeaderText = "Until";
        colEnd.Name = "End";
        colEnd.Width = 80;

        // colDuration
        colDuration.HeaderText = "Hours";
        colDuration.Name = "Duration";
        colDuration.Width = 70;

        // colZone
        colZone.HeaderText = "Zone";
        colZone.Name = "Zone";
        colZone.Width = 160;

        // pnlAddSlot
        pnlAddSlot.Controls.Add(lblDay);
        pnlAddSlot.Controls.Add(cmbDay);
        pnlAddSlot.Controls.Add(lblFrom);
        pnlAddSlot.Controls.Add(dtpStart);
        pnlAddSlot.Controls.Add(lblUntil);
        pnlAddSlot.Controls.Add(dtpEnd);
        pnlAddSlot.Controls.Add(lblZone);
        pnlAddSlot.Controls.Add(cmbZone);
        pnlAddSlot.Controls.Add(btnAddSlot);
        pnlAddSlot.Controls.Add(btnDelSlot);
        pnlAddSlot.Location = new Point(12, 246);
        pnlAddSlot.Name = "pnlAddSlot";
        pnlAddSlot.Size = new Size(646, 80);

        // lblDay
        lblDay.AutoSize = true;
        lblDay.Location = new Point(0, 8);
        lblDay.Name = "lblDay";
        lblDay.Text = "Day:";

        // cmbDay
        cmbDay.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbDay.Location = new Point(40, 4);
        cmbDay.Name = "cmbDay";
        cmbDay.Size = new Size(120, 23);

        // lblFrom
        lblFrom.AutoSize = true;
        lblFrom.Location = new Point(170, 8);
        lblFrom.Name = "lblFrom";
        lblFrom.Text = "From:";

        // dtpStart
        dtpStart.Format = DateTimePickerFormat.Time;
        dtpStart.Location = new Point(210, 4);
        dtpStart.Name = "dtpStart";
        dtpStart.ShowUpDown = true;
        dtpStart.Size = new Size(100, 23);
        dtpStart.Value = DateTime.Today.AddHours(8);

        // lblUntil
        lblUntil.AutoSize = true;
        lblUntil.Location = new Point(318, 8);
        lblUntil.Name = "lblUntil";
        lblUntil.Text = "Until:";

        // dtpEnd
        dtpEnd.Format = DateTimePickerFormat.Time;
        dtpEnd.Location = new Point(358, 4);
        dtpEnd.Name = "dtpEnd";
        dtpEnd.ShowUpDown = true;
        dtpEnd.Size = new Size(100, 23);
        dtpEnd.Value = DateTime.Today.AddHours(12);

        // lblZone
        lblZone.AutoSize = true;
        lblZone.Location = new Point(0, 46);
        lblZone.Name = "lblZone";
        lblZone.Text = "Zone:";

        // cmbZone
        cmbZone.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbZone.Location = new Point(50, 42);
        cmbZone.Name = "cmbZone";
        cmbZone.Size = new Size(200, 23);

        // btnAddSlot
        btnAddSlot.BackColor = Color.FromArgb(30, 120, 60);
        btnAddSlot.FlatStyle = FlatStyle.Flat;
        btnAddSlot.ForeColor = Color.White;
        btnAddSlot.Location = new Point(260, 40);
        btnAddSlot.Name = "btnAddSlot";
        btnAddSlot.Size = new Size(110, 28);
        btnAddSlot.Text = "+ Add Slot";
        btnAddSlot.UseVisualStyleBackColor = false;
        btnAddSlot.Click += BtnAddSlot_Click;

        // btnDelSlot
        btnDelSlot.FlatStyle = FlatStyle.Flat;
        btnDelSlot.Location = new Point(378, 40);
        btnDelSlot.Name = "btnDelSlot";
        btnDelSlot.Size = new Size(140, 28);
        btnDelSlot.Text = "Remove Selected";
        btnDelSlot.Click += BtnDelSlot_Click;

        // btnClose
        btnClose.FlatStyle = FlatStyle.Flat;
        btnClose.Location = new Point(564, 346);
        btnClose.Name = "btnClose";
        btnClose.Size = new Size(92, 30);
        btnClose.Text = "Close";
        btnClose.Click += BtnClose_Click;

        // WalkerAvailabilityForm
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.WhiteSmoke;
        ClientSize = new Size(670, 390);
        Controls.Add(lblError);
        Controls.Add(lblHeader);
        Controls.Add(gridSlots);
        Controls.Add(pnlAddSlot);
        Controls.Add(btnClose);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        Name = "WalkerAvailabilityForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Availability";
        ((System.ComponentModel.ISupportInitialize)gridSlots).EndInit();
        pnlAddSlot.ResumeLayout(false);
        pnlAddSlot.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label lblError;
    private Label lblHeader;
    private DataGridView gridSlots;
    private DataGridViewTextBoxColumn colId;
    private DataGridViewTextBoxColumn colDay;
    private DataGridViewTextBoxColumn colStart;
    private DataGridViewTextBoxColumn colEnd;
    private DataGridViewTextBoxColumn colDuration;
    private DataGridViewTextBoxColumn colZone;
    private Panel pnlAddSlot;
    private Label lblDay;
    private ComboBox cmbDay;
    private Label lblFrom;
    private DateTimePicker dtpStart;
    private Label lblUntil;
    private DateTimePicker dtpEnd;
    private Label lblZone;
    private ComboBox cmbZone;
    private Button btnAddSlot;
    private Button btnDelSlot;
    private Button btnClose;
}
