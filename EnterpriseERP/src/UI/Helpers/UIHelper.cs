using System;
using System.Drawing;
using System.Windows.Forms;

namespace EnterpriseERP.UI.Helpers
{
    public static class UIHelper
    {
        public static Button CreateStandardButton(string text, Color? backColor = null)
        {
            var button = new Button
            {
                Text = text,
                FlatStyle = FlatStyle.Flat,
                BackColor = backColor ?? Color.FromArgb(0, 102, 153),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Size = new Size(100, 35),
                Cursor = Cursors.Hand,
                FlatAppearance =
                {
                    BorderSize = 0,
                    MouseOverBackColor = Color.FromArgb(0, 120, 180),
                    MouseDownBackColor = Color.FromArgb(0, 80, 120)
                }
            };

            return button;
        }

        public static TextBox CreateStandardTextBox()
        {
            var textBox = new TextBox
            {
                Font = new Font("Segoe UI", 9F),
                Padding = new Padding(5),
                BorderStyle = BorderStyle.FixedSingle,
                Size = new Size(200, 30)
            };

            return textBox;
        }

        public static Label CreateStandardLabel(string text, bool isBold = false)
        {
            var label = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9F, isBold ? FontStyle.Bold : FontStyle.Regular),
                AutoSize = true
            };

            return label;
        }

        public static ComboBox CreateStandardComboBox()
        {
            var comboBox = new ComboBox
            {
                Font = new Font("Segoe UI", 9F),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Size = new Size(200, 30),
                FlatStyle = FlatStyle.Flat
            };

            return comboBox;
        }

        public static DateTimePicker CreateStandardDatePicker()
        {
            var datePicker = new DateTimePicker
            {
                Font = new Font("Segoe UI", 9F),
                Format = DateTimePickerFormat.Short,
                Size = new Size(150, 30),
                CalendarFont = new Font("Segoe UI", 9F)
            };

            return datePicker;
        }

        public static Panel CreateHeaderPanel(string title)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(0, 102, 153)
            };

            var label = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 15)
            };

            panel.Controls.Add(label);
            return panel;
        }

        public static void ShowSuccessMessage(string message)
        {
            MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void ShowWarningMessage(string message)
        {
            MessageBox.Show(message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static DialogResult ShowConfirmDialog(string message)
        {
            return MessageBox.Show(message, "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }
    }
}
