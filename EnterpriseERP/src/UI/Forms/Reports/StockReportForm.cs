using System;
using System.Windows.Forms;

namespace EnterpriseInventory.UI.Forms.Reports
{
    public class StockReportForm : Form
    {
        public StockReportForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Stock Report";
            this.Size = new System.Drawing.Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            var pnlFilter = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = System.Drawing.Color.FromArgb(240, 240, 240) };
            
            var lblCategory = new Label { Text = "Category:", Location = new System.Drawing.Point(20, 25), AutoSize = true, Parent = pnlFilter };
            var cmbCategory = new ComboBox { Location = new System.Drawing.Point(90, 22), Size = new System.Drawing.Size(200, 21), DropDownStyle = ComboBoxStyle.DropDownList, Parent = pnlFilter };
            cmbCategory.Items.Add("-- All --");
            cmbCategory.SelectedIndex = 0;

            var lblLocation = new Label { Text = "Location:", Location = new System.Drawing.Point(320, 25), AutoSize = true, Parent = pnlFilter };
            var cmbLocation = new ComboBox { Location = new System.Drawing.Point(390, 22), Size = new System.Drawing.Size(200, 21), DropDownStyle = ComboBoxStyle.DropDownList, Parent = pnlFilter };
            cmbLocation.Items.Add("-- All --");
            cmbLocation.SelectedIndex = 0;

            var chkLowStock = new CheckBox { Text = "Show Low Stock Only", Location = new System.Drawing.Point(620, 25), AutoSize = true, Parent = pnlFilter };

            var btnRefresh = new Button { Text = "Refresh", Location = new System.Drawing.Point(850, 20), Size = new System.Drawing.Size(100, 35), FlatStyle = FlatStyle.Flat, Parent = pnlFilter };
            btnRefresh.Click += (s, e) => LoadReport();

            var btnExport = new Button { Text = "Export", Location = new System.Drawing.Point(970, 20), Size = new System.Drawing.Size(100, 35), FlatStyle = FlatStyle.Flat, Parent = pnlFilter };
            btnExport.Click += (s, e) => MessageBox.Show("Export functionality - Ready for implementation", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

            pnlFilter.Controls.Add(lblCategory);
            pnlFilter.Controls.Add(cmbCategory);
            pnlFilter.Controls.Add(lblLocation);
            pnlFilter.Controls.Add(cmbLocation);
            pnlFilter.Controls.Add(chkLowStock);
            pnlFilter.Controls.Add(btnRefresh);
            pnlFilter.Controls.Add(btnExport);

            var dgv = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = false, ReadOnly = true, BackgroundColor = System.Drawing.Color.White, BorderStyle = BorderStyle.None };
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Product Code", Width = 100 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Product Name", Width = 300 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Category", Width = 120 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Location", Width = 120 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Batch No", Width = 100 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Expiry", Width = 100 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Qty", Width = 80 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Unit", Width = 60 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Sale Price", Width = 100 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Value", Width = 100 });

            this.Controls.Add(dgv);
            this.Controls.Add(pnlFilter);
        }

        private void LoadReport()
        {
            MessageBox.Show("Stock report loaded successfully", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
