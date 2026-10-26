# Bài 11 — Tách môi trường Dev / Staging / Production (+ JWT + Rollback)

Kế thừa trực tiếp từ **Bài 10 (CI/CD với Jenkins)**. Bài 10 đã có 1 pipeline → 1 môi
trường. Bài 11 lắp thêm **4 mảnh** để thành hệ thống 3 môi trường như thực tế:

1. **Chia môi trường theo nhánh Git** — `dev` → `staging` → `main`, mỗi nhánh một pipeline
   (Jenkins Multibranch, rẽ nhánh theo `env.BRANCH_NAME`).
2. **Tách Env/Config/Secret ra khỏi code** — JWT login với SecretKey **khác nhau mỗi môi
   trường**; file secret thật đặt sẵn trên máy host, bind-mount vào container lúc chạy; image
   hoàn toàn "sạch" secret.
3. **Rollback nhanh** — Jenkins Parameterized Build chọn tag image cũ, gọi webhook cho máy CD
   pull tag cũ chạy lại, **không build lại từ source**.
4. **Mô phỏng 3 server** — máy mỗi thành viên là một máy CD (Dev/Staging/Prod) trong cùng LAN.

> **Cửa chặn Bài 10 vẫn giữ:** stage `Test` fail → pipeline dừng, không build/push image.
> Bài 11 có thêm `JwtTokenServiceTests` nằm trong cửa chặn này.

---

## Cấu trúc thư mục

```
Bai11_MultiEnvironment/
├─ backend/                         # ASP.NET Core (.NET 10, Clean Architecture - kế thừa Bài 10)
│  ├─ Shop.Business/
│  │  ├─ Interfaces/IJwtTokenService.cs      # MỚI: hợp đồng ký/kiểm tra JWT
│  │  ├─ Services/JwtTokenService.cs         # MỚI: JWT HS256 tự cài (HMAC-SHA256, không cần NuGet)
│  │  └─ DTOs/AuthDtos.cs                     # MỚI: LoginRequest/Response, MeResponse
│  ├─ Shop.Presentation/
│  │  ├─ Controllers/AuthController.cs        # MỚI: POST /api/auth/login, GET /api/auth/me
│  │  ├─ Controllers/EnvController.cs         # MỚI: GET /api/env/info
│  │  ├─ appsettings.json                     # commit — KHÔNG chứa secret thật
│  │  ├─ appsettings.Development.json         # commit — key demo cho Dev
│  │  └─ appsettings.Example.json             # commit — FILE MẪU liệt kê các khoá
│  └─ Shop.Tests/JwtTokenServiceTests.cs      # MỚI: test secret tách biệt (chặn pipeline)
├─ frontend/index.html              # FE demo: badge môi trường + login + test API cần token
├─ nginx/conf.d/default.conf        # reverse proxy / → FE, /api/ → backend
├─ docker-compose.yml               # MÁY DEV: build từ source
├─ cd-machine/                      # MÁY CD (Staging/Prod)
│  ├─ docker-compose.cd.yml         # pull image theo tag + bind-mount secret host
│  ├─ .env.example                  # mẫu .env (REGISTRY, IMAGE_TAG, ASPNETCORE_ENVIRONMENT, SECRET_FILE)
│  ├─ webhook-listener.js           # nhận webhook từ Jenkins (port 9000) → gọi deploy.sh
│  └─ deploy.sh                     # docker pull đúng tag + compose up -d (dùng cho cả rollback)
├─ host-secrets-samples/            # MẪU file secret đặt trên máy host (KHÔNG commit bản thật)
│  ├─ appsettings.Staging.json
│  └─ appsettings.Production.json
├─ Jenkinsfile                      # Multibranch pipeline (rẽ nhánh theo BRANCH_NAME)
├─ Jenkinsfile.rollback             # Job rollback (Parameterized Build)
└─ .gitignore                       # chặn commit appsettings.Staging/Production.json, .env
```

---

## Chạy thử nhanh trên máy Dev

**Cách A — chạy trực tiếp bằng .NET SDK (nhanh nhất để thử API):**

```bash
cd backend
dotnet restore
dotnet test           # chạy unit test (gồm JwtTokenServiceTests) — phải PASS
cd Shop.Presentation
dotnet run            # mặc định môi trường Development, đọc appsettings.Development.json
```

Mở thử:
```bash
curl http://localhost:5113/api/env/info
# đăng nhập lấy token
curl -X POST http://localhost:5113/api/auth/login -H 'Content-Type: application/json' \
     -d '{"username":"alice","password":"123456"}'
# gọi API cần token (thay <TOKEN>)
curl http://localhost:5113/api/auth/me -H 'Authorization: Bearer <TOKEN>'
```

**Cách B — chạy bằng Docker Compose (giống CD hơn):**

```bash
docker compose up -d --build        # backend + frontend + nginx
# mở http://localhost:8080  (FE demo)
```

---

## Thiết lập 3 máy (Dev / Staging / Production)

Trên **mỗi máy CD** (ví dụ máy Staging = 192.168.1.12):

```bash
# 1) Đặt SẴN file secret THẬT của môi trường này (KHÔNG qua Git)
sudo mkdir -p /opt/secrets
sudo cp host-secrets-samples/appsettings.Staging.json /opt/secrets/appsettings.Staging.json
sudo nano /opt/secrets/appsettings.Staging.json    # điền JWT:SecretKey RIÊNG, khác Dev/Prod

# 2) Cấu hình .env cho máy CD
cd cd-machine
cp .env.example .env
nano .env    # đặt REGISTRY, IMAGE_NAME, ASPNETCORE_ENVIRONMENT=Staging, SECRET_FILE=/opt/secrets/appsettings.Staging.json

# 3) Chạy webhook listener (nhận lệnh deploy từ Jenkins)
DEPLOY_TOKEN=matkhau-chung-cua-nhom node webhook-listener.js
#   GET  http://192.168.1.12:9000/health   -> kiểm tra máy sống
#   POST http://192.168.1.12:9000/deploy    <- Jenkins gọi vào
```

Máy Production làm y hệt với `ASPNETCORE_ENVIRONMENT=Production` và
`/opt/secrets/appsettings.Production.json` (key PHẢI khác hẳn).

---

## Jenkins

- **Multibranch Pipeline Job** trỏ vào repo, dùng `Jenkinsfile`. Jenkins tự tạo pipeline
  cho mỗi nhánh `dev`/`staging`/`main`. Pipeline tự tính:
  - `envName`: `dev` / `staging` / `prod`
  - tag image: `myapp:${envName}-${BUILD_NUMBER}` (vd `myapp:staging-42`)
  - IP máy CD: dev `192.168.1.11`, staging `.12`, prod `.13`
- **Rollback Job** (Pipeline Job riêng) trỏ vào `Jenkinsfile.rollback`: chọn `TARGET_ENV`,
  nhập `IMAGE_TAG` cũ (vd `staging-41`) → gọi webhook pull tag cũ chạy lại, không build lại.
- **Credentials cần tạo trong Jenkins:** `cd-deploy-token` (khớp `DEPLOY_TOKEN` máy CD),
  `registry-credentials` (user/pass Registry).

---

## Kịch bản demo (xem báo cáo để biết chi tiết 7 bước)

1. Mở repo: chỉ có `appsettings.Example.json`; trên mỗi máy `ls /opt/secrets/` thấy secret thật khác nhau.
2. Sửa code → push `dev` → Jenkins chạy → FE máy Dev cập nhật, badge `dev-18`.
3. Merge `dev`→`staging` → FE máy Staging badge `staging-42`.
4. **JWT:** login ở Dev lấy token → dán sang FE Staging → **401** (khác SecretKey). Login lại ở Staging → OK.
5. Merge `staging`→`main` → FE Production badge `prod-7`.
6. **Rollback:** cố tình đưa bug → deploy → FE lỗi 500 → job rollback chọn tag cũ → ~15s sau OK.

> **Giới hạn cần nói rõ:** rollback image KHÔNG tự rollback database. Nếu migration mới đổi
> schema không tương thích ngược, rollback code có thể lỗi dữ liệu. Thực tế dùng
> "backward-compatible migration" (expand-contract).

---

## Nguyên tắc bảo mật (quan trọng khi chấm)

- **KHÔNG commit secret thật**, dù một lần — đã vào lịch sử Git là coi như lộ vĩnh viễn.
  `.gitignore` đã chặn `appsettings.Staging.json`, `appsettings.Production.json`, `.env`.
- **Mỗi môi trường một SecretKey khác nhau** — JWT key, connection string của Production phải
  khác hẳn Dev/Staging.
- Kiểm tra trước khi demo: `git log -- '**/appsettings.*.json'` chỉ thấy `.Example`, không thấy giá trị thật.

---

## Đã kiểm thử (build này)

| Phần | Cách kiểm | Kết quả |
|---|---|---|
| webhook-listener.js | `node --check` + chạy thật, curl /health & /deploy | PASS (chặn cả tag injection) |
| deploy.sh | `bash -n` + chạy DRY_RUN qua webhook | PASS (cập nhật IMAGE_TAG trong .env) |
| Thuật toán JWT | cài lại y hệt bằng Node, test Dev-token verify ở Staging | PASS (Dev→Staging = 401) |
| FE demo | render headless + thao tác login/gọi API | PASS (200 OK, và 401 khi token sai môi trường) |
| docker-compose (dev & cd) | `docker compose config` | PASS (resolve đúng tag + mount secret) |
| JSON (appsettings…) | `JSON.parse` | PASS |
| Jenkinsfile / .rollback | cân bằng ngoặc | PASS |
| C# mới (JWT, controller, test) | cân bằng ngoặc + namespace | PASS về cấu trúc |

> ⚠️ `dotnet build` / `dotnet test` và `docker build` **chưa chạy trong môi trường dựng gói
> này** (không có .NET 10 SDK và Docker daemon ở đây). Hãy chạy `dotnet test` ở Cách A trên máy
> của bạn để xác nhận lần cuối — code đã viết đúng .NET 10, không thêm dependency NuGet mới
> (JWT tự cài bằng thư viện chuẩn), và thuật toán JWT đã được kiểm chứng độc lập bằng Node.
