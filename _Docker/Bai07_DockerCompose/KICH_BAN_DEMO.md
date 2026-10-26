# 🎤 KỊCH BẢN THUYẾT TRÌNH VÀ DEMO DOCKER COMPOSE (BÀI 7)

Dưới đây là kịch bản "cầm tay chỉ việc", bạn cứ làm và nói y hệt từng bước này đảm bảo lấy trọn vẹn điểm phần thực hành.

## 1. Mở đầu (Giới thiệu kiến trúc)
**Bạn vừa mở slide có sơ đồ kiến trúc vừa nói:**
> "Thưa thầy và các bạn, hôm nay nhóm em xin demo hệ thống Backend (OnlineShop) chạy bằng Docker Compose. Thay vì chạy lẻ tẻ từng cái như bài trước, hệ thống của nhóm em mô phỏng đúng môi trường Production thực tế gồm 5 thành phần (Services):
> 1. **Database PostgreSQL**: Lưu trữ dữ liệu.
> 2. **Redis**: Hệ thống Cache.
> 3. **Backend ASP.NET Core**: Xử lý logic, được cấu hình **Multi-instance** (chạy nhiều bản sao song song).
> 4. **Nginx**: Đóng vai trò là Load Balancer (Cân bằng tải) đứng trước Backend.
> 5. Tất cả cấu hình mật khẩu đều được giấu kín trong file `.env`."

---

## 2. Bắt đầu Demo (Các bước gõ lệnh)

*(Mở Terminal / PowerShell tại thư mục `Bai07_DockerCompose`)*

### Bước 1: Khởi động hệ thống & Horizontal Scaling
**Gõ lệnh:**
```bash
docker compose up -d --scale backend=2
```
**Vừa gõ vừa giải thích:**
> "Đầu tiên, em dùng lệnh `up -d` để khởi động toàn bộ hệ thống ngầm. Điểm đặc biệt ở đây là em thêm cờ `--scale backend=2`. Thay vì chỉ chạy 1 server backend dễ bị quá tải, em yêu cầu Docker tự động đẻ ra **2 bản sao Backend** chạy song song. Đây chính là Horizontal Scaling (Scale ngang)."

### Bước 2: Chứng minh hệ thống đang chạy
**Gõ lệnh:**
```bash
docker compose ps
```
**Chỉ tay vào màn hình giải thích:**
> "Như thầy thấy, hệ thống hiện có 5 container đang trạng thái `Up`. Có 2 thằng `backend-1` và `backend-2`. Nginx đang lắng nghe ở cổng `8080` của máy thật. Nó sẽ tự động chia việc 50-50 cho 2 anh backend kia."

### Bước 3: Test API thực tế qua Load Balancer
**Gõ lệnh (hoặc mở Google Chrome gõ):**
```bash
curl.exe -s http://127.0.0.1:8080/api/products
```
*(Nếu muốn chứng minh nó là C# API của nhóm, có thể gọi API Shipping)*
```bash
curl.exe -s "http://127.0.0.1:8080/api/shipping/fee-preview?method=Express&itemsAmount=450000"
```
**Giải thích:**
> "Em gọi API vào cổng 8080. Dữ liệu JSON đã trả về thành công! Thực chất request này đi vào Nginx, Nginx tự điều hướng tới 1 trong 2 cái backend bên dưới chứ em không hề gọi trực tiếp vào backend."

### Bước 4: Xem Log tập trung
**Gõ lệnh:**
```bash
docker compose logs -f
```
*(Để nó chạy khoảng 5 giây rồi ấn Ctrl+C tắt đi)*
**Giải thích:**
> "Điểm hay của Compose là em có thể xem Log tập trung của tất cả 5 services cùng 1 lúc ở đây. Cực kỳ tiện lợi để Debug. Ở trong file `docker-compose.yml`, nhóm em cũng đã cài thêm cấu hình giới hạn kích thước log (`max-size: 10m`) và giới hạn RAM/CPU để server chạy lâu ngày không bị đầy ổ cứng hay tràn RAM (Trình bày thêm cái này để lấy điểm mục 2.6 Nâng cao)."

### Bước 5: Thử thách Dữ Liệu Bền Vững (Persistent Data)
**Gõ lệnh:**
```bash
docker compose down
```
**Giải thích:**
> "Cuối cùng, em dùng lệnh `down` để đập bỏ hoàn toàn tất cả container và mạng ảo. Tuy nhiên, dữ liệu khách hàng trong Database vẫn còn nguyên vẹn. Tại sao? Vì nhóm em đã sử dụng **Named Volume** (`pg_data`). Lần sau khởi động lại, DB tự động map vào đúng ổ đĩa cũ.
> Chỉ khi nào em cố tình thêm tham số `-v` (như `docker compose down -v`) thì toàn bộ dữ liệu DB mới bị hủy diệt."

---

## 3. Tổng kết
> "Dạ phần Demo kiến trúc Multi-instance bằng Docker Compose của nhóm em đến đây là hết. Xin mời thầy đặt câu hỏi ạ!"
