using Shop.Business;
using Shop.DataAccess;
using Shop.Presentation.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Dang ky dich vu tang DataAccess & Business qua Extension Methods (giong WebLibrary)
builder.Services.AddDataAccessServices(builder.Configuration);
builder.Services.AddBusinessServices(builder.Configuration);

var app = builder.Build();

// Bat loi tap trung
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

// Trang chu liet ke nhanh cac endpoint de demo
app.MapGet("/", () => Results.Content(
    """
    === Online Shop - Bai 11: Tach moi truong Dev/Staging/Prod + JWT + Rollback ===

    MOI TRUONG (Bai 11):
      GET  /api/env/info                                  Thong tin moi truong + build tag (FE doc de hien badge)

    DANG NHAP JWT (Bai 11):
      POST /api/auth/login    body {"username":"alice","password":"x"}   -> tra JWT ky bang secret moi truong
      GET  /api/auth/me       header: Authorization: Bearer <token>       -> CAN TOKEN; token sai moi truong -> 401

    SAN PHAM:
      GET  /api/products                                  Danh sach san pham (du lieu that)

    OCP (Open/Closed) - tinh phi van chuyen:
      GET  /api/shipping/methods                          Cac phuong thuc van chuyen
      GET  /api/shipping/fee-preview?method=Express&itemsAmount=450000        (AP DUNG OCP - Strategy)
      GET  /api/shipping/legacy-fee-preview?method=Express&itemsAmount=450000 (VI PHAM OCP - switch-case)

    SRP (Single Responsibility) - dat hang:
      GET  /api/orders                                    Danh sach don hang
      POST /api/orders        (AP DUNG SRP - tach Repository + ShippingFee + Email)
      POST /api/orders/bad    (VI PHAM SRP - 1 God class om 5 trach nhiem)

    Body mau cho POST /api/orders:
      { "customerName": "Le Van C", "email": "c@shop.vn", "productId": 1, "quantity": 2, "shippingMethod": 2 }

    Xem README.md de biet lenh curl mau day du.
    """, "text/plain; charset=utf-8"));

app.Run();
