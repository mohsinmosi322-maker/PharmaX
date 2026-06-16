using System;
using System.Collections.Generic;
using System.Windows.Forms;
using EnterpriseInventory.Core.Domain;
using EnterpriseInventory.Core.Interfaces;

namespace EnterpriseInventory.UI.Forms
{
    public partial class LoginForm : Form
    {
        private readonly IAuthService _authService;
        
        public User LoggedInUser { get; private set; }

        public LoginForm(IAuthService authService)
        {
            InitializeComponent();
            _authService = authService;
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Enterprise ERP - Login";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Size = new System.Drawing.Size(400, 300);

            var lblTitle = new Label
            {
                Text = "Enterprise Inventory System",
                Font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(50, 30),
                AutoSize = true
            };

            var lblUsername = new Label
            {
                Text = "Username:",
                Location = new System.Drawing.Point(50, 80),
                AutoSize = true
            };

            txtUsername = new TextBox
            {
                Location = new System.Drawing.Point(50, 100),
                Size = new System.Drawing.Size(280, 23),
                TabIndex = 0
            };

            var lblPassword = new Label
            {
                Text = "Password:",
                Location = new System.Drawing.Point(50, 130),
                AutoSize = true
            };

            txtPassword = new TextBox
            {
                Location = new System.Drawing.Point(50, 150),
                Size = new System.Drawing.Size(280, 23),
                PasswordChar = '*',
                TabIndex = 1
            };

            btnLogin = new Button
            {
                Text = "Login",
                Location = new System.Drawing.Point(50, 190),
                Size = new System.Drawing.Size(120, 35),
                TabIndex = 2
            };
            btnLogin.Click += BtnLogin_Click;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new System.Drawing.Point(210, 190),
                Size = new System.Drawing.Size(120, 35),
                TabIndex = 3,
                DialogResult = DialogResult.Cancel
            };

            lblError = new Label
            {
                ForeColor = System.Drawing.Color.Red,
                Location = new System.Drawing.Point(50, 230),
                AutoSize = true,
                Visible = false
            };

            this.Controls.Add(lblTitle);
            this.Controls.Add(lblUsername);
            this.Controls.Add(txtUsername);
            this.Controls.Add(lblPassword);
            this.Controls.Add(txtPassword);
            this.Controls.Add(btnLogin);
            this.Controls.Add(btnCancel);
            this.Controls.Add(lblError);

            this.AcceptButton = btnLogin;
            this.CancelButton = btnCancel;
        }

        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnCancel;
        private Label lblError;

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(username))
            {
                ShowError("Please enter username");
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowError("Please enter password");
                return;
            }

            try
            {
                btnLogin.Enabled = false;
                btnLogin.Text = "Logging in...";

                // In production, use async/await properly with Task.Run for DB operations
                LoggedInUser = _authService.Login(username, password);

                if (LoggedInUser != null)
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                btnLogin.Enabled = true;
                btnLogin.Text = "Login";
            }
        }

        private void ShowError(string message)
        {
            lblError.Text = message;
            lblError.Visible = true;
        }
    }

    public partial class MainForm : Form
    {
        private readonly IAuthService _authService;
        private readonly IFeatureRepository _featureRepository;
        private User _currentUser;
        private Panel _sidePanel;
        private Panel _contentPanel;

        public MainForm(IAuthService authService, IFeatureRepository featureRepository, User user)
        {
            _authService = authService;
            _featureRepository = featureRepository;
            _currentUser = user;
            
            InitializeComponent();
            InitializeMainMenu();
            LoadDashboard();
        }

        private void InitializeComponent()
        {
            this.Text = $"Enterprise ERP - {_currentUser.FullName}";
            this.WindowState = FormWindowState.Maximized;
            this.IsMdiContainer = true;
        }

        private void InitializeMainMenu()
        {
            // Side Panel
            _sidePanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 200,
                BackColor = System.Drawing.Color.FromArgb(45, 45, 48)
            };

            // Logo/Title
            var lblTitle = new Label
            {
                Text = "ERP System",
                Font = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.White,
                Location = new System.Drawing.Point(10, 20),
                AutoSize = true,
                Parent = _sidePanel
            };

            // User Info
            var lblUser = new Label
            {
                Text = _currentUser.FullName,
                ForeColor = System.Drawing.Color.LightGray,
                Location = new System.Drawing.Point(10, 50),
                AutoSize = true,
                Parent = _sidePanel
            };

            int yPos = 100;

            // Dashboard Button
            CreateMenuButton("Dashboard", yPos, (s, e) => LoadDashboard());
            yPos += 40;

            // Products Button
            CreateMenuButton("Products", yPos, (s, e) => ShowForm(new ProductListForm()));
            yPos += 40;

            // Purchases Button
            if (_featureRepository.IsEnabled("PurchaseModule"))
            {
                CreateMenuButton("Purchases", yPos, (s, e) => ShowForm(new PurchaseForm()));
                yPos += 40;
            }

            // Sales Button
            if (_featureRepository.IsEnabled("SalesModule"))
            {
                CreateMenuButton("Sales", yPos, (s, e) => OpenSalesForm());
                yPos += 40;
            }

            // Inventory Button
            CreateMenuButton("Inventory", yPos, (s, e) => ShowForm(new InventoryForm()));
            yPos += 40;

            // Reports Button
            CreateMenuButton("Reports", yPos, (s, e) => ShowForm(new ReportsForm()));
            yPos += 40;

            // Settings (Admin only)
            if (_currentUser.RoleID <= 2)
            {
                CreateMenuButton("Settings", yPos, (s, e) => ShowForm(new SettingsForm()));
                yPos += 40;
            }

            // Logout Button
            yPos = _sidePanel.Height - 50;
            var btnLogout = new Button
            {
                Text = "Logout",
                Location = new System.Drawing.Point(10, yPos),
                Size = new System.Drawing.Size(180, 35),
                Parent = _sidePanel,
                FlatStyle = FlatStyle.Flat
            };
            btnLogout.BackColor = System.Drawing.Color.FromArgb(201, 48, 48);
            btnLogout.ForeColor = System.Drawing.Color.White;
            btnLogout.Click += (s, e) => Logout();

            // Content Panel
            _contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = System.Drawing.Color.White
            };

            this.Controls.Add(_contentPanel);
            this.Controls.Add(_sidePanel);
        }

        private void CreateMenuButton(string text, int y, EventHandler clickHandler)
        {
            var btn = new Button
            {
                Text = text,
                Location = new System.Drawing.Point(10, y),
                Size = new System.Drawing.Size(180, 35),
                Parent = _sidePanel,
                FlatStyle = FlatStyle.Flat,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            btn.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
            btn.ForeColor = System.Drawing.Color.White;
            btn.Click += clickHandler;
        }

        private void LoadDashboard()
        {
            _contentPanel.Controls.Clear();
            
            var dashboard = new DashboardView(_currentUser.BranchID);
            dashboard.Dock = DockStyle.Fill;
            _contentPanel.Controls.Add(dashboard);
        }

        private void ShowForm(Form form)
        {
            form.MdiParent = this;
            form.Show();
        }

        private void OpenSalesForm()
        {
            // Check feature flag
            if (!_featureRepository.IsEnabled("SalesModule"))
            {
                MessageBox.Show("Sales module is not enabled.", "Feature Disabled", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var saleForm = new SaleForm();
            saleForm.MdiParent = this;
            saleForm.Show();
        }

        private void Logout()
        {
            _authService.Logout(_currentUser.UserID);
            this.Close();
        }
    }

    public partial class DashboardView : UserControl
    {
        private readonly int _branchId;

        public DashboardView(int branchId)
        {
            _branchId = branchId;
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.AutoScroll = true;
        }

        private void LoadData()
        {
            // This would call ReportingService.GetDashboardData()
            // For now, show placeholder
            var lbl = new Label
            {
                Text = "Dashboard - Branch ID: " + _branchId,
                Font = new System.Drawing.Font("Arial", 16, System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(20, 20),
                AutoSize = true
            };
            this.Controls.Add(lbl);

            // Add stat cards
            int x = 20, y = 70;
            CreateStatCard("Total Products", "0", x, y);
            CreateStatCard("Stock Value", "$0.00", x + 200, y);
            CreateStatCard("Low Stock", "0", x + 400, y);
            CreateStatCard("Today's Sales", "$0.00", x, y + 120);
            CreateStatCard("Today's Purchases", "$0.00", x + 200, y + 120);
            CreateStatCard("Near Expiry", "0", x + 400, y + 120);
        }

        private void CreateStatCard(string title, string value, int x, int y)
        {
            var panel = new Panel
            {
                Location = new System.Drawing.Point(x, y),
                Size = new System.Drawing.Size(180, 100),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = System.Drawing.Color.FromArgb(245, 245, 245)
            };

            var lblTitle = new Label
            {
                Text = title,
                Font = new System.Drawing.Font("Arial", 10),
                Location = new System.Drawing.Point(10, 10),
                AutoSize = false,
                Size = new System.Drawing.Size(160, 30),
                Parent = panel
            };

            var lblValue = new Label
            {
                Text = value,
                Font = new System.Drawing.Font("Arial", 18, System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(10, 45),
                AutoSize = true,
                Parent = panel,
                ForeColor = System.Drawing.Color.FromArgb(0, 120, 215)
            };

            this.Controls.Add(panel);
        }
    }

    // Placeholder forms for other modules
    public partial class ProductListForm : Form
    {
        public ProductListForm()
        {
            this.Text = "Product Management";
            this.Size = new System.Drawing.Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            
            var lbl = new Label
            {
                Text = "Product List - Coming Soon",
                Font = new System.Drawing.Font("Arial", 14),
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lbl);
        }
    }

    public partial class PurchaseForm : Form
    {
        public PurchaseForm()
        {
            this.Text = "Purchase Entry";
            this.Size = new System.Drawing.Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            
            var lbl = new Label
            {
                Text = "Purchase Entry Form - Coming Soon",
                Font = new System.Drawing.Font("Arial", 14),
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lbl);
        }
    }

    public partial class SaleForm : Form
    {
        public SaleForm()
        {
            this.Text = "Point of Sale";
            this.Size = new System.Drawing.Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            
            var lbl = new Label
            {
                Text = "POS / Sale Entry Form - Coming Soon",
                Font = new System.Drawing.Font("Arial", 14),
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lbl);
        }
    }

    public partial class InventoryForm : Form
    {
        public InventoryForm()
        {
            this.Text = "Inventory Management";
            this.Size = new System.Drawing.Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            
            var lbl = new Label
            {
                Text = "Inventory Ledger & Adjustments - Coming Soon",
                Font = new System.Drawing.Font("Arial", 14),
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lbl);
        }
    }

    public partial class ReportsForm : Form
    {
        public ReportsForm()
        {
            this.Text = "Reports";
            this.Size = new System.Drawing.Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            
            var lbl = new Label
            {
                Text = "Reports Module - Coming Soon",
                Font = new System.Drawing.Font("Arial", 14),
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lbl);
        }
    }

    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            this.Text = "System Settings";
            this.Size = new System.Drawing.Size(800, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            
            var lbl = new Label
            {
                Text = "Settings & Configuration - Coming Soon",
                Font = new System.Drawing.Font("Arial", 14),
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lbl);
        }
    }
}
