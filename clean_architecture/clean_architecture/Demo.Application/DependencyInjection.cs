using Microsoft.Extensions.DependencyInjection;

namespace Demo.Application;

/// <summary>
/// Extension method để đăng ký các services của Application layer vào DI container.
/// Mỗi layer tự chịu trách nhiệm đăng ký các services của mình — Single Responsibility.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Đăng ký MediatR — tự động quét và đăng ký tất cả Handlers trong assembly này
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }
}
