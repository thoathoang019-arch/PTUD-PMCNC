# OnlineShop Mini — Demo SOLID phần 1 (SRP + OCP)

Bản **tối giản, 1 project** (chỉ ~10 file) minh họa hai nguyên lý của buổi 2:

- **SRP** — Một class chỉ có một lý do để thay đổi.
- **OCP** — Mở để mở rộng, đóng để sửa đổi.

Mỗi nguyên lý có **bản VI PHẠM** và **bản ÁP DỤNG** để đối chiếu. Dữ liệu lưu **in-memory**
(seed sẵn), chạy được ngay, không cần database.

> Đây là bản rút gọn của dự án `OnlineShop` 3 tầng: đã bỏ tách Business/DataAccess/Presentation,
> bỏ Repository/DTO/middleware để dễ hiểu và dễ demo. Bản chất SRP/OCP giữ nguyên.

## Chạy

```bash
cd OnlineShopMini
dotnet run
```

Mở `http://localhost:5113` (trang chủ liệt kê endpoint). Cần .NET SDK 10.

## Cấu trúc (10 file .cs)

```
OnlineShopMini/
├── Program.cs                     # Đăng ký DI + định tuyến (gộp tất cả ở đây)
├── Models/
│   ├── ShopModels.cs              # Product, Order, enum ShippingMethod, PlaceOrderRequest
│   └── ShopData.cs                # Kho in-memory + seed dữ liệu (thay cho Repository)
├── Ocp/
│   ├── ShippingStrategies.cs      # ✅ OCP: interface + 4 strategy (mỗi loại 1 class)
│   ├── ShippingFeeService.cs      # ✅ OCP: chọn strategy qua DI, KHÔNG if/switch
│   └── LegacyShippingCalculator.cs# ❌ OCP: switch-case (để đối chiếu)
├── Srp/
│   ├── OrderService.cs            # ✅ SRP: điều phối, ủy thác cho Data/Shipping/Email
│   ├── EmailService.cs            # ✅ SRP: chỉ lo gửi email
│   └── BadOrderManager.cs         # ❌ SRP: 1 God class ôm 5 trách nhiệm
└── Controllers/
    └── ShopController.cs          # Tất cả endpoint (try/catch trả đúng mã HTTP)
```

## SRP — đặt hàng

Bản đúng `Srp/OrderService.cs` chỉ **điều phối**: ủy thác lưu cho `ShopData`, tính phí cho
`ShippingFeeService`, gửi mail cho `EmailService`. Bản sai `Srp/BadOrderManager.cs` gộp cả 5
việc (validate + switch tính phí + lưu + in hóa đơn + log) → 5 lý do phải sửa.

```bash
B=http://localhost:5113
# ✅ đúng
curl -X POST $B/api/orders -H "Content-Type: application/json" \
  -d '{"customerName":"Le Van C","email":"c@shop.vn","productId":1,"quantity":2,"shippingMethod":2}'
# ❌ sai (God class)
curl -X POST $B/api/orders/bad -H "Content-Type: application/json" \
  -d '{"customerName":"Bad Guy","email":"bad@shop.vn","productId":4,"quantity":1,"shippingMethod":3}'
```

## OCP — phí vận chuyển

Bản đúng: mỗi phương thức là 1 strategy (`Ocp/ShippingStrategies.cs`), `ShippingFeeService`
không có if/switch. Bản sai: `LegacyShippingCalculator` dùng switch-case.

```bash
curl "$B/api/shipping/fee-preview?method=Express&itemsAmount=450000"        # ✅ đúng
curl "$B/api/shipping/legacy-fee-preview?method=Express&itemsAmount=450000" # ❌ sai
```

`shippingMethod`: `1=Standard, 2=Express, 3=SameDay, 4=International`.
Phí: Standard 15k (miễn phí nếu đơn ≥ 500k), Express 30k, SameDay 50k, International 200k.

### Chứng minh OCP: thêm phương thức mới KHÔNG sửa code cũ

Thêm `Drone = 5` vào enum, tạo class `DroneShipping : IShippingFeeStrategy` trong
`ShippingStrategies.cs`, và thêm 1 dòng `AddScoped<IShippingFeeStrategy, DroneShipping>()`
trong `Program.cs`. Không đụng `ShippingFeeService` và các strategy cũ.

## Xử lý lỗi cơ bản (đã kiểm thử)

| Tình huống | Kết quả |
|---|---|
| Đặt hàng hợp lệ | `200` |
| Sai ràng buộc (tên/email/số lượng) | `400` + danh sách lỗi |
| Sản phẩm không tồn tại | `404` |
| Vượt tồn kho | `400` |
| Phương thức vận chuyển chưa hỗ trợ | `400` |
