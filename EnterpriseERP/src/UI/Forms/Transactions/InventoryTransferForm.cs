using System;
using System.Windows.Forms;

namespace EnterpriseERP.UI.Forms.Transactions
{
    public class InventoryTransferForm : BaseForm
    {
        private ComboBox cmbFromBranch;
        private ComboBox cmbToBranch;
        private ComboBox cmbProduct;
        private TextBox txtBatchNumber;
        private NumericUpDown nudQuantity;
        private DateTimePicker dtpTransferDate;
        private TextBox txtRemarks;
        private Button btnTransfer;
        private Label lblCurrentStock;

        public InventoryTransferForm()
        {
            InitializeComponent();
            LoadBranches();
            LoadProducts();
        }

        private void InitializeComponent()
        {
            this.Text = "Inventory Transfer";
            this.Size = new System.Drawing.Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            var headerPanel = CreateHeaderPanel("Inter-Branch Inventory Transfer");
            this.Controls.Add(headerPanel);

            var mainPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(30),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };

            // From Branch
            mainPanel.Controls.Add(CreateLabel("From Branch *"));
            cmbFromBranch = CreateComboBox(400);
            cmbFromBranch.SelectedIndexChanged += CmbFromBranch_SelectedIndexChanged;
            mainPanel.Controls.Add(cmbFromBranch);

            // To Branch
            mainPanel.Controls.Add(CreateLabel("To Branch *"));
            cmbToBranch = CreateComboBox(400);
            mainPanel.Controls.Add(cmbToBranch);

            // Product
            mainPanel.Controls.Add(CreateLabel("Product *"));
            cmbProduct = CreateComboBox(400);
            cmbProduct.SelectedIndexChanged += CmbProduct_SelectedIndexChanged;
            mainPanel.Controls.Add(cmbProduct);

            // Batch Number
            mainPanel.Controls.Add(CreateLabel("Batch Number"));
            txtBatchNumber = CreateTextBox(400);
            mainPanel.Controls.Add(txtBatchNumber);

            // Current Stock Info
            lblCurrentStock = new Label
            {
                Text = "Current Stock: 0",
                Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.FromArgb(0, 102, 153),
                AutoSize = true,
                Margin = new Padding(0, 5, 0, 10)
            };
            mainPanel.Controls.Add(lblCurrentStock);

            // Quantity
            mainPanel.Controls.Add(CreateLabel("Quantity to Transfer *"));
            nudQuantity = new NumericUpDown
            {
                Width = 400,
                Height = 35,
                Font = new System.Drawing.Font("Segoe UI", 9F),
                Minimum = 1,
                Maximum = 1000000,
                Margin = new Padding(0, 0, 0, 15)
            };
            mainPanel.Controls.Add(nudQuantity);

            // Transfer Date
            mainPanel.Controls.Add(CreateLabel("Transfer Date *"));
            dtpTransferDate = new DateTimePicker
            {
                Width = 400,
                Height = 35,
                Format = DateTimePickerFormat.Short,
                Font = new System.Drawing.Font("Segoe UI", 9F),
                Margin = new Padding(0, 0, 0, 15)
            };
            mainPanel.Controls.Add(dtpTransferDate);

            // Remarks
            mainPanel.Controls.Add(CreateLabel("Remarks"));
            txtRemarks = CreateTextBox(400, height: 80);
            txtRemarks.Multiline = true;
            mainPanel.Controls.Add(txtRemarks);

            // Transfer Button
            btnTransfer = CreateButton("Execute Transfer", System.Drawing.Color.FromArgb(0, 102, 153));
            btnTransfer.Size = new System.Drawing.Size(200, 45);
            btnTransfer.Margin = new Padding(0, 20, 0, 0);
            btnTransfer.Click += BtnTransfer_Click;
            mainPanel.Controls.Add(btnTransfer);

            this.Controls.Add(mainPanel);
        }

        private Label CreateLabel(string text)
        {
            return new Label
            {
                Text = text,
                Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 10, 0, 5)
            };
        }

        private TextBox CreateTextBox(int width, int height = 35)
        {
            return new TextBox
            {
                Width = width,
                Height = height,
                Font = new System.Drawing.Font("Segoe UI", 9F),
                Margin = new Padding(0, 0, 0, 15),
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        private ComboBox CreateComboBox(int width)
        {
            return new ComboBox
            {
                Width = width,
                Height = 35,
                Font = new System.Drawing.Font("Segoe UI", 9F),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(0, 0, 0, 15)
            };
        }

        private async void LoadBranches()
        {
            try
            {
                cmbFromBranch.Items.Add("Main Branch");
                cmbToBranch.Items.Add("Branch A");
                cmbToBranch.Items.Add("Branch B");
            }
            catch (Exception ex)
            {
                ShowError($"Error loading branches: {ex.Message}");
            }
        }

        private async void LoadProducts()
        {
            try
            {
                var products = await InventoryService.GetActiveProductsAsync();
                
                foreach (var product in products)
                {
                    cmbProduct.Items.Add(new { 
                        ID = product.ProductID, 
                        Name = $"{product.ProductCode} - {product.ProductName}" 
                    });
                }
                
                if (cmbProduct.Items.Count > 0)
                    cmbProduct.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                ShowError($"Error loading products: {ex.Message}");
            }
        }

        private async void CmbProduct_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cmbProduct.SelectedItem != null && cmbFromBranch.SelectedItem != null)
                {
                    dynamic product = cmbProduct.SelectedItem;
                    var stock = await InventoryService.GetCurrentStockAsync(product.ID, GetCurrentBranchId());
                    lblCurrentStock.Text = $"Current Stock: {stock.Quantity}";
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error fetching stock: {ex.Message}");
            }
        }

        private async void CmbFromBranch_SelectedIndexChanged(object sender, EventArgs e)
        {
            CmbProduct_SelectedIndexChanged(sender, e);
        }

        private int GetCurrentBranchId()
        {
            return 1;
        }

        private async void BtnTransfer_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbFromBranch.SelectedItem == null || cmbToBranch.SelectedItem == null)
                {
                    ShowWarning("Please select both source and destination branches");
                    return;
                }

                if (cmbProduct.SelectedItem == null)
                {
                    ShowWarning("Please select a product");
                    return;
                }

                if (nudQuantity.Value <= 0)
                {
                    ShowWarning("Quantity must be greater than 0");
                    return;
                }

                var confirm = ShowConfirm("Are you sure you want to execute this transfer?");
                if (confirm != DialogResult.Yes)
                    return;

                dynamic product = cmbProduct.SelectedItem;
                
                var result = await InventoryService.TransferStockAsync(
                    GetCurrentBranchId(),
                    2,
                    product.ID,
                    txtBatchNumber.Text,
                    (int)nudQuantity.Value,
                    dtpTransferDate.Value,
                    txtRemarks.Text
                );

                if (result.Success)
                {
                    ShowSuccess("Inventory transferred successfully!");
                    ClearForm();
                }
                else
                {
                    ShowError($"Transfer failed: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error transferring inventory: {ex.Message}");
            }
        }

        private void ClearForm()
        {
            txtBatchNumber.Clear();
            nudQuantity.Value = 1;
            txtRemarks.Clear();
            lblCurrentStock.Text = "Current Stock: 0";
        }
    }
}
