using System;
using System.Windows.Forms;
using EnterpriseInventory.Core.Interfaces;

namespace EnterpriseInventory.UI.Forms.Common
{
    public class BaseForm : Form
    {
        protected Label lblTitle;
        protected Panel pnlHeader;
        protected Panel pnlFooter;
        protected Button btnSave;
        protected Button btnCancel;
        protected Button btnDelete;
        protected ErrorProvider errorProvider;
        protected ToolTip toolTip;

        public BaseForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Size = new System.Drawing.Size(800, 600);
            this.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);

            errorProvider = new ErrorProvider();
            toolTip = new ToolTip();

            // Header Panel
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = System.Drawing.Color.FromArgb(0, 120, 215)
            };

            lblTitle = new Label
            {
                Font = new System.Drawing.Font("Arial", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point),
                ForeColor = System.Drawing.Color.White,
                Location = new System.Drawing.Point(20, 15),
                AutoSize = true,
                Parent = pnlHeader
            };

            // Footer Panel
            pnlFooter = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = System.Drawing.Color.FromArgb(240, 240, 240)
            };

            btnSave = new Button
            {
                Text = "Save",
                Size = new System.Drawing.Size(100, 35),
                Location = new System.Drawing.Point(500, 8),
                FlatStyle = FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(0, 120, 215),
                ForeColor = System.Drawing.Color.White
            };
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button
            {
                Text = "Cancel",
                Size = new System.Drawing.Size(100, 35),
                Location = new System.Drawing.Point(610, 8),
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel
            };

            btnDelete = new Button
            {
                Text = "Delete",
                Size = new System.Drawing.Size(100, 35),
                Location = new System.Drawing.Point(390, 8),
                FlatStyle = FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(201, 48, 48),
                ForeColor = System.Drawing.Color.White,
                Visible = false
            };
            btnDelete.Click += BtnDelete_Click;

            pnlFooter.Controls.Add(btnSave);
            pnlFooter.Controls.Add(btnCancel);
            pnlFooter.Controls.Add(btnDelete);

            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlFooter);

            this.Load += BaseForm_Load;
        }

        protected virtual void BaseForm_Load(object sender, EventArgs e)
        {
            // Override in derived classes
        }

        protected virtual void BtnSave_Click(object sender, EventArgs e)
        {
            if (ValidateChildren(ValidationConstraints.Enabled))
            {
                SaveData();
            }
        }

        protected virtual void BtnDelete_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to delete this record?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                DeleteData();
            }
        }

        protected virtual void SaveData()
        {
            // Override in derived classes
        }

        protected virtual void DeleteData()
        {
            // Override in derived classes
        }

        protected void ShowSuccess(string message)
        {
            MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        protected void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        protected bool ConfirmAction(string message)
        {
            var result = MessageBox.Show(message, "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            return result == DialogResult.Yes;
        }

        protected void SetError(Control control, string errorMessage)
        {
            errorProvider.SetError(control, errorMessage);
        }

        protected void ClearError(Control control)
        {
            errorProvider.SetError(control, string.Empty);
        }

        protected void SetToolTip(Control control, string tooltip)
        {
            toolTip.SetToolTip(control, tooltip);
        }

        protected DataGridView CreateDataGridView()
        {
            var dgv = new DataGridView
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
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = System.Drawing.Color.FromArgb(240, 240, 240)
                },
                RowHeadersVisible = false,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
            };

            dgv.CellFormatting += (s, e) =>
            {
                if (e.RowIndex % 2 == 0)
                {
                    e.CellStyle.BackColor = System.Drawing.Color.White;
                }
                else
                {
                    e.CellStyle.BackColor = System.Drawing.Color.FromArgb(250, 250, 250);
                }
            };

            return dgv;
        }

        protected TextBox CreateTextBox(int x, int y, int width, string labelText, string name, bool required = false)
        {
            var lbl = new Label
            {
                Text = labelText + (required ? " *" : ""),
                Location = new System.Drawing.Point(x, y),
                AutoSize = true
            };

            var txt = new TextBox
            {
                Name = name,
                Location = new System.Drawing.Point(x, y + 25),
                Size = new System.Drawing.Size(width, 23),
                Tag = required ? "Required" : null
            };

            if (required)
            {
                txt.Validating += (s, e) =>
                {
                    if (string.IsNullOrWhiteSpace(txt.Text))
                    {
                        SetError(txt, $"{labelText} is required");
                        e.Cancel = true;
                    }
                    else
                    {
                        ClearError(txt);
                    }
                };
            }

            this.Controls.Add(lbl);
            this.Controls.Add(txt);

            return txt;
        }

        protected ComboBox CreateComboBox(int x, int y, int width, string labelText, string name, bool required = false)
        {
            var lbl = new Label
            {
                Text = labelText + (required ? " *" : ""),
                Location = new System.Drawing.Point(x, y),
                AutoSize = true
            };

            var cmb = new ComboBox
            {
                Name = name,
                Location = new System.Drawing.Point(x, y + 25),
                Size = new System.Drawing.Size(width, 21),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Tag = required ? "Required" : null
            };

            if (required)
            {
                cmb.Validating += (s, e) =>
                {
                    if (cmb.SelectedValue == null || cmb.SelectedValue.ToString() == "0")
                    {
                        SetError(cmb, $"{labelText} is required");
                        e.Cancel = true;
                    }
                    else
                    {
                        ClearError(cmb);
                    }
                };
            }

            this.Controls.Add(lbl);
            this.Controls.Add(cmb);

            return cmb;
        }

        protected CheckBox CreateCheckBox(int x, int y, string text, string name)
        {
            var chk = new CheckBox
            {
                Name = name,
                Text = text,
                Location = new System.Drawing.Point(x, y),
                AutoSize = true
            };

            this.Controls.Add(chk);
            return chk;
        }

        protected DateTimePicker CreateDateTimePicker(int x, int y, string labelText, string name, bool required = false)
        {
            var lbl = new Label
            {
                Text = labelText + (required ? " *" : ""),
                Location = new System.Drawing.Point(x, y),
                AutoSize = true
            };

            var dtp = new DateTimePicker
            {
                Name = name,
                Location = new System.Drawing.Point(x, y + 25),
                Size = new System.Drawing.Size(200, 23),
                Format = DateTimePickerFormat.Short,
                Tag = required ? "Required" : null
            };

            this.Controls.Add(lbl);
            this.Controls.Add(dtp);

            return dtp;
        }

        protected NumericUpDown CreateNumericUpDown(int x, int y, int width, string labelText, string name, decimal min = 0, decimal max = 999999, int decimals = 2, bool required = false)
        {
            var lbl = new Label
            {
                Text = labelText + (required ? " *" : ""),
                Location = new System.Drawing.Point(x, y),
                AutoSize = true
            };

            var nud = new NumericUpDown
            {
                Name = name,
                Location = new System.Drawing.Point(x, y + 25),
                Size = new System.Drawing.Size(width, 23),
                Minimum = min,
                Maximum = max,
                DecimalPlaces = decimals,
                ThousandsSeparator = true,
                Tag = required ? "Required" : null
            };

            if (required)
            {
                nud.Validating += (s, e) =>
                {
                    if (nud.Value <= 0)
                    {
                        SetError(nud, $"{labelText} must be greater than 0");
                        e.Cancel = true;
                    }
                    else
                    {
                        ClearError(nud);
                    }
                };
            }

            this.Controls.Add(lbl);
            this.Controls.Add(nud);

            return nud;
        }

        protected GroupBox CreateGroupBox(int x, int y, int width, int height, string text)
        {
            var gb = new GroupBox
            {
                Text = text,
                Location = new System.Drawing.Point(x, y),
                Size = new System.Drawing.Size(width, height),
                Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point)
            };

            this.Controls.Add(gb);
            return gb;
        }

        protected Label CreateLabel(int x, int y, string text, bool bold = false, System.Drawing.Color? color = null)
        {
            var lbl = new Label
            {
                Text = text,
                Location = new System.Drawing.Point(x, y),
                AutoSize = true,
                Font = new System.Drawing.Font("Arial", bold ? 10F : 9F, bold ? System.Drawing.FontStyle.Bold : System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point)
            };

            if (color.HasValue)
            {
                lbl.ForeColor = color.Value;
            }

            this.Controls.Add(lbl);
            return lbl;
        }

        protected Button CreateButton(int x, int y, string text, EventHandler clickHandler, string backColorName = "Blue")
        {
            var btn = new Button
            {
                Text = text,
                Location = new System.Drawing.Point(x, y),
                Size = new System.Drawing.Size(100, 35),
                FlatStyle = FlatStyle.Flat
            };

            btn.BackColor = backColorName switch
            {
                "Red" => System.Drawing.Color.FromArgb(201, 48, 48),
                "Green" => System.Drawing.Color.FromArgb(76, 175, 80),
                "Orange" => System.Drawing.Color.FromArgb(255, 152, 0),
                _ => System.Drawing.Color.FromArgb(0, 120, 215)
            };

            btn.ForeColor = System.Drawing.Color.White;
            btn.Click += clickHandler;

            this.Controls.Add(btn);
            return btn;
        }
    }
}
