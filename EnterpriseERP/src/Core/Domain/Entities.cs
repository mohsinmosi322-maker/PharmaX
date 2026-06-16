using System;
using System.Collections.Generic;

namespace EnterpriseERP.Core.Domain
{
    /// <summary>
    /// Base entity with common properties
    /// </summary>
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Branch entity for multi-branch support
    /// </summary>
    public class Branch : BaseEntity
    {
        public string BranchCode { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        
        // Navigation properties
        public virtual ICollection<User> Users { get; set; } = new List<User>();
        public virtual ICollection<ProductBatch> ProductBatches { get; set; } = new List<ProductBatch>();
        public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
        public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
        public virtual ICollection<StockAdjustment> StockAdjustments { get; set; } = new List<StockAdjustment>();
    }

    /// <summary>
    /// Role entity for RBAC
    /// </summary>
    public class Role : BaseEntity
    {
        public string RoleCode { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        
        // Navigation properties
        public virtual ICollection<User> Users { get; set; } = new List<User>();
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }

    /// <summary>
    /// Permission entity for fine-grained access control
    /// </summary>
    public class Permission : BaseEntity
    {
        public string PermissionCode { get; set; } = string.Empty;
        public string PermissionName { get; set; } = string.Empty;
        public string? ModuleName { get; set; }
        public string? Description { get; set; }
        
        // Navigation properties
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }

    /// <summary>
    /// Role-Permission mapping
    /// </summary>
    public class RolePermission
    {
        public int RolePermissionId { get; set; }
        public int RoleId { get; set; }
        public int PermissionId { get; set; }
        
        // Navigation properties
        public virtual Role Role { get; set; } = null!;
        public virtual Permission Permission { get; set; } = null!;
    }

    /// <summary>
    /// Feature flag entity for dynamic feature management
    /// </summary>
    public class Feature
    {
        public int FeatureId { get; set; }
        public string FeatureCode { get; set; } = string.Empty;
        public string FeatureName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// User entity with security features
    /// </summary>
    public class User : BaseEntity
    {
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Salt { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public int RoleId { get; set; }
        public int BranchId { get; set; }
        public DateTime? LastLoginDate { get; set; }
        
        // Navigation properties
        public virtual Role Role { get; set; } = null!;
        public virtual Branch Branch { get; set; } = null!;
        public virtual ICollection<LoginAudit> LoginAudits { get; set; } = new List<LoginAudit>();
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }

    /// <summary>
    /// Login audit trail
    /// </summary>
    public class LoginAudit
    {
        public int AuditId { get; set; }
        public int UserId { get; set; }
        public DateTime LoginTime { get; set; }
        public DateTime? LogoutTime { get; set; }
        public string? IPAddress { get; set; }
        public string? MachineName { get; set; }
        public bool Success { get; set; }
        public string? FailureReason { get; set; }
        
        // Navigation properties
        public virtual User User { get; set; } = null!;
    }

    /// <summary>
    /// Product category
    /// </summary>
    public class Category : BaseEntity
    {
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
        
        // Navigation properties
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }

    /// <summary>
    /// Unit of measure
    /// </summary>
    public class Unit : BaseEntity
    {
        public string UnitName { get; set; } = string.Empty;
        public string? Symbol { get; set; }
        
        // Navigation properties
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }

    /// <summary>
    /// Storage location within a branch
    /// </summary>
    public class StorageLocation : BaseEntity
    {
        public string LocationCode { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int BranchId { get; set; }
        
        // Navigation properties
        public virtual Branch Branch { get; set; } = null!;
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }

    /// <summary>
    /// Supplier entity
    /// </summary>
    public class Supplier : BaseEntity
    {
        public string SupplierCode { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public string? ContactPerson { get; set; }
        public string? Mobile { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string Country { get; set; } = "Pakistan";
        public decimal OpeningBalance { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal? CreditLimit { get; set; }
        
        // Navigation properties
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
        public virtual ICollection<SupplierLedger> LedgerEntries { get; set; } = new List<SupplierLedger>();
    }

    /// <summary>
    /// Customer entity
    /// </summary>
    public class Customer : BaseEntity
    {
        public string CustomerCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string? Mobile { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string Country { get; set; } = "Pakistan";
        public decimal OpeningBalance { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal? CreditLimit { get; set; }
        
        // Navigation properties
        public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
        public virtual ICollection<CustomerLedger> LedgerEntries { get; set; } = new List<CustomerLedger>();
    }

    /// <summary>
    /// Manufacturer (Pharmacy module)
    /// </summary>
    public class Manufacturer : BaseEntity
    {
        public string ManufacturerCode { get; set; } = string.Empty;
        public string ManufacturerName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? LicenseNumber { get; set; }
        
        // Navigation properties
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }

    /// <summary>
    /// Generic medicine (Pharmacy module)
    /// </summary>
    public class GenericMedicine : BaseEntity
    {
        public string GenericName { get; set; } = string.Empty;
        public string? Description { get; set; }
        
        // Navigation properties
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }

    /// <summary>
    /// Product master with pharmacy-specific fields
    /// </summary>
    public class Product : BaseEntity
    {
        public string ProductCode { get; set; } = string.Empty;
        public string? Barcode { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int? GenericId { get; set; }
        public int? ManufacturerId { get; set; }
        public int CategoryId { get; set; }
        public int? SupplierId { get; set; }
        public int UnitId { get; set; }
        public int? LocationId { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SalePrice { get; set; }
        public decimal MinimumStock { get; set; }
        public decimal ReorderLevel { get; set; }
        public string? Description { get; set; }
        
        // Pharmacy-specific fields
        public string? DrugSchedule { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? Strength { get; set; }
        public string? DosageForm { get; set; }
        public bool ControlledDrugFlag { get; set; }
        
        public string Status { get; set; } = "Active"; // Active, Inactive, Discontinued
        
        // Navigation properties
        public virtual GenericMedicine? GenericMedicine { get; set; }
        public virtual Manufacturer? Manufacturer { get; set; }
        public virtual Category Category { get; set; } = null!;
        public virtual Supplier? Supplier { get; set; }
        public virtual Unit Unit { get; set; } = null!;
        public virtual StorageLocation? Location { get; set; }
        public virtual ICollection<ProductBatch> Batches { get; set; } = new List<ProductBatch>();
        public virtual ICollection<PurchaseDetail> PurchaseDetails { get; set; } = new List<PurchaseDetail>();
        public virtual ICollection<SaleDetail> SaleDetails { get; set; } = new List<SaleDetail>();
    }

    /// <summary>
    /// Product batch with expiry tracking
    /// </summary>
    public class ProductBatch
    {
        public int BatchId { get; set; }
        public int ProductId { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public DateTime? ManufacturingDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal Quantity { get; set; }
        public decimal CostPrice { get; set; }
        public int BranchId { get; set; }
        public bool Status { get; set; } = true;
        public DateTime CreatedDate { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
        
        // Navigation properties
        public virtual Product Product { get; set; } = null!;
        public virtual Branch Branch { get; set; } = null!;
        public virtual ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();
        public virtual ICollection<SaleDetail> SaleDetails { get; set; } = new List<SaleDetail>();
    }

    /// <summary>
    /// Opening stock record
    /// </summary>
    public class OpeningStock
    {
        public int OpeningId { get; set; }
        public int BranchId { get; set; }
        public int ProductId { get; set; }
        public int? BatchId { get; set; }
        public decimal Quantity { get; set; }
        public decimal CostPrice { get; set; }
        public DateTime OpeningDate { get; set; }
        public string? Remarks { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedDate { get; set; }
        
        // Navigation properties
        public virtual Branch Branch { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
        public virtual ProductBatch? Batch { get; set; }
        public virtual User User { get; set; } = null!;
    }

    /// <summary>
    /// Core inventory transaction - heart of the system
    /// </summary>
    public class InventoryTransaction
    {
        public long TransactionId { get; set; }
        public int BranchId { get; set; }
        public int ProductId { get; set; }
        public int? BatchId { get; set; }
        public string TransactionType { get; set; } = string.Empty; // OpeningStock, Purchase, Sale, etc.
        public string? ReferenceType { get; set; } // PurchaseID, SaleID, etc.
        public int? ReferenceId { get; set; }
        public decimal QtyIn { get; set; }
        public decimal QtyOut { get; set; }
        public decimal BalanceAfterTransaction { get; set; }
        public decimal UnitCost { get; set; }
        public DateTime TransactionDate { get; set; }
        public int UserId { get; set; }
        public string? Remarks { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
        
        // Navigation properties
        public virtual Branch Branch { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
        public virtual ProductBatch? Batch { get; set; }
        public virtual User User { get; set; } = null!;
    }

    /// <summary>
    /// Purchase header
    /// </summary>
    public class Purchase
    {
        public int PurchaseId { get; set; }
        public int BranchId { get; set; }
        public int SupplierId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime PurchaseDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
        public decimal NetAmount { get; set; }
        public string? Remarks { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Approved, Cancelled
        public int UserId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
        
        // Navigation properties
        public virtual Branch Branch { get; set; } = null!;
        public virtual Supplier Supplier { get; set; } = null!;
        public virtual User User { get; set; } = null!;
        public virtual ICollection<PurchaseDetail> Details { get; set; } = new List<PurchaseDetail>();
    }

    /// <summary>
    /// Purchase detail line item
    /// </summary>
    public class PurchaseDetail
    {
        public int PurchaseDetailId { get; set; }
        public int PurchaseId { get; set; }
        public int ProductId { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public DateTime? ExpiryDate { get; set; }
        public DateTime? ManufacturingDate { get; set; }
        public decimal Quantity { get; set; }
        public decimal BonusQuantity { get; set; }
        public decimal CostPrice { get; set; }
        public decimal LineTotal { get; set; }
        public decimal ReceivedQuantity { get; set; }
        public string Status { get; set; } = "Pending";
        
        // Navigation properties
        public virtual Purchase Purchase { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
    }

    /// <summary>
    /// Sale header
    /// </summary>
    public class Sale
    {
        public int SaleId { get; set; }
        public int BranchId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public int? CustomerId { get; set; }
        public DateTime SaleDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
        public decimal NetAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DueAmount { get; set; }
        public string PaymentStatus { get; set; } = "Paid"; // Paid, Partial, Unpaid
        public string? Remarks { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
        
        // Navigation properties
        public virtual Branch Branch { get; set; } = null!;
        public virtual Customer? Customer { get; set; }
        public virtual User User { get; set; } = null!;
        public virtual ICollection<SaleDetail> Details { get; set; } = new List<SaleDetail>();
    }

    /// <summary>
    /// Sale detail line item
    /// </summary>
    public class SaleDetail
    {
        public int SaleDetailId { get; set; }
        public int SaleId { get; set; }
        public int ProductId { get; set; }
        public int BatchId { get; set; }
        public decimal Quantity { get; set; }
        public decimal SalePrice { get; set; }
        public decimal Discount { get; set; }
        public decimal LineTotal { get; set; }
        
        // Navigation properties
        public virtual Sale Sale { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
        public virtual ProductBatch Batch { get; set; } = null!;
    }

    /// <summary>
    /// Stock adjustment
    /// </summary>
    public class StockAdjustment
    {
        public int AdjustmentId { get; set; }
        public int BranchId { get; set; }
        public string AdjustmentNumber { get; set; } = string.Empty;
        public DateTime AdjustmentDate { get; set; }
        public int ProductId { get; set; }
        public int? BatchId { get; set; }
        public decimal Quantity { get; set; }
        public string AdjustmentType { get; set; } = string.Empty; // Add, Remove
        public string Reason { get; set; } = string.Empty; // PhysicalCount, Damage, Expiry, Correction
        public decimal CostPrice { get; set; }
        public string? Remarks { get; set; }
        public int UserId { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedDate { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
        
        // Navigation properties
        public virtual Branch Branch { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
        public virtual ProductBatch? Batch { get; set; }
        public virtual User User { get; set; } = null!;
    }

    /// <summary>
    /// Audit log for tracking all system activities
    /// </summary>
    public class AuditLog
    {
        public long LogId { get; set; }
        public int UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public int? RecordId { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? Description { get; set; }
        public string? IPAddress { get; set; }
        public string? MachineName { get; set; }
        public DateTime Timestamp { get; set; }
        
        // Navigation properties
        public virtual User User { get; set; } = null!;
    }

    /// <summary>
    /// System settings
    /// </summary>
    public class SystemSetting
    {
        public int SettingId { get; set; }
        public string SettingKey { get; set; } = string.Empty;
        public string? SettingValue { get; set; }
        public string DataType { get; set; } = "String";
        public string? Description { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// Customer ledger for credit sales
    /// </summary>
    public class CustomerLedger
    {
        public long LedgerId { get; set; }
        public int CustomerId { get; set; }
        public int BranchId { get; set; }
        public DateTime TransactionDate { get; set; }
        public string ReferenceType { get; set; } = string.Empty;
        public int ReferenceId { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public decimal BalanceAfterTransaction { get; set; }
        public string? Remarks { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedDate { get; set; }
        
        // Navigation properties
        public virtual Customer Customer { get; set; } = null!;
        public virtual Branch Branch { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }

    /// <summary>
    /// Supplier ledger
    /// </summary>
    public class SupplierLedger
    {
        public long LedgerId { get; set; }
        public int SupplierId { get; set; }
        public int BranchId { get; set; }
        public DateTime TransactionDate { get; set; }
        public string ReferenceType { get; set; } = string.Empty;
        public int ReferenceId { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public decimal BalanceAfterTransaction { get; set; }
        public string? Remarks { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedDate { get; set; }
        
        // Navigation properties
        public virtual Supplier Supplier { get; set; } = null!;
        public virtual Branch Branch { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }

    /// <summary>
    /// Database version tracking
    /// </summary>
    public class DatabaseVersion
    {
        public int VersionId { get; set; }
        public string VersionNumber { get; set; } = string.Empty;
        public DateTime AppliedDate { get; set; }
        public string? Description { get; set; }
    }
}
