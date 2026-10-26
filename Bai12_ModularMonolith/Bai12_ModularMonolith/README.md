# Bài 12 — E-commerce Modular Monolith (.NET 10)

Một project Modular Monolithic cho bài toán thương mại điện tử, đủ 3 phần demo bắt buộc:
cấu trúc module (schema riêng, giao tiếp qua interface), **gọi đồng bộ REST** (Order → Inventory),
và **gọi bất đồng bộ RabbitMQ** (OrderCreated fanout tới nhiều consumer).

## Cấu trúc (ranh giới module rõ ràng)

```
Bai12_ModularMonolith/
├─ src/
│  ├─ ECommerce.Shared/                 # HỢP ĐỒNG dùng chung (các "đường cắt")
│  │  ├─ Contracts/Inventory.cs         #   IInventoryService, StockCheckResult
│  │  ├─ Events/OrderCreatedEvent.cs    #   event fanout
│  │  └─ Messaging/*.cs                 #   IEventBus, IIntegrationEventHandler, đăng ký handler
│  ├─ ECommerce.Infrastructure.Messaging/
│  │  ├─ InProcessEventBus.cs           #   bus in-process (mặc định, chạy KHÔNG cần broker)
│  │  └─ RabbitMqEventBus.cs            #   bus RabbitMQ thật (RabbitMQ.Client 6.8.1)
│  ├─ Modules/
│  │  ├─ ECommerce.Modules.Order/       #   store riêng + OrderService (sync check + publish)
│  │  ├─ ECommerce.Modules.Inventory/   #   store riêng + IInventoryService + REST + consumer trừ kho
│  │  └─ ECommerce.Modules.Notification/#   consumer gửi email (giả lập)
│  └─ ECommerce.Api/                    # HOST — composition root, nối các "đường cắt"
├─ docker-compose.yml                   # RabbitMQ + app
├─ Dockerfile                           # multi-stage .NET 10
└─ ECommerce.ModularMonolith.slnx
```

**Hai quy tắc xương sống đã tuân thủ** (kiểm bằng code):
- Mỗi module một store/"schema" riêng (`InventoryStore`, `OrderStore`) — module khác không chạm vào.
- Module **không** tham chiếu lẫn nhau: Order chỉ biết `IInventoryService` + `IEventBus` từ `Shared`, không hề import project Inventory.

## Chạy nhanh (chế độ InProcess — không cần RabbitMQ)

```bash
cd src/ECommerce.Api
dotnet run
# mở http://localhost:5xxx  (xem cổng ở log), hoặc dùng curl bên dưới
```

## Demo 1 — Cấu trúc module + gọi ĐỒNG BỘ (REST)

```bash
# Tồn kho ban đầu: SKU-LAPTOP=5, SKU-PHONE=20, SKU-TABLET=0
curl http://localhost:8080/api/inventory
curl "http://localhost:8080/api/inventory/check?sku=SKU-LAPTOP&qty=2"

# Tạo đơn ĐỦ hàng -> 200 (Order gọi đồng bộ Inventory.check trước khi tạo)
curl -X POST http://localhost:8080/api/orders -H 'Content-Type: application/json' \
  -d '{"customerName":"An","sku":"SKU-LAPTOP","quantity":2,"unitPrice":15000000}'

# Tạo đơn HẾT hàng -> 422 (SKU-TABLET còn 0) — chứng minh gọi đồng bộ chặn được đơn
curl -X POST http://localhost:8080/api/orders -H 'Content-Type: application/json' \
  -d '{"customerName":"Binh","sku":"SKU-TABLET","quantity":1,"unitPrice":9000000}'
```

Chuyển "đường cắt" sang **gọi qua HTTP/REST** (mô phỏng Inventory đã tách microservice): đặt
`Inventory:Mode=Http` (và `Inventory:BaseUrl`) — `OrderService` **không đổi dòng nào**, chỉ host nối khác.

## Demo 2 — Gọi BẤT ĐỒNG BỘ (RabbitMQ) + fanout

```bash
# Chạy kèm RabbitMQ broker:
docker compose up -d --build
# App: http://localhost:8080 | RabbitMQ Management: http://localhost:15672 (guest/guest)
```

Tạo một đơn đủ hàng → Order publish `OrderCreated` lên fanout exchange → **2 consumer** cùng nhận:
Inventory trừ kho, Notification gửi email (xem log app, và Exchange/Queue trong Management UI).

## Demo 3 — Tắt một Consumer, message vẫn nằm chờ trong queue

Chế độ InProcess có sẵn endpoint điều khiển consumer:

```bash
curl http://localhost:8080/api/demo/consumers                       # xem trạng thái
curl -X POST http://localhost:8080/api/demo/consumers/inventory/pause   # TẮT consumer Inventory
# Tạo thêm vài đơn...
curl -X POST http://localhost:8080/api/orders -H 'Content-Type: application/json' \
  -d '{"customerName":"Cuong","sku":"SKU-PHONE","quantity":1,"unitPrice":8000000}'
curl http://localhost:8080/api/demo/consumers                       # inventory: pending > 0 (message ĐANG CHỜ)
curl -X POST http://localhost:8080/api/demo/consumers/inventory/resume  # BẬT lại -> xử lý nốt
```

Với RabbitMQ: queue durable giữ message ngay cả khi consumer/broker tắt — xem Queue trong Management UI.
→ Bất đồng bộ **không** đồng nghĩa dễ mất dữ liệu.

## Tách Inventory thành microservice — 4 bước (không viết lại logic)

1. Tách `InventoryStore` → database vật lý riêng của Inventory.
2. Đổi `Inventory:Mode=Http` (hoặc gRPC) — `OrderService` giữ nguyên vì chỉ phụ thuộc `IInventoryService`.
3. Đóng gói module Inventory thành image Docker riêng (Dockerfile riêng).
4. Pipeline CI/CD riêng cho service Inventory (kế thừa Buổi 10–11).

## Đã kiểm thử (build này)

| Phần | Cách kiểm | Kết quả |
|---|---|---|
| Ranh giới module | grep ProjectReference / using | PASS (không module nào ref module khác; Order không import Inventory) |
| Logic pub/sub + pause | mô phỏng InProcessEventBus bằng Node | PASS (fanout 2 consumer; pause giữ message; resume drain) |
| Logic check kho đồng bộ | mô phỏng OrderService | PASS (đủ hàng OK, hết hàng từ chối) |
| docker-compose | `docker compose config` | PASS |
| JSON (appsettings) | `JSON.parse` | PASS |
| C# (toàn bộ .cs) | cân bằng ngoặc + namespace | PASS về cấu trúc |

> ⚠️ `dotnet build`/`dotnet test`, RabbitMQ thật và `docker build` **chưa chạy trong môi trường
> dựng gói này** (không có .NET 10 SDK và Docker daemon ở đây). Code viết đúng .NET 10; chỉ một gói
> NuGet ngoài là `RabbitMQ.Client 6.8.1` (đã ghim phiên bản để khớp API). Hãy chạy
> `dotnet run` ở `src/ECommerce.Api` trên máy bạn để xác nhận — logic lõi (fanout/pause/sync-check)
> đã được kiểm chứng độc lập bằng Node.
