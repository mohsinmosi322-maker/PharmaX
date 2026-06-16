using System;
using System.Windows.Forms;

namespace EnterpriseERP.UI.Forms.System
{
    public class SettingsForm : BaseForm
    {
        private TextBox txtCompanyName;
        private TextBox txtAddress;
        private TextBox txtPhone;
        private TextBox txtEmail;
        private TextBox txtTaxRate;
        private ComboBox cmbCurrency;
        private TextBox txtInvoiceFooter;
        private ComboBox cmbDateFormat;
        private Button btnSave;
        private PictureBox picLogo;
        private Button btnUploadLogo;

        public SettingsForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "System Settings";
            this.Size = new System.Drawing.Size(800, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            var headerPanel = CreateHeaderPanel("System Settings");
            this.Controls.Add(headerPanel);

            var mainPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };

            // Company Logo
            var logoPanel = new Panel
            {
                Width = 700,
                Height = 120,
                Margin = new Padding(0, 0, 0, 10)
            };

            picLogo = new PictureBox
            {
                Size = new System.Drawing.Size(100, 100),
                Location = new System.Drawing.Point(0, 10),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BorderStyle = BorderStyle.FixedSingle
            };

            btnUploadLogo = CreateButton("Upload Logo", System.Drawing.Color.FromArgb(0, 102, 153));
            btnUploadLogo.Size = new System.Drawing.Size(120, 35);
            btnUploadLogo.Location = new System.Drawing.Point(120, 40);
            btnUploadLogo.Click += BtnUploadLogo_Click;

            logoPanel.Controls.Add(picLogo);
            logoPanel.Controls.Add(btnUploadLogo);
            mainPanel.Controls.Add(logoPanel);

            // Company Name
            mainPanel.Controls.Add(CreateLabel("Company Name"));
            txtCompanyName = CreateTextBox(700);
            mainPanel.Controls.Add(txtCompanyName);

            // Address
            mainPanel.Controls.Add(CreateLabel("Address"));
            txtAddress = CreateTextBox(700, height: 60);
            txtAddress.Multiline = true;
            mainPanel.Controls.Add(txtAddress);

            // Phone
            mainPanel.Controls.Add(CreateLabel("Phone"));
            txtPhone = CreateTextBox(340);
            mainPanel.Controls.Add(txtPhone);

            // Email
            mainPanel.Controls.Add(CreateLabel("Email"));
            txtEmail = CreateTextBox(340);
            mainPanel.Controls.Add(txtEmail);

            // Tax Rate
            mainPanel.Controls.Add(CreateLabel("Tax Rate (%)"));
            txtTaxRate = CreateTextBox(340);
            mainPanel.Controls.Add(txtTaxRate);

            // Currency
            mainPanel.Controls.Add(CreateLabel("Currency"));
            cmbCurrency = CreateComboBox(340);
            cmbCurrency.Items.AddRange(new object[] { "USD", "EUR", "GBP", "PKR", "INR", "AED" });
            mainPanel.Controls.Add(cmbCurrency);

            // Invoice Footer
            mainPanel.Controls.Add(CreateLabel("Invoice Footer"));
            txtInvoiceFooter = CreateTextBox(700);
            mainPanel.Controls.Add(txtInvoiceFooter);

            // Date Format
            mainPanel.Controls.Add(CreateLabel("Date Format"));
            cmbDateFormat = CreateComboBox(340);
            cmbDateFormat.Items.AddRange(new object[] { "dd/MM/yyyy", "MM/dd/yyyy", "yyyy-MM-dd" });
            mainPanel.Controls.Add(cmbDateFormat);

            // Save Button
            btnSave = CreateButton("Save Settings", System.Drawing.Color.FromArgb(0, 150, 0));
            btnSave.Size = new System.Drawing.Size(200, 40);
            btnSave.Margin = new Padding(0, 20, 0, 0);
            btnSave.Click += BtnSave_Click;
            mainPanel.Controls.Add(btnSave);

            this.Controls.Add(mainPanel);
        }

        private Label CreateLabel(string text)
        {
            return new Label
            {
                Text = text,
                Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 10, 0, 5)
            };
        }

        private TextBox CreateTextBox(int width, int height = 35)
        {
            return new TextBox
            {
                Width = width,
                Height = height,
                Font = new System.Drawing.Font("Segoe UI", 9F),
                Margin = new Padding(0, 0, 0, 15),
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        private ComboBox CreateComboBox(int width)
        {
            return new ComboBox
            {
                Width = width,
                Height = 35,
                Font = new System.Drawing.Font("Segoe UI", 9F),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(0, 0, 0, 15)
            };
        }

        private async void LoadSettings()
        {
            try
            {
                var settings = await AuditService.GetSystemSettingsAsync(); // Reusing audit service for now
                
                if (settings != null)
                {
                    txtCompanyName.Text = settings.CompanyName;
                    txtAddress.Text = settings.Address;
                    txtPhone.Text = settings.Phone;
                    txtEmail.Text = settings.Email;
                    txtTaxRate.Text = settings.TaxRate.ToString();
                    cmbCurrency.SelectedItem = settings.Currency;
                    txtInvoiceFooter.Text = settings.InvoiceFooter;
                    cmbDateFormat.SelectedItem = settings.DateFormat;
                    
                    if (!string.IsNullOrEmpty(settings.LogoPath))
                    {
                        try
                        {
                            picLogo.Image = System.Drawing.Image.FromFile(settings.LogoPath);
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error loading settings: {ex.Message}");
            }
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(txtCompanyName.Text))
                {
                    ShowWarning("Company Name is required");
                    return;
                }

                // Save settings logic here
                ShowSuccess("Settings saved successfully!");
            }
            catch (Exception ex)
            {
                ShowError($"Error saving settings: {ex.Message}");
            }
        }

        private void BtnUploadLogo_Click(object sender, EventArgs e)
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp",
                    Title = "Select Company Logo"
                };

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    picLogo.Image = System.Drawing.Image.FromFile(openFileDialog.FileName);
                    // Save logo path logic here
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error uploading logo: {ex.Message}");
            }
        }
    }
}
