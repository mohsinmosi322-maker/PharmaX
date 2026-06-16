using System;
using System.Windows.Forms;
using EnterpriseERP.Application.Services;

namespace EnterpriseERP.UI.Forms.Reports
{
    public partial class StockReportForm : BaseForm
    {
        private DataGridView dgvStock;
        private Button btnRefresh, btnExport;
        private ComboBox cmbCategory, cmbFilter;

        public StockReportForm(AuthService authService, FeatureService featureService) 
            : base(authService, featureService)
        {
            InitializeComponent();
            LoadStockReport();
        }

        private void InitializeComponent()
        {
            this.Text = "Current Stock Report";
            this.Size = new Size(1000, 600);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50 };
            var lblTitle = new Label { Text = "Current Stock Report", Font = new Font("Segoe UI", 14F, FontStyle.Bold), Location = new Point(10, 10) };
            
            var lblCategory = new Label { Text = "Category:", Location = new Point(200, 15), Size = new Size(60, 25) };
            cmbCategory = new ComboBox { Location = new Point(270, 15), Size = new Size(150, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCategory.Items.AddRange(new object[] { "All", "Medicines", "Equipment", "Consumables" });

            var lblFilter = new Label { Text = "Filter:", Location = new Point(440, 15), Size = new Size(50, 25) };
            cmbFilter = new ComboBox { Location = new Point(500, 15), Size = new Size(150, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbFilter.Items.AddRange(new object[] { "All Stock", "Low Stock", "Out of Stock", "Near Expiry" });

            btnRefresh = new Button { Text = "Refresh", Location = new Point(670, 12), Size = new Size(80, 30) };
            btnRefresh.Click += (s, e) => LoadStockReport();

            btnExport = new Button { Text = "Export Excel", Location = new Point(760, 12), Size = new Size(100, 30) };
            btnExport.Click += (s, e) => ExportReport();

            panelTop.Controls.AddRange(new Control[] { lblTitle, lblCategory, cmbCategory, lblFilter, cmbFilter, btnRefresh, btnExport });

            dgvStock = new DataGridView 
            { 
                Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, AllowUserToAddRows = false, ReadOnly = true
            };
            dgvStock.Columns.Add("ProductCode", "Code");
            dgvStock.Columns.Add("ProductName", "Product Name");
            dgvStock.Columns.Add("Category", "Category");
            dgvStock.Columns.Add("BatchNumber", "Batch No");
            dgvStock.Columns.Add("ExpiryDate", "Expiry Date");
            dgvStock.Columns.Add("Quantity", "Quantity");
            dgvStock.Columns.Add("UnitPrice", "Unit Price");
            dgvStock.Columns.Add("TotalValue", "Total Value");
            dgvStock.Columns.Add("Status", "Status");

            this.Controls.Add(dgvStock);
            this.Controls.Add(panelTop);
        }

        private void LoadStockReport()
        {
            try
            {
                dgvStock.Rows.Clear();
                dgvStock.Rows.Add("PROD001", "Paracetamol 500mg", "Medicines", "BATCH001", "2025-12-31", "1000", "15.00", "15000.00", "OK");
                dgvStock.Rows.Add("PROD002", "Amoxicillin 250mg", "Medicines", "BATCH002", "2025-06-30", "50", "35.00", "1750.00", "Low");
                dgvStock.Rows.Add("PROD003", "Syrup Cough 100ml", "Medicines", "BATCH003", "2024-03-15", "200", "75.00", "15000.00", "Near Expiry");
            }
            catch (Exception ex) { ShowError($"Error loading report: {ex.Message}"); }
        }

        private void ExportReport()
        {
            MessageBox.Show("Export to Excel functionality", "Export");
        }
    }
}
