using DogWalking.Application.DTOs;
using DogWalking.Application.Interfaces;
using DogWalking.Domain.Enums;
using DogWalking.Domain.Services;
using DogWalking.WinForms.Controls;
using DogWalking.WinForms.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace DogWalking.WinForms.Forms;

/// <summary>
/// Main shell. Shows role-specific tabs after login:
///   Admin  -> Clients, Walk Events, Walkers, Users  (Designer-defined)
///   Walker -> My Schedule (calendar), My Availability (populated at runtime)
///   Client -> My Dogs, My Walks, My Subscription     (populated at runtime)
/// </summary>
public partial class MainForm : Form
{
    private readonly IClientService             _clients;
    private readonly IWalkEventService          _walks;
    private readonly IUserService               _users;
    private readonly IAuthService               _auth;
    private readonly IDogService                _dogSvc;
    private readonly IWalkerAvailabilityService _availSvc;

    // Session state
    private int     _userId;
    private string  _userRole = string.Empty;
    private string  _fullName = string.Empty;
    private int     _clientId; // populated for Client role
    private Action? _onLogout;

    // Form-scoped DI scope + cancellation
    private readonly IServiceScope           _scope;
    private readonly CancellationTokenSource _cts = new();

    // Walker calendar (created at runtime)
    private WalkCalendarPanel _calWalker = null!;
    private IEnumerable<WalkerAvailabilityDto> _cachedAvailability = [];

    // Client controls (created at runtime)
    private DataGridView      _dgvMyDogs = null!;
    private WalkCalendarPanel _calClient = null!;

    // Walker availability config controls (created at runtime)
    private TextBox      _cfgPhone  = null!;
    private TextBox      _cfgEmail  = null!;
    private Label        _cfgStatus = null!;

    // LAN notification service
    private readonly INotificationService _notifier;

    // Walk Events pagination
    private int _walksPage = 1;
    private const int WalksPageSize = 10;

    public MainForm(IServiceScopeFactory scopeFactory)
    {
        _scope    = scopeFactory.CreateScope();
        var sp    = _scope.ServiceProvider;
        _clients  = sp.GetRequiredService<IClientService>();
        _walks    = sp.GetRequiredService<IWalkEventService>();
        _users    = sp.GetRequiredService<IUserService>();
        _auth     = sp.GetRequiredService<IAuthService>();
        _dogSvc   = sp.GetRequiredService<IDogService>();
        _availSvc = sp.GetRequiredService<IWalkerAvailabilityService>();
        _notifier = sp.GetRequiredService<INotificationService>();
        InitializeComponent();

        // Populate walk status combo (Designer created the control, we fill items here)
        cmbWalkStatus.Items.AddRange(Enum.GetNames<WalkStatus>());
        if (cmbWalkStatus.Items.Count > 0)
            cmbWalkStatus.SelectedIndex = 0;

        // Start with no tabs — ShowTabs will add the right ones on login
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

    public void SetSession(int userId, string fullName, string role, Action? onLogout = null)
    {
        _userId   = userId;
        _fullName = fullName;
        _userRole = role;
        _onLogout = onLogout;
        lblSession.Text = $"  {fullName}  [{role}]";
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

        // Subscribe to LAN notifications
        _notifier.NotificationReceived -= OnNotificationReceived; // avoid double-subscribe
        _notifier.NotificationReceived += OnNotificationReceived;
    }

    private void OnNotificationReceived(WalkNotification n)
    {
        // Only notify the dog owner when a walker accepts their walk
        if (_userRole != "Client") return;
        if (n.ClientId != _clientId) return;
        if (n.Type is not NotificationType.WalkAccepted
                   and not NotificationType.WalkClaimed) return;

        if (InvokeRequired)
            BeginInvoke(() => OnWalkAcceptedByWalker(n));
        else
            OnWalkAcceptedByWalker(n);
    }

    private async void OnWalkAcceptedByWalker(WalkNotification n)
    {
        new ToastNotification(this, "Walk Accepted", n.Message, Color.SeaGreen).Show();
        await LoadClientWalksAsync();
    }

    // ── Role-based tab setup ─────────────────────────────────────────

    private void ShowTabs(params TabPage[] pages)
    {
        tabs.TabPages.Clear();
        tabs.TabPages.AddRange(pages);
    }

    private void BtnLogout_Click(object? sender, EventArgs e) => Logout();

    private void Logout()
    {
        _notifier.NotificationReceived -= OnNotificationReceived;
        _onLogout?.Invoke();  // Re-shows the LoginForm
        tabs.TabPages.Clear();
        _userId = 0; _userRole = string.Empty; _fullName = string.Empty; _clientId = 0;
        lblSession.Text = string.Empty;
        _onLogout = null;
        Hide();
    }

    // ════════════════════════════════════════════════════════════════
    // ADMIN — Event Handlers (wired in Designer)
    // ════════════════════════════════════════════════════════════════

    private async void TxtSearchClients_TextChanged(object? sender, EventArgs e)
        => await LoadClientsAsync(txtSearchClients.Text);

    private async void BtnRefreshClients_Click(object? sender, EventArgs e)
        => await LoadClientsAsync();

    private async void TxtSearchWalks_TextChanged(object? sender, EventArgs e)
    { _walksPage = 1; await LoadWalksAsync(); }

    private async void CmbWalkStatus_SelectedIndexChanged(object? sender, EventArgs e)
    { _walksPage = 1; await LoadWalksAsync(); }

    private async void BtnWalksPrev_Click(object? sender, EventArgs e)
    { if (_walksPage > 1) { _walksPage--; await LoadWalksAsync(); } }

    private async void BtnWalksNext_Click(object? sender, EventArgs e)
    { _walksPage++; await LoadWalksAsync(); }

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

    // ════════════════════════════════════════════════════════════════
    // ADMIN DATA LOADING
    // ════════════════════════════════════════════════════════════════

    private async Task LoadClientsAsync(string search = "")
    {
        try
        {
            var list = string.IsNullOrWhiteSpace(search)
                ? await _clients.GetAllActiveAsync(_cts.Token)
                : await _clients.SearchAsync(search, _cts.Token);

            dgvClients.DataSource = list.Select(c => new
            {
                c.Id, c.Name, Phone = c.PhoneNumber, c.Email,
                Subscription = c.Subscription.ToString(), Dogs = c.DogCount
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
            var search = string.IsNullOrWhiteSpace(txtSearchWalks.Text) ? null : txtSearchWalks.Text.Trim();
            var paged  = await _walks.GetByStatusPagedAsync(status, _walksPage, WalksPageSize, search, _cts.Token);

            // Clamp page if it overshoots (e.g. after deleting the last item on a page)
            if (_walksPage > paged.TotalPages && paged.TotalPages > 0)
            {
                _walksPage = paged.TotalPages;
                paged = await _walks.GetByStatusPagedAsync(status, _walksPage, WalksPageSize, search, _cts.Token);
            }

            dgvWalks.DataSource = WalkRows(paged.Items);
            HideCol(dgvWalks, "Id");

            lblWalksPage.Text    = $"Page {paged.Page} of {Math.Max(1, paged.TotalPages)} ({paged.TotalCount} total)";
            btnWalksPrev.Enabled = paged.HasPreviousPage;
            btnWalksNext.Enabled = paged.HasNextPage;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { ShowError(ex.Message); }
    }

    private Task LoadWalkersAsync() => RunAsync(async () =>
    {
        var list = await _users.GetWalkersAsync();
        dgvWalkers.DataSource = list.Select(u => new
        {
            u.Id, u.FullName, Username = u.Username,
            Phone = u.Phone ?? "\u2014", Email = u.Email ?? "\u2014", Active = u.IsActive
        }).ToList();
        HideCol(dgvWalkers, "Id");
    });

    private Task LoadAllUsersAsync() => RunAsync(async () =>
    {
        var list = await _users.GetAllAsync();
        dgvAllUsers.DataSource = list.Select(u => new
        {
            u.Id, u.FullName, u.Username, Role = u.Role.ToString(), Active = u.IsActive
        }).ToList();
        HideCol(dgvAllUsers, "Id");
    });

    private static TextBox InlineField(Form form, string label, int y, bool isPassword = false)
    {
        form.Controls.Add(new Label { Text = label, Location = new Point(16, y), AutoSize = true });
        var tb = new TextBox
        {
            Location = new Point(16, y + 18), Width = 310,
            UseSystemPasswordChar = isPassword
        };
        form.Controls.Add(tb);
        return tb;
    }

    // ════════════════════════════════════════════════════════════════
    // WALKER TABS
    // ════════════════════════════════════════════════════════════════

    private void PopulateMyScheduleTab()
    {
        tabMySchedule.Controls.Clear();
        tabMySchedule.Enter -= TabMySchedule_Enter;
        tabMySchedule.Enter += TabMySchedule_Enter;

        // Merge: walks directly assigned to me + open requests matching my zone/availability
        _calWalker = new WalkCalendarPanel(async () =>
        {
            var assigned = await _walks.GetByWalkerIdAsync(_userId);
            var matching = await _walks.GetMatchingRequestsForWalkerAsync(_userId);
            return assigned.Concat(matching).DistinctBy(w => w.Id);
        })
        {
            ShowRespondButton       = true,
            HideWalkerColumn        = true,
            ShowClientAddressColumn = true
        };

        _calWalker.RespondButtonClicked += async walk => await ShowWalkRespondDialogAsync(walk);

        var ctx = new ContextMenuStrip();
        ctx.Items.Add("\u25b6 Start Walk",    null, async (_, _) =>
            await ChangeWalkStatus(_calWalker.DetailGrid, WalkStatus.InProgress, LoadMyScheduleAsync));
        ctx.Items.Add("\u2714 Complete Walk", null, async (_, _) =>
            await ChangeWalkStatus(_calWalker.DetailGrid, WalkStatus.Completed, LoadMyScheduleAsync));
        ctx.Items.Add("\u21a9 Release Walk",  null, async (_, _) =>
            await ReleaseAcceptedWalkAsync());
        ctx.Items.Add("\u274c Cancel Walk",  null, async (_, _) =>
            await CancelWalkWithNoteAsync(_calWalker.DetailGrid, LoadMyScheduleAsync));
        _calWalker.DetailGrid.ContextMenuStrip = ctx;

        // ── Summary panel: dog count + availability ───────────────────
        var pnlSummary = new Panel
        {
            Dock      = DockStyle.Bottom,
            Height    = 50,
            Padding   = new Padding(8, 6, 8, 6),
            BackColor = Color.FromArgb(248, 248, 248)
        };
        var lblDogCount = new Label
        {
            Text     = "Dogs to walk: 0",
            Location = new Point(8, 4),
            AutoSize = true,
            Font     = new Font("Segoe UI", 9, FontStyle.Bold)
        };
        var lblAvail = new Label
        {
            Text     = "Your availability: --",
            Location = new Point(8, 24),
            AutoSize = true,
            Font     = new Font("Segoe UI", 9)
        };
        pnlSummary.Controls.AddRange(new Control[] { lblDogCount, lblAvail });

        _calWalker.DaySelected += (day, walks) =>
        {
            int count = walks.Count(w => w.Status is WalkStatus.Accepted or WalkStatus.InProgress);
            lblDogCount.Text = $"Dogs to walk: {count}";

            if (day > 0 && _calWalker.SelectedDate.HasValue)
            {
                var dow = _calWalker.SelectedDate.Value.DayOfWeek;
                var match = _cachedAvailability.FirstOrDefault(s => s.DayOfWeek == dow);
                lblAvail.Text = match != null
                    ? $"Your availability: {match.StartTime:HH:mm} \u2013 {match.EndTime:HH:mm} ({match.Zone})"
                    : "Your availability: No availability set";
            }
            else
            {
                lblAvail.Text = "Your availability: --";
            }
        };

        tabMySchedule.Controls.Add(_calWalker);
        tabMySchedule.Controls.Add(pnlSummary);
    }

    private Task LoadMyScheduleAsync() => RunAsync(async () =>
    {
        await _calWalker.LoadAsync();
        _cachedAvailability = await _availSvc.GetByWalkerIdAsync(_userId);
    });

    /// <summary>
    /// Walker releases an accepted walk: prompts for an optional reason,
    /// then returns the walk to Requested so another walker can claim it.
    /// </summary>
    private async Task ReleaseAcceptedWalkAsync()
    {
        if (SelectedId(_calWalker.DetailGrid) is not int id) return;

        var dlg = new Form
        {
            Text            = "Release Walk",
            Size            = new Size(420, 200),
            StartPosition   = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox     = false,
            BackColor       = Color.WhiteSmoke
        };
        dlg.Controls.Add(new Label
        {
            Text     = "Reason for releasing (optional):",
            Location = new Point(16, 16), AutoSize = true
        });
        var txtNote = new TextBox { Location = new Point(16, 38), Width = 374, Height = 60, Multiline = true };
        dlg.Controls.Add(txtNote);

        bool confirmed = false;
        var btnOk = new Button
        {
            Text      = "\u21a9 Release Walk",
            Location  = new Point(16, 118), Width = 140, Height = 28,
            BackColor = Color.FromArgb(180, 100, 20), ForeColor = Color.White, FlatStyle = FlatStyle.Flat
        };
        btnOk.Click += (_, _) => { confirmed = true; dlg.DialogResult = DialogResult.OK; dlg.Close(); };
        var btnAbort = new Button
        {
            Text      = "Go Back",
            Location  = new Point(172, 118), Width = 100, Height = 28,
            FlatStyle = FlatStyle.Flat
        };
        btnAbort.Click += (_, _) => dlg.Close();
        dlg.Controls.AddRange(new Control[] { txtNote, btnOk, btnAbort });
        dlg.ShowDialog(this);

        if (!confirmed) return;
        try
        {
            var note = string.IsNullOrWhiteSpace(txtNote.Text) ? null : txtNote.Text.Trim();
            await _walks.UnacceptWalkAsync(id, note);
            await LoadMyScheduleAsync();
        }
        catch (Exception ex) { ShowError(ex.Message); }
    }

    private void PopulateMyAvailabilityTab()
    {
        tabMyAvailability.Controls.Clear();

        // ── Manage Availability button ──────────────────────────────────
        tabMyAvailability.Controls.Add(new Label
        {
            Text = "Availability Slots",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            Location = new Point(12, 12), AutoSize = true
        });

        var btnManage = new Button
        {
            Text      = "Manage Availability Windows...",
            Location  = new Point(12, 42),
            Width     = 260,
            Height    = 32,
            BackColor = Color.FromArgb(30, 70, 150),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Segoe UI", 9)
        };
        btnManage.Click += (_, _) =>
        {
            using var dlg = new WalkerAvailabilityForm(_availSvc, _userId, _fullName);
            dlg.ShowDialog();
        };
        tabMyAvailability.Controls.Add(btnManage);

        // ── Separator ─────────────────────────────────────────────────────
        int y = 90;
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

    // ════════════════════════════════════════════════════════════════
    // CLIENT TABS
    // ════════════════════════════════════════════════════════════════

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
        _dgvMyDogs.Size     = new Size(1005, 525);

        var ctx = new ContextMenuStrip();
        ctx.Items.Add("\u270f\ufe0f Edit",   null, async (_, _) => await ShowMyDogDialogAsync(SelectedId(_dgvMyDogs)));
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
                d.Id, d.Name, d.Breed,
                BirthDate = d.BirthDate.ToString("yyyy-MM-dd"),
                Age       = $"{d.AgeInYears} yrs"
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

        _calClient = new WalkCalendarPanel(() => _walks.GetByClientIdAsync(_clientId))
        {
            HideClientColumn     = true,
            ShowWalkerInfoButton = true
        };

        _calClient.WalkerInfoButtonClicked += async walk => await ShowWalkerInfoFromWalkAsync(walk);

        var ctx = new ContextMenuStrip();
        ctx.Items.Add("\u274c Cancel Walk", null, async (_, _) =>
            await CancelWalkWithNoteAsync(_calClient.DetailGrid, LoadClientWalksAsync));
        ctx.Items.Add("\U0001f5d1\ufe0f Delete Walk", null, async (_, _) => await DeleteMyWalkAsync());
        _calClient.DetailGrid.ContextMenuStrip = ctx;

        var pnl = new Panel { Dock = DockStyle.Top, Height = 46, Padding = new Padding(8, 8, 0, 0) };
        pnl.Controls.AddRange(new Control[]
        {
            Btn("+ Request Walk", 0, async () =>
            {
                var f = Program.ServiceProvider.GetRequiredService<WalkEventForm>();
                f.LockToClient(_clientId);
                if (_calClient.SelectedDate.HasValue)
                    f.PreSelectDate(_calClient.SelectedDate.Value);
                if (f.ShowDialog() == DialogResult.OK)
                    await LoadClientWalksAsync();
            }),
            Btn("\u21bb Refresh", 140, async () => await LoadClientWalksAsync()),
            Btn("\U0001f5d1\ufe0f Delete",  260, async () => await DeleteMyWalkAsync())
        });

        // ── Remaining walks panel ─────────────────────────────────────
        var pnlRemaining = new Panel
        {
            Dock      = DockStyle.Bottom,
            Height    = 35,
            Padding   = new Padding(8, 6, 8, 6),
            BackColor = Color.FromArgb(248, 248, 248)
        };
        var lblRemaining = new Label
        {
            Text     = "Remaining walks this month: --",
            Location = new Point(8, 6),
            AutoSize = true,
            Font     = new Font("Segoe UI", 9, FontStyle.Bold)
        };
        pnlRemaining.Controls.Add(lblRemaining);

        _calClient.WalksLoaded += async (month, _) =>
        {
            try
            {
                var summary = await _walks.GetMonthlySummaryAsync(_clientId, month.Year, month.Month);
                lblRemaining.Text = $"Remaining walks this month: {summary.Remaining} / {summary.MaxWalksPerMonth} ({summary.PlanDescription})";
            }
            catch { /* best-effort display */ }
        };

        tabMyWalks.Controls.Add(_calClient);
        tabMyWalks.Controls.Add(pnlRemaining);
        tabMyWalks.Controls.Add(pnl);
    }

    private Task LoadClientWalksAsync() => RunAsync(() => _calClient.LoadAsync());

    private async Task DeleteMyWalkAsync()
    {
        if (SelectedId(_calClient.DetailGrid) is not int id) return;
        if (MessageBox.Show("Delete this walk? This cannot be undone.",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        try { await _walks.DeleteAsync(id); await LoadClientWalksAsync(); }
        catch (Exception ex) { ShowError(ex.Message); }
    }

    /// <summary>
    /// Shows walker contact info (name, phone, email, zones from availability slots) for the given walk.
    /// </summary>
    private async Task ShowWalkerInfoFromWalkAsync(WalkEventDto walk)
    {
        if (!walk.WalkerId.HasValue || string.IsNullOrEmpty(walk.WalkerName))
        {
            MessageBox.Show("No walker has been assigned to this walk yet.",
                            "Walker Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        try
        {
            var walkerUser = await _users.GetByIdAsync(walk.WalkerId.Value);
            var slots      = await _availSvc.GetByWalkerIdAsync(walk.WalkerId.Value);
            var zoneList   = string.Join(", ", slots.Select(s => s.Zone).Where(z => !string.IsNullOrWhiteSpace(z)).Distinct());

            var info = $"Name:           {walk.WalkerName}\n" +
                       $"Phone:          {walkerUser?.Phone ?? "\u2014"}\n" +
                       $"Email:          {walkerUser?.Email ?? "\u2014"}\n" +
                       $"Working zones:  {(string.IsNullOrEmpty(zoneList) ? "\u2014" : zoneList)}";

            MessageBox.Show(info, "Walker Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { ShowError(ex.Message); }
    }

    /// <summary>
    /// Dialog for a walker to accept or decline a walk request/proposal.
    /// For Requested walks, accepting calls ClaimWalkAsync (atomic assign + accept).
    /// For Proposed walks, accepting calls WalkerRespondAsync.
    /// </summary>
    private async Task ShowWalkRespondDialogAsync(WalkEventDto walk)
    {
        bool isProposed = walk.Status == WalkStatus.Proposed;

        var dlg = new Form
        {
            Text            = isProposed ? "Respond to Proposal" : "Open Walk Request",
            Size            = new Size(450, 300),
            StartPosition   = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox     = false,
            BackColor       = Color.WhiteSmoke
        };

        dlg.Controls.Add(new Label
        {
            Text     = $"Dog: {walk.DogName}  \u2022  {walk.WalkDate.ToLocalTime():yyyy-MM-dd HH:mm}  \u2022  {walk.Location}",
            Location = new Point(16, 14), AutoSize = true,
            Font     = new Font("Segoe UI", 9, FontStyle.Bold)
        });
        dlg.Controls.Add(new Label
        {
            Text      = $"Status: {walk.Status}  \u2022  Client: {walk.ClientName}",
            Location  = new Point(16, 36), AutoSize = true,
            ForeColor = Color.DimGray, Font = new Font("Segoe UI", 8)
        });

        dlg.Controls.Add(new Label
        {
            Text     = isProposed ? "Decline reason (required):" : "Decline reason (optional):",
            Location = new Point(16, 62), AutoSize = true
        });
        var txtNote = new TextBox { Location = new Point(16, 82), Width = 404, Height = 60, Multiline = true };
        dlg.Controls.Add(txtNote);

        var lblErr = new Label
        {
            ForeColor = Color.Crimson, Location = new Point(16, 150),
            Size = new Size(404, 18), Visible = false
        };
        dlg.Controls.Add(lblErr);

        bool   decided       = false;
        bool   accepted      = false;
        string rejectionNote = string.Empty;

        var btnAccept = new Button
        {
            Text = "\u2714 Accept", Location = new Point(16, 178), Width = 120, Height = 30,
            BackColor = Color.FromArgb(40, 140, 40), ForeColor = Color.White, FlatStyle = FlatStyle.Flat
        };
        btnAccept.Click += (_, _) => { decided = true; accepted = true; dlg.Close(); };
        dlg.Controls.Add(btnAccept);

        var btnDecline = new Button
        {
            Text = "\u2718 Decline", Location = new Point(152, 178), Width = 120, Height = 30,
            BackColor = Color.FromArgb(180, 40, 40), ForeColor = Color.White, FlatStyle = FlatStyle.Flat
        };
        btnDecline.Click += (_, _) =>
        {
            if (isProposed && string.IsNullOrWhiteSpace(txtNote.Text))
            { lblErr.Text = "Please provide a rejection reason."; lblErr.Visible = true; return; }
            decided = true; accepted = false;
            rejectionNote = txtNote.Text.Trim();
            dlg.Close();
        };
        dlg.Controls.Add(btnDecline);

        var btnCancel = new Button
        {
            Text = "Cancel", Location = new Point(296, 178), Width = 120, Height = 30,
            FlatStyle = FlatStyle.Flat
        };
        btnCancel.Click += (_, _) => dlg.Close();
        dlg.Controls.Add(btnCancel);

        dlg.ShowDialog(this);

        if (!decided) return;

        try
        {
            if (accepted)
            {
                if (walk.Status == WalkStatus.Requested)
                    await _walks.ClaimWalkAsync(walk.Id, _userId);
                else
                    await _walks.WalkerRespondAsync(new WalkerResponseDto(walk.Id, _userId, true));
            }
            else
            {
                await _walks.WalkerRespondAsync(new WalkerResponseDto(walk.Id, _userId, false, rejectionNote));
            }
            await LoadMyScheduleAsync();
        }
        catch (Exception ex) { ShowError(ex.Message); }
    }

    private void PopulateMySubscriptionTab(ClientDto client)
    {
        tabMySubscription.Controls.Clear();

        var strategy = WalkLimitStrategyFactory.Create(client.Subscription);

        var lblPlan = new Label
        {
            Text      = $"Current Plan: {strategy.Description}",
            Font      = new Font("Segoe UI", 13, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 70, 150),
            AutoSize  = true, Location = new Point(40, 60)
        };
        var lblLimits = new Label
        {
            Text     = $"\u2022 Up to {strategy.MaxWalksPerMonth} walks per month\n" +
                       $"\u2022 {(strategy.AllowsMultiplePerDay ? "Unlimited walks per day" : "Max 1 walk per day")}",
            Font     = new Font("Segoe UI", 10),
            AutoSize = true, Location = new Point(40, 100)
        };
        var lblAddress = new Label
        {
            Text      = $"Address: {(string.IsNullOrWhiteSpace(client.Address) ? "\u2014" : client.Address)}",
            Font      = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(60, 60, 60),
            AutoSize  = true, Location = new Point(40, 148)
        };

        var btnChange = new Button
        {
            Text      = "Change Plan",
            Location  = new Point(40, 188),
            Size      = new Size(160, 34),
            BackColor = Color.FromArgb(30, 70, 150),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Segoe UI", 10)
        };
        btnChange.FlatAppearance.BorderSize = 0;
        btnChange.Click += async (_, _) =>
        {
            await ChangeSubscriptionAsync(_clientId);
            var updated = await _clients.GetByUserIdAsync(_userId);
            if (updated is null) return;
            var s = WalkLimitStrategyFactory.Create(updated.Subscription);
            lblPlan.Text   = $"Current Plan: {s.Description}";
            lblLimits.Text = $"\u2022 Up to {s.MaxWalksPerMonth} walks per month\n" +
                             $"\u2022 {(s.AllowsMultiplePerDay ? "Unlimited walks per day" : "Max 1 walk per day")}";
        };

        var btnContact = new Button
        {
            Text      = "Edit Contact Info (Phone / Email)",
            Location  = new Point(40, 228),
            Size      = new Size(310, 34),
            BackColor = Color.FromArgb(80, 120, 200),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Segoe UI", 10)
        };
        btnContact.FlatAppearance.BorderSize = 0;
        btnContact.Click += async (_, _) => await EditClientContactInfoAsync();

        tabMySubscription.Controls.AddRange(new Control[] { lblPlan, lblLimits, lblAddress, btnChange, btnContact });
    }

    private async Task EditClientContactInfoAsync()
    {
        var current = await _clients.GetByIdAsync(_clientId);

        var dlg = new Form
        {
            Text = "Edit Contact Info", Size = new Size(360, 220),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false, BackColor = Color.WhiteSmoke
        };
        var txtPhone = InlineField(dlg, "Phone *", 16);
        var txtEmail = InlineField(dlg, "Email *", 76);
        txtPhone.Text = current?.PhoneNumber ?? string.Empty;
        txtEmail.Text = current?.Email       ?? string.Empty;

        var lblErr = new Label
        {
            ForeColor = Color.Crimson, Location = new Point(16, 136),
            Size = new Size(310, 20), Visible = false
        };
        dlg.Controls.Add(lblErr);

        bool saved = false;
        var btnSave = new Button
        {
            Text = "Save", Location = new Point(16, 136), Width = 120, Height = 28,
            BackColor = Color.FromArgb(30, 70, 150), ForeColor = Color.White, FlatStyle = FlatStyle.Flat
        };
        btnSave.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(txtPhone.Text) || string.IsNullOrWhiteSpace(txtEmail.Text))
            { lblErr.Text = "Phone and email are required."; lblErr.Visible = true; return; }
            saved = true; dlg.DialogResult = DialogResult.OK; dlg.Close();
        };
        dlg.Controls.Add(btnSave);

        var btnCancel = new Button { Text = "Cancel", Location = new Point(152, 136), Width = 80, Height = 28, FlatStyle = FlatStyle.Flat };
        btnCancel.Click += (_, _) => dlg.Close();
        dlg.Controls.Add(btnCancel);

        dlg.ShowDialog(this);
        if (!saved) return;

        try
        {
            await _clients.UpdateContactInfoAsync(_clientId, txtPhone.Text.Trim(), txtEmail.Text.Trim());
            MessageBox.Show("Contact info updated.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { ShowError(ex.Message); }
    }

    // ════════════════════════════════════════════════════════════════
    // SHARED HELPERS
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Shows subscription picker. clientId = null -> reads from selected Admin grid row.
    /// Uses sync button capture to avoid EF concurrency errors.
    /// </summary>
    private async Task ChangeSubscriptionAsync(int? clientId)
    {
        int id = clientId ?? SelectedId(dgvClients) ?? -1;
        if (id < 0) return;

        var strategies = WalkLimitStrategyFactory.GetAll().ToList();
        var picker = new Form
        {
            Text = "Change Subscription", Size = new Size(360, 170),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false
        };
        var cmb = new ComboBox
        {
            Location = new Point(20, 20), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList
        };
        foreach (var s in strategies) cmb.Items.Add(s.Description);
        cmb.SelectedIndex = 0;

        int selectedIndex = -1;
        var btnOk = new Button
        {
            Text = "Apply", Location = new Point(20, 70), Width = 120, Height = 28,
            BackColor = Color.FromArgb(30, 70, 150), ForeColor = Color.White, FlatStyle = FlatStyle.Flat
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

        var dlg = new Form
        {
            Text = "Cancel Walk", Size = new Size(420, 200),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false,
            BackColor = Color.WhiteSmoke
        };
        dlg.Controls.Add(new Label { Text = "Reason for cancellation (optional):", Location = new Point(16, 16), AutoSize = true });
        var txtNote = new TextBox { Location = new Point(16, 38), Width = 374, Height = 60, Multiline = true };
        dlg.Controls.Add(txtNote);

        bool confirmed = false;
        var btnOk = new Button
        {
            Text = "Cancel Walk", Location = new Point(16, 118), Width = 130, Height = 28,
            BackColor = Color.FromArgb(180, 40, 40), ForeColor = Color.White, FlatStyle = FlatStyle.Flat
        };
        btnOk.Click += (_, _) => { confirmed = true; dlg.DialogResult = DialogResult.OK; dlg.Close(); };
        var btnAbort = new Button { Text = "Go Back", Location = new Point(162, 118), Width = 100, Height = 28, FlatStyle = FlatStyle.Flat };
        btnAbort.Click += (_, _) => dlg.Close();
        dlg.Controls.AddRange(new Control[] { txtNote, btnOk, btnAbort });
        dlg.ShowDialog(this);

        if (!confirmed) return;
        try
        {
            await _walks.CancelWithNoteAsync(id, string.IsNullOrWhiteSpace(txtNote.Text) ? null : txtNote.Text.Trim());
            await reload();
        }
        catch (Exception ex) { ShowError(ex.Message); }
    }

    private static IList<object> WalkRows(IEnumerable<WalkEventDto> list) =>
        list.Select(w => (object)new
        {
            w.Id,
            Dog             = w.DogName,
            Client          = w.ClientName,
            ClientAddress   = w.ClientAddress,
            Walker          = w.WalkerName ?? "\u2014",
            Date            = w.WalkDate.ToLocalTime().ToString("yyyy-MM-dd HH:mm"),
            Duration        = $"{w.DurationMinutes} min",
            w.Location,
            ETA             = w.WalkerId.HasValue
                              ? $"\u2248{Random.Shared.Next(5, 21)} min"
                              : "\u2014",
            Status          = w.Status.ToString(),
            w.Notes
        }).ToList();

    private static DataGridView Grid() => new()
    {
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

    private static Button Btn(string text, int x, Func<Task> onClick)
    {
        var b = new Button
        {
            Text      = text, Location = new Point(x, 5), Height = 28, AutoSize = true,
            BackColor = Color.FromArgb(30, 70, 150), ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9)
        };
        b.Click += async (_, _) => await onClick();
        return b;
    }

    private static int? SelectedId(DataGridView dgv)
        => dgv.CurrentRow?.Cells["Id"].Value is int id ? id : null;

    private static void HideCol(DataGridView dgv, string col)
    {
        if (dgv.Columns.Contains(col)) dgv.Columns[col].Visible = false;
    }

    private void ShowError(string msg)
        => MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);

    /// <summary>
    /// Runs an async action, showing a MessageBox on failure.
    /// Silently swallows OperationCanceledException (form closing).
    /// Replaces per-method try/catch boilerplate.
    /// </summary>
    private async Task RunAsync(Func<Task> action)
    {
        var result = await action().ToResultAsync();
        if (!result.IsSuccess) ShowError(result.Error!);
    }
}
