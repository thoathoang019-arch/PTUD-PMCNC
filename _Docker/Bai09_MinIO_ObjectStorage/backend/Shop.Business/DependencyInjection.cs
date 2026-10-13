using Minio;
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
        // ── Đăng ký Strategy OCP (từ các buổi trước) ──
        services.AddScoped<IShippingFeeStrategy, StandardShippingStrategy>();
        services.AddScoped<IShippingFeeStrategy, ExpressShippingStrategy>();
        services.AddScoped<IShippingFeeStrategy, SameDayShippingStrategy>();
        services.AddScoped<IShippingFeeStrategy, InternationalShippingStrategy>();

        // Legacy phục vụ đối chiếu demo
        services.AddSingleton<LegacyShippingCalculator>();
        services.AddScoped<BadOrderManager>();

        // Application services (từ các buổi trước)
        services.AddScoped<IShippingFeeService, ShippingFeeService>();
        services.AddScoped<IEmailService, ConsoleEmailService>();
        services.AddScoped<IProductAppService, ProductAppService>();
        services.AddScoped<IShippingAppService, ShippingAppService>();
        services.AddScoped<IOrderAppService, OrderAppService>();

        // ══════════════════════════════════════════════════════════
        // ── MỚI (Bài 9): Đăng ký MinIO Client + Storage Service ──
        // ══════════════════════════════════════════════════════════
        //
        // IMinioClient: Client kết nối tới MinIO/S3 server
        //   - Endpoint: địa chỉ MinIO server (trong Docker dùng tên service "minio")
        //   - AccessKey/SecretKey: cặp khóa xác thực (giống username/password)
        //   - WithSSL(): bật HTTPS (tắt cho local dev)
        //
        // Đọc cấu hình từ biến môi trường (truyền qua docker-compose):
        //   MINIO_ENDPOINT, MINIO_ACCESS_KEY, MINIO_SECRET_KEY, MINIO_USE_SSL

        var minioEndpoint  = configuration["MinIO:Endpoint"]  ?? "minio:9000";
        var minioAccessKey = configuration["MinIO:AccessKey"]  ?? "minioadmin";
        var minioSecretKey = configuration["MinIO:SecretKey"]  ?? "minioadmin";
        var minioUseSsl    = configuration["MinIO:UseSSL"]     ?? "false";

        services.AddSingleton<IMinioClient>(_ =>
        {
            var client = new MinioClient()
                .WithEndpoint(minioEndpoint)
                .WithCredentials(minioAccessKey, minioSecretKey);

            if (minioUseSsl.Equals("true", StringComparison.OrdinalIgnoreCase))
                client = client.WithSSL();

            return client.Build();
        });

        // Đăng ký IObjectStorageService → MinioStorageService
        // Nhờ DIP (SOLID): controller chỉ biết interface, không biết MinIO cụ thể
        // → Đổi sang AWS S3/Cloudflare R2 chỉ cần thay MinioStorageService
        //   bằng S3StorageService (implement cùng interface)
        services.AddScoped<IObjectStorageService, MinioStorageService>();

        return services;
    }
}
