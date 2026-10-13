# Hướng dẫn thuyết trình & demo — Bài 2: LSP + ISP + DIP (nghiệp vụ E-commerce)

Toàn bộ ví dụ là nghiệp vụ thật của hệ thống đặt hàng online. Mỗi nguyên lý có 2 endpoint
`violation` / `solution` để bấm chạy trực tiếp.

## Phần 1. Ba câu cần thuộc

- **LSP** — lớp con phải thay thế được lớp cha mà không phá hợp đồng. Ví dụ: `OneTimeVoucher`
  hết lượt thì **trả kết quả thất bại**, KHÔNG ném Exception làm sập API.
- **ISP** — đừng ép client phụ thuộc interface không dùng. Tách `IOrderReadService` (khách) khỏi
  `IOrderManageService` (admin), thay vì một Fat `IOrderService`.
- **DIP** — phụ thuộc abstraction, **inject qua DI**; bỏ `new` class cụ thể và bỏ `GetType().Name`.

## Phần 2. Chạy & thử API

```powershell
cd "D:\PTUD PMCNC\OnlineShop_Bai02"
dotnet run
```
Mở `http://localhost:5113`. Tất cả endpoint L/I/D đều là **GET** nên dán thẳng URL vào trình duyệt,
hoặc mở `requests.http` bấm "Send Request".

## Phần 3. Kịch bản demo (10–15 phút/nguyên lý)

### LSP — Voucher
1. Mở `Lsp/VoucherViolation.cs`, chỉ `OneTimeVoucherBad.Apply()` ném `Exception` khi hết lượt.
2. Gọi `GET /api/lsp/violation?orderAmount=500000` → **HTTP 500**. "Class con phá hợp đồng class cha
   → làm sập API."
3. Mở `Lsp/VoucherStrategy.cs`, chỉ `IVoucherStrategy` trả `VoucherResult`. Gọi
   `GET /api/lsp/solution?orderAmount=500000` → **HTTP 200**, lần 1 `Success=true`, lần 2
   `Success=false` (mượt, không sập). "Đó là tuân thủ LSP."

### ISP — Phân quyền đơn hàng
1. Mở `Isp/OrderOperations.cs`, chỉ Fat `IOrderServiceFat` và `CustomerOrderServiceBad.DeleteOrder()`
   ném `NotSupportedException`.
2. Gọi `GET /api/isp/violation` → thấy `deleteError = "Khach hang khong duoc xoa don!"`.
3. Chỉ 2 interface tách nhỏ + gọi `GET /api/isp/solution` → khách đọc đơn, admin đổi trạng thái sang
   "Delivered". "Mỗi bên chỉ phụ thuộc thứ mình cần."

### DIP — Lưu đơn + gửi thông báo
1. Mở `Dip/OrderPlacement.cs`, chỉ `BadOrderPlacementService`: `new EmailSender()` + `GetType().Name`.
   Gọi `GET /api/dip/violation` → `channel = "EmailSender"`.
2. Chỉ `OrderPlacementService` nhận `IOrderStore` + `INotificationSender` qua constructor; tên kênh lấy
   từ property `Channel`. Gọi `GET /api/dip/solution` → `channel = "Email"`.
3. **Đòn quyết định:** mở `Program.cs`, đổi 1 dòng `AddScoped<INotificationSender, EmailSender>()` →
   `SmsSender`, chạy lại, gọi lại `solution` → `channel = "SMS"`. "Đổi kênh mà không sửa service — DIP."

## Phần 4. Câu hỏi thầy hay hỏi & cách đáp gọn

- **"Vì sao ném Exception trong Voucher lại vi phạm LSP?"** → Lớp cha cam kết trả số tiền; lớp con ném
  Exception là **thu hẹp/thay đổi hành vi** → không thay thế an toàn được.
- **"Sao không validate trước cho khỏi Exception?"** → Vấn đề không phải validate mà là **hợp đồng của
  kiểu**: dùng kiểu trả về (`VoucherResult`) để lỗi nghiệp vụ là trạng thái, không phải sự cố.
- **"ISP khác SRP?"** → SRP về *class một trách nhiệm*; ISP về *interface đừng bắt implement thừa*.
- **"DIP có phải Dependency Injection?"** → DIP là nguyên lý (phụ thuộc abstraction); DI là kỹ thuật
  để đạt DIP. Ở đây dùng DI của .NET Core.
- **"Vì sao bỏ GetType().Name?"** → Nó lấy tên class **cụ thể** (kết nối chặt, khó test). Dùng property
  `Channel` trên abstraction là cách đúng.

## Phần 5. Checklist
- [ ] 6 endpoint chạy: lsp/isp/dip × violation/solution.
- [ ] LSP violation ra **500**, solution ra **200**.
- [ ] Chỉ được đúng class vi phạm và class giải pháp cho mỗi nguyên lý.
- [ ] Diễn được màn đổi 1 dòng DI (Email → SMS) cho DIP.
- [ ] Trả lời được các câu hỏi Phần 4.
