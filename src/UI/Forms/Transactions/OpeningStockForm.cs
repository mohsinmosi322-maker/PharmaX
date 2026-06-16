using System;
using System.Windows.Forms;
using EnterpriseERP.Application.Services;

namespace EnterpriseERP.UI.Forms.Transactions
{
    public partial class OpeningStockForm : BaseForm
    {
        private DataGridView dgvDetails;
        private DateTimePicker dtpDate;
        private Button btnSave, btnAddItem, btnRemoveItem;

        public OpeningStockForm(AuthService authService, FeatureService featureService) 
            : base(authService, featureService)
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Opening Stock Entry";
            this.Size = new Size(900, 600);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 60 };
            var lblTitle = new Label { Text = "Opening Stock", Font = new Font("Segoe UI", 14F, FontStyle.Bold), Location = new Point(10, 10) };
            
            var lblDate = new Label { Text = "Date:", Location = new Point(200, 15), Size = new Size(40, 25) };
            dtpDate = new DateTimePicker { Location = new Point(250, 15), Size = new Size(150, 25), Format = DateTimePickerFormat.Short };

            panelTop.Controls.AddRange(new Control[] { lblTitle, lblDate, dtpDate });

            var panelButtons = new Panel { Dock = DockStyle.Bottom, Height = 50 };
            btnAddItem = new Button { Text = "Add Item", Location = new Point(20, 10), Size = new Size(100, 30) };
            btnAddItem.Click += (s, e) => AddItem();
            
            btnRemoveItem = new Button { Text = "Remove", Location = new Point(130, 10), Size = new Size(100, 30) };
            btnRemoveItem.Click += (s, e) => RemoveItem();
            
            btnSave = new Button { Text = "Save Opening Stock", Location = new Point(700, 10), Size = new Size(150, 30), BackColor = System.Drawing.Color.Green, ForeColor = System.Drawing.Color.White };
            btnSave.Click += (s, e) => SaveOpeningStock();

            panelButtons.Controls.AddRange(new Control[] { btnAddItem, btnRemoveItem, btnSave });

            dgvDetails = new DataGridView 
            { 
                Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, AllowUserToAddRows = false, ReadOnly = true
            };
            dgvDetails.Columns.Add("ProductID", "Product ID");
            dgvDetails.Columns.Add("ProductName", "Product Name");
            dgvDetails.Columns.Add("BatchNumber", "Batch No");
            dgvDetails.Columns.Add("ExpiryDate", "Expiry Date");
            dgvDetails.Columns.Add("Quantity", "Quantity");
            dgvDetails.Columns.Add("CostPrice", "Cost Price");
            dgvDetails.Columns.Add("LineTotal", "Line Total");

            this.Controls.Add(dgvDetails);
            this.Controls.Add(panelTop);
            this.Controls.Add(panelButtons);
        }

        private void AddItem()
        {
            MessageBox.Show("Open product selection dialog for opening stock", "Add Item");
        }

        private void RemoveItem()
        {
            if (dgvDetails.CurrentRow != null)
                dgvDetails.Rows.RemoveAt(dgvDetails.CurrentRow.Index);
        }

        private void SaveOpeningStock()
        {
            if (dgvDetails.Rows.Count == 0) { ShowError("Please add at least one item"); return; }
            
            if (ConfirmAction("Save opening stock? This will create inventory transactions."))
            {
                ShowSuccess("Opening stock saved successfully!");
            }
        }
    }
}
