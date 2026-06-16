using System;
using System.Windows.Forms;
using EnterpriseERP.Application.Services;

namespace EnterpriseERP.UI.Forms.MasterData
{
    public partial class SupplierForm : BaseForm
    {
        private DataGridView dgvSuppliers;
        private Button btnAdd, btnEdit, btnDelete, btnRefresh;
        private TextBox txtSearch;

        public SupplierForm(AuthService authService, FeatureService featureService) 
            : base(authService, featureService)
        {
            InitializeComponent();
            LoadSuppliers();
        }

        private void InitializeComponent()
        {
            this.Text = "Supplier Management";
            this.Size = new Size(1000, 600);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50 };
            var lblTitle = new Label 
            { 
                Text = "Supplier Management", 
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Location = new Point(10, 10)
            };
            
            txtSearch = new TextBox 
            { 
                Location = new Point(300, 15), 
                Size = new Size(200, 25),
                PlaceholderText = "Search..."
            };
            txtSearch.TextChanged += (s, e) => FilterSuppliers();

            btnRefresh = new Button 
            { 
                Text = "Refresh", 
                Location = new Point(520, 12),
                Size = new Size(80, 30)
            };
            btnRefresh.Click += (s, e) => LoadSuppliers();

            btnAdd = new Button 
            { 
                Text = "Add New", 
                Location = new Point(620, 12),
                Size = new Size(80, 30)
            };
            btnAdd.Click += (s, e) => OpenSupplierDialog(null);

            panelTop.Controls.AddRange(new Control[] { lblTitle, txtSearch, btnRefresh, btnAdd });

            dgvSuppliers = new DataGridView 
            { 
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                ReadOnly = true
            };
            dgvSuppliers.Columns.Add("SupplierID", "ID");
            dgvSuppliers.Columns.Add("SupplierCode", "Code");
            dgvSuppliers.Columns.Add("SupplierName", "Name");
            dgvSuppliers.Columns.Add("ContactPerson", "Contact Person");
            dgvSuppliers.Columns.Add("Mobile", "Mobile");
            dgvSuppliers.Columns.Add("Phone", "Phone");
            dgvSuppliers.Columns.Add("City", "City");
            dgvSuppliers.Columns.Add("Status", "Status");

            var contextMenu = new ContextMenuStrip();
            var editItem = new ToolStripMenuItem("Edit");
            editItem.Click += (s, e) => OpenSupplierDialog(GetSelectedSupplier());
            var deleteItem = new ToolStripMenuItem("Delete");
            deleteItem.Click += (s, e) => DeleteSupplier();
            contextMenu.Items.AddRange(new ToolStripItem[] { editItem, deleteItem });
            dgvSuppliers.ContextMenuStrip = contextMenu;

            this.Controls.Add(dgvSuppliers);
            this.Controls.Add(panelTop);
        }

        private void LoadSuppliers()
        {
            try
            {
                dgvSuppliers.Rows.Clear();
                
                // Sample data
                dgvSuppliers.Rows.Add(1, "SUP001", "ABC Pharmaceuticals", "John Smith", "03001234567", "0421234567", "Lahore", "Active");
                dgvSuppliers.Rows.Add(2, "SUP002", "Medi Supplies Ltd", "Ali Khan", "03009876543", "0429876543", "Karachi", "Active");
                dgvSuppliers.Rows.Add(3, "SUP003", "HealthCare Distributors", "Sara Ahmed", "03111234567", "0511234567", "Islamabad", "Active");
            }
            catch (Exception ex)
            {
                ShowError($"Error loading suppliers: {ex.Message}");
            }
        }

        private void FilterSuppliers()
        {
            // Implement filtering logic
        }

        private dynamic GetSelectedSupplier()
        {
            if (dgvSuppliers.CurrentRow != null)
            {
                return new 
                {
                    SupplierID = Convert.ToInt32(dgvSuppliers.CurrentRow.Cells["SupplierID"].Value),
                    SupplierCode = dgvSuppliers.CurrentRow.Cells["SupplierCode"].Value?.ToString(),
                    SupplierName = dgvSuppliers.CurrentRow.Cells["SupplierName"].Value?.ToString(),
                    ContactPerson = dgvSuppliers.CurrentRow.Cells["ContactPerson"].Value?.ToString(),
                    Mobile = dgvSuppliers.CurrentRow.Cells["Mobile"].Value?.ToString(),
                    Phone = dgvSuppliers.CurrentRow.Cells["Phone"].Value?.ToString(),
                    City = dgvSuppliers.CurrentRow.Cells["City"].Value?.ToString(),
                    Status = dgvSuppliers.CurrentRow.Cells["Status"].Value?.ToString()
                };
            }
            return null;
        }

        private void OpenSupplierDialog(dynamic supplier)
        {
            using (var dialog = new SupplierDialogForm(_authService, _featureService, supplier))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    LoadSuppliers();
                    ShowSuccess(supplier == null ? "Supplier added successfully" : "Supplier updated successfully");
                }
            }
        }

        private void DeleteSupplier()
        {
            var supplier = GetSelectedSupplier();
            if (supplier == null)
            {
                ShowError("Please select a supplier to delete");
                return;
            }

            if (ConfirmAction($"Are you sure you want to delete supplier '{supplier.SupplierName}'?"))
            {
                try
                {
                    LoadSuppliers();
                    ShowSuccess("Supplier deleted successfully");
                }
                catch (Exception ex)
                {
                    ShowError($"Error deleting supplier: {ex.Message}");
                }
            }
        }
    }

    public partial class SupplierDialogForm : Form
    {
        private readonly AuthService _authService;
        private readonly FeatureService _featureService;
        private readonly dynamic _supplier;

        private TextBox txtCode, txtName, txtContact, txtMobile, txtPhone, txtEmail, txtAddress, txtCity;
        private ComboBox cmbStatus;
        private Button btnSave, btnCancel;

        public SupplierDialogForm(AuthService authService, FeatureService featureService, dynamic supplier)
        {
            _authService = authService;
            _featureService = featureService;
            _supplier = supplier;

            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = _supplier == null ? "Add Supplier" : "Edit Supplier";
            this.Size = new Size(500, 500);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.AutoScroll = true;

            int y = 20;
            var lblCode = new Label { Text = "Supplier Code:", Location = new Point(20, y), Size = new Size(100, 25) };
            txtCode = new TextBox { Location = new Point(140, y), Size = new Size(320, 25) };
            y += 35;

            var lblName = new Label { Text = "Supplier Name:", Location = new Point(20, y), Size = new Size(100, 25) };
            txtName = new TextBox { Location = new Point(140, y), Size = new Size(320, 25) };
            y += 35;

            var lblContact = new Label { Text = "Contact Person:", Location = new Point(20, y), Size = new Size(100, 25) };
            txtContact = new TextBox { Location = new Point(140, y), Size = new Size(320, 25) };
            y += 35;

            var lblMobile = new Label { Text = "Mobile:", Location = new Point(20, y), Size = new Size(100, 25) };
            txtMobile = new TextBox { Location = new Point(140, y), Size = new Size(320, 25) };
            y += 35;

            var lblPhone = new Label { Text = "Phone:", Location = new Point(20, y), Size = new Size(100, 25) };
            txtPhone = new TextBox { Location = new Point(140, y), Size = new Size(320, 25) };
            y += 35;

            var lblEmail = new Label { Text = "Email:", Location = new Point(20, y), Size = new Size(100, 25) };
            txtEmail = new TextBox { Location = new Point(140, y), Size = new Size(320, 25) };
            y += 35;

            var lblAddress = new Label { Text = "Address:", Location = new Point(20, y), Size = new Size(100, 25) };
            txtAddress = new TextBox { Location = new Point(140, y), Size = new Size(320, 25), Multiline = true, Height = 50 };
            y += 60;

            var lblCity = new Label { Text = "City:", Location = new Point(20, y), Size = new Size(100, 25) };
            txtCity = new TextBox { Location = new Point(140, y), Size = new Size(320, 25) };
            y += 35;

            var lblStatus = new Label { Text = "Status:", Location = new Point(20, y), Size = new Size(100, 25) };
            cmbStatus = new ComboBox { Location = new Point(140, y), Size = new Size(320, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.AddRange(new object[] { "Active", "Inactive" });
            y += 45;

            btnSave = new Button { Text = "Save", Location = new Point(180, y), Size = new Size(80, 30) };
            btnSave.Click += (s, e) => SaveSupplier();

            btnCancel = new Button { Text = "Cancel", Location = new Point(280, y), Size = new Size(80, 30) };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.AddRange(new Control[] { 
                lblCode, txtCode, lblName, txtName, lblContact, txtContact, 
                lblMobile, txtMobile, lblPhone, txtPhone, lblEmail, txtEmail,
                lblAddress, txtAddress, lblCity, txtCity, lblStatus, cmbStatus, 
                btnSave, btnCancel 
            });
            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }

        private void LoadData()
        {
            if (_supplier != null)
            {
                txtCode.Text = _supplier.SupplierCode;
                txtName.Text = _supplier.SupplierName;
                txtContact.Text = _supplier.ContactPerson;
                txtMobile.Text = _supplier.Mobile;
                txtPhone.Text = _supplier.Phone;
                txtEmail.Text = _supplier.Email;
                txtAddress.Text = _supplier.Address;
                txtCity.Text = _supplier.City;
                cmbStatus.SelectedItem = _supplier.Status;
            }
            else
            {
                cmbStatus.SelectedItem = "Active";
            }
        }

        private void SaveSupplier()
        {
            if (string.IsNullOrWhiteSpace(txtCode.Text))
            {
                MessageBox.Show("Supplier code is required", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCode.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Supplier name is required", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            this.DialogResult = DialogResult.OK;
        }
    }
}
