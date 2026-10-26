using Demo.Domain.Interfaces;
using Demo.Infrastructure.Data;
using Demo.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.Infrastructure;

/// <summary>
/// Extension method để đăng ký các services của Infrastructure layer.
/// Bao gồm: DbContext (In-Memory DB) và các Repository implementations.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Đăng ký DbContext với In-Memory Database (phù hợp cho demo)
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("CleanArchitectureDemo"));

        // Đăng ký Repository — map interface (Domain) sang implementation (Infrastructure)
        services.AddScoped<IProductRepository, ProductRepository>();

        return services;
    }
}
