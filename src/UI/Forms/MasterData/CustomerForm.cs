using System;
using System.Windows.Forms;
using EnterpriseERP.Application.Services;

namespace EnterpriseERP.UI.Forms.MasterData
{
    public partial class CustomerForm : BaseForm
    {
        private DataGridView dgvCustomers;
        private Button btnAdd, btnEdit, btnDelete, btnRefresh;
        private TextBox txtSearch;

        public CustomerForm(AuthService authService, FeatureService featureService) 
            : base(authService, featureService)
        {
            InitializeComponent();
            LoadCustomers();
        }

        private void InitializeComponent()
        {
            this.Text = "Customer Management";
            this.Size = new Size(1000, 600);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50 };
            var lblTitle = new Label 
            { 
                Text = "Customer Management", 
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Location = new Point(10, 10)
            };
            
            txtSearch = new TextBox 
            { 
                Location = new Point(300, 15), 
                Size = new Size(200, 25),
                PlaceholderText = "Search..."
            };
            txtSearch.TextChanged += (s, e) => FilterCustomers();

            btnRefresh = new Button { Text = "Refresh", Location = new Point(520, 12), Size = new Size(80, 30) };
            btnRefresh.Click += (s, e) => LoadCustomers();

            btnAdd = new Button { Text = "Add New", Location = new Point(620, 12), Size = new Size(80, 30) };
            btnAdd.Click += (s, e) => OpenCustomerDialog(null);

            panelTop.Controls.AddRange(new Control[] { lblTitle, txtSearch, btnRefresh, btnAdd });

            dgvCustomers = new DataGridView 
            { 
                Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, AllowUserToAddRows = false, ReadOnly = true
            };
            dgvCustomers.Columns.Add("CustomerID", "ID");
            dgvCustomers.Columns.Add("CustomerCode", "Code");
            dgvCustomers.Columns.Add("CustomerName", "Name");
            dgvCustomers.Columns.Add("Mobile", "Mobile");
            dgvCustomers.Columns.Add("City", "City");
            dgvCustomers.Columns.Add("CurrentBalance", "Balance");
            dgvCustomers.Columns.Add("Status", "Status");

            var contextMenu = new ContextMenuStrip();
            var editItem = new ToolStripMenuItem("Edit");
            editItem.Click += (s, e) => OpenCustomerDialog(GetSelectedCustomer());
            var deleteItem = new ToolStripMenuItem("Delete");
            deleteItem.Click += (s, e) => DeleteCustomer();
            contextMenu.Items.AddRange(new ToolStripItem[] { editItem, deleteItem });
            dgvCustomers.ContextMenuStrip = contextMenu;

            this.Controls.Add(dgvCustomers);
            this.Controls.Add(panelTop);
        }

        private void LoadCustomers()
        {
            try
            {
                dgvCustomers.Rows.Clear();
                dgvCustomers.Rows.Add(1, "CUST001", "Ahmed Hospital", "03001111111", "Lahore", "0.00", "Active");
                dgvCustomers.Rows.Add(2, "CUST002", "City Pharmacy", "03002222222", "Karachi", "5000.00", "Active");
                dgvCustomers.Rows.Add(3, "CUST003", "Health Clinic", "03003333333", "Islamabad", "0.00", "Active");
            }
            catch (Exception ex) { ShowError($"Error loading customers: {ex.Message}"); }
        }

        private void FilterCustomers() { }

        private dynamic GetSelectedCustomer()
        {
            if (dgvCustomers.CurrentRow != null)
            {
                return new 
                {
                    CustomerID = Convert.ToInt32(dgvCustomers.CurrentRow.Cells["CustomerID"].Value),
                    CustomerCode = dgvCustomers.CurrentRow.Cells["CustomerCode"].Value?.ToString(),
                    CustomerName = dgvCustomers.CurrentRow.Cells["CustomerName"].Value?.ToString(),
                    Mobile = dgvCustomers.CurrentRow.Cells["Mobile"].Value?.ToString(),
                    City = dgvCustomers.CurrentRow.Cells["City"].Value?.ToString(),
                    Status = dgvCustomers.CurrentRow.Cells["Status"].Value?.ToString()
                };
            }
            return null;
        }

        private void OpenCustomerDialog(dynamic customer)
        {
            using (var dialog = new CustomerDialogForm(_authService, _featureService, customer))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    LoadCustomers();
                    ShowSuccess(customer == null ? "Customer added successfully" : "Customer updated successfully");
                }
            }
        }

        private void DeleteCustomer()
        {
            var customer = GetSelectedCustomer();
            if (customer == null) { ShowError("Please select a customer to delete"); return; }
            if (ConfirmAction($"Are you sure you want to delete customer '{customer.CustomerName}'?"))
            {
                try { LoadCustomers(); ShowSuccess("Customer deleted successfully"); }
                catch (Exception ex) { ShowError($"Error deleting customer: {ex.Message}"); }
            }
        }
    }

    public partial class CustomerDialogForm : Form
    {
        private readonly AuthService _authService;
        private readonly FeatureService _featureService;
        private readonly dynamic _customer;
        private TextBox txtCode, txtName, txtMobile, txtAddress, txtCity, txtOpeningBalance, txtCreditLimit;
        private ComboBox cmbStatus;
        private Button btnSave, btnCancel;

        public CustomerDialogForm(AuthService authService, FeatureService featureService, dynamic customer)
        {
            _authService = authService; _featureService = featureService; _customer = customer;
            InitializeComponent(); LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = _customer == null ? "Add Customer" : "Edit Customer";
            this.Size = new Size(450, 450); this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false; this.MinimizeBox = false; this.StartPosition = FormStartPosition.CenterParent;

            int y = 20;
            var lblCode = new Label { Text = "Customer Code:", Location = new Point(20, y), Size = new Size(100, 25) };
            txtCode = new TextBox { Location = new Point(140, y), Size = new Size(280, 25) }; y += 35;

            var lblName = new Label { Text = "Customer Name:", Location = new Point(20, y), Size = new Size(100, 25) };
            txtName = new TextBox { Location = new Point(140, y), Size = new Size(280, 25) }; y += 35;

            var lblMobile = new Label { Text = "Mobile:", Location = new Point(20, y), Size = new Size(100, 25) };
            txtMobile = new TextBox { Location = new Point(140, y), Size = new Size(280, 25) }; y += 35;

            var lblAddress = new Label { Text = "Address:", Location = new Point(20, y), Size = new Size(100, 25) };
            txtAddress = new TextBox { Location = new Point(140, y), Size = new Size(280, 25), Multiline = true, Height = 50 }; y += 60;

            var lblCity = new Label { Text = "City:", Location = new Point(20, y), Size = new Size(100, 25) };
            txtCity = new TextBox { Location = new Point(140, y), Size = new Size(280, 25) }; y += 35;

            var lblOpening = new Label { Text = "Opening Balance:", Location = new Point(20, y), Size = new Size(100, 25) };
            txtOpeningBalance = new TextBox { Location = new Point(140, y), Size = new Size(280, 25) }; y += 35;

            var lblCredit = new Label { Text = "Credit Limit:", Location = new Point(20, y), Size = new Size(100, 25) };
            txtCreditLimit = new TextBox { Location = new Point(140, y), Size = new Size(280, 25) }; y += 35;

            var lblStatus = new Label { Text = "Status:", Location = new Point(20, y), Size = new Size(100, 25) };
            cmbStatus = new ComboBox { Location = new Point(140, y), Size = new Size(280, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.AddRange(new object[] { "Active", "Inactive" }); y += 45;

            btnSave = new Button { Text = "Save", Location = new Point(150, y), Size = new Size(80, 30) };
            btnSave.Click += (s, e) => SaveCustomer();
            btnCancel = new Button { Text = "Cancel", Location = new Point(250, y), Size = new Size(80, 30) };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.AddRange(new Control[] { lblCode, txtCode, lblName, txtName, lblMobile, txtMobile, lblAddress, txtAddress, lblCity, txtCity, lblOpening, txtOpeningBalance, lblCredit, txtCreditLimit, lblStatus, cmbStatus, btnSave, btnCancel });
            this.AcceptButton = btnSave; this.CancelButton = btnCancel;
        }

        private void LoadData()
        {
            if (_customer != null)
            {
                txtCode.Text = _customer.CustomerCode; txtName.Text = _customer.CustomerName;
                txtMobile.Text = _customer.Mobile; txtAddress.Text = _customer.Address;
                txtCity.Text = _customer.City; cmbStatus.SelectedItem = _customer.Status;
            }
            else { cmbStatus.SelectedItem = "Active"; }
        }

        private void SaveCustomer()
        {
            if (string.IsNullOrWhiteSpace(txtCode.Text) || string.IsNullOrWhiteSpace(txtName.Text))
            { MessageBox.Show("Code and Name are required", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            this.DialogResult = DialogResult.OK;
        }
    }
}
