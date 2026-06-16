using System;
using System.Configuration;

namespace EnterpriseERP.Core.Configuration
{
    public static class AppSettings
    {
        private static string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString 
                ?? "Server=localhost;Database=EnterpriseERP;Trusted_Connection=True;";
        }

        public static string ConnectionString => GetConnectionString();
        
        public static int SessionTimeoutMinutes => 
            int.Parse(ConfigurationManager.AppSettings["SessionTimeoutMinutes"] ?? "30");
            
        public static int MaxLoginAttempts => 
            int.Parse(ConfigurationManager.AppSettings["MaxLoginAttempts"] ?? "5");
            
        public static string CompanyName => 
            ConfigurationManager.AppSettings["CompanyName"] ?? "Enterprise ERP";
            
        public static string DefaultCurrency => 
            ConfigurationManager.AppSettings["DefaultCurrency"] ?? "USD";
            
        public static string BackupPath => 
            ConfigurationManager.AppSettings["BackupPath"] ?? @"C:\ERPBackups\";
            
        public static int LowStockThreshold => 
            int.Parse(ConfigurationManager.AppSettings["LowStockThreshold"] ?? "10");
            
        public static int NearExpiryDays => 
            int.Parse(ConfigurationManager.AppSettings["NearExpiryDays"] ?? "30");
    }
}
