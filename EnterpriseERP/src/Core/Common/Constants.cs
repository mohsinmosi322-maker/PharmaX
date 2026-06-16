namespace EnterpriseERP.Core.Common
{
    /// <summary>
    /// System-wide constants
    /// </summary>
    public static class Constants
    {
        public static class TransactionTypes
        {
            public const string OpeningStock = "OpeningStock";
            public const string Purchase = "Purchase";
            public const string PurchaseReturn = "PurchaseReturn";
            public const string Sale = "Sale";
            public const string SalesReturn = "SalesReturn";
            public const string Adjustment = "Adjustment";
            public const string Damage = "Damage";
            public const string Expiry = "Expiry";
            public const string TransferOut = "TransferOut";
            public const string TransferIn = "TransferIn";
        }

        public static class ProductStatus
        {
            public const string Active = "Active";
            public const string Inactive = "Inactive";
            public const string Discontinued = "Discontinued";
        }

        public static class PurchaseStatus
        {
            public const string Pending = "Pending";
            public const string Approved = "Approved";
            public const string Cancelled = "Cancelled";
        }

        public static class PaymentStatus
        {
            public const string Paid = "Paid";
            public const string Partial = "Partial";
            public const string Unpaid = "Unpaid";
        }

        public static class AdjustmentTypes
        {
            public const string Add = "Add";
            public const string Remove = "Remove";
        }

        public static class AdjustmentReasons
        {
            public const string PhysicalCount = "PhysicalCount";
            public const string Damage = "Damage";
            public const string Expiry = "Expiry";
            public const string Correction = "Correction";
        }

        public static class TransferStatus
        {
            public const string Pending = "Pending";
            public const string InTransit = "InTransit";
            public const string Received = "Received";
            public const string Cancelled = "Cancelled";
        }

        public static class FeatureCodes
        {
            public const string MultiBranch = "MULTIBRANCH";
            public const string Accounting = "ACCOUNTING";
            public const string PurchaseReturns = "PURCHASERETURN";
            public const string SalesReturns = "SALESRETURN";
            public const string BatchTracking = "BATCHTRACKING";
            public const string ExpiryTracking = "EXPIRYTRACKING";
            public const string BarcodePrinting = "BARCODEPRINTING";
            public const string CreditSales = "CREDITSALES";
            public const string CustomerLedger = "CUSTOMERLEDGER";
            public const string SupplierLedger = "SUPPLIERLEDGER";
            public const string Manufacturing = "MANUFACTURING";
            public const string PharmacyModule = "PHARMACYMODULE";
            public const string InventoryTransfer = "INVENTORYTRANSFER";
        }

        public static class PermissionCodes
        {
            // User Management
            public const string UserView = "USER_VIEW";
            public const string UserAdd = "USER_ADD";
            public const string UserEdit = "USER_EDIT";
            public const string UserDelete = "USER_DELETE";
            
            // Product Management
            public const string ProductView = "PRODUCT_VIEW";
            public const string ProductAdd = "PRODUCT_ADD";
            public const string ProductEdit = "PRODUCT_EDIT";
            public const string ProductDelete = "PRODUCT_DELETE";
            
            // Purchase Management
            public const string PurchaseView = "PURCHASE_VIEW";
            public const string PurchaseAdd = "PURCHASE_ADD";
            public const string PurchaseEdit = "PURCHASE_EDIT";
            
            // Sales Management
            public const string SaleView = "SALE_VIEW";
            public const string SaleAdd = "SALE_ADD";
            public const string SaleEdit = "SALE_EDIT";
            
            // Reporting
            public const string ReportView = "REPORT_VIEW";
            
            // Inventory
            public const string AdjustmentAdd = "ADJUSTMENT_ADD";
            public const string TransferAdd = "TRANSFER_ADD";
        }

        public static class RoleCodes
        {
            public const string SuperAdmin = "SUPERADMIN";
            public const string Admin = "ADMIN";
            public const string Manager = "MANAGER";
            public const string Operator = "OPERATOR";
            public const string Auditor = "AUDITOR";
        }

        public static class AuditActions
        {
            public const string Login = "Login";
            public const string Logout = "Logout";
            public const string Add = "Add";
            public const string Edit = "Edit";
            public const string Delete = "Delete";
            public const string View = "View";
            public const string Print = "Print";
            public const string Export = "Export";
            public const string Import = "Import";
            public const string Approve = "Approve";
            public const string Cancel = "Cancel";
        }

        public static class SettingKeys
        {
            public const string CompanyName = "CompanyName";
            public const string Address = "Address";
            public const string Phone = "Phone";
            public const string Email = "Email";
            public const string Logo = "Logo";
            public const string TaxRate = "TaxRate";
            public const string Currency = "Currency";
            public const string CurrencySymbol = "CurrencySymbol";
            public const string InvoiceFooter = "InvoiceFooter";
            public const string DateFormat = "DateFormat";
            public const string SessionTimeout = "SessionTimeout";
            public const string BackupPath = "BackupPath";
            public const string AutoBackupEnabled = "AutoBackupEnabled";
            public const string BackupFrequency = "BackupFrequency";
        }

        public static class Messages
        {
            public const string Success = "Operation completed successfully.";
            public const string Error = "An error occurred. Please try again.";
            public const string NotFound = "Record not found.";
            public const string Unauthorized = "You are not authorized to perform this action.";
            public const string InsufficientStock = "Insufficient stock available.";
            public const string DuplicateRecord = "A record with this information already exists.";
            public const string InvalidData = "Invalid data provided.";
            public const string SaveSuccess = "Record saved successfully.";
            public const string DeleteSuccess = "Record deleted successfully.";
            public const string UpdateSuccess = "Record updated successfully.";
        }
    }
}
