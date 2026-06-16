using System;
using System.Windows.Forms;
using EnterpriseERP.Application.Services;

namespace EnterpriseERP.UI.Forms.MasterData
{
    public partial class UnitForm : BaseForm
    {
        private DataGridView dgvUnits;
        private Button btnAdd, btnEdit, btnDelete, btnRefresh;

        public UnitForm(AuthService authService, FeatureService featureService) 
            : base(authService, featureService)
        {
            InitializeComponent();
            LoadUnits();
        }

        private void InitializeComponent()
        {
            this.Text = "Unit Management";
            this.Size = new Size(800, 500);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50 };
            var lblTitle = new Label 
            { 
                Text = "Unit of Measurement", 
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Location = new Point(10, 10)
            };
            
            btnRefresh = new Button 
            { 
                Text = "Refresh", 
                Location = new Point(600, 12),
                Size = new Size(80, 30)
            };
            btnRefresh.Click += (s, e) => LoadUnits();

            btnAdd = new Button 
            { 
                Text = "Add New", 
                Location = new Point(700, 12),
                Size = new Size(80, 30)
            };
            btnAdd.Click += (s, e) => OpenUnitDialog(null);

            panelTop.Controls.AddRange(new Control[] { lblTitle, btnRefresh, btnAdd });

            dgvUnits = new DataGridView 
            { 
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                ReadOnly = true
            };
            dgvUnits.Columns.Add("UnitID", "ID");
            dgvUnits.Columns.Add("UnitName", "Unit Name");
            dgvUnits.Columns.Add("Symbol", "Symbol");

            var contextMenu = new ContextMenuStrip();
            var editItem = new ToolStripMenuItem("Edit");
            editItem.Click += (s, e) => OpenUnitDialog(GetSelectedUnit());
            var deleteItem = new ToolStripMenuItem("Delete");
            deleteItem.Click += (s, e) => DeleteUnit();
            contextMenu.Items.AddRange(new ToolStripItem[] { editItem, deleteItem });
            dgvUnits.ContextMenuStrip = contextMenu;

            this.Controls.Add(dgvUnits);
            this.Controls.Add(panelTop);
        }

        private void LoadUnits()
        {
            try
            {
                dgvUnits.Rows.Clear();
                
                // Sample data
                dgvUnits.Rows.Add(1, "Piece", "Pcs");
                dgvUnits.Rows.Add(2, "Pack", "Pkt");
                dgvUnits.Rows.Add(3, "Box", "Box");
                dgvUnits.Rows.Add(4, "Carton", "Ctn");
                dgvUnits.Rows.Add(5, "Bottle", "Btl");
                dgvUnits.Rows.Add(6, "Strip", "Str");
                dgvUnits.Rows.Add(7, "Kilogram", "Kg");
                dgvUnits.Rows.Add(8, "Gram", "Gm");
            }
            catch (Exception ex)
            {
                ShowError($"Error loading units: {ex.Message}");
            }
        }

        private dynamic GetSelectedUnit()
        {
            if (dgvUnits.CurrentRow != null)
            {
                return new 
                {
                    UnitID = Convert.ToInt32(dgvUnits.CurrentRow.Cells["UnitID"].Value),
                    UnitName = dgvUnits.CurrentRow.Cells["UnitName"].Value?.ToString(),
                    Symbol = dgvUnits.CurrentRow.Cells["Symbol"].Value?.ToString()
                };
            }
            return null;
        }

        private void OpenUnitDialog(dynamic unit)
        {
            using (var dialog = new UnitDialogForm(_authService, _featureService, unit))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    LoadUnits();
                    ShowSuccess(unit == null ? "Unit added successfully" : "Unit updated successfully");
                }
            }
        }

        private void DeleteUnit()
        {
            var unit = GetSelectedUnit();
            if (unit == null)
            {
                ShowError("Please select a unit to delete");
                return;
            }

            if (ConfirmAction($"Are you sure you want to delete unit '{unit.UnitName}'?"))
            {
                try
                {
                    LoadUnits();
                    ShowSuccess("Unit deleted successfully");
                }
                catch (Exception ex)
                {
                    ShowError($"Error deleting unit: {ex.Message}");
                }
            }
        }
    }

    public partial class UnitDialogForm : Form
    {
        private readonly AuthService _authService;
        private readonly FeatureService _featureService;
        private readonly dynamic _unit;

        private TextBox txtName, txtSymbol;
        private Button btnSave, btnCancel;

        public UnitDialogForm(AuthService authService, FeatureService featureService, dynamic unit)
        {
            _authService = authService;
            _featureService = featureService;
            _unit = unit;

            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = _unit == null ? "Add Unit" : "Edit Unit";
            this.Size = new Size(350, 200);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            var lblName = new Label { Text = "Unit Name:", Location = new Point(20, 20), Size = new Size(100, 25) };
            txtName = new TextBox { Location = new Point(120, 20), Size = new Size(180, 25) };

            var lblSymbol = new Label { Text = "Symbol:", Location = new Point(20, 60), Size = new Size(100, 25) };
            txtSymbol = new TextBox { Location = new Point(120, 60), Size = new Size(180, 25) };

            btnSave = new Button { Text = "Save", Location = new Point(80, 110), Size = new Size(80, 30) };
            btnSave.Click += (s, e) => SaveUnit();

            btnCancel = new Button { Text = "Cancel", Location = new Point(180, 110), Size = new Size(80, 30) };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.AddRange(new Control[] { lblName, txtName, lblSymbol, txtSymbol, btnSave, btnCancel });
            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }

        private void LoadData()
        {
            if (_unit != null)
            {
                txtName.Text = _unit.UnitName;
                txtSymbol.Text = _unit.Symbol;
            }
        }

        private void SaveUnit()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Unit name is required", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            this.DialogResult = DialogResult.OK;
        }
    }
}
