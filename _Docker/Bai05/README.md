# Bài 5 (Buổi 6) — Docker hóa app backend của nhóm (OnlineShop)

Đóng gói **chính app backend của nhóm** (`OnlineShop` — ASP.NET Core) vào Docker, đúng yêu cầu:
**multi-stage build**, **.dockerignore**, chạy bằng **user không phải root**, cấu hình qua **biến môi
trường**. Không dùng ví dụ "Hello World".

> Lộ trình 12 bài: … Bài 4 = Linux/vận hành → **Bài 5 = Docker** → (Bài 6 = Docker Compose + DB).

Thư mục này: `OnlineShop/` (mã nguồn app + `Dockerfile` + `.dockerignore` + `.env.example`),
`docker-build-run.sh` (build & chạy & test tự động), `COMMANDS_CHEATSHEET.md`, `HUONG_DAN_THUYET_TRINH.md`.

---

## 1. Docker là gì?

**Docker** là một nền tảng mở để **phát triển, vận chuyển và chạy ứng dụng**. Docker sử dụng công
nghệ **ảo hóa cấp hệ điều hành** (containerization) để đóng gói ứng dụng cùng với toàn bộ môi
trường chạy vào một đơn vị nhỏ gọn gọi là **container**.

### Lợi ích của Docker

- **Cô lập môi trường**: Mỗi container chạy độc lập, không ảnh hưởng đến ứng dụng khác hay hệ
  thống host.
- **Nhất quán giữa các môi trường**: Ứng dụng chạy giống nhau ở máy dev, máy bạn cùng nhóm, và
  server thật — "chạy được trên máy tôi" không còn là vấn đề.
- **Dễ dàng triển khai và mở rộng**: Chỉ cần có Docker là chạy được, không phải cài đặt SDK hay cấu
  hình phức tạp trên server.

### Vì sao Docker là kỹ năng bắt buộc cho lập trình viên backend?

Khi phát triển và triển khai ứng dụng phía server, Docker đảm bảo **môi trường chạy nhất quán** giữa
máy dev và server thật. Không cần lo về sự khác biệt phiên bản .NET, thư viện, hay cấu hình hệ điều
hành — mọi thứ đã được đóng gói sẵn trong image.

---

## 2. Ba khái niệm cốt lõi

- **Image**: Một **template chỉ đọc** (read-only) chứa các hướng dẫn để tạo ra một container. Giống
  như một **bản thiết kế** của ngôi nhà.
- **Container**: Một **instance có thể chạy được** của một image. Giống như **ngôi nhà được xây** từ
  bản thiết kế. Một image có thể tạo ra nhiều container.
- **Dockerfile**: Một **file văn bản** chứa các lệnh để **tự động build** một Docker image. Giống như
  tập hướng dẫn xây nhà từng bước.

---

## 3. Yêu cầu

- Docker Desktop (Windows/macOS) hoặc Docker Engine (Linux/WSL). Kiểm tra: `docker --version`.
- Không cần cài .NET trên máy: **toàn bộ compile diễn ra bên trong image build**.

## 4. Build & chạy nhanh

```bash
# cách 1: dùng script (build + run + test)
bash docker-build-run.sh

# cách 2: gõ tay (đứng trong thư mục OnlineShop/)
cd OnlineShop
docker build -t onlineshop-api:v1 .
docker run -d --name onlineshop -p 8080:8080 onlineshop-api:v1
curl http://localhost:8080/api/products
```

Ứng dụng bên trong container lắng nghe cổng **8080**; `-p 8080:8080` ánh xạ ra máy thật.

---

## 5. Dockerfile — giải thích từng phần

File `OnlineShop/Dockerfile` dùng **multi-stage build** (2 stage):

### Cấu trúc cơ bản của Dockerfile (theo yêu cầu buổi 6)

| Lệnh | Ý nghĩa |
|---|---|
| `FROM` | Chỉ định base image |
| `WORKDIR` | Thiết lập thư mục làm việc |
| `COPY` | Sao chép file/thư mục từ host vào image |
| `RUN` | Thực thi lệnh trong quá trình build image |
| `EXPOSE` | Khai báo cổng mà container sẽ lắng nghe |
| `ENV` | Biến môi trường **runtime** — tồn tại khi container chạy |
| `ARG` | Biến môi trường **build-time** — chỉ tồn tại khi build image |
| `CMD` | Lệnh **mặc định** khi container khởi động (có thể ghi đè) |
| `ENTRYPOINT` | Lệnh **cố định** khi container khởi động (không bị ghi đè) |

### Stage 1 — BUILD (dùng image có SDK)
```dockerfile
ARG BUILD_CONFIGURATION=Release
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY nuget.config ./
COPY Shop.Business/Shop.Business.csproj          Shop.Business/
COPY Shop.DataAccess/Shop.DataAccess.csproj      Shop.DataAccess/
COPY Shop.Presentation/Shop.Presentation.csproj  Shop.Presentation/
RUN dotnet restore Shop.Presentation/Shop.Presentation.csproj
COPY . .
RUN dotnet publish Shop.Presentation/Shop.Presentation.csproj -c ${BUILD_CONFIGURATION} -o /app/publish /p:UseAppHost=false
```
- `ARG BUILD_CONFIGURATION=Release` : biến **build-time** — cho phép đổi cấu hình khi build:
  `docker build --build-arg BUILD_CONFIGURATION=Debug ...`. Khác `ENV`: ARG **không tồn tại**
  khi container chạy.
- `FROM ... sdk` : base image có bộ **SDK** để compile.
- Copy **riêng các `.csproj` trước** rồi `restore` → tận dụng **cache**: chỉ restore lại khi `.csproj`
  đổi, sửa code không phải restore lại (build nhanh hơn nhiều).
- `dotnet publish` : biên dịch ra bản chạy trong `/app/publish`.

### Stage 2 — RUNTIME (image gọn, chỉ có runtime)
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
RUN addgroup --system appgrp \
 && adduser  --system --ingroup appgrp --no-create-home --disabled-password appuser
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_ENVIRONMENT=Production
EXPOSE 8080
USER appuser
ENTRYPOINT ["dotnet", "Shop.Presentation.dll"]
CMD []
```
- `FROM ... aspnet` : chỉ **runtime**, KHÔNG có SDK → **image nhỏ hơn nhiều** so với mang cả SDK.
- `COPY --from=build` : chỉ lấy **kết quả publish** từ stage 1 (không kèm mã nguồn/SDK/công cụ build).
- `adduser ... appuser` + `USER appuser` : **chạy bằng user không phải root** (least privilege — kiến
  thức buổi Linux). Nếu app bị khai thác, thiệt hại bị giới hạn.
- `ENV` : cấu hình qua **biến môi trường runtime** (URL, môi trường) thay vì hard-code. Có thể ghi
  đè khi chạy: `docker run -e KEY=VALUE ...`.
- `EXPOSE 8080` : khai báo cổng. Dùng **8080 (>1024)** để user non-root bind được (cổng <1024 cần root).
  Lưu ý: EXPOSE chỉ là khai báo, muốn truy cập từ bên ngoài phải dùng `-p` khi `docker run`.

### ENTRYPOINT vs CMD — sự khác biệt

| | ENTRYPOINT | CMD |
|---|---|---|
| Vai trò | Lệnh **cố định** (chương trình chính) | Tham số **mặc định** (có thể thay) |
| Ghi đè khi `docker run`? | **Không** (trừ khi dùng `--entrypoint`) | **Có** — ghi đè bằng đối số cuối |
| Ví dụ | `ENTRYPOINT ["dotnet", "app.dll"]` | `CMD ["--urls", "http://+:80"]` |

Kết hợp: `ENTRYPOINT` chạy ứng dụng, `CMD` cung cấp tham số mặc định. Khi chạy
`docker run onlineshop-api:v1 --urls http://+:9090` sẽ ghi đè `CMD` nhưng vẫn giữ `ENTRYPOINT`.

> **Multi-stage lợi gì?** Image cuối **không chứa SDK/mã nguồn**, chỉ có runtime + bản publish →
> nhỏ, ít lỗ hổng, tải/deploy nhanh.
>
> Ghi chú: image .NET 8+ đã có sẵn user `app`; ở đây nhóm **tự tạo user** để minh họa kỹ năng
> buổi Linux (lệnh `addgroup`, `adduser`).

---

## 6. `.dockerignore`

File `OnlineShop/.dockerignore` loại các thứ **không cần** khỏi build context → image nhỏ & an toàn:
`bin/`, `obj/`, `.git/`, `.vs/`, `*.user`, **`.env`** và `appsettings.Development.json` (tránh lộ
secret vào image), `*.md`… Giống `.gitignore` nhưng dành cho Docker build.

---

## 7. Biến môi trường (ENV vs ARG) & kết nối database

### ENV — biến runtime (tồn tại khi container chạy)

Cấu hình đổi giữa dev/test/prod qua `-e` khi `docker run` (không sửa image):
```bash
docker run -d --name onlineshop -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__Default="Server=db;Database=shop;User=app;Password=secret" \
  onlineshop-api:v1
```

Hoặc dùng file `.env.example` (gọn hơn khi có nhiều biến):
```bash
docker run -d --name onlineshop -p 8080:8080 \
  --env-file .env.example \
  onlineshop-api:v1
```

- Trong .NET, cấu hình lồng nhau dùng **dấu `__`** (2 gạch dưới): `ConnectionStrings__Default`
  tương ứng `ConnectionStrings:Default` trong `appsettings.json`.

### ARG — biến build-time (chỉ tồn tại khi build image)

Truyền tham số khi build:
```bash
# Build bản Debug (thay vì Release mặc định)
docker build --build-arg BUILD_CONFIGURATION=Debug -t onlineshop-api:debug .
```

| | ENV | ARG |
|---|---|---|
| Tồn tại khi nào? | **Khi container chạy** | **Chỉ khi build image** |
| Ghi đè bằng? | `docker run -e KEY=VALUE` | `docker build --build-arg KEY=VALUE` |
| Dùng cho? | Connection string, port, môi trường | Cấu hình build (Debug/Release), version |

- Kết nối tới **database chạy trong container khác** sẽ làm gọn ở **buổi sau (Docker Compose)**;
  ở buổi này ta minh họa việc **truyền connection string qua ENV**.

> Đã kiểm chứng: app chạy bằng user non-root trên cổng 8080, nhận connection string qua ENV, và
> `curl /api/products` trả JSON thật (xem HUONG_DAN_THUYET_TRINH.md, phần kết quả).

---

## 8. Các lệnh Docker cơ bản (gắn với app)

```bash
docker build -t onlineshop-api:v1 .          # build image, gắn tag rõ ràng
docker images                                 # liệt kê image (xem image vừa tạo)
docker run -d --name onlineshop -p 8080:8080 onlineshop-api:v1   # chạy container nền
docker ps            # container đang chạy      | docker ps -a  # cả đã dừng
docker logs onlineshop                          # xem log (debug khi lỗi)
docker exec -it onlineshop bash                 # vào trong container
docker stop onlineshop                          # dừng
docker rm onlineshop                            # xóa container (dừng trước)
docker rmi onlineshop-api:v1                    # xóa image
```

---

## 9. Ghi chú khi demo

- **Image ≠ Container**: image là bản thiết kế (read-only); container là instance đang chạy. Một image
  chạy được nhiều container.
- **Luôn xem `docker logs`** khi container không lên — đây là cách debug phổ biến nhất.
- Nếu cổng 8080 bận, đổi ánh xạ: `-p 8090:8080` (host 8090 → container 8080).
- **Thực hành là cách tốt nhất để học Docker** — chạy thử từng lệnh, xem kết quả, sửa khi lỗi.
