# Bài 10: CI/CD với Jenkins — Tích hợp & Triển khai Liên tục

> **Môn học:** Phát triển ứng dụng phía máy chủ
> **Kiến thức tích lũy:** SOLID + Clean Architecture + Docker + NGINX + MinIO + **CI/CD**

---

## 1. CI/CD là gì?

### CI — Continuous Integration (Tích hợp Liên tục)

CI là thực hành **tự động hóa** quá trình: mỗi khi developer push code lên repository, hệ thống sẽ tự động **build**, **chạy test**, và **báo kết quả**. Mục tiêu: phát hiện lỗi sớm, trước khi code được merge.

```
Developer Push Code → CI Server tự động:
  1. Checkout code mới nhất
  2. Restore dependencies (dotnet restore)
  3. Chạy Unit Test (dotnet test)
  4. Build Docker Image
  5. Push Image lên Registry
```

### CD — Continuous Deployment (Triển khai Liên tục)

CD là bước tiếp theo: sau khi CI thành công, hệ thống **tự động triển khai** phiên bản mới lên server production/staging. Không cần SSH vào server để chạy lệnh thủ công.

```
CI thành công → Gửi webhook → CD Server:
  1. Nhận thông báo (HTTP POST)
  2. Pull image mới từ Registry
  3. Cập nhật .env (IMAGE_TAG mới)
  4. docker compose up -d (zero-downtime)
  5. Health check xác nhận deploy OK
```

### Vì sao cần CI/CD?

| Không có CI/CD | Có CI/CD |
|---------------|----------|
| Build thủ công trên máy dev | Build tự động trên CI server |
| Quên chạy test → lỗi lên production | Test bắt buộc, fail = dừng pipeline |
| Deploy bằng SSH + copy file | Deploy tự động qua webhook |
| "Trên máy tôi chạy được!" | Mọi người build cùng 1 môi trường |
| Không biết version nào đang chạy | Mỗi build có tag riêng (BUILD_NUMBER) |

---

## 2. Kiến trúc 2 Máy

```
┌─────────────────────────────────────────────────────┐
│                    GITHUB (Private Repo)             │
│    Online Shop API + Jenkinsfile + Dockerfile        │
└───────────────┬─────────────────────────────────────┘
                │ git pull (Deploy Key / PAT)
                ▼
┌─────────────────────────────────────────────────────┐
│              MÁY A — CI SERVER (Jenkins)             │
│                                                      │
│  Jenkins Pipeline:                                   │
│  ┌──────────┐ ┌──────┐ ┌───────┐ ┌──────┐ ┌──────┐ │
│  │ Checkout  │→│ Test │→│ Build │→│ Push │→│Deploy│ │
│  │   Code   │ │(xUnit)│ │Image │ │(ghcr)│ │Trigger│ │
│  └──────────┘ └──────┘ └───────┘ └──────┘ └──┬───┘ │
│                                               │      │
│  Nếu Test FAIL → Pipeline DỪNG, không build!  │      │
└───────────────────────────────────────────────┼──────┘
                HTTP POST /deploy               │
                (webhook qua mạng LAN)          │
                ▼                               │
┌─────────────────────────────────────────────────────┐
│              MÁY B — CD SERVER (Deploy)              │
│                                                      │
│  webhook-listener.js (Node.js, port 5000):           │
│  1. Nhận POST /deploy {image, tag, build_number}     │
│  2. docker pull <image>:<tag>                        │
│  3. Cập nhật .env → IMAGE_TAG=<tag>                  │
│  4. docker compose up -d                             │
│  5. Health check → Xác nhận OK                       │
│                                                      │
│  ┌────────┐ ┌───────┐ ┌─────────┐ ┌───────┐         │
│  │  NGINX │→│Backend│→│PostgreSQL│ │ Redis │         │
│  │ :8080  │ │(image)│ │  :5432   │ │ :6379 │         │
│  └────────┘ └───────┘ └─────────┘ └───────┘         │
└─────────────────────────────────────────────────────┘
```

**Tại sao cần 2 máy?**
Trong thực tế, CI server (nơi build + test) và Production server (nơi chạy ứng dụng) luôn là máy khác nhau. CI server cần nhiều CPU/RAM để build; Production server cần ổn định và bảo mật. Việc tách riêng giúp: (1) CI không ảnh hưởng production, (2) Production không cần cài build tools, (3) Mô phỏng đúng quy trình thực tế.

---

## 3. Jenkins — CI Server

### Jenkins là gì?

Jenkins là **CI/CD server mã nguồn mở** phổ biến nhất thế giới. Jenkins đọc file `Jenkinsfile` trong repo để biết cần làm gì (Pipeline as Code).

### Cài đặt Jenkins (2 cách)

#### Cách 1: Docker (Khuyến nghị cho bài này)

```bash
cd ci-machine/
docker compose -f docker-compose.jenkins.yml up -d
```

Jenkins sẽ chạy tại `http://localhost:8080`. Lấy mật khẩu ban đầu:

```bash
docker exec bai10-jenkins cat /var/jenkins_home/secrets/initialAdminPassword
```

#### Cách 2: Cài trên Windows

1. Cài Java JDK 17+ (Jenkins yêu cầu Java)
2. Tải Jenkins MSI từ https://www.jenkins.io/download/
3. Cài đặt và truy cập http://localhost:8080

### Kết nối GitHub (Private Repo)

#### Deploy Key (SSH) — Khuyến nghị

```bash
# Chạy script tạo Deploy Key
cd ci-machine/
bash generate-deploy-key.sh
```

Script sẽ hướng dẫn:
1. Tạo cặp SSH key (ed25519)
2. Copy public key → GitHub repo → Settings → Deploy Keys
3. Copy private key → Jenkins → Credentials → SSH Username with private key

#### Personal Access Token (PAT) — Cách 2

1. GitHub → Settings → Developer settings → Personal access tokens
2. Tạo token với quyền `repo` (full control of private repositories)
3. Jenkins → Credentials → Username with password (username = GitHub username, password = PAT)

⚠️ **Bảo mật:** KHÔNG hard-code token/key trong Jenkinsfile. Luôn dùng Jenkins Credentials.

---

## 4. Jenkinsfile — Pipeline as Code

Pipeline gồm 5 stage chính:

| Stage | Mô tả | Lệnh chính |
|-------|--------|------------|
| **Checkout** | Lấy code từ GitHub | `checkout scm` |
| **Test** | Chạy Unit Test | `dotnet restore` → `dotnet test` |
| **Build Image** | Build Docker image | `docker build -t <image>:<tag>` |
| **Push Image** | Đẩy image lên Registry | `docker push <image>:<tag>` |
| **Trigger Deploy** | Gửi webhook đến máy CD | `curl POST http://<cd-ip>:5000/deploy` |

### Test FAIL → Pipeline DỪNG

Đây là điểm quan trọng nhất của CI: nếu có **bất kỳ test nào fail**, pipeline **dừng ngay** tại stage Test. Image sẽ **KHÔNG được build**, **KHÔNG được push**, và **KHÔNG được deploy**. Điều này đảm bảo code lỗi không bao giờ lên production.

### Image Tag = BUILD_NUMBER

```
ghcr.io/username/online-shop-api:7   ← Build #7
ghcr.io/username/online-shop-api:8   ← Build #8 (mới nhất)
```

Không dùng mãi tag `latest` vì:
- Không biết đang chạy version nào
- Không thể rollback chính xác
- Docker có thể cache `latest` cũ

---

## 5. Unit Test — Bảo vệ Code

### Tại sao Unit Test quan trọng trong CI/CD?

Unit Test là **cửa ngõ bảo vệ** trong pipeline. Test phải có ý nghĩa thật — không viết test chỉ để cho có.

### Test trong project này

#### ShippingFeeServiceTests (8 test cases)

Test **logic tính phí vận chuyển** theo OCP Strategy Pattern:

- `Standard_FreeShipping_WhenOrderAbove500k` — Đơn ≥500k → miễn phí
- `Standard_ChargesFee_WhenOrderBelow500k` — Đơn <500k → phí 15k
- `Standard_FreeShipping_WhenOrderExactly500k` — Boundary: đúng 500k → miễn phí
- `Express_AlwaysCharges30k` — Giao nhanh luôn 30k
- `SameDay_AlwaysCharges50k` — Giao trong ngày luôn 50k
- `International_AlwaysCharges200k` — Quốc tế luôn 200k
- `UnsupportedMethod_ThrowsException` — Phương thức không hỗ trợ → Exception
- `[Theory] Standard_FeeThreshold_WithMultipleAmounts` — 5 data points kiểm tra ngưỡng

#### ProductAppServiceTests (3 test cases)

Test **mapping Entity → DTO** bằng FakeRepository (manual mock, không dùng thư viện):

- `GetAllProducts_ReturnsCorrectCount` — Đếm đúng số sản phẩm
- `GetAllProducts_MapsEntityToDto` — Map đúng fields
- `GetAllProducts_ReturnsEmpty_WhenNoProducts` — Trả empty list khi không có data

### Chạy test local

```bash
cd backend/
dotnet test --verbosity normal
```

---

## 6. Docker Registry — Kho chứa Image

### GitHub Container Registry (ghcr.io) — Khuyến nghị

```bash
# Login (dùng PAT với quyền write:packages)
echo $PAT | docker login ghcr.io -u <username> --password-stdin

# Push
docker push ghcr.io/<username>/online-shop-api:7
```

### Docker Hub — Cách 2

```bash
docker login -u <username>
docker push <username>/online-shop-api:7
```

⚠️ **Không dùng mãi tag `latest`**: Luôn gắn tag rõ ràng (build number hoặc commit SHA).

---

## 7. Webhook — Kết nối CI ↔ CD

### Cơ chế hoạt động

```
Jenkins (Máy A)                    CD Server (Máy B)
     │                                    │
     │  POST /deploy                      │
     │  {                                 │
     │    "image": "ghcr.io/.../api",     │
     │    "tag": "7",                     │
     │    "build_number": "7"             │
     │  }                                 │
     │ ──────────────────────────────────→ │
     │                                    │ docker pull
     │                                    │ update .env
     │                                    │ docker compose up -d
     │                                    │ health check
     │  200 OK                            │
     │ ←────────────────────────────────── │
```

### Webhook Listener (CD Machine)

File `cd-machine/webhook-listener.js` — Node.js HTTP server:
- `POST /deploy` — Nhận webhook, thực hiện deploy
- `GET /status` — Kiểm tra listener đang chạy
- `GET /logs` — Xem log deploy gần nhất

Chạy listener:

```bash
cd cd-machine/
node webhook-listener.js
# Hoặc dùng PM2 cho production:
# pm2 start webhook-listener.js --name cd-listener
```

---

## 8. SOLID trong Bài 10

| Nguyên tắc | Thể hiện |
|-----------|----------|
| **SRP** | Jenkinsfile mỗi stage 1 nhiệm vụ; webhook-listener chỉ xử lý deploy |
| **OCP** | ShippingFeeService mở rộng bằng Strategy mới, không sửa code cũ |
| **LSP** | Mọi Strategy thay thế được nhau qua IShippingFeeStrategy |
| **ISP** | Interface nhỏ gọn: IProductRepository, IShippingFeeService |
| **DIP** | Business layer phụ thuộc abstraction, test dùng FakeRepository |

---

## 9. Cấu trúc thư mục

```
Bai10_CICD_Jenkins/
├── backend/                          # Source code ASP.NET Core
│   ├── Dockerfile                    # Multi-stage build (Bài 07+)
│   ├── OnlineShop.slnx              # Solution file (gồm Shop.Tests)
│   ├── Shop.Business/               # Business Layer (SOLID patterns)
│   ├── Shop.DataAccess/             # Data Access Layer
│   ├── Shop.Presentation/           # API Layer (Controllers)
│   └── Shop.Tests/                  # ⭐ Unit Tests (xUnit)
│       ├── ShippingFeeServiceTests.cs
│       └── ProductAppServiceTests.cs
│
├── frontend/                         # SPA demo CI/CD
│   ├── Dockerfile                    # NGINX container cho frontend
│   ├── nginx-frontend.conf           # NGINX config cho SPA
│   └── index.html                    # UI: pipeline, test, webhook demo
│
├── ci-machine/                       # ⭐ MÁY A — CI Server
│   ├── docker-compose.jenkins.yml    # Jenkins container
│   ├── jenkins/Dockerfile            # Jenkins + Docker CLI + .NET SDK
│   └── generate-deploy-key.sh        # Tạo SSH Deploy Key
│
├── cd-machine/                       # ⭐ MÁY B — CD Server
│   ├── docker-compose.yml            # Backend (pull image) + DB + NGINX
│   ├── webhook-listener.js           # Node.js listener (port 5000)
│   ├── deploy.sh                     # Script deploy thủ công
│   ├── nginx/conf.d/default.conf     # Reverse proxy
│   └── .env                          # IMAGE_NAME + IMAGE_TAG
│
├── nginx/conf.d/default.conf         # NGINX cho local dev
├── docker-compose.yml                # Local dev environment
├── Jenkinsfile                       # ⭐ Pipeline as Code (5 stages)
├── .env / .env.example               # Environment variables
├── README.md                         # (File này)
└── KICH_BAN_DEMO.md                  # Kịch bản demo trên lớp
```

---

## 10. Quick Start — Chạy nhanh

### Chạy Local Development

```bash
# 1. Copy env
cp .env.example .env

# 2. Build và chạy tất cả services
docker compose up --build -d

# 3. Kiểm tra
docker compose ps
curl http://localhost:8080/api/products

# 4. Mở frontend
# Truy cập http://localhost:8080 trên trình duyệt
```

### Setup CI Machine (Máy A)

```bash
cd ci-machine/
docker compose -f docker-compose.jenkins.yml up -d
# Truy cập Jenkins: http://localhost:8080
# Lấy password: docker exec bai10-jenkins cat /var/jenkins_home/secrets/initialAdminPassword
```

### Setup CD Machine (Máy B)

```bash
cd cd-machine/
# 1. Chạy webhook listener
node webhook-listener.js &

# 2. Chạy services (chưa có backend image)
docker compose up -d db redis nginx
# Backend sẽ được deploy tự động khi CI push image
```

---

## 11. Luồng Demo Hoàn chỉnh

```
1. Push code lên GitHub private repo
2. Jenkins "Build Now" (hoặc webhook trigger)
3. Jenkins checkout code → chạy test → build image → push ghcr.io
4. Jenkins gửi webhook POST đến Máy B
5. Máy B pull image → cập nhật .env → docker compose up
6. Truy cập frontend → kiểm tra version mới
7. Sửa code cho test FAIL → push → Jenkins pipeline dừng!
8. Sửa lại code → push → Pipeline xanh → Deploy thành công
```

---

## 12. Bảo mật — Lưu ý quan trọng

⚠️ **PHẢI làm:**
- Lưu SSH key, PAT, registry password trong **Jenkins Credentials**
- Dùng `.gitignore` để loại `.env`, key files
- Dùng **Deploy Key** (chỉ read access cho 1 repo) thay vì PAT (access tất cả repo)
- Gắn **tag cụ thể** cho mỗi image (BUILD_NUMBER), không chỉ `latest`

❌ **KHÔNG làm:**
- Hard-code token/password trong Jenkinsfile
- Commit `.env` hay private key lên GitHub
- Dùng `latest` tag mà không có version cụ thể
- Mở webhook listener ra public internet không có authentication
