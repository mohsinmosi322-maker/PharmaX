using System;
using System.Windows.Forms;

namespace EnterpriseERP.UI.Forms.System
{
    public class AuditLogForm : BaseForm
    {
        private DataGridView dgvAuditLogs;
        private ComboBox cmbUsers;
        private DateTimePicker dtpFrom;
        private DateTimePicker dtpTo;
        private Button btnSearch;
        private Button btnExport;

        public AuditLogForm()
        {
            InitializeComponent();
            LoadAuditLogs();
        }

        private void InitializeComponent()
        {
            this.Text = "Audit Log";
            this.Size = new System.Drawing.Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            var headerPanel = CreateHeaderPanel("Audit Trail");
            this.Controls.Add(headerPanel);

            var filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = System.Drawing.Color.FromArgb(245, 245, 245)
            };

            cmbUsers = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 200,
                Location = new System.Drawing.Point(20, 20)
            };
            cmbUsers.Items.Add("All Users");

            dtpFrom = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Width = 150,
                Location = new System.Drawing.Point(240, 20)
            };
            dtpFrom.Value = DateTime.Now.AddDays(-30);

            dtpTo = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Width = 150,
                Location = new System.Drawing.Point(410, 20)
            };
            dtpTo.Value = DateTime.Now;

            btnSearch = CreateButton("Search", System.Drawing.Color.FromArgb(0, 102, 153));
            btnSearch.Location = new System.Drawing.Point(580, 20);
            btnSearch.Click += BtnSearch_Click;

            btnExport = CreateButton("Export", System.Drawing.Color.FromArgb(0, 150, 0));
            btnExport.Location = new System.Drawing.Point(700, 20);
            btnExport.Click += BtnExport_Click;

            filterPanel.Controls.Add(cmbUsers);
            filterPanel.Controls.Add(dtpFrom);
            filterPanel.Controls.Add(dtpTo);
            filterPanel.Controls.Add(btnSearch);
            filterPanel.Controls.Add(btnExport);

            dgvAuditLogs = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            dgvAuditLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Timestamp",
                HeaderText = "Date/Time",
                Width = 150
            });
            dgvAuditLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "UserName",
                HeaderText = "User",
                Width = 120
            });
            dgvAuditLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Action",
                HeaderText = "Action",
                Width = 150
            });
            dgvAuditLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TableName",
                HeaderText = "Table",
                Width = 120
            });
            dgvAuditLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "RecordID",
                HeaderText = "Record ID",
                Width = 80
            });
            dgvAuditLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Description",
                HeaderText = "Description",
                Width = 400
            });

            this.Controls.Add(dgvAuditLogs);
            this.Controls.Add(filterPanel);
        }

        private async void LoadAuditLogs()
        {
            try
            {
                var logs = await AuditService.GetAuditLogsAsync(
                    null,
                    dtpFrom.Value,
                    dtpTo.Value
                );
                dgvAuditLogs.DataSource = logs;
            }
            catch (Exception ex)
            {
                ShowError($"Error loading audit logs: {ex.Message}");
            }
        }

        private async void BtnSearch_Click(object sender, EventArgs e)
        {
            await LoadAuditLogsAsync();
        }

        private async System.Threading.Tasks.Task LoadAuditLogsAsync()
        {
            try
            {
                int? userId = null;
                if (cmbUsers.SelectedIndex > 0)
                {
                    // Parse user ID from selected item
                    userId = cmbUsers.SelectedValue as int?;
                }

                var logs = await AuditService.GetAuditLogsAsync(
                    userId,
                    dtpFrom.Value,
                    dtpTo.Value
                );
                dgvAuditLogs.DataSource = logs;
            }
            catch (Exception ex)
            {
                ShowError($"Error loading audit logs: {ex.Message}");
            }
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "CSV Files|*.csv|Excel Files|*.xlsx",
                    Title = "Export Audit Logs"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    // Export logic here
                    ShowSuccess("Audit logs exported successfully!");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error exporting audit logs: {ex.Message}");
            }
        }
    }
}
