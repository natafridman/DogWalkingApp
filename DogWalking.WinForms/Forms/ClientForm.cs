using DogWalking.Application.DTOs;
using DogWalking.Application.Interfaces;
using DogWalking.Domain.Enums;
using DogWalking.Domain.Services;

namespace DogWalking.WinForms.Forms;

/// <summary>
/// Create / edit a client.
/// Address is set at creation and shown read-only when editing.
/// Zone is chosen from the WalkZone enum (predefined neighbourhoods).
/// </summary>
public partial class ClientForm : Form
{
    private readonly IClientService _svc;
    private int? _clientId;

    private TextBox    _txtName    = null!;
    private TextBox    _txtPhone   = null!;
    private TextBox    _txtEmail   = null!;
    private TextBox    _txtAddress = null!;
    private ComboBox   _cmbZone    = null!;
    private ComboBox   _cmbSub     = null!;
    private Label      _lblSubInfo = null!;
    private Label      _lblErr     = null!;
    private Button     _btnSave    = null!;

    public ClientForm(IClientService svc) { _svc = svc; InitializeComponent(); BuildUI(); }

    public void SetClientId(int id) => _clientId = id;

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (_clientId.HasValue) await LoadAsync();
    }

    private void BuildUI()
    {
        Text = "Client";
        Size = new Size(440, 530);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        BackColor = Color.WhiteSmoke;

        Field("Full Name *",     20,  out _txtName);
        Field("Phone Number *",  80,  out _txtPhone);
        Field("Email *",         140, out _txtEmail);
        Field("Address *",       200, out _txtAddress);

        // Zone combo
        Controls.Add(new Label { Text = "Zone *", Location = new Point(20, 260), AutoSize = true });
        _cmbZone = new ComboBox
        {
            Location = new Point(20, 278), Width = 380, DropDownStyle = ComboBoxStyle.DropDownList
        };
        foreach (var z in WalkZoneExtensions.All())
            _cmbZone.Items.Add(z.ToDisplayName());
        _cmbZone.SelectedIndex = 0;
        Controls.Add(_cmbZone);

        // Subscription combo
        var lblSub = new Label { Text = "Subscription Plan *", Location = new Point(20, 314), AutoSize = true };
        _cmbSub = new ComboBox
        {
            Location = new Point(20, 332), Width = 380, DropDownStyle = ComboBoxStyle.DropDownList
        };
        foreach (var strategy in WalkLimitStrategyFactory.GetAll())
            _cmbSub.Items.Add(strategy.Description);
        _cmbSub.SelectedIndex = 0;
        _cmbSub.SelectedIndexChanged += (_, _) => UpdateSubInfo();

        _lblSubInfo = new Label
        {
            Location = new Point(20, 360), Size = new Size(380, 30),
            ForeColor = Color.DimGray, Font = new Font("Segoe UI", 8)
        };
        UpdateSubInfo();

        _lblErr = new Label
        {
            ForeColor = Color.Crimson, Location = new Point(20, 396),
            Size = new Size(380, 40), Visible = false
        };

        _btnSave = BtnPrimary("Save", 444, async () => await SaveAsync());
        var btnCancel = BtnSecondary("Cancel", 215, 444, () => { DialogResult = DialogResult.Cancel; Close(); });

        Controls.AddRange(new Control[]
            { lblSub, _cmbSub, _lblSubInfo, _lblErr, _btnSave, btnCancel });

        AcceptButton = _btnSave;
    }

    private void UpdateSubInfo()
    {
        var strategies = WalkLimitStrategyFactory.GetAll().ToList();
        if (_cmbSub.SelectedIndex >= 0 && _cmbSub.SelectedIndex < strategies.Count)
        {
            var s = strategies[_cmbSub.SelectedIndex];
            _lblSubInfo.Text =
                $"Up to {s.MaxWalksPerMonth} walks/month  \u2022  " +
                (s.AllowsMultiplePerDay ? "Unlimited walks per day" : "Max 1 walk per day");
        }
    }

    private async Task LoadAsync()
    {
        var c = await _svc.GetByIdAsync(_clientId!.Value);
        if (c is null) return;
        _txtName.Text  = c.Name;
        _txtPhone.Text = c.PhoneNumber;
        _txtEmail.Text = c.Email;

        // Address is read-only after creation
        _txtAddress.Text     = c.Address;
        _txtAddress.ReadOnly = true;
        _txtAddress.BackColor = Color.FromArgb(235, 235, 235);

        // Pre-select zone
        for (int i = 0; i < _cmbZone.Items.Count; i++)
        {
            if (string.Equals(_cmbZone.Items[i]?.ToString(), c.Zone, StringComparison.OrdinalIgnoreCase))
            { _cmbZone.SelectedIndex = i; break; }
        }

        _cmbSub.SelectedIndex = (int)c.Subscription - 1;
        Text = "Edit Client";
    }

    private async Task SaveAsync()
    {
        _lblErr.Visible = false;
        _btnSave.Enabled = false;

        try
        {
            var sub     = (SubscriptionType)(_cmbSub.SelectedIndex + 1);
            var zone    = _cmbZone.SelectedItem?.ToString() ?? string.Empty;
            var address = _txtAddress.Text.Trim();

            if (_clientId.HasValue)
            {
                await _svc.UpdateAsync(new UpdateClientDto(
                    _clientId.Value, _txtName.Text, _txtPhone.Text, _txtEmail.Text, sub, zone));
            }
            else
            {
                if (string.IsNullOrWhiteSpace(address))
                    throw new InvalidOperationException("Address is required.");
                await _svc.CreateAsync(new CreateClientDto(
                    _txtName.Text, _txtPhone.Text, _txtEmail.Text, sub, zone, address));
            }

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            _lblErr.Text    = ex.Message;
            _lblErr.Visible = true;
        }
        finally { _btnSave.Enabled = true; }
    }

    private void Field(string label, int y, out TextBox tb)
    {
        Controls.Add(new Label { Text = label, Location = new Point(20, y), AutoSize = true });
        tb = new TextBox { Location = new Point(20, y + 20), Width = 380 };
        Controls.Add(tb);
    }

    private Button BtnPrimary(string text, int y, Func<Task> fn)
    {
        var b = new Button
        {
            Text = text, Location = new Point(20, y), Width = 170, Height = 32,
            BackColor = Color.FromArgb(30, 70, 150), ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10)
        };
        b.Click += async (_, _) => await fn();
        Controls.Add(b);
        return b;
    }

    private static Button BtnSecondary(string text, int x, int y, Action fn)
    {
        var b = new Button
        {
            Text = text, Location = new Point(x, y), Width = 170, Height = 32, FlatStyle = FlatStyle.Flat
        };
        b.Click += (_, _) => fn();
        return b;
    }

    private void InitializeComponent()
    {
        SuspendLayout();
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(440, 490);
        ResumeLayout(false);
    }
}
