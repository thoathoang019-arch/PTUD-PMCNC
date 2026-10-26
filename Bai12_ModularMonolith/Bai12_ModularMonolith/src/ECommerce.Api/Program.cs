using ECommerce.Infrastructure.Messaging;
using ECommerce.Modules.Inventory;
using ECommerce.Modules.Inventory.Application;
using ECommerce.Modules.Notification;
using ECommerce.Modules.Order;
using ECommerce.Modules.Order.Infrastructure;
using ECommerce.Shared.Contracts;
using ECommerce.Shared.Messaging;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// ── Controllers của các module (ApplicationParts: controller nằm ở project khác) ──
builder.Services.AddControllers()
    .AddApplicationPart(typeof(ECommerce.Modules.Order.Api.OrdersController).Assembly)
    .AddApplicationPart(typeof(ECommerce.Modules.Inventory.Api.InventoryController).Assembly);

// ── Đăng ký từng MODULE (mỗi module có store/schema + handler riêng) ──
builder.Services.AddInventoryModule();
builder.Services.AddOrderModule();
builder.Services.AddNotificationModule();

// ── Event bus bất đồng bộ (InProcess mặc định / RabbitMq theo cấu hình) ──
builder.Services.AddEventBus(config);

// ── ĐƯỜNG CẮT đồng bộ: Order gọi Inventory in-process HAY qua HTTP/REST ──
// Đây là NƠI DUY NHẤT quyết định cách nối. Khi tách Inventory thành microservice,
// chỉ đổi cấu hình "Inventory:Mode" = "Http" — OrderService không đổi dòng nào.
var inventoryMode = config["Inventory:Mode"] ?? "InProcess";
if (string.Equals(inventoryMode, "Http", StringComparison.OrdinalIgnoreCase))
{
    var baseUrl = config["Inventory:BaseUrl"] ?? "http://localhost:8080";
    builder.Services.AddHttpClient<IInventoryService, InventoryHttpClient>(c => c.BaseAddress = new Uri(baseUrl));
}
else
{
    builder.Services.AddScoped<IInventoryService>(sp => sp.GetRequiredService<InventoryService>());
}

var app = builder.Build();
app.MapControllers();

// ── Endpoint DEMO: xem/tắt/bật consumer (chỉ InProcess bus hỗ trợ) ──
// Dùng để demo "tắt 1 consumer trước khi tạo đơn -> message vẫn nằm chờ trong queue".
app.MapGet("/api/demo/consumers", (IServiceProvider sp) =>
{
    var ctrl = sp.GetService<IConsumerControl>();
    return ctrl is null
        ? Results.Ok(new { note = "Đang chạy RabbitMq mode — pause/resume xem ở RabbitMQ Management (cổng 15672)." })
        : Results.Ok(ctrl.GetStatus());
});
app.MapPost("/api/demo/consumers/{name}/pause", (string name, IServiceProvider sp) =>
{
    var ctrl = sp.GetService<IConsumerControl>();
    if (ctrl is null) return Results.BadRequest(new { message = "Chỉ hỗ trợ ở InProcess mode." });
    ctrl.Pause(name);
    return Results.Ok(new { paused = name });
});
app.MapPost("/api/demo/consumers/{name}/resume", (string name, IServiceProvider sp) =>
{
    var ctrl = sp.GetService<IConsumerControl>();
    if (ctrl is null) return Results.BadRequest(new { message = "Chỉ hỗ trợ ở InProcess mode." });
    ctrl.Resume(name);
    return Results.Ok(new { resumed = name });
});

app.MapGet("/", () => Results.Content(
    $"""
    === E-commerce Modular Monolith (Bai 12) ===
    Inventory:Mode = {inventoryMode}   |   Messaging:Mode = {config["Messaging:Mode"] ?? "InProcess"}

    MODULE: Order, Inventory, Notification — mỗi module store/schema riêng, giao tiếp qua interface.

    ĐỒNG BỘ (REST): Order gọi Inventory kiểm tra tồn kho trước khi tạo đơn.
      GET  /api/inventory                 Danh sach ton kho (SKU-LAPTOP=5, SKU-PHONE=20, SKU-TABLET=0)
      GET  /api/inventory/check?sku=SKU-LAPTOP&qty=2

    TAO DON (đồng bộ check + bất đồng bộ publish):
      POST /api/orders   body: {{ "customerName":"An", "sku":"SKU-LAPTOP", "quantity":2, "unitPrice":15000000 }}
        - Đủ hàng  -> 200, tạo đơn, publish OrderCreated (Inventory trừ kho, Notification gửi email)
        - Hết hàng -> 422 (vd sku=SKU-TABLET)

    BAT DONG BO (demo "tắt consumer, message vẫn chờ"):
      GET  /api/demo/consumers                        Trạng thái + số message đang chờ (Pending)
      POST /api/demo/consumers/inventory/pause        Tắt consumer Inventory
      POST /api/demo/consumers/inventory/resume       Bật lại -> xử lý tiếp các message đã chờ

    Xem README.md để biết kịch bản demo đầy đủ.
    """, "text/plain; charset=utf-8"));

app.Run();
