using DogWalking.Application.DTOs;
using DogWalking.Application.Interfaces;
using DogWalking.Domain.Enums;
using DogWalking.WinForms.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace DogWalking.WinForms.Forms;

public partial class MainForm : Form
{
    private readonly IClientService _clients;
    private readonly IWalkEventService _walks;
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
        _walks = sp.GetRequiredService<IWalkEventService>();
        _users = sp.GetRequiredService<IUserService>();
        _dogSvc = sp.GetRequiredService<IDogService>();
        InitializeComponent();

        cmbWalkStatus.Items.AddRange(Enum.GetNames<WalkStatus>());
        if (cmbWalkStatus.Items.Count > 0)
            cmbWalkStatus.SelectedIndex = 0;

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
                ShowTabs(tabClients, tabWalks, tabWalkers, tabUsers);
                await LoadClientsAsync();
                break;
        }
    }

    private void ShowTabs(params TabPage[] pages)
    {
        tabs.TabPages.Clear();
        tabs.TabPages.AddRange(pages);
    }

    // ── Event Handlers (wired in Designer) ────────────────

    private void BtnLogout_Click(object? sender, EventArgs e)
    {
        _onLogout?.Invoke();
        Hide();
    }

    private async void TxtSearchClients_TextChanged(object? sender, EventArgs e)
        => await LoadClientsAsync(txtSearchClients.Text.Trim());

    private async void BtnRefreshClients_Click(object? sender, EventArgs e)
        => await LoadClientsAsync();

    private async void CmbWalkStatus_SelectedIndexChanged(object? sender, EventArgs e)
        => await LoadWalksAsync();

    private async void BtnNewWalk_Click(object? sender, EventArgs e)
    {
        var f = _scope.ServiceProvider.GetRequiredService<WalkEventForm>();
        if (f.ShowDialog() == DialogResult.OK)
            await LoadWalksAsync();
    }

    private async void BtnRefreshWalkers_Click(object? sender, EventArgs e)
        => await LoadWalkersAsync();

    private async void BtnRefreshUsers_Click(object? sender, EventArgs e)
        => await LoadAllUsersAsync();

    private async void TabClients_Enter(object? sender, EventArgs e)
        => await LoadClientsAsync();

    private async void TabWalks_Enter(object? sender, EventArgs e)
        => await LoadWalksAsync();

    private async void TabWalkers_Enter(object? sender, EventArgs e)
        => await LoadWalkersAsync();

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

    private async Task LoadWalksAsync()
    {
        try
        {
            var status = Enum.Parse<WalkStatus>(cmbWalkStatus.SelectedItem!.ToString()!);
            var list = await _walks.GetByStatusAsync(status, _cts.Token);

            dgvWalks.DataSource = list.Select(w => new
            {
                w.Id,
                Dog = w.DogName,
                Client = w.ClientName,
                Walker = w.WalkerName ?? "—",
                Date = w.WalkDate.ToLocalTime().ToString("yyyy-MM-dd HH:mm"),
                Duration = $"{w.DurationMinutes} min",
                w.Status,
                w.Location,
                w.Notes
            }).ToList();

            if (dgvWalks.Columns.Contains("Id"))
                dgvWalks.Columns["Id"]!.Visible = false;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { ShowError(ex.Message); }
    }

    private async Task LoadWalkersAsync()
    {
        try
        {
            var list = await _users.GetWalkersAsync(_cts.Token);
            dgvWalkers.DataSource = list.Select(u => new
            {
                u.Id,
                u.FullName,
                u.Username,
                Phone = u.Phone ?? "—",
                Email = u.Email ?? "—",
                Active = u.IsActive
            }).ToList();

            if (dgvWalkers.Columns.Contains("Id"))
                dgvWalkers.Columns["Id"]!.Visible = false;
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