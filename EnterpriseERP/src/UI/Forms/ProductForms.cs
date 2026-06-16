using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using EnterpriseInventory.Core.Domain;
using EnterpriseInventory.Core.Interfaces;
using EnterpriseInventory.Application.Services;

namespace EnterpriseInventory.UI.Forms
{
    public partial class ProductListForm : Form
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly ISupplierRepository _supplierRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly IFeatureRepository _featureRepository;
        
        private DataGridView dgvProducts;
        private TextBox txtSearch;
        private ComboBox cmbCategoryFilter;
        private Button btnAdd, btnEdit, btnDelete, btnRefresh, btnImport, btnExport;
        private Panel pnlTop, pnlButtons;
        private int _currentBranchId;
        private User _currentUser;

        public ProductListForm(int branchId, User currentUser)
        {
            InitializeComponent();
            _currentBranchId = branchId;
            _currentUser = currentUser;
            
            // Initialize repositories via DI in production
            _productRepository = new ProductRepository(DbConnectionFactory.CreateConnection());
            _categoryRepository = new CategoryRepository(DbConnectionFactory.CreateConnection());
            _unitRepository = new UnitRepository(DbConnectionFactory.CreateConnection());
            _supplierRepository = new SupplierRepository(DbConnectionFactory.CreateConnection());
            _locationRepository = new LocationRepository(DbConnectionFactory.CreateConnection());
            _featureRepository = new FeatureRepository(DbConnectionFactory.CreateConnection());
            
            LoadProducts();
            LoadCategories();
        }

        private void InitializeComponent()
        {
            this.Text = "Product Management";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;

            // Top Panel
            pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(245, 245, 245)
            };

            var lblTitle = new Label
            {
                Text = "Product Master",
                Font = new Font("Arial", 16, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true,
                Parent = pnlTop
            };

            var lblSearch = new Label
            {
                Text = "Search:",
                Location = new Point(300, 25),
                AutoSize = true,
                Parent = pnlTop
            };

            txtSearch = new TextBox
            {
                Location = new Point(360, 23),
                Size = new Size(250, 23),
                Parent = pnlTop
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;

            var lblCategory = new Label
            {
                Text = "Category:",
                Location = new Point(650, 25),
                AutoSize = true,
                Parent = pnlTop
            };

            cmbCategoryFilter = new ComboBox
            {
                Location = new Point(720, 22),
                Size = new Size(200, 23),
                Parent = pnlTop,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbCategoryFilter.SelectedIndexChanged += CmbCategoryFilter_SelectedIndexChanged;

            // Buttons Panel
            pnlButtons = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.White
            };

            btnAdd = CreateButton("Add New", Color.FromArgb(0, 120, 215), Color.White);
            btnAdd.Location = new Point(20, 10);
            btnAdd.Click += BtnAdd_Click;

            btnEdit = CreateButton("Edit", Color.FromArgb(255, 193, 7), Color.Black);
            btnEdit.Location = new Point(130, 10);
            btnEdit.Click += BtnEdit_Click;

            btnDelete = CreateButton("Delete", Color.FromArgb(201, 48, 48), Color.White);
            btnDelete.Location = new Point(220, 10);
            btnDelete.Click += BtnDelete_Click;

            btnRefresh = CreateButton("Refresh", Color.Gray, Color.White);
            btnRefresh.Location = new Point(330, 10);
            btnRefresh.Click += (s, e) => LoadProducts();

            btnImport = CreateButton("Import", Color.FromArgb(16, 124, 16), Color.White);
            btnImport.Location = new Point(440, 10);
            btnImport.Click += BtnImport_Click;

            btnExport = CreateButton("Export", Color.FromArgb(0, 102, 204), Color.White);
            btnExport.Location = new Point(540, 10);
            btnExport.Click += BtnExport_Click;

            pnlButtons.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnRefresh, btnImport, btnExport });

            // DataGridView
            dgvProducts = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                BackgroundColor = Color.White,
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(250, 250, 250) }
            };

            SetupProductGridColumns();

            dgvProducts.CellDoubleClick += DgvProducts_CellDoubleClick;
            dgvProducts.SelectionChanged += DgvProducts_SelectionChanged;

            this.Controls.Add(dgvProducts);
            this.Controls.Add(pnlButtons);
            this.Controls.Add(pnlTop);
        }

        private Button CreateButton(string text, Color backColor, Color foreColor)
        {
            return new Button
            {
                Text = text,
                Size = new Size(100, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = backColor,
                ForeColor = foreColor,
                Cursor = Cursors.Hand
            };
        }

        private void SetupProductGridColumns()
        {
            dgvProducts.Columns.Clear();
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "ProductID", 
                HeaderText = "ID", 
                DataPropertyName = "ProductID",
                Width = 60,
                Visible = false
            });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "ProductCode", 
                HeaderText = "Code", 
                DataPropertyName = "ProductCode",
                Width = 100
            });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "Barcode", 
                HeaderText = "Barcode", 
                DataPropertyName = "Barcode",
                Width = 120
            });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "ProductName", 
                HeaderText = "Product Name", 
                DataPropertyName = "ProductName",
                Width = 250
            });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "CategoryName", 
                HeaderText = "Category", 
                DataPropertyName = "CategoryName",
                Width = 120
            });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "UnitName", 
                HeaderText = "Unit", 
                DataPropertyName = "UnitName",
                Width = 80
            });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "PurchasePrice", 
                HeaderText = "Cost Price", 
                DataPropertyName = "PurchasePrice",
                Width = 90,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
            });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "SalePrice", 
                HeaderText = "Sale Price", 
                DataPropertyName = "SalePrice",
                Width = 90,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
            });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "CurrentStock", 
                HeaderText = "Stock", 
                DataPropertyName = "CurrentStock",
                Width = 80,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight }
            });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "MinimumStock", 
                HeaderText = "Min Stock", 
                DataPropertyName = "MinimumStock",
                Width = 80,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight }
            });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "Status", 
                HeaderText = "Status", 
                DataPropertyName = "Status",
                Width = 90
            });
        }

        private void LoadProducts()
        {
            try
            {
                var products = _productRepository.GetAllWithDetails(_currentBranchId);
                var dt = new DataTable();
                dt.Columns.Add("ProductID", typeof(int));
                dt.Columns.Add("ProductCode", typeof(string));
                dt.Columns.Add("Barcode", typeof(string));
                dt.Columns.Add("ProductName", typeof(string));
                dt.Columns.Add("CategoryName", typeof(string));
                dt.Columns.Add("UnitName", typeof(string));
                dt.Columns.Add("PurchasePrice", typeof(decimal));
                dt.Columns.Add("SalePrice", typeof(decimal));
                dt.Columns.Add("CurrentStock", typeof(decimal));
                dt.Columns.Add("MinimumStock", typeof(decimal));
                dt.Columns.Add("Status", typeof(string));

                foreach (var p in products)
                {
                    dt.Rows.Add(
                        p.ProductID,
                        p.ProductCode,
                        p.Barcode ?? "",
                        p.ProductName,
                        p.CategoryName ?? "N/A",
                        p.UnitName ?? "N/A",
                        p.PurchasePrice,
                        p.SalePrice,
                        p.CurrentStock ?? 0,
                        p.MinimumStock ?? 0,
                        p.Status
                    );
                }

                dgvProducts.DataSource = dt;
                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading products: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadCategories()
        {
            try
            {
                var categories = _categoryRepository.GetAll();
                cmbCategoryFilter.Items.Clear();
                cmbCategoryFilter.Items.Add(new KeyValuePair<int, string>(0, "All Categories"));
                
                foreach (var cat in categories)
                {
                    if (cat.Status == "Active")
                        cmbCategoryFilter.Items.Add(new KeyValuePair<int, string>(cat.CategoryID, cat.CategoryName));
                }
                
                cmbCategoryFilter.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading categories: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            FilterProducts();
        }

        private void CmbCategoryFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterProducts();
        }

        private void FilterProducts()
        {
            string searchText = txtSearch.Text.Trim().ToLower();
            int categoryId = ((KeyValuePair<int, string>)cmbCategoryFilter.SelectedItem).Key;

            var dv = dgvProducts.DataSource as DataTable;
            if (dv == null) return;

            string filter = "";

            if (!string.IsNullOrEmpty(searchText))
            {
                filter += $"ProductName LIKE '%{searchText}%' OR ProductCode LIKE '%{searchText}%' OR Barcode LIKE '%{searchText}%'";
            }

            if (categoryId > 0)
            {
                var selectedCat = ((KeyValuePair<int, string>)cmbCategoryFilter.SelectedItem).Value;
                if (!string.IsNullOrEmpty(filter)) filter += " AND ";
                filter += $"CategoryName = '{selectedCat}'";
            }

            if (!string.IsNullOrEmpty(filter))
            {
                dv.DefaultView.RowFilter = filter;
            }
            else
            {
                dv.DefaultView.RowFilter = "";
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            OpenProductForm(null);
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count > 0)
            {
                int productId = Convert.ToInt32(dgvProducts.SelectedRows[0].Cells["ProductID"].Value);
                var product = _productRepository.GetById(productId);
                OpenProductForm(product);
            }
            else
            {
                MessageBox.Show("Please select a product to edit.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a product to delete.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int productId = Convert.ToInt32(dgvProducts.SelectedRows[0].Cells["ProductID"].Value);
            string productName = dgvProducts.SelectedRows[0].Cells["ProductName"].Value.ToString();

            var result = MessageBox.Show(
                $"Are you sure you want to delete '{productName}'?\n\nThis will also delete all associated batches and transactions.",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    _productRepository.Delete(productId);
                    MessageBox.Show("Product deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadProducts();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting product: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void DgvProducts_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                BtnEdit_Click(sender, e);
            }
        }

        private void DgvProducts_SelectionChanged(object sender, EventArgs e)
        {
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            bool hasSelection = dgvProducts.SelectedRows.Count > 0;
            btnEdit.Enabled = hasSelection;
            btnDelete.Enabled = hasSelection;
        }

        private void OpenProductForm(Product product)
        {
            var form = new ProductDetailForm(_currentBranchId, _currentUser, product);
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadProducts();
            }
        }

        private void BtnImport_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx;*.xls|CSV Files|*.csv|All Files|*.*",
                Title = "Import Products"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    int importedCount = _productRepository.ImportFromExcel(ofd.FileName, _currentBranchId);
                    MessageBox.Show($"{importedCount} products imported successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadProducts();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error importing products: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx|CSV Files|*.csv",
                Title = "Export Products"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _productRepository.ExportToExcel(sfd.FileName, dgvProducts.DataSource as DataTable);
                    MessageBox.Show("Products exported successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting products: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
