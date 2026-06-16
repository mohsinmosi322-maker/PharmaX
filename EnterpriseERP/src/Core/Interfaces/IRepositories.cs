using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EnterpriseERP.Core.Interfaces
{
    /// <summary>
    /// Generic repository interface following Repository Pattern
    /// </summary>
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);
        Task<T> AddAsync(T entity);
        Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);
        void Update(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
    }

    /// <summary>
    /// Unit of Work pattern interface for transaction management
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Branch> Branches { get; }
        IRepository<Role> Roles { get; }
        IRepository<Permission> Permissions { get; }
        IRepository<Feature> Features { get; }
        IRepository<User> Users { get; }
        IRepository<Category> Categories { get; }
        IRepository<Unit> Units { get; }
        IRepository<StorageLocation> StorageLocations { get; }
        IRepository<Supplier> Suppliers { get; }
        IRepository<Customer> Customers { get; }
        IRepository<Manufacturer> Manufacturers { get; }
        IRepository<GenericMedicine> GenericMedicines { get; }
        IRepository<Product> Products { get; }
        IRepository<ProductBatch> ProductBatches { get; }
        IRepository<Purchase> Purchases { get; }
        IRepository<Sale> Sales { get; }
        IRepository<StockAdjustment> StockAdjustments { get; }
        IRepository<InventoryTransaction> InventoryTransactions { get; }
        IRepository<AuditLog> AuditLogs { get; }
        IRepository<SystemSetting> SystemSettings { get; }
        
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }

    /// <summary>
    /// Authentication service interface
    /// </summary>
    public interface IAuthenticationService
    {
        Task<LoginResult> LoginAsync(string username, string password, string ipAddress, string machineName);
        Task LogoutAsync(int userId);
        Task<bool> ValidateSessionAsync(int userId);
        Task<User?> GetCurrentUserAsync(int userId);
        Task<IEnumerable<string>> GetUserPermissionsAsync(int userId);
        Task<bool> HasPermissionAsync(int userId, string permissionCode);
    }

    /// <summary>
    /// Login result
    /// </summary>
    public class LoginResult
    {
        public bool Success { get; set; }
        public User? User { get; set; }
        public string? Message { get; set; }
        public IEnumerable<string>? Permissions { get; set; }
    }

    /// <summary>
    /// Feature flag service interface
    /// </summary>
    public interface IFeatureService
    {
        Task<bool> IsEnabledAsync(string featureCode);
        Task<IEnumerable<Feature>> GetAllFeaturesAsync();
        Task<Feature?> GetFeatureByCodeAsync(string featureCode);
        Task EnableFeatureAsync(string featureCode);
        Task DisableFeatureAsync(string featureCode);
    }

    /// <summary>
    /// Inventory engine interface - core transaction-based inventory
    /// </summary>
    public interface IInventoryEngine
    {
        Task<InventoryResult> ProcessOpeningStockAsync(OpeningStockData data, int userId);
        Task<InventoryResult> ProcessPurchaseAsync(Purchase purchase, int userId);
        Task<InventoryResult> ProcessSaleAsync(Sale sale, int userId);
        Task<InventoryResult> ProcessStockAdjustmentAsync(StockAdjustment adjustment, int userId);
        Task<InventoryResult> ProcessTransferOutAsync(InventoryTransferData data, int userId);
        Task<InventoryResult> ProcessTransferInAsync(InventoryTransferData data, int userId);
        Task<decimal> GetCurrentStockAsync(int productId, int? batchId = null, int? branchId = null);
        Task<bool> CheckStockAvailabilityAsync(int productId, decimal quantity, int? batchId = null, int? branchId = null);
        Task<IEnumerable<BatchWithQuantity>> GetAvailableBatchesAsync(int productId, int branchId);
    }

    public class OpeningStockData
    {
        public int BranchId { get; set; }
        public int ProductId { get; set; }
        public int? BatchId { get; set; }
        public decimal Quantity { get; set; }
        public decimal CostPrice { get; set; }
        public DateTime OpeningDate { get; set; }
        public string? Remarks { get; set; }
    }

    public class InventoryTransferData
    {
        public int TransferId { get; set; }
        public int FromBranchId { get; set; }
        public int ToBranchId { get; set; }
        public int ProductId { get; set; }
        public int BatchId { get; set; }
        public decimal Quantity { get; set; }
        public decimal CostPrice { get; set; }
        public string? Remarks { get; set; }
    }

    public class InventoryResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public long? TransactionId { get; set; }
        public Exception? Error { get; set; }
    }

    public class BatchWithQuantity
    {
        public int BatchId { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public DateTime? ExpiryDate { get; set; }
        public decimal Quantity { get; set; }
        public decimal CostPrice { get; set; }
    }

    /// <summary>
    /// Audit logging service
    /// </summary>
    public interface IAuditService
    {
        Task LogActionAsync(int userId, string action, string tableName, int? recordId, 
                           string? oldValues, string? newValues, string? description,
                           string? ipAddress, string? machineName);
        Task<IEnumerable<AuditLog>> GetUserActionsAsync(int userId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<IEnumerable<AuditLog>> GetTableActionsAsync(string tableName, DateTime? fromDate = null, DateTime? toDate = null);
        Task<IEnumerable<AuditLog>> GetRecordActionsAsync(string tableName, int recordId);
    }

    /// <summary>
    /// Reporting service interface
    /// </summary>
    public interface IReportingService
    {
        Task<DashboardStats> GetDashboardStatsAsync(int? branchId = null);
        Task<IEnumerable<StockReportItem>> GetCurrentStockReportAsync(int? branchId = null, int? categoryId = null);
        Task<IEnumerable<StockReportItem>> GetLowStockReportAsync(int? branchId = null);
        Task<IEnumerable<ExpiryReportItem>> GetNearExpiryReportAsync(int daysThreshold = 90);
        Task<IEnumerable<ExpiryReportItem>> GetExpiredStockReportAsync();
        Task<IEnumerable<SalesSummaryItem>> GetSalesSummaryAsync(DateTime fromDate, DateTime toDate, int? branchId = null);
        Task<IEnumerable<PurchaseSummaryItem>> GetPurchaseSummaryAsync(DateTime fromDate, DateTime toDate, int? branchId = null);
        Task<IEnumerable<InventoryTransaction>> GetProductLedgerAsync(int productId, DateTime? fromDate = null, DateTime? toDate = null);
    }

    public class DashboardStats
    {
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalSuppliers { get; set; }
        public int TotalCustomers { get; set; }
        public decimal StockValue { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }
        public int NearExpiryCount { get; set; }
        public int ExpiredCount { get; set; }
        public decimal TodaysPurchases { get; set; }
        public decimal TodaysSales { get; set; }
    }

    public class StockReportItem
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string? Barcode { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string UnitName { get; set; } = string.Empty;
        public int BranchId { get; set; }
        public string BranchCode { get; set; } = string.Empty;
        public int? BatchId { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public DateTime? ExpiryDate { get; set; }
        public decimal CurrentQuantity { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SalePrice { get; set; }
        public decimal StockValue { get; set; }
        public string StockStatus { get; set; } = string.Empty;
    }

    public class ExpiryReportItem
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int BatchId { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public DateTime? ExpiryDate { get; set; }
        public decimal Quantity { get; set; }
        public int DaysUntilExpiry { get; set; }
        public string BranchCode { get; set; } = string.Empty;
    }

    public class SalesSummaryItem
    {
        public DateTime Date { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
        public decimal NetAmount { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
    }

    public class PurchaseSummaryItem
    {
        public DateTime Date { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
        public decimal NetAmount { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Backup service interface
    /// </summary>
    public interface IBackupService
    {
        Task<BackupResult> CreateBackupAsync(string backupPath, int userId);
        Task<BackupResult> RestoreBackupAsync(string backupPath);
        Task<IEnumerable<BackupHistory>> GetBackupHistoryAsync();
        Task DeleteBackupAsync(int backupId);
    }

    public class BackupResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? BackupPath { get; set; }
        public decimal? BackupSize { get; set; }
        public Exception? Error { get; set; }
    }

    public class BackupHistory
    {
        public int BackupId { get; set; }
        public string BackupPath { get; set; } = string.Empty;
        public decimal? BackupSize { get; set; }
        public DateTime BackupDate { get; set; }
        public string BackupType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
    }
}
