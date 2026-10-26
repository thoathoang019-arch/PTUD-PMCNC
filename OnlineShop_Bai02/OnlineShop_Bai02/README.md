# OnlineShop — Bài 2: SOLID phần 2 (LSP + ISP + DIP) — nghiệp vụ E-commerce

**Bài 2** kế thừa Bài 1 (SRP + OCP) và làm tiếp 3 nguyên lý còn lại **bằng nghiệp vụ thật của một hệ
thống đặt hàng online** — không dùng ví dụ lý thuyết sáo rỗng. Mỗi nguyên lý có **2 endpoint**
(`violation` và `solution`) để bấm chạy demo, dữ liệu mock thực tế, API chạy được.

## Chạy

```bash
cd OnlineShop_Bai02
dotnet run
```

Mở `http://localhost:5113` (trang chủ liệt kê mọi endpoint). Cần .NET SDK 10.
Thử API: file `requests.http` (VS Code REST Client) hoặc trình duyệt (tất cả endpoint L/I/D đều GET).

---

## Kiến thức kế thừa Bài 1 (vẫn còn trong project)
- **SRP**: `Srp/OrderService.cs` (đúng) vs `Srp/BadOrderManager.cs` (God class).
- **OCP**: `Ocp/ShippingStrategies.cs` + `ShippingFeeService.cs` (đúng) vs `LegacyShippingCalculator.cs`.

---

## 1) LSP — Xử lý Mã giảm giá (Voucher)

> Lớp con phải thay thế được lớp cha mà **không phá hợp đồng** hành vi.

**Vi phạm** (`Lsp/VoucherViolation.cs`): lớp cha `Voucher.Apply()` cam kết **trả về số tiền sau giảm**.
Lớp con `OneTimeVoucherBad` khi hết lượt lại **ném `Exception`** thay vì trả tiền → khi API gọi lại
lần 2 thì **sập (HTTP 500)**.

**Giải pháp** (`Lsp/VoucherStrategy.cs`): interface `IVoucherStrategy.Apply()` trả về
`VoucherResult(Success, Message, Discount, FinalAmount)`. `OneTimeVoucher` hết lượt thì trả
`Success=false` (không ném Exception) → **API luôn 200**, mọi voucher thay thế cho nhau được.

```
GET /api/lsp/violation?orderAmount=500000   ->  HTTP 500 (Exception làm sập)
GET /api/lsp/solution?orderAmount=500000    ->  HTTP 200 (first Success=true, second Success=false)
```

## 2) ISP — Phân quyền thao tác Đơn hàng

> Không ép client phụ thuộc interface nó không dùng.

**Vi phạm** (`Isp/OrderOperations.cs`): Fat interface `IOrderServiceFat` gộp cả thao tác của Khách
(`GetById`, `GetMyOrders`) lẫn Admin (`DeleteOrder`, `UpdateStatus`). `CustomerOrderServiceBad` (của
khách) buộc phải implement `DeleteOrder`/`UpdateStatus` → **ném `NotSupportedException`**.

**Giải pháp**: tách `IOrderReadService` (khách) và `IOrderManageService` (admin). `CustomerOrderService`
chỉ implement đọc; `AdminOrderService` implement quản lý. Cả hai **inject qua DI**.

```
GET /api/isp/violation   ->  DeleteOrder ném NotSupportedException
GET /api/isp/solution    ->  khách đọc đơn, admin đổi trạng thái sang "Delivered"
```

## 3) DIP — Lưu đơn & Gửi thông báo

> Module cấp cao phụ thuộc **abstraction**, không phụ thuộc chi tiết cụ thể.

**Vi phạm** (`Dip/OrderPlacement.cs`): `BadOrderPlacementService` tự `new EmailSender()` bên trong và
dùng `GetType().Name` để lấy tên kênh → kết nối chặt, đổi sang SMS phải sửa code.

**Giải pháp**: `OrderPlacementService` nhận `IOrderStore` + `INotificationSender` **qua constructor
(DI)**; tên kênh lấy từ **property `Channel`** (bỏ hẳn `GetType().Name`). Đổi Email↔SMS chỉ sửa **1
dòng** trong `Program.cs` (`AddScoped<INotificationSender, EmailSender>()` → `SmsSender`), **không sửa**
service.

```
GET /api/dip/violation   ->  channel lấy bằng GetType().Name = "EmailSender"
GET /api/dip/solution    ->  channel lấy từ property = "Email" (inject qua DI)
```

---

## Các class đã sửa/viết lại (so với bản cũ)

Đã **xóa** ví dụ sáo rỗng: `Lsp/RectangleSquareBad.cs`, `Lsp/Shapes.cs`, `Isp/Workers.cs`,
`Dip/Notification.cs` (dùng `GetType().Name`). Thay bằng:

```
Lsp/VoucherViolation.cs      Lsp/VoucherStrategy.cs
Isp/OrderOperations.cs
Dip/OrderPlacement.cs
Controllers/LspController.cs  Controllers/IspController.cs  Controllers/DipController.cs
Program.cs                   (đăng ký DI cho ISP + DIP)
```

## Mối liên hệ (thầy hay hỏi)
- **DIP** là lý do Bài 1 đã dùng interface + DI cho Shipping/Order.
- **ISP** giúp interface nhỏ → DIP inject đúng thứ cần; **LSP** giữ cho việc thay thế qua interface luôn an toàn.
