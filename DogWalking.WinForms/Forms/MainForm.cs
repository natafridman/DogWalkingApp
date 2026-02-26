using DogWalking.Application.Interfaces;
using DogWalking.WinForms.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace DogWalking.WinForms.Forms;

public partial class MainForm : Form
{
    private readonly IClientService _clients;
    private readonly IUserService _users;
    private readonly IDogService _dogSvc;

    private int _userId;
    private string _userRole = string.Empty;
    private string _fullName = string.Empty;
    private Action? _onLogout;

    private readonly IServiceScope _scope;
    private readonly CancellationTokenSource _cts = new();

    public MainForm(IServiceScopeFactory scopeFactory)
    {
        _scope = scopeFactory.CreateScope();
        var sp = _scope.ServiceProvider;

        _clients = sp.GetRequiredService<IClientService>();
        _users = sp.GetRequiredService<IUserService>();
        _dogSvc = sp.GetRequiredService<IDogService>();

        InitializeComponent();

        tabs.TabPages.Clear();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _cts.Cancel();
        base.OnFormClosed(e);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { _cts.Dispose(); _scope.Dispose(); }
        base.Dispose(disposing);
    }

    // ── Session ───────────────────────────────────────────

    public void SetSession(int userId, string fullName, string role, Action? onLogout = null)
    {
        _userId = userId;
        _fullName = fullName;
        _userRole = role;
        _onLogout = onLogout;
        lblSession.Text = $"{fullName}  [{role}]";
    }

    public async Task ApplyRoleLayoutAsync()
    {
        switch (_userRole)
        {
            case "Admin":
                ShowTabs(tabClients, tabUsers);
                await LoadClientsAsync();
                break;
        }
    }

    private void ShowTabs(params TabPage[] pages)
    {
        tabs.TabPages.Clear();
        tabs.TabPages.AddRange(pages);
    }

    // ── Event Handlers ────────────────

    private void BtnLogout_Click(object? sender, EventArgs e)
    {
        _onLogout?.Invoke();
        Hide();
    }

    private async void TxtSearchClients_TextChanged(object? sender, EventArgs e)
        => await LoadClientsAsync(txtSearchClients.Text.Trim());

    private async void BtnRefreshClients_Click(object? sender, EventArgs e)
        => await LoadClientsAsync();

    private async void BtnRefreshUsers_Click(object? sender, EventArgs e)
        => await LoadAllUsersAsync();

    private async void TabClients_Enter(object? sender, EventArgs e)
        => await LoadClientsAsync();

    private async void TabUsers_Enter(object? sender, EventArgs e)
        => await LoadAllUsersAsync();

    // ── Data Loading ──────────────────────────────────────

    private async Task LoadClientsAsync(string search = "")
    {
        try
        {
            var list = string.IsNullOrWhiteSpace(search)
                ? await _clients.GetAllActiveAsync(_cts.Token)
                : await _clients.SearchAsync(search, _cts.Token);

            dgvClients.DataSource = list.Select(c => new
            {
                c.Id,
                c.Name,
                c.Email,
                c.PhoneNumber,
                c.Address,
                c.Zone,
                Subscription = c.Subscription.ToString(),
                Dogs = c.DogCount
            }).ToList();

            if (dgvClients.Columns.Contains("Id"))
                dgvClients.Columns["Id"]!.Visible = false;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { ShowError(ex.Message); }
    }

    private async Task LoadAllUsersAsync()
    {
        try
        {
            var list = await _users.GetAllAsync(_cts.Token);
            dgvAllUsers.DataSource = list.Select(u => new
            {
                u.Id,
                u.Username,
                u.FullName,
                u.Role,
                u.IsActive
            }).ToList();

            if (dgvAllUsers.Columns.Contains("Id"))
                dgvAllUsers.Columns["Id"]!.Visible = false;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { ShowError(ex.Message); }
    }

    // ── Helpers ───────────────────────────────────────────

    private async Task RunAsync(Func<Task> action)
    {
        var result = await action().ToResultAsync();
        if (!result.IsSuccess) ShowError(result.Error!);
    }

    private void ShowError(string msg)
        => MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}