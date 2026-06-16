using System;
using System.Data;
using System.Windows.Forms;
using EnterpriseERP.Core.Domain;
using EnterpriseERP.Core.Interfaces;
using EnterpriseERP.Application.Services;

namespace EnterpriseERP.UI.Forms.Transactions
{
    public partial class SalesReturnForm : Form
    {
        private readonly IInventoryService _inventoryService;
        private readonly ISaleRepository _saleRepository;
        private readonly IProductRepository _productRepository;
        private readonly IAuditService _auditService;
        private readonly int _userId;

        private ComboBox cmbCustomer;
        private ComboBox cmbSaleInvoice;
        private ComboBox cmbProduct;
        private ComboBox cmbBatch;
        private NumericUpDown nudQuantity;
        private TextBox txtReason;
        private DateTimePicker dtpDate;
        private Button btnSave;
        private Button btnClose;
        private Label lblCustomer, lblInvoice, lblProduct, lblBatch, lblQty, lblReason, lblDate;

        public SalesReturnForm(IInventoryService inventoryService, ISaleRepository saleRepository,
            IProductRepository productRepository, IAuditService auditService, int userId)
        {
            _inventoryService = inventoryService;
            _saleRepository = saleRepository;
            _productRepository = productRepository;
            _auditService = auditService;
            _userId = userId;

            InitializeComponent();
            LoadCustomers();
        }

        private void InitializeComponent()
        {
            this.Text = "Sales Return";
            this.Size = new System.Drawing.Size(750, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            lblCustomer = new Label { Text = "Customer:", Location = new System.Drawing.Point(20, 20), AutoSize = true };
            cmbCustomer = new ComboBox { Location = new System.Drawing.Point(150, 17), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            
            lblInvoice = new Label { Text = "Sale Invoice:", Location = new System.Drawing.Point(20, 60), AutoSize = true };
            cmbSaleInvoice = new ComboBox { Location = new System.Drawing.Point(150, 57), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
            
            lblProduct = new Label { Text = "Product:", Location = new System.Drawing.Point(20, 100), AutoSize = true };
            cmbProduct = new ComboBox { Location = new System.Drawing.Point(150, 97), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
            
            lblBatch = new Label { Text = "Batch:", Location = new System.Drawing.Point(20, 140), AutoSize = true };
            cmbBatch = new ComboBox { Location = new System.Drawing.Point(150, 137), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
            
            lblQty = new Label { Text = "Return Quantity:", Location = new System.Drawing.Point(20, 180), AutoSize = true };
            nudQuantity = new NumericUpDown { Location = new System.Drawing.Point(150, 177), Width = 150, Minimum = 1, Maximum = 999999 };
            
            lblReason = new Label { Text = "Return Reason:", Location = new System.Drawing.Point(20, 220), AutoSize = true };
            txtReason = new TextBox { Location = new System.Drawing.Point(150, 217), Width = 400, Height = 60, Multiline = true };
            
            lblDate = new Label { Text = "Return Date:", Location = new System.Drawing.Point(20, 300), AutoSize = true };
            dtpDate = new DateTimePicker { Location = new System.Drawing.Point(150, 297), Width = 200, Value = DateTime.Today };

            btnSave = new Button { Text = "Save Return", Location = new System.Drawing.Point(20, 350), Width = 150, Height = 35, BackColor = System.Drawing.Color.Orange, ForeColor = System.Drawing.Color.White };
            btnClose = new Button { Text = "Close", Location = new System.Drawing.Point(570, 350), Width = 150, Height = 35 };

            btnSave.Click += BtnSave_Click;
            btnClose.Click += (s, e) => this.Close();
            cmbCustomer.SelectedIndexChanged += CmbCustomer_SelectedIndexChanged;
            cmbSaleInvoice.SelectedIndexChanged += CmbSaleInvoice_SelectedIndexChanged;
            cmbProduct.SelectedIndexChanged += CmbProduct_SelectedIndexChanged;

            this.Controls.AddRange(new Control[] { 
                lblCustomer, cmbCustomer, lblInvoice, cmbSaleInvoice,
                lblProduct, cmbProduct, lblBatch, cmbBatch, lblQty, nudQuantity,
                lblReason, txtReason, lblDate, dtpDate,
                btnSave, btnClose 
            });
        }

        private void LoadCustomers()
        {
            try
            {
                var customers = _productRepository.GetAllCustomers();
                cmbCustomer.DataSource = customers;
                cmbCustomer.DisplayMember = "CustomerName";
                cmbCustomer.ValueMember = "CustomerID";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading customers: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CmbCustomer_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbCustomer.SelectedValue != null)
            {
                LoadSaleInvoices((int)cmbCustomer.SelectedValue);
            }
        }

        private void LoadSaleInvoices(int customerId)
        {
            try
            {
                var invoices = _saleRepository.GetSaleInvoicesByCustomer(customerId);
                cmbSaleInvoice.DataSource = invoices;
                cmbSaleInvoice.DisplayMember = "InvoiceNumber";
                cmbSaleInvoice.ValueMember = "SaleID";
                cmbSaleInvoice.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading invoices: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CmbSaleInvoice_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbSaleInvoice.SelectedValue != null)
            {
                LoadProductsFromSale((int)cmbSaleInvoice.SelectedValue);
            }
        }

        private void LoadProductsFromSale(int saleId)
        {
            try
            {
                var products = _saleRepository.GetProductsFromSale(saleId);
                cmbProduct.DataSource = products;
                cmbProduct.DisplayMember = "ProductName";
                cmbProduct.ValueMember = "ProductID";
                cmbProduct.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading products: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CmbProduct_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbProduct.SelectedValue != null)
            {
                LoadBatchesFromSale((int)cmbSaleInvoice.SelectedValue, (int)cmbProduct.SelectedValue);
            }
        }

        private void LoadBatchesFromSale(int saleId, int productId)
        {
            try
            {
                var batches = _saleRepository.GetBatchesFromSale(saleId, productId);
                cmbBatch.DataSource = batches;
                cmbBatch.DisplayMember = "BatchNumber";
                cmbBatch.ValueMember = "BatchID";
                cmbBatch.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading batches: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbCustomer.SelectedValue == null)
                {
                    MessageBox.Show("Please select a customer.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (cmbSaleInvoice.SelectedValue == null)
                {
                    MessageBox.Show("Please select a sale invoice.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (cmbProduct.SelectedValue == null)
                {
                    MessageBox.Show("Please select a product.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (cmbBatch.SelectedValue == null)
                {
                    MessageBox.Show("Please select a batch.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int customerId = (int)cmbCustomer.SelectedValue;
                int saleId = (int)cmbSaleInvoice.SelectedValue;
                int productId = (int)cmbProduct.SelectedValue;
                int batchId = (int)cmbBatch.SelectedValue;
                int quantity = (int)nudQuantity.Value;
                string reason = txtReason.Text.Trim();
                DateTime date = dtpDate.Value;

                // Create sales return
                var salesReturn = new SalesReturn
                {
                    SaleID = saleId,
                    CustomerID = customerId,
                    ProductID = productId,
                    BatchID = batchId,
                    Quantity = quantity,
                    Reason = reason,
                    ReturnDate = date
                };

                bool success = _inventoryService.SaveSalesReturn(salesReturn, _userId);

                if (success)
                {
                    MessageBox.Show("Sales return saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    _auditService.LogAction(_userId, "Sales Return", "SalesReturns", salesReturn.ProductID.ToString(),
                        $"Returned {quantity} units from Customer ID {customerId}. Reason: {reason}");
                    
                    // Clear fields
                    nudQuantity.Value = 1;
                    txtReason.Clear();
                    cmbCustomer.Focus();
                }
                else
                {
                    MessageBox.Show("Failed to save sales return.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
