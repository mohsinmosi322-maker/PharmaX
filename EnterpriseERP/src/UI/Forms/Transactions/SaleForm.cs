using System;
using System.Windows.Forms;

namespace EnterpriseInventory.UI.Forms.Transactions
{
    public class SaleForm : Form
    {
        public SaleForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Point of Sale";
            this.Size = new System.Drawing.Size(1200, 750);
            this.StartPosition = FormStartPosition.CenterScreen;

            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = System.Drawing.Color.FromArgb(240, 240, 240) };
            var lblTitle = new Label { Text = "POS - Point of Sale", Font = new System.Drawing.Font("Arial", 16F, System.Drawing.FontStyle.Bold), Location = new System.Drawing.Point(20, 15), AutoSize = true, Parent = pnlHeader };
            pnlHeader.Controls.Add(lblTitle);

            var dgv = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = false, AllowUserToAddRows = true, BackgroundColor = System.Drawing.Color.White };
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Product", Width = 350 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Batch", Width = 100 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Qty", Width = 80 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Price", Width = 100 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Discount", Width = 80 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total", Width = 100 });

            var pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 80, BackColor = System.Drawing.Color.FromArgb(240, 240, 240) };
            var btnSave = new Button { Text = "Complete Sale", Location = new System.Drawing.Point(20, 20), Size = new System.Drawing.Size(140, 45), FlatStyle = FlatStyle.Flat, BackColor = System.Drawing.Color.FromArgb(76, 175, 80), ForeColor = System.Drawing.Color.White, Parent = pnlFooter };
            btnSave.Click += (s, e) => MessageBox.Show("Sale functionality - Ready for implementation", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            var btnCancel = new Button { Text = "Cancel", Location = new System.Drawing.Point(170, 20), Size = new System.Drawing.Size(100, 45), DialogResult = DialogResult.Cancel, Parent = pnlFooter };
            pnlFooter.Controls.Add(btnSave);
            pnlFooter.Controls.Add(btnCancel);

            this.Controls.Add(dgv);
            this.Controls.Add(pnlFooter);
            this.Controls.Add(pnlHeader);
        }
    }
}
