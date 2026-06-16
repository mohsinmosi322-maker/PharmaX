using System;
using System.Windows.Forms;
using EnterpriseERP.Application.Services;

namespace EnterpriseERP.UI.Forms.MasterData
{
    public partial class ProductForm : BaseForm
    {
        private DataGridView dgvProducts;
        private Button btnAdd, btnEdit, btnDelete, btnRefresh, btnBarcode;
        private TextBox txtSearch;

        public ProductForm(AuthService authService, FeatureService featureService) 
            : base(authService, featureService)
        {
            InitializeComponent();
            LoadProducts();
        }

        private void InitializeComponent()
        {
            this.Text = "Product Management";
            this.Size = new Size(1200, 700);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50 };
            var lblTitle = new Label 
            { 
                Text = "Product Management", 
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Location = new Point(10, 10)
            };
            
            txtSearch = new TextBox 
            { 
                Location = new Point(300, 15), 
                Size = new Size(250, 25),
                PlaceholderText = "Search by name/barcode..."
            };
            txtSearch.TextChanged += (s, e) => FilterProducts();

            btnRefresh = new Button { Text = "Refresh", Location = new Point(570, 12), Size = new Size(80, 30) };
            btnRefresh.Click += (s, e) => LoadProducts();

            btnAdd = new Button { Text = "Add New", Location = new Point(660, 12), Size = new Size(80, 30) };
            btnAdd.Click += (s, e) => OpenProductDialog(null);

            btnBarcode = new Button { Text = "Print Barcode", Location = new Point(750, 12), Size = new Size(100, 30) };
            btnBarcode.Click += (s, e) => PrintBarcode();

            panelTop.Controls.AddRange(new Control[] { lblTitle, txtSearch, btnRefresh, btnAdd, btnBarcode });

            dgvProducts = new DataGridView 
            { 
                Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, AllowUserToAddRows = false, ReadOnly = true
            };
            dgvProducts.Columns.Add("ProductID", "ID");
            dgvProducts.Columns.Add("ProductCode", "Code");
            dgvProducts.Columns.Add("Barcode", "Barcode");
            dgvProducts.Columns.Add("ProductName", "Product Name");
            dgvProducts.Columns.Add("CategoryName", "Category");
            dgvProducts.Columns.Add("UnitName", "Unit");
            dgvProducts.Columns.Add("PurchasePrice", "Purchase Price");
            dgvProducts.Columns.Add("SalePrice", "Sale Price");
            dgvProducts.Columns.Add("StockQty", "Stock");
            dgvProducts.Columns.Add("Status", "Status");

            var contextMenu = new ContextMenuStrip();
            var editItem = new ToolStripMenuItem("Edit");
            editItem.Click += (s, e) => OpenProductDialog(GetSelectedProduct());
            var historyItem = new ToolStripMenuItem("View History");
            historyItem.Click += (s, e) => ViewProductHistory();
            var barcodeItem = new ToolStripMenuItem("Print Barcode");
            barcodeItem.Click += (s, e) => PrintBarcode();
            var deleteItem = new ToolStripMenuItem("Delete");
            deleteItem.Click += (s, e) => DeleteProduct();
            contextMenu.Items.AddRange(new ToolStripItem[] { editItem, historyItem, barcodeItem, deleteItem });
            dgvProducts.ContextMenuStrip = contextMenu;

            this.Controls.Add(dgvProducts);
            this.Controls.Add(panelTop);
        }

        private void LoadProducts()
        {
            try
            {
                dgvProducts.Rows.Clear();
                dgvProducts.Rows.Add(1, "PROD001", "5012345678900", "Paracetamol 500mg", "Medicines", "Strip", "10.00", "15.00", "1000", "Active");
                dgvProducts.Rows.Add(2, "PROD002", "5012345678917", "Amoxicillin 250mg", "Medicines", "Box", "25.00", "35.00", "500", "Active");
                dgvProducts.Rows.Add(3, "PROD003", "5012345678924", "Syrup Cough 100ml", "Medicines", "Bottle", "50.00", "75.00", "200", "Active");
            }
            catch (Exception ex) { ShowError($"Error loading products: {ex.Message}"); }
        }

        private void FilterProducts() { }

        private dynamic GetSelectedProduct()
        {
            if (dgvProducts.CurrentRow != null)
            {
                return new 
                {
                    ProductID = Convert.ToInt32(dgvProducts.CurrentRow.Cells["ProductID"].Value),
                    ProductCode = dgvProducts.CurrentRow.Cells["ProductCode"].Value?.ToString(),
                    Barcode = dgvProducts.CurrentRow.Cells["Barcode"].Value?.ToString(),
                    ProductName = dgvProducts.CurrentRow.Cells["ProductName"].Value?.ToString(),
                    Status = dgvProducts.CurrentRow.Cells["Status"].Value?.ToString()
                };
            }
            return null;
        }

        private void OpenProductDialog(dynamic product)
        {
            using (var dialog = new ProductDialogForm(_authService, _featureService, product))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    LoadProducts();
                    ShowSuccess(product == null ? "Product added successfully" : "Product updated successfully");
                }
            }
        }

        private void ViewProductHistory()
        {
            var product = GetSelectedProduct();
            if (product == null) { ShowError("Please select a product"); return; }
            MessageBox.Show($"Product History for: {product.ProductName}", "Product History");
        }

        private void PrintBarcode()
        {
            var product = GetSelectedProduct();
            if (product == null) { ShowError("Please select a product"); return; }
            MessageBox.Show($"Printing barcode for: {product.Barcode}", "Print Barcode");
        }

        private void DeleteProduct()
        {
            var product = GetSelectedProduct();
            if (product == null) { ShowError("Please select a product to delete"); return; }
            if (ConfirmAction($"Are you sure you want to delete product '{product.ProductName}'?"))
            {
                try { LoadProducts(); ShowSuccess("Product deleted successfully"); }
                catch (Exception ex) { ShowError($"Error deleting product: {ex.Message}"); }
            }
        }
    }

    public partial class ProductDialogForm : Form
    {
        private readonly AuthService _authService;
        private readonly FeatureService _featureService;
        private readonly dynamic _product;
        private TextBox txtCode, txtBarcode, txtName, txtDescription, txtPurchasePrice, txtSalePrice, txtMinStock, txtReorderLevel;
        private ComboBox cmbCategory, cmbUnit, cmbSupplier, cmbLocation, cmbStatus;
        private Button btnSave, btnCancel, btnGenerateBarcode;

        public ProductDialogForm(AuthService authService, FeatureService featureService, dynamic product)
        {
            _authService = authService; _featureService = featureService; _product = product;
            InitializeComponent(); LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = _product == null ? "Add Product" : "Edit Product";
            this.Size = new Size(600, 600); this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false; this.MinimizeBox = false; this.StartPosition = FormStartPosition.CenterParent;
            this.AutoScroll = true;

            int y = 20;
            var lblCode = new Label { Text = "Product Code:", Location = new Point(20, y), Size = new Size(100, 25) };
            txtCode = new TextBox { Location = new Point(140, y), Size = new Size(200, 25) };
            btnGenerateBarcode = new Button { Text = "Generate", Location = new Point(350, y), Size = new Size(80, 25) };
            btnGenerateBarcode.Click += (s, e) => GenerateBarcode();
            y += 35;

            var lblBarcode = new Label { Text = "Barcode:", Location = new Point(20, y), Size = new Size(100, 25) };
            txtBarcode = new TextBox { Location = new Point(140, y), Size = new Size(290, 25) }; y += 35;

            var lblName = new Label { Text = "Product Name:", Location = new Point(20, y), Size = new Size(100, 25) };
            txtName = new TextBox { Location = new Point(140, y), Size = new Size(420, 25) }; y += 35;

            var lblCategory = new Label { Text = "Category:", Location = new Point(20, y), Size = new Size(100, 25) };
            cmbCategory = new ComboBox { Location = new Point(140, y), Size = new Size(200, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCategory.Items.AddRange(new object[] { "Medicines", "Equipment", "Consumables" }); y += 35;

            var lblUnit = new Label { Text = "Unit:", Location = new Point(20, y), Size = new Size(100, 25) };
            cmbUnit = new ComboBox { Location = new Point(140, y), Size = new Size(200, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbUnit.Items.AddRange(new object[] { "Piece", "Pack", "Box", "Strip", "Bottle" }); y += 35;

            var lblSupplier = new Label { Text = "Supplier:", Location = new Point(20, y), Size = new Size(100, 25) };
            cmbSupplier = new ComboBox { Location = new Point(140, y), Size = new Size(200, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbSupplier.Items.AddRange(new object[] { "ABC Pharmaceuticals", "Medi Supplies Ltd" }); y += 35;

            var lblLocation = new Label { Text = "Location:", Location = new Point(20, y), Size = new Size(100, 25) };
            cmbLocation = new ComboBox { Location = new Point(140, y), Size = new Size(200, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbLocation.Items.AddRange(new object[] { "Rack A", "Rack B", "Shelf 1" }); y += 35;

            var lblPurchase = new Label { Text = "Purchase Price:", Location = new Point(20, y), Size = new Size(100, 25) };
            txtPurchasePrice = new TextBox { Location = new Point(140, y), Size = new Size(200, 25) }; y += 35;

            var lblSale = new Label { Text = "Sale Price:", Location = new Point(20, y), Size = new Size(100, 25) };
            txtSalePrice = new TextBox { Location = new Point(140, y), Size = new Size(200, 25) }; y += 35;

            var lblMinStock = new Label { Text = "Min Stock:", Location = new Point(20, y), Size = new Size(100, 25) };
            txtMinStock = new TextBox { Location = new Point(140, y), Size = new Size(200, 25) }; y += 35;

            var lblReorder = new Label { Text = "Reorder Level:", Location = new Point(20, y), Size = new Size(100, 25) };
            txtReorderLevel = new TextBox { Location = new Point(140, y), Size = new Size(200, 25) }; y += 35;

            var lblDesc = new Label { Text = "Description:", Location = new Point(20, y), Size = new Size(100, 25) };
            txtDescription = new TextBox { Location = new Point(140, y), Size = new Size(420, 25), Multiline = true, Height = 50 }; y += 60;

            var lblStatus = new Label { Text = "Status:", Location = new Point(20, y), Size = new Size(100, 25) };
            cmbStatus = new ComboBox { Location = new Point(140, y), Size = new Size(200, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.AddRange(new object[] { "Active", "Inactive", "Discontinued" }); y += 45;

            btnSave = new Button { Text = "Save", Location = new Point(200, y), Size = new Size(80, 30) };
            btnSave.Click += (s, e) => SaveProduct();
            btnCancel = new Button { Text = "Cancel", Location = new Point(300, y), Size = new Size(80, 30) };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.AddRange(new Control[] { lblCode, txtCode, btnGenerateBarcode, lblBarcode, txtBarcode, lblName, txtName, lblCategory, cmbCategory, lblUnit, cmbUnit, lblSupplier, cmbSupplier, lblLocation, cmbLocation, lblPurchase, txtPurchasePrice, lblSale, txtSalePrice, lblMinStock, txtMinStock, lblReorder, txtReorderLevel, lblDesc, txtDescription, lblStatus, cmbStatus, btnSave, btnCancel });
            this.AcceptButton = btnSave; this.CancelButton = btnCancel;
        }

        private void LoadData()
        {
            if (_product != null)
            {
                txtCode.Text = _product.ProductCode; txtBarcode.Text = _product.Barcode;
                txtName.Text = _product.ProductName; cmbStatus.SelectedItem = _product.Status;
            }
            else { cmbStatus.SelectedItem = "Active"; }
        }

        private void GenerateBarcode()
        {
            var random = new Random();
            txtBarcode.Text = $"50{random.Next(1000000000, 9999999999)}";
        }

        private void SaveProduct()
        {
            if (string.IsNullOrWhiteSpace(txtCode.Text) || string.IsNullOrWhiteSpace(txtName.Text))
            { MessageBox.Show("Code and Name are required", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            this.DialogResult = DialogResult.OK;
        }
    }
}
