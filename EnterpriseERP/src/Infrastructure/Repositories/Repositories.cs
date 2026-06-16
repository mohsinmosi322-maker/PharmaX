using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using EnterpriseInventory.Core.Domain;
using EnterpriseInventory.Core.Interfaces;
using Dapper;

namespace EnterpriseInventory.Infrastructure.Repositories
{
    public class GenericRepository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly IDbConnection _connection;
        protected readonly string _tableName;

        public GenericRepository(IDbConnection connection)
        {
            _connection = connection;
            _tableName = typeof(T).Name + "s";
            if (_tableName == "Entitys") _tableName = "Entities";
        }

        public virtual T GetById(int id)
        {
            var sql = $"SELECT * FROM {_tableName} WHERE {GetPrimaryKey()} = @Id";
            return _connection.QueryFirstOrDefault<T>(sql, new { Id = id });
        }

        public virtual IEnumerable<T> GetAll()
        {
            return _connection.Query<T>($"SELECT * FROM {_tableName}");
        }

        public virtual IEnumerable<T> Find(Expression<Func<T, bool>> predicate)
        {
            return _connection.Query<T>($"SELECT * FROM {_tableName}").AsEnumerable().Where(predicate.Compile());
        }

        public virtual void Add(T entity)
        {
            var columns = GetColumns(false);
            var columnNames = string.Join(", ", columns);
            var paramNames = string.Join(", ", columns.Select(c => "@" + c));
            
            var sql = $"INSERT INTO {_tableName} ({columnNames}) VALUES ({paramNames}); SELECT CAST(SCOPE_IDENTITY() as int);";
            
            var id = _connection.ExecuteScalar<int>(sql, entity);
            
            var prop = typeof(T).GetProperty(GetPrimaryKey());
            if (prop != null) prop.SetValue(entity, id);
        }

        public virtual void Update(T entity)
        {
            var columns = GetColumns(true);
            var setClause = string.Join(", ", columns.Select(c => $"{c} = @{c}"));
            var pk = GetPrimaryKey();
            
            var sql = $"UPDATE {_tableName} SET {setClause} WHERE {pk} = @{pk}";
            _connection.Execute(sql, entity);
        }

        public virtual void Delete(int id)
        {
            var sql = $"DELETE FROM {_tableName} WHERE {GetPrimaryKey()} = @Id";
            _connection.Execute(sql, new { Id = id });
        }

        protected virtual string GetPrimaryKey() => "Id";

        protected virtual List<string> GetColumns(bool includeId)
        {
            var props = typeof(T).GetProperties()
                .Where(p => p.PropertyType.IsValueType || p.PropertyType == typeof(string) || p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?))
                .Select(p => p.Name)
                .ToList();

            if (!includeId)
                props.Remove(GetPrimaryKey());

            return props; 
        }
    }

    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(IDbConnection connection) : base(connection) { }

        public User GetByUsername(string username)
        {
            var sql = "SELECT * FROM Users WHERE Username = @Username AND Status = 'Active'";
            return _connection.QueryFirstOrDefault<User>(sql, new { Username = username });
        }

        public override string GetPrimaryKey() => "UserID";
        
        protected override List<string> GetColumns(bool includeId)
        {
            return new List<string> { "Username", "PasswordHash", "Salt", "FullName", "Mobile", "Email", "RoleID", "BranchID", "Status" };
        }
    }

    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(IDbConnection connection) : base(connection) { }

        public override string GetPrimaryKey() => "ProductID";

        public Product GetByBarcode(string barcode)
        {
            var sql = "SELECT * FROM Products WHERE Barcode = @Barcode AND Status = 'Active'";
            return _connection.QueryFirstOrDefault<Product>(sql, new { Barcode = barcode });
        }

        public IEnumerable<Product> Search(string searchTerm)
        {
            var sql = @"SELECT * FROM Products 
                        WHERE (ProductName LIKE @Term OR ProductCode LIKE @Term OR Barcode LIKE @Term) 
                        AND Status = 'Active'";
            return _connection.Query<Product>(sql, new { Term = "%" + searchTerm + "%" });
        }

        protected override List<string> GetColumns(bool includeId)
        {
            var cols = new List<string> { "ProductCode", "Barcode", "ProductName", "GenericID", "ManufacturerID", 
                                          "CategoryID", "SupplierID", "UnitID", "LocationID", "PurchasePrice", 
                                          "SalePrice", "MinimumStock", "ReorderLevel", "Description", "Status", "CreatedDate" };
            if (includeId) cols.Insert(0, "ProductID");
            return cols;
        }
    }

    public class InventoryTransactionRepository : IInventoryTransactionRepository
    {
        private readonly IDbConnection _connection;

        public InventoryTransactionRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public void AddTransaction(InventoryTransaction transaction)
        {
            var sql = @"INSERT INTO InventoryTransactions 
                        (BranchID, ProductID, BatchID, TransactionType, ReferenceType, ReferenceID, 
                         QtyIn, QtyOut, BalanceAfterTransaction, UnitCost, TransactionDate, UserID, Remarks)
                        VALUES 
                        (@BranchID, @ProductID, @BatchID, @TransactionType, @ReferenceType, @ReferenceID, 
                         @QtyIn, @QtyOut, @BalanceAfterTransaction, @UnitCost, @TransactionDate, @UserID, @Remarks)";
            
            _connection.Execute(sql, transaction);
        }

        public IEnumerable<InventoryTransaction> GetProductLedger(int productId, int? branchId = null)
        {
            var sql = @"SELECT * FROM InventoryTransactions 
                        WHERE ProductID = @ProductId 
                        AND (@BranchId IS NULL OR BranchID = @BranchId)
                        ORDER BY TransactionDate DESC, TransactionID DESC";
            
            return _connection.Query<InventoryTransaction>(sql, new { ProductId = productId, BranchId = branchId });
        }
    }

    public class FeatureRepository : GenericRepository<Feature>, IFeatureRepository
    {
        public FeatureRepository(IDbConnection connection) : base(connection) { }
        
        public override string GetPrimaryKey() => "FeatureID";

        public bool IsEnabled(string featureCode)
        {
            var sql = "SELECT IsEnabled FROM Features WHERE FeatureCode = @Code";
            return _connection.ExecuteScalar<bool>(sql, new { Code = featureCode });
        }

        protected override List<string> GetColumns(bool includeId)
        {
            return new List<string> { "FeatureCode", "FeatureName", "IsEnabled" };
        }
    }

    public class BranchRepository : GenericRepository<Branch>, IBranchRepository
    {
        public BranchRepository(IDbConnection connection) : base(connection) { }
        public override string GetPrimaryKey() => "BranchID";
        protected override List<string> GetColumns(bool includeId)
        {
            return new List<string> { "BranchCode", "BranchName", "Address", "Phone", "Status" };
        }
    }

    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(IDbConnection connection) : base(connection) { }
        public override string GetPrimaryKey() => "CategoryID";
        protected override List<string> GetColumns(bool includeId)
        {
            return new List<string> { "CategoryName", "Description", "Status" };
        }
    }

    public class SupplierRepository : GenericRepository<Supplier>, ISupplierRepository
    {
        public SupplierRepository(IDbConnection connection) : base(connection) { }
        public override string GetPrimaryKey() => "SupplierID";
        protected override List<string> GetColumns(bool includeId)
        {
            return new List<string> { "SupplierCode", "SupplierName", "ContactPerson", "Mobile", "Phone", "Email", "Address", "City", "Status" };
        }
    }

    public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
    {
        public CustomerRepository(IDbConnection connection) : base(connection) { }
        public override string GetPrimaryKey() => "CustomerID";
        protected override List<string> GetColumns(bool includeId)
        {
            return new List<string> { "CustomerCode", "CustomerName", "Mobile", "Address", "City", "OpeningBalance", "CurrentBalance", "CreditLimit", "Status" };
        }
    }

    public class UnitRepository : GenericRepository<Unit>, IUnitRepository
    {
        public UnitRepository(IDbConnection connection) : base(connection) { }
        public override string GetPrimaryKey() => "UnitID";
        protected override List<string> GetColumns(bool includeId)
        {
            return new List<string> { "UnitName", "Symbol" };
        }
    }

    public class LocationRepository : GenericRepository<Location>, ILocationRepository
    {
        public LocationRepository(IDbConnection connection) : base(connection) { }
        public override string GetPrimaryKey() => "LocationID";
        protected override List<string> GetColumns(bool includeId)
        {
            return new List<string> { "LocationCode", "LocationName", "Description" };
        }
    }

    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly IDbConnection _connection;

        public AuditLogRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public void Log(AuditLog log)
        {
            var sql = @"INSERT INTO AuditLogs (UserID, Action, TableName, RecordID, Description, Timestamp, IPAddress)
                        VALUES (@UserID, @Action, @TableName, @RecordID, @Description, @Timestamp, @IPAddress)";
            _connection.Execute(sql, log);
        }

        public IEnumerable<AuditLog> GetLogs(int userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var sql = @"SELECT * FROM AuditLogs 
                        WHERE UserID = @UserId 
                        AND (@StartDate IS NULL OR Timestamp >= @StartDate)
                        AND (@EndDate IS NULL OR Timestamp <= @EndDate)
                        ORDER BY Timestamp DESC";
            return _connection.Query<AuditLog>(sql, new { UserId = userId, StartDate = startDate, EndDate = endDate });
        }
    }

    public class PurchaseRepository : IPurchaseRepository
    {
        private readonly IDbConnection _connection;

        public PurchaseRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public int CreatePurchase(PurchaseHeader header, IEnumerable<PurchaseDetail> details)
        {
            using (var transaction = _connection.BeginTransaction())
            {
                try
                {
                    // Insert Header
                    var headerSql = @"INSERT INTO Purchases (BranchID, SupplierID, InvoiceNumber, PurchaseDate, Remarks, TotalAmount, Status)
                                      VALUES (@BranchID, @SupplierID, @InvoiceNumber, @PurchaseDate, @Remarks, @TotalAmount, @Status);
                                      SELECT CAST(SCOPE_IDENTITY() as int);";
                    
                    var purchaseId = _connection.ExecuteScalar<int>(headerSql, header, transaction);
                    header.PurchaseID = purchaseId;

                    // Insert Details
                    var detailSql = @"INSERT INTO PurchaseDetails (PurchaseID, ProductID, BatchNumber, ExpiryDate, Quantity, BonusQuantity, CostPrice, LineTotal)
                                      VALUES (@PurchaseID, @ProductID, @BatchNumber, @ExpiryDate, @Quantity, @BonusQuantity, @CostPrice, @LineTotal)";
                    
                    foreach (var detail in details)
                    {
                        detail.PurchaseID = purchaseId;
                        _connection.Execute(detailSql, detail, transaction);
                    }

                    transaction.Commit();
                    return purchaseId;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public PurchaseHeader GetPurchaseById(int purchaseId)
        {
            var sql = "SELECT * FROM Purchases WHERE PurchaseID = @PurchaseId";
            return _connection.QueryFirstOrDefault<PurchaseHeader>(sql, new { PurchaseId = purchaseId });
        }

        public IEnumerable<PurchaseDetail> GetPurchaseDetails(int purchaseId)
        {
            var sql = "SELECT * FROM PurchaseDetails WHERE PurchaseID = @PurchaseId";
            return _connection.Query<PurchaseDetail>(sql, new { PurchaseId = purchaseId });
        }
    }

    public class SalesRepository : ISalesRepository
    {
        private readonly IDbConnection _connection;

        public SalesRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public int CreateSale(SaleHeader header, IEnumerable<SaleDetail> details)
        {
            using (var transaction = _connection.BeginTransaction())
            {
                try
                {
                    // Insert Header
                    var headerSql = @"INSERT INTO Sales (BranchID, CustomerID, InvoiceNumber, SaleDate, Discount, Tax, Remarks, TotalAmount, Status)
                                      VALUES (@BranchID, @CustomerID, @InvoiceNumber, @SaleDate, @Discount, @Tax, @Remarks, @TotalAmount, @Status);
                                      SELECT CAST(SCOPE_IDENTITY() as int);";
                    
                    var saleId = _connection.ExecuteScalar<int>(headerSql, header, transaction);
                    header.SaleID = saleId;

                    // Insert Details
                    var detailSql = @"INSERT INTO SaleDetails (SaleID, ProductID, BatchID, Quantity, SalePrice, Discount, Total)
                                      VALUES (@SaleID, @ProductID, @BatchID, @Quantity, @SalePrice, @Discount, @Total)";
                    
                    foreach (var detail in details)
                    {
                        detail.SaleID = saleId;
                        _connection.Execute(detailSql, detail, transaction);
                    }

                    transaction.Commit();
                    return saleId;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public SaleHeader GetSaleById(int saleId)
        {
            var sql = "SELECT * FROM Sales WHERE SaleID = @SaleId";
            return _connection.QueryFirstOrDefault<SaleHeader>(sql, new { SaleId = saleId });
        }

        public IEnumerable<SaleDetail> GetSaleDetails(int saleId)
        {
            var sql = "SELECT * FROM SaleDetails WHERE SaleID = @SaleId";
            return _connection.Query<SaleDetail>(sql, new { SaleId = saleId });
        }
    }

    public class BatchRepository : IBatchRepository
    {
        private readonly IDbConnection _connection;

        public BatchRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public ProductBatch GetBatchByFEFO(int productId)
        {
            // First Expired First Out - get earliest expiry batch with stock
            var sql = @"SELECT TOP 1 * FROM ProductBatches 
                        WHERE ProductID = @ProductId AND Quantity > 0 
                        AND (ExpiryDate IS NULL OR ExpiryDate > GETDATE())
                        ORDER BY ExpiryDate ASC, BatchID ASC";
            
            return _connection.QueryFirstOrDefault<ProductBatch>(sql, new { ProductId = productId });
        }

        public IEnumerable<ProductBatch> GetProductBatches(int productId)
        {
            var sql = "SELECT * FROM ProductBatches WHERE ProductID = @ProductId ORDER BY ExpiryDate ASC";
            return _connection.Query<ProductBatch>(sql, new { ProductId = productId });
        }

        public void UpdateBatchQuantity(int batchId, int quantityChange)
        {
            var sql = "UPDATE ProductBatches SET Quantity = Quantity + @QtyChange WHERE BatchID = @BatchId";
            _connection.Execute(sql, new { BatchId = batchId, QtyChange = quantityChange });
        }

        public ProductBatch GetBatchById(int batchId)
        {
            var sql = "SELECT * FROM ProductBatches WHERE BatchID = @BatchId";
            return _connection.QueryFirstOrDefault<ProductBatch>(sql, new { BatchId = batchId });
        }
    }
}
