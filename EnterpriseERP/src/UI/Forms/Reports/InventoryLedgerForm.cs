using System;
using System.Data;
using System.Windows.Forms;
using EnterpriseERP.Core.Interfaces;
using EnterpriseERP.Application.Services;

namespace EnterpriseERP.UI.Forms.Reports
{
    public partial class InventoryLedgerForm : Form
    {
        private readonly IReportingService _reportingService;
        private readonly IProductRepository _productRepository;

        private ComboBox cmbProduct;
        private DateTimePicker dtpFromDate;
        private DateTimePicker dtpToDate;
        private Button btnLoad, btnExport, btnClose;
        private DataGridView dgvLedger;
        private Label lblProduct, lblFrom, lblTo;

        public InventoryLedgerForm(IReportingService reportingService, IProductRepository productRepository)
        {
            _reportingService = reportingService;
            _productRepository = productRepository;

            InitializeComponent();
            LoadProducts();
        }

        private void InitializeComponent()
        {
            this.Text = "Inventory Ledger Report";
            this.Size = new System.Drawing.Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;

            // Filters
            lblProduct = new Label { Text = "Product:", Location = new System.Drawing.Point(20, 15), AutoSize = true };
            cmbProduct = new ComboBox { Location = new System.Drawing.Point(80, 12), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            
            lblFrom = new Label { Text = "From Date:", Location = new System.Drawing.Point(400, 15), AutoSize = true };
            dtpFromDate = new DateTimePicker { Location = new System.Drawing.Point(480, 12), Width = 150, Value = DateTime.Today.AddMonths(-1) };
            
            lblTo = new Label { Text = "To Date:", Location = new System.Drawing.Point(650, 15), AutoSize = true };
            dtpToDate = new DateTimePicker { Location = new System.Drawing.Point(720, 12), Width = 150, Value = DateTime.Today };

            btnLoad = new Button { Text = "Load Report", Location = new System.Drawing.Point(900, 10), Width = 100, Height = 30 };
            btnExport = new Button { Text = "Export to Excel", Location = new System.Drawing.Point(1010, 10), Width = 120, Height = 30 };
            btnClose = new Button { Text = "Close", Location = new System.Drawing.Point(1080, 10), Width = 100, Height = 30 };

            btnLoad.Click += BtnLoad_Click;
            btnExport.Click += BtnExport_Click;
            btnClose.Click += (s, e) => this.Close();

            // Data grid
            dgvLedger = new DataGridView
            {
                Location = new System.Drawing.Point(20, 50),
                Size = new System.Drawing.Size(1150, 600),
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true
            };

            dgvLedger.Columns.Add(new DataGridViewTextBoxColumn { Name = "Date", HeaderText = "Date", Width = 100 });
            dgvLedger.Columns.Add(new DataGridViewTextBoxColumn { Name = "TransactionType", HeaderText = "Transaction", Width = 120 });
            dgvLedger.Columns.Add(new DataGridViewTextBoxColumn { Name = "Reference", HeaderText = "Reference", Width = 120 });
            dgvLedger.Columns.Add(new DataGridViewTextBoxColumn { Name = "QtyIn", HeaderText = "Qty In", Width = 80 });
            dgvLedger.Columns.Add(new DataGridViewTextBoxColumn { Name = "QtyOut", HeaderText = "Qty Out", Width = 80 });
            dgvLedger.Columns.Add(new DataGridViewTextBoxColumn { Name = "Balance", HeaderText = "Balance", Width = 80 });
            dgvLedger.Columns.Add(new DataGridViewTextBoxColumn { Name = "UnitCost", HeaderText = "Unit Cost", Width = 90 });
            dgvLedger.Columns.Add(new DataGridViewTextBoxColumn { Name = "TotalValue", HeaderText = "Total Value", Width = 100 });
            dgvLedger.Columns.Add(new DataGridViewTextBoxColumn { Name = "UserID", HeaderText = "User", Width = 80 });
            dgvLedger.Columns.Add(new DataGridViewTextBoxColumn { Name = "Remarks", HeaderText = "Remarks", Width = 200 });

            this.Controls.AddRange(new Control[] {
                lblProduct, cmbProduct, lblFrom, dtpFromDate, lblTo, dtpToDate,
                btnLoad, btnExport, btnClose,
                dgvLedger
            });
        }

        private void LoadProducts()
        {
            try
            {
                var products = _productRepository.GetAllActive();
                
                // Add "All Products" option
                var allProducts = new DataTable();
                allProducts.Columns.Add("ProductID", typeof(int));
                allProducts.Columns.Add("ProductName", typeof(string));
                allProducts.Rows.Add(0, "-- All Products --");
                
                foreach (var product in products)
                {
                    allProducts.Rows.Add(product.ProductID, product.ProductName);
                }
                
                cmbProduct.DataSource = allProducts;
                cmbProduct.DisplayMember = "ProductName";
                cmbProduct.ValueMember = "ProductID";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading products: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            try
            {
                int productId = cmbProduct.SelectedValue != null ? Convert.ToInt32(cmbProduct.SelectedValue) : 0;
                DateTime fromDate = dtpFromDate.Value.Date;
                DateTime toDate = dtpToDate.Value.Date;

                var reportData = _reportingService.GetInventoryLedger(productId, fromDate, toDate);
                
                dgvLedger.DataSource = reportData;
                
                // Format columns
                foreach (DataGridViewColumn col in dgvLedger.Columns)
                {
                    if (col.Name == "QtyIn" || col.Name == "QtyOut" || col.Name == "Balance")
                    {
                        col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    }
                    if (col.Name == "UnitCost" || col.Name == "TotalValue")
                    {
                        col.DefaultCellStyle.Format = "N2";
                        col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading report: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvLedger.Rows.Count == 0)
                {
                    MessageBox.Show("No data to export.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx|CSV Files|*.csv",
                    Title = "Export Report",
                    FileName = $"Inventory_Ledger_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Export logic here - simplified for demonstration
                    MessageBox.Show($"Report exported to: {saveFileDialog.FileName}\n\n(Note: Implement actual Excel/CSV export using EPPlus or similar library)", 
                        "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting report: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
