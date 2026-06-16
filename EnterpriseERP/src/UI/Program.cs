using System;
using System.Windows.Forms;
using System.Configuration;
using EnterpriseInventory.Core.Interfaces;
using EnterpriseInventory.Infrastructure.Data;
using EnterpriseInventory.Application.Services;
using EnterpriseInventory.UI.Forms;

namespace EnterpriseInventory
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                // Get connection string from config
                string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString;

                if (string.IsNullOrEmpty(connectionString))
                {
                    MessageBox.Show("Database connection string not found in configuration.", "Configuration Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Create database connection
                var connection = DbConnectionFactory.CreateConnection(connectionString);
                connection.Open();

                // Initialize Unit of Work
                var unitOfWork = new UnitOfWork(connection);

                // Initialize Services
                IAuthService authService = new AuthenticationService(unitOfWork);
                IFeatureRepository featureRepository = unitOfWork.Features;

                // Show Login Form
                var loginForm = new LoginForm(authService);
                
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    var user = loginForm.LoggedInUser;
                    
                    if (user != null)
                    {
                        // Show Main Form
                        var mainForm = new MainForm(authService, featureRepository, user);
                        Application.Run(mainForm);
                    }
                }

                connection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Application Error: {ex.Message}\n\nPlease check your database connection and ensure the database is properly initialized.",
                    "Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                // Log error to file (simplified)
                try
                {
                    System.IO.File.AppendAllText("error.log", 
                        $"{DateTime.Now}: {ex.ToString()}{Environment.NewLine}");
                }
                catch { }
            }
        }
    }
}
