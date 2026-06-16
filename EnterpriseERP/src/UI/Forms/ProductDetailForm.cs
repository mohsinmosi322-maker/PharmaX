using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using EnterpriseInventory.Core.Domain;
using EnterpriseInventory.Core.Interfaces;
using EnterpriseInventory.Application.Services;

namespace EnterpriseInventory.UI.Forms
{
    public partial class ProductDetailForm : Form
    {
        private readonly int _branchId;
        private readonly User _currentUser;
        private readonly Product _existingProduct;
        
        private TextBox txtProductCode, txtBarcode, txtProductName, txtDescription;
        private ComboBox cmbCategory, cmbUnit, cmbSupplier, cmbLocation, cmbStatus;
        private NumericUpDown numPurchasePrice, numSalePrice, numMinStock, numReorderLevel;
        private Button btnSave, btnCancel;
        private Panel pnlMain;
        
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly ISupplierRepository _supplierRepository;
        private readonly ILocationRepository _locationRepository;

        public ProductDetailForm(int branchId, User currentUser, Product existingProduct = null)
        {
            _branchId = branchId;
            _currentUser = currentUser;
            _existingProduct = existingProduct;
            
            _productRepository = new ProductRepository(DbConnectionFactory.CreateConnection());
            _categoryRepository = new CategoryRepository(DbConnectionFactory.CreateConnection());
            _unitRepository = new UnitRepository(DbConnectionFactory.CreateConnection());
            _supplierRepository = new SupplierRepository(DbConnectionFactory.CreateConnection());
            _locationRepository = new LocationRepository(DbConnectionFactory.CreateConnection());
            
            InitializeComponent();
            LoadComboData();
            
            if (_existingProduct != null)
            {
                LoadProductData();
                this.Text = "Edit Product";
            }
            else
            {
                this.Text = "Add New Product";
                GenerateProductCode();
            }
        }

        private void InitializeComponent()
        {
            this.Size = new Size(800, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            pnlMain = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20)
            };

            int y = 20;
            int labelWidth = 120;
            int fieldWidth = 300;
            int gap = 40;

            // Row 1: Product Code & Barcode
            CreateLabel("Product Code*:", 20, y, labelWidth);
            txtProductCode = CreateTextBox(140, y, fieldWidth);
            y += 10;
            txtProductCode.Height = 25;
            
            CreateLabel("Barcode:", 20 + labelWidth + gap, y - 35, labelWidth);
            txtBarcode = CreateTextBox(20 + labelWidth + gap + labelWidth, y - 35, fieldWidth);
            var btnGenerateBarcode = new Button
            {
                Text = "Generate",
                Location = new Point(20 + labelWidth + gap + labelWidth + fieldWidth + 10, y - 35),
                Size = new Size(80, 25)
            };
            btnGenerateBarcode.Click += BtnGenerateBarcode_Click;
            pnlMain.Controls.Add(btnGenerateBarcode);

            y += 50;

            // Row 2: Product Name
            CreateLabel("Product Name*:", 20, y, labelWidth);
            txtProductName = CreateTextBox(140, y, fieldWidth * 2 + gap + labelWidth);
            y += 50;

            // Row 3: Category & Unit
            CreateLabel("Category*:", 20, y, labelWidth);
            cmbCategory = CreateComboBox(140, y, fieldWidth);
            
            CreateLabel("Unit*:", 20 + labelWidth + gap, y, labelWidth);
            cmbUnit = CreateComboBox(20 + labelWidth + gap + labelWidth, y, fieldWidth);
            y += 50;

            // Row 4: Supplier & Location
            CreateLabel("Supplier:", 20, y, labelWidth);
            cmbSupplier = CreateComboBox(140, y, fieldWidth);
            
            CreateLabel("Location:", 20 + labelWidth + gap, y, labelWidth);
            cmbLocation = CreateComboBox(20 + labelWidth + gap + labelWidth, y, fieldWidth);
            y += 50;

            // Row 5: Purchase Price & Sale Price
            CreateLabel("Cost Price*:", 20, y, labelWidth);
            numPurchasePrice = CreateNumericUpDown(140, y, fieldWidth, 2);
            
            CreateLabel("Sale Price*:", 20 + labelWidth + gap, y, labelWidth);
            numSalePrice = CreateNumericUpDown(20 + labelWidth + gap + labelWidth, y, fieldWidth, 2);
            y += 50;

            // Row 6: Min Stock & Reorder Level
            CreateLabel("Min Stock:", 20, y, labelWidth);
            numMinStock = CreateNumericUpDown(140, y, fieldWidth, 0);
            
            CreateLabel("Reorder Level:", 20 + labelWidth + gap, y, labelWidth);
            numReorderLevel = CreateNumericUpDown(20 + labelWidth + gap + labelWidth, y, fieldWidth, 0);
            y += 50;

            // Row 7: Status & Description
            CreateLabel("Status:", 20, y, labelWidth);
            cmbStatus = CreateComboBox(140, y, fieldWidth);
            cmbStatus.Items.AddRange(new object[] { "Active", "Inactive", "Discontinued" });
            cmbStatus.SelectedIndex = 0;
            
            CreateLabel("Description:", 20 + labelWidth + gap, y, labelWidth);
            txtDescription = CreateTextBox(20 + labelWidth + gap + labelWidth, y, fieldWidth);
            y += 50;

            // Description (multi-line)
            CreateLabel("", 20, y, labelWidth);
            txtDescription = new TextBox
            {
                Location = new Point(140, y),
                Size = new Size(fieldWidth * 2 + gap + labelWidth, 80),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            pnlMain.Controls.Add(txtDescription);
            y += 100;

            // Buttons
            btnSave = new Button
            {
                Text = "Save",
                Location = new Point(140, y),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(260, y),
                Size = new Size(100, 35),
                DialogResult = DialogResult.Cancel
            };

            pnlMain.Controls.Add(btnSave);
            pnlMain.Controls.Add(btnCancel);
            this.Controls.Add(pnlMain);
        }

        private Label CreateLabel(string text, int x, int y, int width)
        {
            var lbl = new Label
            {
                Text = text,
                Location = new Point(x, y + 5),
                Size = new Size(width, 23),
                TextAlign = ContentAlignment.MiddleRight
            };
            pnlMain.Controls.Add(lbl);
            return lbl;
        }

        private TextBox CreateTextBox(int x, int y, int width)
        {
            var txt = new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(width, 23)
            };
            pnlMain.Controls.Add(txt);
            return txt;
        }

        private ComboBox CreateComboBox(int x, int y, int width)
        {
            var cmb = new ComboBox
            {
                Location = new Point(x, y),
                Size = new Size(width, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            pnlMain.Controls.Add(cmb);
            return cmb;
        }

        private NumericUpDown CreateNumericUpDown(int x, int y, int width, int decimalPlaces)
        {
            var num = new NumericUpDown
            {
                Location = new Point(x, y),
                Size = new Size(width, 23),
                DecimalPlaces = decimalPlaces,
                Minimum = 0,
                Maximum = 999999999,
                ThousandsSeparator = true
            };
            pnlMain.Controls.Add(num);
            return num;
        }

        private void LoadComboData()
        {
            try
            {
                // Categories
                foreach (var cat in _categoryRepository.GetAll())
                {
                    if (cat.Status == "Active")
                        cmbCategory.Items.Add(new KeyValuePair<int, string>(cat.CategoryID, cat.CategoryName));
                }
                if (cmbCategory.Items.Count > 0) cmbCategory.SelectedIndex = 0;

                // Units
                foreach (var unit in _unitRepository.GetAll())
                {
                    cmbUnit.Items.Add(new KeyValuePair<int, string>(unit.UnitID, unit.UnitName));
                }
                if (cmbUnit.Items.Count > 0) cmbUnit.SelectedIndex = 0;

                // Suppliers
                cmbSupplier.Items.Add(new KeyValuePair<int, string>(0, "-- None --"));
                foreach (var sup in _supplierRepository.GetAll())
                {
                    if (sup.Status == "Active")
                        cmbSupplier.Items.Add(new KeyValuePair<int, string>(sup.SupplierID, sup.SupplierName));
                }
                cmbSupplier.SelectedIndex = 0;

                // Locations
                cmbLocation.Items.Add(new KeyValuePair<int, string>(0, "-- None --"));
                foreach (var loc in _locationRepository.GetAll())
                {
                    cmbLocation.Items.Add(new KeyValuePair<int, string>(loc.LocationID, loc.LocationName));
                }
                cmbLocation.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading combo data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadProductData()
        {
            txtProductCode.Text = _existingProduct.ProductCode;
            txtBarcode.Text = _existingProduct.Barcode ?? "";
            txtProductName.Text = _existingProduct.ProductName;
            txtDescription.Text = _existingProduct.Description ?? "";
            numPurchasePrice.Value = _existingProduct.PurchasePrice;
            numSalePrice.Value = _existingProduct.SalePrice;
            numMinStock.Value = _existingProduct.MinimumStock ?? 0;
            numReorderLevel.Value = _existingProduct.ReorderLevel ?? 0;
            cmbStatus.SelectedItem = _existingProduct.Status;

            // Set combobox selections
            SetComboBoxValue(cmbCategory, _existingProduct.CategoryID);
            SetComboBoxValue(cmbUnit, _existingProduct.UnitID);
            SetComboBoxValue(cmbSupplier, _existingProduct.SupplierID ?? 0);
            SetComboBoxValue(cmbLocation, _existingProduct.LocationID ?? 0);
        }

        private void SetComboBoxValue(ComboBox cmb, int value)
        {
            for (int i = 0; i < cmb.Items.Count; i++)
            {
                var item = (KeyValuePair<int, string>)cmb.Items[i];
                if (item.Key == value)
                {
                    cmb.SelectedIndex = i;
                    return;
                }
            }
        }

        private void GenerateProductCode()
        {
            txtProductCode.Text = "PRD-" + DateTime.Now.ToString("yyyyMMddHHmmss");
        }

        private void BtnGenerateBarcode_Click(object sender, EventArgs e)
        {
            // Generate EAN-13 or CODE128 barcode
            txtBarcode.Text = "890" + new Random().Next(1000000000, 9999999999).ToString();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                var product = new Product
                {
                    ProductCode = txtProductCode.Text.Trim(),
                    Barcode = string.IsNullOrEmpty(txtBarcode.Text) ? null : txtBarcode.Text.Trim(),
                    ProductName = txtProductName.Text.Trim(),
                    Description = string.IsNullOrEmpty(txtDescription.Text) ? null : txtDescription.Text.Trim(),
                    CategoryID = ((KeyValuePair<int, string>)cmbCategory.SelectedItem).Key,
                    UnitID = ((KeyValuePair<int, string>)cmbUnit.SelectedItem).Key,
                    SupplierID = ((KeyValuePair<int, string>)cmbSupplier.SelectedItem).Key > 0 
                        ? ((KeyValuePair<int, string>)cmbSupplier.SelectedItem).Key 
                        : (int?)null,
                    LocationID = ((KeyValuePair<int, string>)cmbLocation.SelectedItem).Key > 0 
                        ? ((KeyValuePair<int, string>)cmbLocation.SelectedItem).Key 
                        : (int?)null,
                    PurchasePrice = numPurchasePrice.Value,
                    SalePrice = numSalePrice.Value,
                    MinimumStock = numMinStock.Value > 0 ? numMinStock.Value : (decimal?)null,
                    ReorderLevel = numReorderLevel.Value > 0 ? numReorderLevel.Value : (decimal?)null,
                    Status = cmbStatus.SelectedItem.ToString(),
                    CreatedDate = _existingProduct?.CreatedDate ?? DateTime.Now
                };

                if (_existingProduct != null)
                {
                    product.ProductID = _existingProduct.ProductID;
                    _productRepository.Update(product);
                    MessageBox.Show("Product updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    _productRepository.Add(product);
                    MessageBox.Show("Product created successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving product: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(txtProductCode.Text))
            {
                MessageBox.Show("Please enter product code.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtProductCode.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtProductName.Text))
            {
                MessageBox.Show("Please enter product name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtProductName.Focus();
                return false;
            }

            if (cmbCategory.SelectedIndex < 0 || ((KeyValuePair<int, string>)cmbCategory.SelectedItem).Key == 0)
            {
                MessageBox.Show("Please select a category.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbCategory.Focus();
                return false;
            }

            if (cmbUnit.SelectedIndex < 0 || ((KeyValuePair<int, string>)cmbUnit.SelectedItem).Key == 0)
            {
                MessageBox.Show("Please select a unit.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbUnit.Focus();
                return false;
            }

            if (numPurchasePrice.Value <= 0)
            {
                MessageBox.Show("Please enter a valid cost price.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numPurchasePrice.Focus();
                return false;
            }

            if (numSalePrice.Value <= 0)
            {
                MessageBox.Show("Please enter a valid sale price.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numSalePrice.Focus();
                return false;
            }

            return true;
        }
    }
}
