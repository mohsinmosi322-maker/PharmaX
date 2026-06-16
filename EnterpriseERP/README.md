# Enterprise Inventory & Pharmacy-Ready Management System

A comprehensive, production-ready Enterprise Resource Planning (ERP) system built with C# .NET 8, Windows Forms, and SQL Server. Designed as a future-proof foundation for Pharmacy POS and Multi-Branch operations.

## Architecture Overview

### Technology Stack
- **Language**: C# .NET 8
- **UI Framework**: Windows Forms
- **Database**: SQL Server 2008+
- **Data Access**: ADO.NET
- **Architecture**: Clean Architecture with Layered Design

### Architectural Patterns
- Repository Pattern
- Unit of Work Pattern
- Service Layer
- Dependency Injection
- SOLID Principles
- Transaction-Based Inventory Engine

## Project Structure

```
EnterpriseERP/
├── db/                          # Database scripts
│   └── DatabaseSchema.sql       # Complete database schema
├── src/
│   ├── Core/                    # Core business logic
│   │   ├── Domain/              # Entity definitions
│   │   ├── DTOs/                # Data Transfer Objects
│   │   ├── Interfaces/          # Service contracts
│   │   └── Common/              # Utilities, constants, extensions
│   ├── Infrastructure/          # External implementations
│   │   ├── Data/                # Database connectivity
│   │   ├── Repositories/        # Repository implementations
│   │   ├── Services/            # Service implementations
│   │   └── Security/            # Authentication, authorization
│   └── UI/                      # Windows Forms application
│       ├── Forms/               # Main forms
│       ├── Controls/            # Custom controls
│       ├── Helpers/             # UI helpers
│       └── Resources/           # Images, icons, resources
└── scripts/                     # Utility scripts
```

## Key Features

### Core Modules (Active)
- ✅ User Management with RBAC
- ✅ Feature Flag System
- ✅ Branch Management (Multi-branch ready)
- ✅ Product Management with Barcode Support
- ✅ Category & Unit Management
- ✅ Supplier & Customer Management
- ✅ Purchase Management
- ✅ Sales Management
- ✅ Stock Adjustments
- ✅ Batch & Expiry Tracking
- ✅ Inventory Transaction Ledger
- ✅ Dashboard with Real-time Stats
- ✅ Comprehensive Reporting
- ✅ Audit Logging
- ✅ Backup & Restore

### Future-Ready Modules (Database Ready, UI Hidden)
- 🔄 Purchase Returns
- 🔄 Sales Returns
- 🔄 Credit Sales & Customer Ledger
- 🔄 Supplier Ledger
- 🔄 Inventory Transfers (Multi-branch)
- 🔄 Pharmacy Module (Drug schedules, dosage forms)
- 🔄 Accounting Module (GL, Cash Book, Bank Book)
- 🔄 Manufacturing

## Security Features

- SHA256 + Salt Password Hashing
- Role-Based Access Control (RBAC)
- Permission-Based Access
- Feature Flags for dynamic enabling/disabling
- Login Audit Trail
- Session Timeout Management
- User Activity Monitoring
- Immutable Audit Logs

## Database Highlights

### Core Tables (30+)
- Branches, Roles, Permissions, Users, Features
- Categories, Units, StorageLocations
- Suppliers, Customers, Manufacturers, GenericMedicines
- Products, ProductBatches
- OpeningStock, InventoryTransactions
- Purchases, PurchaseDetails, PurchaseReturns
- Sales, SaleDetails, SalesReturns
- StockAdjustments, InventoryTransfers
- CustomerLedger, SupplierLedger
- Accounts, JournalEntries, JournalDetails, CashBook, BankBook
- AuditLogs, SystemSettings, BackupHistory

### Built-in Views
- vw_CurrentStock - Real-time stock levels
- vw_LowStock - Products below reorder level
- vw_NearExpiry - Products expiring within 90 days
- vw_ExpiredProducts - Expired stock

### Stored Procedures
- usp_GetDashboardStats - Dashboard statistics

## Inventory Engine

The system uses a **Transaction-Based Inventory Engine** where:
- Every stock movement creates an InventoryTransaction record
- No direct stock editing allowed
- All movements traceable to source documents
- FEFO (First Expired, First Out) support
- Real-time stock calculations

### Transaction Types
1. Opening Stock
2. Purchase
3. Purchase Return
4. Sale
5. Sales Return
6. Adjustment
7. Damage
8. Expiry
9. Transfer Out
10. Transfer In

## Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server 2008 or later
- Visual Studio 2022 (recommended)

### Database Setup
1. Open SQL Server Management Studio
2. Create a new database: `EnterpriseERP`
3. Execute `db/DatabaseSchema.sql`
4. Default credentials: `admin` / `admin123`

### Build & Run
```bash
# Restore packages
dotnet restore

# Build solution
dotnet build

# Run application
dotnet run --project src/UI/EnterpriseERP.UI.csproj
```

## Configuration

Update connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=EnterpriseERP;Integrated Security=true;TrustServerCertificate=true;"
  },
  "DatabaseSettings": {
    "Server": "localhost",
    "Database": "EnterpriseERP",
    "IntegratedSecurity": true,
    "Timeout": 30
  }
}
```

## Feature Flags

Enable/disable features without code changes:
```sql
-- Enable Multi-Branch
UPDATE Features SET IsEnabled = 1 WHERE FeatureCode = 'MULTIBRANCH';

-- Enable Pharmacy Module
UPDATE Features SET IsEnabled = 1 WHERE FeatureCode = 'PHARMACYMODULE';

-- Enable Accounting
UPDATE Features SET IsEnabled = 1 WHERE FeatureCode = 'ACCOUNTING';
```

## Roles & Permissions

### Default Roles
| Role | Description |
|------|-------------|
| Super Admin | Full system access |
| Admin | Administrative access |
| Manager | Management operations |
| Operator | Basic operations |
| Auditor | Read-only audit access |

### Permission Categories
- User Management (View, Add, Edit, Delete)
- Product Management (View, Add, Edit, Delete)
- Purchase Management (View, Add, Edit)
- Sales Management (View, Add, Edit)
- Reporting (View)
- Inventory (Adjustment, Transfer)

## Best Practices Implemented

1. **Never edit stock directly** - Always use transactions
2. **Always use transactions** - For data integrity
3. **Audit everything** - Complete audit trail
4. **Validate input** - At all layers
5. **Handle concurrency** - Row versioning
6. **Log errors** - Comprehensive error handling
7. **Secure passwords** - SHA256 + Salt
8. **Feature flags** - Dynamic feature management
9. **Branch-aware** - All transactions branch-specific
10. **Future-proof** - Extensible architecture

## Concurrency Handling

- Optimistic concurrency with RowVersion timestamps
- SQL Transactions for data integrity
- Rollback mechanisms on failures
- Prevents stock corruption in multi-user scenarios

## Reporting Capabilities

### Inventory Reports
- Current Stock Report
- Stock Valuation
- Low Stock Alert
- Reorder Level Report
- Dead Stock Analysis
- Near Expiry Alert
- Expired Stock Report
- Product Ledger (Movement History)

### Purchase Reports
- Supplier Purchase Summary
- Date-wise Purchases
- Purchase Return Report

### Sales Reports
- Product Sales Summary
- Customer Sales Summary
- Sales Return Report
- Daily/Monthly Sales

## Backup Strategy

- Manual backup on demand
- Scheduled automatic backups
- Backup history tracking
- One-click restore functionality

## Next Steps

1. **Immediate Use**: System is ready for inventory, purchases, and sales
2. **Phase 2**: Enable returns, credit sales, ledgers
3. **Phase 3**: Activate multi-branch transfers
4. **Phase 4**: Enable pharmacy-specific features
5. **Phase 5**: Implement accounting module

## Support & Maintenance

- Regular database backups recommended
- Monitor audit logs for security
- Review low stock and expiry alerts daily
- Keep feature flags updated per business needs
- Apply patches and updates regularly

## License

Proprietary - Enterprise Solutions

---

**Built for scalability, security, and long-term growth.**
