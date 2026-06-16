-- =============================================
-- Enterprise Inventory & Pharmacy-Ready Management System
-- Database Schema - SQL Server 2008+ Compatible
-- Version: 1.0.0
-- =============================================

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

-- =============================================
-- DATABASE VERSIONING
-- =============================================
IF OBJECT_ID('dbo.DatabaseVersion', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.DatabaseVersion (
        VersionID INT IDENTITY(1,1) PRIMARY KEY,
        VersionNumber NVARCHAR(50) NOT NULL,
        AppliedDate DATETIME NOT NULL DEFAULT GETDATE(),
        Description NVARCHAR(500) NULL
    );
    
    INSERT INTO dbo.DatabaseVersion (VersionNumber, Description) 
    VALUES ('1.0.0', 'Initial schema with all core tables and future-ready structures');
END
GO

-- =============================================
-- MASTER DATA - BRANCHES
-- =============================================
IF OBJECT_ID('dbo.Branches', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Branches (
        BranchID INT IDENTITY(1,1) PRIMARY KEY,
        BranchCode NVARCHAR(50) NOT NULL UNIQUE,
        BranchName NVARCHAR(200) NOT NULL,
        Address NVARCHAR(500) NULL,
        Phone NVARCHAR(50) NULL,
        Email NVARCHAR(100) NULL,
        Status BIT NOT NULL DEFAULT 1, -- 1=Active, 0=Inactive
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        ModifiedDate DATETIME NULL
    );
    
    -- Insert default branch
    INSERT INTO dbo.Branches (BranchCode, BranchName, Status) 
    VALUES ('HQ001', 'Headquarters', 1);
END
GO

-- =============================================
-- SECURITY - ROLES AND PERMISSIONS
-- =============================================
IF OBJECT_ID('dbo.Roles', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Roles (
        RoleID INT IDENTITY(1,1) PRIMARY KEY,
        RoleCode NVARCHAR(50) NOT NULL UNIQUE,
        RoleName NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500) NULL,
        Status BIT NOT NULL DEFAULT 1
    );
    
    -- Insert default roles
    INSERT INTO dbo.Roles (RoleCode, RoleName, Description) VALUES
    ('SUPERADMIN', 'Super Admin', 'Full system access'),
    ('ADMIN', 'Admin', 'Administrative access'),
    ('MANAGER', 'Manager', 'Management access'),
    ('OPERATOR', 'Operator', 'Basic operational access'),
    ('AUDITOR', 'Auditor', 'Read-only audit access');
END
GO

IF OBJECT_ID('dbo.Permissions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Permissions (
        PermissionID INT IDENTITY(1,1) PRIMARY KEY,
        PermissionCode NVARCHAR(100) NOT NULL UNIQUE,
        PermissionName NVARCHAR(200) NOT NULL,
        ModuleName NVARCHAR(100) NULL,
        Description NVARCHAR(500) NULL
    );
    
    -- Insert core permissions
    INSERT INTO dbo.Permissions (PermissionCode, PermissionName, ModuleName) VALUES
    ('USER_VIEW', 'View Users', 'User Management'),
    ('USER_ADD', 'Add User', 'User Management'),
    ('USER_EDIT', 'Edit User', 'User Management'),
    ('USER_DELETE', 'Delete User', 'User Management'),
    ('PRODUCT_VIEW', 'View Products', 'Product Management'),
    ('PRODUCT_ADD', 'Add Product', 'Product Management'),
    ('PRODUCT_EDIT', 'Edit Product', 'Product Management'),
    ('PRODUCT_DELETE', 'Delete Product', 'Product Management'),
    ('PURCHASE_VIEW', 'View Purchases', 'Purchase Management'),
    ('PURCHASE_ADD', 'Add Purchase', 'Purchase Management'),
    ('PURCHASE_EDIT', 'Edit Purchase', 'Purchase Management'),
    ('SALE_VIEW', 'View Sales', 'Sales Management'),
    ('SALE_ADD', 'Add Sale', 'Sales Management'),
    ('SALE_EDIT', 'Edit Sale', 'Sales Management'),
    ('REPORT_VIEW', 'View Reports', 'Reporting'),
    ('ADJUSTMENT_ADD', 'Add Stock Adjustment', 'Inventory'),
    ('TRANSFER_ADD', 'Add Stock Transfer', 'Inventory');
END
GO

IF OBJECT_ID('dbo.RolePermissions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RolePermissions (
        RolePermissionID INT IDENTITY(1,1) PRIMARY KEY,
        RoleID INT NOT NULL FOREIGN KEY REFERENCES dbo.Roles(RoleID) ON DELETE CASCADE,
        PermissionID INT NOT NULL FOREIGN KEY REFERENCES dbo.Permissions(PermissionID) ON DELETE CASCADE,
        UNIQUE(RoleID, PermissionID)
    );
    
    -- Grant all permissions to Super Admin
    INSERT INTO dbo.RolePermissions (RoleID, PermissionID)
    SELECT r.RoleID, p.PermissionID
    FROM dbo.Roles r
    CROSS JOIN dbo.Permissions p
    WHERE r.RoleCode = 'SUPERADMIN';
END
GO

-- =============================================
-- FEATURE FLAG SYSTEM
-- =============================================
IF OBJECT_ID('dbo.Features', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Features (
        FeatureID INT IDENTITY(1,1) PRIMARY KEY,
        FeatureCode NVARCHAR(100) NOT NULL UNIQUE,
        FeatureName NVARCHAR(200) NOT NULL,
        Description NVARCHAR(500) NULL,
        IsEnabled BIT NOT NULL DEFAULT 0,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE()
    );
    
    -- Insert all features (disabled by default)
    INSERT INTO dbo.Features (FeatureCode, FeatureName, Description) VALUES
    ('MULTIBRANCH', 'Multi-Branch', 'Enable multi-branch operations'),
    ('ACCOUNTING', 'Accounting', 'Enable accounting module'),
    ('PURCHASERETURN', 'Purchase Returns', 'Enable purchase return functionality'),
    ('SALESRETURN', 'Sales Returns', 'Enable sales return functionality'),
    ('BATCHTRACKING', 'Batch Tracking', 'Enable batch-level tracking'),
    ('EXPIRYTRACKING', 'Expiry Tracking', 'Enable expiry date tracking'),
    ('BARCODEPRINTING', 'Barcode Printing', 'Enable barcode printing'),
    ('CREDITSALES', 'Credit Sales', 'Enable credit sales to customers'),
    ('CUSTOMERLEDGER', 'Customer Ledger', 'Enable customer ledger management'),
    ('SUPPLIERLEDGER', 'Supplier Ledger', 'Enable supplier ledger management'),
    ('MANUFACTURING', 'Manufacturing', 'Enable manufacturing module'),
    ('PHARMACYMODULE', 'Pharmacy Module', 'Enable pharmacy-specific features'),
    ('INVENTORYTRANSFER', 'Inventory Transfer', 'Enable inter-branch transfers');
END
GO

-- =============================================
-- USER MANAGEMENT
-- =============================================
IF OBJECT_ID('dbo.Users', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users (
        UserID INT IDENTITY(1,1) PRIMARY KEY,
        Username NVARCHAR(100) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(256) NOT NULL,
        Salt NVARCHAR(128) NOT NULL,
        FullName NVARCHAR(200) NOT NULL,
        Mobile NVARCHAR(50) NULL,
        Email NVARCHAR(100) NULL,
        RoleID INT NOT NULL FOREIGN KEY REFERENCES dbo.Roles(RoleID),
        BranchID INT NOT NULL FOREIGN KEY REFERENCES dbo.Branches(BranchID),
        Status BIT NOT NULL DEFAULT 1, -- 1=Active, 0=Inactive
        LastLoginDate DATETIME NULL,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        ModifiedDate DATETIME NULL,
        RowVersion TIMESTAMP NOT NULL
    );
    
    -- Default admin user (password: admin123)
    -- Hash generated using SHA256 + Salt
    INSERT INTO dbo.Users (Username, PasswordHash, Salt, FullName, RoleID, BranchID, Status) 
    VALUES ('admin', 
            '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918', 
            'randomsalt123', 
            'System Administrator', 
            1, -- SUPERADMIN
            1, -- HQ001
            1);
END
GO

IF OBJECT_ID('dbo.LoginAudit', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.LoginAudit (
        AuditID INT IDENTITY(1,1) PRIMARY KEY,
        UserID INT NOT NULL FOREIGN KEY REFERENCES dbo.Users(UserID),
        LoginTime DATETIME NOT NULL DEFAULT GETDATE(),
        LogoutTime DATETIME NULL,
        IPAddress NVARCHAR(50) NULL,
        MachineName NVARCHAR(100) NULL,
        Success BIT NOT NULL,
        FailureReason NVARCHAR(500) NULL
    );
    
    CREATE INDEX IX_LoginAudit_UserID ON dbo.LoginAudit(UserID);
    CREATE INDEX IX_LoginAudit_LoginTime ON dbo.LoginAudit(LoginTime);
END
GO

-- =============================================
-- MASTER DATA - CATEGORIES, UNITS, LOCATIONS
-- =============================================
IF OBJECT_ID('dbo.Categories', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Categories (
        CategoryID INT IDENTITY(1,1) PRIMARY KEY,
        CategoryName NVARCHAR(200) NOT NULL,
        Description NVARCHAR(500) NULL,
        Status BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE()
    );
    
    CREATE UNIQUE INDEX IX_CategoryName ON dbo.Categories(CategoryName) WHERE Status = 1;
END
GO

IF OBJECT_ID('dbo.Units', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Units (
        UnitID INT IDENTITY(1,1) PRIMARY KEY,
        UnitName NVARCHAR(100) NOT NULL,
        Symbol NVARCHAR(20) NULL,
        Status BIT NOT NULL DEFAULT 1
    );
    
    -- Insert common units
    INSERT INTO dbo.Units (UnitName, Symbol) VALUES
    ('Piece', 'Pcs'),
    ('Pack', 'Pk'),
    ('Box', 'Bx'),
    ('Carton', 'Ctn'),
    ('Bottle', 'Btl'),
    ('Strip', 'Str'),
    ('Kilogram', 'Kg'),
    ('Gram', 'g'),
    ('Liter', 'L'),
    ('Milliliter', 'mL');
END
GO

IF OBJECT_ID('dbo.StorageLocations', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.StorageLocations (
        LocationID INT IDENTITY(1,1) PRIMARY KEY,
        LocationCode NVARCHAR(50) NOT NULL UNIQUE,
        LocationName NVARCHAR(200) NOT NULL,
        Description NVARCHAR(500) NULL,
        BranchID INT NOT NULL FOREIGN KEY REFERENCES dbo.Branches(BranchID),
        Status BIT NOT NULL DEFAULT 1
    );
    
    CREATE INDEX IX_StorageLocations_BranchID ON dbo.StorageLocations(BranchID);
END
GO

-- =============================================
-- MASTER DATA - SUPPLIERS, CUSTOMERS
-- =============================================
IF OBJECT_ID('dbo.Suppliers', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Suppliers (
        SupplierID INT IDENTITY(1,1) PRIMARY KEY,
        SupplierCode NVARCHAR(50) NOT NULL UNIQUE,
        SupplierName NVARCHAR(200) NOT NULL,
        ContactPerson NVARCHAR(200) NULL,
        Mobile NVARCHAR(50) NULL,
        Phone NVARCHAR(50) NULL,
        Email NVARCHAR(100) NULL,
        Address NVARCHAR(500) NULL,
        City NVARCHAR(100) NULL,
        Country NVARCHAR(100) NULL DEFAULT 'Pakistan',
        OpeningBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
        CurrentBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
        CreditLimit DECIMAL(18,2) NULL,
        Status BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        RowVersion TIMESTAMP NOT NULL
    );
    
    CREATE INDEX IX_Suppliers_Status ON dbo.Suppliers(Status);
END
GO

IF OBJECT_ID('dbo.Customers', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Customers (
        CustomerID INT IDENTITY(1,1) PRIMARY KEY,
        CustomerCode NVARCHAR(50) NOT NULL UNIQUE,
        CustomerName NVARCHAR(200) NOT NULL,
        Mobile NVARCHAR(50) NULL,
        Phone NVARCHAR(50) NULL,
        Email NVARCHAR(100) NULL,
        Address NVARCHAR(500) NULL,
        City NVARCHAR(100) NULL,
        Country NVARCHAR(100) NULL DEFAULT 'Pakistan',
        OpeningBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
        CurrentBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
        CreditLimit DECIMAL(18,2) NULL,
        Status BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        RowVersion TIMESTAMP NOT NULL
    );
    
    CREATE INDEX IX_Customers_Status ON dbo.Customers(Status);
END
GO

-- =============================================
-- PHARMACY MODULE - MANUFACTURERS, GENERICS
-- =============================================
IF OBJECT_ID('dbo.Manufacturers', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Manufacturers (
        ManufacturerID INT IDENTITY(1,1) PRIMARY KEY,
        ManufacturerCode NVARCHAR(50) NOT NULL UNIQUE,
        ManufacturerName NVARCHAR(200) NOT NULL,
        Address NVARCHAR(500) NULL,
        City NVARCHAR(100) NULL,
        Country NVARCHAR(100) NULL,
        Phone NVARCHAR(50) NULL,
        Email NVARCHAR(100) NULL,
        LicenseNumber NVARCHAR(100) NULL,
        Status BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE()
    );
    
    CREATE INDEX IX_Manufacturers_Status ON dbo.Manufacturers(Status);
END
GO

IF OBJECT_ID('dbo.GenericMedicines', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.GenericMedicines (
        GenericID INT IDENTITY(1,1) PRIMARY KEY,
        GenericName NVARCHAR(200) NOT NULL,
        Description NVARCHAR(500) NULL,
        Status BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE()
    );
    
    CREATE UNIQUE INDEX IX_GenericName ON dbo.GenericMedicines(GenericName) WHERE Status = 1;
END
GO

-- =============================================
-- PRODUCT MANAGEMENT
-- =============================================
IF OBJECT_ID('dbo.Products', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products (
        ProductID INT IDENTITY(1,1) PRIMARY KEY,
        ProductCode NVARCHAR(50) NOT NULL UNIQUE,
        Barcode NVARCHAR(100) NULL UNIQUE,
        ProductName NVARCHAR(300) NOT NULL,
        GenericID INT NULL FOREIGN KEY REFERENCES dbo.GenericMedicines(GenericID),
        ManufacturerID INT NULL FOREIGN KEY REFERENCES dbo.Manufacturers(ManufacturerID),
        CategoryID INT NOT NULL FOREIGN KEY REFERENCES dbo.Categories(CategoryID),
        SupplierID INT NULL FOREIGN KEY REFERENCES dbo.Suppliers(SupplierID),
        UnitID INT NOT NULL FOREIGN KEY REFERENCES dbo.Units(UnitID),
        LocationID INT NULL FOREIGN KEY REFERENCES dbo.StorageLocations(LocationID),
        PurchasePrice DECIMAL(18,4) NOT NULL DEFAULT 0,
        SalePrice DECIMAL(18,4) NOT NULL DEFAULT 0,
        MinimumStock DECIMAL(18,2) NOT NULL DEFAULT 0,
        ReorderLevel DECIMAL(18,2) NOT NULL DEFAULT 0,
        Description NVARCHAR(1000) NULL,
        
        -- Pharmacy-specific fields
        DrugSchedule NVARCHAR(50) NULL, -- Schedule H, H1, X, etc.
        RegistrationNumber NVARCHAR(100) NULL,
        Strength NVARCHAR(100) NULL,
        DosageForm NVARCHAR(100) NULL, -- Tablet, Capsule, Syrup, etc.
        ControlledDrugFlag BIT NOT NULL DEFAULT 0,
        
        Status NVARCHAR(20) NOT NULL DEFAULT 'Active', -- Active, Inactive, Discontinued
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        ModifiedDate DATETIME NULL,
        RowVersion TIMESTAMP NOT NULL
    );
    
    CREATE INDEX IX_Products_CategoryID ON dbo.Products(CategoryID);
    CREATE INDEX IX_Products_Status ON dbo.Products(Status);
    CREATE INDEX IX_Products_Barcode ON dbo.Products(Barcode) WHERE Barcode IS NOT NULL;
END
GO

-- =============================================
-- BATCH AND EXPIRY MANAGEMENT
-- =============================================
IF OBJECT_ID('dbo.ProductBatches', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProductBatches (
        BatchID INT IDENTITY(1,1) PRIMARY KEY,
        ProductID INT NOT NULL FOREIGN KEY REFERENCES dbo.Products(ProductID),
        BatchNumber NVARCHAR(100) NOT NULL,
        ManufacturingDate DATE NULL,
        ExpiryDate DATE NULL,
        Quantity DECIMAL(18,2) NOT NULL DEFAULT 0,
        CostPrice DECIMAL(18,4) NOT NULL DEFAULT 0,
        BranchID INT NOT NULL FOREIGN KEY REFERENCES dbo.Branches(BranchID),
        Status BIT NOT NULL DEFAULT 1, -- 1=Active, 0=Expired/Used
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        RowVersion TIMESTAMP NOT NULL
    );
    
    CREATE INDEX IX_ProductBatches_ProductID ON dbo.ProductBatches(ProductID);
    CREATE INDEX IX_ProductBatches_ExpiryDate ON dbo.ProductBatches(ExpiryDate);
    CREATE INDEX IX_ProductBatches_BranchID ON dbo.ProductBatches(BranchID);
    CREATE UNIQUE INDEX IX_BatchUnique ON dbo.ProductBatches(ProductID, BatchNumber, BranchID);
END
GO

-- =============================================
-- OPENING STOCK
-- =============================================
IF OBJECT_ID('dbo.OpeningStock', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.OpeningStock (
        OpeningID INT IDENTITY(1,1) PRIMARY KEY,
        BranchID INT NOT NULL FOREIGN KEY REFERENCES dbo.Branches(BranchID),
        ProductID INT NOT NULL FOREIGN KEY REFERENCES dbo.Products(ProductID),
        BatchID INT NULL FOREIGN KEY REFERENCES dbo.ProductBatches(BatchID),
        Quantity DECIMAL(18,2) NOT NULL,
        CostPrice DECIMAL(18,4) NOT NULL,
        OpeningDate DATETIME NOT NULL DEFAULT GETDATE(),
        Remarks NVARCHAR(500) NULL,
        UserID INT NOT NULL FOREIGN KEY REFERENCES dbo.Users(UserID),
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE()
    );
    
    CREATE INDEX IX_OpeningStock_ProductID ON dbo.OpeningStock(ProductID);
    CREATE INDEX IX_OpeningStock_BranchID ON dbo.OpeningStock(BranchID);
END
GO

-- =============================================
-- INVENTORY TRANSACTION ENGINE (CORE)
-- =============================================
IF OBJECT_ID('dbo.InventoryTransactions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.InventoryTransactions (
        TransactionID BIGINT IDENTITY(1,1) PRIMARY KEY,
        BranchID INT NOT NULL FOREIGN KEY REFERENCES dbo.Branches(BranchID),
        ProductID INT NOT NULL FOREIGN KEY REFERENCES dbo.Products(ProductID),
        BatchID INT NULL FOREIGN KEY REFERENCES dbo.ProductBatches(BatchID),
        TransactionType NVARCHAR(50) NOT NULL, -- OpeningStock, Purchase, PurchaseReturn, Sale, SalesReturn, Adjustment, Damage, Expiry, Transfer
        ReferenceType NVARCHAR(50) NULL, -- PurchaseID, SaleID, AdjustmentID, etc.
        ReferenceID INT NULL,
        QtyIn DECIMAL(18,2) NOT NULL DEFAULT 0,
        QtyOut DECIMAL(18,2) NOT NULL DEFAULT 0,
        BalanceAfterTransaction DECIMAL(18,2) NOT NULL,
        UnitCost DECIMAL(18,4) NOT NULL DEFAULT 0,
        TransactionDate DATETIME NOT NULL DEFAULT GETDATE(),
        UserID INT NOT NULL FOREIGN KEY REFERENCES dbo.Users(UserID),
        Remarks NVARCHAR(1000) NULL,
        RowVersion TIMESTAMP NOT NULL
    );
    
    CREATE INDEX IX_InventoryTransactions_ProductID ON dbo.InventoryTransactions(ProductID);
    CREATE INDEX IX_InventoryTransactions_BatchID ON dbo.InventoryTransactions(BatchID);
    CREATE INDEX IX_InventoryTransactions_BranchID ON dbo.InventoryTransactions(BranchID);
    CREATE INDEX IX_InventoryTransactions_TransactionDate ON dbo.InventoryTransactions(TransactionDate);
    CREATE INDEX IX_InventoryTransactions_Reference ON dbo.InventoryTransactions(ReferenceType, ReferenceID);
END
GO

-- =============================================
-- PURCHASE MANAGEMENT
-- =============================================
IF OBJECT_ID('dbo.Purchases', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Purchases (
        PurchaseID INT IDENTITY(1,1) PRIMARY KEY,
        BranchID INT NOT NULL FOREIGN KEY REFERENCES dbo.Branches(BranchID),
        SupplierID INT NOT NULL FOREIGN KEY REFERENCES dbo.Suppliers(SupplierID),
        InvoiceNumber NVARCHAR(100) NOT NULL,
        PurchaseDate DATETIME NOT NULL DEFAULT GETDATE(),
        TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        Discount DECIMAL(18,2) NOT NULL DEFAULT 0,
        Tax DECIMAL(18,2) NOT NULL DEFAULT 0,
        NetAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        Remarks NVARCHAR(1000) NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT 'Pending', -- Pending, Approved, Cancelled
        UserID INT NOT NULL FOREIGN KEY REFERENCES dbo.Users(UserID),
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        ModifiedDate DATETIME NULL,
        RowVersion TIMESTAMP NOT NULL,
        UNIQUE(BranchID, InvoiceNumber)
    );
    
    CREATE INDEX IX_Purchases_SupplierID ON dbo.Purchases(SupplierID);
    CREATE INDEX IX_Purchases_BranchID ON dbo.Purchases(BranchID);
    CREATE INDEX IX_Purchases_PurchaseDate ON dbo.Purchases(PurchaseDate);
END
GO

IF OBJECT_ID('dbo.PurchaseDetails', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PurchaseDetails (
        PurchaseDetailID INT IDENTITY(1,1) PRIMARY KEY,
        PurchaseID INT NOT NULL FOREIGN KEY REFERENCES dbo.Purchases(PurchaseID) ON DELETE CASCADE,
        ProductID INT NOT NULL FOREIGN KEY REFERENCES dbo.Products(ProductID),
        BatchNumber NVARCHAR(100) NOT NULL,
        ExpiryDate DATE NULL,
        ManufacturingDate DATE NULL,
        Quantity DECIMAL(18,2) NOT NULL,
        BonusQuantity DECIMAL(18,2) NOT NULL DEFAULT 0,
        CostPrice DECIMAL(18,4) NOT NULL,
        LineTotal DECIMAL(18,2) NOT NULL,
        ReceivedQuantity DECIMAL(18,2) NOT NULL DEFAULT 0,
        Status NVARCHAR(20) NOT NULL DEFAULT 'Pending'
    );
    
    CREATE INDEX IX_PurchaseDetails_PurchaseID ON dbo.PurchaseDetails(PurchaseID);
    CREATE INDEX IX_PurchaseDetails_ProductID ON dbo.PurchaseDetails(ProductID);
END
GO

-- =============================================
-- PURCHASE RETURNS (FUTURE READY)
-- =============================================
IF OBJECT_ID('dbo.PurchaseReturns', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PurchaseReturns (
        ReturnID INT IDENTITY(1,1) PRIMARY KEY,
        BranchID INT NOT NULL FOREIGN KEY REFERENCES dbo.Branches(BranchID),
        PurchaseID INT NOT NULL FOREIGN KEY REFERENCES dbo.Purchases(PurchaseID),
        SupplierID INT NOT NULL FOREIGN KEY REFERENCES dbo.Suppliers(SupplierID),
        ReturnNumber NVARCHAR(100) NOT NULL UNIQUE,
        ReturnDate DATETIME NOT NULL DEFAULT GETDATE(),
        TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        Remarks NVARCHAR(1000) NULL,
        UserID INT NOT NULL FOREIGN KEY REFERENCES dbo.Users(UserID),
        Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        RowVersion TIMESTAMP NOT NULL
    );
    
    CREATE INDEX IX_PurchaseReturns_PurchaseID ON dbo.PurchaseReturns(PurchaseID);
    CREATE INDEX IX_PurchaseReturns_SupplierID ON dbo.PurchaseReturns(SupplierID);
END
GO

IF OBJECT_ID('dbo.PurchaseReturnDetails', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PurchaseReturnDetails (
        ReturnDetailID INT IDENTITY(1,1) PRIMARY KEY,
        ReturnID INT NOT NULL FOREIGN KEY REFERENCES dbo.PurchaseReturns(ReturnID) ON DELETE CASCADE,
        ProductID INT NOT NULL FOREIGN KEY REFERENCES dbo.Products(ProductID),
        BatchID INT NOT NULL FOREIGN KEY REFERENCES dbo.ProductBatches(BatchID),
        Quantity DECIMAL(18,2) NOT NULL,
        ReturnPrice DECIMAL(18,4) NOT NULL,
        LineTotal DECIMAL(18,2) NOT NULL,
        Reason NVARCHAR(500) NULL
    );
    
    CREATE INDEX IX_PurchaseReturnDetails_ReturnID ON dbo.PurchaseReturnDetails(ReturnID);
END
GO

-- =============================================
-- SALES MANAGEMENT
-- =============================================
IF OBJECT_ID('dbo.Sales', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Sales (
        SaleID INT IDENTITY(1,1) PRIMARY KEY,
        BranchID INT NOT NULL FOREIGN KEY REFERENCES dbo.Branches(BranchID),
        InvoiceNumber NVARCHAR(100) NOT NULL UNIQUE,
        CustomerID INT NULL FOREIGN KEY REFERENCES dbo.Customers(CustomerID),
        SaleDate DATETIME NOT NULL DEFAULT GETDATE(),
        TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        Discount DECIMAL(18,2) NOT NULL DEFAULT 0,
        Tax DECIMAL(18,2) NOT NULL DEFAULT 0,
        NetAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        PaidAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        DueAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        PaymentStatus NVARCHAR(20) NOT NULL DEFAULT 'Paid', -- Paid, Partial, Unpaid
        Remarks NVARCHAR(1000) NULL,
        UserID INT NOT NULL FOREIGN KEY REFERENCES dbo.Users(UserID),
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        ModifiedDate DATETIME NULL,
        RowVersion TIMESTAMP NOT NULL
    );
    
    CREATE INDEX IX_Sales_CustomerID ON dbo.Sales(CustomerID);
    CREATE INDEX IX_Sales_BranchID ON dbo.Sales(BranchID);
    CREATE INDEX IX_Sales_SaleDate ON dbo.Sales(SaleDate);
END
GO

IF OBJECT_ID('dbo.SaleDetails', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SaleDetails (
        SaleDetailID INT IDENTITY(1,1) PRIMARY KEY,
        SaleID INT NOT NULL FOREIGN KEY REFERENCES dbo.Sales(SaleID) ON DELETE CASCADE,
        ProductID INT NOT NULL FOREIGN KEY REFERENCES dbo.Products(ProductID),
        BatchID INT NOT NULL FOREIGN KEY REFERENCES dbo.ProductBatches(BatchID),
        Quantity DECIMAL(18,2) NOT NULL,
        SalePrice DECIMAL(18,4) NOT NULL,
        Discount DECIMAL(18,2) NOT NULL DEFAULT 0,
        LineTotal DECIMAL(18,2) NOT NULL
    );
    
    CREATE INDEX IX_SaleDetails_SaleID ON dbo.SaleDetails(SaleID);
    CREATE INDEX IX_SaleDetails_ProductID ON dbo.SaleDetails(ProductID);
    CREATE INDEX IX_SaleDetails_BatchID ON dbo.SaleDetails(BatchID);
END
GO

-- =============================================
-- SALES RETURNS (FUTURE READY)
-- =============================================
IF OBJECT_ID('dbo.SalesReturns', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SalesReturns (
        ReturnID INT IDENTITY(1,1) PRIMARY KEY,
        BranchID INT NOT NULL FOREIGN KEY REFERENCES dbo.Branches(BranchID),
        SaleID INT NOT NULL FOREIGN KEY REFERENCES dbo.Sales(SaleID),
        CustomerID INT NULL FOREIGN KEY REFERENCES dbo.Customers(CustomerID),
        ReturnNumber NVARCHAR(100) NOT NULL UNIQUE,
        ReturnDate DATETIME NOT NULL DEFAULT GETDATE(),
        TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        RefundAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        Remarks NVARCHAR(1000) NULL,
        UserID INT NOT NULL FOREIGN KEY REFERENCES dbo.Users(UserID),
        Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        RowVersion TIMESTAMP NOT NULL
    );
    
    CREATE INDEX IX_SalesReturns_SaleID ON dbo.SalesReturns(SaleID);
    CREATE INDEX IX_SalesReturns_CustomerID ON dbo.SalesReturns(CustomerID);
END
GO

IF OBJECT_ID('dbo.SalesReturnDetails', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SalesReturnDetails (
        ReturnDetailID INT IDENTITY(1,1) PRIMARY KEY,
        ReturnID INT NOT NULL FOREIGN KEY REFERENCES dbo.SalesReturns(ReturnID) ON DELETE CASCADE,
        ProductID INT NOT NULL FOREIGN KEY REFERENCES dbo.Products(ProductID),
        BatchID INT NOT NULL FOREIGN KEY REFERENCES dbo.ProductBatches(BatchID),
        Quantity DECIMAL(18,2) NOT NULL,
        ReturnPrice DECIMAL(18,4) NOT NULL,
        LineTotal DECIMAL(18,2) NOT NULL,
        Reason NVARCHAR(500) NULL
    );
    
    CREATE INDEX IX_SalesReturnDetails_ReturnID ON dbo.SalesReturnDetails(ReturnID);
END
GO

-- =============================================
-- STOCK ADJUSTMENTS
-- =============================================
IF OBJECT_ID('dbo.StockAdjustments', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.StockAdjustments (
        AdjustmentID INT IDENTITY(1,1) PRIMARY KEY,
        BranchID INT NOT NULL FOREIGN KEY REFERENCES dbo.Branches(BranchID),
        AdjustmentNumber NVARCHAR(100) NOT NULL UNIQUE,
        AdjustmentDate DATETIME NOT NULL DEFAULT GETDATE(),
        ProductID INT NOT NULL FOREIGN KEY REFERENCES dbo.Products(ProductID),
        BatchID INT NULL FOREIGN KEY REFERENCES dbo.ProductBatches(BatchID),
        Quantity DECIMAL(18,2) NOT NULL,
        AdjustmentType NVARCHAR(20) NOT NULL, -- Add, Remove
        Reason NVARCHAR(500) NOT NULL, -- PhysicalCount, Damage, Expiry, Correction
        CostPrice DECIMAL(18,4) NOT NULL DEFAULT 0,
        Remarks NVARCHAR(1000) NULL,
        UserID INT NOT NULL FOREIGN KEY REFERENCES dbo.Users(UserID),
        Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        RowVersion TIMESTAMP NOT NULL
    );
    
    CREATE INDEX IX_StockAdjustments_ProductID ON dbo.StockAdjustments(ProductID);
    CREATE INDEX IX_StockAdjustments_BranchID ON dbo.StockAdjustments(BranchID);
    CREATE INDEX IX_StockAdjustments_AdjustmentDate ON dbo.StockAdjustments(AdjustmentDate);
END
GO

-- =============================================
-- INVENTORY TRANSFERS (MULTI-BRANCH READY)
-- =============================================
IF OBJECT_ID('dbo.InventoryTransfers', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.InventoryTransfers (
        TransferID INT IDENTITY(1,1) PRIMARY KEY,
        TransferNumber NVARCHAR(100) NOT NULL UNIQUE,
        FromBranchID INT NOT NULL FOREIGN KEY REFERENCES dbo.Branches(BranchID),
        ToBranchID INT NOT NULL FOREIGN KEY REFERENCES dbo.Branches(BranchID),
        TransferDate DATETIME NOT NULL DEFAULT GETDATE(),
        Status NVARCHAR(20) NOT NULL DEFAULT 'Pending', -- Pending, InTransit, Received, Cancelled
        Remarks NVARCHAR(1000) NULL,
        UserID INT NOT NULL FOREIGN KEY REFERENCES dbo.Users(UserID),
        ReceivedBy INT NULL FOREIGN KEY REFERENCES dbo.Users(UserID),
        ReceivedDate DATETIME NULL,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        RowVersion TIMESTAMP NOT NULL,
        CHECK (FromBranchID <> ToBranchID)
    );
    
    CREATE INDEX IX_InventoryTransfers_FromBranch ON dbo.InventoryTransfers(FromBranchID);
    CREATE INDEX IX_InventoryTransfers_ToBranch ON dbo.InventoryTransfers(ToBranchID);
    CREATE INDEX IX_InventoryTransfers_Status ON dbo.InventoryTransfers(Status);
END
GO

IF OBJECT_ID('dbo.InventoryTransferDetails', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.InventoryTransferDetails (
        TransferDetailID INT IDENTITY(1,1) PRIMARY KEY,
        TransferID INT NOT NULL FOREIGN KEY REFERENCES dbo.InventoryTransfers(TransferID) ON DELETE CASCADE,
        ProductID INT NOT NULL FOREIGN KEY REFERENCES dbo.Products(ProductID),
        BatchID INT NOT NULL FOREIGN KEY REFERENCES dbo.ProductBatches(BatchID),
        Quantity DECIMAL(18,2) NOT NULL,
        TransferredQuantity DECIMAL(18,2) NOT NULL DEFAULT 0,
        ReceivedQuantity DECIMAL(18,2) NOT NULL DEFAULT 0,
        CostPrice DECIMAL(18,4) NOT NULL DEFAULT 0
    );
    
    CREATE INDEX IX_InventoryTransferDetails_TransferID ON dbo.InventoryTransferDetails(TransferID);
END
GO

-- =============================================
-- CUSTOMER LEDGER (CREDIT SALES READY)
-- =============================================
IF OBJECT_ID('dbo.CustomerLedger', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CustomerLedger (
        LedgerID BIGINT IDENTITY(1,1) PRIMARY KEY,
        CustomerID INT NOT NULL FOREIGN KEY REFERENCES dbo.Customers(CustomerID),
        BranchID INT NOT NULL FOREIGN KEY REFERENCES dbo.Branches(BranchID),
        TransactionDate DATETIME NOT NULL DEFAULT GETDATE(),
        ReferenceType NVARCHAR(50) NOT NULL, -- Sale, Payment, Return, Adjustment
        ReferenceID INT NOT NULL,
        DebitAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        CreditAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        BalanceAfterTransaction DECIMAL(18,2) NOT NULL,
        Remarks NVARCHAR(1000) NULL,
        UserID INT NOT NULL FOREIGN KEY REFERENCES dbo.Users(UserID),
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE()
    );
    
    CREATE INDEX IX_CustomerLedger_CustomerID ON dbo.CustomerLedger(CustomerID);
    CREATE INDEX IX_CustomerLedger_TransactionDate ON dbo.CustomerLedger(TransactionDate);
END
GO

-- =============================================
-- SUPPLIER LEDGER
-- =============================================
IF OBJECT_ID('dbo.SupplierLedger', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SupplierLedger (
        LedgerID BIGINT IDENTITY(1,1) PRIMARY KEY,
        SupplierID INT NOT NULL FOREIGN KEY REFERENCES dbo.Suppliers(SupplierID),
        BranchID INT NOT NULL FOREIGN KEY REFERENCES dbo.Branches(BranchID),
        TransactionDate DATETIME NOT NULL DEFAULT GETDATE(),
        ReferenceType NVARCHAR(50) NOT NULL, -- Purchase, Payment, Return, Adjustment
        ReferenceID INT NOT NULL,
        DebitAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        CreditAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        BalanceAfterTransaction DECIMAL(18,2) NOT NULL,
        Remarks NVARCHAR(1000) NULL,
        UserID INT NOT NULL FOREIGN KEY REFERENCES dbo.Users(UserID),
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE()
    );
    
    CREATE INDEX IX_SupplierLedger_SupplierID ON dbo.SupplierLedger(SupplierID);
    CREATE INDEX IX_SupplierLedger_TransactionDate ON dbo.SupplierLedger(TransactionDate);
END
GO

-- =============================================
-- ACCOUNTING MODULE (FUTURE READY)
-- =============================================
IF OBJECT_ID('dbo.Accounts', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Accounts (
        AccountID INT IDENTITY(1,1) PRIMARY KEY,
        AccountCode NVARCHAR(50) NOT NULL UNIQUE,
        AccountName NVARCHAR(200) NOT NULL,
        AccountType NVARCHAR(50) NOT NULL, -- Asset, Liability, Equity, Income, Expense
        ParentAccountID INT NULL FOREIGN KEY REFERENCES dbo.Accounts(AccountID),
        Description NVARCHAR(500) NULL,
        OpeningBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
        CurrentBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
        Status BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE()
    );
    
    CREATE INDEX IX_Accounts_ParentAccountID ON dbo.Accounts(ParentAccountID);
END
GO

IF OBJECT_ID('dbo.JournalEntries', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.JournalEntries (
        JournalID INT IDENTITY(1,1) PRIMARY KEY,
        JournalNumber NVARCHAR(100) NOT NULL UNIQUE,
        JournalDate DATETIME NOT NULL DEFAULT GETDATE(),
        Description NVARCHAR(1000) NULL,
        ReferenceType NVARCHAR(50) NULL,
        ReferenceID INT NULL,
        TotalDebit DECIMAL(18,2) NOT NULL DEFAULT 0,
        TotalCredit DECIMAL(18,2) NOT NULL DEFAULT 0,
        Posted BIT NOT NULL DEFAULT 0,
        UserID INT NOT NULL FOREIGN KEY REFERENCES dbo.Users(UserID),
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        RowVersion TIMESTAMP NOT NULL
    );
    
    CREATE INDEX IX_JournalEntries_JournalDate ON dbo.JournalEntries(JournalDate);
END
GO

IF OBJECT_ID('dbo.JournalDetails', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.JournalDetails (
        JournalDetailID INT IDENTITY(1,1) PRIMARY KEY,
        JournalID INT NOT NULL FOREIGN KEY REFERENCES dbo.JournalEntries(JournalID) ON DELETE CASCADE,
        AccountID INT NOT NULL FOREIGN KEY REFERENCES dbo.Accounts(AccountID),
        DebitAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        CreditAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        Description NVARCHAR(500) NULL
    );
    
    CREATE INDEX IX_JournalDetails_JournalID ON dbo.JournalDetails(JournalID);
    CREATE INDEX IX_JournalDetails_AccountID ON dbo.JournalDetails(AccountID);
END
GO

IF OBJECT_ID('dbo.CashBook', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CashBook (
        CashBookID BIGINT IDENTITY(1,1) PRIMARY KEY,
        TransactionDate DATETIME NOT NULL DEFAULT GETDATE(),
        Description NVARCHAR(1000) NOT NULL,
        ReferenceType NVARCHAR(50) NULL,
        ReferenceID INT NULL,
        DebitAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        CreditAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        BalanceAfterTransaction DECIMAL(18,2) NOT NULL,
        UserID INT NOT NULL FOREIGN KEY REFERENCES dbo.Users(UserID),
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE()
    );
    
    CREATE INDEX IX_CashBook_TransactionDate ON dbo.CashBook(TransactionDate);
END
GO

IF OBJECT_ID('dbo.BankBook', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.BankBook (
        BankBookID BIGINT IDENTITY(1,1) PRIMARY KEY,
        BankAccountID INT NOT NULL FOREIGN KEY REFERENCES dbo.Accounts(AccountID),
        TransactionDate DATETIME NOT NULL DEFAULT GETDATE(),
        Description NVARCHAR(1000) NOT NULL,
        ReferenceType NVARCHAR(50) NULL,
        ReferenceID INT NULL,
        CheckNumber NVARCHAR(50) NULL,
        DebitAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        CreditAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        BalanceAfterTransaction DECIMAL(18,2) NOT NULL,
        UserID INT NOT NULL FOREIGN KEY REFERENCES dbo.Users(UserID),
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE()
    );
    
    CREATE INDEX IX_BankBook_TransactionDate ON dbo.BankBook(TransactionDate);
END
GO

-- =============================================
-- AUDIT LOG SYSTEM
-- =============================================
IF OBJECT_ID('dbo.AuditLogs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditLogs (
        LogID BIGINT IDENTITY(1,1) PRIMARY KEY,
        UserID INT NOT NULL FOREIGN KEY REFERENCES dbo.Users(UserID),
        Action NVARCHAR(100) NOT NULL, -- Add, Edit, Delete, Login, Logout, Purchase, Sale, etc.
        TableName NVARCHAR(100) NOT NULL,
        RecordID INT NULL,
        OldValues NVARCHAR(MAX) NULL,
        NewValues NVARCHAR(MAX) NULL,
        Description NVARCHAR(1000) NULL,
        IPAddress NVARCHAR(50) NULL,
        MachineName NVARCHAR(100) NULL,
        Timestamp DATETIME NOT NULL DEFAULT GETDATE()
    );
    
    CREATE INDEX IX_AuditLogs_UserID ON dbo.AuditLogs(UserID);
    CREATE INDEX IX_AuditLogs_Timestamp ON dbo.AuditLogs(Timestamp);
    CREATE INDEX IX_AuditLogs_TableName ON dbo.AuditLogs(TableName);
    
    -- Prevent modifications to audit logs
    -- This is enforced at application level as well
END
GO

-- =============================================
-- SYSTEM SETTINGS
-- =============================================
IF OBJECT_ID('dbo.SystemSettings', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SystemSettings (
        SettingID INT IDENTITY(1,1) PRIMARY KEY,
        SettingKey NVARCHAR(100) NOT NULL UNIQUE,
        SettingValue NVARCHAR(MAX) NULL,
        DataType NVARCHAR(50) NOT NULL DEFAULT 'String',
        Description NVARCHAR(500) NULL,
        ModifiedBy INT NULL FOREIGN KEY REFERENCES dbo.Users(UserID),
        ModifiedDate DATETIME NOT NULL DEFAULT GETDATE()
    );
    
    -- Insert default settings
    INSERT INTO dbo.SystemSettings (SettingKey, SettingValue, DataType, Description) VALUES
    ('CompanyName', 'Enterprise Solutions', 'String', 'Company name for invoices'),
    ('Address', 'Main Street, Business District', 'String', 'Company address'),
    ('Phone', '+1234567890', 'String', 'Company phone number'),
    ('Email', 'info@enterprise.com', 'String', 'Company email'),
    ('Logo', NULL, 'Image', 'Company logo path'),
    ('TaxRate', '0', 'Decimal', 'Default tax rate percentage'),
    ('Currency', 'USD', 'String', 'Default currency'),
    ('CurrencySymbol', '$', 'String', 'Currency symbol'),
    ('InvoiceFooter', 'Thank you for your business!', 'String', 'Footer text for invoices'),
    ('DateFormat', 'MM/dd/yyyy', 'String', 'Default date format'),
    ('SessionTimeout', '30', 'Integer', 'Session timeout in minutes'),
    ('BackupPath', 'C:\\Backups', 'String', 'Default backup location'),
    ('AutoBackupEnabled', 'false', 'Boolean', 'Enable automatic backups'),
    ('BackupFrequency', 'Daily', 'String', 'Backup frequency');
END
GO

-- =============================================
-- BACKUP HISTORY
-- =============================================
IF OBJECT_ID('dbo.BackupHistory', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.BackupHistory (
        BackupID INT IDENTITY(1,1) PRIMARY KEY,
        BackupPath NVARCHAR(500) NOT NULL,
        BackupSize DECIMAL(18,2) NULL,
        BackupDate DATETIME NOT NULL DEFAULT GETDATE(),
        BackupType NVARCHAR(50) NOT NULL DEFAULT 'Full', -- Full, Differential, TransactionLog
        Status NVARCHAR(20) NOT NULL DEFAULT 'Success', -- Success, Failed
        Remarks NVARCHAR(1000) NULL,
        UserID INT NOT NULL FOREIGN KEY REFERENCES dbo.Users(UserID)
    );
    
    CREATE INDEX IX_BackupHistory_BackupDate ON dbo.BackupHistory(BackupDate);
END
GO

-- =============================================
-- TRIGGERS FOR DATA INTEGRITY
-- =============================================

-- Trigger to update product batch quantity on inventory transaction
IF OBJECT_ID('dbo.trg_UpdateBatchQuantity', 'TR') IS NULL
BEGIN
    EXEC('
    CREATE TRIGGER dbo.trg_UpdateBatchQuantity
    ON dbo.InventoryTransactions
    AFTER INSERT
    AS
    BEGIN
        SET NOCOUNT ON;
        
        -- Update batch quantity for incoming transactions
        UPDATE pb
        SET pb.Quantity = pb.Quantity + i.QtyIn - i.QtyOut
        FROM dbo.ProductBatches pb
        INNER JOIN inserted i ON pb.BatchID = i.BatchID
        WHERE i.BatchID IS NOT NULL;
    END
    ');
END
GO

-- =============================================
-- VIEWS FOR REPORTING
-- =============================================

-- View: Current Stock by Product and Batch
IF OBJECT_ID('dbo.vw_CurrentStock', 'V') IS NULL
BEGIN
    EXEC('
    CREATE VIEW dbo.vw_CurrentStock
    AS
    SELECT 
        p.ProductID,
        p.ProductCode,
        p.ProductName,
        p.Barcode,
        c.CategoryName,
        u.UnitName,
        b.BranchID,
        b.BranchCode,
        pb.BatchID,
        pb.BatchNumber,
        pb.ExpiryDate,
        SUM(pb.Quantity) AS CurrentQuantity,
        p.PurchasePrice,
        p.SalePrice,
        SUM(pb.Quantity * p.PurchasePrice) AS StockValue
    FROM dbo.Products p
    INNER JOIN dbo.Categories c ON p.CategoryID = c.CategoryID
    INNER JOIN dbo.Units u ON p.UnitID = u.UnitID
    INNER JOIN dbo.ProductBatches pb ON p.ProductID = pb.ProductID
    INNER JOIN dbo.Branches b ON pb.BranchID = b.BranchID
    WHERE pb.Status = 1 AND p.Status = ''Active''
    GROUP BY p.ProductID, p.ProductCode, p.ProductName, p.Barcode, 
             c.CategoryName, u.UnitName, b.BranchID, b.BranchCode,
             pb.BatchID, pb.BatchNumber, pb.ExpiryDate, 
             p.PurchasePrice, p.SalePrice
    ');
END
GO

-- View: Low Stock Products
IF OBJECT_ID('dbo.vw_LowStock', 'V') IS NULL
BEGIN
    EXEC('
    CREATE VIEW dbo.vw_LowStock
    AS
    SELECT 
        p.ProductID,
        p.ProductCode,
        p.ProductName,
        SUM(pb.Quantity) AS CurrentStock,
        p.MinimumStock,
        p.ReorderLevel,
        CASE 
            WHEN SUM(pb.Quantity) <= 0 THEN ''Out of Stock''
            WHEN SUM(pb.Quantity) <= p.MinimumStock THEN ''Below Minimum''
            WHEN SUM(pb.Quantity) <= p.ReorderLevel THEN ''Reorder Level''
            ELSE ''OK''
        END AS StockStatus
    FROM dbo.Products p
    INNER JOIN dbo.ProductBatches pb ON p.ProductID = pb.ProductID
    WHERE pb.Status = 1 AND p.Status = ''Active''
    GROUP BY p.ProductID, p.ProductCode, p.ProductName, p.MinimumStock, p.ReorderLevel
    HAVING SUM(pb.Quantity) <= p.ReorderLevel
    ');
END
GO

-- View: Near Expiry Products (Within 90 days)
IF OBJECT_ID('dbo.vw_NearExpiry', 'V') IS NULL
BEGIN
    EXEC('
    CREATE VIEW dbo.vw_NearExpiry
    AS
    SELECT 
        p.ProductID,
        p.ProductCode,
        p.ProductName,
        pb.BatchID,
        pb.BatchNumber,
        pb.ExpiryDate,
        pb.Quantity,
        DATEDIFF(DAY, GETDATE(), pb.ExpiryDate) AS DaysUntilExpiry,
        b.BranchCode
    FROM dbo.ProductBatches pb
    INNER JOIN dbo.Products p ON pb.ProductID = p.ProductID
    INNER JOIN dbo.Branches b ON pb.BranchID = b.BranchID
    WHERE pb.Status = 1 
      AND pb.ExpiryDate IS NOT NULL
      AND pb.ExpiryDate > GETDATE()
      AND pb.ExpiryDate <= DATEADD(DAY, 90, GETDATE())
      AND pb.Quantity > 0
    ');
END
GO

-- View: Expired Products
IF OBJECT_ID('dbo.vw_ExpiredProducts', 'V') IS NULL
BEGIN
    EXEC('
    CREATE VIEW dbo.vw_ExpiredProducts
    AS
    SELECT 
        p.ProductID,
        p.ProductCode,
        p.ProductName,
        pb.BatchID,
        pb.BatchNumber,
        pb.ExpiryDate,
        pb.Quantity,
        DATEDIFF(DAY, pb.ExpiryDate, GETDATE()) AS DaysSinceExpiry,
        b.BranchCode
    FROM dbo.ProductBatches pb
    INNER JOIN dbo.Products p ON pb.ProductID = p.ProductID
    INNER JOIN dbo.Branches b ON pb.BranchID = b.BranchID
    WHERE pb.Status = 1 
      AND pb.ExpiryDate IS NOT NULL
      AND pb.ExpiryDate < GETDATE()
      AND pb.Quantity > 0
    ');
END
GO

-- =============================================
-- STORED PROCEDURES FOR COMMON OPERATIONS
-- =============================================

-- Procedure: Get Dashboard Statistics
IF OBJECT_ID('dbo.usp_GetDashboardStats', 'P') IS NULL
BEGIN
    EXEC('
    CREATE PROCEDURE dbo.usp_GetDashboardStats
        @BranchID INT = NULL
    AS
    BEGIN
        SET NOCOUNT ON;
        
        DECLARE @TotalProducts INT,
                @TotalCategories INT,
                @TotalSuppliers INT,
                @TotalCustomers INT,
                @StockValue DECIMAL(18,2),
                @LowStockCount INT,
                @OutOfStockCount INT,
                @NearExpiryCount INT,
                @ExpiredCount INT,
                @TodaysPurchases DECIMAL(18,2),
                @TodaysSales DECIMAL(18,2);
        
        SELECT @TotalProducts = COUNT(*) FROM dbo.Products WHERE Status = ''Active'';
        SELECT @TotalCategories = COUNT(*) FROM dbo.Categories WHERE Status = 1;
        SELECT @TotalSuppliers = COUNT(*) FROM dbo.Suppliers WHERE Status = 1;
        SELECT @TotalCustomers = COUNT(*) FROM dbo.Customers WHERE Status = 1;
        
        SELECT @StockValue = ISNULL(SUM(pb.Quantity * p.PurchasePrice), 0)
        FROM dbo.ProductBatches pb
        INNER JOIN dbo.Products p ON pb.ProductID = p.ProductID
        WHERE pb.Status = 1 AND (@BranchID IS NULL OR pb.BranchID = @BranchID);
        
        SELECT @LowStockCount = COUNT(*) FROM dbo.vw_LowStock;
        
        SELECT @OutOfStockCount = COUNT(*)
        FROM dbo.Products p
        LEFT JOIN dbo.ProductBatches pb ON p.ProductID = pb.ProductID AND pb.Status = 1
        WHERE p.Status = ''Active''
        GROUP BY p.ProductID
        HAVING ISNULL(SUM(pb.Quantity), 0) = 0;
        
        SELECT @NearExpiryCount = COUNT(*) FROM dbo.vw_NearExpiry;
        SELECT @ExpiredCount = COUNT(*) FROM dbo.vw_ExpiredProducts;
        
        SELECT @TodaysPurchases = ISNULL(SUM(NetAmount), 0)
        FROM dbo.Purchases
        WHERE CAST(PurchaseDate AS DATE) = CAST(GETDATE() AS DATE);
        
        SELECT @TodaysSales = ISNULL(SUM(NetAmount), 0)
        FROM dbo.Sales
        WHERE CAST(SaleDate AS DATE) = CAST(GETDATE() AS DATE);
        
        SELECT 
            @TotalProducts AS TotalProducts,
            @TotalCategories AS TotalCategories,
            @TotalSuppliers AS TotalSuppliers,
            @TotalCustomers AS TotalCustomers,
            @StockValue AS StockValue,
            @LowStockCount AS LowStockCount,
            @OutOfStockCount AS OutOfStockCount,
            @NearExpiryCount AS NearExpiryCount,
            @ExpiredCount AS ExpiredCount,
            @TodaysPurchases AS TodaysPurchases,
            @TodaysSales AS TodaysSales;
    END
    ');
END
GO

-- =============================================
-- FINALIZE SCHEMA
-- =============================================
PRINT 'Database schema creation completed successfully.';
PRINT 'Database Version: 1.0.0';
PRINT 'All tables, indexes, constraints, views, and stored procedures have been created.';
GO
