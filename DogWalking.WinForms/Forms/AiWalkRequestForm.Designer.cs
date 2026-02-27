namespace DogWalking.WinForms.Forms;

partial class AiWalkRequestForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        lblTitle = new Label();
        lblInstruction = new Label();
        txtInput = new TextBox();
        btnParse = new Button();
        pnlResult = new Panel();
        lblResultTitle = new Label();
        txtResult = new TextBox();
        btnConfirm = new Button();
        btnCancel = new Button();
        lblStatus = new Label();

        pnlResult.SuspendLayout();
        SuspendLayout();

        // ── lblTitle ────────────────────────────────────────────
        lblTitle.AutoSize = true;
        lblTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
        lblTitle.ForeColor = Color.FromArgb(30, 70, 150);
        lblTitle.Location = new Point(16, 12);
        lblTitle.Name = "lblTitle";
        lblTitle.Text = "\U0001f916 AI Walk Request";

        // ── lblInstruction ──────────────────────────────────────
        lblInstruction.AutoSize = true;
        lblInstruction.Font = new Font("Segoe UI", 9F);
        lblInstruction.ForeColor = Color.DimGray;
        lblInstruction.Location = new Point(16, 44);
        lblInstruction.Name = "lblInstruction";
        lblInstruction.MaximumSize = new Size(520, 0);
        lblInstruction.Text = "Describe your walk schedule in natural language. Example:\n" +
            "\"I want my dog Rocky to have walks on Mondays and Tuesdays at 9am and 6pm in Palermo\"";

        // ── txtInput ────────────────────────────────────────────
        txtInput.Font = new Font("Segoe UI", 10F);
        txtInput.Location = new Point(16, 95);
        txtInput.Multiline = true;
        txtInput.Name = "txtInput";
        txtInput.ScrollBars = ScrollBars.Vertical;
        txtInput.Size = new Size(520, 80);
        txtInput.PlaceholderText = "Type your walk request here...";

        // ── btnParse ────────────────────────────────────────────
        btnParse.AutoSize = true;
        btnParse.BackColor = Color.FromArgb(30, 70, 150);
        btnParse.FlatStyle = FlatStyle.Flat;
        btnParse.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnParse.ForeColor = Color.White;
        btnParse.Location = new Point(16, 185);
        btnParse.Name = "btnParse";
        btnParse.Size = new Size(160, 32);
        btnParse.Text = "\U0001f9e0 Parse with AI";
        btnParse.UseVisualStyleBackColor = false;
        btnParse.Click += BtnParse_Click;

        // ── lblStatus ───────────────────────────────────────────
        lblStatus.AutoSize = true;
        lblStatus.Font = new Font("Segoe UI", 9F);
        lblStatus.ForeColor = Color.DimGray;
        lblStatus.Location = new Point(185, 192);
        lblStatus.Name = "lblStatus";
        lblStatus.Text = "";

        // ── pnlResult (shown after successful parse) ────────────
        pnlResult.Controls.Add(lblResultTitle);
        pnlResult.Controls.Add(txtResult);
        pnlResult.Controls.Add(btnConfirm);
        pnlResult.Controls.Add(btnCancel);
        pnlResult.Location = new Point(16, 225);
        pnlResult.Name = "pnlResult";
        pnlResult.Size = new Size(520, 290);
        pnlResult.Visible = false;

        // lblResultTitle
        lblResultTitle.AutoSize = true;
        lblResultTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblResultTitle.ForeColor = Color.FromArgb(30, 70, 150);
        lblResultTitle.Location = new Point(0, 4);
        lblResultTitle.Name = "lblResultTitle";
        lblResultTitle.Text = "AI parsed your request. Please review:";

        // txtResult
        txtResult.Font = new Font("Consolas", 9F);
        txtResult.Location = new Point(0, 30);
        txtResult.Multiline = true;
        txtResult.Name = "txtResult";
        txtResult.ReadOnly = true;
        txtResult.ScrollBars = ScrollBars.Vertical;
        txtResult.Size = new Size(520, 210);
        txtResult.BackColor = Color.White;

        // btnConfirm
        btnConfirm.BackColor = Color.SeaGreen;
        btnConfirm.FlatStyle = FlatStyle.Flat;
        btnConfirm.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnConfirm.ForeColor = Color.White;
        btnConfirm.Location = new Point(0, 250);
        btnConfirm.Name = "btnConfirm";
        btnConfirm.Size = new Size(180, 32);
        btnConfirm.Text = "\u2705 Confirm & Create";
        btnConfirm.UseVisualStyleBackColor = false;
        btnConfirm.Click += BtnConfirm_Click;

        // btnCancel
        btnCancel.BackColor = Color.FromArgb(180, 60, 60);
        btnCancel.FlatStyle = FlatStyle.Flat;
        btnCancel.Font = new Font("Segoe UI", 9F);
        btnCancel.ForeColor = Color.White;
        btnCancel.Location = new Point(195, 250);
        btnCancel.Name = "btnCancel";
        btnCancel.Size = new Size(120, 32);
        btnCancel.Text = "Cancel";
        btnCancel.UseVisualStyleBackColor = false;
        btnCancel.Click += BtnCancel_Click;

        // ── AiWalkRequestForm ───────────────────────────────────
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.WhiteSmoke;
        ClientSize = new Size(555, 530);
        Controls.Add(lblTitle);
        Controls.Add(lblInstruction);
        Controls.Add(txtInput);
        Controls.Add(btnParse);
        Controls.Add(lblStatus);
        Controls.Add(pnlResult);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "AiWalkRequestForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "AI Walk Request";

        pnlResult.ResumeLayout(false);
        pnlResult.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label lblTitle;
    private Label lblInstruction;
    private TextBox txtInput;
    private Button btnParse;
    private Label lblStatus;
    private Panel pnlResult;
    private Label lblResultTitle;
    private TextBox txtResult;
    private Button btnConfirm;
    private Button btnCancel;
}
