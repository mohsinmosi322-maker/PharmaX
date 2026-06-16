using System;
using System.Windows.Forms;
using EnterpriseERP.Application.Services;
using EnterpriseERP.Core.DTOs;

namespace EnterpriseERP.UI.Forms.MasterData
{
    public partial class CategoryForm : BaseForm
    {
        private DataGridView dgvCategories;
        private Button btnAdd, btnEdit, btnDelete, btnRefresh;
        private TextBox txtSearch;

        public CategoryForm(AuthService authService, FeatureService featureService) 
            : base(authService, featureService)
        {
            InitializeComponent();
            LoadCategories();
        }

        private void InitializeComponent()
        {
            this.Text = "Category Management";
            this.Size = new Size(900, 600);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50 };
            var lblTitle = new Label 
            { 
                Text = "Category Management", 
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Location = new Point(10, 10)
            };
            
            txtSearch = new TextBox 
            { 
                Location = new Point(300, 15), 
                Size = new Size(200, 25),
                PlaceholderText = "Search..."
            };
            txtSearch.TextChanged += (s, e) => FilterCategories();

            btnRefresh = new Button 
            { 
                Text = "Refresh", 
                Location = new Point(520, 12),
                Size = new Size(80, 30)
            };
            btnRefresh.Click += (s, e) => LoadCategories();

            btnAdd = new Button 
            { 
                Text = "Add New", 
                Location = new Point(620, 12),
                Size = new Size(80, 30)
            };
            btnAdd.Click += (s, e) => OpenCategoryDialog(null);

            panelTop.Controls.AddRange(new Control[] { lblTitle, txtSearch, btnRefresh, btnAdd });

            dgvCategories = new DataGridView 
            { 
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                ReadOnly = true
            };
            dgvCategories.Columns.Add("CategoryID", "ID");
            dgvCategories.Columns.Add("CategoryName", "Category Name");
            dgvCategories.Columns.Add("Description", "Description");
            dgvCategories.Columns.Add("Status", "Status");

            var contextMenu = new ContextMenuStrip();
            var editItem = new ToolStripMenuItem("Edit");
            editItem.Click += (s, e) => OpenCategoryDialog(GetSelectedCategory());
            var deleteItem = new ToolStripMenuItem("Delete");
            deleteItem.Click += (s, e) => DeleteCategory();
            contextMenu.Items.AddRange(new ToolStripItem[] { editItem, deleteItem });
            dgvCategories.ContextMenuStrip = contextMenu;

            this.Controls.Add(dgvCategories);
            this.Controls.Add(panelTop);
        }

        private void LoadCategories()
        {
            try
            {
                // In real implementation, call service to load categories
                dgvCategories.Rows.Clear();
                
                // Sample data for demonstration
                dgvCategories.Rows.Add(1, "Medicines", "All medicine products", "Active");
                dgvCategories.Rows.Add(2, "Equipment", "Medical equipment", "Active");
                dgvCategories.Rows.Add(3, "Consumables", "Disposable items", "Active");
            }
            catch (Exception ex)
            {
                ShowError($"Error loading categories: {ex.Message}");
            }
        }

        private void FilterCategories()
        {
            // Implement filtering logic
        }

        private CategoryDTO GetSelectedCategory()
        {
            if (dgvCategories.CurrentRow != null)
            {
                return new CategoryDTO
                {
                    CategoryID = Convert.ToInt32(dgvCategories.CurrentRow.Cells["CategoryID"].Value),
                    CategoryName = dgvCategories.CurrentRow.Cells["CategoryName"].Value?.ToString(),
                    Description = dgvCategories.CurrentRow.Cells["Description"].Value?.ToString(),
                    Status = dgvCategories.CurrentRow.Cells["Status"].Value?.ToString()
                };
            }
            return null;
        }

        private void OpenCategoryDialog(CategoryDTO category)
        {
            using (var dialog = new CategoryDialogForm(_authService, _featureService, category))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    LoadCategories();
                    ShowSuccess(category == null ? "Category added successfully" : "Category updated successfully");
                }
            }
        }

        private void DeleteCategory()
        {
            var category = GetSelectedCategory();
            if (category == null)
            {
                ShowError("Please select a category to delete");
                return;
            }

            if (ConfirmAction($"Are you sure you want to delete category '{category.CategoryName}'?"))
            {
                try
                {
                    // Call service to delete
                    LoadCategories();
                    ShowSuccess("Category deleted successfully");
                }
                catch (Exception ex)
                {
                    ShowError($"Error deleting category: {ex.Message}");
                }
            }
        }
    }

    public partial class CategoryDialogForm : Form
    {
        private readonly AuthService _authService;
        private readonly FeatureService _featureService;
        private readonly CategoryDTO _category;

        private TextBox txtName, txtDescription;
        private ComboBox cmbStatus;
        private Button btnSave, btnCancel;

        public CategoryDialogForm(AuthService authService, FeatureService featureService, CategoryDTO category)
        {
            _authService = authService;
            _featureService = featureService;
            _category = category;

            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = _category == null ? "Add Category" : "Edit Category";
            this.Size = new Size(400, 300);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            var lblName = new Label { Text = "Category Name:", Location = new Point(20, 20), Size = new Size(100, 25) };
            txtName = new TextBox { Location = new Point(140, 20), Size = new Size(220, 25) };

            var lblDesc = new Label { Text = "Description:", Location = new Point(20, 60), Size = new Size(100, 25) };
            txtDescription = new TextBox { Location = new Point(140, 60), Size = new Size(220, 25), Multiline = true, Height = 60 };

            var lblStatus = new Label { Text = "Status:", Location = new Point(20, 140), Size = new Size(100, 25) };
            cmbStatus = new ComboBox { Location = new Point(140, 140), Size = new Size(220, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.AddRange(new object[] { "Active", "Inactive" });

            btnSave = new Button { Text = "Save", Location = new Point(140, 190), Size = new Size(80, 30) };
            btnSave.Click += (s, e) => SaveCategory();

            btnCancel = new Button { Text = "Cancel", Location = new Point(240, 190), Size = new Size(80, 30) };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.AddRange(new Control[] { lblName, txtName, lblDesc, txtDescription, lblStatus, cmbStatus, btnSave, btnCancel });
            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }

        private void LoadData()
        {
            if (_category != null)
            {
                txtName.Text = _category.CategoryName;
                txtDescription.Text = _category.Description;
                cmbStatus.SelectedItem = _category.Status;
            }
            else
            {
                cmbStatus.SelectedItem = "Active";
            }
        }

        private void SaveCategory()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Category name is required", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            // In real implementation, call service to save
            this.DialogResult = DialogResult.OK;
        }
    }
}
