using System;
using Microsoft.Extensions.DependencyInjection;
using EnterpriseERP.Infrastructure.Data;
using EnterpriseERP.Infrastructure.Repositories;
using EnterpriseERP.Application.Services;
using EnterpriseERP.Core.Interfaces;

namespace EnterpriseERP.Core.DI
{
    public static class DependencyInjector
    {
        public static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Register Database Context
            services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

            // Register Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IFeatureRepository, FeatureRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IUnitRepository, UnitRepository>();
            services.AddScoped<ISupplierRepository, SupplierRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ILocationRepository, LocationRepository>();
            services.AddScoped<IPurchaseRepository, PurchaseRepository>();
            services.AddScoped<ISaleRepository, SaleRepository>();
            services.AddScoped<IInventoryTransactionRepository, InventoryTransactionRepository>();
            services.AddScoped<IStockAdjustmentRepository, StockAdjustmentRepository>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();

            // Register Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IFeatureService, FeatureService>();
            services.AddScoped<IInventoryService, InventoryService>();
            services.AddScoped<IReportingService, ReportingService>();
            services.AddScoped<IBackupService, BackupService>();
            services.AddScoped<IAuditService, AuditService>();

            return services.BuildServiceProvider();
        }
    }
}
