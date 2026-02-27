using System.ComponentModel;
using System.Globalization;
using DogWalking.Application.DTOs;
using DogWalking.Domain.Enums;

namespace DogWalking.WinForms.Controls;

/// <summary>
/// Reusable monthly calendar panel.
/// Shows a 7×7 grid (header row + up to 6 week rows) of day buttons.
/// Clicking a day populates the detail DataGridView below with that day's walks,
/// colour-coded by status. Supports optional Respond and Walker Info action buttons.
/// </summary>
public sealed class WalkCalendarPanel : Panel
{
    // ── Public API ──────────────────────────────────────────────────────────

    /// <summary>Detail DataGridView — exposed so the host can attach a context menu.</summary>
    public DataGridView DetailGrid => _dgv;

    /// <summary>When true, hides the Client column (use for the Client role view).</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool HideClientColumn { get; set; } = false;

    /// <summary>When true, hides the Walker column (use for the Walker role view).</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool HideWalkerColumn { get; set; } = false;

    /// <summary>When true, shows the Client Address column.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool ShowClientAddressColumn { get; set; } = false;

    /// <summary>
    /// When true, shows a "Respond" button on Requested/Proposed rows so the walker
    /// can accept or reject the walk request.
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool ShowRespondButton { get; set; } = false;

    /// <summary>
    /// When true, shows a "Walker Info" button on rows that have an assigned walker.
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool ShowWalkerInfoButton { get; set; } = false;

    /// <summary>
    /// Returns the currently selected date, or null if no day is selected.
    /// </summary>
    public DateTime? SelectedDate =>
        _selectedDay > 0 ? _month.AddDays(_selectedDay - 1) : null;

    /// <summary>Fired when the walker clicks "Respond" on a proposal row.</summary>
    public event Action<WalkEventDto>? RespondButtonClicked;

    /// <summary>Fired when the client clicks "Walker Info" on an accepted walk row.</summary>
    public event Action<WalkEventDto>? WalkerInfoButtonClicked;

    // ── Private state ───────────────────────────────────────────────────────

    private readonly Func<Task<IEnumerable<WalkEventDto>>> _loader;
    private List<WalkEventDto> _walks    = new();
    private List<WalkEventDto> _dayWalks = new();   // walks for the currently selected day
    private DateTime _month;
    private int      _selectedDay;                  // 0 = none selected

    private readonly Label            _lblMonth;
    private readonly Button           _btnPrev;
    private readonly Button           _btnNext;
    private readonly TableLayoutPanel _grid;
    private readonly DataGridView     _dgv;

    private static readonly string[] DayHeaders = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };

    // Calendar cell background colours
    private static readonly Color ColRequested  = Color.FromArgb(255, 243, 205);
    private static readonly Color ColAccepted   = Color.FromArgb(198, 239, 206);
    private static readonly Color ColInProgress = Color.FromArgb(155, 194, 230);
    private static readonly Color ColMixed      = Color.FromArgb(230, 215, 255);
    private static readonly Color ColEmpty      = Color.FromArgb(245, 245, 245);
    private static readonly Color ColOutside    = Color.FromArgb(220, 220, 220);

    // Detail grid row colours
    private static readonly Color RowProposal   = Color.FromArgb(255, 243, 205); // yellow
    private static readonly Color RowAccepted   = Color.FromArgb(198, 239, 206); // green
    private static readonly Color RowInProgress = Color.FromArgb(155, 194, 230); // blue
    private static readonly Color RowCompleted  = Color.FromArgb(235, 235, 235); // light grey
    private static readonly Color RowCancelled  = Color.FromArgb(220, 220, 220); // grey

    // Action button colours
    private static readonly Color BtnBlue = Color.FromArgb(30, 70, 150);

    public WalkCalendarPanel(Func<Task<IEnumerable<WalkEventDto>>> loader)
    {
        _loader = loader;
        _month  = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        Dock    = DockStyle.Fill;

        // ── Navigation bar ──────────────────────────────────────────────────
        var nav = new Panel { Dock = DockStyle.Top, Height = 36, Padding = new Padding(4) };

        _btnPrev = new Button { Text = "\u25c0", Width = 30, Dock = DockStyle.Left,  FlatStyle = FlatStyle.Flat };
        _btnNext = new Button { Text = "\u25b6", Width = 30, Dock = DockStyle.Right, FlatStyle = FlatStyle.Flat };
        _lblMonth = new Label
        {
            Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        _btnPrev.Click += async (_, _) => { _month = _month.AddMonths(-1); _selectedDay = 0; await LoadAsync(); };
        _btnNext.Click += async (_, _) => { _month = _month.AddMonths(1);  _selectedDay = 0; await LoadAsync(); };

        var btnRefresh = new Button
        {
            Text      = "\u21bb Refresh",
            Width     = 80,
            Dock      = DockStyle.Right,
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Segoe UI", 8)
        };
        btnRefresh.Click += async (_, _) => await LoadAsync();

        nav.Controls.Add(_lblMonth);
        nav.Controls.Add(_btnPrev);
        nav.Controls.Add(btnRefresh);
        nav.Controls.Add(_btnNext);

        // ── Calendar grid (TableLayoutPanel) ────────────────────────────────
        _grid = new TableLayoutPanel
        {
            Dock = DockStyle.Top, Height = 240, ColumnCount = 7, RowCount = 7,
            CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
        };
        for (int c = 0; c < 7; c++)
            _grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 7));
        _grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        for (int r = 1; r <= 6; r++)
            _grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / 6));

        for (int c = 0; c < 7; c++)
        {
            var lbl = new Label
            {
                Text = DayHeaders[c], Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                BackColor = Color.FromArgb(70, 130, 180), ForeColor = Color.White
            };
            _grid.Controls.Add(lbl, c, 0);
        }

        for (int r = 1; r <= 6; r++)
        for (int c = 0; c < 7; c++)
        {
            var btn = new Button
            {
                Dock = DockStyle.Fill, FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8), Margin = new Padding(1), Tag = (int?)null
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += OnDayClicked;
            _grid.Controls.Add(btn, c, r);
        }

        // ── Detail DataGridView ─────────────────────────────────────────────
        _dgv = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly              = true,
            AllowUserToAddRows    = false,
            AllowUserToDeleteRows = false,
            SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
            AutoGenerateColumns   = false,   // we define columns explicitly
            RowHeadersVisible     = false,
            BackgroundColor       = Color.White
        };

        // Bind columns (DataPropertyName must match WalkCalendarRow property names)
        AddTextCol("Id",            "Id",            visible: false);
        AddTextCol("Dog",           "Dog",           "Dog");
        AddTextCol("Client",        "Client",        "Client");
        AddTextCol("Walker",        "Walker",        "Walker");
        AddTextCol("ClientAddress", "ClientAddress", "Address");
        AddTextCol("Time",          "Time",          "Time",     fillWeight: 55);
        AddTextCol("Duration",      "Duration",      "Duration", fillWeight: 65);
        AddTextCol("ETA",           "ETA",           "ETA",      fillWeight: 55);
        AddTextCol("Status",        "Status",        "Status",   fillWeight: 75);
        AddTextCol("Location",      "Location",      "Location");
        AddTextCol("Notes",         "Notes",         "Notes");
        AddTextCol("WalkerId",      "WalkerId",      visible: false);

        // Optional action button columns (always created, visibility set per view)
        _dgv.Columns.Add(new DataGridViewButtonColumn
        {
            Name = "Respond", HeaderText = string.Empty, Width = 80,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
            FlatStyle = FlatStyle.Flat, Visible = false
        });
        _dgv.Columns.Add(new DataGridViewButtonColumn
        {
            Name = "WalkerInfo", HeaderText = string.Empty, Width = 90,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
            FlatStyle = FlatStyle.Flat, Visible = false
        });

        _dgv.CellContentClick += OnDetailCellContentClick;

        Controls.Add(_dgv);
        Controls.Add(_grid);
        Controls.Add(nav);

        RebuildGrid();
        UpdateMonthLabel();
    }

    // ── Public methods ───────────────────────────────────────────────────────

    public async Task LoadAsync()
    {
        _walks = (await _loader()).ToList();
        // Only auto-select today on the first load; subsequent reloads preserve the selected day
        if (_selectedDay == 0)
        {
            bool todayIsVisible = DateTime.Today.Year  == _month.Year &&
                                  DateTime.Today.Month == _month.Month;
            _selectedDay = todayIsVisible ? DateTime.Today.Day : 0;
        }
        RebuildGrid();
        UpdateMonthLabel();
        ShowDayWalks(_selectedDay);
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private void AddTextCol(string name, string dataProp, string? headerText = null,
                             bool visible = true, float fillWeight = 100)
    {
        _dgv.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = name, DataPropertyName = dataProp,
            HeaderText = headerText ?? name, Visible = visible,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight   = fillWeight
        });
    }

    private void UpdateMonthLabel() =>
        _lblMonth.Text = _month.ToString("MMMM yyyy", CultureInfo.InvariantCulture);

    private void RebuildGrid()
    {
        var byDay = _walks
            .Where(w => w.WalkDate.ToLocalTime().Year  == _month.Year &&
                        w.WalkDate.ToLocalTime().Month == _month.Month)
            .GroupBy(w => w.WalkDate.ToLocalTime().Day)
            .ToDictionary(g => g.Key, g => g.ToList());

        int firstCol    = ((int)_month.DayOfWeek + 6) % 7; // Mon=0 ... Sun=6
        int daysInMonth = DateTime.DaysInMonth(_month.Year, _month.Month);
        int day = 1;

        for (int r = 1; r <= 6; r++)
        for (int c = 0; c < 7; c++)
        {
            var  btn       = (Button)_grid.GetControlFromPosition(c, r)!;
            int  cellIndex = (r - 1) * 7 + c;
            bool active    = cellIndex >= firstCol && day <= daysInMonth;

            if (!active)
            {
                btn.Text = string.Empty; btn.BackColor = ColOutside;
                btn.Tag  = null; btn.Enabled = false;
                btn.FlatAppearance.BorderSize = 0;
            }
            else
            {
                int d = day;
                btn.Tag = d; btn.Enabled = true;

                if (byDay.TryGetValue(d, out var dayWalks) && dayWalks.Count > 0)
                {
                    btn.Text      = $"{d}\n({dayWalks.Count})";
                    btn.BackColor = PickColour(dayWalks);
                    btn.ForeColor = Color.Black;
                }
                else
                {
                    btn.Text = d.ToString(); btn.BackColor = ColEmpty; btn.ForeColor = Color.DimGray;
                }

                bool isToday    = _month.Year  == DateTime.Today.Year  &&
                                  _month.Month == DateTime.Today.Month &&
                                  d == DateTime.Today.Day;
                bool isSelected = d == _selectedDay;
                btn.Font = new Font("Segoe UI", 8, isToday ? FontStyle.Bold : FontStyle.Regular);
                btn.FlatAppearance.BorderSize = isSelected ? 2 : 0;
                if (isSelected)
                    btn.FlatAppearance.BorderColor = Color.FromArgb(30, 70, 150);

                day++;
            }
        }
    }

    private static Color PickColour(List<WalkEventDto> walks)
    {
        var statuses = walks.Select(w => w.Status).Distinct().ToList();
        if (statuses.Count > 1) return ColMixed;
        return statuses[0] switch
        {
            WalkStatus.Requested or WalkStatus.Proposed => ColRequested,
            WalkStatus.Accepted                         => ColAccepted,
            WalkStatus.InProgress                       => ColInProgress,
            WalkStatus.Completed                        => ColAccepted,
            _                                           => ColEmpty
        };
    }

    private void OnDayClicked(object? sender, EventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not int day) return;
        _selectedDay = day;
        RebuildGrid();
        ShowDayWalks(day);
    }

    private void ShowDayWalks(int day)
    {
        // Apply column visibility based on panel configuration
        if (_dgv.Columns.Contains("Client"))
            _dgv.Columns["Client"]!.Visible        = !HideClientColumn;
        if (_dgv.Columns.Contains("Walker"))
            _dgv.Columns["Walker"]!.Visible        = !HideWalkerColumn;
        if (_dgv.Columns.Contains("ClientAddress"))
            _dgv.Columns["ClientAddress"]!.Visible = ShowClientAddressColumn;
        if (_dgv.Columns.Contains("Respond"))
            _dgv.Columns["Respond"]!.Visible       = ShowRespondButton;
        if (_dgv.Columns.Contains("WalkerInfo"))
            _dgv.Columns["WalkerInfo"]!.Visible    = ShowWalkerInfoButton;

        if (day == 0) { _dayWalks = new(); _dgv.DataSource = null; return; }

        _dayWalks = _walks
            .Where(w => w.WalkDate.ToLocalTime().Year  == _month.Year  &&
                        w.WalkDate.ToLocalTime().Month == _month.Month &&
                        w.WalkDate.ToLocalTime().Day   == day)
            .OrderBy(w => w.WalkDate)
            .ToList();

        _dgv.DataSource = _dayWalks
            .Select(w => new WalkCalendarRow(
                w.Id, w.DogName, w.ClientName, w.WalkerName ?? "\u2014",
                w.ClientAddress,
                w.WalkDate.ToLocalTime().ToString("HH:mm"),
                $"{w.DurationMinutes} min",
                w.WalkerId.HasValue ? $"\u2248{Random.Shared.Next(5, 21)} min" : "\u2014",
                w.Status.ToString(),
                w.Location,
                w.Notes,
                w.WalkerId))
            .ToList();

        // Colour rows and set action-button labels
        for (int i = 0; i < _dgv.Rows.Count && i < _dayWalks.Count; i++)
        {
            var walk = _dayWalks[i];
            var row  = _dgv.Rows[i];

            row.DefaultCellStyle.BackColor = walk.Status switch
            {
                WalkStatus.Accepted                         => RowAccepted,
                WalkStatus.InProgress                       => RowInProgress,
                WalkStatus.Completed                        => RowCompleted,
                WalkStatus.Cancelled                        => RowCancelled,
                WalkStatus.Requested or WalkStatus.Proposed => RowProposal,
                _                                           => Color.White
            };

            if (ShowRespondButton)
            {
                bool canRespond = walk.Status is WalkStatus.Requested or WalkStatus.Proposed;
                row.Cells["Respond"].Value            = canRespond ? "Respond" : string.Empty;
                row.Cells["Respond"].Style.ForeColor  = canRespond ? Color.White : Color.Transparent;
                row.Cells["Respond"].Style.BackColor  = canRespond ? BtnBlue    : row.DefaultCellStyle.BackColor;
            }

            if (ShowWalkerInfoButton)
            {
                bool hasWalker = walk.WalkerId.HasValue &&
                                 walk.Status is WalkStatus.Accepted or WalkStatus.InProgress or WalkStatus.Completed;
                row.Cells["WalkerInfo"].Value           = hasWalker ? "Walker Info" : string.Empty;
                row.Cells["WalkerInfo"].Style.ForeColor = hasWalker ? Color.White : Color.Transparent;
                row.Cells["WalkerInfo"].Style.BackColor = hasWalker ? BtnBlue     : row.DefaultCellStyle.BackColor;
            }
        }
    }

    private void OnDetailCellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.RowIndex >= _dayWalks.Count) return;
        var walk    = _dayWalks[e.RowIndex];
        var colName = e.ColumnIndex >= 0 ? _dgv.Columns[e.ColumnIndex]?.Name : null;

        switch (colName)
        {
            case "Respond" when walk.Status is WalkStatus.Requested or WalkStatus.Proposed:
                RespondButtonClicked?.Invoke(walk);
                break;

            case "WalkerInfo" when walk.WalkerId.HasValue:
                WalkerInfoButtonClicked?.Invoke(walk);
                break;
        }
    }

    // ── Row record bound to the detail DataGridView ───────────────────────────
    public record WalkCalendarRow(
        int     Id,
        string  Dog,
        string  Client,
        string  Walker,
        string  ClientAddress,
        string  Time,
        string  Duration,
        string  ETA,
        string  Status,
        string  Location,
        string? Notes,
        int?    WalkerId);
}
