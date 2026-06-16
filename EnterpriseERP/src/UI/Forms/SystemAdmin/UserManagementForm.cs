using System;
using System.Data;
using System.Windows.Forms;
using EnterpriseERP.Core.Interfaces;
using EnterpriseERP.Application.Services;

namespace EnterpriseERP.UI.Forms.SystemAdmin
{
    public partial class UserManagementForm : Form
    {
        private readonly IAuthService _authService;
        private readonly IRoleRepository _roleRepository;
        private readonly int _currentUserId;

        private DataGridView dgvUsers;
        private Button btnAdd, btnEdit, btnDelete, btnResetPassword, btnClose;
        private TextBox txtUsername, txtFullName, txtMobile, txtEmail;
        private ComboBox cmbRole, cmbBranch, cmbStatus;
        private Label lblUsername, lblFullName, lblMobile, lblEmail, lblRole, lblBranch, lblStatus;

        public UserManagementForm(IAuthService authService, IRoleRepository roleRepository, int currentUserId)
        {
            _authService = authService;
            _roleRepository = roleRepository;
            _currentUserId = currentUserId;

            InitializeComponent();
            LoadRoles();
            LoadUsers();
        }

        private void InitializeComponent()
        {
            this.Text = "User Management";
            this.Size = new System.Drawing.Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Create data grid
            dgvUsers = new DataGridView
            {
                Location = new System.Drawing.Point(20, 20),
                Size = new System.Drawing.Size(950, 300),
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true
            };

            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "UserID", HeaderText = "ID", Width = 50, Visible = false });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "Username", HeaderText = "Username", Width = 120 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "FullName", HeaderText = "Full Name", Width = 150 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "Mobile", HeaderText = "Mobile", Width = 120 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "Email", HeaderText = "Email", Width = 180 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "Role", HeaderText = "Role", Width = 100 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "Branch", HeaderText = "Branch", Width = 120 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", Width = 80 });

            dgvUsers.CellDoubleClick += DgvUsers_CellDoubleClick;

            // Input fields
            int yPos = 340;
            lblUsername = new Label { Text = "Username:", Location = new System.Drawing.Point(20, yPos), AutoSize = true };
            txtUsername = new TextBox { Location = new System.Drawing.Point(120, yPos - 3), Width = 200 };

            lblFullName = new Label { Text = "Full Name:", Location = new System.Drawing.Point(350, yPos), AutoSize = true };
            txtFullName = new TextBox { Location = new System.Drawing.Point(430, yPos - 3), Width = 200 };

            yPos += 40;
            lblMobile = new Label { Text = "Mobile:", Location = new System.Drawing.Point(20, yPos), AutoSize = true };
            txtMobile = new TextBox { Location = new System.Drawing.Point(120, yPos - 3), Width = 200 };

            lblEmail = new Label { Text = "Email:", Location = new System.Drawing.Point(350, yPos), AutoSize = true };
            txtEmail = new TextBox { Location = new System.Drawing.Point(430, yPos - 3), Width = 300 };

            yPos += 40;
            lblRole = new Label { Text = "Role:", Location = new System.Drawing.Point(20, yPos), AutoSize = true };
            cmbRole = new ComboBox { Location = new System.Drawing.Point(120, yPos - 3), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };

            lblBranch = new Label { Text = "Branch:", Location = new System.Drawing.Point(350, yPos), AutoSize = true };
            cmbBranch = new ComboBox { Location = new System.Drawing.Point(430, yPos - 3), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };

            yPos += 40;
            lblStatus = new Label { Text = "Status:", Location = new System.Drawing.Point(20, yPos), AutoSize = true };
            cmbStatus = new ComboBox { Location = new System.Drawing.Point(120, yPos - 3), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.AddRange(new object[] { "Active", "Inactive" });
            cmbStatus.SelectedIndex = 0;

            // Buttons
            yPos += 60;
            btnAdd = new Button { Text = "Add New", Location = new System.Drawing.Point(20, yPos), Width = 100, Height = 35 };
            btnEdit = new Button { Text = "Edit", Location = new System.Drawing.Point(130, yPos), Width = 100, Height = 35 };
            btnDelete = new Button { Text = "Delete", Location = new System.Drawing.Point(240, yPos), Width = 100, Height = 35, BackColor = System.Drawing.Color.Red, ForeColor = System.Drawing.Color.White };
            btnResetPassword = new Button { Text = "Reset Password", Location = new System.Drawing.Point(350, yPos), Width = 120, Height = 35 };
            btnClose = new Button { Text = "Close", Location = new System.Drawing.Point(870, yPos), Width = 100, Height = 35 };

            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnResetPassword.Click += BtnResetPassword_Click;
            btnClose.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] {
                dgvUsers,
                lblUsername, txtUsername, lblFullName, txtFullName,
                lblMobile, txtMobile, lblEmail, txtEmail,
                lblRole, cmbRole, lblBranch, cmbBranch,
                lblStatus, cmbStatus,
                btnAdd, btnEdit, btnDelete, btnResetPassword, btnClose
            });
        }

        private void LoadRoles()
        {
            try
            {
                var roles = _roleRepository.GetAll();
                cmbRole.DataSource = roles;
                cmbRole.DisplayMember = "RoleName";
                cmbRole.ValueMember = "RoleID";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading roles: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadUsers()
        {
            try
            {
                var users = _authService.GetAllUsers();
                dgvUsers.DataSource = users;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvUsers_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                LoadUserDataIntoForm(dgvUsers.Rows[e.RowIndex]);
            }
        }

        private void LoadUserDataIntoForm(DataGridViewRow row)
        {
            if (row.Cells["UserID"].Value != null)
            {
                txtUsername.Text = row.Cells["Username"].Value?.ToString();
                txtFullName.Text = row.Cells["FullName"].Value?.ToString();
                txtMobile.Text = row.Cells["Mobile"].Value?.ToString();
                txtEmail.Text = row.Cells["Email"].Value?.ToString();
                
                // Note: Setting combobox values requires finding the right item
                // This is simplified for demonstration
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Add user functionality - Implement form validation and call AuthService.AddUser()", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a user to edit.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show("Edit user functionality - Implement form validation and call AuthService.UpdateUser()", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a user to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this user?", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                MessageBox.Show("Delete user functionality - Call AuthService.DeleteUser()", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnResetPassword_Click(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a user to reset password.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Are you sure you want to reset this user's password to default?", "Confirm Reset",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                MessageBox.Show("Reset password functionality - Call AuthService.ResetPassword()", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
