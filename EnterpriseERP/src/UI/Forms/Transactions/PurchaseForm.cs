using System;
using System.Windows.Forms;
using EnterpriseInventory.Core.Domain;
using EnterpriseInventory.Infrastructure.Repositories;
using EnterpriseInventory.UI.Helpers;

namespace EnterpriseInventory.UI.Forms.Transactions
{
    public class PurchaseForm : Form
    {
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly ISupplierRepository _supplierRepository;
        private readonly IProductRepository _productRepository;

        public PurchaseForm()
        {
            _purchaseRepository = new PurchaseRepository(DbConnectionFactory.CreateConnection());
            _supplierRepository = new SupplierRepository(DbConnectionFactory.CreateConnection());
            _productRepository = new ProductRepository(DbConnectionFactory.CreateConnection());
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Purchase Entry";
            this.Size = new System.Drawing.Size(1100, 750);
            this.StartPosition = FormStartPosition.CenterScreen;

            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = System.Drawing.Color.FromArgb(240, 240, 240) };
            
            var lblTitle = new Label { Text = "New Purchase", Font = new System.Drawing.Font("Arial", 16F, System.Drawing.FontStyle.Bold), Location = new System.Drawing.Point(20, 15), AutoSize = true, Parent = pnlHeader };
            
            var lblSupplier = new Label { Text = "Supplier:", Location = new System.Drawing.Point(20, 45), AutoSize = true, Parent = pnlHeader };
            var cmbSupplier = new ComboBox { Location = new System.Drawing.Point(90, 42), Size = new System.Drawing.Size(250, 21), DropDownStyle = ComboBoxStyle.DropDownList, Parent = pnlHeader };
            cmbSupplier.DataSource = _supplierRepository.GetAll();
            cmbSupplier.DisplayMember = "SupplierName";
            cmbSupplier.ValueMember = "SupplierID";

            var lblInvoice = new Label { Text = "Invoice No:", Location = new System.Drawing.Point(380, 45), AutoSize = true, Parent = pnlHeader };
            var txtInvoice = new TextBox { Location = new System.Drawing.Point(460, 42), Size = new System.Drawing.Size(150, 23), Parent = pnlHeader };

            var dtpDate = new DateTimePicker { Location = new System.Drawing.Point(650, 42), Format = DateTimePickerFormat.Short, Parent = pnlHeader };

            pnlHeader.Controls.Add(lblSupplier);
            pnlHeader.Controls.Add(cmbSupplier);
            pnlHeader.Controls.Add(lblInvoice);
            pnlHeader.Controls.Add(txtInvoice);
            pnlHeader.Controls.Add(dtpDate);

            var dgv = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = false, AllowUserToAddRows = true, BackgroundColor = System.Drawing.Color.White, BorderStyle = BorderStyle.None };
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Product", Width = 300 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Batch No", Width = 120 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Expiry", Width = 100 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Qty", Width = 80 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Cost Price", Width = 100 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total", Width = 100 });

            var pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 70, BackColor = System.Drawing.Color.FromArgb(240, 240, 240) };
            var btnSave = new Button { Text = "Save Purchase", Location = new System.Drawing.Point(20, 15), Size = new System.Drawing.Size(130, 40), FlatStyle = FlatStyle.Flat, BackColor = System.Drawing.Color.FromArgb(0, 120, 215), ForeColor = System.Drawing.Color.White, Parent = pnlFooter };
            btnSave.Click += (s, e) => SavePurchase();
            var btnCancel = new Button { Text = "Cancel", Location = new System.Drawing.Point(160, 15), Size = new System.Drawing.Size(100, 40), DialogResult = DialogResult.Cancel, Parent = pnlFooter };

            pnlFooter.Controls.Add(btnSave);
            pnlFooter.Controls.Add(btnCancel);

            this.Controls.Add(dgv);
            this.Controls.Add(pnlFooter);
            this.Controls.Add(pnlHeader);
        }

        private void SavePurchase()
        {
            MessageBox.Show("Purchase functionality - Ready for implementation", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
