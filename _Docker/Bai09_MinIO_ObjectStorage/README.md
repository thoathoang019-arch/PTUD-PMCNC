# Bài 09 — MinIO Object Storage + S3 SDK

## Mục tiêu

- Hiểu **Object Storage** là gì, khác gì so với File System truyền thống
- Hiểu **S3 API** — tiêu chuẩn công nghiệp cho Object Storage
- Sử dụng **MinIO** — S3-compatible Object Storage chạy bằng Docker
- Tích hợp **MinIO .NET SDK** vào backend ASP.NET Core (Clean Architecture)
- Thực hành: Upload, Download, Move/Rename, Delete, **Presigned URLs**
- Biết cách chuyển sang **Cloud** (Cloudflare R2 / AWS S3) chỉ bằng đổi config

---

## Kiến trúc tổng thể

```
                    ┌─────────────────┐
                    │    Client        │
                    │  (Browser :80)   │
                    └────────┬────────┘
                             │
                    ┌────────▼────────┐
                    │     NGINX       │  Reverse Proxy (Bài 08)
                    │  Load Balancer  │
                    └──┬──────────┬───┘
                       │          │
              ┌────────▼──┐  ┌───▼─────────┐
              │  Frontend  │  │   Backend    │ ← scale được
              │  (SPA)     │  │  ASP.NET     │
              └────────────┘  └──┬───┬───┬──┘
                                 │   │   │
                    ┌────────────▼┐ ┌▼┐ ┌▼──────────┐
                    │  PostgreSQL  │ │R│ │   MinIO    │
                    │   Database   │ │e│ │  Object    │
                    │              │ │d│ │  Storage   │
                    └──────────────┘ │i│ │ :9000 API  │
                                     │s│ │ :9001 GUI  │
                                     └─┘ └───────────┘
```

---

## Khái niệm quan trọng

### 1. Object Storage vs File System

| Đặc điểm          | File System (ext4, NTFS)       | Object Storage (S3/MinIO)      |
|--------------------|-------------------------------|-------------------------------|
| Cấu trúc           | Thư mục / cây thư mục         | Flat — Bucket + Object Key    |
| Truy cập            | Đường dẫn file (`/home/a.txt`) | Key (`uploads/avatar/a.jpg`)  |
| Metadata            | Hạn chế (name, size, date)     | Tuỳ ý (content-type, tags...) |
| Scale               | Giới hạn bởi disk             | Phân tán, scale ngang vô hạn  |
| API                 | Syscall (open/read/write)      | HTTP REST (PUT/GET/DELETE)     |
| Use case            | OS, local apps                 | Web apps, CDN, backup, AI/ML  |

### 2. Các thuật ngữ S3

- **Bucket**: "Thùng chứa" — tương đương một thư mục gốc. Tên bucket phải unique trong hệ thống.
- **Object**: Một file được lưu trong bucket. Mỗi object có:
  - **Key**: Đường dẫn đầy đủ (VD: `uploads/2024/avatar.jpg`)
  - **Value**: Nội dung file (binary data)
  - **Metadata**: Thông tin kèm theo (content-type, size, custom tags)
- **Access Key / Secret Key**: Cặp credential để xác thực (giống username/password)
- **Endpoint**: Địa chỉ server S3 (VD: `minio:9000`, `s3.amazonaws.com`)

### 3. Presigned URL — Link tạm thời

Presigned URL là URL có chữ ký (signature) cho phép truy cập object mà **không cần credential**.

```
Luồng hoạt động:
1. Client gọi API backend: GET /api/storage/presigned-url?key=photo.jpg&expiry=3600
2. Backend dùng SDK tạo presigned URL (có signature + thời hạn)
3. Client dùng URL đó để download trực tiếp từ MinIO (không qua backend)

                  ┌──────────┐
                  │  Client   │
                  └──┬────┬──┘
          (1) API    │    │  (3) Download trực tiếp
                  ┌──▼──┐ │
                  │ BE  │ │
                  └──┬──┘ │
          (2) SDK    │    │
                  ┌──▼────▼──┐
                  │   MinIO   │
                  └───────────┘
```

**Use cases thực tế:**
- Cho user download file private mà không expose credentials
- Link chia sẻ có thời hạn (hết hạn → không truy cập được)
- Giảm tải backend — client download trực tiếp từ storage

### 4. Vì sao S3 không có "Move" / "Rename"?

S3 API không có operation `move` hay `rename` vì objects được thiết kế là **immutable** (bất biến). Để "di chuyển" một object:

```
Move = Copy + Delete
1. CopyObject: tạo bản sao tại key mới
2. RemoveObject: xóa object tại key cũ
```

Đây là pattern chuẩn trong mọi SDK S3 (không riêng MinIO).

---

## Cấu trúc thư mục

```
Bai09_MinIO_ObjectStorage/
├── backend/                        # ASP.NET Core — Clean Architecture
│   ├── Shop.Presentation/
│   │   ├── Controllers/
│   │   │   └── StorageController.cs    # 8 endpoints cho Object Storage
│   │   └── Program.cs
│   ├── Shop.Business/
│   │   ├── Interfaces/
│   │   │   └── IObjectStorageService.cs # Interface (DIP — SOLID)
│   │   ├── Services/
│   │   │   └── MinioStorageService.cs   # Implementation dùng MinIO SDK
│   │   ├── DTOs/
│   │   │   └── StorageDtos.cs           # Data Transfer Objects
│   │   └── DependencyInjection.cs       # Đăng ký MinIO client + service
│   └── Dockerfile                       # Multi-stage build (Bài 05)
│
├── frontend/
│   ├── index.html                  # Demo UI — upload, download, list, presigned URL
│   ├── Dockerfile                  # Đóng gói SPA vào NGINX (Bài 08)
│   └── nginx-frontend.conf         # NGINX config cho frontend
│
├── nginx/
│   └── conf.d/
│       └── default.conf            # Reverse Proxy config (Bài 08)
│
├── docker-compose.yml              # 6 services: db, redis, minio, backend, frontend, nginx
├── .env                            # Biến môi trường (KHÔNG commit lên git)
├── .env.example                    # Mẫu biến môi trường
├── README.md                       # ← File này
└── KICH_BAN_DEMO.md               # Kịch bản demo trên lớp
```

---

## SOLID trong bài này

| Nguyên tắc | Áp dụng |
|-----------|---------|
| **SRP** (Single Responsibility) | `MinioStorageService` chỉ lo Object Storage, `StorageController` chỉ lo HTTP |
| **OCP** (Open/Closed) | Thêm provider mới (R2, S3) → tạo class mới implement `IObjectStorageService`, không sửa code cũ |
| **DIP** (Dependency Inversion) | Controller phụ thuộc `IObjectStorageService` (interface), không phụ thuộc `MinioStorageService` (concrete) |
| **ISP** (Interface Segregation) | `IObjectStorageService` chỉ chứa các method liên quan Object Storage, tách biệt khỏi các interface khác |

---

## Hướng dẫn chạy

### Bước 1: Clone và cấu hình

```bash
# Copy file environment
cp .env.example .env
# Chỉnh sửa .env nếu cần (mật khẩu, bucket name...)
```

### Bước 2: Build và khởi chạy

```bash
# Build tất cả services
docker compose build

# Khởi chạy
docker compose up -d

# Scale backend (tùy chọn, tái sử dụng Bài 08)
docker compose up -d --scale backend=3
```

### Bước 3: Truy cập

| Service         | URL                          | Mô tả                      |
|----------------|------------------------------|-----------------------------|
| Frontend (SPA) | http://localhost              | Demo UI — upload/download   |
| MinIO Console  | http://localhost:9001         | Quản lý buckets, objects    |
| Backend API    | http://localhost/api/products | API qua NGINX reverse proxy |
| MinIO API      | http://localhost:9000         | S3 API (cho presigned URLs) |

### Bước 4: Đăng nhập MinIO Console

- URL: http://localhost:9001
- Username: `minioadmin` (hoặc giá trị MINIO_ROOT_USER trong .env)
- Password: `minioadmin123` (hoặc giá trị MINIO_ROOT_PASSWORD trong .env)

---

## API Endpoints — Storage

| Method | Endpoint                          | Mô tả                              |
|--------|----------------------------------|-------------------------------------|
| POST   | `/api/storage/upload`             | Upload file (multipart/form-data)   |
| GET    | `/api/storage/download?key=`      | Download file                       |
| GET    | `/api/storage/list?prefix=`       | Liệt kê objects                     |
| GET    | `/api/storage/info?key=`          | Thông tin chi tiết object           |
| GET    | `/api/storage/presigned-url?key=` | Tạo presigned URL                   |
| POST   | `/api/storage/move`               | Di chuyển / đổi tên object          |
| DELETE | `/api/storage/delete?key=`        | Xóa object                          |
| POST   | `/api/storage/avatar/{userId}`    | Upload avatar (use case thực tế)    |

---

## Chuyển sang Cloud Storage

Một trong những ưu điểm lớn nhất của S3 API là **tính tương thích** — code viết cho MinIO có thể chạy với bất kỳ S3-compatible provider nào chỉ bằng thay đổi config.

### Cloudflare R2

```env
MINIO_ENDPOINT=<account-id>.r2.cloudflarestorage.com
MINIO_ACCESS_KEY=<r2-access-key>
MINIO_SECRET_KEY=<r2-secret-key>
```

**Ưu điểm:** Không tính phí egress (bandwidth ra), giá storage rẻ.

### AWS S3

```env
MINIO_ENDPOINT=s3.amazonaws.com
MINIO_ACCESS_KEY=<aws-access-key-id>
MINIO_SECRET_KEY=<aws-secret-access-key>
```

**Lưu ý:** Cần thêm `WithRegion("ap-southeast-1")` trong code nếu dùng AWS S3.

### ⚠️ Bảo mật khi dùng Cloud

> **QUAN TRỌNG:** Luôn bảo mật Access Key / Secret Key!
> - Không hardcode trong source code
> - Không commit file `.env` lên GitHub
> - Thêm `.env` vào `.gitignore`
> - Dùng key thật của R2/S3 mà bị lộ → **phát sinh chi phí ngoài ý muốn**

---

## Liên kết các bài học

| Bài | Kiến thức | Tái sử dụng trong Bài 09 |
|-----|----------|--------------------------|
| 01–04 | SOLID, Clean Architecture | Interface, DI, tách layer |
| 05 | Docker, Dockerfile, multi-stage build | Backend Dockerfile |
| 07 | Docker Compose, service discovery | 6 services, container gọi nhau bằng tên |
| 08 | NGINX Reverse Proxy, Upstream, SSL | Reverse proxy → backend + frontend |
| **09** | **MinIO, Object Storage, S3 SDK** | **Upload, download, presigned URLs** |
