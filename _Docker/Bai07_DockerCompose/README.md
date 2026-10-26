# 🚀 Hướng Dẫn Thuyết Trình Bài 7 - Docker Compose (Multi-container & Multi-instance)

Thư mục này chứa hệ thống Backend hoàn chỉnh của dự án `OnlineShop`, được thiết kế với kiến trúc Microservices cơ bản mô phỏng đúng môi trường Production.

## 🏗️ Sơ đồ Kiến Trúc

```text
[Người Dùng] 
     │ (Gọi vào Cổng 8080)
     ▼
[ NGINX Load Balancer ] ───── Khai báo trong docker-compose.yml
     │
     ├── Tới 50% Request ──▶ [ Backend Instance 1 ] (ASP.NET Core)
     │                             │
     └── Tới 50% Request ──▶ [ Backend Instance 2 ] (ASP.NET Core)
                                   │
                                   ├──▶ [ PostgreSQL Database ] (Dữ liệu bền vững)
                                   └──▶ [ Redis Cache ]
```

---

## 🛠️ 4 Lệnh "Thần Thánh" Để Demo Cho Giảng Viên

Vào Terminal, chuyển đến thư mục `Bai07_DockerCompose` và thực hiện lần lượt các bước sau để báo cáo:

### Bước 1: Khởi động hệ thống & Scale ngang (Multi-instance)
Giảng viên yêu cầu *"Từ 1 image backend, chạy ra 2 instance khác nhau"*. Bạn gõ lệnh này:
```bash
docker compose up -d --scale backend=2
```
*Giải thích lúc gõ: "Thưa thầy, lệnh này sẽ bật toàn bộ DB, Redis, Nginx lên. Riêng thằng Backend em truyền tham số `--scale backend=2` để nó đẻ ra 2 bản sao chạy song song, giúp ứng dụng chịu tải tốt hơn (Horizontal Scaling)."*

### Bước 2: Kiểm tra trạng thái
```bash
docker compose ps
```
*Giải thích: "Màn hình sẽ hiển thị rõ ràng 2 container backend đang chạy, cùng với DB, Redis và Nginx."*

### Bước 3: Test API thực tế
Trực tiếp click hoặc gõ vào trình duyệt link sau (Trỏ vào Nginx cổng 8080):
👉 `http://127.0.0.1:8080/api/products`

*Giải thích: "Dữ liệu JSON trả về hoàn toàn đi qua cổng Nginx, Nginx tự biết cách điều phối request chia đều cho 2 backend bên trong."*

### Bước 4: Xem Log tổng hợp
```bash
docker compose logs -f
```
*Giải thích: "Thay vì xem log từng cái một như buổi trước, Docker Compose gộp chung log của DB, Redis, Nginx, Backend vào 1 luồng duy nhất rất dễ theo dõi."*

### Bước 5: Chứng minh Dữ Liệu Bền Vững (Volume) và Dọn dẹp
```bash
docker compose down
```
*Giải thích: "Lệnh down sẽ đập bỏ toàn bộ container và network. Tuy nhiên, dữ liệu DB vẫn được bảo toàn nhờ cơ chế Named Volume. Lần sau bật lên lại (lệnh up) dữ liệu cũ vẫn còn nguyên."*

> **Ghi chú cực kỳ ăn điểm:**
> "Thưa thầy, để hệ thống chạy ổn định lâu dài (Production), nhóm em đã cấu hình thêm **Giới hạn RAM/CPU** (Memory limit) và **Tự động xóa log cũ** (Log rotation `max-size: 10m`) ngay trong file `docker-compose.yml`. Tránh tình trạng server bị đầy ổ cứng hoặc hết RAM do 1 container nào đó bị lỗi Memory Leak."
