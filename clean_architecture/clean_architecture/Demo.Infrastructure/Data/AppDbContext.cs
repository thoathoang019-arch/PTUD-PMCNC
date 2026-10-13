using Demo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Demo.Infrastructure.Data;

/// <summary>
/// DbContext — cầu nối giữa ứng dụng và cơ sở dữ liệu.
/// Được đặt ở Infrastructure layer vì đây là chi tiết triển khai (implementation detail),
/// không thuộc về business logic.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Cấu hình entity Product
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Description).HasMaxLength(1000);
            entity.Property(p => p.Price).HasPrecision(18, 2);
            entity.Property(p => p.Category).HasMaxLength(100);
        });

        // Seed data — dữ liệu mẫu để demo
        modelBuilder.Entity<Product>().HasData(
            new
            {
                Id = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                Name = "iPhone 15 Pro Max",
                Description = "Điện thoại cao cấp Apple với chip A17 Pro",
                Price = 34990000m,
                StockQuantity = 50,
                Category = "Điện thoại",
                CreatedAt = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc)
            },
            new
            {
                Id = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901"),
                Name = "Samsung Galaxy S24 Ultra",
                Description = "Flagship Samsung với bút S-Pen tích hợp AI",
                Price = 31990000m,
                StockQuantity = 35,
                Category = "Điện thoại",
                CreatedAt = new DateTime(2024, 2, 20, 0, 0, 0, DateTimeKind.Utc)
            },
            new
            {
                Id = Guid.Parse("c3d4e5f6-a7b8-9012-cdef-123456789012"),
                Name = "MacBook Pro M3 14 inch",
                Description = "Laptop chuyên nghiệp với chip Apple M3",
                Price = 49990000m,
                StockQuantity = 20,
                Category = "Laptop",
                CreatedAt = new DateTime(2024, 3, 10, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
