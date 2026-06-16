using System;
using System.Windows.Forms;
using EnterpriseInventory.Core.Domain;
using EnterpriseInventory.Infrastructure.Repositories;
using EnterpriseInventory.UI.Helpers;

namespace EnterpriseInventory.UI.Forms.MasterData
{
    public class CustomerForm : Form
    {
        private readonly ICustomerRepository _customerRepository;
        private DataGridView dgvCustomers;
        private TextBox txtSearch;

        public CustomerForm()
        {
            _customerRepository = new CustomerRepository(DbConnectionFactory.CreateConnection());
            InitializeComponent();
            LoadCustomers();
        }

        private void InitializeComponent()
        {
            this.Text = "Customer Management";
            this.Size = new System.Drawing.Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterScreen;

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = System.Drawing.Color.FromArgb(240, 240, 240) };
            var lblSearch = new Label { Text = "Search:", Location = new System.Drawing.Point(20, 25), AutoSize = true };
            txtSearch = new TextBox { Location = new System.Drawing.Point(80, 22), Size = new System.Drawing.Size(300, 23) };
            txtSearch.TextChanged += (s, e) => LoadCustomers();

            var btnAdd = new Button { Text = "Add New Customer", Location = new System.Drawing.Point(500, 20), Size = new System.Drawing.Size(130, 35), FlatStyle = FlatStyle.Flat, BackColor = System.Drawing.Color.FromArgb(0, 120, 215), ForeColor = System.Drawing.Color.White };
            btnAdd.Click += (s, e) => { using (var form = new CustomerDetailForm()) { if (form.ShowDialog() == DialogResult.OK) LoadCustomers(); } };

            var btnRefresh = new Button { Text = "Refresh", Location = new System.Drawing.Point(700, 20), Size = new System.Drawing.Size(100, 35), FlatStyle = FlatStyle.Flat };
            btnRefresh.Click += (s, e) => LoadCustomers();

            pnlTop.Controls.Add(lblSearch);
            pnlTop.Controls.Add(txtSearch);
            pnlTop.Controls.Add(btnAdd);
            pnlTop.Controls.Add(btnRefresh);

            dgvCustomers = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = false, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, BackgroundColor = System.Drawing.Color.White, BorderStyle = BorderStyle.None, RowHeadersVisible = false };
            dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn { Name = "CustomerID", HeaderText = "ID", DataPropertyName = "CustomerID", Width = 60 });
            dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn { Name = "CustomerCode", HeaderText = "Code", DataPropertyName = "CustomerCode", Width = 100 });
            dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn { Name = "CustomerName", HeaderText = "Customer Name", DataPropertyName = "CustomerName", Width = 250 });
            dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn { Name = "Mobile", HeaderText = "Mobile", DataPropertyName = "Mobile", Width = 120 });
            dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn { Name = "City", HeaderText = "City", DataPropertyName = "City", Width = 120 });
            dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn { Name = "CurrentBalance", HeaderText = "Balance", DataPropertyName = "CurrentBalance", Width = 100 });
            dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", DataPropertyName = "Status", Width = 80 });
            dgvCustomers.DoubleClick += (s, e) => EditCustomer();

            this.Controls.Add(dgvCustomers);
            this.Controls.Add(pnlTop);
        }

        private void LoadCustomers()
        {
            try
            {
                var list = _customerRepository.GetAll();
                if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                    list = list.FindAll(c => c.CustomerName.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase) || (c.CustomerCode != null && c.CustomerCode.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase)) || (c.Mobile != null && c.Mobile.Contains(txtSearch.Text)));
                dgvCustomers.DataSource = null;
                dgvCustomers.DataSource = list;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EditCustomer()
        {
            if (dgvCustomers.CurrentRow == null) return;
            var id = Convert.ToInt32(dgvCustomers.CurrentRow.Cells["CustomerID"].Value);
            var customer = _customerRepository.GetById(id);
            if (customer != null)
            {
                using (var form = new CustomerDetailForm(customer))
                {
                    if (form.ShowDialog() == DialogResult.OK) LoadCustomers();
                }
            }
        }
    }

    public class CustomerDetailForm : Form
    {
        private readonly ICustomerRepository _customerRepository;
        private Customer _customer;
        private bool _isEditMode;
        private TextBox txtCode, txtName, txtMobile, txtAddress, txtCity, txtOpeningBalance, txtCreditLimit;
        private ComboBox cmbStatus;

        public CustomerDetailForm(Customer customer = null)
        {
            _customerRepository = new CustomerRepository(DbConnectionFactory.CreateConnection());
            _customer = customer ?? new Customer();
            _isEditMode = customer != null && customer.CustomerID > 0;
            InitializeComponent();
            if (_isEditMode) LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = _isEditMode ? "Edit Customer" : "Add Customer";
            this.Size = new System.Drawing.Size(700, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            int y = 20;
            var lblTitle = new Label { Text = _isEditMode ? "Edit Customer" : "Add New Customer", Font = new System.Drawing.Font("Arial", 14F, System.Drawing.FontStyle.Bold), Location = new System.Drawing.Point(20, y), AutoSize = true };
            y += 40;

            txtCode = CreateTextBox(20, y, "Customer Code *", 300); y += 55;
            txtName = CreateTextBox(20, y, "Customer Name *", 600); y += 55;
            txtMobile = CreateTextBox(20, y, "Mobile *", 300); y += 55;
            txtAddress = CreateTextBox(20, y, "Address", 600, multiline: true); y += 90;
            txtCity = CreateTextBox(20, y, "City", 300); y += 55;
            txtOpeningBalance = CreateNumericTextBox(20, y, "Opening Balance", 150); y += 55;
            txtCreditLimit = CreateNumericTextBox(20, y, "Credit Limit", 150); y += 55;

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

        private TextBox CreateNumericTextBox(int x, int y, string labelText, int width)
        {
            var lbl = new Label { Text = labelText, Location = new System.Drawing.Point(x, y), AutoSize = true };
            var txt = new TextBox { Location = new System.Drawing.Point(x, y + 25), Size = new System.Drawing.Size(width, 23), TextAlign = HorizontalAlignment.Right };
            this.Controls.Add(lbl);
            this.Controls.Add(txt);
            return txt;
        }

        private void LoadData()
        {
            txtCode.Text = _customer.CustomerCode;
            txtName.Text = _customer.CustomerName;
            txtMobile.Text = _customer.Mobile;
            txtAddress.Text = _customer.Address;
            txtCity.Text = _customer.City;
            txtOpeningBalance.Text = _customer.OpeningBalance.ToString("F2");
            txtCreditLimit.Text = _customer.CreditLimit.ToString("F2");
            cmbStatus.SelectedItem = _customer.Status ?? "Active";
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
                _customer.CustomerCode = txtCode.Text.Trim();
                _customer.CustomerName = txtName.Text.Trim();
                _customer.Mobile = txtMobile.Text.Trim();
                _customer.Address = txtAddress.Text.Trim();
                _customer.City = txtCity.Text.Trim();
                decimal openingBalance = 0, creditLimit = 0;
                decimal.TryParse(txtOpeningBalance.Text, out openingBalance);
                decimal.TryParse(txtCreditLimit.Text, out creditLimit);
                _customer.OpeningBalance = openingBalance;
                _customer.CurrentBalance = _isEditMode ? _customer.CurrentBalance : openingBalance;
                _customer.CreditLimit = creditLimit;
                _customer.Status = cmbStatus.SelectedItem.ToString();
                if (_isEditMode) _customerRepository.Update(_customer);
                else _customerRepository.Add(_customer);
                MessageBox.Show($"Customer {_isEditMode ? "updated" : "created"} successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
