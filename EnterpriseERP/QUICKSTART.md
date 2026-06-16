# Quick Start Guide

## Prerequisites Installation

### 1. Install .NET 8 SDK
Download from: https://dotnet.microsoft.com/download/dotnet/8.0

### 2. Install SQL Server
- SQL Server Express (Free): https://www.microsoft.com/sql-server/sql-server-downloads
- Or SQL Server Developer Edition (Free for development)

### 3. Install Visual Studio 2022 (Recommended)
- Community Edition (Free): https://visualstudio.microsoft.com/downloads/
- Select ".NET desktop development" workload

## Step-by-Step Setup

### Step 1: Database Setup

1. Open SQL Server Management Studio (SSMS)
2. Connect to your SQL Server instance
3. Create new database:
```sql
CREATE DATABASE EnterpriseERP;
GO
```

4. Open `db/DatabaseSchema.sql` in SSMS
5. Execute the script (F5 or click Execute)
6. Verify tables were created:
```sql
USE EnterpriseERP;
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';
```

### Step 2: Configure Connection String

1. Open `src/UI/App.config`
2. Update the connection string:
```xml
<connectionStrings>
  <add name="DefaultConnection" 
       connectionString="Server=YOUR_SERVER_NAME;Database=EnterpriseERP;Trusted_Connection=True;TrustServerCertificate=True;" 
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

Replace `YOUR_SERVER_NAME` with:
- `localhost` for local SQL Server
- `.\SQLEXPRESS` for SQL Express
- `server-name\instance-name` for named instances

### Step 3: Build the Application

#### Option A: Visual Studio
1. Open `EnterpriseERP.sln` in Visual Studio 2022
2. Wait for NuGet package restoration
3. Build → Build Solution (Ctrl+Shift+B)
4. Run → Start Debugging (F5)

#### Option B: Command Line
```bash
cd EnterpriseERP
dotnet restore
dotnet build
dotnet run --project src/UI/EnterpriseERP.csproj
```

### Step 4: First Login

Default credentials:
- **Username**: `admin`
- **Password**: `Admin@123`

**Important**: Change the default password immediately after first login!

## Verification Checklist

- [ ] Database `EnterpriseERP` exists
- [ ] All tables created (run: `SELECT COUNT(*) FROM sys.tables`)
- [ ] Default admin user exists (run: `SELECT Username FROM Users WHERE RoleID = 1`)
- [ ] Features are configured (run: `SELECT FeatureCode, IsEnabled FROM Features`)
- [ ] Application builds without errors
- [ ] Login successful with admin credentials
- [ ] Dashboard displays correctly

## Common Issues & Solutions

### Issue: "Login failed for user"
**Solution**: 
- Use Windows Authentication: `Trusted_Connection=True`
- Or add SQL login: `User Id=sa;Password=yourpassword;`

### Issue: "A network-related or instance-specific error occurred"
**Solution**:
- Ensure SQL Server service is running
- Check server name in connection string
- Enable TCP/IP in SQL Server Configuration Manager

### Issue: "The target framework .NET 8.0 is not installed"
**Solution**:
- Install .NET 8 SDK from Microsoft website
- Restart Visual Studio

### Issue: "NuGet package restore failed"
**Solution**:
```bash
dotnet nuget locals all --clear
dotnet restore
```

### Issue: "Object reference not set to an instance of an object"
**Solution**:
- Check that database schema was fully executed
- Verify connection string is correct
- Check application logs in `error.log`

## Next Steps

1. **Change Default Password**: Settings → Users → Reset admin password
2. **Create Additional Users**: Settings → Users → Add New User
3. **Configure Company Info**: Settings → Company Settings
4. **Add Categories**: Products → Categories → Add New
5. **Add Units**: Products → Units → Add New (Piece, Pack, Box, etc.)
6. **Add Suppliers**: Suppliers → Add New Supplier
7. **Add Products**: Products → Add New Product
8. **Set Opening Stock**: Inventory → Opening Stock

## Feature Activation

To enable additional modules, run these SQL commands:

```sql
-- Enable Purchase Returns
UPDATE Features SET IsEnabled = 1 WHERE FeatureCode = 'PurchaseReturns';

-- Enable Sales Returns  
UPDATE Features SET IsEnabled = 1 WHERE FeatureCode = 'SalesReturns';

-- Enable Multi-Branch
UPDATE Features SET IsEnabled = 1 WHERE FeatureCode = 'MultiBranch';

-- Enable Pharmacy Module
UPDATE Features SET IsEnabled = 1 WHERE FeatureCode = 'PharmacyModule';

-- Enable Accounting
UPDATE Features SET IsEnabled = 1 WHERE FeatureCode = 'Accounting';
```

Restart the application after enabling features.

## Backup Strategy

### Initial Backup
After setup, create a backup:
```sql
BACKUP DATABASE EnterpriseERP 
TO DISK = 'C:\Backups\EnterpriseERP_Initial.bak'
WITH FORMAT, INIT;
```

### Regular Backups
Configure automated backups in SQL Server Agent or use the application's backup feature.

## Support

For issues:
1. Check `error.log` in application directory
2. Review SQL Server error logs
3. Verify all prerequisites are installed
4. Check connection string configuration

---

**Ready to use! Start managing your inventory efficiently.**
