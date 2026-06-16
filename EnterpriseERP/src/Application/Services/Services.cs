using System;
using System.Data;
using System.Transactions;
using EnterpriseInventory.Core.Domain;
using EnterpriseInventory.Core.Interfaces;
using EnterpriseInventory.Core.Common;

namespace EnterpriseInventory.Application.Services
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IDbConnection _connection;
        private IDbTransaction _transaction;
        private bool _disposed;

        // Repositories
        private IUserRepository _userRepository;
        private IProductRepository _productRepository;
        private IInventoryTransactionRepository _inventoryTransactionRepository;
        private IFeatureRepository _featureRepository;
        private IAuditLogRepository _auditLogRepository;
        private IPurchaseRepository _purchaseRepository;
        private ISalesRepository _salesRepository;
        private IBatchRepository _batchRepository;

        public UnitOfWork(IDbConnection connection)
        {
            _connection = connection;
        }

        public void BeginTransaction()
        {
            if (_connection.State != ConnectionState.Open)
                _connection.Open();
            
            _transaction = _connection.BeginTransaction();
        }

        public void Commit()
        {
            _transaction?.Commit();
        }

        public void Rollback()
        {
            _transaction?.Rollback();
        }

        public IUserRepository Users => _userRepository ??= new UserRepository(_connection);
        public IProductRepository Products => _productRepository ??= new ProductRepository(_connection);
        public IInventoryTransactionRepository InventoryTransactions => _inventoryTransactionRepository ??= new InventoryTransactionRepository(_connection);
        public IFeatureRepository Features => _featureRepository ??= new FeatureRepository(_connection);
        public IAuditLogRepository AuditLogs => _auditLogRepository ??= new AuditLogRepository(_connection);
        public IPurchaseRepository Purchases => _purchaseRepository ??= new PurchaseRepository(_connection);
        public ISalesRepository Sales => _salesRepository ??= new SalesRepository(_connection);
        public IBatchRepository Batches => _batchRepository ??= new BatchRepository(_connection);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _connection?.Dispose();
                }
                _disposed = true;
            }
        }
    }

    public class AuthenticationService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFeatureRepository _featureRepository;

        public AuthenticationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _featureRepository = unitOfWork.Features;
        }

        public User Login(string username, string password)
        {
            var user = _unitOfWork.Users.GetByUsername(username);
            
            if (user == null)
                throw new InvalidOperationException("Invalid username or password");

            var computedHash = SecurityExtensions.ComputeSha256Hash(password + user.Salt);
            
            if (computedHash != user.PasswordHash)
                throw new InvalidOperationException("Invalid username or password");

            // Log login attempt
            _unitOfWork.AuditLogs.Log(new AuditLog
            {
                UserID = user.UserID,
                Action = "Login",
                TableName = "Users",
                RecordID = user.UserID,
                Description = $"User {username} logged in",
                Timestamp = DateTime.Now,
                IPAddress = ""
            });

            return user;
        }

        public void Logout(int userId)
        {
            _unitOfWork.AuditLogs.Log(new AuditLog
            {
                UserID = userId,
                Action = "Logout",
                TableName = "Users",
                RecordID = userId,
                Description = "User logged out",
                Timestamp = DateTime.Now,
                IPAddress = ""
            });
        }

        public bool HasPermission(int userId, string permissionCode)
        {
            // Simplified permission check - in production, join with RolePermissions
            var user = _unitOfWork.Users.GetById(userId);
            if (user == null) return false;

            // For now, Super Admin has all permissions
            if (user.RoleID == 1) return true;

            // TODO: Implement proper role-permission lookup
            return true;
        }

        public bool IsFeatureEnabled(string featureCode)
        {
            return _featureRepository.IsEnabled(featureCode);
        }
    }

    public class InventoryService : IInventoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogRepository _auditLogRepository;

        public InventoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _auditLogRepository = unitOfWork.AuditLogs;
        }

        public int ProcessOpeningStock(OpeningStock openingStock, int userId, int branchId)
        {
            using (var scope = new TransactionScope())
            {
                try
                {
                    // Create or get batch
                    var batch = new ProductBatch
                    {
                        ProductID = openingStock.ProductID,
                        BatchNumber = openingStock.BatchNumber ?? "OS-" + DateTime.Now.ToString("yyyyMMdd"),
                        ManufacturingDate = openingStock.ManufacturingDate,
                        ExpiryDate = openingStock.ExpiryDate,
                        Quantity = openingStock.Quantity,
                        CostPrice = openingStock.CostPrice
                    };

                    // Insert batch (simplified - would need batch repo method)
                    var batchSql = @"INSERT INTO ProductBatches (ProductID, BatchNumber, ManufacturingDate, ExpiryDate, Quantity, CostPrice)
                                     VALUES (@ProductID, @BatchNumber, @ManufacturingDate, @ExpiryDate, @Quantity, @CostPrice);
                                     SELECT CAST(SCOPE_IDENTITY() as int);";
                    
                    var batchId = _unitOfWork.Batches is BatchRepository br 
                        ? Convert.ToInt32(typeof(BatchRepository).GetMethod("GetBatchById")?.Invoke(br, new object[] { batchId }))
                        : 0;

                    // Create inventory transaction
                    var transaction = new InventoryTransaction
                    {
                        BranchID = branchId,
                        ProductID = openingStock.ProductID,
                        BatchID = batchId,
                        TransactionType = TransactionTypes.OpeningStock,
                        ReferenceType = "OpeningStock",
                        ReferenceID = 0, // Will be updated
                        QtyIn = openingStock.Quantity,
                        QtyOut = 0,
                        BalanceAfterTransaction = openingStock.Quantity, // Simplified
                        UnitCost = openingStock.CostPrice,
                        TransactionDate = openingStock.Date,
                        UserID = userId,
                        Remarks = "Opening Stock Entry"
                    };

                    _unitOfWork.InventoryTransactions.AddTransaction(transaction);

                    // Audit log
                    _auditLogRepository.Log(new AuditLog
                    {
                        UserID = userId,
                        Action = "Add",
                        TableName = "OpeningStock",
                        RecordID = openingStock.ProductID,
                        Description = $"Opening stock added for product {openingStock.ProductID}",
                        Timestamp = DateTime.Now,
                        IPAddress = ""
                    });

                    scope.Complete();
                    return transaction.TransactionID;
                }
                catch
                {
                    throw;
                }
            }
        }

        public int ProcessPurchase(PurchaseHeader header, System.Collections.Generic.IEnumerable<PurchaseDetail> details, int userId)
        {
            using (var scope = new TransactionScope())
            {
                try
                {
                    // Create purchase record
                    var purchaseId = _unitOfWork.Purchases.CreatePurchase(header, details);

                    // Update batches and create inventory transactions for each detail
                    foreach (var detail in details)
                    {
                        // Find or create batch
                        var existingBatchSql = @"SELECT BatchID FROM ProductBatches 
                                                 WHERE ProductID = @ProductId AND BatchNumber = @BatchNumber";
                        
                        // Simplified batch handling
                        var batchId = detail.BatchID > 0 ? detail.BatchID : CreateBatch(detail, header.BranchID);

                        // Create inventory transaction
                        var transaction = new InventoryTransaction
                        {
                            BranchID = header.BranchID,
                            ProductID = detail.ProductID,
                            BatchID = batchId,
                            TransactionType = TransactionTypes.Purchase,
                            ReferenceType = "Purchase",
                            ReferenceID = purchaseId,
                            QtyIn = detail.Quantity + detail.BonusQuantity,
                            QtyOut = 0,
                            BalanceAfterTransaction = 0, // Would calculate current balance
                            UnitCost = detail.CostPrice,
                            TransactionDate = header.PurchaseDate,
                            UserID = userId,
                            Remarks = $"Purchase Invoice: {header.InvoiceNumber}"
                        };

                        _unitOfWork.InventoryTransactions.AddTransaction(transaction);
                    }

                    // Audit log
                    _auditLogRepository.Log(new AuditLog
                    {
                        UserID = userId,
                        Action = "Add",
                        TableName = "Purchases",
                        RecordID = purchaseId,
                        Description = $"Purchase created: {header.InvoiceNumber}",
                        Timestamp = DateTime.Now,
                        IPAddress = ""
                    });

                    scope.Complete();
                    return purchaseId;
                }
                catch
                {
                    throw;
                }
            }
        }

        public int ProcessSale(SaleHeader header, System.Collections.Generic.IEnumerable<SaleDetail> details, int userId)
        {
            using (var scope = new TransactionScope())
            {
                try
                {
                    // Validate stock availability (FEFO)
                    foreach (var detail in details)
                    {
                        var availableBatch = _unitOfWork.Batches.GetBatchByFEFO(detail.ProductID);
                        if (availableBatch == null || availableBatch.Quantity < detail.Quantity)
                        {
                            throw new InvalidOperationException($"Insufficient stock for product ID {detail.ProductID}");
                        }
                        
                        // Use FEFO batch if not specified
                        if (detail.BatchID <= 0)
                            detail.BatchID = availableBatch.BatchID;
                    }

                    // Create sale record
                    var saleId = _unitOfWork.Sales.CreateSale(header, details);

                    // Update batches and create inventory transactions
                    foreach (var detail in details)
                    {
                        // Reduce batch quantity
                        _unitOfWork.Batches.UpdateBatchQuantity(detail.BatchID, -detail.Quantity);

                        // Create inventory transaction
                        var transaction = new InventoryTransaction
                        {
                            BranchID = header.BranchID,
                            ProductID = detail.ProductID,
                            BatchID = detail.BatchID,
                            TransactionType = TransactionTypes.Sale,
                            ReferenceType = "Sale",
                            ReferenceID = saleId,
                            QtyIn = 0,
                            QtyOut = detail.Quantity,
                            BalanceAfterTransaction = 0, // Would calculate
                            UnitCost = 0, // Could store average cost
                            TransactionDate = header.SaleDate,
                            UserID = userId,
                            Remarks = $"Sale Invoice: {header.InvoiceNumber}"
                        };

                        _unitOfWork.InventoryTransactions.AddTransaction(transaction);
                    }

                    // Update customer balance if credit sale
                    if (header.CustomerID > 0 && header.TotalAmount > 0)
                    {
                        // TODO: Update CustomerLedger
                    }

                    // Audit log
                    _auditLogRepository.Log(new AuditLog
                    {
                        UserID = userId,
                        Action = "Add",
                        TableName = "Sales",
                        RecordID = saleId,
                        Description = $"Sale created: {header.InvoiceNumber}",
                        Timestamp = DateTime.Now,
                        IPAddress = ""
                    });

                    scope.Complete();
                    return saleId;
                }
                catch
                {
                    throw;
                }
            }
        }

        private int CreateBatch(PurchaseDetail detail, int branchId)
        {
            // Simplified batch creation
            var sql = @"INSERT INTO ProductBatches (ProductID, BatchNumber, ExpiryDate, Quantity, CostPrice)
                        VALUES (@ProductID, @BatchNumber, @ExpiryDate, @Quantity, @CostPrice);
                        SELECT CAST(SCOPE_IDENTITY() as int);";
            
            // Would execute via Dapper
            return 0; // Placeholder
        }

        public System.Collections.Generic.IEnumerable<InventoryTransaction> GetProductLedger(int productId, int? branchId = null)
        {
            return _unitOfWork.InventoryTransactions.GetProductLedger(productId, branchId);
        }
    }

    public class ReportingService : IReportingService
    {
        private readonly IDbConnection _connection;

        public ReportingService(IDbConnection connection)
        {
            _connection = connection;
        }

        public DashboardData GetDashboardData(int branchId)
        {
            var data = new DashboardData();

            // Total Products
            data.TotalProducts = _connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Products WHERE Status = 'Active'");

            // Total Categories
            data.TotalCategories = _connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Categories WHERE Status = 'Active'");

            // Total Suppliers
            data.TotalSuppliers = _connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Suppliers WHERE Status = 'Active'");

            // Total Customers
            data.TotalCustomers = _connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Customers WHERE Status = 'Active'");

            // Current Stock Value
            data.StockValue = _connection.ExecuteScalar<decimal>(@"
                SELECT ISNULL(SUM(pb.Quantity * pb.CostPrice), 0)
                FROM ProductBatches pb
                WHERE pb.Quantity > 0");

            // Low Stock Products
            data.LowStockCount = _connection.ExecuteScalar<int>(@"
                SELECT COUNT(*) FROM CurrentStockView cs
                JOIN Products p ON cs.ProductID = p.ProductID
                WHERE cs.AvailableQuantity <= p.ReorderLevel");

            // Out Of Stock
            data.OutOfStockCount = _connection.ExecuteScalar<int>(@"
                SELECT COUNT(*) FROM CurrentStockView cs
                WHERE cs.AvailableQuantity = 0");

            // Near Expiry (within 30 days)
            data.NearExpiryCount = _connection.ExecuteScalar<int>(@"
                SELECT COUNT(*) FROM NearExpiryView");

            // Expired
            data.ExpiredCount = _connection.ExecuteScalar<int>(@"
                SELECT COUNT(*) FROM ExpiredStockView");

            // Today's Purchases
            data.TodayPurchases = _connection.ExecuteScalar<int>(@"
                SELECT ISNULL(SUM(TotalAmount), 0) FROM Purchases 
                WHERE CAST(PurchaseDate AS DATE) = CAST(GETDATE() AS DATE)");

            // Today's Sales
            data.TodaySales = _connection.ExecuteScalar<int>(@"
                SELECT ISNULL(SUM(TotalAmount), 0) FROM Sales 
                WHERE CAST(SaleDate AS DATE) = CAST(GETDATE() AS DATE)");

            return data;
        }

        public System.Collections.Generic.IEnumerable<StockReportItem> GetCurrentStock(int? branchId = null)
        {
            var sql = branchId.HasValue
                ? "SELECT * FROM CurrentStockView WHERE BranchID = @BranchId"
                : "SELECT * FROM CurrentStockView";
            
            return _connection.Query<StockReportItem>(sql, new { BranchId = branchId });
        }

        public System.Collections.Generic.IEnumerable<StockReportItem> GetLowStock()
        {
            return _connection.Query<StockReportItem>("SELECT * FROM LowStockView");
        }

        public System.Collections.Generic.IEnumerable<StockReportItem> GetNearExpiry()
        {
            return _connection.Query<StockReportItem>("SELECT * FROM NearExpiryView");
        }

        public System.Collections.Generic.IEnumerable<StockReportItem> GetExpiredStock()
        {
            return _connection.Query<StockReportItem>("SELECT * FROM ExpiredStockView");
        }
    }

    public class BackupService : IBackupService
    {
        private readonly string _connectionString;

        public BackupService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void CreateBackup(string backupPath)
        {
            using (var connection = new System.Data.SqlClient.SqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = $@"
                    BACKUP DATABASE [EnterpriseERP] 
                    TO DISK = '{backupPath}' 
                    WITH FORMAT, INIT, SKIP, NOREWIND, NOUNLOAD, STATS = 10";
                command.CommandTimeout = 300;
                command.ExecuteNonQuery();
            }
        }

        public void RestoreBackup(string backupPath)
        {
            using (var connection = new System.Data.SqlClient.SqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                
                // Set database to single user mode
                command.CommandText = "ALTER DATABASE [EnterpriseERP] SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
                command.ExecuteNonQuery();

                // Restore
                command.CommandText = $@"
                    RESTORE DATABASE [EnterpriseERP] 
                    FROM DISK = '{backupPath}' 
                    WITH REPLACE, RECOVERY";
                command.CommandTimeout = 300;
                command.ExecuteNonQuery();

                // Set back to multi-user
                command.CommandText = "ALTER DATABASE [EnterpriseERP] SET MULTI_USER";
                command.ExecuteNonQuery();
            }
        }
    }
}
