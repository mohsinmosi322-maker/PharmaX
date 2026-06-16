using System;
using System.Data;
using System.Windows.Forms;
using EnterpriseERP.Core.Domain;
using EnterpriseERP.Core.Interfaces;
using EnterpriseERP.Application.Services;

namespace EnterpriseERP.UI.Forms.Transactions
{
    public partial class StockAdjustmentForm : Form
    {
        private readonly IInventoryService _inventoryService;
        private readonly IProductRepository _productRepository;
        private readonly IAuditService _auditService;
        private readonly int _userId;

        private ComboBox cmbProduct;
        private ComboBox cmbBatch;
        private NumericUpDown nudQuantity;
        private ComboBox cmbReason;
        private TextBox txtRemarks;
        private DateTimePicker dtpDate;
        private Button btnSave;
        private Button btnClose;
        private Label lblProduct, lblBatch, lblQty, lblReason, lblRemarks, lblDate;

        public StockAdjustmentForm(IInventoryService inventoryService, IProductRepository productRepository, 
            IAuditService auditService, int userId)
        {
            _inventoryService = inventoryService;
            _productRepository = productRepository;
            _auditService = auditService;
            _userId = userId;

            InitializeComponent();
            LoadProducts();
            LoadReasons();
        }

        private void InitializeComponent()
        {
            this.Text = "Stock Adjustment";
            this.Size = new System.Drawing.Size(700, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            lblProduct = new Label { Text = "Product:", Location = new System.Drawing.Point(20, 20), AutoSize = true };
            cmbProduct = new ComboBox { Location = new System.Drawing.Point(150, 17), Width = 400, DropDownStyle = ComboBoxStyle.DropDownList };
            
            lblBatch = new Label { Text = "Batch:", Location = new System.Drawing.Point(20, 60), AutoSize = true };
            cmbBatch = new ComboBox { Location = new System.Drawing.Point(150, 57), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            
            lblQty = new Label { Text = "Adjustment Qty:", Location = new System.Drawing.Point(20, 100), AutoSize = true };
            nudQuantity = new NumericUpDown { Location = new System.Drawing.Point(150, 97), Width = 150, Minimum = -999999, Maximum = 999999 };
            lblQtyInfo = new Label { Text = "(Negative for decrease, Positive for increase)", Location = new System.Drawing.Point(310, 100), AutoSize = true, ForeColor = System.Drawing.Color.Gray };
            
            lblReason = new Label { Text = "Reason:", Location = new System.Drawing.Point(20, 140), AutoSize = true };
            cmbReason = new ComboBox { Location = new System.Drawing.Point(150, 137), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            
            lblRemarks = new Label { Text = "Remarks:", Location = new System.Drawing.Point(20, 180), AutoSize = true };
            txtRemarks = new TextBox { Location = new System.Drawing.Point(150, 177), Width = 400, Height = 80, Multiline = true };
            
            lblDate = new Label { Text = "Date:", Location = new System.Drawing.Point(20, 280), AutoSize = true };
            dtpDate = new DateTimePicker { Location = new System.Drawing.Point(150, 277), Width = 200, Value = DateTime.Today };

            btnSave = new Button { Text = "Save Adjustment", Location = new System.Drawing.Point(20, 330), Width = 150, Height = 35, BackColor = System.Drawing.Color.Green, ForeColor = System.Drawing.Color.White };
            btnClose = new Button { Text = "Close", Location = new System.Drawing.Point(520, 330), Width = 150, Height = 35 };

            btnSave.Click += BtnSave_Click;
            btnClose.Click += (s, e) => this.Close();
            cmbProduct.SelectedIndexChanged += CmbProduct_SelectedIndexChanged;

            this.Controls.AddRange(new Control[] { 
                lblProduct, cmbProduct, lblBatch, cmbBatch, lblQty, nudQuantity, lblQtyInfo,
                lblReason, cmbReason, lblRemarks, txtRemarks, lblDate, dtpDate,
                btnSave, btnClose 
            });
        }

        private Label lblQtyInfo;

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

        private void LoadReasons()
        {
            var reasons = new[]
            {
                "Physical Count Correction",
                "Damage",
                "Expiry",
                "Lost/Theft",
                "Quality Issue",
                "Other"
            };

            cmbReason.Items.AddRange(reasons);
            cmbReason.SelectedIndex = 0;
        }

        private void CmbProduct_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbProduct.SelectedValue != null)
            {
                LoadBatches((int)cmbProduct.SelectedValue);
            }
        }

        private void LoadBatches(int productId)
        {
            try
            {
                var batches = _productRepository.GetProductBatches(productId);
                cmbBatch.DataSource = batches;
                cmbBatch.DisplayMember = "BatchNumber";
                cmbBatch.ValueMember = "BatchID";
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

                if (nudQuantity.Value == 0)
                {
                    MessageBox.Show("Adjustment quantity cannot be zero.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int productId = (int)cmbProduct.SelectedValue;
                int batchId = (int)cmbBatch.SelectedValue;
                decimal adjustmentQty = nudQuantity.Value;
                string reason = cmbReason.SelectedItem?.ToString() ?? "";
                string remarks = txtRemarks.Text.Trim();
                DateTime date = dtpDate.Value;

                // Create adjustment
                var adjustment = new StockAdjustment
                {
                    ProductID = productId,
                    BatchID = batchId,
                    Quantity = (int)adjustmentQty,
                    Reason = reason,
                    Remarks = remarks,
                    Date = date
                };

                bool success = _inventoryService.SaveStockAdjustment(adjustment, _userId);

                if (success)
                {
                    MessageBox.Show("Stock adjustment saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    _auditService.LogAction(_userId, "Stock Adjustment", "StockAdjustments", adjustment.ProductID.ToString(),
                        $"Adjusted {adjustmentQty} units for Product ID {productId}, Batch ID {batchId}. Reason: {reason}");
                    
                    // Clear fields
                    nudQuantity.Value = 0;
                    txtRemarks.Clear();
                    cmbProduct.Focus();
                }
                else
                {
                    MessageBox.Show("Failed to save stock adjustment.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
