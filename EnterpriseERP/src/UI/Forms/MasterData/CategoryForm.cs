using System;
using System.Data;
using System.Windows.Forms;
using EnterpriseInventory.Core.Domain;
using EnterpriseInventory.Core.Interfaces;
using EnterpriseInventory.Infrastructure.Repositories;
using EnterpriseInventory.UI.Helpers;

namespace EnterpriseInventory.UI.Forms.MasterData
{
    public class CategoryForm : Form
    {
        private readonly ICategoryRepository _categoryRepository;
        private DataGridView dgvCategories;
        private TextBox txtSearch;
        private Button btnAdd, btnEdit, btnDelete, btnRefresh;
        private int _currentBranchId = 1; // Default branch

        public CategoryForm()
        {
            _categoryRepository = new CategoryRepository(DbConnectionFactory.CreateConnection());
            InitializeComponent();
            LoadCategories();
        }

        private void InitializeComponent()
        {
            this.Text = "Category Management";
            this.Size = new System.Drawing.Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);

            // Search Panel
            var pnlSearch = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = System.Drawing.Color.FromArgb(240, 240, 240)
            };

            var lblSearch = new Label
            {
                Text = "Search:",
                Location = new System.Drawing.Point(20, 20),
                AutoSize = true
            };

            txtSearch = new TextBox
            {
                Location = new System.Drawing.Point(80, 17),
                Size = new System.Drawing.Size(300, 23)
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;

            btnAdd = new Button
            {
                Text = "Add New",
                Location = new System.Drawing.Point(500, 15),
                Size = new System.Drawing.Size(100, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(0, 120, 215),
                ForeColor = System.Drawing.Color.White
            };
            btnAdd.Click += BtnAdd_Click;

            btnRefresh = new Button
            {
                Text = "Refresh",
                Location = new System.Drawing.Point(770, 15),
                Size = new System.Drawing.Size(100, 30),
                FlatStyle = FlatStyle.Flat
            };
            btnRefresh.Click += (s, e) => LoadCategories();

            pnlSearch.Controls.Add(lblSearch);
            pnlSearch.Controls.Add(txtSearch);
            pnlSearch.Controls.Add(btnAdd);
            pnlSearch.Controls.Add(btnRefresh);

            // Grid
            dgvCategories = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = System.Drawing.Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false
            };
            dgvCategories.CellFormatting += (s, e) =>
            {
                if (e.RowIndex % 2 == 0)
                    e.CellStyle.BackColor = System.Drawing.Color.White;
                else
                    e.CellStyle.BackColor = System.Drawing.Color.FromArgb(250, 250, 250);
            };
            dgvCategories.DoubleClick += DgvCategories_DoubleClick;

            // Columns
            dgvCategories.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CategoryID",
                HeaderText = "ID",
                DataPropertyName = "CategoryID",
                Width = 60
            });
            dgvCategories.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CategoryName",
                HeaderText = "Category Name",
                DataPropertyName = "CategoryName",
                Width = 300
            });
            dgvCategories.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Description",
                HeaderText = "Description",
                DataPropertyName = "Description",
                Width = 400
            });
            dgvCategories.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Status",
                HeaderText = "Status",
                DataPropertyName = "Status",
                Width = 100
            });

            btnEdit = new Button { Text = "Edit", Visible = false };
            btnEdit.Click += BtnEdit_Click;
            btnDelete = new Button { Text = "Delete", Visible = false };
            btnDelete.Click += BtnDelete_Click;

            this.Controls.Add(dgvCategories);
            this.Controls.Add(pnlSearch);
        }

        private void LoadCategories()
        {
            try
            {
                var categories = _categoryRepository.GetAll();
                dgvCategories.DataSource = categories;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading categories: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            var filtered = _categoryRepository.GetAll();
            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                filtered = filtered.Find(c => 
                    c.CategoryName.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase) ||
                    (c.Description != null && c.Description.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase)));
            }
            dgvCategories.DataSource = filtered;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var form = new CategoryDetailForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadCategories();
                }
            }
        }

        private void DgvCategories_DoubleClick(object sender, EventArgs e)
        {
            if (dgvCategories.CurrentRow != null)
            {
                BtnEdit_Click(sender, e);
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvCategories.CurrentRow == null) return;

            var categoryId = Convert.ToInt32(dgvCategories.CurrentRow.Cells["CategoryID"].Value);
            var category = _categoryRepository.GetById(categoryId);

            if (category != null)
            {
                using (var form = new CategoryDetailForm(category))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadCategories();
                    }
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvCategories.CurrentRow == null) return;

            var result = MessageBox.Show(
                "Are you sure you want to delete this category?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    var categoryId = Convert.ToInt32(dgvCategories.CurrentRow.Cells["CategoryID"].Value);
                    _categoryRepository.Delete(categoryId);
                    ShowSuccess("Category deleted successfully");
                    LoadCategories();
                }
                catch (Exception ex)
                {
                    ShowError($"Error deleting category: {ex.Message}");
                }
            }
        }

        private void ShowSuccess(string message)
        {
            MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public class CategoryDetailForm : Form
    {
        private readonly ICategoryRepository _categoryRepository;
        private Category _category;
        private bool _isEditMode;
        private TextBox txtCategoryName, txtDescription;
        private ComboBox cmbStatus;

        public CategoryDetailForm(Category category = null)
        {
            _categoryRepository = new CategoryRepository(DbConnectionFactory.CreateConnection());
            _category = category ?? new Category();
            _isEditMode = category != null && category.CategoryID > 0;
            InitializeComponent();
            if (_isEditMode) LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = _isEditMode ? "Edit Category" : "Add Category";
            this.Size = new System.Drawing.Size(500, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            var lblTitle = new Label
            {
                Text = _isEditMode ? "Edit Category" : "Add New Category",
                Font = new System.Drawing.Font("Arial", 14F, System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(20, 20),
                AutoSize = true
            };

            var lblCategoryName = new Label
            {
                Text = "Category Name *",
                Location = new System.Drawing.Point(20, 70),
                AutoSize = true
            };

            txtCategoryName = new TextBox
            {
                Location = new System.Drawing.Point(20, 95),
                Size = new System.Drawing.Size(440, 23)
            };

            var lblDescription = new Label
            {
                Text = "Description",
                Location = new System.Drawing.Point(20, 130),
                AutoSize = true
            };

            txtDescription = new TextBox
            {
                Location = new System.Drawing.Point(20, 155),
                Size = new System.Drawing.Size(440, 60),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };

            var lblStatus = new Label
            {
                Text = "Status *",
                Location = new System.Drawing.Point(20, 230),
                AutoSize = true
            };

            cmbStatus = new ComboBox
            {
                Location = new System.Drawing.Point(20, 255),
                Size = new System.Drawing.Size(200, 21),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbStatus.Items.Add("Active");
            cmbStatus.Items.Add("Inactive");
            cmbStatus.SelectedIndex = 0;

            var btnSave = new Button
            {
                Text = "Save",
                Location = new System.Drawing.Point(20, 300),
                Size = new System.Drawing.Size(100, 35),
                FlatStyle = FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(0, 120, 215),
                ForeColor = System.Drawing.Color.White
            };
            btnSave.Click += BtnSave_Click;

            var btnCancel = new Button
            {
                Text = "Cancel",
                Location = new System.Drawing.Point(130, 300),
                Size = new System.Drawing.Size(100, 35),
                DialogResult = DialogResult.Cancel
            };

            this.Controls.Add(lblTitle);
            this.Controls.Add(lblCategoryName);
            this.Controls.Add(txtCategoryName);
            this.Controls.Add(lblDescription);
            this.Controls.Add(txtDescription);
            this.Controls.Add(lblStatus);
            this.Controls.Add(cmbStatus);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }

        private void LoadData()
        {
            txtCategoryName.Text = _category.CategoryName;
            txtDescription.Text = _category.Description;
            cmbStatus.SelectedItem = _category.Status ?? "Active";
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCategoryName.Text))
            {
                MessageBox.Show("Category name is required", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCategoryName.Focus();
                return;
            }

            try
            {
                _category.CategoryName = txtCategoryName.Text.Trim();
                _category.Description = txtDescription.Text.Trim();
                _category.Status = cmbStatus.SelectedItem.ToString();

                if (_isEditMode)
                {
                    _categoryRepository.Update(_category);
                    MessageBox.Show("Category updated successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    _categoryRepository.Add(_category);
                    MessageBox.Show("Category created successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving category: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
