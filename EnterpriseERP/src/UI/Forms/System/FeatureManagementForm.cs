using System;
using System.Windows.Forms;

namespace EnterpriseERP.UI.Forms.System
{
    public class FeatureManagementForm : BaseForm
    {
        private DataGridView dgvFeatures;
        private Button btnSave;
        
        public FeatureManagementForm()
        {
            InitializeComponent();
            LoadFeatures();
        }

        private void InitializeComponent()
        {
            this.Text = "Feature Management";
            this.Size = new System.Drawing.Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            var headerPanel = CreateHeaderPanel("Feature Management");
            this.Controls.Add(headerPanel);

            dgvFeatures = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                ReadOnly = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            dgvFeatures.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "IsEnabled",
                HeaderText = "Enabled",
                Width = 80
            });
            dgvFeatures.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "FeatureCode",
                HeaderText = "Feature Code",
                Width = 200
            });
            dgvFeatures.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "FeatureName",
                HeaderText = "Feature Name",
                Width = 300
            });

            var panelBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = System.Drawing.Color.FromArgb(245, 245, 245)
            };

            btnSave = CreateButton("Save Changes", System.Drawing.Color.FromArgb(0, 150, 0));
            btnSave.Location = new System.Drawing.Point(20, 15);
            btnSave.Click += BtnSave_Click;

            panelBottom.Controls.Add(btnSave);

            this.Controls.Add(dgvFeatures);
            this.Controls.Add(panelBottom);
        }

        private async void LoadFeatures()
        {
            try
            {
                var features = await FeatureService.GetAllFeaturesAsync();
                dgvFeatures.DataSource = features;
            }
            catch (Exception ex)
            {
                ShowError($"Error loading features: {ex.Message}");
            }
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (DataGridViewRow row in dgvFeatures.Rows)
                {
                    if (row.DataBoundItem != null)
                    {
                        dynamic feature = row.DataBoundItem;
                        bool isEnabled = (bool)row.Cells["IsEnabled"].Value;
                        
                        await FeatureService.UpdateFeatureStatusAsync(feature.FeatureID, isEnabled);
                    }
                }

                ShowSuccess("Features updated successfully!");
                LoadFeatures();
            }
            catch (Exception ex)
            {
                ShowError($"Error saving features: {ex.Message}");
            }
        }
    }
}
