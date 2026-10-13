using Demo.API.Middleware;
using Demo.Application;
using Demo.Infrastructure;
using Demo.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Demo chạy được với quyền người dùng thông thường trên Windows:
// tránh EventLog provider yêu cầu quyền ghi Windows Event Log.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// =====================================================
// ĐĂNG KÝ SERVICES (Dependency Injection)
// Mỗi layer tự đăng ký services của mình thông qua extension methods.
// Đây là nơi DUY NHẤT biết về tất cả các layer — Composition Root.
// =====================================================

builder.Services.AddControllers();

// Swagger / OpenAPI để test API
builder.Services.AddOpenApi();

// Đăng ký services từ Application layer (MediatR handlers)
builder.Services.AddApplicationServices();

// Đăng ký services từ Infrastructure layer (DbContext, Repositories)
builder.Services.AddInfrastructureServices();

var app = builder.Build();

// =====================================================
// SEED DATA: Đảm bảo database có dữ liệu mẫu khi khởi động
// =====================================================
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}

// =====================================================
// CONFIGURE MIDDLEWARE PIPELINE
// =====================================================

// Global Exception Handler — bắt mọi lỗi, trả về JSON response chuẩn
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // Thêm Swagger UI để dễ dàng test API
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Clean Architecture Demo API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
