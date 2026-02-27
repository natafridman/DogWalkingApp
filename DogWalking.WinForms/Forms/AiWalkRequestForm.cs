using System.Text;
using DogWalking.Application.DTOs;
using DogWalking.Application.Interfaces;
using DogWalking.Domain.Enums;

namespace DogWalking.WinForms.Forms;

/// <summary>
/// Natural language walk request form powered by AI (OpenAI ChatGPT).
/// The user types a free-text description, AI parses it, and a confirmation
/// panel appears before any dogs or walks are actually created.
/// API key is read from appsettings.json — no runtime prompt.
/// </summary>
public partial class AiWalkRequestForm : Form
{
    private readonly IAiWalkParserService _aiParser;
    private readonly IWalkEventService _walkSvc;
    private readonly IDogService _dogSvc;
    private readonly int _clientId;
    private readonly IReadOnlyList<DogDto> _existingDogs;
    private readonly string _defaultZone;

    private AiParsedWalkRequest? _lastParsed;

    public AiWalkRequestForm(
        IAiWalkParserService aiParser,
        IWalkEventService walkSvc,
        IDogService dogSvc,
        int clientId,
        IReadOnlyList<DogDto> existingDogs,
        string defaultZone)
    {
        _aiParser = aiParser;
        _walkSvc = walkSvc;
        _dogSvc = dogSvc;
        _clientId = clientId;
        _existingDogs = existingDogs;
        _defaultZone = defaultZone;

        InitializeComponent();
    }

    // ── Parse ───────────────────────────────────────────────────

    private async void BtnParse_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtInput.Text))
        {
            MessageBox.Show("Please describe your walk request first.", "Empty Input",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        btnParse.Enabled = false;
        pnlResult.Visible = false;
        lblStatus.Text = "Parsing with AI...";
        lblStatus.ForeColor = Color.DimGray;

        try
        {
            var dogNames = _existingDogs.Select(d => d.Name);
            _lastParsed = await _aiParser.ParseAsync(txtInput.Text, dogNames, _defaultZone);

            if (_lastParsed.Walks.Count == 0)
            {
                lblStatus.Text = "AI could not identify any walk requests. Try rephrasing.";
                lblStatus.ForeColor = Color.DarkOrange;
                return;
            }

            txtResult.Text = FormatParsedResult(_lastParsed);
            pnlResult.Visible = true;
            lblStatus.Text = $"Parsed {_lastParsed.Walks.Count} walk(s) successfully.";
            lblStatus.ForeColor = Color.SeaGreen;
        }
        catch (Exception ex)
        {
            lblStatus.Text = "";
            MessageBox.Show($"AI parsing failed:\n{ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnParse.Enabled = true;
        }
    }

    // ── Confirm & Create ────────────────────────────────────────

    private async void BtnConfirm_Click(object? sender, EventArgs e)
    {
        if (_lastParsed is null) return;

        btnConfirm.Enabled = false;
        lblStatus.Text = "Creating dogs and walks...";
        lblStatus.ForeColor = Color.DimGray;

        try
        {
            // Step 1: Create new dogs
            var dogNameToId = _existingDogs.ToDictionary(d => d.Name, d => d.Id,
                StringComparer.OrdinalIgnoreCase);

            foreach (var newDog in _lastParsed.NewDogs)
            {
                if (dogNameToId.ContainsKey(newDog.Name)) continue;

                var created = await _dogSvc.CreateAsync(new CreateDogDto(
                    _clientId, newDog.Name, newDog.Breed, DateOnly.FromDateTime(DateTime.Today.AddYears(-2))));
                dogNameToId[created.Name] = created.Id;
            }

            // Step 2: Create walk events
            int createdCount = 0;
            var errors = new List<string>();

            foreach (var walk in _lastParsed.Walks)
            {
                if (!dogNameToId.TryGetValue(walk.DogName, out var dogId))
                {
                    errors.Add($"Dog '{walk.DogName}' not found.");
                    continue;
                }

                var walkDate = ComputeNextDate(walk.DayOfWeek, walk.Time);

                try
                {
                    await _walkSvc.ScheduleAsync(new CreateWalkEventDto(
                        dogId, walkDate.ToUniversalTime(), walk.DurationMinutes,
                        walk.Location, null, walk.Recurrence));
                    createdCount++;
                }
                catch (Exception ex)
                {
                    errors.Add($"{walk.DogName} @ {walk.Time}: {ex.Message}");
                }
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Successfully created {createdCount} walk request(s).");
            if (_lastParsed.NewDogs.Count > 0)
                sb.AppendLine($"Created {_lastParsed.NewDogs.Count} new dog(s).");
            if (errors.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Some walks could not be created:");
                foreach (var err in errors)
                    sb.AppendLine($"  - {err}");
            }

            MessageBox.Show(sb.ToString(), "AI Walk Request Complete",
                MessageBoxButtons.OK, errors.Count > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

            if (createdCount > 0)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error creating walks:\n{ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnConfirm.Enabled = true;
            lblStatus.Text = "";
        }
    }

    private void BtnCancel_Click(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    // ── Helpers ──────────────────────────────────────────────────

    private static DateTime ComputeNextDate(DayOfWeek? dayOfWeek, TimeOnly time)
    {
        var today = DateTime.Today;

        if (dayOfWeek is null)
            return today.AddDays(1) + time.ToTimeSpan();

        var target = dayOfWeek.Value;
        var daysUntil = ((int)target - (int)today.DayOfWeek + 7) % 7;
        if (daysUntil == 0) daysUntil = 7;

        return today.AddDays(daysUntil) + time.ToTimeSpan();
    }

    private static string FormatParsedResult(AiParsedWalkRequest parsed)
    {
        var sb = new StringBuilder();

        if (parsed.NewDogs.Count > 0)
        {
            sb.AppendLine("=== NEW DOGS TO CREATE ===");
            foreach (var dog in parsed.NewDogs)
                sb.AppendLine($"  Name: {dog.Name}  |  Breed: {dog.Breed}");
            sb.AppendLine();
        }

        sb.AppendLine("=== WALK SCHEDULE ===");
        int i = 1;
        foreach (var walk in parsed.Walks)
        {
            var dayStr = walk.DayOfWeek?.ToString() ?? "Next available day";
            var recStr = walk.Recurrence switch
            {
                RecurrenceType.WeeklySameDay => "Weekly (same day)",
                RecurrenceType.AllWorkingDays => "All working days (Mon-Fri)",
                RecurrenceType.EveryTwoWorkingDays => "Every other working day",
                _ => "One-time"
            };

            sb.AppendLine($"  {i}. {walk.DogName}");
            sb.AppendLine($"     Day: {dayStr}  |  Time: {walk.Time:HH:mm}");
            sb.AppendLine($"     Duration: {walk.DurationMinutes} min  |  Zone: {walk.Location}");
            sb.AppendLine($"     Recurrence: {recStr}");
            i++;
        }

        return sb.ToString();
    }
}
