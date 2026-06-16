using System;
using System.Data;
using System.Windows.Forms;
using EnterpriseERP.Core.Interfaces;
using EnterpriseERP.Application.Services;

namespace EnterpriseERP.UI.Forms.Reports
{
    public partial class PurchaseReportForm : Form
    {
        private readonly IReportingService _reportingService;

        private DateTimePicker dtpFromDate;
        private DateTimePicker dtpToDate;
        private ComboBox cmbSupplier;
        private ComboBox cmbProduct;
        private Button btnLoad, btnExport, btnClose;
        private DataGridView dgvPurchase;
        private Label lblFrom, lblTo, lblSupplier, lblProduct;
        private GroupBox grpFilters;

        public PurchaseReportForm(IReportingService reportingService)
        {
            _reportingService = reportingService;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Purchase Report";
            this.Size = new System.Drawing.Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;

            // Filter group box
            grpFilters = new GroupBox
            {
                Text = "Filters",
                Location = new System.Drawing.Point(20, 10),
                Size = new System.Drawing.Size(1150, 80)
            };

            lblFrom = new Label { Text = "From Date:", Location = new System.Drawing.Point(15, 25), AutoSize = true };
            dtpFromDate = new DateTimePicker { Location = new System.Drawing.Point(90, 22), Width = 150, Value = DateTime.Today.AddMonths(-1) };

            lblTo = new Label { Text = "To Date:", Location = new System.Drawing.Point(260, 25), AutoSize = true };
            dtpToDate = new DateTimePicker { Location = new System.Drawing.Point(330, 22), Width = 150, Value = DateTime.Today };

            lblSupplier = new Label { Text = "Supplier:", Location = new System.Drawing.Point(500, 25), AutoSize = true };
            cmbSupplier = new ComboBox { Location = new System.Drawing.Point(570, 22), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };

            lblProduct = new Label { Text = "Product:", Location = new System.Drawing.Point(790, 25), AutoSize = true };
            cmbProduct = new ComboBox { Location = new System.Drawing.Point(860, 22), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };

            btnLoad = new Button { Text = "Load Report", Location = new System.Drawing.Point(10, 45), Width = 100, Height = 28 };
            btnExport = new Button { Text = "Export", Location = new System.Drawing.Point(120, 45), Width = 90, Height = 28 };
            btnClose = new Button { Text = "Close", Location = new System.Drawing.Point(1040, 45), Width = 90, Height = 28 };

            btnLoad.Click += BtnLoad_Click;
            btnExport.Click += BtnExport_Click;
            btnClose.Click += (s, e) => this.Close();

            grpFilters.Controls.AddRange(new Control[] {
                lblFrom, dtpFromDate, lblTo, dtpToDate,
                lblSupplier, cmbSupplier, lblProduct, cmbProduct,
                btnLoad, btnExport, btnClose
            });

            // Data grid
            dgvPurchase = new DataGridView
            {
                Location = new System.Drawing.Point(20, 95),
                Size = new System.Drawing.Size(1150, 550),
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true
            };

            dgvPurchase.Columns.Add(new DataGridViewTextBoxColumn { Name = "PurchaseDate", HeaderText = "Date", Width = 100 });
            dgvPurchase.Columns.Add(new DataGridViewTextBoxColumn { Name = "InvoiceNumber", HeaderText = "Invoice #", Width = 120 });
            dgvPurchase.Columns.Add(new DataGridViewTextBoxColumn { Name = "SupplierName", HeaderText = "Supplier", Width = 150 });
            dgvPurchase.Columns.Add(new DataGridViewTextBoxColumn { Name = "ProductCode", HeaderText = "Code", Width = 80 });
            dgvPurchase.Columns.Add(new DataGridViewTextBoxColumn { Name = "ProductName", HeaderText = "Product", Width = 180 });
            dgvPurchase.Columns.Add(new DataGridViewTextBoxColumn { Name = "BatchNumber", HeaderText = "Batch", Width = 90 });
            dgvPurchase.Columns.Add(new DataGridViewTextBoxColumn { Name = "ExpiryDate", HeaderText = "Expiry", Width = 100 });
            dgvPurchase.Columns.Add(new DataGridViewTextBoxColumn { Name = "Quantity", HeaderText = "Qty", Width = 70 });
            dgvPurchase.Columns.Add(new DataGridViewTextBoxColumn { Name = "BonusQty", HeaderText = "Bonus", Width = 70 });
            dgvPurchase.Columns.Add(new DataGridViewTextBoxColumn { Name = "UnitCost", HeaderText = "Unit Cost", Width = 90 });
            dgvPurchase.Columns.Add(new DataGridViewTextBoxColumn { Name = "TotalAmount", HeaderText = "Total", Width = 100 });

            // Summary panel at bottom
            Panel pnlSummary = new Panel
            {
                Location = new System.Drawing.Point(20, 650),
                Size = new System.Drawing.Size(1150, 40),
                BackColor = System.Drawing.Color.LightGray
            };

            Label lblSummary = new Label
            {
                Text = "Total Purchases: 0 | Total Quantity: 0",
                Location = new System.Drawing.Point(10, 10),
                AutoSize = true,
                Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold)
            };
            lblSummary.Name = "lblSummary";
            pnlSummary.Controls.Add(lblSummary);

            this.Controls.AddRange(new Control[] { grpFilters, dgvPurchase, pnlSummary });
        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime fromDate = dtpFromDate.Value.Date;
                DateTime toDate = dtpToDate.Value.Date;
                int supplierId = cmbSupplier.SelectedValue != null ? Convert.ToInt32(cmbSupplier.SelectedValue) : 0;
                int productId = cmbProduct.SelectedValue != null ? Convert.ToInt32(cmbProduct.SelectedValue) : 0;

                var reportData = _reportingService.GetPurchaseReport(fromDate, toDate, supplierId, productId);
                
                dgvPurchase.DataSource = reportData;

                // Calculate summary
                decimal totalPurchases = 0;
                int totalQty = 0;

                foreach (DataGridViewRow row in dgvPurchase.Rows)
                {
                    if (row.Cells["TotalAmount"].Value != null)
                        totalPurchases += Convert.ToDecimal(row.Cells["TotalAmount"].Value);
                    if (row.Cells["Quantity"].Value != null)
                        totalQty += Convert.ToInt32(row.Cells["Quantity"].Value);
                }

                Panel pnl = this.Controls.Find("pnlSummary", false)[0] as Panel;
                if (pnl != null)
                {
                    Label lbl = pnl.Controls.Find("lblSummary", false)[0] as Label;
                    if (lbl != null)
                    {
                        lbl.Text = $"Total Purchases: {totalPurchases:N2} | Total Quantity: {totalQty:N0}";
                    }
                }

                // Format currency columns
                foreach (DataGridViewColumn col in dgvPurchase.Columns)
                {
                    if (col.Name == "UnitCost" || col.Name == "TotalAmount")
                    {
                        col.DefaultCellStyle.Format = "N2";
                        col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    }
                    if (col.Name == "Quantity" || col.Name == "BonusQty")
                    {
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
                if (dgvPurchase.Rows.Count == 0)
                {
                    MessageBox.Show("No data to export.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx|CSV Files|*.csv",
                    Title = "Export Purchase Report",
                    FileName = $"Purchase_Report_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    MessageBox.Show($"Report exported to: {saveFileDialog.FileName}\n\n(Note: Implement actual Excel export)",
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
