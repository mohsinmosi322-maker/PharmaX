using System;
using System.Data;
using System.Windows.Forms;
using EnterpriseERP.Core.Domain;
using EnterpriseERP.Core.Interfaces;
using EnterpriseERP.Application.Services;

namespace EnterpriseERP.UI.Forms.Transactions
{
    public partial class InventoryTransferForm : Form
    {
        private readonly IInventoryService _inventoryService;
        private readonly IProductRepository _productRepository;
        private readonly IAuditService _auditService;
        private readonly int _userId;

        private ComboBox cmbSourceBranch;
        private ComboBox cmbDestinationBranch;
        private ComboBox cmbProduct;
        private ComboBox cmbBatch;
        private NumericUpDown nudQuantity;
        private TextBox txtRemarks;
        private DateTimePicker dtpDate;
        private Button btnSave;
        private Button btnClose;
        private Label lblSource, lblDestination, lblProduct, lblBatch, lblQty, lblRemarks, lblDate;

        public InventoryTransferForm(IInventoryService inventoryService, IProductRepository productRepository,
            IAuditService auditService, int userId)
        {
            _inventoryService = inventoryService;
            _productRepository = productRepository;
            _auditService = auditService;
            _userId = userId;

            InitializeComponent();
            LoadBranches();
            LoadProducts();
        }

        private void InitializeComponent()
        {
            this.Text = "Inventory Transfer Between Branches";
            this.Size = new System.Drawing.Size(750, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            lblSource = new Label { Text = "Source Branch:", Location = new System.Drawing.Point(20, 20), AutoSize = true };
            cmbSourceBranch = new ComboBox { Location = new System.Drawing.Point(150, 17), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            
            lblDestination = new Label { Text = "Destination Branch:", Location = new System.Drawing.Point(20, 60), AutoSize = true };
            cmbDestinationBranch = new ComboBox { Location = new System.Drawing.Point(180, 57), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            
            lblProduct = new Label { Text = "Product:", Location = new System.Drawing.Point(20, 100), AutoSize = true };
            cmbProduct = new ComboBox { Location = new System.Drawing.Point(150, 97), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            
            lblBatch = new Label { Text = "Batch:", Location = new System.Drawing.Point(20, 140), AutoSize = true };
            cmbBatch = new ComboBox { Location = new System.Drawing.Point(150, 137), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            
            lblQty = new Label { Text = "Quantity:", Location = new System.Drawing.Point(20, 180), AutoSize = true };
            nudQuantity = new NumericUpDown { Location = new System.Drawing.Point(150, 177), Width = 150, Minimum = 1, Maximum = 999999 };
            
            lblRemarks = new Label { Text = "Remarks:", Location = new System.Drawing.Point(20, 220), AutoSize = true };
            txtRemarks = new TextBox { Location = new System.Drawing.Point(150, 217), Width = 400, Height = 60, Multiline = true };
            
            lblDate = new Label { Text = "Transfer Date:", Location = new System.Drawing.Point(20, 300), AutoSize = true };
            dtpDate = new DateTimePicker { Location = new System.Drawing.Point(150, 297), Width = 200, Value = DateTime.Today };

            btnSave = new Button { Text = "Save Transfer", Location = new System.Drawing.Point(20, 350), Width = 150, Height = 35, BackColor = System.Drawing.Color.Blue, ForeColor = System.Drawing.Color.White };
            btnClose = new Button { Text = "Close", Location = new System.Drawing.Point(570, 350), Width = 150, Height = 35 };

            btnSave.Click += BtnSave_Click;
            btnClose.Click += (s, e) => this.Close();
            cmbProduct.SelectedIndexChanged += CmbProduct_SelectedIndexChanged;

            this.Controls.AddRange(new Control[] { 
                lblSource, cmbSourceBranch, lblDestination, cmbDestinationBranch,
                lblProduct, cmbProduct, lblBatch, cmbBatch, lblQty, nudQuantity,
                lblRemarks, txtRemarks, lblDate, dtpDate,
                btnSave, btnClose 
            });
        }

        private void LoadBranches()
        {
            try
            {
                var branches = _inventoryService.GetAllBranches();
                cmbSourceBranch.DataSource = branches;
                cmbSourceBranch.DisplayMember = "BranchName";
                cmbSourceBranch.ValueMember = "BranchID";
                
                cmbDestinationBranch.DataSource = branches.Clone() as DataTable;
                cmbDestinationBranch.DisplayMember = "BranchName";
                cmbDestinationBranch.ValueMember = "BranchID";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading branches: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
                if (cmbSourceBranch.SelectedValue == null)
                {
                    MessageBox.Show("Please select source branch.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (cmbDestinationBranch.SelectedValue == null)
                {
                    MessageBox.Show("Please select destination branch.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (cmbSourceBranch.SelectedValue.Equals(cmbDestinationBranch.SelectedValue))
                {
                    MessageBox.Show("Source and destination branches must be different.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

                int sourceBranchId = (int)cmbSourceBranch.SelectedValue;
                int destBranchId = (int)cmbDestinationBranch.SelectedValue;
                int productId = (int)cmbProduct.SelectedValue;
                int batchId = (int)cmbBatch.SelectedValue;
                int quantity = (int)nudQuantity.Value;
                string remarks = txtRemarks.Text.Trim();
                DateTime date = dtpDate.Value;

                // Create transfer
                var transfer = new InventoryTransfer
                {
                    SourceBranchID = sourceBranchId,
                    DestinationBranchID = destBranchId,
                    ProductID = productId,
                    BatchID = batchId,
                    Quantity = quantity,
                    Remarks = remarks,
                    TransferDate = date
                };

                bool success = _inventoryService.SaveInventoryTransfer(transfer, _userId);

                if (success)
                {
                    MessageBox.Show("Inventory transfer saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    _auditService.LogAction(_userId, "Inventory Transfer", "InventoryTransfers", transfer.ProductID.ToString(),
                        $"Transferred {quantity} units from Branch {sourceBranchId} to Branch {destBranchId}");
                    
                    // Clear fields
                    nudQuantity.Value = 1;
                    txtRemarks.Clear();
                    cmbProduct.Focus();
                }
                else
                {
                    MessageBox.Show("Failed to save inventory transfer.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
