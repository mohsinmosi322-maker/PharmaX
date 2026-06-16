using System;
using System.Windows.Forms;
using EnterpriseERP.Application.Services;

namespace EnterpriseERP.UI.Forms.SystemAdmin
{
    public partial class UserForm : BaseForm
    {
        private DataGridView dgvUsers;
        private Button btnAdd, btnEdit, btnDelete, btnRefresh, btnResetPassword;

        public UserForm(AuthService authService, FeatureService featureService) 
            : base(authService, featureService)
        {
            InitializeComponent();
            LoadUsers();
        }

        private void InitializeComponent()
        {
            this.Text = "User Management";
            this.Size = new Size(1000, 600);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50 };
            var lblTitle = new Label { Text = "User Management", Font = new Font("Segoe UI", 14F, FontStyle.Bold), Location = new Point(10, 10) };
            
            btnRefresh = new Button { Text = "Refresh", Location = new Point(700, 12), Size = new Size(80, 30) };
            btnRefresh.Click += (s, e) => LoadUsers();

            btnAdd = new Button { Text = "Add New", Location = new Point(790, 12), Size = new Size(80, 30) };
            btnAdd.Click += (s, e) => OpenUserDialog(null);

            panelTop.Controls.AddRange(new Control[] { lblTitle, btnRefresh, btnAdd });

            dgvUsers = new DataGridView 
            { 
                Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, AllowUserToAddRows = false, ReadOnly = true
            };
            dgvUsers.Columns.Add("UserID", "ID");
            dgvUsers.Columns.Add("Username", "Username");
            dgvUsers.Columns.Add("FullName", "Full Name");
            dgvUsers.Columns.Add("RoleName", "Role");
            dgvUsers.Columns.Add("BranchName", "Branch");
            dgvUsers.Columns.Add("Mobile", "Mobile");
            dgvUsers.Columns.Add("Email", "Email");
            dgvUsers.Columns.Add("Status", "Status");

            var contextMenu = new ContextMenuStrip();
            var editItem = new ToolStripMenuItem("Edit");
            editItem.Click += (s, e) => OpenUserDialog(GetSelectedUser());
            var resetItem = new ToolStripMenuItem("Reset Password");
            resetItem.Click += (s, e) => ResetPassword();
            var deleteItem = new ToolStripMenuItem("Delete");
            deleteItem.Click += (s, e) => DeleteUser();
            contextMenu.Items.AddRange(new ToolStripItem[] { editItem, resetItem, deleteItem });
            dgvUsers.ContextMenuStrip = contextMenu;

            this.Controls.Add(dgvUsers);
            this.Controls.Add(panelTop);
        }

        private void LoadUsers()
        {
            try
            {
                dgvUsers.Rows.Clear();
                dgvUsers.Rows.Add(1, "admin", "System Administrator", "Super Admin", "Main Branch", "03001234567", "admin@erp.com", "Active");
                dgvUsers.Rows.Add(2, "manager", "Branch Manager", "Manager", "Main Branch", "03009876543", "manager@erp.com", "Active");
                dgvUsers.Rows.Add(3, "operator", "Sales Operator", "Operator", "Main Branch", "03111234567", "operator@erp.com", "Active");
            }
            catch (Exception ex) { ShowError($"Error loading users: {ex.Message}"); }
        }

        private dynamic GetSelectedUser()
        {
            if (dgvUsers.CurrentRow != null)
            {
                return new 
                {
                    UserID = Convert.ToInt32(dgvUsers.CurrentRow.Cells["UserID"].Value),
                    Username = dgvUsers.CurrentRow.Cells["Username"].Value?.ToString(),
                    FullName = dgvUsers.CurrentRow.Cells["FullName"].Value?.ToString(),
                    RoleName = dgvUsers.CurrentRow.Cells["RoleName"].Value?.ToString(),
                    Mobile = dgvUsers.CurrentRow.Cells["Mobile"].Value?.ToString(),
                    Email = dgvUsers.CurrentRow.Cells["Email"].Value?.ToString(),
                    Status = dgvUsers.CurrentRow.Cells["Status"].Value?.ToString()
                };
            }
            return null;
        }

        private void OpenUserDialog(dynamic user)
        {
            using (var dialog = new UserDialogForm(_authService, _featureService, user))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    LoadUsers();
                    ShowSuccess(user == null ? "User added successfully" : "User updated successfully");
                }
            }
        }

        private void ResetPassword()
        {
            var user = GetSelectedUser();
            if (user == null) { ShowError("Please select a user"); return; }
            if (ConfirmAction($"Reset password for user '{user.Username}'?"))
            {
                ShowSuccess("Password reset successfully!");
            }
        }

        private void DeleteUser()
        {
            var user = GetSelectedUser();
            if (user == null) { ShowError("Please select a user to delete"); return; }
            if (user.Username == "admin") { ShowError("Cannot delete admin user"); return; }
            if (ConfirmAction($"Are you sure you want to delete user '{user.Username}'?"))
            {
                try { LoadUsers(); ShowSuccess("User deleted successfully"); }
                catch (Exception ex) { ShowError($"Error deleting user: {ex.Message}"); }
            }
        }
    }

    public partial class UserDialogForm : Form
    {
        private readonly AuthService _authService;
        private readonly FeatureService _featureService;
        private readonly dynamic _user;
        private TextBox txtUsername, txtFullName, txtMobile, txtEmail;
        private ComboBox cmbRole, cmbBranch, cmbStatus;
        private Button btnSave, btnCancel;

        public UserDialogForm(AuthService authService, FeatureService featureService, dynamic user)
        {
            _authService = authService; _featureService = featureService; _user = user;
            InitializeComponent(); LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = _user == null ? "Add User" : "Edit User";
            this.Size = new Size(450, 400); this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false; this.MinimizeBox = false; this.StartPosition = FormStartPosition.CenterParent;

            int y = 20;
            var lblUsername = new Label { Text = "Username:", Location = new Point(20, y), Size = new Size(100, 25) };
            txtUsername = new TextBox { Location = new Point(140, y), Size = new Size(280, 25) }; y += 35;

            var lblFullName = new Label { Text = "Full Name:", Location = new Point(20, y), Size = new Size(100, 25) };
            txtFullName = new TextBox { Location = new Point(140, y), Size = new Size(280, 25) }; y += 35;

            var lblMobile = new Label { Text = "Mobile:", Location = new Point(20, y), Size = new Size(100, 25) };
            txtMobile = new TextBox { Location = new Point(140, y), Size = new Size(280, 25) }; y += 35;

            var lblEmail = new Label { Text = "Email:", Location = new Point(20, y), Size = new Size(100, 25) };
            txtEmail = new TextBox { Location = new Point(140, y), Size = new Size(280, 25) }; y += 35;

            var lblRole = new Label { Text = "Role:", Location = new Point(20, y), Size = new Size(100, 25) };
            cmbRole = new ComboBox { Location = new Point(140, y), Size = new Size(280, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbRole.Items.AddRange(new object[] { "Super Admin", "Admin", "Manager", "Operator", "Auditor" }); y += 35;

            var lblBranch = new Label { Text = "Branch:", Location = new Point(20, y), Size = new Size(100, 25) };
            cmbBranch = new ComboBox { Location = new Point(140, y), Size = new Size(280, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbBranch.Items.AddRange(new object[] { "Main Branch" }); y += 35;

            var lblStatus = new Label { Text = "Status:", Location = new Point(20, y), Size = new Size(100, 25) };
            cmbStatus = new ComboBox { Location = new Point(140, y), Size = new Size(280, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.AddRange(new object[] { "Active", "Inactive" }); y += 45;

            btnSave = new Button { Text = "Save", Location = new Point(150, y), Size = new Size(80, 30) };
            btnSave.Click += (s, e) => SaveUser();
            btnCancel = new Button { Text = "Cancel", Location = new Point(250, y), Size = new Size(80, 30) };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.AddRange(new Control[] { lblUsername, txtUsername, lblFullName, txtFullName, lblMobile, txtMobile, lblEmail, txtEmail, lblRole, cmbRole, lblBranch, cmbBranch, lblStatus, cmbStatus, btnSave, btnCancel });
            this.AcceptButton = btnSave; this.CancelButton = btnCancel;
        }

        private void LoadData()
        {
            if (_user != null)
            {
                txtUsername.Text = _user.Username; txtFullName.Text = _user.FullName;
                txtMobile.Text = _user.Mobile; txtEmail.Text = _user.Email;
                cmbRole.SelectedItem = _user.RoleName; cmbStatus.SelectedItem = _user.Status;
            }
            else { cmbStatus.SelectedItem = "Active"; }
        }

        private void SaveUser()
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtFullName.Text))
            { MessageBox.Show("Username and Full Name are required", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            this.DialogResult = DialogResult.OK;
        }
    }
}
