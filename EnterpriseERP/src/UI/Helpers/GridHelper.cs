using System;
using System.Windows.Forms;

namespace EnterpriseERP.UI.Helpers
{
    public static class GridHelper
    {
        public static DataGridView CreateStandardGrid()
        {
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToOrderRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = System.Drawing.Color.White,
                BorderStyle = BorderStyle.None,
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = System.Drawing.Color.FromArgb(240, 240, 240)
                },
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                RowHeadersVisible = false,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = System.Drawing.Color.FromArgb(0, 102, 153),
                    ForeColor = System.Drawing.Color.White,
                    Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleLeft
                },
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Font = new System.Drawing.Font("Segoe UI", 9F),
                    ForeColor = System.Drawing.Color.Black,
                    Padding = new Padding(5, 3, 5, 3)
                },
                RowTemplate.Height = 35,
                EnableHeadersVisualStyles = false
            };

            return grid;
        }

        public static void AddTextBoxColumn(DataGridView grid, string dataProperty, string headerText, int width = 150)
        {
            var column = new DataGridViewTextBoxColumn
            {
                DataPropertyName = dataProperty,
                HeaderText = headerText,
                Width = width,
                SortMode = DataGridViewColumnSortMode.Automatic
            };
            grid.Columns.Add(column);
        }

        public static void AddDateColumn(DataGridView grid, string dataProperty, string headerText, int width = 120)
        {
            var column = new DataGridViewTextBoxColumn
            {
                DataPropertyName = dataProperty,
                HeaderText = headerText,
                Width = width,
                Format = "dd/MM/yyyy",
                SortMode = DataGridViewColumnSortMode.Automatic
            };
            grid.Columns.Add(column);
        }

        public static void AddDecimalColumn(DataGridView grid, string dataProperty, string headerText, int width = 100)
        {
            var column = new DataGridViewTextBoxColumn
            {
                DataPropertyName = dataProperty,
                HeaderText = headerText,
                Width = width,
                Format = "N2",
                SortMode = DataGridViewColumnSortMode.Automatic
            };
            grid.Columns.Add(column);
        }

        public static void AddButtonColumn(DataGridView grid, string name, string text, int width = 80)
        {
            var buttonColumn = new DataGridViewButtonColumn
            {
                Name = name,
                HeaderText = "",
                Text = text,
                UseColumnTextForButtonValue = true,
                Width = width
            };
            grid.Columns.Add(buttonColumn);
        }
    }
}
