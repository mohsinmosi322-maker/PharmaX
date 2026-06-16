using System;
using System.Windows.Forms;
using EnterpriseInventory.Core.Domain;
using EnterpriseInventory.Infrastructure.Repositories;
using EnterpriseInventory.UI.Helpers;

namespace EnterpriseInventory.UI.Forms.MasterData
{
    public class UnitForm : Form
    {
        private readonly IUnitRepository _unitRepository;
        private DataGridView dgvUnits;
        private TextBox txtSearch;
        private Button btnAdd, btnEdit, btnDelete, btnRefresh;

        public UnitForm()
        {
            _unitRepository = new UnitRepository(DbConnectionFactory.CreateConnection());
            InitializeComponent();
            LoadUnits();
        }

        private void InitializeComponent()
        {
            this.Text = "Unit of Measurement Management";
            this.Size = new System.Drawing.Size(800, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);

            var pnlSearch = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = System.Drawing.Color.FromArgb(240, 240, 240) };
            var lblSearch = new Label { Text = "Search:", Location = new System.Drawing.Point(20, 20), AutoSize = true };
            txtSearch = new TextBox { Location = new System.Drawing.Point(80, 17), Size = new System.Drawing.Size(300, 23) };
            txtSearch.TextChanged += (s, e) => LoadUnits();

            btnAdd = new Button { Text = "Add New", Location = new System.Drawing.Point(500, 15), Size = new System.Drawing.Size(100, 30), FlatStyle = FlatStyle.Flat, BackColor = System.Drawing.Color.FromArgb(0, 120, 215), ForeColor = System.Drawing.Color.White };
            btnAdd.Click += (s, e) => { using (var form = new UnitDetailForm()) { if (form.ShowDialog() == DialogResult.OK) LoadUnits(); } };

            btnRefresh = new Button { Text = "Refresh", Location = new System.Drawing.Point(670, 15), Size = new System.Drawing.Size(100, 30), FlatStyle = FlatStyle.Flat };
            btnRefresh.Click += (s, e) => LoadUnits();

            pnlSearch.Controls.Add(lblSearch);
            pnlSearch.Controls.Add(txtSearch);
            pnlSearch.Controls.Add(btnAdd);
            pnlSearch.Controls.Add(btnRefresh);

            dgvUnits = CreateGrid();
            dgvUnits.DoubleClick += (s, e) => EditUnit();

            this.Controls.Add(dgvUnits);
            this.Controls.Add(pnlSearch);
        }

        private DataGridView CreateGrid()
        {
            var dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = System.Drawing.Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false
            };
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "UnitID", HeaderText = "ID", DataPropertyName = "UnitID", Width = 60 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "UnitName", HeaderText = "Unit Name", DataPropertyName = "UnitName", Width = 300 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Symbol", HeaderText = "Symbol", DataPropertyName = "Symbol", Width = 150 });
            return dgv;
        }

        private void LoadUnits()
        {
            try
            {
                var units = _unitRepository.GetAll();
                if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                    units = units.FindAll(u => u.UnitName.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase) || (u.Symbol != null && u.Symbol.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase)));
                dgvUnits.DataSource = null;
                dgvUnits.DataSource = units;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EditUnit()
        {
            if (dgvUnits.CurrentRow == null) return;
            var unitId = Convert.ToInt32(dgvUnits.CurrentRow.Cells["UnitID"].Value);
            var unit = _unitRepository.GetById(unitId);
            if (unit != null)
            {
                using (var form = new UnitDetailForm(unit))
                {
                    if (form.ShowDialog() == DialogResult.OK) LoadUnits();
                }
            }
        }
    }

    public class UnitDetailForm : Form
    {
        private readonly IUnitRepository _unitRepository;
        private Unit _unit;
        private bool _isEditMode;
        private TextBox txtUnitName, txtSymbol;

        public UnitDetailForm(Unit unit = null)
        {
            _unitRepository = new UnitRepository(DbConnectionFactory.CreateConnection());
            _unit = unit ?? new Unit();
            _isEditMode = unit != null && unit.UnitID > 0;
            InitializeComponent();
            if (_isEditMode) LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = _isEditMode ? "Edit Unit" : "Add Unit";
            this.Size = new System.Drawing.Size(450, 350);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            var lblTitle = new Label { Text = _isEditMode ? "Edit Unit" : "Add New Unit", Font = new System.Drawing.Font("Arial", 14F, System.Drawing.FontStyle.Bold), Location = new System.Drawing.Point(20, 20), AutoSize = true };
            var lblUnitName = new Label { Text = "Unit Name *", Location = new System.Drawing.Point(20, 70), AutoSize = true };
            txtUnitName = new TextBox { Location = new System.Drawing.Point(20, 95), Size = new System.Drawing.Size(390, 23) };
            var lblSymbol = new Label { Text = "Symbol (e.g., pcs, kg, box)", Location = new System.Drawing.Point(20, 130), AutoSize = true };
            txtSymbol = new TextBox { Location = new System.Drawing.Point(20, 155), Size = new System.Drawing.Size(390, 23) };

            var btnSave = new Button { Text = "Save", Location = new System.Drawing.Point(20, 200), Size = new System.Drawing.Size(100, 35), FlatStyle = FlatStyle.Flat, BackColor = System.Drawing.Color.FromArgb(0, 120, 215), ForeColor = System.Drawing.Color.White };
            btnSave.Click += BtnSave_Click;
            var btnCancel = new Button { Text = "Cancel", Location = new System.Drawing.Point(130, 200), Size = new System.Drawing.Size(100, 35), DialogResult = DialogResult.Cancel };

            this.Controls.Add(lblTitle);
            this.Controls.Add(lblUnitName);
            this.Controls.Add(txtUnitName);
            this.Controls.Add(lblSymbol);
            this.Controls.Add(txtSymbol);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);
            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }

        private void LoadData()
        {
            txtUnitName.Text = _unit.UnitName;
            txtSymbol.Text = _unit.Symbol;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUnitName.Text))
            {
                MessageBox.Show("Unit name is required", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                _unit.UnitName = txtUnitName.Text.Trim();
                _unit.Symbol = txtSymbol.Text.Trim();
                if (_isEditMode) _unitRepository.Update(_unit);
                else _unitRepository.Add(_unit);
                MessageBox.Show($"Unit {_isEditMode ? "updated" : "created"} successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
