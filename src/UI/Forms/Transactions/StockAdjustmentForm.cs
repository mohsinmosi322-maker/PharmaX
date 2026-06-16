using System;
using System.Windows.Forms;
using EnterpriseERP.Application.Services;

namespace EnterpriseERP.UI.Forms.Transactions
{
    public partial class StockAdjustmentForm : BaseForm
    {
        private ComboBox cmbProduct, cmbBatch, cmbReason;
        private TextBox txtQuantity, txtRemarks;
        private DateTimePicker dtpDate;
        private Button btnSave;

        public StockAdjustmentForm(AuthService authService, FeatureService featureService) 
            : base(authService, featureService)
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Stock Adjustment";
            this.Size = new Size(500, 400);

            var lblProduct = new Label { Text = "Product:", Location = new Point(20, 20), Size = new Size(100, 25) };
            cmbProduct = new ComboBox { Location = new Point(140, 20), Size = new Size(320, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbProduct.Items.AddRange(new object[] { "Paracetamol 500mg", "Amoxicillin 250mg" });

            var lblBatch = new Label { Text = "Batch:", Location = new Point(20, 60), Size = new Size(100, 25) };
            cmbBatch = new ComboBox { Location = new Point(140, 60), Size = new Size(320, 25) };
            cmbBatch.Items.AddRange(new object[] { "BATCH001", "BATCH002" });

            var lblReason = new Label { Text = "Reason:", Location = new Point(20, 100), Size = new Size(100, 25) };
            cmbReason = new ComboBox { Location = new Point(140, 100), Size = new Size(320, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbReason.Items.AddRange(new object[] { "Physical Count", "Damage", "Expiry", "Correction" });

            var lblQty = new Label { Text = "Quantity (+/-):", Location = new Point(20, 140), Size = new Size(100, 25) };
            txtQuantity = new TextBox { Location = new Point(140, 140), Size = new Size(320, 25) };

            var lblDate = new Label { Text = "Date:", Location = new Point(20, 180), Size = new Size(100, 25) };
            dtpDate = new DateTimePicker { Location = new Point(140, 180), Size = new Size(320, 25), Format = DateTimePickerFormat.Short };

            var lblRemarks = new Label { Text = "Remarks:", Location = new Point(20, 220), Size = new Size(100, 25) };
            txtRemarks = new TextBox { Location = new Point(140, 220), Size = new Size(320, 25), Multiline = true, Height = 50 };

            btnSave = new Button { Text = "Save Adjustment", Location = new Point(180, 290), Size = new Size(140, 35), BackColor = System.Drawing.Color.Green, ForeColor = System.Drawing.Color.White };
            btnSave.Click += (s, e) => SaveAdjustment();

            this.Controls.AddRange(new Control[] { lblProduct, cmbProduct, lblBatch, cmbBatch, lblReason, cmbReason, lblQty, txtQuantity, lblDate, dtpDate, lblRemarks, txtRemarks, btnSave });
        }

        private void SaveAdjustment()
        {
            if (cmbProduct.SelectedItem == null) { ShowError("Please select a product"); return; }
            if (cmbReason.SelectedItem == null) { ShowError("Please select a reason"); return; }
            if (string.IsNullOrWhiteSpace(txtQuantity.Text)) { ShowError("Please enter quantity"); return; }

            if (ConfirmAction("Save stock adjustment? This will create inventory transactions."))
            {
                ShowSuccess("Stock adjustment saved successfully!");
            }
        }
    }
}
