using DogWalking.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DogWalking.WinForms.Forms;

public partial class MainForm : Form
{
    private int? _userId;
    private string? _userRole;
    private string? _fullName;
    private Action? _onLogout;

    private readonly IServiceScope _scope;

    public MainForm(IServiceScopeFactory scopeFactory)
    {
        _scope = scopeFactory.CreateScope();

        InitializeComponent();

        lblSession.Text = $"{_fullName}  [{_userRole}]";
    }

    public void SetSession(int userId, string fullName, string role, Action? onLogout = null)
    {
        _userId = userId;
        _fullName = fullName;
        _userRole = role;
        _onLogout = onLogout;

        lblSession.Text = $"  {fullName}  [{role}]";
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _scope.Dispose();
        base.Dispose(disposing);
    }

    private void BtnLogout_Click(object? sender, EventArgs e)
    {
        _onLogout?.Invoke();
        Hide();
    }
}