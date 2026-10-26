# Bài 8 — Cấu hình NGINX Reverse Proxy & Upstream nhiều Backend

Thư mục này chứa bài thực hành **Buổi 8**, xây dựng trên nền tảng kiến thức và code từ các buổi trước:
- **Bài 5 (Buổi 6)**: Docker hóa backend OnlineShop (multi-stage build, non-root user, ENV)
- **Bài 7 (Buổi 7)**: Docker Compose multi-container (DB + Redis + Backend + NGINX load balancer)
- **Buổi 8 (Bài này)**: NGINX đóng vai trò Web Server + Reverse Proxy + Upstream + SSL

---

## Sơ đồ Kiến trúc

```
[Trình duyệt / Client]
       │
       │  http://myapp.local (port 80)
       │  https://myapp.local (port 443)
       ▼
┌──────────────────────────────────────────┐
│          NGINX REVERSE PROXY             │
│  (Container: bai08_nginx)                │
│                                          │
│  ┌─ location / ──────────────────────┐   │
│  │  proxy_pass → frontend:80         │   │
│  │  (Container FE phục vụ file tĩnh) │   │
│  └───────────────────────────────────┘   │
│                                          │
│  ┌─ location /api/ ──────────────────┐   │
│  │  proxy_pass → upstream backend_pool│  │
│  │  ┌─────────────────────────────┐  │   │
│  │  │ backend:8080  (instance 1)  │  │   │
│  │  │ backend:8080  (instance 2)  │  │   │
│  │  │ ...          (scale=N)      │  │   │
│  │  └─────────────────────────────┘  │   │
│  └───────────────────────────────────┘   │
│                                          │
│  SSL/TLS: listen 443 ssl (self-signed)   │
│  client_max_body_size 20M                │
└──────────────────────────────────────────┘
       │                        │
       ▼                        ▼
┌─────────────┐        ┌──────────────┐
│ PostgreSQL  │        │    Redis     │
│ (bai08_db)  │        │ (bai08_redis)│
└─────────────┘        └──────────────┘
```

---

## Cấu trúc thư mục

```
Bai08_Nginx_ReverseProxy/
├── docker-compose.yml          # Orchestrate toàn bộ hệ thống
├── .env                        # Biến môi trường (KHÔNG commit lên Git)
├── .env.example                # Mẫu biến môi trường
├── README.md                   # File này
├── KICH_BAN_DEMO.md            # Kịch bản demo từng bước cho thuyết trình
│
├── backend/                    # Backend ASP.NET Core (tái sử dụng từ Bài 5, 7)
│   ├── Dockerfile              # Multi-stage build (kiến thức Bài 5)
│   ├── Shop.Presentation/      # API Controllers
│   ├── Shop.Business/          # Business Logic (SOLID, Clean Architecture)
│   └── Shop.DataAccess/        # Data Access Layer
│
├── frontend/                   # Frontend tĩnh (mô phỏng bản build React/Vue)
│   ├── index.html              # SPA với các nút demo API
│   ├── Dockerfile              # Đóng gói file tĩnh vào NGINX image
│   └── nginx-frontend.conf     # Cấu hình NGINX cho SPA (try_files)
│
└── nginx/                      # NGINX Reverse Proxy chính
    ├── conf.d/
    │   └── default.conf        # ⭐ File cấu hình chính (upstream + reverse proxy + SSL)
    ├── ssl/                    # Thư mục chứa chứng chỉ SSL (tự tạo khi chạy)
    └── generate-ssl.sh         # Script tạo chứng chỉ SSL tự ký bằng openssl
```

---

## Khởi chạy nhanh

### Bước 1: Khởi động hệ thống với 2 backend instance

```bash
cd Bai08_Nginx_ReverseProxy

# Tạo chứng chỉ SSL tự ký trước (cần chạy 1 lần)
docker run --rm -v ./nginx/ssl:/etc/nginx/ssl alpine/openssl \
    req -x509 -nodes -newkey rsa:2048 \
    -keyout /etc/nginx/ssl/myapp.key \
    -out /etc/nginx/ssl/myapp.crt \
    -days 365 -subj "/C=VN/ST=HCMC/L=HCMC/O=OnlineShop/CN=myapp.local"

# Khởi động toàn bộ + scale 2 backend
docker compose up -d --build --scale backend=2
```

### Bước 2: Kiểm tra trạng thái

```bash
docker compose ps
```

Kết quả mong đợi: 5 container (db, redis, frontend, backend x2, nginx) đều `Up`.

### Bước 3: Truy cập

- **HTTP**: http://localhost (hoặc http://myapp.local nếu đã sửa hosts)
- **HTTPS**: https://myapp.local (trình duyệt sẽ cảnh báo vì chứng chỉ tự ký)
- **API trực tiếp**: http://localhost/api/products

### Bước 4: Dọn dẹp

```bash
docker compose down          # Giữ data (volume)
docker compose down -v       # Xóa luôn data
```

---

## Giải thích các khái niệm chính

### Web Server vs Reverse Proxy

| | Web Server | Reverse Proxy |
|---|---|---|
| **Vai trò** | Phục vụ file tĩnh (HTML, CSS, JS) | Nhận request → chuyển tiếp tới server phía sau |
| **Ví dụ** | NGINX phục vụ bản build React | NGINX chuyển /api/ tới backend |
| **Lợi ích** | Nhanh, nhẹ, hiệu quả | Ẩn backend, cân bằng tải, SSL tập trung |

### Tại sao dùng Reverse Proxy?

1. **Ẩn backend thật**: Client chỉ thấy NGINX, không biết backend ở đâu/bao nhiêu instance
2. **Cân bằng tải**: Phân phối request đều giữa nhiều backend
3. **SSL Termination**: NGINX xử lý HTTPS, backend chỉ cần HTTP (đơn giản hơn)
4. **Không lỗi CORS**: FE và BE cùng domain (FE ở `/`, BE ở `/api/`)

### upstream và Load Balancing

```nginx
upstream backend_pool {
    server backend:8080;    # Docker DNS tự phân giải tới các container
}
```

Khi `--scale backend=2`, Docker tạo 2 container → NGINX chia đều request (round-robin).

### client_max_body_size

Mặc định NGINX chỉ cho phép body ≤ 1MB. Upload file lớn hơn → lỗi **413**. Cần cấu hình:
```nginx
client_max_body_size 20M;   # Cho phép body tối đa 20MB
```

### Proxy Headers quan trọng

```nginx
proxy_set_header Host              $host;
proxy_set_header X-Real-IP         $remote_addr;
proxy_set_header X-Forwarded-For   $proxy_add_x_forwarded_for;
proxy_set_header X-Forwarded-Proto $scheme;
```

Nếu thiếu → backend thấy mọi request từ IP của NGINX (127.0.0.1), không phải IP thật của client.

### SSL/TLS và Chain of Trust

**Tại sao chứng chỉ tự ký bị trình duyệt cảnh báo?**

```
Chứng chỉ ĐƯỢC TIN CẬY:
  [Root CA] ──ký──▶ [Intermediate CA] ──ký──▶ [Chứng chỉ domain]
  (có sẵn trong        (chuỗi tin cậy           (trình duyệt xác minh
   OS/trình duyệt)      chain of trust)          ngược lên → ✅ Tin cậy)

Chứng chỉ TỰ KÝ:
  [Tự làm CA] ──ký──▶ [Chứng chỉ domain]
  (KHÔNG có trong OS/trình duyệt → ❌ Không tin cậy → Cảnh báo!)
```

**Khắc phục trên production**: Dùng **Certbot (Let's Encrypt)** — miễn phí, tự động:
```bash
certbot --nginx -d yourdomain.com
```

### Cấu hình file hosts (3 trường hợp)

| Trường hợp | File cần sửa | Khi nào |
|---|---|---|
| NGINX trên Windows thuần | `C:\Windows\System32\drivers\etc\hosts` | Không dùng WSL |
| NGINX trong WSL, trình duyệt Windows | `C:\Windows\System32\drivers\etc\hosts` | **Phổ biến nhất** |
| Hoàn toàn trong Linux (curl trong WSL) | `/etc/hosts` (trong Linux) | Test bằng curl |

Thêm dòng:
```
127.0.0.1  myapp.local
```

> **Lưu ý WSL2**: File `/etc/hosts` trong WSL bị tự sinh lại mỗi lần restart. Để giữ cố định, thêm
> `generateHosts = false` vào `[network]` trong `/etc/wsl.conf`.

---

## Liên hệ với các buổi trước

| Buổi | Kiến thức | Áp dụng trong Bài 8 |
|---|---|---|
| SOLID & Clean Architecture | Tách layer (Business, DataAccess, Presentation) | Backend giữ nguyên kiến trúc SOLID |
| Linux cơ bản | User, permission, file system | Non-root user trong Dockerfile, /etc/hosts |
| Bài 5 — Docker | Dockerfile, multi-stage build, ENV/ARG | Backend Dockerfile tái sử dụng |
| Bài 7 — Docker Compose | Multi-container, network, volume, scale | Thêm frontend + NGINX reverse proxy |
| **Bài 8 — NGINX** | **Reverse Proxy, Upstream, SSL** | **File cấu hình NGINX hoàn chỉnh** |
