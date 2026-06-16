using System;
using System.Data;
using System.Windows.Forms;
using EnterpriseERP.Core.Domain;
using EnterpriseERP.Core.Interfaces;
using EnterpriseERP.Application.Services;

namespace EnterpriseERP.UI.Forms.Transactions
{
    public partial class PurchaseReturnForm : Form
    {
        private readonly IInventoryService _inventoryService;
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IProductRepository _productRepository;
        private readonly IAuditService _auditService;
        private readonly int _userId;

        private ComboBox cmbSupplier;
        private ComboBox cmbPurchaseInvoice;
        private ComboBox cmbProduct;
        private ComboBox cmbBatch;
        private NumericUpDown nudQuantity;
        private TextBox txtReason;
        private DateTimePicker dtpDate;
        private Button btnSave;
        private Button btnClose;
        private Label lblSupplier, lblInvoice, lblProduct, lblBatch, lblQty, lblReason, lblDate;

        public PurchaseReturnForm(IInventoryService inventoryService, IPurchaseRepository purchaseRepository,
            IProductRepository productRepository, IAuditService auditService, int userId)
        {
            _inventoryService = inventoryService;
            _purchaseRepository = purchaseRepository;
            _productRepository = productRepository;
            _auditService = auditService;
            _userId = userId;

            InitializeComponent();
            LoadSuppliers();
        }

        private void InitializeComponent()
        {
            this.Text = "Purchase Return";
            this.Size = new System.Drawing.Size(750, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            lblSupplier = new Label { Text = "Supplier:", Location = new System.Drawing.Point(20, 20), AutoSize = true };
            cmbSupplier = new ComboBox { Location = new System.Drawing.Point(150, 17), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            
            lblInvoice = new Label { Text = "Purchase Invoice:", Location = new System.Drawing.Point(20, 60), AutoSize = true };
            cmbPurchaseInvoice = new ComboBox { Location = new System.Drawing.Point(150, 57), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
            
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
            cmbSupplier.SelectedIndexChanged += CmbSupplier_SelectedIndexChanged;
            cmbPurchaseInvoice.SelectedIndexChanged += CmbPurchaseInvoice_SelectedIndexChanged;
            cmbProduct.SelectedIndexChanged += CmbProduct_SelectedIndexChanged;

            this.Controls.AddRange(new Control[] { 
                lblSupplier, cmbSupplier, lblInvoice, cmbPurchaseInvoice,
                lblProduct, cmbProduct, lblBatch, cmbBatch, lblQty, nudQuantity,
                lblReason, txtReason, lblDate, dtpDate,
                btnSave, btnClose 
            });
        }

        private void LoadSuppliers()
        {
            try
            {
                var suppliers = _productRepository.GetAllSuppliers();
                cmbSupplier.DataSource = suppliers;
                cmbSupplier.DisplayMember = "SupplierName";
                cmbSupplier.ValueMember = "SupplierID";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading suppliers: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CmbSupplier_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbSupplier.SelectedValue != null)
            {
                LoadPurchaseInvoices((int)cmbSupplier.SelectedValue);
            }
        }

        private void LoadPurchaseInvoices(int supplierId)
        {
            try
            {
                var invoices = _purchaseRepository.GetPurchaseInvoicesBySupplier(supplierId);
                cmbPurchaseInvoice.DataSource = invoices;
                cmbPurchaseInvoice.DisplayMember = "InvoiceNumber";
                cmbPurchaseInvoice.ValueMember = "PurchaseID";
                cmbPurchaseInvoice.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading invoices: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CmbPurchaseInvoice_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbPurchaseInvoice.SelectedValue != null)
            {
                LoadProductsFromInvoice((int)cmbPurchaseInvoice.SelectedValue);
            }
        }

        private void LoadProductsFromInvoice(int purchaseId)
        {
            try
            {
                var products = _purchaseRepository.GetProductsFromPurchase(purchaseId);
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
                LoadBatchesFromPurchase((int)cmbPurchaseInvoice.SelectedValue, (int)cmbProduct.SelectedValue);
            }
        }

        private void LoadBatchesFromPurchase(int purchaseId, int productId)
        {
            try
            {
                var batches = _purchaseRepository.GetBatchesFromPurchase(purchaseId, productId);
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
                if (cmbSupplier.SelectedValue == null)
                {
                    MessageBox.Show("Please select a supplier.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (cmbPurchaseInvoice.SelectedValue == null)
                {
                    MessageBox.Show("Please select a purchase invoice.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

                int supplierId = (int)cmbSupplier.SelectedValue;
                int purchaseId = (int)cmbPurchaseInvoice.SelectedValue;
                int productId = (int)cmbProduct.SelectedValue;
                int batchId = (int)cmbBatch.SelectedValue;
                int quantity = (int)nudQuantity.Value;
                string reason = txtReason.Text.Trim();
                DateTime date = dtpDate.Value;

                // Create purchase return
                var purchaseReturn = new PurchaseReturn
                {
                    PurchaseID = purchaseId,
                    SupplierID = supplierId,
                    ProductID = productId,
                    BatchID = batchId,
                    Quantity = quantity,
                    Reason = reason,
                    ReturnDate = date
                };

                bool success = _inventoryService.SavePurchaseReturn(purchaseReturn, _userId);

                if (success)
                {
                    MessageBox.Show("Purchase return saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    _auditService.LogAction(_userId, "Purchase Return", "PurchaseReturns", purchaseReturn.ProductID.ToString(),
                        $"Returned {quantity} units to Supplier ID {supplierId}. Reason: {reason}");
                    
                    // Clear fields
                    nudQuantity.Value = 1;
                    txtReason.Clear();
                    cmbSupplier.Focus();
                }
                else
                {
                    MessageBox.Show("Failed to save purchase return.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
