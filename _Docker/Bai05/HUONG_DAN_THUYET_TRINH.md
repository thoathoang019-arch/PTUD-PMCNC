# Hướng dẫn thuyết trình & demo — Bài 5 (Buổi 6): Docker

Gắn với app thật `OnlineShop`. Nguyên tắc: giải thích **Image vs Container**, cho xem **Dockerfile
multi-stage + non-root**, rồi **build → run → curl → exec/logs → stop/rm/rmi** trực tiếp.

---

## Phần 0. Gợi ý nội dung slide lý thuyết

> Yêu cầu đề bài: "Slide: Giới thiệu Docker, các khái niệm, cấu trúc Dockerfile và các lệnh cơ bản."

Nên chia slide thành 4 phần chính:

### Slide 1–3: Docker là gì?
- **Docker** = nền tảng mở để phát triển, vận chuyển và chạy ứng dụng.
- Sử dụng công nghệ **ảo hóa cấp hệ điều hành** (containerization).
- **Lợi ích**: cô lập môi trường, nhất quán giữa dev/test/prod, dễ triển khai & mở rộng.
- **Vì sao bắt buộc?** → Đảm bảo môi trường chạy nhất quán giữa máy dev và server thật.

### Slide 4–5: Ba khái niệm cốt lõi
- **Image** = bản thiết kế (read-only template) → ví dụ: "bản vẽ ngôi nhà".
- **Container** = instance đang chạy của image → ví dụ: "ngôi nhà xây xong từ bản vẽ".
- **Dockerfile** = file văn bản chứa các lệnh để build ra image → ví dụ: "tập hướng dẫn xây nhà".

### Slide 6–8: Cấu trúc Dockerfile
- Bảng liệt kê các lệnh: `FROM`, `WORKDIR`, `COPY`, `RUN`, `EXPOSE`, `ENV`, `ARG`, `CMD`, `ENTRYPOINT`.
- Minh họa bằng chính Dockerfile của nhóm (2 stage: build + runtime).
- Giải thích: **multi-stage build** (tại sao?), **.dockerignore** (loại gì?), **non-root user** (vì sao?).
- Sự khác biệt: **ENV vs ARG**, **ENTRYPOINT vs CMD**.

### Slide 9–10: Các lệnh Docker cơ bản
- `docker build`, `docker images`, `docker run`, `docker ps`, `docker logs`,
  `docker exec`, `docker stop`, `docker rm`, `docker rmi`.
- Có thể dùng bảng 2 cột: Lệnh | Ý nghĩa.

---

## Phần 1. Ba câu cần thuộc
- **Image** = bản thiết kế (read-only). **Container** = instance đang chạy của image.
- **Multi-stage build**: stage BUILD có SDK để compile; stage RUNTIME chỉ có runtime → image nhỏ,
  an toàn (không mang SDK/mã nguồn).
- **Non-root + .dockerignore**: chạy bằng user thường (least privilege) và loại file thừa/secret khỏi image.

## Phần 2. Chuẩn bị
- Cài Docker Desktop / bật Docker trong WSL. `docker --version` chạy được.
- Copy thư mục `OnlineShop/` (đã có Dockerfile) vào máy. Mở terminal tại đó.
- (Tùy chọn) chạy thử `bash docker-build-run.sh` ở nhà cho mượt.

## Phần 3. Kịch bản demo (bám đúng yêu cầu thầy)

1. **Mở Dockerfile**, chỉ 2 stage: "stage build có SDK để compile, stage runtime chỉ có ASP.NET
   runtime nên image nhỏ". Chỉ dòng `ARG BUILD_CONFIGURATION=Release` → "biến build-time,
   khác ENV ở chỗ chỉ tồn tại lúc build". Chỉ dòng `USER appuser` → "container chạy bằng user
   không phải root". Chỉ `.dockerignore` → "loại bin/obj/.git/.env cho gọn và không lộ secret".

2. **Build image, gắn tag rõ ràng:**
   ```
   docker build -t onlineshop-api:v1 .
   docker images        # cho thay image onlineshop-api:v1 vua tao
   ```

3. **Chạy container, ánh xạ cổng, truyền ENV (connection string):**
   ```
   # Cách 1: truyền từng biến bằng -e
   docker run -d --name onlineshop -p 8080:8080 \
     -e ConnectionStrings__Default="Server=db;Database=shop;User=app;Password=secret" \
     onlineshop-api:v1

   # Cách 2: truyền bằng file (gọn hơn khi có nhiều biến)
   docker run -d --name onlineshop -p 8080:8080 \
     --env-file .env.example \
     onlineshop-api:v1

   docker ps            # container dang chay
   ```

4. **Gọi API thật để chứng minh chạy được:**
   ```
   curl http://localhost:8080/api/products
   ```
   (Kết quả đã kiểm chứng: HTTP 200, trả JSON danh sách sản phẩm; app chạy bằng user non-root, cổng 8080.)

5. **Tương tác/debug container:**
   ```
   docker logs onlineshop            # xem log
   docker exec -it onlineshop bash   # vao trong, vd: whoami -> appuser (khong phai root)
   ```

6. **Dừng & xóa (dọn sạch):**
   ```
   docker stop onlineshop
   docker rm onlineshop
   docker rmi onlineshop-api:v1
   ```

## Phần 4. Câu hỏi thầy hay hỏi & cách đáp gọn
- **"Docker là gì?"** → Nền tảng mở để phát triển, vận chuyển và chạy ứng dụng; dùng công nghệ ảo hóa cấp hệ điều hành (containerization).
- **"Image khác Container?"** → Image là bản thiết kế read-only; container là instance đang chạy. 1 image → nhiều container.
- **"Multi-stage build để làm gì?"** → Tách compile (cần SDK) khỏi chạy (chỉ cần runtime) → image cuối nhỏ, ít lỗ hổng, không kèm mã nguồn.
- **"Vì sao chạy non-root trong container?"** → Least privilege; nếu app bị khai thác, hạn chế thiệt hại. (Cùng tư tưởng buổi Linux.)
- **"EXPOSE có tự mở cổng ra ngoài không?"** → Không. EXPOSE chỉ khai báo; muốn truy cập từ máy thật phải `-p host:container`.
- **"CMD khác ENTRYPOINT?"** → ENTRYPOINT là lệnh cố định khi container chạy (không bị ghi đè); CMD là tham số mặc định có thể ghi đè lúc `docker run`.
- **"ENV khác ARG?"** → ENV là biến runtime (tồn tại khi container chạy, ghi đè bằng `-e`); ARG là biến build-time (chỉ tồn tại khi build image, truyền bằng `--build-arg`).
- **"Sao dùng cổng 8080?"** → user non-root không bind được cổng <1024, nên chọn 8080.
- **"Sửa cấu hình đổi môi trường thế nào?"** → truyền `-e` hoặc `--env-file` khi `docker run` (không build lại image).
- **".dockerignore để làm gì?"** → Loại file thừa/secret khỏi build context → image nhỏ, an toàn, build nhanh.

## Phần 5. Checklist
- [ ] `docker build` ra image `onlineshop-api:v1`, `docker images` thấy nó.
- [ ] `docker run -p 8080:8080` + `curl /api/products` ra JSON.
- [ ] `docker exec -it onlineshop whoami` → `appuser` (không phải root).
- [ ] Diễn được `docker logs`, `docker ps -a`, `docker stop/rm/rmi`.
- [ ] Giải thích được multi-stage, non-root, .dockerignore, ENV vs ARG, ENTRYPOINT vs CMD.
