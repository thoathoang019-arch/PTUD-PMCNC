# Kịch bản Demo — Bài 10: CI/CD với Jenkins

> **Thời lượng dự kiến:** 20–25 phút
> **Chuẩn bị:** 2 máy (hoặc 2 terminal), GitHub private repo, Jenkins đã cấu hình
> **Frontend:** http://localhost:8080 (local dev) hoặc http://<cd-ip>:8080 (CD machine)

---

## Chuẩn bị trước khi Demo

### Máy A (CI — Jenkins)
```bash
cd ci-machine/
docker compose -f docker-compose.jenkins.yml up -d
# Đảm bảo Jenkins đang chạy tại http://localhost:8080
# Đã tạo Pipeline job trỏ đến GitHub repo
# Đã cấu hình Credentials (Deploy Key + Registry Token)
```

### Máy B (CD — Deploy)
```bash
cd cd-machine/
# Chạy webhook listener
node webhook-listener.js &
# Chạy infrastructure
docker compose up -d db redis nginx
```

### GitHub Repo
- Push toàn bộ code Bài 10 lên private repo
- Đảm bảo Jenkinsfile ở root repo
- Sửa `CD_SERVER` trong Jenkinsfile = IP thật của Máy B

---

## Phần 1: Giới thiệu CI/CD (2 phút)

**Mở frontend → phần "Khái niệm CI/CD"**

> "Trong các bài trước, chúng ta build và deploy thủ công bằng `docker compose up --build`. 
> Trong thực tế với team 5-10 người, mỗi ngày push code hàng chục lần, việc build-test-deploy 
> thủ công không khả thi. CI/CD tự động hóa toàn bộ quy trình này."

**Giải thích 4 concept cards trên frontend:**
1. **CI (Continuous Integration):** Tự động build + test mỗi khi push code
2. **CD (Continuous Deployment):** Tự động deploy sau khi CI thành công
3. **Unit Test:** Bảo vệ code — test fail = pipeline dừng
4. **Vì sao cần 2 máy:** CI server ≠ Production server

**Chỉ vào sơ đồ kiến trúc** (cuối trang frontend):
> "Máy A chạy Jenkins — chịu trách nhiệm build và test.
> Máy B chạy ứng dụng thật — chỉ nhận image đã được test.
> Kết nối giữa 2 máy bằng HTTP webhook."

---

## Phần 2: Giới thiệu Jenkins & Pipeline (2 phút)

**Mở Jenkins Blue Ocean UI** (http://<ci-ip>:8080/blue)

> "Jenkins là CI/CD server phổ biến nhất, mã nguồn mở. 
> Chúng ta cấu hình pipeline bằng file Jenkinsfile trong repo — gọi là Pipeline as Code."

**Mở Jenkinsfile và giải thích 5 stages:**

| Stage | Vai trò |
|-------|---------|
| Checkout | Pull code mới nhất từ GitHub |
| Test | `dotnet test` — nếu fail thì DỪNG |
| Build Image | `docker build` với tag = BUILD_NUMBER |
| Push Image | Đẩy lên ghcr.io (GitHub Container Registry) |
| Trigger Deploy | HTTP POST webhook → Máy B |

> "Điểm quan trọng: nếu bất kỳ test nào fail, pipeline DỪNG NGAY tại stage Test.
> Image sẽ KHÔNG được build, KHÔNG được push, KHÔNG được deploy.
> Đây là 'cửa ngõ bảo vệ' — code lỗi không bao giờ lên production."

---

## Phần 3: Unit Test — Chứng minh ý nghĩa (3 phút)

**Mở frontend → phần "Unit Test Simulator"**

> "Test không viết cho có. Mỗi test kiểm tra 1 business rule cụ thể."

**Demo từng loại test trên frontend:**

1. **Chọn "Standard" + nhập 600000** → Kết quả: 0 VNĐ (miễn phí ≥500k)
2. **Chọn "Standard" + nhập 200000** → Kết quả: 15,000 VNĐ  
3. **Chọn "Express" + nhập bất kỳ** → Kết quả: 30,000 VNĐ (luôn cố định)
4. **Chọn "SameDay"** → 50,000 VNĐ

> "Đây chính là logic trong ShippingFeeServiceTests.cs — 8 test cases kiểm tra 
> mọi trường hợp: giá trị bình thường, boundary (đúng 500k), exception handling."

**Mở file test (optional):**
```
backend/Shop.Tests/ShippingFeeServiceTests.cs
```
> "Pattern Arrange-Act-Assert: chuẩn bị data → gọi hàm → kiểm tra kết quả.
> [Fact] cho test cố định, [Theory]+[InlineData] cho test với nhiều bộ data."

---

## Phần 4: Chạy Pipeline lần đầu — PASS (5 phút)

### Bước 4.1: Trigger Build

**Trên Jenkins:** Build Now (hoặc push code lên GitHub)

**Trên frontend:** Click "▶ Mô phỏng Pipeline (PASS)" để xem animation

> "Jenkins bắt đầu chạy 5 stages tuần tự. Nếu stage nào fail, 
> các stage sau sẽ KHÔNG chạy."

### Bước 4.2: Quan sát Jenkins

**Chuyển sang Jenkins Blue Ocean** — xem pipeline chạy realtime:

1. ✅ Checkout — Pull code thành công
2. ✅ Test — 11/11 tests passed
3. ✅ Build Image — Docker image built, tag #1
4. ✅ Push Image — Pushed to ghcr.io
5. ✅ Trigger Deploy — Webhook sent to CD machine

> "Tất cả xanh! Image đã được push với tag BUILD_NUMBER = 1."

### Bước 4.3: Kiểm tra CD Machine

**Trên frontend → phần "CD Webhook":** Click "Kiểm tra Status" và "Xem Logs"

> "Máy B đã nhận webhook, pull image, và deploy thành công."

**Click "Kiểm tra Version":**
> "Thấy version = BUILD_NUMBER từ Jenkins. Đây là cách biết chính xác 
> phiên bản nào đang chạy — không bao giờ dùng tag 'latest'."

---

## Phần 5: Làm test FAIL — Pipeline DỪNG (5 phút)

### ⭐ Đây là phần quan trọng nhất — chứng minh test thật sự bảo vệ code

### Bước 5.1: Sửa code để test fail

**Mở `backend/Shop.Business/Strategies/StandardShippingStrategy.cs`**

Sửa ngưỡng miễn phí từ 500,000 → 1,000,000:

```csharp
// TRƯỚC (đúng):
private const decimal FREE_SHIPPING_THRESHOLD = 500_000m;

// SAU (sửa sai — để test fail):
private const decimal FREE_SHIPPING_THRESHOLD = 1_000_000m;
```

> "Giả sử developer vô tình thay đổi ngưỡng miễn phí vận chuyển. 
> Đơn 600k giờ sẽ bị tính phí — sai business logic!"

### Bước 5.2: Push code lỗi

```bash
git add -A
git commit -m "Change shipping threshold (bug)"
git push
```

### Bước 5.3: Jenkins pipeline FAIL

**Trên Jenkins:** Build Now hoặc tự trigger

**Trên frontend:** Click "▶ Mô phỏng Pipeline (FAIL)"

> "Quan sát: stage Checkout ✅, nhưng stage Test ❌ FAIL!"

**Xem console output của Jenkins:**
```
Failed   Shop.Tests.ShippingFeeServiceTests.Standard_FreeShipping_WhenOrderAbove500k
Expected: 0
Actual:   15000
```

> "Test `Standard_FreeShipping_WhenOrderAbove500k` kiểm tra: đơn 600k phải miễn phí.
> Nhưng với ngưỡng mới 1 triệu, 600k bị tính phí 15k → TEST FAIL!
> 
> Pipeline DỪNG NGAY. Image KHÔNG được build. KHÔNG push. KHÔNG deploy.
> Code lỗi KHÔNG BAO GIỜ lên production. Đây là giá trị của CI/CD + Unit Test."

---

## Phần 6: Sửa lại — Pipeline PASS (3 phút)

### Bước 6.1: Sửa code về đúng

```csharp
private const decimal FREE_SHIPPING_THRESHOLD = 500_000m; // Sửa lại đúng
```

### Bước 6.2: Push và build lại

```bash
git add -A
git commit -m "Fix shipping threshold back to 500k"  
git push
```

### Bước 6.3: Pipeline xanh trở lại

> "Lần này tất cả test PASS. Pipeline chạy hoàn chỉnh 5 stages.
> Image mới (BUILD_NUMBER = 3) được build, push, và deploy.
> Máy B tự động cập nhật phiên bản mới."

**Kiểm tra version trên frontend** — version đã thay đổi!

---

## Phần 7: Kiến trúc Clean Architecture & SOLID (2 phút)

**Trên frontend → phần "Shipping Fee API" và "So sánh OCP vs Legacy"**

> "Bài 10 tích lũy kiến thức từ tất cả bài trước:"

| Bài | Kiến thức | Vai trò trong CI/CD |
|-----|-----------|-------------------|
| 05-06 | SOLID + Clean Architecture | Code dễ test, dễ mở rộng |
| 07 | Docker + Compose | Build image, multi-container |
| 08 | NGINX | Reverse proxy trong CD machine |
| 09 | MinIO | Object storage (nếu cần) |
| 10 | CI/CD | Tự động hóa toàn bộ |

> "OCP (Open-Closed Principle) cho phép thêm shipping method mới 
> mà không sửa ShippingFeeService. DIP cho phép test bằng FakeRepository 
> mà không cần database thật. Đây là lý do SOLID quan trọng!"

---

## Phần 8: Docker & Triển khai (2 phút)

### So sánh Dev vs CD

> "Sự khác biệt quan trọng giữa docker-compose.yml (dev) và cd-machine/docker-compose.yml:"

| Aspect | Local Dev | CD Machine |
|--------|----------|------------|
| Backend | `build: ./backend` | `image: ghcr.io/.../api:7` |
| Source code | Cần source code | Chỉ cần image |
| Version | `local-dev` | BUILD_NUMBER từ Jenkins |
| Cập nhật | `docker compose up --build` | Webhook → pull → up |

> "Ở dev, ta build từ source code. Ở production, ta chỉ pull image đã được CI test.
> Đây là nguyên tắc: code chỉ build 1 lần trên CI, deploy nhiều lần."

### Jenkinsfile Credentials

> "Lưu ý bảo mật: SSH key, PAT, registry password đều nằm trong Jenkins Credentials.
> Jenkinsfile dùng `withCredentials` để truy cập — KHÔNG BAO GIỜ hard-code."

---

## Tổng kết (1 phút)

> "Hôm nay chúng ta đã thấy toàn bộ quy trình CI/CD thực tế:
> 
> 1. Developer push code → GitHub
> 2. Jenkins tự động build + test
> 3. Test FAIL → Pipeline dừng → Bảo vệ production  
> 4. Test PASS → Build image → Push registry → Deploy tự động
> 5. Máy CD pull image mới → Cập nhật → Health check
>
> Tất cả diễn ra TỰ ĐỘNG. Developer chỉ cần push code đúng."

---

## Troubleshooting — Xử lý sự cố khi demo

| Vấn đề | Giải pháp |
|--------|-----------|
| Jenkins không thấy repo | Kiểm tra Deploy Key/PAT trong Credentials |
| `dotnet test` fail vì không có SDK | Kiểm tra Jenkins Dockerfile đã cài .NET SDK |
| `docker build` fail trong Jenkins | Kiểm tra docker.sock mounted, jenkins user trong docker group |
| Webhook không đến Máy B | Kiểm tra IP, port 5000, firewall |
| CD machine pull fail | Kiểm tra docker login ghcr.io trên Máy B |
| Frontend không load | `docker compose logs frontend` kiểm tra NGINX |
