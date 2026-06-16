using System;
using System.Windows.Forms;
using EnterpriseERP.Application.Services;

namespace EnterpriseERP.UI.Forms.Transactions
{
    public partial class SaleForm : BaseForm
    {
        private DataGridView dgvDetails;
        private ComboBox cmbCustomer;
        private TextBox txtInvoiceNumber, txtBarcode, txtRemarks;
        private DateTimePicker dtpDate;
        private Button btnSave, btnAddItem, btnRemoveItem, btnPrint, btnScan;
        private Label lblTotalAmount, lblDiscount, lblNetTotal;
        private TextBox txtDiscount;

        public SaleForm(AuthService authService, FeatureService featureService) 
            : base(authService, featureService)
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Sales Entry (POS)";
            this.Size = new Size(1000, 700);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 120 };
            
            var lblCustomer = new Label { Text = "Customer:", Location = new Point(20, 15), Size = new Size(60, 25) };
            cmbCustomer = new ComboBox { Location = new Point(90, 15), Size = new Size(200, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCustomer.Items.AddRange(new object[] { "Walk-in Customer", "Ahmed Hospital", "City Pharmacy" });

            var lblInvoice = new Label { Text = "Invoice #:", Location = new Point(320, 15), Size = new Size(60, 25) };
            txtInvoiceNumber = new TextBox { Location = new Point(385, 15), Size = new Size(150, 25) };

            var lblDate = new Label { Text = "Date:", Location = new Point(560, 15), Size = new Size(40, 25) };
            dtpDate = new DateTimePicker { Location = new Point(610, 15), Size = new Size(150, 25), Format = DateTimePickerFormat.Short };

            var lblBarcode = new Label { Text = "Barcode:", Location = new Point(20, 50), Size = new Size(60, 25) };
            txtBarcode = new TextBox { Location = new Point(90, 50), Size = new Size(200, 25) };
            txtBarcode.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) AddByBarcode(); };

            btnScan = new Button { Text = "Scan", Location = new Point(300, 50), Size = new Size(60, 25) };
            btnScan.Click += (s, e) => AddByBarcode();

            var lblDiscount = new Label { Text = "Discount:", Location = new Point(380, 50), Size = new Size(60, 25) };
            txtDiscount = new TextBox { Location = new Point(445, 50), Size = new Size(100, 25) };
            txtDiscount.TextChanged += (s, e) => CalculateTotal();

            var lblRemarks = new Label { Text = "Remarks:", Location = new Point(560, 50), Size = new Size(60, 25) };
            txtRemarks = new TextBox { Location = new Point(630, 50), Size = new Size(130, 25) };

            panelTop.Controls.AddRange(new Control[] { lblCustomer, cmbCustomer, lblInvoice, txtInvoiceNumber, lblDate, dtpDate, lblBarcode, txtBarcode, btnScan, lblDiscount, txtDiscount, lblRemarks, txtRemarks });

            var panelButtons = new Panel { Dock = DockStyle.Bottom, Height = 50 };
            btnAddItem = new Button { Text = "Add Item", Location = new Point(20, 10), Size = new Size(100, 30) };
            btnAddItem.Click += (s, e) => AddItem();
            
            btnRemoveItem = new Button { Text = "Remove", Location = new Point(130, 10), Size = new Size(100, 30) };
            btnRemoveItem.Click += (s, e) => RemoveItem();
            
            btnSave = new Button { Text = "Save Sale", Location = new Point(700, 10), Size = new Size(120, 30), BackColor = System.Drawing.Color.Green, ForeColor = System.Drawing.Color.White };
            btnSave.Click += (s, e) => SaveSale();
            
            btnPrint = new Button { Text = "Print", Location = new Point(830, 10), Size = new Size(100, 30) };
            btnPrint.Click += (s, e) => PrintInvoice();

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
            dgvDetails.Columns.Add("SalePrice", "Sale Price");
            dgvDetails.Columns.Add("Discount", "Disc");
            dgvDetails.Columns.Add("LineTotal", "Line Total");

            var panelBottom = new Panel { Dock = DockStyle.Bottom, Height = 50 };
            lblTotalAmount = new Label { Text = "Total: 0.00", Font = new Font("Segoe UI", 10F), Location = new Point(400, 10), Size = new Size(120, 25) };
            lblDiscount = new Label { Text = "Disc: 0.00", Font = new Font("Segoe UI", 10F), Location = new Point(530, 10), Size = new Size(100, 25) };
            lblNetTotal = new Label { Text = "Net: 0.00", Font = new Font("Segoe UI", 12F, FontStyle.Bold), Location = new Point(650, 10), Size = new Size(150, 25) };
            panelBottom.Controls.AddRange(new Control[] { lblTotalAmount, lblDiscount, lblNetTotal });

            this.Controls.Add(dgvDetails);
            this.Controls.Add(panelTop);
            this.Controls.Add(panelButtons);
            this.Controls.Add(panelBottom);
        }

        private void AddByBarcode()
        {
            if (string.IsNullOrWhiteSpace(txtBarcode.Text)) return;
            MessageBox.Show($"Add product with barcode: {txtBarcode.Text}", "Scan");
            txtBarcode.Clear();
            txtBarcode.Focus();
        }

        private void AddItem()
        {
            MessageBox.Show("Open product selection dialog", "Add Item");
        }

        private void RemoveItem()
        {
            if (dgvDetails.CurrentRow != null)
                dgvDetails.Rows.RemoveAt(dgvDetails.CurrentRow.Index);
        }

        private void CalculateTotal()
        {
            decimal total = 0, discount = 0;
            foreach (DataGridViewRow row in dgvDetails.Rows)
            {
                if (row.Cells["LineTotal"].Value != null)
                    total += Convert.ToDecimal(row.Cells["LineTotal"].Value);
            }
            decimal.TryParse(txtDiscount.Text, out discount);
            lblTotalAmount.Text = $"Total: {total:F2}";
            lblDiscount.Text = $"Disc: {discount:F2}";
            lblNetTotal.Text = $"Net: {total - discount:F2}";
        }

        private void SaveSale()
        {
            if (dgvDetails.Rows.Count == 0) { ShowError("Please add at least one item"); return; }
            
            if (ConfirmAction("Save this sale?"))
            {
                ShowSuccess("Sale saved successfully!");
            }
        }

        private void PrintInvoice()
        {
            MessageBox.Show("Print sales invoice", "Print");
        }
    }
}
