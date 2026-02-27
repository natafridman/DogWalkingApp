using DogWalking.Application.DTOs;
using DogWalking.Application.Interfaces;
using DogWalking.Domain.Enums;

namespace DogWalking.WinForms.Forms;

/// <summary>
/// Add / Edit dog dialog — Designer-based version.
/// All visual layout lives in DogDialog.Designer.cs.
/// This file contains only logic: loading, saving, events.
/// </summary>
public sealed partial class DogDialog : Form
{
    private readonly IDogService _svc;
    private readonly int _clientId;
    private readonly int? _dogId;

    public DogDialog(IDogService svc, int clientId, int? dogId = null)
    {
        _svc = svc;
        _clientId = clientId;
        _dogId = dogId;

        InitializeComponent();
        PopulateBreeds();

        if (_dogId.HasValue)
        {
            Text = "Edit Dog";
            _ = LoadAsync();
        }
    }

    // ── Setup ──────────────────────────────────────────────

    private void PopulateBreeds()
    {
        foreach (var breed in DogBreedExtensions.All())
            cmbBreed.Items.Add(breed.ToDisplayName());
        cmbBreed.SelectedIndex = 0;
    }

    // ── Load (edit mode) ───────────────────────────────────

    private async Task LoadAsync()
    {
        var dog = await _svc.GetByIdAsync(_dogId!.Value);
        if (dog is null) return;

        txtName.Text = dog.Name;
        dtpBirthDate.Value = dog.BirthDate.ToDateTime(TimeOnly.MinValue);

        var match = cmbBreed.Items.Cast<string>()
            .FirstOrDefault(i => string.Equals(i, dog.Breed, StringComparison.OrdinalIgnoreCase));
        cmbBreed.SelectedItem = match ?? "Other";
    }

    // ── Save ───────────────────────────────────────────────

    private async Task SaveAsync()
    {
        lblError.Visible = false;
        try
        {
            var breed = cmbBreed.SelectedItem?.ToString() ?? "Other";
            var birthDate = DateOnly.FromDateTime(dtpBirthDate.Value);

            if (_dogId.HasValue)
                await _svc.UpdateAsync(new UpdateDogDto(_dogId.Value, txtName.Text, breed, birthDate));
            else
                await _svc.CreateAsync(new CreateDogDto(_clientId, txtName.Text, breed, birthDate));

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            lblError.Text = ex.Message;
            lblError.Visible = true;
        }
    }

    // ── Event Handlers ─────────────────────────────────────

    private async void BtnSave_Click(object? sender, EventArgs e) => await SaveAsync();

    private void BtnClear_Click(object? sender, EventArgs e)
    {
        txtName.Clear();
        cmbBreed.SelectedIndex = 0;
        dtpBirthDate.Value = DateTime.Today.AddYears(-1);
        lblError.Visible = false;
    }

    private void BtnCancel_Click(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}
