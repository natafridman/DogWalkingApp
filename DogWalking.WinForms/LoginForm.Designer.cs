namespace DogWalking.WinForms.Forms;

partial class LoginForm
{
    private System.ComponentModel.IContainer components = null;

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        lblTitle = new Label();
        lblUsername = new Label();
        txtUser = new TextBox();
        lblPassword = new Label();
        txtPass = new TextBox();
        btnLogin = new Button();
        lblErr = new Label();
        pnlDivider = new Panel();
        lblNewAccount = new Label();
        lblAccountType = new Label();
        btnWalkerType = new Button();
        btnClientType = new Button();
        lblRegName = new Label();
        txtRegName = new TextBox();
        lblRegUsername = new Label();
        txtRegUser = new TextBox();
        lblRegPassword = new Label();
        txtRegPass = new TextBox();
        lblRegConfirm = new Label();
        txtRegConfirm = new TextBox();
        pnlWalkerExtra = new Panel();
        lblWalkerPhone = new Label();
        txtWalkerPhone = new TextBox();
        lblWalkerEmail = new Label();
        txtWalkerEmail = new TextBox();
        pnlClientExtra = new Panel();
        lblClientPhone = new Label();
        txtRegPhone = new TextBox();
        lblClientEmail = new Label();
        txtRegEmail = new TextBox();
        lblAddress = new Label();
        txtRegAddress = new TextBox();
        lblSubscription = new Label();
        cmbSub = new ComboBox();
        btnRegister = new Button();
        lblRegStatus = new Label();
        pnlWalkerExtra.SuspendLayout();
        pnlClientExtra.SuspendLayout();
        SuspendLayout();
        // 
        // lblTitle
        // 
        lblTitle.AutoSize = true;
        lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        lblTitle.ForeColor = Color.FromArgb(30, 70, 150);
        lblTitle.Location = new Point(88, 52);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(312, 32);
        lblTitle.TabIndex = 0;
        lblTitle.Text = "🐾 Dog Walking Manager";
        // 
        // lblUsername
        // 
        lblUsername.AutoSize = true;
        lblUsername.Location = new Point(70, 136);
        lblUsername.Name = "lblUsername";
        lblUsername.Size = new Size(78, 20);
        lblUsername.TabIndex = 1;
        lblUsername.Text = "Username:";
        // 
        // txtUser
        // 
        txtUser.Location = new Point(70, 160);
        txtUser.Margin = new Padding(3, 4, 3, 4);
        txtUser.Name = "txtUser";
        txtUser.Size = new Size(342, 27);
        txtUser.TabIndex = 2;
        // 
        // lblPassword
        // 
        lblPassword.AutoSize = true;
        lblPassword.Location = new Point(70, 214);
        lblPassword.Name = "lblPassword";
        lblPassword.Size = new Size(73, 20);
        lblPassword.TabIndex = 3;
        lblPassword.Text = "Password:";
        // 
        // txtPass
        // 
        txtPass.Location = new Point(70, 238);
        txtPass.Margin = new Padding(3, 4, 3, 4);
        txtPass.Name = "txtPass";
        txtPass.Size = new Size(342, 27);
        txtPass.TabIndex = 4;
        txtPass.UseSystemPasswordChar = true;
        // 
        // btnLogin
        // 
        btnLogin.BackColor = Color.FromArgb(30, 70, 150);
        btnLogin.FlatAppearance.BorderSize = 0;
        btnLogin.FlatStyle = FlatStyle.Flat;
        btnLogin.Font = new Font("Segoe UI", 10F);
        btnLogin.ForeColor = Color.White;
        btnLogin.Location = new Point(70, 312);
        btnLogin.Margin = new Padding(3, 4, 3, 4);
        btnLogin.Name = "btnLogin";
        btnLogin.Size = new Size(343, 45);
        btnLogin.TabIndex = 5;
        btnLogin.Text = "Login";
        btnLogin.UseVisualStyleBackColor = false;
        // 
        // lblErr
        // 
        lblErr.ForeColor = Color.Crimson;
        lblErr.Location = new Point(70, 358);
        lblErr.Name = "lblErr";
        lblErr.Size = new Size(354, 53);
        lblErr.TabIndex = 6;
        lblErr.Visible = false;
        // 
        // pnlDivider
        // 
        pnlDivider.BackColor = Color.FromArgb(180, 180, 180);
        pnlDivider.Location = new Point(501, 20);
        pnlDivider.Margin = new Padding(3, 4, 3, 4);
        pnlDivider.Name = "pnlDivider";
        pnlDivider.Size = new Size(1, 800);
        pnlDivider.TabIndex = 7;
        // 
        // lblNewAccount
        // 
        lblNewAccount.AutoSize = true;
        lblNewAccount.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        lblNewAccount.ForeColor = Color.FromArgb(30, 70, 150);
        lblNewAccount.Location = new Point(523, 24);
        lblNewAccount.Name = "lblNewAccount";
        lblNewAccount.Size = new Size(139, 28);
        lblNewAccount.TabIndex = 8;
        lblNewAccount.Text = "New Account";
        // 
        // lblAccountType
        // 
        lblAccountType.AutoSize = true;
        lblAccountType.Location = new Point(523, 80);
        lblAccountType.Name = "lblAccountType";
        lblAccountType.Size = new Size(174, 20);
        lblAccountType.TabIndex = 9;
        lblAccountType.Text = "Select your account type:";
        // 
        // btnWalkerType
        // 
        btnWalkerType.BackColor = Color.FromArgb(30, 70, 150);
        btnWalkerType.FlatAppearance.BorderSize = 0;
        btnWalkerType.FlatStyle = FlatStyle.Flat;
        btnWalkerType.Font = new Font("Segoe UI", 8.5F);
        btnWalkerType.ForeColor = Color.White;
        btnWalkerType.Location = new Point(523, 104);
        btnWalkerType.Margin = new Padding(3, 4, 3, 4);
        btnWalkerType.Name = "btnWalkerType";
        btnWalkerType.Size = new Size(169, 40);
        btnWalkerType.TabIndex = 10;
        btnWalkerType.Text = "Dog Walker";
        btnWalkerType.UseVisualStyleBackColor = false;
        // 
        // btnClientType
        // 
        btnClientType.BackColor = Color.FromArgb(220, 220, 220);
        btnClientType.FlatAppearance.BorderSize = 0;
        btnClientType.FlatStyle = FlatStyle.Flat;
        btnClientType.Font = new Font("Segoe UI", 8.5F);
        btnClientType.ForeColor = Color.FromArgb(70, 70, 70);
        btnClientType.Location = new Point(697, 104);
        btnClientType.Margin = new Padding(3, 4, 3, 4);
        btnClientType.Name = "btnClientType";
        btnClientType.Size = new Size(169, 40);
        btnClientType.TabIndex = 11;
        btnClientType.Text = "Dog Owner";
        btnClientType.UseVisualStyleBackColor = false;
        // 
        // lblRegName
        // 
        lblRegName.AutoSize = true;
        lblRegName.Location = new Point(523, 163);
        lblRegName.Name = "lblRegName";
        lblRegName.Size = new Size(79, 20);
        lblRegName.TabIndex = 12;
        lblRegName.Text = "Full Name:";
        // 
        // txtRegName
        // 
        txtRegName.Location = new Point(523, 187);
        txtRegName.Margin = new Padding(3, 4, 3, 4);
        txtRegName.Name = "txtRegName";
        txtRegName.Size = new Size(342, 27);
        txtRegName.TabIndex = 13;
        // 
        // lblRegUsername
        // 
        lblRegUsername.AutoSize = true;
        lblRegUsername.Location = new Point(523, 237);
        lblRegUsername.Name = "lblRegUsername";
        lblRegUsername.Size = new Size(78, 20);
        lblRegUsername.TabIndex = 14;
        lblRegUsername.Text = "Username:";
        // 
        // txtRegUser
        // 
        txtRegUser.Location = new Point(523, 261);
        txtRegUser.Margin = new Padding(3, 4, 3, 4);
        txtRegUser.Name = "txtRegUser";
        txtRegUser.Size = new Size(342, 27);
        txtRegUser.TabIndex = 15;
        // 
        // lblRegPassword
        // 
        lblRegPassword.AutoSize = true;
        lblRegPassword.Location = new Point(523, 312);
        lblRegPassword.Name = "lblRegPassword";
        lblRegPassword.Size = new Size(73, 20);
        lblRegPassword.TabIndex = 16;
        lblRegPassword.Text = "Password:";
        // 
        // txtRegPass
        // 
        txtRegPass.Location = new Point(523, 336);
        txtRegPass.Margin = new Padding(3, 4, 3, 4);
        txtRegPass.Name = "txtRegPass";
        txtRegPass.Size = new Size(342, 27);
        txtRegPass.TabIndex = 17;
        txtRegPass.UseSystemPasswordChar = true;
        // 
        // lblRegConfirm
        // 
        lblRegConfirm.AutoSize = true;
        lblRegConfirm.Location = new Point(523, 387);
        lblRegConfirm.Name = "lblRegConfirm";
        lblRegConfirm.Size = new Size(130, 20);
        lblRegConfirm.TabIndex = 18;
        lblRegConfirm.Text = "Confirm Password:";
        // 
        // txtRegConfirm
        // 
        txtRegConfirm.Location = new Point(523, 411);
        txtRegConfirm.Margin = new Padding(3, 4, 3, 4);
        txtRegConfirm.Name = "txtRegConfirm";
        txtRegConfirm.Size = new Size(342, 27);
        txtRegConfirm.TabIndex = 19;
        txtRegConfirm.UseSystemPasswordChar = true;
        // 
        // pnlWalkerExtra
        // 
        pnlWalkerExtra.Controls.Add(lblWalkerPhone);
        pnlWalkerExtra.Controls.Add(txtWalkerPhone);
        pnlWalkerExtra.Controls.Add(lblWalkerEmail);
        pnlWalkerExtra.Controls.Add(txtWalkerEmail);
        pnlWalkerExtra.Location = new Point(523, 456);
        pnlWalkerExtra.Margin = new Padding(3, 4, 3, 4);
        pnlWalkerExtra.Name = "pnlWalkerExtra";
        pnlWalkerExtra.Size = new Size(347, 141);
        pnlWalkerExtra.TabIndex = 20;
        // 
        // lblWalkerPhone
        // 
        lblWalkerPhone.AutoSize = true;
        lblWalkerPhone.Location = new Point(0, 0);
        lblWalkerPhone.Name = "lblWalkerPhone";
        lblWalkerPhone.Size = new Size(53, 20);
        lblWalkerPhone.TabIndex = 0;
        lblWalkerPhone.Text = "Phone:";
        // 
        // txtWalkerPhone
        // 
        txtWalkerPhone.Location = new Point(0, 24);
        txtWalkerPhone.Margin = new Padding(3, 4, 3, 4);
        txtWalkerPhone.Name = "txtWalkerPhone";
        txtWalkerPhone.Size = new Size(342, 27);
        txtWalkerPhone.TabIndex = 1;
        // 
        // lblWalkerEmail
        // 
        lblWalkerEmail.AutoSize = true;
        lblWalkerEmail.Location = new Point(0, 73);
        lblWalkerEmail.Name = "lblWalkerEmail";
        lblWalkerEmail.Size = new Size(49, 20);
        lblWalkerEmail.TabIndex = 2;
        lblWalkerEmail.Text = "Email:";
        // 
        // txtWalkerEmail
        // 
        txtWalkerEmail.Location = new Point(0, 97);
        txtWalkerEmail.Margin = new Padding(3, 4, 3, 4);
        txtWalkerEmail.Name = "txtWalkerEmail";
        txtWalkerEmail.Size = new Size(342, 27);
        txtWalkerEmail.TabIndex = 3;
        // 
        // pnlClientExtra
        // 
        pnlClientExtra.Controls.Add(lblClientPhone);
        pnlClientExtra.Controls.Add(txtRegPhone);
        pnlClientExtra.Controls.Add(lblClientEmail);
        pnlClientExtra.Controls.Add(txtRegEmail);
        pnlClientExtra.Controls.Add(lblAddress);
        pnlClientExtra.Controls.Add(txtRegAddress);
        pnlClientExtra.Controls.Add(lblSubscription);
        pnlClientExtra.Controls.Add(cmbSub);
        pnlClientExtra.Location = new Point(523, 456);
        pnlClientExtra.Margin = new Padding(3, 4, 3, 4);
        pnlClientExtra.Name = "pnlClientExtra";
        pnlClientExtra.Size = new Size(347, 261);
        pnlClientExtra.TabIndex = 21;
        pnlClientExtra.Visible = false;
        // 
        // lblClientPhone
        // 
        lblClientPhone.AutoSize = true;
        lblClientPhone.Location = new Point(0, 0);
        lblClientPhone.Name = "lblClientPhone";
        lblClientPhone.Size = new Size(53, 20);
        lblClientPhone.TabIndex = 0;
        lblClientPhone.Text = "Phone:";
        // 
        // txtRegPhone
        // 
        txtRegPhone.Location = new Point(0, 24);
        txtRegPhone.Margin = new Padding(3, 4, 3, 4);
        txtRegPhone.Name = "txtRegPhone";
        txtRegPhone.Size = new Size(342, 27);
        txtRegPhone.TabIndex = 1;
        // 
        // lblClientEmail
        // 
        lblClientEmail.AutoSize = true;
        lblClientEmail.Location = new Point(0, 73);
        lblClientEmail.Name = "lblClientEmail";
        lblClientEmail.Size = new Size(49, 20);
        lblClientEmail.TabIndex = 2;
        lblClientEmail.Text = "Email:";
        // 
        // txtRegEmail
        // 
        txtRegEmail.Location = new Point(0, 97);
        txtRegEmail.Margin = new Padding(3, 4, 3, 4);
        txtRegEmail.Name = "txtRegEmail";
        txtRegEmail.Size = new Size(342, 27);
        txtRegEmail.TabIndex = 3;
        // 
        // lblAddress
        // 
        lblAddress.AutoSize = true;
        lblAddress.Location = new Point(0, 144);
        lblAddress.Name = "lblAddress";
        lblAddress.Size = new Size(65, 20);
        lblAddress.TabIndex = 4;
        lblAddress.Text = "Address:";
        // 
        // txtRegAddress
        // 
        txtRegAddress.Location = new Point(0, 168);
        txtRegAddress.Margin = new Padding(3, 4, 3, 4);
        txtRegAddress.Name = "txtRegAddress";
        txtRegAddress.Size = new Size(342, 27);
        txtRegAddress.TabIndex = 5;
        // 
        // lblSubscription
        // 
        lblSubscription.AutoSize = true;
        lblSubscription.Location = new Point(0, 211);
        lblSubscription.Name = "lblSubscription";
        lblSubscription.Size = new Size(94, 20);
        lblSubscription.TabIndex = 6;
        lblSubscription.Text = "Subscription:";
        // 
        // cmbSub
        // 
        cmbSub.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbSub.Location = new Point(0, 232);
        cmbSub.Margin = new Padding(3, 4, 3, 4);
        cmbSub.Name = "cmbSub";
        cmbSub.Size = new Size(342, 28);
        cmbSub.TabIndex = 7;
        // 
        // btnRegister
        // 
        btnRegister.BackColor = Color.FromArgb(30, 70, 150);
        btnRegister.FlatAppearance.BorderSize = 0;
        btnRegister.FlatStyle = FlatStyle.Flat;
        btnRegister.Font = new Font("Segoe UI", 10F);
        btnRegister.ForeColor = Color.White;
        btnRegister.Location = new Point(523, 731);
        btnRegister.Margin = new Padding(3, 4, 3, 4);
        btnRegister.Name = "btnRegister";
        btnRegister.Size = new Size(343, 45);
        btnRegister.TabIndex = 22;
        btnRegister.Text = "Create Account";
        btnRegister.UseVisualStyleBackColor = false;
        // 
        // lblRegStatus
        // 
        lblRegStatus.Location = new Point(523, 784);
        lblRegStatus.Name = "lblRegStatus";
        lblRegStatus.Size = new Size(347, 48);
        lblRegStatus.TabIndex = 23;
        lblRegStatus.Visible = false;
        // 
        // LoginForm
        // 
        AcceptButton = btnLogin;
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.WhiteSmoke;
        ClientSize = new Size(905, 817);
        Controls.Add(lblTitle);
        Controls.Add(lblUsername);
        Controls.Add(txtUser);
        Controls.Add(lblPassword);
        Controls.Add(txtPass);
        Controls.Add(btnLogin);
        Controls.Add(lblErr);
        Controls.Add(pnlDivider);
        Controls.Add(lblNewAccount);
        Controls.Add(lblAccountType);
        Controls.Add(btnWalkerType);
        Controls.Add(btnClientType);
        Controls.Add(lblRegName);
        Controls.Add(txtRegName);
        Controls.Add(lblRegUsername);
        Controls.Add(txtRegUser);
        Controls.Add(lblRegPassword);
        Controls.Add(txtRegPass);
        Controls.Add(lblRegConfirm);
        Controls.Add(txtRegConfirm);
        Controls.Add(pnlWalkerExtra);
        Controls.Add(pnlClientExtra);
        Controls.Add(btnRegister);
        Controls.Add(lblRegStatus);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        Margin = new Padding(3, 4, 3, 4);
        MaximizeBox = false;
        Name = "LoginForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Dog Walking Manager — Login";
        pnlWalkerExtra.ResumeLayout(false);
        pnlWalkerExtra.PerformLayout();
        pnlClientExtra.ResumeLayout(false);
        pnlClientExtra.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    // Login controls
    private Label lblTitle;
    private Label lblUsername;
    private TextBox txtUser;
    private Label lblPassword;
    private TextBox txtPass;
    private Button btnLogin;
    private Label lblErr;
    private Panel pnlDivider;

    // Register header
    private Label lblNewAccount;
    private Label lblAccountType;
    private Button btnWalkerType;
    private Button btnClientType;

    // Register shared fields
    private Label lblRegName;
    private TextBox txtRegName;
    private Label lblRegUsername;
    private TextBox txtRegUser;
    private Label lblRegPassword;
    private TextBox txtRegPass;
    private Label lblRegConfirm;
    private TextBox txtRegConfirm;

    // Walker extra
    private Panel pnlWalkerExtra;
    private Label lblWalkerPhone;
    private TextBox txtWalkerPhone;
    private Label lblWalkerEmail;
    private TextBox txtWalkerEmail;

    // Client extra
    private Panel pnlClientExtra;
    private Label lblClientPhone;
    private TextBox txtRegPhone;
    private Label lblClientEmail;
    private TextBox txtRegEmail;
    private Label lblAddress;
    private TextBox txtRegAddress;
    private Label lblSubscription;
    private ComboBox cmbSub;

    // Register footer
    private Button btnRegister;
    private Label lblRegStatus;
}
