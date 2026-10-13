using ShopMini;
using ShopMini.Ocp;
using ShopMini.Srp;
using ShopMini.Isp;
using ShopMini.Dip;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// ===== Bài 1 (kế thừa): SRP + OCP =====
builder.Services.AddSingleton<ShopData>();
builder.Services.AddScoped<IShippingFeeStrategy, StandardShipping>();
builder.Services.AddScoped<IShippingFeeStrategy, ExpressShipping>();
builder.Services.AddScoped<IShippingFeeStrategy, SameDayShipping>();
builder.Services.AddScoped<IShippingFeeStrategy, InternationalShipping>();
builder.Services.AddScoped<ShippingFeeService>();
builder.Services.AddSingleton<LegacyShippingCalculator>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<BadOrderManager>();

// ===== Bài 2 — ISP: tách interface đọc (khách) và quản lý (admin) =====
builder.Services.AddSingleton<OrderStore>();
builder.Services.AddScoped<IOrderReadService, CustomerOrderService>();
builder.Services.AddScoped<IOrderManageService, AdminOrderService>();

// ===== Bài 2 — DIP: inject abstraction qua DI =====
builder.Services.AddScoped<IOrderStore, InMemoryOrderStore>();
// Đổi DUY NHẤT dòng dưới sang SmsSender là cả hệ thống gửi SMS,
// KHÔNG cần sửa OrderPlacementService -> đó là sức mạnh của DIP.
builder.Services.AddScoped<INotificationSender, EmailSender>();
builder.Services.AddScoped<OrderPlacementService>();
// (LSP: controller tự khởi tạo Voucher nên không cần đăng ký DI.)

var app = builder.Build();

app.MapControllers();

app.MapGet("/", () => Results.Content(
    """
    === OnlineShop - Bài 2: SOLID phần 2 (LSP + ISP + DIP) — nghiệp vụ E-commerce ===

    BÀI 1 (kế thừa): SRP + OCP
      GET  /api/products | GET /api/orders
      GET  /api/shipping/fee-preview?method=Express&itemsAmount=450000        (OCP - áp dụng)
      GET  /api/shipping/legacy-fee-preview?method=Express&itemsAmount=450000 (OCP - vi phạm)
      POST /api/orders  (SRP - áp dụng)   |  POST /api/orders/bad (SRP - vi phạm)

    BÀI 2 (viết lại theo nghiệp vụ thật):
      LSP - Voucher (mã giảm giá):
        GET  /api/lsp/violation?orderAmount=500000   (class con ném Exception -> sập API)
        GET  /api/lsp/solution?orderAmount=500000    (trả VoucherResult -> API mượt)
      ISP - Phân quyền thao tác đơn hàng:
        GET  /api/isp/violation   (Fat interface ép Customer ném NotSupportedException)
        GET  /api/isp/solution    (tách IOrderReadService / IOrderManageService)
      DIP - Lưu đơn + gửi thông báo:
        GET  /api/dip/violation   (new EmailSender + GetType().Name)
        GET  /api/dip/solution    (inject IOrderStore + INotificationSender qua DI)
    """, "text/plain; charset=utf-8"));

app.Run();
