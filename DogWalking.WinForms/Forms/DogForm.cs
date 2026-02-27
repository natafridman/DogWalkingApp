using DogWalking.Application.Interfaces;

namespace DogWalking.WinForms.Forms;

public partial class DogForm : Form
{
    private readonly IDogService    _dogs;
    private readonly IClientService _clients;
    private int _clientId;

    public DogForm(IDogService dogs, IClientService clients)
    {
        _dogs    = dogs;
        _clients = clients;
        InitializeComponent();
    }

    public void SetClientId(int id) => _clientId = id;

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        await LoadAsync();
    }

    // -- Data -------------------------------------------------------

    private async Task LoadAsync()
    {
        var client = await _clients.GetByIdAsync(_clientId);
        lblClient.Text = $"Dogs for: {client?.Name ?? "Unknown"}";

        var dogs = await _dogs.GetByClientIdAsync(_clientId);
        dgvDogs.DataSource = dogs.Select(d => new
        {
            d.Id, d.Name, d.Breed,
            BirthDate = d.BirthDate.ToString("yyyy-MM-dd"),
            Age = $"{d.AgeInYears} yrs"
        }).ToList();
        if (dgvDogs.Columns.Contains("Id")) dgvDogs.Columns["Id"].Visible = false;
    }

    private async Task ShowDogDialog(int? dogId = null)
    {
        using var dlg = new DogDialog(_dogs, _clientId, dogId);
        if (dlg.ShowDialog() == DialogResult.OK)
            await LoadAsync();
    }

    private async Task DeleteAsync()
    {
        if (SelectedId() is not int id) return;
        var dogName = dgvDogs.CurrentRow?.Cells["Name"].Value?.ToString() ?? "this dog";
        if (MessageBox.Show(
                $"Delete {dogName}?\n\nThis will also permanently delete all associated walk history.",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        try { await _dogs.DeleteAsync(id); await LoadAsync(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Cannot Delete", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }

    private int? SelectedId() =>
        dgvDogs.CurrentRow?.Cells["Id"].Value is int id ? id : null;

    // -- Event Handlers ---------------------------------------------

    private async void BtnAddDog_Click(object? sender, EventArgs e) => await ShowDogDialog();
    private async void TsmiEdit_Click(object? sender, EventArgs e) => await ShowDogDialog(SelectedId());
    private async void TsmiDelete_Click(object? sender, EventArgs e) => await DeleteAsync();
}
