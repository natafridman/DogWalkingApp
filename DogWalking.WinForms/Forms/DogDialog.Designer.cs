namespace DogWalking.WinForms.Forms;

partial class DogDialog
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
        lblName = new Label();
        txtName = new TextBox();
        lblBreed = new Label();
        cmbBreed = new ComboBox();
        lblBirthDate = new Label();
        dtpBirthDate = new DateTimePicker();
        lblError = new Label();
        btnSave = new Button();
        btnCancel = new Button();
        SuspendLayout();

        // lblName
        lblName.AutoSize = true;
        lblName.Location = new Point(20, 20);
        lblName.Name = "lblName";
        lblName.Text = "Name *";

        // txtName
        txtName.Location = new Point(20, 40);
        txtName.Name = "txtName";
        txtName.Size = new Size(300, 23);

        // lblBreed
        lblBreed.AutoSize = true;
        lblBreed.Location = new Point(20, 78);
        lblBreed.Name = "lblBreed";
        lblBreed.Text = "Breed *";

        // cmbBreed
        cmbBreed.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbBreed.FormattingEnabled = true;
        cmbBreed.Location = new Point(20, 98);
        cmbBreed.Name = "cmbBreed";
        cmbBreed.Size = new Size(300, 23);

        // lblBirthDate
        lblBirthDate.AutoSize = true;
        lblBirthDate.Location = new Point(20, 136);
        lblBirthDate.Name = "lblBirthDate";
        lblBirthDate.Text = "Birth Date *";

        // dtpBirthDate
        dtpBirthDate.Format = DateTimePickerFormat.Short;
        dtpBirthDate.Location = new Point(20, 156);
        dtpBirthDate.MaxDate = DateTime.Today;
        dtpBirthDate.MinDate = DateTime.Today.AddYears(-30);
        dtpBirthDate.Name = "dtpBirthDate";
        dtpBirthDate.Size = new Size(160, 23);
        dtpBirthDate.Value = DateTime.Today.AddYears(-1);

        // lblError
        lblError.ForeColor = Color.Crimson;
        lblError.Location = new Point(20, 190);
        lblError.Name = "lblError";
        lblError.Size = new Size(300, 30);
        lblError.Visible = false;

        // btnSave
        btnSave.BackColor = Color.FromArgb(30, 70, 150);
        btnSave.FlatStyle = FlatStyle.Flat;
        btnSave.ForeColor = Color.White;
        btnSave.Location = new Point(20, 225);
        btnSave.Name = "btnSave";
        btnSave.Size = new Size(95, 28);
        btnSave.Text = "Save";
        btnSave.UseVisualStyleBackColor = false;
        btnSave.Click += BtnSave_Click;

        // btnClear
        btnClear = new Button();
        btnClear.FlatStyle = FlatStyle.Flat;
        btnClear.Location = new Point(125, 225);
        btnClear.Name = "btnClear";
        btnClear.Size = new Size(85, 28);
        btnClear.Text = "Clear";
        btnClear.Click += BtnClear_Click;

        // btnCancel
        btnCancel.FlatStyle = FlatStyle.Flat;
        btnCancel.Location = new Point(220, 225);
        btnCancel.Name = "btnCancel";
        btnCancel.Size = new Size(95, 28);
        btnCancel.Text = "Cancel";
        btnCancel.Click += BtnCancel_Click;

        // DogDialog
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.WhiteSmoke;
        ClientSize = new Size(344, 281);
        Controls.Add(lblName);
        Controls.Add(txtName);
        Controls.Add(lblBreed);
        Controls.Add(cmbBreed);
        Controls.Add(lblBirthDate);
        Controls.Add(dtpBirthDate);
        Controls.Add(lblError);
        Controls.Add(btnSave);
        Controls.Add(btnClear);
        Controls.Add(btnCancel);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "DogDialog";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Add Dog";
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label lblName;
    private TextBox txtName;
    private Label lblBreed;
    private ComboBox cmbBreed;
    private Label lblBirthDate;
    private DateTimePicker dtpBirthDate;
    private Label lblError;
    private Button btnSave;
    private Button btnClear;
    private Button btnCancel;
}
