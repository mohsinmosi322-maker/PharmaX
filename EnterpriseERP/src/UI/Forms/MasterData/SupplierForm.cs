using System;
using System.Windows.Forms;
using EnterpriseInventory.Core.Domain;
using EnterpriseInventory.Infrastructure.Repositories;
using EnterpriseInventory.UI.Helpers;

namespace EnterpriseInventory.UI.Forms.MasterData
{
    public class SupplierForm : Form
    {
        private readonly ISupplierRepository _supplierRepository;
        private DataGridView dgvSuppliers;
        private TextBox txtSearch;

        public SupplierForm()
        {
            _supplierRepository = new SupplierRepository(DbConnectionFactory.CreateConnection());
            InitializeComponent();
            LoadSuppliers();
        }

        private void InitializeComponent()
        {
            this.Text = "Supplier Management";
            this.Size = new System.Drawing.Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterScreen;

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = System.Drawing.Color.FromArgb(240, 240, 240) };
            var lblSearch = new Label { Text = "Search:", Location = new System.Drawing.Point(20, 25), AutoSize = true };
            txtSearch = new TextBox { Location = new System.Drawing.Point(80, 22), Size = new System.Drawing.Size(300, 23) };
            txtSearch.TextChanged += (s, e) => LoadSuppliers();

            var btnAdd = new Button { Text = "Add New Supplier", Location = new System.Drawing.Point(500, 20), Size = new System.Drawing.Size(130, 35), FlatStyle = FlatStyle.Flat, BackColor = System.Drawing.Color.FromArgb(0, 120, 215), ForeColor = System.Drawing.Color.White };
            btnAdd.Click += (s, e) => { using (var form = new SupplierDetailForm()) { if (form.ShowDialog() == DialogResult.OK) LoadSuppliers(); } };

            var btnRefresh = new Button { Text = "Refresh", Location = new System.Drawing.Point(700, 20), Size = new System.Drawing.Size(100, 35), FlatStyle = FlatStyle.Flat };
            btnRefresh.Click += (s, e) => LoadSuppliers();

            pnlTop.Controls.Add(lblSearch);
            pnlTop.Controls.Add(txtSearch);
            pnlTop.Controls.Add(btnAdd);
            pnlTop.Controls.Add(btnRefresh);

            dgvSuppliers = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = false, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, BackgroundColor = System.Drawing.Color.White, BorderStyle = BorderStyle.None, RowHeadersVisible = false };
            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn { Name = "SupplierID", HeaderText = "ID", DataPropertyName = "SupplierID", Width = 60 });
            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn { Name = "SupplierCode", HeaderText = "Code", DataPropertyName = "SupplierCode", Width = 100 });
            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn { Name = "SupplierName", HeaderText = "Supplier Name", DataPropertyName = "SupplierName", Width = 250 });
            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn { Name = "ContactPerson", HeaderText = "Contact Person", DataPropertyName = "ContactPerson", Width = 150 });
            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn { Name = "Mobile", HeaderText = "Mobile", DataPropertyName = "Mobile", Width = 120 });
            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn { Name = "City", HeaderText = "City", DataPropertyName = "City", Width = 120 });
            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", DataPropertyName = "Status", Width = 80 });
            dgvSuppliers.DoubleClick += (s, e) => EditSupplier();

            this.Controls.Add(dgvSuppliers);
            this.Controls.Add(pnlTop);
        }

        private void LoadSuppliers()
        {
            try
            {
                var list = _supplierRepository.GetAll();
                if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                    list = list.FindAll(s => s.SupplierName.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase) || (s.SupplierCode != null && s.SupplierCode.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase)) || (s.Mobile != null && s.Mobile.Contains(txtSearch.Text)));
                dgvSuppliers.DataSource = null;
                dgvSuppliers.DataSource = list;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EditSupplier()
        {
            if (dgvSuppliers.CurrentRow == null) return;
            var id = Convert.ToInt32(dgvSuppliers.CurrentRow.Cells["SupplierID"].Value);
            var supplier = _supplierRepository.GetById(id);
            if (supplier != null)
            {
                using (var form = new SupplierDetailForm(supplier))
                {
                    if (form.ShowDialog() == DialogResult.OK) LoadSuppliers();
                }
            }
        }
    }

    public class SupplierDetailForm : Form
    {
        private readonly ISupplierRepository _supplierRepository;
        private Supplier _supplier;
        private bool _isEditMode;
        private TextBox txtCode, txtName, txtContact, txtMobile, txtPhone, txtEmail, txtAddress, txtCity;
        private ComboBox cmbStatus;

        public SupplierDetailForm(Supplier supplier = null)
        {
            _supplierRepository = new SupplierRepository(DbConnectionFactory.CreateConnection());
            _supplier = supplier ?? new Supplier();
            _isEditMode = supplier != null && supplier.SupplierID > 0;
            InitializeComponent();
            if (_isEditMode) LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = _isEditMode ? "Edit Supplier" : "Add Supplier";
            this.Size = new System.Drawing.Size(700, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            int y = 20;
            var lblTitle = new Label { Text = _isEditMode ? "Edit Supplier" : "Add New Supplier", Font = new System.Drawing.Font("Arial", 14F, System.Drawing.FontStyle.Bold), Location = new System.Drawing.Point(20, y), AutoSize = true };
            y += 40;

            txtCode = CreateTextBox(20, y, "Supplier Code *", 300); y += 55;
            txtName = CreateTextBox(20, y, "Supplier Name *", 600); y += 55;
            txtContact = CreateTextBox(20, y, "Contact Person", 300); y += 55;
            txtMobile = CreateTextBox(20, y, "Mobile *", 300); y += 55;
            txtPhone = CreateTextBox(350, y - 55, "Phone", 300); y += 55;
            txtEmail = CreateTextBox(20, y, "Email", 300); y += 55;
            txtAddress = CreateTextBox(20, y, "Address", 600, multiline: true); y += 90;
            txtCity = CreateTextBox(20, y, "City", 300); y += 55;

            var lblStatus = new Label { Text = "Status *", Location = new System.Drawing.Point(20, y), AutoSize = true };
            cmbStatus = new ComboBox { Location = new System.Drawing.Point(20, y + 25), Size = new System.Drawing.Size(150, 21), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.Add("Active");
            cmbStatus.Items.Add("Inactive");
            cmbStatus.SelectedIndex = 0;

            var btnSave = new Button { Text = "Save", Location = new System.Drawing.Point(20, y + 70), Size = new System.Drawing.Size(100, 35), FlatStyle = FlatStyle.Flat, BackColor = System.Drawing.Color.FromArgb(0, 120, 215), ForeColor = System.Drawing.Color.White };
            btnSave.Click += BtnSave_Click;
            var btnCancel = new Button { Text = "Cancel", Location = new System.Drawing.Point(130, y + 70), Size = new System.Drawing.Size(100, 35), DialogResult = DialogResult.Cancel };

            this.Controls.Add(lblTitle);
            this.Controls.Add(lblStatus);
            this.Controls.Add(cmbStatus);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);
            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }

        private TextBox CreateTextBox(int x, int y, string labelText, int width, bool multiline = false)
        {
            var lbl = new Label { Text = labelText, Location = new System.Drawing.Point(x, y), AutoSize = true };
            var txt = new TextBox { Location = new System.Drawing.Point(x, y + 25), Size = new System.Drawing.Size(width, multiline ? 60 : 23), Multiline = multiline, ScrollBars = multiline ? ScrollBars.Vertical : ScrollBars.None };
            this.Controls.Add(lbl);
            this.Controls.Add(txt);
            return txt;
        }

        private void LoadData()
        {
            txtCode.Text = _supplier.SupplierCode;
            txtName.Text = _supplier.SupplierName;
            txtContact.Text = _supplier.ContactPerson;
            txtMobile.Text = _supplier.Mobile;
            txtPhone.Text = _supplier.Phone;
            txtEmail.Text = _supplier.Email;
            txtAddress.Text = _supplier.Address;
            txtCity.Text = _supplier.City;
            cmbStatus.SelectedItem = _supplier.Status ?? "Active";
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCode.Text) || string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtMobile.Text))
            {
                MessageBox.Show("Please fill all required fields", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                _supplier.SupplierCode = txtCode.Text.Trim();
                _supplier.SupplierName = txtName.Text.Trim();
                _supplier.ContactPerson = txtContact.Text.Trim();
                _supplier.Mobile = txtMobile.Text.Trim();
                _supplier.Phone = txtPhone.Text.Trim();
                _supplier.Email = txtEmail.Text.Trim();
                _supplier.Address = txtAddress.Text.Trim();
                _supplier.City = txtCity.Text.Trim();
                _supplier.Status = cmbStatus.SelectedItem.ToString();
                if (_isEditMode) _supplierRepository.Update(_supplier);
                else _supplierRepository.Add(_supplier);
                MessageBox.Show($"Supplier {_isEditMode ? "updated" : "created"} successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
