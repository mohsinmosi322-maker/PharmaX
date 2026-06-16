using System;
using System.Windows.Forms;
using EnterpriseERP.Application.Services;
using EnterpriseERP.UI.Forms.MasterData;
using EnterpriseERP.UI.Forms.Transactions;
using EnterpriseERP.UI.Forms.Reports;
using EnterpriseERP.UI.Forms.SystemAdmin;

namespace EnterpriseERP.UI.Forms
{
    public partial class MainForm : Form
    {
        private readonly AuthService _authService;
        private readonly FeatureService _featureService;

        public MainForm(AuthService authService, FeatureService featureService)
        {
            InitializeComponent();
            _authService = authService;
            _featureService = featureService;
            
            InitializeMainMenu();
            ApplyPermissions();
        }

        private void InitializeMainMenu()
        {
            this.Text = $"Enterprise ERP - {_authService.CurrentUser?.FullName} ({_authService.CurrentUser?.RoleName})";
            this.WindowState = FormWindowState.Maximized;
            this.IsMdiContainer = true;

            // Create MenuStrip
            var menuStrip = new MenuStrip();
            
            // File Menu
            var fileMenu = new ToolStripMenuItem("File");
            var dashboardItem = new ToolStripMenuItem("Dashboard");
            dashboardItem.Click += (s, e) => OpenForm(typeof(DashboardForm));
            
            var exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += (s, e) => Application.Exit();
            
            fileMenu.DropDownItems.Add(dashboardItem);
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add(exitItem);

            // Master Data Menu
            var masterDataMenu = new ToolStripMenuItem("Master Data");
            
            var categoryItem = new ToolStripMenuItem("Categories");
            categoryItem.Click += (s, e) => OpenForm(typeof(CategoryForm));
            
            var unitItem = new ToolStripMenuItem("Units");
            unitItem.Click += (s, e) => OpenForm(typeof(UnitForm));
            
            var supplierItem = new ToolStripMenuItem("Suppliers");
            supplierItem.Click += (s, e) => OpenForm(typeof(SupplierForm));
            
            var customerItem = new ToolStripMenuItem("Customers");
            customerItem.Click += (s, e) => OpenForm(typeof(CustomerForm));
            
            var productItem = new ToolStripMenuItem("Products");
            productItem.Click += (s, e) => OpenForm(typeof(ProductForm));
            
            var locationItem = new ToolStripMenuItem("Storage Locations");
            locationItem.Click += (s, e) => OpenForm(typeof(LocationForm));
            
            masterDataMenu.DropDownItems.AddRange(new[] { 
                categoryItem, unitItem, locationItem, 
                new ToolStripSeparator(),
                supplierItem, customerItem,
                new ToolStripSeparator(),
                productItem 
            });

            // Transactions Menu
            var transactionsMenu = new ToolStripMenuItem("Transactions");
            
            var openingStockItem = new ToolStripMenuItem("Opening Stock");
            openingStockItem.Click += (s, e) => OpenForm(typeof(OpeningStockForm));
            
            var purchaseItem = new ToolStripMenuItem("Purchase");
            purchaseItem.Click += (s, e) => OpenForm(typeof(PurchaseForm));
            
            var saleItem = new ToolStripMenuItem("Sale");
            saleItem.Click += (s, e) => OpenForm(typeof(SaleForm));
            
            var adjustmentItem = new ToolStripMenuItem("Stock Adjustment");
            adjustmentItem.Click += (s, e) => OpenForm(typeof(StockAdjustmentForm));
            
            transactionsMenu.DropDownItems.AddRange(new[] { 
                openingStockItem, purchaseItem, saleItem, adjustmentItem 
            });

            // Reports Menu
            var reportsMenu = new ToolStripMenuItem("Reports");
            
            var stockReportItem = new ToolStripMenuItem("Current Stock Report");
            stockReportItem.Click += (s, e) => OpenForm(typeof(StockReportForm));
            
            var salesReportItem = new ToolStripMenuItem("Sales Report");
            salesReportItem.Click += (s, e) => OpenForm(typeof(SalesReportForm));
            
            var purchaseReportItem = new ToolStripMenuItem("Purchase Report");
            purchaseReportItem.Click += (s, e) => OpenForm(typeof(PurchaseReportForm));
            
            var ledgerReportItem = new ToolStripMenuItem("Inventory Ledger");
            ledgerReportItem.Click += (s, e) => OpenForm(typeof(InventoryLedgerForm));
            
            reportsMenu.DropDownItems.AddRange(new[] { 
                stockReportItem, salesReportItem, purchaseReportItem, ledgerReportItem 
            });

            // System Menu
            var systemMenu = new ToolStripMenuItem("System");
            
            var userItem = new ToolStripMenuItem("User Management");
            userItem.Click += (s, e) => OpenForm(typeof(UserForm));
            
            var roleItem = new ToolStripMenuItem("Role Management");
            roleItem.Click += (s, e) => OpenForm(typeof(RoleForm));
            
            var featureItem = new ToolStripMenuItem("Feature Management");
            featureItem.Click += (s, e) => OpenForm(typeof(FeatureForm));
            
            var settingsItem = new ToolStripMenuItem("System Settings");
            settingsItem.Click += (s, e) => OpenForm(typeof(SettingsForm));
            
            var backupItem = new ToolStripMenuItem("Backup & Restore");
            backupItem.Click += (s, e) => OpenForm(typeof(BackupForm));
            
            var auditItem = new ToolStripMenuItem("Audit Log");
            auditItem.Click += (s, e) => OpenForm(typeof(AuditLogForm));
            
            systemMenu.DropDownItems.AddRange(new[] { 
                userItem, roleItem, featureItem,
                new ToolStripSeparator(),
                settingsItem, backupItem, auditItem 
            });

            menuStrip.Items.AddRange(new[] { 
                fileMenu, masterDataMenu, transactionsMenu, reportsMenu, systemMenu 
            });

            this.Controls.Add(menuStrip);
        }

        private void ApplyPermissions()
        {
            // Hide features based on permissions and feature flags
            // This is a simplified example - implement full permission checking
        }

        private void OpenForm(Type formType)
        {
            try
            {
                var existingForm = Application.OpenForms[formType.Name];
                if (existingForm != null)
                {
                    existingForm.BringToFront();
                    return;
                }

                var form = (Form)Activator.CreateInstance(formType, _authService, _featureService);
                form.MdiParent = this;
                form.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening form: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
