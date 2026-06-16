using System;
using System.Data;
using System.Windows.Forms;
using EnterpriseERP.Core.Interfaces;
using EnterpriseERP.Application.Services;

namespace EnterpriseERP.UI.Forms.Reports
{
    public partial class ExpiryReportForm : Form
    {
        private readonly IReportingService _reportingService;

        private ComboBox cmbDays;
        private Button btnLoad, btnExport, btnClose;
        private DataGridView dgvExpiry;
        private Label lblDays;
        private TabControl tabControl;
        private DataGridView dgvNearExpiry;
        private DataGridView dgvExpired;

        public ExpiryReportForm(IReportingService reportingService)
        {
            _reportingService = reportingService;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Expiry Report";
            this.Size = new System.Drawing.Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;

            // Filter controls
            lblDays = new Label { Text = "Days to Expiry:", Location = new System.Drawing.Point(20, 15), AutoSize = true };
            cmbDays = new ComboBox { Location = new System.Drawing.Point(110, 12), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbDays.Items.AddRange(new object[] { "30 Days", "60 Days", "90 Days", "180 Days", "All" });
            cmbDays.SelectedIndex = 0;

            btnLoad = new Button { Text = "Load Report", Location = new System.Drawing.Point(280, 10), Width = 100, Height = 30 };
            btnExport = new Button { Text = "Export", Location = new System.Drawing.Point(390, 10), Width = 100, Height = 30 };
            btnClose = new Button { Text = "Close", Location = new System.Drawing.Point(1080, 10), Width = 100, Height = 30 };

            btnLoad.Click += BtnLoad_Click;
            btnExport.Click += BtnExport_Click;
            btnClose.Click += (s, e) => this.Close();

            // Tab control for Near Expiry and Expired
            tabControl = new TabControl
            {
                Location = new System.Drawing.Point(20, 50),
                Size = new System.Drawing.Size(1150, 600)
            };

           TabPage tabNearExpiry = new TabPage("Near Expiry");
            TabPage tabExpired = new TabPage("Expired");

            // Near Expiry Grid
            dgvNearExpiry = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true
            };

            dgvNearExpiry.Columns.Add(new DataGridViewTextBoxColumn { Name = "ProductCode", HeaderText = "Code", Width = 80 });
            dgvNearExpiry.Columns.Add(new DataGridViewTextBoxColumn { Name = "ProductName", HeaderText = "Product Name", Width = 200 });
            dgvNearExpiry.Columns.Add(new DataGridViewTextBoxColumn { Name = "BatchNumber", HeaderText = "Batch No", Width = 100 });
            dgvNearExpiry.Columns.Add(new DataGridViewTextBoxColumn { Name = "ExpiryDate", HeaderText = "Expiry Date", Width = 100 });
            dgvNearExpiry.Columns.Add(new DataGridViewTextBoxColumn { Name = "DaysRemaining", HeaderText = "Days Left", Width = 80 });
            dgvNearExpiry.Columns.Add(new DataGridViewTextBoxColumn { Name = "Quantity", HeaderText = "Qty", Width = 70 });
            dgvNearExpiry.Columns.Add(new DataGridViewTextBoxColumn { Name = "CostPrice", HeaderText = "Cost", Width = 80 });
            dgvNearExpiry.Columns.Add(new DataGridViewTextBoxColumn { Name = "TotalValue", HeaderText = "Total Value", Width = 100 });

            // Expired Grid
            dgvExpired = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true
            };

            dgvExpired.Columns.Add(new DataGridViewTextBoxColumn { Name = "ProductCode", HeaderText = "Code", Width = 80 });
            dgvExpired.Columns.Add(new DataGridViewTextBoxColumn { Name = "ProductName", HeaderText = "Product Name", Width = 200 });
            dgvExpired.Columns.Add(new DataGridViewTextBoxColumn { Name = "BatchNumber", HeaderText = "Batch No", Width = 100 });
            dgvExpired.Columns.Add(new DataGridViewTextBoxColumn { Name = "ExpiryDate", HeaderText = "Expiry Date", Width = 100 });
            dgvExpired.Columns.Add(new DataGridViewTextBoxColumn { Name = "DaysExpired", HeaderText = "Days Expired", Width = 90 });
            dgvExpired.Columns.Add(new DataGridViewTextBoxColumn { Name = "Quantity", HeaderText = "Qty", Width = 70 });
            dgvExpired.Columns.Add(new DataGridViewTextBoxColumn { Name = "CostPrice", HeaderText = "Cost", Width = 80 });
            dgvExpired.Columns.Add(new DataGridViewTextBoxColumn { Name = "TotalValue", HeaderText = "Loss Value", Width = 100 });

            tabNearExpiry.Controls.Add(dgvNearExpiry);
            tabExpired.Controls.Add(dgvExpired);
            tabControl.TabPages.AddRange(new TabPage[] { tabNearExpiry, tabExpired });

            this.Controls.AddRange(new Control[] {
                lblDays, cmbDays, btnLoad, btnExport, btnClose,
                tabControl
            });
        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            try
            {
                int days = 30;
                if (cmbDays.SelectedItem != null)
                {
                    string selected = cmbDays.SelectedItem.ToString();
                    if (selected.Contains("30")) days = 30;
                    else if (selected.Contains("60")) days = 30;
                    else if (selected.Contains("90")) days = 90;
                    else if (selected.Contains("180")) days = 180;
                }

                // Load near expiry
                var nearExpiryData = _reportingService.GetNearExpiryProducts(days);
                dgvNearExpiry.DataSource = nearExpiryData;

                // Load expired
                var expiredData = _reportingService.GetExpiredProducts();
                dgvExpired.DataSource = expiredData;

                // Highlight expired rows
                foreach (DataGridViewRow row in dgvExpired.Rows)
                {
                    row.DefaultCellStyle.BackColor = System.Drawing.Color.LightCoral;
                }

                // Highlight critical near expiry (less than 15 days)
                foreach (DataGridViewRow row in dgvNearExpiry.Rows)
                {
                    if (row.Cells["DaysRemaining"].Value != null)
                    {
                        int daysRemaining = Convert.ToInt32(row.Cells["DaysRemaining"].Value);
                        if (daysRemaining <= 15)
                        {
                            row.DefaultCellStyle.BackColor = System.Drawing.Color.Orange;
                        }
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
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx|CSV Files|*.csv",
                    Title = "Export Expiry Report",
                    FileName = $"Expiry_Report_{DateTime.Now:yyyyMMdd_HHmmss}"
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
