using DogWalking.Application.DTOs;
using DogWalking.Application.Interfaces;
using DogWalking.Domain.Enums;
using DogWalking.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DogWalking.WinForms.Forms;

/// <summary>
/// Form to request a new walk event.
/// Cascading dropdowns: Client -> Dogs (multi-select CheckedListBox).
/// Location is chosen from the WalkZone enum (predefined neighbourhoods).
/// Shows active subscription and its limits before requesting.
/// Creates one WalkEvent per selected dog.
/// Walker assignment happens later via the propose/accept workflow.
/// </summary>
public partial class WalkEventForm : Form
{
    private readonly IWalkEventService _walkSvc;
    private readonly IClientService    _clientSvc;
    private readonly IDogService       _dogSvc;

    private IReadOnlyList<ClientDto> _clientList = [];
    private List<DogDto>             _dogList    = [];
    private int? _lockedClientId;
    private DateTime? _preSelectedDate;

    private readonly IServiceScope _scope;

    public WalkEventForm(IServiceScopeFactory scopeFactory)
    {
        _scope     = scopeFactory.CreateScope();
        var sp     = _scope.ServiceProvider;
        _walkSvc   = sp.GetRequiredService<IWalkEventService>();
        _clientSvc = sp.GetRequiredService<IClientService>();
        _dogSvc    = sp.GetRequiredService<IDogService>();
        InitializeComponent();
        PopulateCombos();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _scope.Dispose();
        base.Dispose(disposing);
    }

    /// <summary>
    /// Pre-selects this client and disables the Client combo -- used by the Client role.
    /// Must be called before the form is shown.
    /// </summary>
    public void LockToClient(int clientId) => _lockedClientId = clientId;

    /// <summary>
    /// Pre-selects the walk date -- pass the calendar's selected day so it matches the user's selection.
    /// Must be called before the form is shown.
    /// </summary>
    public void PreSelectDate(DateTime date) => _preSelectedDate = date;

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        ApplyLockMode();
        await LoadClientsAsync();
    }

    // ── Setup ──────────────────────────────────────────────

    private void PopulateCombos()
    {
        foreach (var zone in WalkZoneExtensions.All())
            cmbLocation.Items.Add(zone.ToDisplayName());
        cmbLocation.SelectedIndex = 0;

        cmbRecurrence.Items.AddRange(new object[]
        {
            "One-time walk",
            "All working days (Mon\u2013Fri, rest of month)",
            "Every two working days (rest of month)",
            "Same weekday every week (rest of month)"
        });
        cmbRecurrence.SelectedIndex = 0;
    }

    private void ApplyLockMode()
    {
        if (_lockedClientId.HasValue)
        {
            lblClient.Visible = false;
            cmbClient.Visible = false;

            // Shift all controls up by 56px to reclaim the space
            const int shift = 56;
            foreach (Control c in Controls)
            {
                if (c == lblClient || c == cmbClient) continue;
                c.Top -= shift;
            }
            Height -= shift;
        }
    }

    // ── Data ───────────────────────────────────────────────

    private async Task LoadClientsAsync()
    {
        _clientList = (await _clientSvc.GetAllActiveAsync()).ToList();

        // Unwire event for the entire initial setup to prevent async-void
        // concurrency on the shared DbContext scope.
        cmbClient.SelectedIndexChanged -= CmbClient_SelectedIndexChanged;
        try
        {
            cmbClient.DataSource    = _clientList;
            cmbClient.DisplayMember = "Name";
            cmbClient.ValueMember   = "Id";

            if (_lockedClientId.HasValue)
            {
                cmbClient.SelectedValue = _lockedClientId.Value;
                cmbClient.Enabled       = false;

                var client = _clientList.FirstOrDefault(c => c.Id == _lockedClientId.Value);
                PreSelectZone(client?.Zone);
            }

            // Apply pre-selected date (set via PreSelectDate() after constructor)
            if (_preSelectedDate.HasValue)
                dtpWalkDate.Value = _preSelectedDate.Value.Date.AddHours(9);

            await OnClientChangedAsync();
        }
        finally
        {
            cmbClient.SelectedIndexChanged += CmbClient_SelectedIndexChanged;
        }
    }

    private async Task OnClientChangedAsync()
    {
        int clientId;
        if (_lockedClientId.HasValue)
            clientId = _lockedClientId.Value;
        else if (cmbClient.SelectedValue is int id)
            clientId = id;
        else
            return;

        _dogList = (await _dogSvc.GetByClientIdAsync(clientId)).ToList();
        clbDogs.Items.Clear();
        foreach (var d in _dogList)
            clbDogs.Items.Add(d.Name);

        var client = _clientList.FirstOrDefault(c => c.Id == clientId);
        if (client != null && !string.IsNullOrWhiteSpace(client.Zone)
            && cmbLocation.SelectedIndex == 0)
            PreSelectZone(client.Zone);

        UpdateSubInfo();
    }

    private void PreSelectZone(string? zoneName)
    {
        if (string.IsNullOrWhiteSpace(zoneName)) return;
        for (int i = 0; i < cmbLocation.Items.Count; i++)
        {
            if (string.Equals(cmbLocation.Items[i]?.ToString(), zoneName,
                              StringComparison.OrdinalIgnoreCase))
            {
                cmbLocation.SelectedIndex = i;
                return;
            }
        }
    }

    private void UpdateSubInfo()
    {
        ClientDto? c = _lockedClientId.HasValue
            ? _clientList.FirstOrDefault(x => x.Id == _lockedClientId.Value)
            : cmbClient.SelectedItem as ClientDto;

        if (c is null) return;
        var s = WalkLimitStrategyFactory.Create(c.Subscription);
        lblSubInfo.Text =
            $"Plan: {s.Description}  |  " +
            (s.AllowsMultiplePerDay ? "Multiple walks/day allowed" : "Max 1 walk/day");
        lblSubInfo.ForeColor = c.Subscription == SubscriptionType.Free
            ? Color.DarkOrange : Color.DarkGreen;
    }

    // ── Save ───────────────────────────────────────────────

    private async Task SaveAsync()
    {
        lblError.Visible = false;
        btnSave.Enabled  = false;

        try
        {
            var selectedDogs = _dogList
                .Where((_, i) => i < clbDogs.Items.Count && clbDogs.GetItemChecked(i))
                .ToList();

            if (selectedDogs.Count == 0)
                throw new InvalidOperationException("Please select at least one dog.");

            var recurrence = (RecurrenceType)cmbRecurrence.SelectedIndex;
            var walkDate   = dtpWalkDate.Value.ToUniversalTime();
            var duration   = (int)numDuration.Value;
            var location   = cmbLocation.SelectedItem?.ToString() ?? "";
            var notes      = string.IsNullOrWhiteSpace(txtNotes.Text) ? null : txtNotes.Text;

            foreach (var dog in selectedDogs)
            {
                await _walkSvc.ScheduleAsync(new CreateWalkEventDto(
                    dog.Id, walkDate, duration, location, notes, recurrence));
            }

            string msg = recurrence == RecurrenceType.OneTime
                ? $"Walk request submitted for {selectedDogs.Count} dog(s)! A walker will be assigned shortly."
                : $"Recurring walk requests submitted for {selectedDogs.Count} dog(s) for the rest of the month.";

            MessageBox.Show(msg, "Request Submitted", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (FluentValidation.ValidationException ex)
        {
            lblError.Text    = ex.Errors.First().ErrorMessage;
            lblError.Visible = true;
        }
        catch (Exception ex)
        {
            lblError.Text    = ex.Message;
            lblError.Visible = true;
        }
        finally { btnSave.Enabled = true; }
    }

    // ── Event Handlers ─────────────────────────────────────

    private async void CmbClient_SelectedIndexChanged(object? sender, EventArgs e) => await OnClientChangedAsync();
    private async void BtnSave_Click(object? sender, EventArgs e) => await SaveAsync();

    private void BtnClear_Click(object? sender, EventArgs e)
    {
        for (int i = 0; i < clbDogs.Items.Count; i++)
            clbDogs.SetItemChecked(i, false);
        cmbLocation.SelectedIndex = 0;
        dtpWalkDate.Value = DateTime.Now.Date.AddDays(1).AddHours(9);
        numDuration.Value = 60;
        cmbRecurrence.SelectedIndex = 0;
        txtNotes.Clear();
        lblError.Visible = false;
    }

    private void BtnCancel_Click(object? sender, EventArgs e) { DialogResult = DialogResult.Cancel; Close(); }
}
