using System;
using System.Windows.Forms;

namespace EnterpriseERP.UI.Forms.System
{
    public class BackupRestoreForm : BaseForm
    {
        private ListBox lstBackups;
        private Button btnBackup;
        private Button btnRestore;
        private Button btnDelete;
        private Button btnRefresh;
        private Label lblLastBackup;

        public BackupRestoreForm()
        {
            InitializeComponent();
            LoadBackupHistory();
        }

        private void InitializeComponent()
        {
            this.Text = "Backup & Restore";
            this.Size = new System.Drawing.Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            var headerPanel = CreateHeaderPanel("Backup & Restore");
            this.Controls.Add(headerPanel);

            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                BackColor = System.Drawing.Color.FromArgb(245, 245, 245)
            };

            btnBackup = CreateButton("Create Backup", System.Drawing.Color.FromArgb(0, 150, 0));
            btnBackup.Size = new System.Drawing.Size(150, 40);
            btnBackup.Location = new System.Drawing.Point(20, 20);
            btnBackup.Click += BtnBackup_Click;

            btnRestore = CreateButton("Restore Selected", System.Drawing.Color.FromArgb(0, 102, 153));
            btnRestore.Size = new System.Drawing.Size(150, 40);
            btnRestore.Location = new System.Drawing.Point(190, 20);
            btnRestore.Click += BtnRestore_Click;

            btnDelete = CreateButton("Delete Selected", System.Drawing.Color.FromArgb(200, 50, 50));
            btnDelete.Size = new System.Drawing.Size(150, 40);
            btnDelete.Location = new System.Drawing.Point(360, 20);
            btnDelete.Click += BtnDelete_Click;

            btnRefresh = CreateButton("Refresh", System.Drawing.Color.Gray);
            btnRefresh.Size = new System.Drawing.Size(100, 40);
            btnRefresh.Location = new System.Drawing.Point(530, 20);
            btnRefresh.Click += BtnRefresh_Click;

            lblLastBackup = new Label
            {
                Text = "Last Backup: Never",
                Font = new System.Drawing.Font("Segoe UI", 10F),
                AutoSize = true,
                Location = new System.Drawing.Point(20, 80)
            };

            topPanel.Controls.Add(btnBackup);
            topPanel.Controls.Add(btnRestore);
            topPanel.Controls.Add(btnDelete);
            topPanel.Controls.Add(btnRefresh);
            topPanel.Controls.Add(lblLastBackup);

            lstBackups = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new System.Drawing.Font("Segoe UI", 10F),
                ItemHeight = 30
            };

            this.Controls.Add(lstBackups);
            this.Controls.Add(topPanel);
        }

        private async void LoadBackupHistory()
        {
            try
            {
                var backups = await BackupService.GetBackupHistoryAsync();
                lstBackups.Items.Clear();

                if (backups.Count > 0)
                {
                    var lastBackup = backups[0];
                    lblLastBackup.Text = $"Last Backup: {lastBackup.BackupDate:dd/MM/yyyy HH:mm}";

                    foreach (var backup in backups)
                    {
                        lstBackups.Items.Add($"{backup.BackupDate:dd/MM/yyyy HH:mm} - {backup.FileName} ({backup.FileSizeMB} MB)");
                    }
                }
                else
                {
                    lblLastBackup.Text = "Last Backup: Never";
                    lstBackups.Items.Add("No backups found");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error loading backup history: {ex.Message}");
            }
        }

        private async void BtnBackup_Click(object sender, EventArgs e)
        {
            try
            {
                var result = await BackupService.CreateBackupAsync();
                
                if (result.Success)
                {
                    ShowSuccess($"Backup created successfully!\nLocation: {result.BackupPath}\nSize: {result.FileSizeMB} MB");
                    LoadBackupHistory();
                }
                else
                {
                    ShowError($"Backup failed: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error creating backup: {ex.Message}");
            }
        }

        private async void BtnRestore_Click(object sender, EventArgs e)
        {
            if (lstBackups.SelectedIndex < 0)
            {
                ShowWarning("Please select a backup to restore");
                return;
            }

            var confirm = ShowConfirm("Are you sure you want to restore the selected backup?\n\nWARNING: This will overwrite all current data!");
            
            if (confirm != DialogResult.Yes)
                return;

            try
            {
                // Extract filename from list item
                var selectedItem = lstBackups.SelectedItem.ToString();
                var fileName = selectedItem.Split('-')[1].Trim().Split(' ')[0];

                var result = await BackupService.RestoreBackupAsync(fileName);

                if (result.Success)
                {
                    ShowSuccess("Database restored successfully!\nPlease restart the application.");
                    Application.Restart();
                }
                else
                {
                    ShowError($"Restore failed: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error restoring backup: {ex.Message}");
            }
        }

        private async void BtnDelete_Click(object sender, EventArgs e)
        {
            if (lstBackups.SelectedIndex < 0)
            {
                ShowWarning("Please select a backup to delete");
                return;
            }

            var confirm = ShowConfirm("Are you sure you want to delete the selected backup?");
            
            if (confirm != DialogResult.Yes)
                return;

            try
            {
                var selectedItem = lstBackups.SelectedItem.ToString();
                var fileName = selectedItem.Split('-')[1].Trim().Split(' ')[0];

                await BackupService.DeleteBackupAsync(fileName);
                ShowSuccess("Backup deleted successfully");
                LoadBackupHistory();
            }
            catch (Exception ex)
            {
                ShowError($"Error deleting backup: {ex.Message}");
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadBackupHistory();
        }
    }
}
