# Kịch bản Thuyết trình và Demo — Bài 8: NGINX Reverse Proxy & Upstream

Kịch bản "cầm tay chỉ việc" — làm và nói theo từng bước, đúng lộ trình giảng viên yêu cầu.

---

## Phần 0: Chuẩn bị trước khi demo (làm ở nhà)

### 0.1 Sửa file hosts để giả lập domain

**Trên Windows** (dù NGINX chạy trong WSL hay Docker Desktop):
1. Mở Notepad **với quyền Administrator** (chuột phải → Run as Administrator)
2. File → Open → `C:\Windows\System32\drivers\etc\hosts`
3. Thêm dòng cuối: `127.0.0.1  myapp.local`
4. Lưu file

### 0.2 Tạo chứng chỉ SSL tự ký

```bash
cd Bai08_Nginx_ReverseProxy

# Tạo thư mục SSL nếu chưa có
mkdir -p nginx/ssl

# Tạo chứng chỉ bằng openssl (chạy trong Docker cho tiện)
docker run --rm -v ./nginx/ssl:/etc/nginx/ssl alpine/openssl \
    req -x509 -nodes -newkey rsa:2048 \
    -keyout /etc/nginx/ssl/myapp.key \
    -out /etc/nginx/ssl/myapp.crt \
    -days 365 -subj "/C=VN/ST=HCMC/L=HCMC/O=OnlineShop/CN=myapp.local"
```

### 0.3 Build và khởi động

```bash
docker compose up -d --build --scale backend=2
```

---

## Phần 1: NGINX là Web Server (phục vụ file tĩnh)

**Mở slide giải thích rồi chuyển sang demo:**

> "Đầu tiên, em demo vai trò đơn giản nhất của NGINX — phục vụ file tĩnh. Trong thực tế, khi build xong ứng dụng React hay Vue (npm run build), ta copy bản build vào NGINX image. NGINX sẽ phục vụ file HTML/CSS/JS trực tiếp cho trình duyệt."

**Demo:**

```bash
# Xem container frontend đang chạy
docker compose ps frontend

# Frontend được đóng gói trong NGINX image riêng
# Xem Dockerfile của frontend
cat frontend/Dockerfile
```

> "Container frontend này dùng NGINX image, copy file `index.html` vào `/usr/share/nginx/html/`, và cấu hình `try_files` cho SPA routing."

**Mở trình duyệt → http://localhost** (hoặc http://myapp.local)

> "Như thầy thấy, trang web hiển thị thành công. Đây là NGINX đang phục vụ file tĩnh — vai trò Web Server."

**Chứng minh SPA routing:**

> "Em thử truy cập 1 đường dẫn không tồn tại file vật lý — ví dụ http://localhost/about"

Bấm link `/about` trên trang → vẫn hiển thị trang chính.

> "Nhờ cấu hình `try_files $uri $uri/ /index.html;`, mọi route không tìm thấy file thật đều fallback về index.html → client-side routing hoạt động đúng."

---

## Phần 2: NGINX là Reverse Proxy cho FE + BE (TRỌNG TÂM)

> "Bây giờ em demo phần quan trọng nhất — NGINX đóng vai trò Reverse Proxy. Một domain duy nhất phục vụ cả frontend lẫn backend API."

**Mở file cấu hình NGINX và chỉ vào:**

```bash
cat nginx/conf.d/default.conf
```

> "Trong file cấu hình, thầy thấy:
> - `location /` → proxy tới container frontend (phục vụ file tĩnh)
> - `location /api/` → proxy tới upstream backend_pool (chuyển tiếp tới backend)
> - Client gọi cùng domain nên KHÔNG bị lỗi CORS!"

**Demo gọi API từ frontend:**

Trên trang web, bấm nút **"GET /api/products"**

> "Em bấm nút gọi API. Request đi tới NGINX ở `/api/products`, NGINX chuyển tiếp tới backend qua upstream. Dữ liệu JSON trả về thành công — tất cả qua cùng 1 domain!"

**Demo proxy headers — chứng minh X-Real-IP:**

```bash
# Xem log NGINX (sẽ thấy IP thật của client)
docker compose logs nginx --tail=10
```

> "Nhờ các header `proxy_set_header X-Real-IP`, `X-Forwarded-For`, backend có thể biết IP thật của client chứ không phải IP của NGINX. Đây là điều quan trọng cho logging, rate-limit, và bảo mật."

---

## Phần 3: Upstream nhiều Backend — Load Balancing (TRỌNG TÂM)

> "Tiếp theo, em chứng minh NGINX phân phối request tới nhiều backend instance."

**Kiểm tra số instance đang chạy:**

```bash
docker compose ps
```

> "Hiện có 2 container backend đang chạy song song (nhờ `--scale backend=2`). NGINX dùng upstream với round-robin để chia đều request."

**Demo load balancing — bấm nút "Gọi 10 lần liên tiếp" trên trang web:**

> "Em gọi API 10 lần liên tiếp. Bảng kết quả cho thấy request được phân phối luân phiên giữa 2 instance — đây chính là round-robin load balancing!"

**Hoặc dùng Terminal để chứng minh rõ hơn:**

```bash
# Gọi 6 lần, xem header X-Served-By hoặc container hostname
for i in 1 2 3 4 5 6; do
    echo "--- Request $i ---"
    curl -s http://localhost/api/products | head -c 100
    echo ""
done
```

> "Mỗi lần gọi, request được điều hướng tới container khác nhau. Đây là cơ chế cân bằng tải — nếu 1 node chết, các node còn lại vẫn phục vụ được."

**Giới thiệu thêm các chiến lược load balancing khác:**

> "Ngoài round-robin (mặc định), NGINX còn hỗ trợ:
> - `least_conn`: ưu tiên server ít kết nối nhất
> - `ip_hash`: cùng 1 IP client luôn tới cùng 1 server (sticky session)
> - `weight=N`: server mạnh hơn nhận nhiều request hơn"

---

## Phần 4: Cấu hình Container FE/BE trong Docker Network

> "Điểm đặc biệt khi chạy trong Docker: thay vì dùng `localhost` hay IP cứng, NGINX dùng TÊN SERVICE trong Docker network."

**Chỉ vào file cấu hình:**

> "Trong `docker-compose.yml`, frontend là 1 service có tên `frontend`, backend là `backend`. NGINX proxy tới `http://frontend:80` và `http://backend:8080` — Docker DNS tự động phân giải tên thành IP container. Đây là Service Discovery mà nhóm em đã học ở Buổi 7."

---

## Phần 5: client_max_body_size (lỗi 413)

> "Em demo giới hạn kích thước body request."

**Trên trang web, bấm nút "Gửi body 0.5MB":**

> "Body 0.5MB — thành công, vì nhỏ hơn giới hạn."

**Bấm nút "Gửi body 2MB":**

> "Body 2MB — nếu chưa cấu hình `client_max_body_size`, NGINX sẽ trả lỗi 413 Request Entity Too Large. Trong file config của nhóm em đã set `client_max_body_size 20M;` nên 2MB vẫn qua được."

**Để demo lỗi 413 thật:**

```bash
# Tạm sửa config: đặt client_max_body_size 1M
# Sửa trong nginx/conf.d/default.conf: client_max_body_size 1M;
# Sau đó reload NGINX:
docker exec bai08_nginx nginx -t          # Kiểm tra cú pháp
docker exec bai08_nginx nginx -s reload   # Reload không downtime
```

> "Sau khi đổi xuống 1M và reload, bấm gửi body 2MB → lỗi 413! Đây là cách NGINX bảo vệ server khỏi request quá lớn."

---

## Phần 6: Domain + SSL mô phỏng

> "Cuối cùng, em demo domain giả lập và HTTPS."

**Chứng minh domain hoạt động:**

Mở trình duyệt → **http://myapp.local**

> "Nhờ sửa file hosts (`127.0.0.1 myapp.local`), trình duyệt gửi request tới localhost nhưng với domain `myapp.local`. NGINX nhận và xử lý nhờ `server_name myapp.local;`."

**Demo HTTPS:**

Mở trình duyệt → **https://myapp.local**

> "Trình duyệt cảnh báo 'NET::ERR_CERT_AUTHORITY_INVALID'. Đây là chứng chỉ TỰ KÝ — trình duyệt không tin cậy vì nó không nằm trong chain of trust."

**Giải thích Chain of Trust (vẽ lên bảng hoặc slide):**

> "Chứng chỉ bình thường: Root CA (có sẵn trong OS) → ký cho Intermediate CA → ký cho domain.
> Trình duyệt xác minh ngược lên → thấy Root CA đáng tin → Ổn!
> Chứng chỉ tự ký: Tự mình ký cho mình → trình duyệt không có cách xác minh → Cảnh báo!"

> "Trên server thật, dùng Certbot (Let's Encrypt): `certbot --nginx -d yourdomain.com` — miễn phí, tự động gia hạn, trình duyệt tin cậy."

**Bấm "Advanced" → "Proceed to myapp.local" để truy cập:**

> "Khi bấm tiếp tục, trang vẫn hoạt động bình thường qua HTTPS. Chỉ là trình duyệt cảnh báo thôi."

---

## Tổng kết

> "Như vậy, NGINX chính là thành phần đứng trước toàn bộ hệ thống Docker Compose từ Buổi 6, 7:
> - Phục vụ frontend (Web Server)
> - Chuyển tiếp API tới backend (Reverse Proxy)
> - Cân bằng tải giữa nhiều instance (Upstream)
> - Xử lý SSL/HTTPS (SSL Termination)
> - Bảo vệ server (giới hạn body size, ẩn backend)
>
> Đây chính là kiến trúc chuẩn để deploy một hệ thống backend thực tế lên internet."

---

## Lệnh debug hữu ích khi gặp lỗi

```bash
# Kiểm tra cú pháp NGINX trước khi reload (LUÔN LUÔN CHẠY!)
docker exec bai08_nginx nginx -t

# Reload NGINX không cần restart (zero downtime)
docker exec bai08_nginx nginx -s reload

# Xem log NGINX
docker compose logs nginx --tail=50

# Xem log backend
docker compose logs backend --tail=50

# Vào trong container NGINX để debug
docker exec -it bai08_nginx sh

# Kiểm tra DNS resolution trong Docker network
docker exec bai08_nginx nslookup backend
docker exec bai08_nginx nslookup frontend
```
