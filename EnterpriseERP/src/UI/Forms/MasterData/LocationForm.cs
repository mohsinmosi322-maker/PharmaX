using System;
using System.Windows.Forms;
using EnterpriseInventory.Core.Domain;
using EnterpriseInventory.Infrastructure.Repositories;
using EnterpriseInventory.UI.Helpers;

namespace EnterpriseInventory.UI.Forms.MasterData
{
    public class LocationForm : Form
    {
        private readonly ILocationRepository _locationRepository;
        private DataGridView dgvLocations;
        private TextBox txtSearch;

        public LocationForm()
        {
            _locationRepository = new LocationRepository(DbConnectionFactory.CreateConnection());
            InitializeComponent();
            LoadLocations();
        }

        private void InitializeComponent()
        {
            this.Text = "Storage Location Management";
            this.Size = new System.Drawing.Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = System.Drawing.Color.FromArgb(240, 240, 240) };
            var lblSearch = new Label { Text = "Search:", Location = new System.Drawing.Point(20, 25), AutoSize = true };
            txtSearch = new TextBox { Location = new System.Drawing.Point(80, 22), Size = new System.Drawing.Size(300, 23) };
            txtSearch.TextChanged += (s, e) => LoadLocations();

            var btnAdd = new Button { Text = "Add New", Location = new System.Drawing.Point(500, 20), Size = new System.Drawing.Size(100, 35), FlatStyle = FlatStyle.Flat, BackColor = System.Drawing.Color.FromArgb(0, 120, 215), ForeColor = System.Drawing.Color.White };
            btnAdd.Click += (s, e) => { using (var form = new LocationDetailForm()) { if (form.ShowDialog() == DialogResult.OK) LoadLocations(); } };

            var btnRefresh = new Button { Text = "Refresh", Location = new System.Drawing.Point(670, 20), Size = new System.Drawing.Size(100, 35), FlatStyle = FlatStyle.Flat };
            btnRefresh.Click += (s, e) => LoadLocations();

            pnlTop.Controls.Add(lblSearch);
            pnlTop.Controls.Add(txtSearch);
            pnlTop.Controls.Add(btnAdd);
            pnlTop.Controls.Add(btnRefresh);

            dgvLocations = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = false, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, BackgroundColor = System.Drawing.Color.White, BorderStyle = BorderStyle.None, RowHeadersVisible = false };
            dgvLocations.Columns.Add(new DataGridViewTextBoxColumn { Name = "LocationID", HeaderText = "ID", DataPropertyName = "LocationID", Width = 60 });
            dgvLocations.Columns.Add(new DataGridViewTextBoxColumn { Name = "LocationCode", HeaderText = "Code", DataPropertyName = "LocationCode", Width = 100 });
            dgvLocations.Columns.Add(new DataGridViewTextBoxColumn { Name = "LocationName", HeaderText = "Location Name", DataPropertyName = "LocationName", Width = 300 });
            dgvLocations.Columns.Add(new DataGridViewTextBoxColumn { Name = "Description", HeaderText = "Description", DataPropertyName = "Description", Width = 400 });
            dgvLocations.DoubleClick += (s, e) => EditLocation();

            this.Controls.Add(dgvLocations);
            this.Controls.Add(pnlTop);
        }

        private void LoadLocations()
        {
            try
            {
                var list = _locationRepository.GetAll();
                if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                    list = list.FindAll(l => l.LocationName.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase) || (l.LocationCode != null && l.LocationCode.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase)));
                dgvLocations.DataSource = null;
                dgvLocations.DataSource = list;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EditLocation()
        {
            if (dgvLocations.CurrentRow == null) return;
            var id = Convert.ToInt32(dgvLocations.CurrentRow.Cells["LocationID"].Value);
            var location = _locationRepository.GetById(id);
            if (location != null)
            {
                using (var form = new LocationDetailForm(location))
                {
                    if (form.ShowDialog() == DialogResult.OK) LoadLocations();
                }
            }
        }
    }

    public class LocationDetailForm : Form
    {
        private readonly ILocationRepository _locationRepository;
        private Location _location;
        private bool _isEditMode;
        private TextBox txtCode, txtName, txtDescription;

        public LocationDetailForm(Location location = null)
        {
            _locationRepository = new LocationRepository(DbConnectionFactory.CreateConnection());
            _location = location ?? new Location();
            _isEditMode = location != null && location.LocationID > 0;
            InitializeComponent();
            if (_isEditMode) LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = _isEditMode ? "Edit Location" : "Add Location";
            this.Size = new System.Drawing.Size(500, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            var lblTitle = new Label { Text = _isEditMode ? "Edit Location" : "Add New Location", Font = new System.Drawing.Font("Arial", 14F, System.Drawing.FontStyle.Bold), Location = new System.Drawing.Point(20, 20), AutoSize = true };
            var lblCode = new Label { Text = "Location Code *", Location = new System.Drawing.Point(20, 70), AutoSize = true };
            txtCode = new TextBox { Location = new System.Drawing.Point(20, 95), Size = new System.Drawing.Size(440, 23) };
            var lblName = new Label { Text = "Location Name *", Location = new System.Drawing.Point(20, 130), AutoSize = true };
            txtName = new TextBox { Location = new System.Drawing.Point(20, 155), Size = new System.Drawing.Size(440, 23) };
            var lblDesc = new Label { Text = "Description", Location = new System.Drawing.Point(20, 190), AutoSize = true };
            txtDescription = new TextBox { Location = new System.Drawing.Point(20, 215), Size = new System.Drawing.Size(440, 60), Multiline = true, ScrollBars = ScrollBars.Vertical };

            var btnSave = new Button { Text = "Save", Location = new System.Drawing.Point(20, 300), Size = new System.Drawing.Size(100, 35), FlatStyle = FlatStyle.Flat, BackColor = System.Drawing.Color.FromArgb(0, 120, 215), ForeColor = System.Drawing.Color.White };
            btnSave.Click += BtnSave_Click;
            var btnCancel = new Button { Text = "Cancel", Location = new System.Drawing.Point(130, 300), Size = new System.Drawing.Size(100, 35), DialogResult = DialogResult.Cancel };

            this.Controls.Add(lblTitle);
            this.Controls.Add(lblCode);
            this.Controls.Add(txtCode);
            this.Controls.Add(lblName);
            this.Controls.Add(txtName);
            this.Controls.Add(lblDesc);
            this.Controls.Add(txtDescription);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);
            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }

        private void LoadData()
        {
            txtCode.Text = _location.LocationCode;
            txtName.Text = _location.LocationName;
            txtDescription.Text = _location.Description;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCode.Text) || string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please fill all required fields", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                _location.LocationCode = txtCode.Text.Trim();
                _location.LocationName = txtName.Text.Trim();
                _location.Description = txtDescription.Text.Trim();
                if (_isEditMode) _locationRepository.Update(_location);
                else _locationRepository.Add(_location);
                MessageBox.Show($"Location {_isEditMode ? "updated" : "created"} successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
