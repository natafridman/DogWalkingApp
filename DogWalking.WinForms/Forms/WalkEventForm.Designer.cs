namespace DogWalking.WinForms.Forms;

partial class WalkEventForm
{
    private System.ComponentModel.IContainer components = null;

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        lblClient = new Label();
        cmbClient = new ComboBox();
        lblDogs = new Label();
        clbDogs = new CheckedListBox();
        lblSubInfo = new Label();
        lblZone = new Label();
        cmbLocation = new ComboBox();
        lblDateTime = new Label();
        dtpWalkDate = new DateTimePicker();
        lblDuration = new Label();
        numDuration = new NumericUpDown();
        lblNotes = new Label();
        txtNotes = new TextBox();
        lblRecurrence = new Label();
        cmbRecurrence = new ComboBox();
        lblError = new Label();
        btnSave = new Button();
        btnCancel = new Button();
        ((System.ComponentModel.ISupportInitialize)numDuration).BeginInit();
        SuspendLayout();
        // 
        // lblClient
        // 
        lblClient.AutoSize = true;
        lblClient.Location = new Point(16, 19);
        lblClient.Name = "lblClient";
        lblClient.Size = new Size(57, 20);
        lblClient.TabIndex = 0;
        lblClient.Text = "Client *";
        // 
        // cmbClient
        // 
        cmbClient.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbClient.Location = new Point(16, 45);
        cmbClient.Margin = new Padding(3, 4, 3, 4);
        cmbClient.Name = "cmbClient";
        cmbClient.Size = new Size(452, 28);
        cmbClient.TabIndex = 1;
        cmbClient.SelectedIndexChanged += CmbClient_SelectedIndexChanged;
        // 
        // lblDogs
        // 
        lblDogs.AutoSize = true;
        lblDogs.Location = new Point(16, 93);
        lblDogs.Name = "lblDogs";
        lblDogs.Size = new Size(215, 20);
        lblDogs.TabIndex = 2;
        lblDogs.Text = "Dog(s) — check all that apply *";
        // 
        // clbDogs
        // 
        clbDogs.BorderStyle = BorderStyle.FixedSingle;
        clbDogs.CheckOnClick = true;
        clbDogs.Location = new Point(16, 120);
        clbDogs.Margin = new Padding(3, 4, 3, 4);
        clbDogs.Name = "clbDogs";
        clbDogs.Size = new Size(452, 112);
        clbDogs.TabIndex = 3;
        // 
        // lblSubInfo
        // 
        lblSubInfo.Font = new Font("Segoe UI", 8F);
        lblSubInfo.ForeColor = Color.DimGray;
        lblSubInfo.Location = new Point(16, 236);
        lblSubInfo.Name = "lblSubInfo";
        lblSubInfo.Size = new Size(453, 37);
        lblSubInfo.TabIndex = 4;
        lblSubInfo.Text = "Select a client to see subscription limits.";
        // 
        // lblZone
        // 
        lblZone.AutoSize = true;
        lblZone.Location = new Point(16, 301);
        lblZone.Name = "lblZone";
        lblZone.Size = new Size(89, 20);
        lblZone.TabIndex = 5;
        lblZone.Text = "Walk Zone *";
        // 
        // cmbLocation
        // 
        cmbLocation.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbLocation.Location = new Point(16, 328);
        cmbLocation.Margin = new Padding(3, 4, 3, 4);
        cmbLocation.Name = "cmbLocation";
        cmbLocation.Size = new Size(452, 28);
        cmbLocation.TabIndex = 6;
        // 
        // lblDateTime
        // 
        lblDateTime.AutoSize = true;
        lblDateTime.Location = new Point(16, 376);
        lblDateTime.Name = "lblDateTime";
        lblDateTime.Size = new Size(120, 20);
        lblDateTime.TabIndex = 7;
        lblDateTime.Text = "Walk DateTime *";
        // 
        // dtpWalkDate
        // 
        dtpWalkDate.CustomFormat = "yyyy-MM-dd  HH:mm";
        dtpWalkDate.Format = DateTimePickerFormat.Custom;
        dtpWalkDate.Location = new Point(16, 403);
        dtpWalkDate.Margin = new Padding(3, 4, 3, 4);
        dtpWalkDate.Name = "dtpWalkDate";
        dtpWalkDate.Size = new Size(452, 27);
        dtpWalkDate.TabIndex = 8;
        dtpWalkDate.Value = new DateTime(2026, 2, 26, 18, 24, 39, 853);
        // 
        // lblDuration
        // 
        lblDuration.AutoSize = true;
        lblDuration.Location = new Point(16, 451);
        lblDuration.Name = "lblDuration";
        lblDuration.Size = new Size(143, 20);
        lblDuration.TabIndex = 9;
        lblDuration.Text = "Duration (minutes) *";
        // 
        // numDuration
        // 
        numDuration.Increment = new decimal(new int[] { 15, 0, 0, 0 });
        numDuration.Location = new Point(16, 477);
        numDuration.Margin = new Padding(3, 4, 3, 4);
        numDuration.Maximum = new decimal(new int[] { 480, 0, 0, 0 });
        numDuration.Minimum = new decimal(new int[] { 15, 0, 0, 0 });
        numDuration.Name = "numDuration";
        numDuration.Size = new Size(137, 27);
        numDuration.TabIndex = 10;
        numDuration.Value = new decimal(new int[] { 60, 0, 0, 0 });
        // 
        // lblNotes
        // 
        lblNotes.AutoSize = true;
        lblNotes.Location = new Point(17, 591);
        lblNotes.Name = "lblNotes";
        lblNotes.Size = new Size(48, 20);
        lblNotes.TabIndex = 11;
        lblNotes.Text = "Notes";
        // 
        // txtNotes
        // 
        txtNotes.Location = new Point(17, 618);
        txtNotes.Margin = new Padding(3, 4, 3, 4);
        txtNotes.Multiline = true;
        txtNotes.Name = "txtNotes";
        txtNotes.Size = new Size(452, 65);
        txtNotes.TabIndex = 12;
        // 
        // lblRecurrence
        // 
        lblRecurrence.AutoSize = true;
        lblRecurrence.Location = new Point(16, 519);
        lblRecurrence.Name = "lblRecurrence";
        lblRecurrence.Size = new Size(82, 20);
        lblRecurrence.TabIndex = 13;
        lblRecurrence.Text = "Recurrence";
        // 
        // cmbRecurrence
        // 
        cmbRecurrence.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbRecurrence.Location = new Point(16, 546);
        cmbRecurrence.Margin = new Padding(3, 4, 3, 4);
        cmbRecurrence.Name = "cmbRecurrence";
        cmbRecurrence.Size = new Size(452, 28);
        cmbRecurrence.TabIndex = 14;
        // 
        // lblError
        // 
        lblError.ForeColor = Color.Crimson;
        lblError.Location = new Point(16, 707);
        lblError.Name = "lblError";
        lblError.Size = new Size(453, 48);
        lblError.TabIndex = 15;
        lblError.Visible = false;
        //
        // btnSave
        //
        btnSave.BackColor = Color.FromArgb(30, 70, 150);
        btnSave.FlatStyle = FlatStyle.Flat;
        btnSave.Font = new Font("Segoe UI", 10F);
        btnSave.ForeColor = Color.White;
        btnSave.Location = new Point(20, 772);
        btnSave.Margin = new Padding(3, 4, 3, 4);
        btnSave.Name = "btnSave";
        btnSave.Size = new Size(185, 43);
        btnSave.TabIndex = 16;
        btnSave.Text = "Submit Walk Request";
        btnSave.UseVisualStyleBackColor = false;
        btnSave.Click += BtnSave_Click;
        //
        // btnClear
        //
        btnClear = new Button();
        btnClear.FlatStyle = FlatStyle.Flat;
        btnClear.Font = new Font("Segoe UI", 10F);
        btnClear.Location = new Point(215, 772);
        btnClear.Margin = new Padding(3, 4, 3, 4);
        btnClear.Name = "btnClear";
        btnClear.Size = new Size(110, 43);
        btnClear.TabIndex = 17;
        btnClear.Text = "Clear";
        btnClear.Click += BtnClear_Click;
        //
        // btnCancel
        //
        btnCancel.FlatStyle = FlatStyle.Flat;
        btnCancel.Location = new Point(335, 772);
        btnCancel.Margin = new Padding(3, 4, 3, 4);
        btnCancel.Name = "btnCancel";
        btnCancel.Size = new Size(130, 43);
        btnCancel.TabIndex = 18;
        btnCancel.Text = "Cancel";
        btnCancel.Click += BtnCancel_Click;
        // 
        // WalkEventForm
        // 
        AcceptButton = btnSave;
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.WhiteSmoke;
        ClientSize = new Size(485, 839);
        Controls.Add(lblClient);
        Controls.Add(cmbClient);
        Controls.Add(lblDogs);
        Controls.Add(clbDogs);
        Controls.Add(lblSubInfo);
        Controls.Add(lblZone);
        Controls.Add(cmbLocation);
        Controls.Add(lblDateTime);
        Controls.Add(dtpWalkDate);
        Controls.Add(lblDuration);
        Controls.Add(numDuration);
        Controls.Add(lblNotes);
        Controls.Add(txtNotes);
        Controls.Add(lblRecurrence);
        Controls.Add(cmbRecurrence);
        Controls.Add(lblError);
        Controls.Add(btnSave);
        Controls.Add(btnClear);
        Controls.Add(btnCancel);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        Margin = new Padding(3, 4, 3, 4);
        MaximizeBox = false;
        Name = "WalkEventForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Request Walk";
        ((System.ComponentModel.ISupportInitialize)numDuration).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label lblClient;
    private ComboBox cmbClient;
    private Label lblDogs;
    private CheckedListBox clbDogs;
    private Label lblSubInfo;
    private Label lblZone;
    private ComboBox cmbLocation;
    private Label lblDateTime;
    private DateTimePicker dtpWalkDate;
    private Label lblDuration;
    private NumericUpDown numDuration;
    private Label lblNotes;
    private TextBox txtNotes;
    private Label lblRecurrence;
    private ComboBox cmbRecurrence;
    private Label lblError;
    private Button btnSave;
    private Button btnClear;
    private Button btnCancel;
}
