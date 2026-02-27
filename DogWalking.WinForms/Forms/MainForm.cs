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
    private readonly IWalkerAvailabilityService _availSvc;

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

    // Walker controls (created at runtime)
    private DataGridView _dgvSchedule = null!;
    private DataGridView _cfgSlots = null!;
    private ComboBox     _cfgDay   = null!;
    private TextBox      _cfgStart = null!;
    private TextBox      _cfgEnd   = null!;
    private ComboBox     _cfgZone  = null!;
    private TextBox      _cfgPhone = null!;
    private TextBox      _cfgEmail = null!;
    private Label        _cfgStatus = null!;

    public MainForm(IServiceScopeFactory scopeFactory)
    {
        _scope = scopeFactory.CreateScope();
        var sp = _scope.ServiceProvider;
        _clients = sp.GetRequiredService<IClientService>();
        _walks = sp.GetRequiredService<IWalkEventService>();
        _users = sp.GetRequiredService<IUserService>();
        _dogSvc = sp.GetRequiredService<IDogService>();
        _availSvc = sp.GetRequiredService<IWalkerAvailabilityService>();
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

            case "Walker":
                PopulateMyScheduleTab();
                PopulateMyAvailabilityTab();
                ShowTabs(tabMySchedule, tabMyAvailability);
                await LoadMyScheduleAsync();
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

    private async void TabMySchedule_Enter(object? sender, EventArgs e)
        => await LoadMyScheduleAsync();

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
                Walker = w.WalkerName ?? "\u2014",
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
                Phone = u.Phone ?? "\u2014",
                Email = u.Email ?? "\u2014",
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
    // WALKER TABS
    // ════════════════════════════════════════════════════════

    private void PopulateMyScheduleTab()
    {
        tabMySchedule.Controls.Clear();
        tabMySchedule.Enter -= TabMySchedule_Enter;
        tabMySchedule.Enter += TabMySchedule_Enter;

        var pnl = new Panel { Location = new Point(10, 8), Size = new Size(1000, 38) };
        pnl.Controls.AddRange(new Control[]
        {
            Btn("\u21bb Refresh", 0, async () => await LoadMyScheduleAsync())
        });

        _dgvSchedule = Grid();
        _dgvSchedule.Location = new Point(10, 56);
        _dgvSchedule.Size = new Size(1005, 525);

        var ctx = new ContextMenuStrip();
        ctx.Items.Add("\u2714 Accept / Claim",  null, async (_, _) => await RespondToWalkAsync(accept: true));
        ctx.Items.Add("\u2718 Decline",         null, async (_, _) => await RespondToWalkAsync(accept: false));
        ctx.Items.Add("\u25b6 Start Walk",      null, async (_, _) => await ChangeWalkStatus(_dgvSchedule, WalkStatus.InProgress, LoadMyScheduleAsync));
        ctx.Items.Add("\u2714 Complete Walk",   null, async (_, _) => await ChangeWalkStatus(_dgvSchedule, WalkStatus.Completed, LoadMyScheduleAsync));
        ctx.Items.Add("\u21a9 Release Walk",    null, async (_, _) => await ReleaseAcceptedWalkAsync());
        ctx.Items.Add("\u274c Cancel Walk",     null, async (_, _) => await CancelWalkWithNoteAsync(_dgvSchedule, LoadMyScheduleAsync));
        _dgvSchedule.ContextMenuStrip = ctx;

        tabMySchedule.Controls.AddRange(new Control[] { pnl, _dgvSchedule });
    }

    private async Task LoadMyScheduleAsync()
    {
        try
        {
            var assigned = await _walks.GetByWalkerIdAsync(_userId);
            var matching = await _walks.GetMatchingRequestsForWalkerAsync(_userId);
            var merged = assigned.Concat(matching).DistinctBy(w => w.Id);

            _dgvSchedule.DataSource = merged.Select(w => new
            {
                w.Id,
                Dog = w.DogName,
                Client = w.ClientName,
                ClientAddress = w.ClientAddress,
                Date = w.WalkDate.ToLocalTime().ToString("yyyy-MM-dd HH:mm"),
                Duration = $"{w.DurationMinutes} min",
                Status = w.Status.ToString(),
                w.Location,
                w.Notes
            }).ToList();
            HideCol(_dgvSchedule, "Id");
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { ShowError(ex.Message); }
    }

    /// <summary>
    /// Walker accepts or declines a walk. For Requested walks, accepting calls ClaimWalkAsync.
    /// For Proposed walks, accepting calls WalkerRespondAsync.
    /// </summary>
    private async Task RespondToWalkAsync(bool accept)
    {
        if (SelectedId(_dgvSchedule) is not int id) return;

        // Read status from grid
        var statusStr = _dgvSchedule.CurrentRow?.Cells["Status"]?.Value?.ToString();
        if (statusStr is null) return;

        try
        {
            if (accept)
            {
                if (statusStr == WalkStatus.Requested.ToString())
                    await _walks.ClaimWalkAsync(id, _userId);
                else
                    await _walks.WalkerRespondAsync(new WalkerResponseDto(id, true));
            }
            else
            {
                // Prompt for rejection reason
                string? note = PromptForNote("Decline Walk", "Reason for declining (optional):");
                if (note is null) return; // user cancelled

                await _walks.WalkerRespondAsync(new WalkerResponseDto(id, false, note));
            }
            await LoadMyScheduleAsync();
        }
        catch (Exception ex) { ShowError(ex.Message); }
    }

    /// <summary>
    /// Walker releases an accepted walk — returns it to Requested so another walker can claim it.
    /// </summary>
    private async Task ReleaseAcceptedWalkAsync()
    {
        if (SelectedId(_dgvSchedule) is not int id) return;

        string? note = PromptForNote("Release Walk", "Reason for releasing (optional):");
        if (note is null) return;

        try
        {
            await _walks.UnacceptWalkAsync(id, string.IsNullOrWhiteSpace(note) ? null : note);
            await LoadMyScheduleAsync();
        }
        catch (Exception ex) { ShowError(ex.Message); }
    }

    private void PopulateMyAvailabilityTab()
    {
        tabMyAvailability.Controls.Clear();

        // ── Availability slots section ────────────────────────────────────
        tabMyAvailability.Controls.Add(new Label
        {
            Text = "Availability Slots",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            Location = new Point(12, 12), AutoSize = true
        });

        _cfgSlots = new DataGridView
        {
            Location              = new Point(12, 38),
            Size                  = new Size(1020, 230),
            ReadOnly              = true,
            AllowUserToAddRows    = false,
            AllowUserToDeleteRows = false,
            SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor       = Color.White,
            BorderStyle           = BorderStyle.None,
            RowHeadersVisible     = false,
            Font                  = new Font("Segoe UI", 9)
        };
        tabMyAvailability.Controls.Add(_cfgSlots);

        // ── Add slot row ──────────────────────────────────────────────────
        int y = 278;
        tabMyAvailability.Controls.Add(new Label
        {
            Text     = "Add Slot:",
            Font     = new Font("Segoe UI", 9, FontStyle.Bold),
            Location = new Point(12, y), AutoSize = true
        });
        y += 22;

        _cfgDay = new ComboBox
        {
            Location      = new Point(12, y),
            Width         = 130,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        foreach (DayOfWeek d in Enum.GetValues<DayOfWeek>())
            _cfgDay.Items.Add(d.ToString());
        _cfgDay.SelectedIndex = 1; // Monday
        tabMyAvailability.Controls.Add(_cfgDay);

        tabMyAvailability.Controls.Add(new Label { Text = "Start:", Location = new Point(150, y + 3), AutoSize = true });
        _cfgStart = new TextBox { Location = new Point(192, y), Width = 70, PlaceholderText = "08:00" };
        tabMyAvailability.Controls.Add(_cfgStart);

        tabMyAvailability.Controls.Add(new Label { Text = "End:", Location = new Point(270, y + 3), AutoSize = true });
        _cfgEnd = new TextBox { Location = new Point(302, y), Width = 70, PlaceholderText = "17:00" };
        tabMyAvailability.Controls.Add(_cfgEnd);

        tabMyAvailability.Controls.Add(new Label { Text = "Zone:", Location = new Point(382, y + 3), AutoSize = true });
        _cfgZone = new ComboBox
        {
            Location      = new Point(420, y),
            Width         = 200,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        foreach (var z in WalkZoneExtensions.All())
            _cfgZone.Items.Add(z.ToDisplayName());
        _cfgZone.SelectedIndex = 0;
        tabMyAvailability.Controls.Add(_cfgZone);

        var btnAdd = new Button
        {
            Text      = "+ Add",
            Location  = new Point(630, y),
            Width     = 90,
            Height    = 26,
            BackColor = Color.FromArgb(30, 70, 150),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnAdd.Click += async (_, _) => await AddAvailabilitySlotAsync();
        tabMyAvailability.Controls.Add(btnAdd);

        y += 34;
        var btnDel = new Button
        {
            Text      = "\U0001f5d1\ufe0f Delete Selected",
            Location  = new Point(12, y),
            Width     = 160,
            Height    = 26,
            BackColor = Color.FromArgb(180, 40, 40),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnDel.Click += async (_, _) => await RemoveAvailabilitySlotAsync();
        tabMyAvailability.Controls.Add(btnDel);

        // ── Separator ─────────────────────────────────────────────────────
        y += 40;
        tabMyAvailability.Controls.Add(new Panel
        {
            Location  = new Point(12, y),
            Size      = new Size(1020, 1),
            BackColor = Color.Silver
        });
        y += 10;

        // ── Contact info section ──────────────────────────────────────────
        tabMyAvailability.Controls.Add(new Label
        {
            Text     = "Contact Info",
            Font     = new Font("Segoe UI", 11, FontStyle.Bold),
            Location = new Point(12, y), AutoSize = true
        });
        y += 28;

        tabMyAvailability.Controls.Add(new Label { Text = "Phone:", Location = new Point(12, y + 3), AutoSize = true });
        _cfgPhone = new TextBox { Location = new Point(60, y), Width = 180 };
        tabMyAvailability.Controls.Add(_cfgPhone);

        tabMyAvailability.Controls.Add(new Label { Text = "Email:", Location = new Point(256, y + 3), AutoSize = true });
        _cfgEmail = new TextBox { Location = new Point(298, y), Width = 240 };
        tabMyAvailability.Controls.Add(_cfgEmail);

        var btnSave = new Button
        {
            Text      = "Save Contact Info",
            Location  = new Point(548, y),
            Width     = 160,
            Height    = 26,
            BackColor = Color.FromArgb(30, 70, 150),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnSave.Click += async (_, _) => await SaveWalkerContactInfoAsync();
        tabMyAvailability.Controls.Add(btnSave);

        // ── Status label ──────────────────────────────────────────────────
        y += 38;
        _cfgStatus = new Label
        {
            Location = new Point(12, y),
            Size     = new Size(700, 20),
            Visible  = false
        };
        tabMyAvailability.Controls.Add(_cfgStatus);
    }

    private async Task LoadMyAvailabilityAsync()
    {
        try
        {
            var slots = await _availSvc.GetByWalkerIdAsync(_userId);
            _cfgSlots.DataSource = slots.Select(s => new
            {
                s.Id,
                Day   = s.DayOfWeek.ToString(),
                Start = s.StartTime.ToString("HH:mm"),
                End   = s.EndTime.ToString("HH:mm"),
                s.Zone
            }).ToList();
            HideCol(_cfgSlots, "Id");

            var user = await _users.GetByIdAsync(_userId);
            _cfgPhone.Text = user?.Phone ?? string.Empty;
            _cfgEmail.Text = user?.Email ?? string.Empty;
        }
        catch (Exception ex) { ShowCfgStatus(ex.Message, isError: true); }
    }

    private async Task AddAvailabilitySlotAsync()
    {
        if (_cfgDay.SelectedItem is not string dayStr ||
            !Enum.TryParse<DayOfWeek>(dayStr, out var day))
            return;

        if (!TimeOnly.TryParseExact(_cfgStart.Text.Trim(), "HH:mm", out var start) ||
            !TimeOnly.TryParseExact(_cfgEnd.Text.Trim(),   "HH:mm", out var end))
        {
            ShowCfgStatus("Invalid time format. Use HH:mm (e.g. 08:00).", isError: true);
            return;
        }

        var zone = _cfgZone.SelectedItem?.ToString() ?? string.Empty;
        try
        {
            await _availSvc.AddAvailabilityAsync(
                new CreateAvailabilityDto(_userId, day, start, end, zone));
            await LoadMyAvailabilityAsync();
            ShowCfgStatus("Slot added.", isError: false);
        }
        catch (Exception ex) { ShowCfgStatus(ex.Message, isError: true); }
    }

    private async Task RemoveAvailabilitySlotAsync()
    {
        if (_cfgSlots.CurrentRow?.Cells["Id"].Value is not int id) return;
        try
        {
            await _availSvc.DeleteAvailabilityAsync(id);
            await LoadMyAvailabilityAsync();
            ShowCfgStatus("Slot removed.", isError: false);
        }
        catch (Exception ex) { ShowCfgStatus(ex.Message, isError: true); }
    }

    private async Task SaveWalkerContactInfoAsync()
    {
        try
        {
            await _users.UpdateContactInfoAsync(
                _userId,
                string.IsNullOrWhiteSpace(_cfgPhone.Text) ? null : _cfgPhone.Text.Trim(),
                string.IsNullOrWhiteSpace(_cfgEmail.Text) ? null : _cfgEmail.Text.Trim());
            ShowCfgStatus("Contact info saved.", isError: false);
        }
        catch (Exception ex) { ShowCfgStatus(ex.Message, isError: true); }
    }

    private void ShowCfgStatus(string msg, bool isError)
    {
        _cfgStatus.Text      = msg;
        _cfgStatus.ForeColor = isError ? Color.Crimson : Color.DarkGreen;
        _cfgStatus.Visible   = true;
    }

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
                Walker = w.WalkerName ?? "\u2014",
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

    private async Task ChangeWalkStatus(DataGridView grid, WalkStatus newStatus, Func<Task> reload)
    {
        if (SelectedId(grid) is not int id) return;
        try { await _walks.UpdateStatusAsync(new UpdateWalkStatusDto(id, newStatus)); await reload(); }
        catch (Exception ex) { ShowError(ex.Message); }
    }

    /// <summary>
    /// Prompts for a cancellation note, then cancels the walk.
    /// </summary>
    private async Task CancelWalkWithNoteAsync(DataGridView grid, Func<Task> reload)
    {
        if (SelectedId(grid) is not int id) return;

        string? note = PromptForNote("Cancel Walk", "Reason for cancellation (optional):");
        if (note is null) return;

        try
        {
            await _walks.CancelWithNoteAsync(id, string.IsNullOrWhiteSpace(note) ? null : note);
            await reload();
        }
        catch (Exception ex) { ShowError(ex.Message); }
    }

    /// <summary>
    /// Shows a small dialog prompting for a note. Returns the note text, or null if cancelled.
    /// </summary>
    private string? PromptForNote(string title, string prompt)
    {
        var dlg = new Form
        {
            Text            = title,
            Size            = new Size(420, 200),
            StartPosition   = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox     = false,
            BackColor       = Color.WhiteSmoke
        };
        dlg.Controls.Add(new Label { Text = prompt, Location = new Point(16, 16), AutoSize = true });
        var txtNote = new TextBox { Location = new Point(16, 38), Width = 374, Height = 60, Multiline = true };
        dlg.Controls.Add(txtNote);

        bool confirmed = false;
        var btnOk = new Button
        {
            Text = "Confirm", Location = new Point(16, 118), Width = 130, Height = 28,
            BackColor = Color.FromArgb(30, 70, 150), ForeColor = Color.White, FlatStyle = FlatStyle.Flat
        };
        btnOk.Click += (_, _) => { confirmed = true; dlg.DialogResult = DialogResult.OK; dlg.Close(); };
        var btnAbort = new Button
        {
            Text = "Go Back", Location = new Point(162, 118), Width = 100, Height = 28, FlatStyle = FlatStyle.Flat
        };
        btnAbort.Click += (_, _) => dlg.Close();
        dlg.Controls.AddRange(new Control[] { txtNote, btnOk, btnAbort });
        dlg.ShowDialog(this);

        return confirmed ? txtNote.Text.Trim() : null;
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
