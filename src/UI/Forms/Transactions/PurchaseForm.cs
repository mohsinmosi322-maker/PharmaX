using System;
using System.Windows.Forms;
using EnterpriseERP.Application.Services;

namespace EnterpriseERP.UI.Forms.Transactions
{
    public partial class PurchaseForm : BaseForm
    {
        private DataGridView dgvDetails;
        private ComboBox cmbSupplier;
        private TextBox txtInvoiceNumber, txtRemarks;
        private DateTimePicker dtpDate;
        private Button btnSave, btnAddItem, btnRemoveItem, btnPrint;
        private Label lblTotalAmount;

        public PurchaseForm(AuthService authService, FeatureService featureService) 
            : base(authService, featureService)
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Purchase Entry";
            this.Size = new Size(1000, 700);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 120 };
            
            var lblSupplier = new Label { Text = "Supplier:", Location = new Point(20, 15), Size = new Size(60, 25) };
            cmbSupplier = new ComboBox { Location = new Point(90, 15), Size = new Size(200, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbSupplier.Items.AddRange(new object[] { "ABC Pharmaceuticals", "Medi Supplies Ltd", "HealthCare Distributors" });

            var lblInvoice = new Label { Text = "Invoice #:", Location = new Point(320, 15), Size = new Size(60, 25) };
            txtInvoiceNumber = new TextBox { Location = new Point(385, 15), Size = new Size(150, 25) };

            var lblDate = new Label { Text = "Date:", Location = new Point(560, 15), Size = new Size(40, 25) };
            dtpDate = new DateTimePicker { Location = new Point(610, 15), Size = new Size(150, 25), Format = DateTimePickerFormat.Short };

            var lblRemarks = new Label { Text = "Remarks:", Location = new Point(20, 50), Size = new Size(60, 25) };
            txtRemarks = new TextBox { Location = new Point(90, 50), Size = new Size(670, 25) };

            panelTop.Controls.AddRange(new Control[] { lblSupplier, cmbSupplier, lblInvoice, txtInvoiceNumber, lblDate, dtpDate, lblRemarks, txtRemarks });

            var panelButtons = new Panel { Dock = DockStyle.Bottom, Height = 50 };
            btnAddItem = new Button { Text = "Add Item", Location = new Point(20, 10), Size = new Size(100, 30) };
            btnAddItem.Click += (s, e) => AddItem();
            
            btnRemoveItem = new Button { Text = "Remove", Location = new Point(130, 10), Size = new Size(100, 30) };
            btnRemoveItem.Click += (s, e) => RemoveItem();
            
            btnSave = new Button { Text = "Save Purchase", Location = new Point(700, 10), Size = new Size(120, 30), BackColor = System.Drawing.Color.Green, ForeColor = System.Drawing.Color.White };
            btnSave.Click += (s, e) => SavePurchase();
            
            btnPrint = new Button { Text = "Print", Location = new Point(830, 10), Size = new Size(100, 30) };
            btnPrint.Click += (s, e) => PrintPurchase();

            panelButtons.Controls.AddRange(new Control[] { btnAddItem, btnRemoveItem, btnSave, btnPrint });

            dgvDetails = new DataGridView 
            { 
                Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, AllowUserToAddRows = false, ReadOnly = true
            };
            dgvDetails.Columns.Add("ProductID", "Product ID");
            dgvDetails.Columns.Add("ProductName", "Product Name");
            dgvDetails.Columns.Add("BatchNumber", "Batch No");
            dgvDetails.Columns.Add("ExpiryDate", "Expiry Date");
            dgvDetails.Columns.Add("Quantity", "Qty");
            dgvDetails.Columns.Add("BonusQty", "Bonus");
            dgvDetails.Columns.Add("CostPrice", "Cost Price");
            dgvDetails.Columns.Add("LineTotal", "Line Total");

            var panelBottom = new Panel { Dock = DockStyle.Bottom, Height = 40 };
            lblTotalAmount = new Label { Text = "Total Amount: 0.00", Font = new Font("Segoe UI", 12F, FontStyle.Bold), Location = new Point(600, 10), Size = new Size(200, 25) };
            panelBottom.Controls.Add(lblTotalAmount);

            this.Controls.Add(dgvDetails);
            this.Controls.Add(panelTop);
            this.Controls.Add(panelButtons);
            this.Controls.Add(panelBottom);
        }

        private void AddItem()
        {
            // Open product selection dialog
            MessageBox.Show("Open product selection dialog", "Add Item");
        }

        private void RemoveItem()
        {
            if (dgvDetails.CurrentRow != null)
                dgvDetails.Rows.RemoveAt(dgvDetails.CurrentRow.Index);
        }

        private void SavePurchase()
        {
            if (cmbSupplier.SelectedItem == null) { ShowError("Please select a supplier"); return; }
            if (string.IsNullOrWhiteSpace(txtInvoiceNumber.Text)) { ShowError("Invoice number is required"); return; }
            if (dgvDetails.Rows.Count == 0) { ShowError("Please add at least one item"); return; }
            
            if (ConfirmAction("Save this purchase?"))
            {
                ShowSuccess("Purchase saved successfully!");
                // Clear form
            }
        }

        private void PrintPurchase()
        {
            MessageBox.Show("Print purchase invoice", "Print");
        }
    }
}
