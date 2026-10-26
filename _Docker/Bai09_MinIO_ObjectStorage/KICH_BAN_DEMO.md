# Kịch bản Demo — Bài 09: MinIO Object Storage

> **Thời lượng ước tính:** 15–20 phút
> **Yêu cầu:** Docker Desktop đã cài, trình duyệt, terminal

---

## Chuẩn bị trước buổi demo

```bash
# 1. Vào thư mục bài 09
cd Bai09_MinIO_ObjectStorage

# 2. Copy file environment
cp .env.example .env

# 3. Build tất cả images (lần đầu mất ~2-3 phút)
docker compose build

# 4. Khởi chạy toàn bộ hệ thống
docker compose up -d

# 5. Kiểm tra tất cả services đang chạy
docker compose ps
```

Chuẩn bị file demo: 1 ảnh nhỏ (avatar.jpg ~100KB), 1 file PDF hoặc docx (~1MB).

---

## Phần 1 — Giới thiệu Object Storage & MinIO (2 phút)

### Nói:
> "Object Storage là phương thức lưu trữ file dạng key-value qua HTTP API, khác với file system
> truyền thống. MinIO là giải pháp Object Storage mã nguồn mở, tương thích hoàn toàn với S3 API
> của AWS — nghĩa là code viết cho MinIO có thể chạy với AWS S3 hay Cloudflare R2 chỉ bằng
> đổi config."

### Mở MinIO Console:

1. Truy cập http://localhost:9001
2. Đăng nhập: `minioadmin` / `minioadmin123`
3. **Chỉ cho lớp thấy giao diện:** Buckets, Objects, Access Keys

### Nói:
> "Đây là giao diện quản lý MinIO. Hiện tại chưa có bucket nào. Khi backend khởi động
> và upload file đầu tiên, bucket sẽ được tự động tạo."

---

## Phần 2 — Upload File qua SDK (3 phút) ⭐ TRỌNG TÂM

### Mở Frontend Demo UI:

1. Truy cập http://localhost
2. Tìm section **"Upload File"**

### Demo Upload:

1. Chọn file ảnh (avatar.jpg)
2. Nhập folder: `images` (optional)
3. Nhấn **Upload**
4. **Chỉ kết quả:** hiện objectKey, size, bucket

### Nói:
> "File được upload qua API `/api/storage/upload`. Backend dùng MinIO SDK (NuGet package Minio)
> để gọi `PutObjectAsync`. Object key là đường dẫn: `images/avatar.jpg`."

### Kiểm chứng trên MinIO Console:

1. Quay lại http://localhost:9001
2. Vào bucket `online-shop`
3. **Chỉ file vừa upload** — thấy folder `images/` và file `avatar.jpg` bên trong

### Giải thích code (mở `MinioStorageService.cs`):

```
Luồng: EnsureBucketExists → PutObjectArgs → PutObjectAsync
- EnsureBucketExists: kiểm tra bucket có chưa, nếu chưa → tạo mới
- PutObjectArgs: cấu hình object key, stream, content-type
- PutObjectAsync: upload lên MinIO
```

---

## Phần 3 — List & Download Objects (2 phút)

### Demo List:

1. Trên Frontend, section **"List Objects"**
2. Nhấn **Tải danh sách** (để prefix rỗng → list tất cả)
3. **Chỉ danh sách objects** hiện ra (tên, size)

### Nói:
> "API `GET /api/storage/list` dùng `ListObjectsAsync` — đặc biệt ở đây là SDK trả về
> `IObservable` (reactive stream) chứ không phải List thông thường, nên code phải subscribe
> và collect kết quả."

### Demo Download:

1. Nhấn nút **Download** cạnh file vừa upload
2. File được tải về browser
3. **Mở file** chứng minh nội dung đúng

---

## Phần 4 — Presigned URL (3 phút) ⭐ TRỌNG TÂM

### Nói:
> "Presigned URL là tính năng rất quan trọng trong thực tế. Nó tạo ra một link tạm thời
> có chữ ký, cho phép download file mà không cần credentials. Rất hữu ích cho:
> - Chia sẻ file có thời hạn
> - Cho user download trực tiếp từ storage (giảm tải backend)
> - Link hết hạn → không truy cập được nữa"

### Demo:

1. Trên Frontend, chọn file → nhấn **Presigned URL**
2. **Chỉ URL được tạo** — để ý có `X-Amz-Signature`, `X-Amz-Expires` trong query string
3. **Copy URL, mở trong tab mới** → file download được
4. Giải thích: "URL này có thời hạn (mặc định 1 giờ). Hết hạn → link chết."

### So sánh Public vs Private:

| Cách truy cập      | Cần credential? | Thời hạn     | Use case                |
|--------------------|----------------|-------------|------------------------|
| Qua Backend API     | Có (backend)    | Không hạn    | Download qua ứng dụng  |
| Presigned URL       | Không           | Có (1h, 24h) | Chia sẻ link tạm thời  |
| MinIO Public Policy | Không           | Vĩnh viễn    | File public (avatar)    |

---

## Phần 5 — Move/Rename và Delete (2 phút)

### Demo Move:

1. Trên Frontend, section **"Move / Rename"**
2. Nhập source: `images/avatar.jpg`
3. Nhập dest: `avatars/user-1.jpg`
4. Nhấn **Move**

### Nói:
> "S3 API không có operation 'move' hay 'rename'. Phải thực hiện Copy + Delete.
> Đây là pattern chuẩn của mọi S3-compatible storage."

### Kiểm chứng trên MinIO Console:

1. Quay lại http://localhost:9001
2. Object đã chuyển từ `images/avatar.jpg` → `avatars/user-1.jpg`

### Demo Delete:

1. Nhấn nút **Xóa** cạnh một file
2. Confirm → file biến mất khỏi danh sách
3. Kiểm tra lại MinIO Console → đã xóa

---

## Phần 6 — Use Case: Upload Avatar (2 phút)

### Demo:

1. Trên Frontend, section **"Upload Avatar"**
2. Nhập User ID: `user-123`
3. Chọn file ảnh
4. Nhấn **Upload Avatar**

### Nói:
> "Đây là use case thực tế: upload avatar cho user. Object key tự động là
> `avatars/user-123.jpg`. Nếu upload lại → file cũ bị ghi đè (overwrite).
> Pattern này rất phổ biến trong ứng dụng thực tế."

### Kiểm chứng:

- Trên MinIO Console: thấy `avatars/user-123.jpg`
- Upload lại ảnh khác → size thay đổi (ghi đè)

---

## Phần 7 — Clean Architecture & SOLID (2 phút)

### Mở code và giải thích:

1. **`IObjectStorageService.cs`** (Interface):
> "Theo DIP — Controller phụ thuộc interface, không phụ thuộc MinIO trực tiếp."

2. **`DependencyInjection.cs`** (Registration):
> "MinIO client được đăng ký qua DI container. Muốn đổi sang S3/R2 → tạo class mới
> implement cùng interface, thay đăng ký ở đây. Đúng OCP — Open for extension,
> Closed for modification."

3. **`StorageController.cs`** (Presentation):
> "Controller chỉ biết `IObjectStorageService`, không import MinIO SDK.
> SRP — mỗi class một trách nhiệm."

---

## Phần 8 — Docker & Service Discovery (2 phút)

### Chạy lệnh:

```bash
# Xem tất cả services
docker compose ps

# Xem logs MinIO
docker compose logs minio --tail 20

# Xem backend kết nối MinIO thế nào
docker compose exec backend env | grep MinIO
# Output: MinIO__Endpoint=minio:9000
```

### Nói:
> "Backend kết nối MinIO bằng tên service `minio:9000` — Docker tự resolve DNS.
> Không cần biết IP, không cần expose port ra ngoài cho backend.
> Chỉ port 9000 (S3 API cho presigned URL) và 9001 (Console) mới expose ra host."

---

## Phần 9 — Chuyển sang Cloud (1 phút)

### Nói:
> "Toàn bộ code hiện tại chạy MinIO local. Muốn chuyển sang Cloudflare R2 hoặc AWS S3,
> chỉ cần thay 3 biến trong `.env`:
> - `MINIO_ENDPOINT`: endpoint của cloud provider
> - `MINIO_ACCESS_KEY`: access key mới
> - `MINIO_SECRET_KEY`: secret key mới
>
> Code backend KHÔNG CẦN SỬA GÌ — vì đều dùng S3 API chuẩn.
> Đây là sức mạnh của S3 compatibility và Dependency Inversion."

### ⚠️ Nhấn mạnh bảo mật:
> "Khi dùng key thật của cloud, TUYỆT ĐỐI không commit `.env` lên GitHub.
> Key bị lộ → phát sinh chi phí ngoài ý muốn."

---

## Tổng kết — Tóm tắt kiến thức

| Khái niệm          | Đã demo                                        |
|--------------------|-----------------------------------------------|
| Object Storage      | MinIO — S3 compatible, bucket + object key     |
| Upload/Download     | SDK PutObjectAsync / GetObjectAsync            |
| Presigned URL       | Link tạm thời có chữ ký, không cần credentials |
| Move/Rename         | Copy + Delete (S3 không có native move)        |
| Clean Architecture  | Interface → Service → Controller (DIP, SRP)    |
| Docker Compose      | 6 services, service discovery, volume          |
| Chuyển Cloud        | Đổi 3 biến .env → chạy với R2/S3              |

---

## Xử lý sự cố

| Vấn đề | Nguyên nhân | Giải pháp |
|--------|------------|-----------|
| MinIO Console không mở | Container chưa healthy | `docker compose logs minio` kiểm tra |
| Upload lỗi 413 | NGINX giới hạn body size | Kiểm tra `client_max_body_size` trong nginx config |
| Presigned URL không mở | URL chứa `minio:9000` (internal) | Backend đã replace → `localhost:9000`, kiểm tra lại |
| Backend lỗi kết nối MinIO | MinIO chưa sẵn sàng | Đợi healthcheck pass, `docker compose ps` kiểm tra |
| `Access Denied` | Sai credentials | Kiểm tra .env: MINIO_ACCESS_KEY, MINIO_SECRET_KEY |
