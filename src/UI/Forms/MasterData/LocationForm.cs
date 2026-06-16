using System;
using System.Windows.Forms;
using EnterpriseERP.Application.Services;

namespace EnterpriseERP.UI.Forms.MasterData
{
    public partial class LocationForm : BaseForm
    {
        private DataGridView dgvLocations;
        private Button btnAdd, btnEdit, btnDelete, btnRefresh;

        public LocationForm(AuthService authService, FeatureService featureService) 
            : base(authService, featureService)
        {
            InitializeComponent();
            LoadLocations();
        }

        private void InitializeComponent()
        {
            this.Text = "Storage Location Management";
            this.Size = new Size(900, 500);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50 };
            var lblTitle = new Label 
            { 
                Text = "Storage Locations", 
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Location = new Point(10, 10)
            };
            
            btnRefresh = new Button { Text = "Refresh", Location = new Point(700, 12), Size = new Size(80, 30) };
            btnRefresh.Click += (s, e) => LoadLocations();

            btnAdd = new Button { Text = "Add New", Location = new Point(800, 12), Size = new Size(80, 30) };
            btnAdd.Click += (s, e) => OpenLocationDialog(null);

            panelTop.Controls.AddRange(new Control[] { lblTitle, btnRefresh, btnAdd });

            dgvLocations = new DataGridView 
            { 
                Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, AllowUserToAddRows = false, ReadOnly = true
            };
            dgvLocations.Columns.Add("LocationID", "ID");
            dgvLocations.Columns.Add("LocationCode", "Code");
            dgvLocations.Columns.Add("LocationName", "Name");
            dgvLocations.Columns.Add("Description", "Description");

            var contextMenu = new ContextMenuStrip();
            var editItem = new ToolStripMenuItem("Edit");
            editItem.Click += (s, e) => OpenLocationDialog(GetSelectedLocation());
            var deleteItem = new ToolStripMenuItem("Delete");
            deleteItem.Click += (s, e) => DeleteLocation();
            contextMenu.Items.AddRange(new ToolStripItem[] { editItem, deleteItem });
            dgvLocations.ContextMenuStrip = contextMenu;

            this.Controls.Add(dgvLocations);
            this.Controls.Add(panelTop);
        }

        private void LoadLocations()
        {
            try
            {
                dgvLocations.Rows.Clear();
                dgvLocations.Rows.Add(1, "LOC001", "Rack A", "Main storage rack A");
                dgvLocations.Rows.Add(2, "LOC002", "Rack B", "Main storage rack B");
                dgvLocations.Rows.Add(3, "LOC003", "Shelf 1", "First shelf unit");
                dgvLocations.Rows.Add(4, "LOC004", "Warehouse A", "Main warehouse");
            }
            catch (Exception ex) { ShowError($"Error loading locations: {ex.Message}"); }
        }

        private dynamic GetSelectedLocation()
        {
            if (dgvLocations.CurrentRow != null)
            {
                return new 
                {
                    LocationID = Convert.ToInt32(dgvLocations.CurrentRow.Cells["LocationID"].Value),
                    LocationCode = dgvLocations.CurrentRow.Cells["LocationCode"].Value?.ToString(),
                    LocationName = dgvLocations.CurrentRow.Cells["LocationName"].Value?.ToString(),
                    Description = dgvLocations.CurrentRow.Cells["Description"].Value?.ToString()
                };
            }
            return null;
        }

        private void OpenLocationDialog(dynamic location)
        {
            using (var dialog = new LocationDialogForm(_authService, _featureService, location))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    LoadLocations();
                    ShowSuccess(location == null ? "Location added successfully" : "Location updated successfully");
                }
            }
        }

        private void DeleteLocation()
        {
            var location = GetSelectedLocation();
            if (location == null) { ShowError("Please select a location to delete"); return; }
            if (ConfirmAction($"Are you sure you want to delete location '{location.LocationName}'?"))
            {
                try { LoadLocations(); ShowSuccess("Location deleted successfully"); }
                catch (Exception ex) { ShowError($"Error deleting location: {ex.Message}"); }
            }
        }
    }

    public partial class LocationDialogForm : Form
    {
        private readonly AuthService _authService;
        private readonly FeatureService _featureService;
        private readonly dynamic _location;
        private TextBox txtCode, txtName, txtDescription;
        private Button btnSave, btnCancel;

        public LocationDialogForm(AuthService authService, FeatureService featureService, dynamic location)
        {
            _authService = authService; _featureService = featureService; _location = location;
            InitializeComponent(); LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = _location == null ? "Add Location" : "Edit Location";
            this.Size = new Size(400, 250); this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false; this.MinimizeBox = false; this.StartPosition = FormStartPosition.CenterParent;

            int y = 20;
            var lblCode = new Label { Text = "Location Code:", Location = new Point(20, y), Size = new Size(100, 25) };
            txtCode = new TextBox { Location = new Point(140, y), Size = new Size(220, 25) }; y += 35;

            var lblName = new Label { Text = "Location Name:", Location = new Point(20, y), Size = new Size(100, 25) };
            txtName = new TextBox { Location = new Point(140, y), Size = new Size(220, 25) }; y += 35;

            var lblDesc = new Label { Text = "Description:", Location = new Point(20, y), Size = new Size(100, 25) };
            txtDescription = new TextBox { Location = new Point(140, y), Size = new Size(220, 25), Multiline = true, Height = 50 }; y += 60;

            btnSave = new Button { Text = "Save", Location = new Point(140, y), Size = new Size(80, 30) };
            btnSave.Click += (s, e) => SaveLocation();
            btnCancel = new Button { Text = "Cancel", Location = new Point(240, y), Size = new Size(80, 30) };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.AddRange(new Control[] { lblCode, txtCode, lblName, txtName, lblDesc, txtDescription, btnSave, btnCancel });
            this.AcceptButton = btnSave; this.CancelButton = btnCancel;
        }

        private void LoadData()
        {
            if (_location != null)
            {
                txtCode.Text = _location.LocationCode;
                txtName.Text = _location.LocationName;
                txtDescription.Text = _location.Description;
            }
        }

        private void SaveLocation()
        {
            if (string.IsNullOrWhiteSpace(txtCode.Text) || string.IsNullOrWhiteSpace(txtName.Text))
            { MessageBox.Show("Code and Name are required", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            this.DialogResult = DialogResult.OK;
        }
    }
}
