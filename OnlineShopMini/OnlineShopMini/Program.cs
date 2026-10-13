using ShopMini;
using ShopMini.Ocp;
using ShopMini.Srp;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Kho du lieu in-memory (dung chung -> Singleton)
builder.Services.AddSingleton<ShopData>();

// OCP: dang ky 4 strategy. Them loai moi chi can them 1 dong o day.
builder.Services.AddScoped<IShippingFeeStrategy, StandardShipping>();
builder.Services.AddScoped<IShippingFeeStrategy, ExpressShipping>();
builder.Services.AddScoped<IShippingFeeStrategy, SameDayShipping>();
builder.Services.AddScoped<IShippingFeeStrategy, InternationalShipping>();
builder.Services.AddScoped<ShippingFeeService>();
builder.Services.AddSingleton<LegacyShippingCalculator>();   // ban vi pham OCP (doi chieu)

// SRP
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<BadOrderManager>();               // ban vi pham SRP (doi chieu)

var app = builder.Build();

app.MapControllers();

app.MapGet("/", () => Results.Content(
    """
    === OnlineShop Mini - Demo SOLID phan 1 (SRP + OCP) - 1 project ===

      GET  /api/products
      GET  /api/orders

    OCP (tinh phi van chuyen):
      GET  /api/shipping/methods
      GET  /api/shipping/fee-preview?method=Express&itemsAmount=450000         (AP DUNG - Strategy)
      GET  /api/shipping/legacy-fee-preview?method=Express&itemsAmount=450000  (VI PHAM - switch-case)

    SRP (dat hang):
      POST /api/orders       (AP DUNG - tach Data + Shipping + Email)
      POST /api/orders/bad   (VI PHAM - 1 God class om 5 trach nhiem)

    Body mau POST /api/orders:
      { "customerName": "Le Van C", "email": "c@shop.vn", "productId": 1, "quantity": 2, "shippingMethod": 2 }
    """, "text/plain; charset=utf-8"));

app.Run();
