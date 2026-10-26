using Shop.Business.Interfaces;
using Shop.Business.Legacy;
using Shop.Business.Services;
using Shop.Business.Strategies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Shop.Business;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Dang ky cac Strategy OCP qua DI (them loai moi chi can them 1 dong o day)
        services.AddScoped<IShippingFeeStrategy, StandardShippingStrategy>();
        services.AddScoped<IShippingFeeStrategy, ExpressShippingStrategy>();
        services.AddScoped<IShippingFeeStrategy, SameDayShippingStrategy>();
        services.AddScoped<IShippingFeeStrategy, InternationalShippingStrategy>();

        // Legacy phuc vu doi chieu demo
        services.AddSingleton<LegacyShippingCalculator>();
        services.AddScoped<BadOrderManager>();

        // Application services
        services.AddScoped<IShippingFeeService, ShippingFeeService>();
        services.AddScoped<IEmailService, ConsoleEmailService>();
        services.AddScoped<IProductAppService, ProductAppService>();
        services.AddScoped<IShippingAppService, ShippingAppService>();
        services.AddScoped<IOrderAppService, OrderAppService>();

        return services;
    }
}
