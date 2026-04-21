using Microsoft.Extensions.DependencyInjection;
using GoodHamburger.Application.Services;
using GoodHamburger.Data.Repository;
using GoodHamburger.Domain;
using GoodHamburger.Domain.Interfaces;
using GoodHamburger.Infrastructure.Services;
using FluentValidation;
using GoodHamburger.Domain.Validators;

namespace GoodHamburger.Infrastructure.Extensions;

/// <summary>
/// Dependency injection extensions for repositories and services
/// Uses Scrutor for automatic assembly scanning and registration
/// </summary>
public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplicationDependencies(this IServiceCollection services, AppSettings appSettings)
    {
        // Scan and register ALL repositories automatically using AsMatchingInterface()
        // AsMatchingInterface() registers only the interface that matches the class name:
        //
        // ✅ ProductRepository → IProductRepository
        // ✅ OrderRepository → IOrderRepository
        // ✅ ProductDapperRepository → IProductDapperRepository (NOT IRepository<Product>)
        // ✅ ProductAdoRepository → IProductAdoRepository (NOT IRepository<Product>)
        // ✅ OrderDapperRepository → IOrderDapperRepository (NOT IRepository<Order>)
        // ✅ OrderAdoRepository → IOrderAdoRepository (NOT IRepository<Order>)
        //
        // This prevents conflicts: alternative ORMs won't overwrite base IRepository<T>
        // Tests use IRepository<T> → EF Core InMemory ✅
        // Production can inject specific ORMs: IProductDapperRepository → ProductDapperRepository ✅
        services.Scan(scan => scan
            .FromAssembliesOf(typeof(Repository<>))
            .AddClasses(classes => classes.AssignableTo(typeof(IRepository<>)))
            .AsMatchingInterface()
            .WithScopedLifetime()
        );

        // Scan and register services
        services.Scan(scan => scan
            .FromAssembliesOf(typeof(Service<>))
            .AddClasses(classes => classes.AssignableTo(typeof(IService<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime()
        );

        // Register generic repository and service (fallback)
        if (appSettings.Infrastructure.EventSourcing.Enabled)
        {
            services.AddScoped(typeof(IRepository<>), typeof(HybridRepository<>));
        }
        else
        {
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        }
        services.AddScoped(typeof(IService<>), typeof(Service<>));

        // Register execution context service (provides user/metadata without HTTP coupling)
        services.AddScoped<IExecutionContextService, ExecutionContextService>();

        // Register all validators from the Domain assembly
        services.AddValidatorsFromAssemblyContaining<CreateProductValidator>();

        return services;
    }
}
