using Budget.Core.Interfaces;
using Budget.Infrastructure.Data;
using Budget.Infrastructure.Data.Repositories;
using Budget.Infrastructure.Excel;
using Budget.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Budget.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<BudgetDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql =>
                {
                    npgsql.MigrationsAssembly(typeof(BudgetDbContext).Assembly.FullName);
                    npgsql.EnableRetryOnFailure(3);
                });
        });

        // Repositories
        services.AddScoped<IBudgetRequestRepository, BudgetRequestRepository>();
        services.AddScoped<IImportRunRepository, ImportRunRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IFieldSchemaRepository, FieldSchemaRepository>();
        services.AddScoped<IDimensionValueRepository, DimensionValueRepository>();

        // File Storage
        services.Configure<FileStorageOptions>(configuration.GetSection(FileStorageOptions.Section));
        
        var storageType = configuration.GetSection(FileStorageOptions.Section)
            .GetValue<string>("StorageType") ?? "Local";

        if (storageType.Equals("SharePoint", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<IFileStorage, SharePointGraphStorage>();
        }
        else
        {
            services.AddScoped<IFileStorage, LocalFileStorage>();
        }

        // Excel Services
        services.Configure<ExcelParserOptions>(configuration.GetSection(ExcelParserOptions.Section));
        services.AddScoped<IExcelParser, ClosedXmlExcelParser>();
        services.AddScoped<IExcelExporter, ClosedXmlExcelExporter>();

        return services;
    }
}

