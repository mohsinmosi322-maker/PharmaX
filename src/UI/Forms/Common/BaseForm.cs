using System;
using System.Windows.Forms;
using EnterpriseERP.Application.Services;

namespace EnterpriseERP.UI.Forms.Common
{
    public class BaseForm : Form
    {
        protected readonly AuthService _authService;
        protected readonly FeatureService _featureService;

        public int CurrentUserId { get; protected set; }
        public int CurrentBranchId { get; protected set; }
        public string CurrentUserName { get; protected set; }
        public string CurrentRole { get; protected set; }

        public BaseForm(AuthService authService, FeatureService featureService)
        {
            _authService = authService;
            _featureService = featureService;

            if (_authService.CurrentUser != null)
            {
                CurrentUserId = _authService.CurrentUser.UserID;
                CurrentBranchId = _authService.CurrentUser.BranchID;
                CurrentUserName = _authService.CurrentUser.FullName;
                CurrentRole = _authService.CurrentUser.RoleName;
            }

            InitializeBaseForm();
        }

        private void InitializeBaseForm()
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9F);
            this.MinimumSize = new Size(800, 600);
        }

        protected bool HasPermission(string permissionCode)
        {
            return _authService.HasPermission(permissionCode);
        }

        protected bool IsFeatureEnabled(string featureCode)
        {
            return _featureService.IsFeatureEnabled(featureCode);
        }

        protected void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        protected void ShowSuccess(string message)
        {
            MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        protected bool ConfirmAction(string message)
        {
            return MessageBox.Show(message, "Confirm", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }
    }
}
