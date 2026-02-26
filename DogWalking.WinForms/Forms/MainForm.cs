using DogWalking.Application.DTOs;
using DogWalking.Application.Interfaces;
using DogWalking.Domain.Enums;
using DogWalking.Domain.Services;
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
    private int _clientId;
    private Action? _onLogout;

    private readonly IServiceScope _scope;
    private readonly CancellationTokenSource _cts = new();

    // Client controls (created at runtime)
    private DataGridView _dgvMyDogs = null!;
    private DataGridView _dgvMyWalks = null!;

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

            case "Client":
                var client = await _clients.GetByUserIdAsync(_userId);
                if (client is null)
                {
                    MessageBox.Show(
                        "No client profile is linked to your account.\nPlease contact an administrator.",
                        "Profile Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                _clientId = client.Id;
                PopulateMyDogsTab();
                PopulateMyClientWalksTab();
                PopulateMySubscriptionTab(client);
                ShowTabs(tabMyDogs, tabMyWalks, tabMySubscription);
                await LoadMyDogsAsync();
                break;
        }
    }

    private void ShowTabs(params TabPage[] pages)
    {
        tabs.TabPages.Clear();
        tabs.TabPages.AddRange(pages);
    }

    // ── Admin Event Handlers (wired in Designer) ──────────

    private void BtnLogout_Click(object? sender, EventArgs e)
    {
        _onLogout?.Invoke();
        tabs.TabPages.Clear();
        _userId = 0; _userRole = string.Empty; _fullName = string.Empty; _clientId = 0;
        lblSession.Text = string.Empty;
        _onLogout = null;
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

    private async void TabMyDogs_Enter(object? sender, EventArgs e)
        => await LoadMyDogsAsync();

    private async void TabMyWalks_Enter(object? sender, EventArgs e)
        => await LoadClientWalksAsync();

    // ════════════════════════════════════════════════════════
    // ADMIN DATA LOADING
    // ════════════════════════════════════════════════════════

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
            HideCol(dgvClients, "Id");
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
            HideCol(dgvWalks, "Id");
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
            HideCol(dgvWalkers, "Id");
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { ShowError(ex.Message); }
    }

    private Task LoadAllUsersAsync() => RunAsync(async () =>
    {
        var list = await _users.GetAllAsync();
        dgvAllUsers.DataSource = list.Select(u => new
        {
            u.Id,
            u.FullName,
            u.Username,
            Role = u.Role.ToString(),
            Active = u.IsActive
        }).ToList();
        HideCol(dgvAllUsers, "Id");
    });

    // ════════════════════════════════════════════════════════
    // CLIENT TABS
    // ════════════════════════════════════════════════════════

    private void PopulateMyDogsTab()
    {
        tabMyDogs.Controls.Clear();
        tabMyDogs.Enter -= TabMyDogs_Enter;
        tabMyDogs.Enter += TabMyDogs_Enter;

        var pnl = new Panel { Location = new Point(10, 8), Size = new Size(1000, 38) };
        pnl.Controls.AddRange(new Control[]
        {
            Btn("+ Add Dog",    0,   async () => await ShowMyDogDialogAsync()),
            Btn("\u21bb Refresh",   112, async () => await LoadMyDogsAsync()),
            Btn("\U0001f5d1\ufe0f Delete",  224, async () => await DeleteMyDogAsync())
        });

        _dgvMyDogs = Grid();
        _dgvMyDogs.Location = new Point(10, 56);
        _dgvMyDogs.Size = new Size(1005, 525);

        var ctx = new ContextMenuStrip();
        ctx.Items.Add("\u270f\ufe0f Edit", null, async (_, _) => await ShowMyDogDialogAsync(SelectedId(_dgvMyDogs)));
        ctx.Items.Add("\U0001f5d1\ufe0f Delete", null, async (_, _) => await DeleteMyDogAsync());
        _dgvMyDogs.ContextMenuStrip = ctx;

        tabMyDogs.Controls.AddRange(new Control[] { pnl, _dgvMyDogs });
    }

    private async Task LoadMyDogsAsync()
    {
        try
        {
            var list = await _dogSvc.GetByClientIdAsync(_clientId, _cts.Token);
            _dgvMyDogs.DataSource = list.Select(d => new
            {
                d.Id,
                d.Name,
                d.Breed,
                BirthDate = d.BirthDate.ToString("yyyy-MM-dd"),
                Age = $"{d.AgeInYears} yrs"
            }).ToList();
            HideCol(_dgvMyDogs, "Id");
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { ShowError(ex.Message); }
    }

    private async Task ShowMyDogDialogAsync(int? dogId = null)
    {
        using var dlg = new DogDialog(_dogSvc, _clientId, dogId);
        if (dlg.ShowDialog() == DialogResult.OK)
            await LoadMyDogsAsync();
    }

    private async Task DeleteMyDogAsync()
    {
        if (SelectedId(_dgvMyDogs) is not int id) return;
        if (MessageBox.Show("Delete this dog? All associated walk events will also be deleted.",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        try { await _dogSvc.DeleteAsync(id); await LoadMyDogsAsync(); }
        catch (Exception ex) { ShowError(ex.Message); }
    }

    private void PopulateMyClientWalksTab()
    {
        tabMyWalks.Controls.Clear();
        tabMyWalks.Enter -= TabMyWalks_Enter;
        tabMyWalks.Enter += TabMyWalks_Enter;

        _dgvMyWalks = Grid();
        _dgvMyWalks.Location = new Point(10, 56);
        _dgvMyWalks.Size = new Size(1005, 525);

        var pnl = new Panel { Location = new Point(10, 8), Size = new Size(1000, 38) };
        pnl.Controls.AddRange(new Control[]
        {
            Btn("+ Request Walk", 0, async () =>
            {
                var f = _scope.ServiceProvider.GetRequiredService<WalkEventForm>();
                f.LockToClient(_clientId);
                if (f.ShowDialog() == DialogResult.OK)
                    await LoadClientWalksAsync();
            }),
            Btn("\u21bb Refresh", 140, async () => await LoadClientWalksAsync()),
            Btn("\U0001f5d1\ufe0f Delete",  260, async () => await DeleteMyWalkAsync())
        });

        tabMyWalks.Controls.AddRange(new Control[] { pnl, _dgvMyWalks });
    }

    private async Task LoadClientWalksAsync()
    {
        try
        {
            var list = await _walks.GetByClientIdAsync(_clientId, _cts.Token);
            _dgvMyWalks.DataSource = list.Select(w => new
            {
                w.Id,
                Dog = w.DogName,
                Walker = w.WalkerName ?? "—",
                Date = w.WalkDate.ToLocalTime().ToString("yyyy-MM-dd HH:mm"),
                Duration = $"{w.DurationMinutes} min",
                Status = w.Status.ToString(),
                w.Location,
                w.Notes
            }).ToList();
            HideCol(_dgvMyWalks, "Id");
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { ShowError(ex.Message); }
    }

    private async Task DeleteMyWalkAsync()
    {
        if (SelectedId(_dgvMyWalks) is not int id) return;
        if (MessageBox.Show("Delete this walk? This cannot be undone.",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        try { await _walks.DeleteAsync(id); await LoadClientWalksAsync(); }
        catch (Exception ex) { ShowError(ex.Message); }
    }

    private void PopulateMySubscriptionTab(ClientDto client)
    {
        tabMySubscription.Controls.Clear();

        var strategy = WalkLimitStrategyFactory.Create(client.Subscription);

        var lblPlan = new Label
        {
            Text = $"Current Plan: {strategy.Description}",
            Font = new Font("Segoe UI", 13, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 70, 150),
            AutoSize = true,
            Location = new Point(40, 60)
        };
        var lblLimits = new Label
        {
            Text = $"\u2022 Up to {strategy.MaxWalksPerMonth} walks per month\n" +
                       $"\u2022 {(strategy.AllowsMultiplePerDay ? "Unlimited walks per day" : "Max 1 walk per day")}",
            Font = new Font("Segoe UI", 10),
            AutoSize = true,
            Location = new Point(40, 100)
        };
        var lblAddress = new Label
        {
            Text = $"Address: {(string.IsNullOrWhiteSpace(client.Address) ? "\u2014" : client.Address)}",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(60, 60, 60),
            AutoSize = true,
            Location = new Point(40, 148)
        };

        var btnChange = new Button
        {
            Text = "Change Plan",
            Location = new Point(40, 188),
            Size = new Size(160, 34),
            BackColor = Color.FromArgb(30, 70, 150),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10)
        };
        btnChange.FlatAppearance.BorderSize = 0;
        btnChange.Click += async (_, _) =>
        {
            await ChangeSubscriptionAsync(_clientId);
            var updated = await _clients.GetByUserIdAsync(_userId);
            if (updated is null) return;
            var s = WalkLimitStrategyFactory.Create(updated.Subscription);
            lblPlan.Text = $"Current Plan: {s.Description}";
            lblLimits.Text = $"\u2022 Up to {s.MaxWalksPerMonth} walks per month\n" +
                             $"\u2022 {(s.AllowsMultiplePerDay ? "Unlimited walks per day" : "Max 1 walk per day")}";
        };

        tabMySubscription.Controls.AddRange(new Control[] { lblPlan, lblLimits, lblAddress, btnChange });
    }

    // ════════════════════════════════════════════════════════
    // SHARED HELPERS
    // ════════════════════════════════════════════════════════

    private async Task ChangeSubscriptionAsync(int? clientId)
    {
        int id = clientId ?? SelectedId(dgvClients) ?? -1;
        if (id < 0) return;

        var strategies = WalkLimitStrategyFactory.GetAll().ToList();
        var picker = new Form
        {
            Text = "Change Subscription",
            Size = new Size(360, 170),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false
        };
        var cmb = new ComboBox
        {
            Location = new Point(20, 20),
            Width = 300,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        foreach (var s in strategies) cmb.Items.Add(s.Description);
        cmb.SelectedIndex = 0;

        int selectedIndex = -1;
        var btnOk = new Button
        {
            Text = "Apply",
            Location = new Point(20, 70),
            Width = 120,
            Height = 28,
            BackColor = Color.FromArgb(30, 70, 150),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnOk.Click += (_, _) =>
        {
            selectedIndex = cmb.SelectedIndex;
            picker.DialogResult = DialogResult.OK;
            picker.Close();
        };
        picker.Controls.AddRange(new Control[] { cmb, btnOk });
        picker.ShowDialog(this);

        if (selectedIndex < 0) return;

        try
        {
            var sub = strategies[selectedIndex].SubscriptionType;
            await _clients.ChangeSubscriptionAsync(new ChangeSubscriptionDto(id, sub));
            if (_userRole == "Admin") await LoadClientsAsync();
        }
        catch (Exception ex) { ShowError(ex.Message); }
    }

    private static DataGridView Grid() => new()
    {
        ReadOnly = true,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        BackgroundColor = Color.White,
        BorderStyle = BorderStyle.None,
        RowHeadersVisible = false,
        Font = new Font("Segoe UI", 9)
    };

    private static Button Btn(string text, int x, Func<Task> onClick)
    {
        var b = new Button
        {
            Text = text,
            Location = new Point(x, 5),
            Height = 28,
            AutoSize = true,
            BackColor = Color.FromArgb(30, 70, 150),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9)
        };
        b.Click += async (_, _) => await onClick();
        return b;
    }

    private static int? SelectedId(DataGridView dgv)
        => dgv.CurrentRow?.Cells["Id"].Value is int id ? id : null;

    private static void HideCol(DataGridView dgv, string col)
    {
        if (dgv.Columns.Contains(col)) dgv.Columns[col]!.Visible = false;
    }

    private void ShowError(string msg)
        => MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);

    private async Task RunAsync(Func<Task> action)
    {
        var result = await action().ToResultAsync();
        if (!result.IsSuccess) ShowError(result.Error!);
    }
}