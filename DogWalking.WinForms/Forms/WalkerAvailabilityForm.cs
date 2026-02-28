using DogWalking.Application.DTOs;
using DogWalking.Application.Interfaces;
using DogWalking.Domain.Enums;

namespace DogWalking.WinForms.Forms;

/// <summary>Walker availability management form.</summary>
public partial class WalkerAvailabilityForm : Form
{
    private readonly IWalkerAvailabilityService _svc;
    private readonly int _walkerId;
    private readonly string _walkerName;

    public WalkerAvailabilityForm(IWalkerAvailabilityService svc, int walkerId, string walkerName)
    {
        _svc        = svc;
        _walkerId   = walkerId;
        _walkerName = walkerName;
        InitializeComponent();
        SetupCombos();
        Text = $"Availability \u2014 {_walkerName}";
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        await RefreshSlotsAsync();
    }

    // -- Setup ------------------------------------------------------

    private void SetupCombos()
    {
        cmbDay.Items.AddRange(Enum.GetNames<DayOfWeek>());
        cmbDay.SelectedIndex = 0;

        foreach (var z in WalkZoneExtensions.All())
            cmbZone.Items.Add(z.ToDisplayName());
        cmbZone.SelectedIndex = 0;
    }

    // -- Data loading -----------------------------------------------

    private async Task RefreshSlotsAsync()
    {
        HideError();
        var slots = await _svc.GetByWalkerIdAsync(_walkerId);
        gridSlots.Rows.Clear();
        foreach (var s in slots)
        {
            double hours = (s.EndTime - s.StartTime).TotalHours;
            gridSlots.Rows.Add(s.Id, s.DayOfWeek.ToString(),
                                s.StartTime.ToString("HH:mm"), s.EndTime.ToString("HH:mm"),
                                $"{hours:F1}h", s.Zone);
        }
    }

    // -- Actions ----------------------------------------------------

    private async Task AddSlotAsync()
    {
        HideError();
        try
        {
            var day   = (DayOfWeek)cmbDay.SelectedIndex;
            var start = TimeOnly.FromDateTime(dtpStart.Value);
            var end   = TimeOnly.FromDateTime(dtpEnd.Value);
            var zone  = cmbZone.SelectedItem?.ToString() ?? string.Empty;
            await _svc.AddAvailabilityAsync(new CreateAvailabilityDto(_walkerId, day, start, end, zone));
            await RefreshSlotsAsync();
        }
        catch (Exception ex) { ShowError(ex.Message); }
    }

    private async Task DeleteSlotAsync()
    {
        HideError();
        if (gridSlots.CurrentRow == null) return;
        if (gridSlots.CurrentRow.Cells["Id"].Value is not int id) return;
        try
        {
            await _svc.DeleteAvailabilityAsync(id);
            await RefreshSlotsAsync();
        }
        catch (Exception ex) { ShowError(ex.Message); }
    }

    // -- Helpers ----------------------------------------------------

    private void ShowError(string msg) { lblError.Text = msg; lblError.Visible = true; }
    private void HideError()           { lblError.Visible = false; }

    // -- Event Handlers ---------------------------------------------

    private async void BtnAddSlot_Click(object? sender, EventArgs e) => await AddSlotAsync();
    private async void BtnDelSlot_Click(object? sender, EventArgs e) => await DeleteSlotAsync();
    private void BtnClose_Click(object? sender, EventArgs e) => Close();
}
