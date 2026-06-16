using System;
using System.Data;
using System.Windows.Forms;
using EnterpriseERP.Core.Domain;
using EnterpriseERP.Core.Interfaces;
using EnterpriseERP.Application.Services;

namespace EnterpriseERP.UI.Forms.Transactions
{
    public partial class OpeningStockForm : Form
    {
        private readonly IInventoryService _inventoryService;
        private readonly IProductRepository _productRepository;
        private readonly int _userId;

        private DataGridView dgvOpeningStock;
        private Button btnSave;
        private Button btnClose;
        private ComboBox cmbProduct;
        private TextBox txtBatchNumber;
        private DateTimePicker dtpExpiryDate;
        private NumericUpDown nudQuantity;
        private NumericUpDown nudCostPrice;
        private DateTimePicker dtpDate;
        private Label lblProduct, lblBatch, lblExpiry, lblQty, lblCost, lblDate;

        public OpeningStockForm(IInventoryService inventoryService, IProductRepository productRepository, int userId)
        {
            _inventoryService = inventoryService;
            _productRepository = productRepository;
            _userId = userId;

            InitializeComponent();
            LoadProducts();
        }

        private void InitializeComponent()
        {
            this.Text = "Opening Stock Entry";
            this.Size = new System.Drawing.Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Create controls
            lblProduct = new Label { Text = "Product:", Location = new System.Drawing.Point(20, 20), AutoSize = true };
            cmbProduct = new ComboBox { Location = new System.Drawing.Point(120, 17), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            
            lblBatch = new Label { Text = "Batch Number:", Location = new System.Drawing.Point(20, 50), AutoSize = true };
            txtBatchNumber = new TextBox { Location = new System.Drawing.Point(120, 47), Width = 200 };
            
            lblExpiry = new Label { Text = "Expiry Date:", Location = new System.Drawing.Point(350, 50), AutoSize = true };
            dtpExpiryDate = new DateTimePicker { Location = new System.Drawing.Point(430, 47), Width = 200 };
            
            lblQty = new Label { Text = "Quantity:", Location = new System.Drawing.Point(20, 80), AutoSize = true };
            nudQuantity = new NumericUpDown { Location = new System.Drawing.Point(120, 77), Width = 150, Minimum = 1, Maximum = 999999 };
            
            lblCost = new Label { Text = "Cost Price:", Location = new System.Drawing.Point(300, 80), AutoSize = true };
            nudCostPrice = new NumericUpDown { Location = new System.Drawing.Point(380, 77), Width = 150, Minimum = 0, Maximum = 999999, DecimalPlaces = 2 };
            
            lblDate = new Label { Text = "Date:", Location = new System.Drawing.Point(20, 110), AutoSize = true };
            dtpDate = new DateTimePicker { Location = new System.Drawing.Point(120, 107), Width = 200, Value = DateTime.Today };

            dgvOpeningStock = new DataGridView 
            { 
                Location = new System.Drawing.Point(20, 150), 
                Size = new System.Drawing.Size(850, 350),
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true
            };
            
            // Add columns
            dgvOpeningStock.Columns.Add(new DataGridViewTextBoxColumn { Name = "Product", HeaderText = "Product", Width = 200 });
            dgvOpeningStock.Columns.Add(new DataGridViewTextBoxColumn { Name = "Batch", HeaderText = "Batch No", Width = 100 });
            dgvOpeningStock.Columns.Add(new DataGridViewTextBoxColumn { Name = "Expiry", HeaderText = "Expiry Date", Width = 100 });
            dgvOpeningStock.Columns.Add(new DataGridViewTextBoxColumn { Name = "Quantity", HeaderText = "Qty", Width = 80 });
            dgvOpeningStock.Columns.Add(new DataGridViewTextBoxColumn { Name = "Cost", HeaderText = "Cost Price", Width = 100 });
            dgvOpeningStock.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total", HeaderText = "Total", Width = 100 });

            btnSave = new Button { Text = "Save Opening Stock", Location = new System.Drawing.Point(20, 520), Width = 150, Height = 35, BackColor = System.Drawing.Color.Green, ForeColor = System.Drawing.Color.White };
            btnClose = new Button { Text = "Close", Location = new System.Drawing.Point(720, 520), Width = 150, Height = 35 };

            btnSave.Click += BtnSave_Click;
            btnClose.Click += (s, e) => this.Close();

            // Add controls to form
            this.Controls.AddRange(new Control[] { 
                lblProduct, cmbProduct, lblBatch, txtBatchNumber, lblExpiry, dtpExpiryDate,
                lblQty, nudQuantity, lblCost, nudCostPrice, lblDate, dtpDate,
                dgvOpeningStock, btnSave, btnClose 
            });
        }

        private void LoadProducts()
        {
            try
            {
                var products = _productRepository.GetAllActive();
                cmbProduct.DataSource = products;
                cmbProduct.DisplayMember = "ProductName";
                cmbProduct.ValueMember = "ProductID";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading products: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbProduct.SelectedValue == null)
                {
                    MessageBox.Show("Please select a product.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtBatchNumber.Text))
                {
                    MessageBox.Show("Please enter batch number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int productId = (int)cmbProduct.SelectedValue;
                string batchNumber = txtBatchNumber.Text.Trim();
                DateTime expiryDate = dtpExpiryDate.Value;
                decimal quantity = nudQuantity.Value;
                decimal costPrice = nudCostPrice.Value;
                DateTime date = dtpDate.Value;

                // Create opening stock entry
                var openingStock = new OpeningStock
                {
                    ProductID = productId,
                    BatchNumber = batchNumber,
                    ExpiryDate = expiryDate,
                    Quantity = (int)quantity,
                    CostPrice = costPrice,
                    Date = date
                };

                // Save through service (creates inventory transaction automatically)
                bool success = _inventoryService.SaveOpeningStock(openingStock, _userId);

                if (success)
                {
                    MessageBox.Show("Opening stock saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // Clear fields
                    txtBatchNumber.Clear();
                    nudQuantity.Value = 1;
                    nudCostPrice.Value = 0;
                    cmbProduct.Focus();
                }
                else
                {
                    MessageBox.Show("Failed to save opening stock.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
