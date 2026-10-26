# Hướng dẫn thuyết trình & demo — OnlineShop Mini (SRP + OCP)

Tài liệu này dành cho bản **`OnlineShopMini`** (1 project, 10 file). Mục tiêu: demo cho thầy và
cho thấy bạn **học hiểu thật**. Nguyên tắc: đừng đọc định nghĩa — **kể chuyện "trước và sau"**,
chỉ ra code tệ → refactor sạch → chạy thật chứng minh.

---

## Phần 1. Hiểu bản chất, gói trong 2 câu

**SRP** — *Một class chỉ nên có một lý do để thay đổi.*
Mẹo kiểm tra: "class này có bao nhiêu lý do khiến tôi phải mở ra sửa?"

| | VI PHẠM | ÁP DỤNG |
|---|---|---|
| File | `Srp/BadOrderManager.cs` | `Srp/OrderService.cs` (+ `EmailService`, `ShopData`, `ShippingFeeService`) |
| Đặc điểm | 1 class ôm 5 việc: validate + switch tính phí + lưu + in hóa đơn + log | Điều phối, ủy thác mỗi việc cho 1 lớp riêng |

**OCP** — *Mở để mở rộng, đóng để sửa đổi.*
Mẹo kiểm tra: "thêm loại mới thì phải SỬA code cũ hay chỉ THÊM code mới?"

| | VI PHẠM | ÁP DỤNG |
|---|---|---|
| File | `Ocp/LegacyShippingCalculator.cs` | `Ocp/ShippingStrategies.cs` + `Ocp/ShippingFeeService.cs` |
| Đặc điểm | `switch-case` — thêm loại phải mở hàm cũ ra sửa | Mỗi loại 1 class strategy; service chọn qua đa hình, không if/switch |

---

## Phần 2. Chạy chương trình

Mở PowerShell tại thư mục project:

```powershell
cd "D:\PTUD PMCNC\OnlineShopMini"   # (đường dẫn nơi bạn giải nén)
dotnet run
```

Chờ dòng `Now listening on: http://localhost:5113`. **Để nguyên cửa sổ này chạy** trong suốt buổi demo.

---

## Phần 3. THỬ API Ở ĐÂU (quan trọng)

Bản này không bật Swagger, nên bạn thử API bằng 1 trong 3 cách sau. **Khuyên dùng cách A** vì trực quan nhất khi trình bày.

### Cách A — File `requests.http` trong VS Code (trực quan nhất)

1. Cài extension **"REST Client"** cho VS Code (một lần).
2. Mở file `requests.http` có sẵn trong project.
3. Phía trên mỗi request có chữ **"Send Request"** — bấm vào, kết quả hiện ngay bên phải.
   Rất hợp để demo: thầy thấy request và response cạnh nhau.

> Nếu dùng Visual Studio (bản đầy đủ), mở file `.http` và bấm nút gửi tương tự.

### Cách B — Trình duyệt (chỉ cho GET)

Dán thẳng URL vào trình duyệt là xong (các endpoint GET):

```
http://localhost:5113/
http://localhost:5113/api/products
http://localhost:5113/api/shipping/fee-preview?method=Express&itemsAmount=450000
http://localhost:5113/api/shipping/legacy-fee-preview?method=Express&itemsAmount=450000
http://localhost:5113/api/orders
```

> Trình duyệt chỉ gửi được GET. Muốn đặt hàng (POST) thì dùng cách A hoặc C.

### Cách C — PowerShell (gọi cả GET lẫn POST)

Mở **PowerShell thứ hai** (giữ cửa sổ đang chạy `dotnet run`). Dùng lệnh `Invoke-RestMethod`:

```powershell
# GET
Invoke-RestMethod http://localhost:5113/api/products
Invoke-RestMethod "http://localhost:5113/api/shipping/fee-preview?method=Express&itemsAmount=450000"

# POST - đặt hàng chuẩn (SRP)
$body = '{ "customerName":"Le Van C","email":"c@shop.vn","productId":1,"quantity":2,"shippingMethod":2 }'
Invoke-RestMethod -Uri http://localhost:5113/api/orders -Method Post -ContentType "application/json" -Body $body

# POST - đặt hàng bằng God class (VI PHẠM SRP)
$bad = '{ "customerName":"Bad Guy","email":"bad@shop.vn","productId":4,"quantity":1,"shippingMethod":3 }'
Invoke-RestMethod -Uri http://localhost:5113/api/orders/bad -Method Post -ContentType "application/json" -Body $bad
```

> Lưu ý: trong PowerShell, `curl` bị đổi thành lệnh khác nên cú pháp `curl -X POST ...` **không**
> chạy đúng. Hãy dùng `Invoke-RestMethod` như trên, hoặc gõ rõ `curl.exe` nếu muốn dùng curl thật.

Xem lỗi HTTP (400/404): thêm `-SkipHttpErrorCheck` hoặc dùng cách A để thấy mã trạng thái rõ ràng.

---

## Phần 4. Kịch bản thuyết trình (5–7 phút)

**Bước 0 — Mở đầu bằng vấn đề:** "Em làm API cửa hàng: đặt hàng + tính phí ship. Em sẽ cho thầy
thấy cùng một nghiệp vụ viết kiểu tệ và kiểu đúng khác nhau ra sao."

**Bước 1 — SRP.** Mở `Srp/BadOrderManager.cs`, chỉ 5 khối comment (validate / switch / lưu /
in hóa đơn / log). Gửi request `POST /api/orders/bad`, cho thầy xem field `reason` liệt kê 5 trách
nhiệm. Rồi mở `Srp/OrderService.cs` (bản đúng): "cùng nghiệp vụ nhưng em ủy thác cho từng lớp".
Gửi `POST /api/orders` → thành công. Chốt: "đổi cách gửi mail thì bản đúng chỉ sửa `EmailService`,
không đụng logic đặt hàng."

**Bước 2 — OCP.** Gửi `/api/shipping/legacy-fee-preview` và mở `LegacyShippingCalculator.cs` chỉ
cái `switch`. Rồi gửi `/api/shipping/fee-preview` và mở `ShippingStrategies.cs`: "mỗi phương thức
là một class riêng, `ShippingFeeService` không có if/switch."

**Bước 3 — ĐÒN QUYẾT ĐỊNH (mở rộng LIVE, chứng minh OCP):** thêm phương thức `Drone` ngay tại chỗ
mà không sửa code cũ (code ở Phần 5), chạy lại, gửi `fee-preview?method=Drone`. Nói: "Em thêm tính
năng mới mà không sửa dòng nào trong code tính phí cũ — đó chính là Open/Closed."

**Bước 4 — Xử lý lỗi:** gửi đơn với `productId:999` → 404; số lượng vượt tồn kho → 400; tên rỗng/
email sai → 400 kèm danh sách lỗi. "Hệ thống xử lý được tình huống lỗi, trả đúng mã HTTP."

**Bước 5 — Kết:** "SRP giúp mỗi lớp một việc nên dễ sửa/test; OCP giúp thêm tính năng mới mà không
phá code cũ. Cả hai dựa trên lập trình theo interface + Dependency Injection."

---

## Phần 5. Code cho màn "mở rộng LIVE" — thêm phương thức Drone

Chỉ **3 thao tác THÊM**, không sửa gì code cũ:

**(1)** `Models/ShopModels.cs` — thêm vào enum:

```csharp
public enum ShippingMethod
{
    Standard = 1, Express = 2, SameDay = 3, International = 4,
    Drone = 5            // <-- THÊM
}
```

**(2)** `Ocp/ShippingStrategies.cs` — thêm một class (cuối file):

```csharp
public class DroneShipping : IShippingFeeStrategy
{
    public ShippingMethod Method => ShippingMethod.Drone;
    public decimal CalculateFee(decimal itemsAmount) => 120_000m;  // giao bằng drone
}
```

**(3)** `Program.cs` — thêm một dòng đăng ký:

```csharp
builder.Services.AddScoped<IShippingFeeStrategy, DroneShipping>();   // <-- THÊM
```

Chạy lại `dotnet run` rồi gửi:

```
http://localhost:5113/api/shipping/fee-preview?method=Drone&itemsAmount=450000
```

→ `shippingFee: 120000`. **Không đụng** `ShippingFeeService` và các strategy cũ → đúng OCP.
So sánh: bản `LegacyShippingCalculator` buộc phải mở hàm cũ ra thêm một `case Drone` (vi phạm OCP).

---

## Phần 6. Cách học để hiểu THẬT

1. **Tự giải thích không nhìn code:** che màn hình, nói SRP/OCP là gì, file nào vi phạm, tách ra sao.
2. **Tự tay gõ lại** một strategy hoặc `OrderService` từ đầu.
3. **Tự chế ví dụ mới của bạn:** SRP tách class `User`; OCP tính giảm giá theo loại khách (Thường/VIP/Sỉ).
4. **Vẽ luồng 1 phút:** Controller → OrderService → (ShopData / ShippingFeeService / EmailService).

---

## Phần 7. Câu hỏi thầy hay hỏi & cách đáp gọn

- **"SRP là mỗi class chỉ một hàm?"** → Không, là *một lý do để thay đổi*.
- **"OCP là không bao giờ sửa code?"** → Không, là hạn chế sửa phần *đang chạy ổn định*, ưu tiên thêm mới.
- **"Sao dùng interface + DI?"** → Để phụ thuộc vào abstraction, dễ thay thế/mở rộng.
- **"Strategy Pattern liên quan gì OCP?"** → Là *cách hiện thực* OCP: mỗi thuật toán một class, chọn qua đa hình.
- **"Sao dữ liệu mất khi khởi động lại?"** → Vì dùng in-memory cho gọn; kiến trúc tách interface nên đổi sang DB dễ.

---

## Phần 8. Checklist trước khi demo

- [ ] `dotnet run` chạy được, mở `http://localhost:5113` thấy trang chủ.
- [ ] Thử được API bằng cách A (requests.http) hoặc C (PowerShell).
- [ ] Tập màn "thêm Drone" (Phần 5) ít nhất 1 lần.
- [ ] Nói được 2 câu định nghĩa SRP/OCP không nhìn giấy.
- [ ] Chỉ ngay được file nào vi phạm, file nào áp dụng.
- [ ] Trả lời được các câu hỏi ở Phần 7.
