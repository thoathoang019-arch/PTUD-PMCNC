# Online Shop — Demo SOLID phần 1 (SRP + OCP) — ASP.NET Core

Bài làm "tự tìm hiểu" của nhóm **không trình bày**: một **ứng dụng backend thật, chạy được,
có API, có dữ liệu thật**, minh họa đúng hai nguyên lý của buổi 2:

- **SRP – Single Responsibility Principle** (Một class chỉ có một lý do để thay đổi)
- **OCP – Open/Closed Principle** (Mở để mở rộng, đóng để sửa đổi)

Kiến trúc **3 lớp** (Business / DataAccess / Presentation) đúng phong cách dự án `WebLibrary`,
nhưng ví dụ được viết lại theo một domain gần gũi khác: **đặt hàng & tính phí vận chuyển của một
cửa hàng online**. Với mỗi nguyên lý đều có **2 phiên bản để đối chiếu**: bản **VI PHẠM** và
bản **ÁP DỤNG**.

> Lưu trữ dùng **in-memory** (có seed sẵn dữ liệu thật) để chạy được ngay, không phụ thuộc mạng
> hay database ngoài. Cách đổi sang **EF Core + SQLite** xem mục 6.

---

## 1. Yêu cầu môi trường

- .NET SDK 10.0 trở lên (`dotnet --version`). Dự án target `net10.0`.

## 2. Cách chạy

```bash
cd OnlineShop
dotnet run --project Shop.Presentation
```

Xong sẽ thấy `Now listening on: http://localhost:5113`. Mở trình duyệt vào địa chỉ đó để xem
danh sách endpoint (trang chủ in sẵn hướng dẫn).

---

## 3. Cấu trúc solution

```
OnlineShop/
├── OnlineShop.sln
├── Shop.Business/            # Tầng nghiệp vụ (không phụ thuộc web/DB)
│   ├── Entities/             #   Product, Order            (SRP: chỉ giữ dữ liệu)
│   ├── Enums/                #   ShippingMethod
│   ├── DTOs/                 #   Dữ liệu vào/ra API
│   ├── Interfaces/           #   Hợp đồng cho từng trách nhiệm
│   ├── Strategies/           #   OCP: IShippingFeeStrategy + 4 class con
│   ├── Services/             #   ShippingFeeService, OrderAppService, EmailService...
│   ├── Legacy/               #   BadOrderManager (vi phạm SRP), LegacyShippingCalculator (vi phạm OCP)
│   └── DependencyInjection.cs#   AddBusinessServices(...)
├── Shop.DataAccess/          # Tầng dữ liệu (in-memory, có thể thay bằng EF Core)
│   ├── Persistence/InMemoryDataStore.cs
│   ├── Persistence/Repositories/
│   └── DependencyInjection.cs#   AddDataAccessServices(...)
└── Shop.Presentation/        # Tầng API
    ├── Controllers/          #   Products, Shipping, Orders
    ├── Middleware/           #   ExceptionHandlingMiddleware (map lỗi -> HTTP status)
    └── Program.cs
```

---

## 4. SRP — Single Responsibility Principle

**Ý tưởng:** thay vì một class "ôm đồm", tách mỗi trách nhiệm thành một lớp riêng.

| Trách nhiệm | Lớp phụ trách (bản ÁP DỤNG) |
|---|---|
| Giữ dữ liệu | `Entities/Order.cs`, `Entities/Product.cs` |
| Lưu / truy xuất | `IOrderRepository`, `IProductRepository` + `Repositories/*` |
| Tính phí vận chuyển | `ShippingFeeService` (dùng Strategy) |
| Gửi email xác nhận | `IEmailService` → `ConsoleEmailService` |
| Điều phối nghiệp vụ đặt hàng | `Services/OrderAppService.cs` |

Bản **vi phạm** để so sánh: `Legacy/BadOrderManager.cs` — **một class ôm 5 trách nhiệm**
(validate + switch-case tính phí + lưu dữ liệu + định dạng hóa đơn + ghi log) → có tới 5 lý do
phải sửa, khó test/bảo trì.

### Demo bằng curl

```bash
B=http://localhost:5113

# ✅ ÁP DỤNG SRP — đặt hàng (tự lưu đơn + trừ tồn kho + gửi email qua các lớp riêng)
curl -X POST $B/api/orders -H "Content-Type: application/json" \
  -d '{"customerName":"Le Van C","email":"c@shop.vn","productId":1,"quantity":2,"shippingMethod":2}'

# Danh sách đơn hàng
curl $B/api/orders

# ❌ VI PHẠM SRP — cùng nghiệp vụ nhưng do 1 God class làm hết (để đối chiếu)
curl -X POST $B/api/orders/bad -H "Content-Type: application/json" \
  -d '{"customerName":"Bad Guy","email":"bad@shop.vn","productId":4,"quantity":1,"shippingMethod":3}'
```

`shippingMethod`: `1=Standard, 2=Express, 3=SameDay, 4=International`.

---

## 5. OCP — Open/Closed Principle

**Ý tưởng:** tính phí vận chuyển theo **loại phương thức** bằng **đa hình** thay vì `switch-case`.

- `Strategies/IShippingFeeStrategy.cs` + `StandardShippingStrategy`, `ExpressShippingStrategy`,
  `SameDayShippingStrategy`, `InternationalShippingStrategy` — mỗi loại tự tính phí của mình.
- `Services/ShippingFeeService.cs` — chỉ chọn strategy phù hợp qua DI, **không có if/switch**,
  nên thêm loại mới thì class này **không đổi**.

Bản **vi phạm** để so sánh: `Legacy/LegacyShippingCalculator.cs` — dùng `switch-case`, thêm
phương thức mới là phải mở class ra sửa.

Bảng phí dùng trong demo:

| Phương thức | Phí |
|---|---|
| Standard | 15.000đ (miễn phí nếu đơn ≥ 500.000đ) |
| Express | 30.000đ |
| SameDay | 50.000đ |
| International | 200.000đ *(loại mở rộng thêm sau — minh họa OCP)* |

### Demo bằng curl

```bash
B=http://localhost:5113

curl $B/api/shipping/methods

# ✅ ÁP DỤNG OCP (Strategy, không if/switch)
curl "$B/api/shipping/fee-preview?method=Express&itemsAmount=450000"
curl "$B/api/shipping/fee-preview?method=Standard&itemsAmount=600000"     # freeship
curl "$B/api/shipping/fee-preview?method=International&itemsAmount=450000" # loại mở rộng

# ❌ VI PHẠM OCP (switch-case) — để đối chiếu
curl "$B/api/shipping/legacy-fee-preview?method=Express&itemsAmount=450000"
```

### Chứng minh "mở để mở rộng, đóng để sửa đổi"

Muốn thêm phương thức **Drone** (giao bằng drone), ta **chỉ cần**:

1. Thêm `ShippingMethod.Drone` vào enum.
2. Thêm class `DroneShippingStrategy : IShippingFeeStrategy` với công thức riêng.
3. Thêm 1 dòng `services.AddScoped<IShippingFeeStrategy, DroneShippingStrategy>();` trong DI.

**Không sửa** `ShippingFeeService` và **không đụng** các strategy cũ. Còn bản `switch-case` thì
luôn phải mở đúng hàm cũ ra thêm một `case` mới (rủi ro làm hỏng logic đang chạy).

---

## 6. Xử lý lỗi cơ bản (đã kiểm thử)

Lỗi được gom về `ExceptionHandlingMiddleware` và trả HTTP status phù hợp:

| Tình huống | Kết quả |
|---|---|
| Đặt hàng hợp lệ | `201 Created` |
| Sai ràng buộc dữ liệu (tên rỗng, email sai, số lượng ngoài 1..1000) | `400` + danh sách lỗi (DataAnnotations) |
| Sản phẩm không tồn tại (`productId` sai) | `404 NotFound` |
| Số lượng vượt tồn kho | `400 BadRequest` |
| Phương thức vận chuyển chưa hỗ trợ | `400 BadRequest` |

---

## 7. Đổi sang EF Core + SQLite (tùy chọn)

Kiến trúc đã tách interface nên chỉ cần thay tầng DataAccess mà **không đụng** Business/Presentation:

1. Thêm package `Microsoft.EntityFrameworkCore.Sqlite` vào `Shop.DataAccess`.
2. Viết `LibraryDbContext` + các `Repository` mới implement `IProductRepository`,
   `IOrderRepository` bằng EF Core.
3. Trong `AddDataAccessServices`, đổi `AddSingleton<InMemoryDataStore>()` thành
   `AddDbContext<...>(o => o.UseSqlite(...))`.

Đây chính là điểm mạnh của việc tuân thủ SRP + phụ thuộc vào abstraction (interface).

---

## 8. Câu hỏi có thể bị hỏi (chuẩn bị trước)

- *SRP nghĩa là mỗi class chỉ 1 hàm?* — Không. Là **một lý do để thay đổi**; một class có thể
  nhiều hàm miễn cùng phục vụ một trách nhiệm.
- *OCP nghĩa là không bao giờ sửa code?* — Không. Là hạn chế **sửa code đang chạy ổn định**;
  ưu tiên **thêm class mới** thay vì sửa cũ.
- *Vì sao dùng interface + DI?* — Để tầng trên phụ thuộc vào abstraction, dễ mở rộng và thay thế
  (nền tảng cho cả OCP lẫn DIP ở các buổi sau).
```
