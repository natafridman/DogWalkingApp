using DogWalking.Application.DTOs;
using DogWalking.Application.Interfaces;
using DogWalking.Domain.Enums;

using Microsoft.Extensions.DependencyInjection;

namespace DogWalking.WinForms.Forms;

/// <summary>
/// Login form with a side registration panel.
/// Left side: login. 
/// Right side: create account as Walker or Client (dog owner).
/// </summary>
public partial class LoginForm : Form
{
    private readonly IAuthService _auth;
    private bool _isWalkerMode = true;

    private readonly IServiceScope _scope;

    public LoginForm(IServiceScopeFactory scopeFactory)
    {
        _scope = scopeFactory.CreateScope();
        _auth = _scope.ServiceProvider.GetRequiredService<IAuthService>();
        InitializeComponent();
        PopulateSubscriptions();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _scope.Dispose();
        base.Dispose(disposing);
    }

    // ── Setup ──────────────────────────────────────────────

    private void PopulateSubscriptions()
    {
        cmbSub.Items.Add("Free");
        cmbSub.Items.Add("Basic");
        cmbSub.Items.Add("Pro");
        cmbSub.Items.Add("Premium");

        cmbSub.SelectedIndex = 0;
    }

    // ── Register mode toggle ───────────────────────────────

    private void SetRegisterMode(bool isWalker)
    {
        _isWalkerMode = isWalker;
        pnlWalkerExtra.Visible = isWalker;
        pnlClientExtra.Visible = !isWalker;
        lblRegStatus.Visible = false;

        btnWalkerType.BackColor = isWalker ? Color.FromArgb(30, 70, 150) : Color.FromArgb(220, 220, 220);
        btnWalkerType.ForeColor = isWalker ? Color.White : Color.FromArgb(70, 70, 70);

        btnClientType.BackColor = isWalker ? Color.FromArgb(220, 220, 220) : Color.FromArgb(30, 70, 150);
        btnClientType.ForeColor = isWalker ? Color.FromArgb(70, 70, 70) : Color.White;
    }

    // ── Register logic ─────────────────────────────────────

    private async Task RegisterAsync()
    {
        lblRegStatus.Visible = false;

        try
        {
            if (_isWalkerMode)
            {
                var ok = await _auth.CreateUserAsync(new CreateUserDto(
                    txtRegUser.Text.Trim(), txtRegPass.Text, txtRegName.Text.Trim(),
                    UserRole.Walker,
                    NullIfEmpty(txtWalkerPhone.Text), NullIfEmpty(txtWalkerEmail.Text)));

                if (!ok) { ShowRegError("Username is already taken."); return; }
            }
            else
            {
                var result = await _auth.RegisterClientUserAsync(new RegisterClientUserDto(
                    txtRegUser.Text.Trim(), txtRegPass.Text, txtRegName.Text.Trim(),
                    txtRegPhone.Text.Trim(), txtRegEmail.Text.Trim(),
                    (SubscriptionType)(cmbSub.SelectedIndex + 1),
                    txtRegAddress.Text.Trim(),
                    txtRegConfirm.Text));

                if (!result.Success) { ShowRegError(result.ErrorMessage!); return; }
            }

            ShowRegSuccess("Account created! You can now log in.");
            ClearRegisterFields();
        }
        catch (FluentValidation.ValidationException ex)
        {
            ShowRegError(ex.Errors.First().ErrorMessage);
        }
        catch (Exception ex)
        {
            ShowRegError(ex.Message);
        }
    }

    private static string? NullIfEmpty(string s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private void ClearRegisterFields()
    {
        txtRegName.Clear(); txtRegUser.Clear();
        txtRegPass.Clear(); txtRegConfirm.Clear();
        txtRegPhone.Clear(); txtRegEmail.Clear(); txtRegAddress.Clear();
        txtWalkerPhone.Clear(); txtWalkerEmail.Clear();
    }

    private void ShowRegError(string msg)
    {
        lblRegStatus.Text = msg;
        lblRegStatus.ForeColor = Color.Crimson;
        lblRegStatus.Visible = true;
    }

    private void ShowRegSuccess(string msg)
    {
        lblRegStatus.Text = msg;
        lblRegStatus.ForeColor = Color.FromArgb(0, 128, 0);
        lblRegStatus.Visible = true;
    }

    // ── Login logic ────────────────────────────────────────

    private async Task LoginAsync()
    {
        lblErr.Visible = false;
        var result = await _auth.LoginAsync(new LoginDto(txtUser.Text.Trim(), txtPass.Text));

        if (!result.Success)
        {
            lblErr.Text = result.ErrorMessage;
            lblErr.Visible = true;
            return;
        }

        var main = Program.ServiceProvider.GetRequiredService<MainForm>();
        main.SetSession(result.UserId!.Value, result.FullName!, result.Role!, () => Show());
        Hide();
        main.FormClosed += (_, _) => { if (!Visible) Close(); };
        main.Show();
    }

    // ── Event Handlers ─────────────────────────────────────

    private async void BtnLogin_Click(object? sender, EventArgs e) => await LoginAsync();
    private async void BtnRegister_Click(object? sender, EventArgs e) => await RegisterAsync();
    private void BtnWalkerType_Click(object? sender, EventArgs e) => SetRegisterMode(isWalker: true);
    private void BtnClientType_Click(object? sender, EventArgs e) => SetRegisterMode(isWalker: false);
}
