using System;
using System.Windows.Forms;
using EnterpriseInventory.Core.Domain;
using EnterpriseInventory.Infrastructure.Repositories;
using EnterpriseInventory.UI.Helpers;

namespace EnterpriseInventory.UI.Forms.MasterData
{
    public class ProductForm : Form
    {
        private readonly IProductRepository _productRepository;
        private DataGridView dgvProducts;
        private TextBox txtSearch;

        public ProductForm()
        {
            _productRepository = new ProductRepository(DbConnectionFactory.CreateConnection());
            InitializeComponent();
            LoadProducts();
        }

        private void InitializeComponent()
        {
            this.Text = "Product Management";
            this.Size = new System.Drawing.Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = System.Drawing.Color.FromArgb(240, 240, 240) };
            var lblSearch = new Label { Text = "Search:", Location = new System.Drawing.Point(20, 25), AutoSize = true };
            txtSearch = new TextBox { Location = new System.Drawing.Point(80, 22), Size = new System.Drawing.Size(300, 23) };
            txtSearch.TextChanged += (s, e) => LoadProducts();

            var btnAdd = new Button { Text = "Add New Product", Location = new System.Drawing.Point(500, 20), Size = new System.Drawing.Size(140, 35), FlatStyle = FlatStyle.Flat, BackColor = System.Drawing.Color.FromArgb(0, 120, 215), ForeColor = System.Drawing.Color.White };
            btnAdd.Click += (s, e) => { using (var form = new ProductDetailForm()) { if (form.ShowDialog() == DialogResult.OK) LoadProducts(); } };

            var btnRefresh = new Button { Text = "Refresh", Location = new System.Drawing.Point(700, 20), Size = new System.Drawing.Size(100, 35), FlatStyle = FlatStyle.Flat };
            btnRefresh.Click += (s, e) => LoadProducts();

            pnlTop.Controls.Add(lblSearch);
            pnlTop.Controls.Add(txtSearch);
            pnlTop.Controls.Add(btnAdd);
            pnlTop.Controls.Add(btnRefresh);

            dgvProducts = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = false, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, BackgroundColor = System.Drawing.Color.White, BorderStyle = BorderStyle.None, RowHeadersVisible = false };
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { Name = "ProductID", HeaderText = "ID", DataPropertyName = "ProductID", Width = 60 });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { Name = "ProductCode", HeaderText = "Code", DataPropertyName = "ProductCode", Width = 100 });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { Name = "ProductName", HeaderText = "Product Name", DataPropertyName = "ProductName", Width = 300 });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { Name = "CategoryName", HeaderText = "Category", DataPropertyName = "CategoryName", Width = 120 });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { Name = "UnitName", HeaderText = "Unit", DataPropertyName = "UnitName", Width = 80 });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { Name = "SalePrice", HeaderText = "Sale Price", DataPropertyName = "SalePrice", Width = 100 });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", DataPropertyName = "Status", Width = 80 });
            dgvProducts.DoubleClick += (s, e) => EditProduct();

            this.Controls.Add(dgvProducts);
            this.Controls.Add(pnlTop);
        }

        private void LoadProducts()
        {
            try
            {
                var list = _productRepository.GetAllWithDetails();
                if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                    list = list.FindAll(p => p.ProductName.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase) || (p.ProductCode != null && p.ProductCode.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase)) || (p.Barcode != null && p.Barcode.Contains(txtSearch.Text)));
                dgvProducts.DataSource = null;
                dgvProducts.DataSource = list;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EditProduct()
        {
            if (dgvProducts.CurrentRow == null) return;
            var id = Convert.ToInt32(dgvProducts.CurrentRow.Cells["ProductID"].Value);
            var product = _productRepository.GetById(id);
            if (product != null)
            {
                using (var form = new ProductDetailForm(product))
                {
                    if (form.ShowDialog() == DialogResult.OK) LoadProducts();
                }
            }
        }
    }

    public class ProductDetailForm : Form
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly ISupplierRepository _supplierRepository;
        private readonly ILocationRepository _locationRepository;
        private Product _product;
        private bool _isEditMode;
        private TextBox txtCode, txtBarcode, txtName, txtDescription;
        private ComboBox cmbCategory, cmbUnit, cmbSupplier, cmbLocation, cmbStatus;
        private NumericUpDown nudPurchasePrice, nudSalePrice, nudMinStock, nudReorderLevel;

        public ProductDetailForm(Product product = null)
        {
            _productRepository = new ProductRepository(DbConnectionFactory.CreateConnection());
            _categoryRepository = new CategoryRepository(DbConnectionFactory.CreateConnection());
            _unitRepository = new UnitRepository(DbConnectionFactory.CreateConnection());
            _supplierRepository = new SupplierRepository(DbConnectionFactory.CreateConnection());
            _locationRepository = new LocationRepository(DbConnectionFactory.CreateConnection());
            _product = product ?? new Product();
            _isEditMode = product != null && product.ProductID > 0;
            InitializeComponent();
            LoadDropdowns();
            if (_isEditMode) LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = _isEditMode ? "Edit Product" : "Add Product";
            this.Size = new System.Drawing.Size(800, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.AutoScroll = true;

            int y = 20, x1 = 20, x2 = 420;
            var lblTitle = new Label { Text = _isEditMode ? "Edit Product" : "Add New Product", Font = new System.Drawing.Font("Arial", 14F, System.Drawing.FontStyle.Bold), Location = new System.Drawing.Point(x1, y), AutoSize = true };
            y += 40;

            txtCode = CreateTextBox(x1, y, "Product Code *", 180); 
            txtBarcode = CreateTextBox(x2, y, "Barcode", 180); 
            y += 55;

            txtName = CreateTextBox(x1, y, "Product Name *", 580); 
            y += 55;

            cmbCategory = CreateComboBox(x1, y, "Category *", 180); 
            cmbUnit = CreateComboBox(x2, y, "Unit *", 180); 
            y += 55;

            cmbSupplier = CreateComboBox(x1, y, "Supplier", 180); 
            cmbLocation = CreateComboBox(x2, y, "Location", 180); 
            y += 55;

            nudPurchasePrice = CreateNumericUpDown(x1, y, "Purchase Price", 180, 2); 
            nudSalePrice = CreateNumericUpDown(x2, y, "Sale Price *", 180, 2); 
            y += 55;

            nudMinStock = CreateNumericUpDown(x1, y, "Minimum Stock", 180, 0); 
            nudReorderLevel = CreateNumericUpDown(x2, y, "Reorder Level", 180, 0); 
            y += 55;

            txtDescription = CreateTextBox(x1, y, "Description", 580, multiline: true); 
            y += 90;

            var lblStatus = new Label { Text = "Status *", Location = new System.Drawing.Point(x1, y), AutoSize = true };
            cmbStatus = new ComboBox { Location = new System.Drawing.Point(x1, y + 25), Size = new System.Drawing.Size(150, 21), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.Add("Active");
            cmbStatus.Items.Add("Inactive");
            cmbStatus.Items.Add("Discontinued");
            cmbStatus.SelectedIndex = 0;

            var btnSave = new Button { Text = "Save", Location = new System.Drawing.Point(x1, y + 70), Size = new System.Drawing.Size(100, 35), FlatStyle = FlatStyle.Flat, BackColor = System.Drawing.Color.FromArgb(0, 120, 215), ForeColor = System.Drawing.Color.White };
            btnSave.Click += BtnSave_Click;
            var btnCancel = new Button { Text = "Cancel", Location = new System.Drawing.Point(x1 + 110, y + 70), Size = new System.Drawing.Size(100, 35), DialogResult = DialogResult.Cancel };

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

        private ComboBox CreateComboBox(int x, int y, string labelText, int width)
        {
            var lbl = new Label { Text = labelText, Location = new System.Drawing.Point(x, y), AutoSize = true };
            var cmb = new ComboBox { Location = new System.Drawing.Point(x, y + 25), Size = new System.Drawing.Size(width, 21), DropDownStyle = ComboBoxStyle.DropDownList };
            this.Controls.Add(lbl);
            this.Controls.Add(cmb);
            return cmb;
        }

        private NumericUpDown CreateNumericUpDown(int x, int y, string labelText, int width, int decimals)
        {
            var lbl = new Label { Text = labelText, Location = new System.Drawing.Point(x, y), AutoSize = true };
            var nud = new NumericUpDown { Location = new System.Drawing.Point(x, y + 25), Size = new System.Drawing.Size(width, 23), DecimalPlaces = decimals, ThousandsSeparator = true, Minimum = 0, Maximum = 99999999 };
            this.Controls.Add(lbl);
            this.Controls.Add(nud);
            return nud;
        }

        private void LoadDropdowns()
        {
            try
            {
                cmbCategory.DataSource = _categoryRepository.GetAll();
                cmbCategory.DisplayMember = "CategoryName";
                cmbCategory.ValueMember = "CategoryID";
                cmbCategory.Items.Insert(0, new System.Collections.Generic.KeyValuePair<int, string>(0, "-- Select --"));

                cmbUnit.DataSource = _unitRepository.GetAll();
                cmbUnit.DisplayMember = "UnitName";
                cmbUnit.ValueMember = "UnitID";

                var suppliers = _supplierRepository.GetAll();
                suppliers.Insert(0, new Supplier { SupplierID = 0, SupplierName = "-- Select --" });
                cmbSupplier.DataSource = suppliers;
                cmbSupplier.DisplayMember = "SupplierName";
                cmbSupplier.ValueMember = "SupplierID";

                var locations = _locationRepository.GetAll();
                locations.Insert(0, new Location { LocationID = 0, LocationName = "-- Select --" });
                cmbLocation.DataSource = locations;
                cmbLocation.DisplayMember = "LocationName";
                cmbLocation.ValueMember = "LocationID";
            }
            catch { }
        }

        private void LoadData()
        {
            txtCode.Text = _product.ProductCode;
            txtBarcode.Text = _product.Barcode;
            txtName.Text = _product.ProductName;
            txtDescription.Text = _product.Description;
            cmbCategory.SelectedValue = _product.CategoryID;
            cmbUnit.SelectedValue = _product.UnitID;
            cmbSupplier.SelectedValue = _product.SupplierID.HasValue ? _product.SupplierID.Value : 0;
            cmbLocation.SelectedValue = _product.LocationID.HasValue ? _product.LocationID.Value : 0;
            nudPurchasePrice.Value = _product.PurchasePrice;
            nudSalePrice.Value = _product.SalePrice;
            nudMinStock.Value = _product.MinimumStock;
            nudReorderLevel.Value = _product.ReorderLevel;
            cmbStatus.SelectedItem = _product.Status ?? "Active";
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCode.Text) || string.IsNullOrWhiteSpace(txtName.Text) || cmbCategory.SelectedValue == null || cmbUnit.SelectedValue == null)
            {
                MessageBox.Show("Please fill all required fields", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                _product.ProductCode = txtCode.Text.Trim();
                _product.Barcode = txtBarcode.Text.Trim();
                _product.ProductName = txtName.Text.Trim();
                _product.Description = txtDescription.Text.Trim();
                _product.CategoryID = Convert.ToInt32(cmbCategory.SelectedValue);
                _product.UnitID = Convert.ToInt32(cmbUnit.SelectedValue);
                _product.SupplierID = cmbSupplier.SelectedValue != null && Convert.ToInt32(cmbSupplier.SelectedValue) > 0 ? Convert.ToInt32(cmbSupplier.SelectedValue) : (int?)null;
                _product.LocationID = cmbLocation.SelectedValue != null && Convert.ToInt32(cmbLocation.SelectedValue) > 0 ? Convert.ToInt32(cmbLocation.SelectedValue) : (int?)null;
                _product.PurchasePrice = nudPurchasePrice.Value;
                _product.SalePrice = nudSalePrice.Value;
                _product.MinimumStock = nudMinStock.Value;
                _product.ReorderLevel = nudReorderLevel.Value;
                _product.Status = cmbStatus.SelectedItem.ToString();
                if (_isEditMode) _productRepository.Update(_product);
                else _productRepository.Add(_product);
                MessageBox.Show($"Product {_isEditMode ? "updated" : "created"} successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
