# Bài 4 (Buổi 5) — Linux/WSL: Người dùng, Nhóm, Phân quyền, Quản lý gói

Buổi **kỹ năng vận hành (Ops)**: chuẩn bị môi trường Linux để **triển khai (deploy) app backend của
nhóm**. Toàn bộ thao tác **gắn với ứng dụng thật** (`OnlineShopMini` — API cửa hàng đã làm ở Bài 1),
đúng yêu cầu của thầy: không demo file/thư mục mẫu chung chung.

Kết quả (đã kiểm chứng thực tế — xem `DEMO_TRANSCRIPT.md`): tạo **user riêng không phải root** để
chạy app, tạo group, `chown/chmod` thư mục **mã nguồn / cấu hình / log**, dùng `apt`/`nano`, chạy app
bằng user hạn chế và **gọi API bằng curl** để chứng minh chạy được.

> Lộ trình 12 bài: … Bài 3 = Kiến trúc → **Bài 4 = Vận hành Linux (deploy backend)**.

---

## 0. Thư mục này có gì

```
Bai04_Linux_WSL/
├── scripts/
│   ├── 00-publish.sh          # publish app backend của nhóm ra bản deploy
│   ├── 01-setup-server.sh     # tạo user/group + thư mục + chown + chmod
│   ├── 02-run-app.sh          # chạy app bằng appuser + curl thử API
│   ├── 03-permissions-demo.sh # so sánh quyền user thường vs root (sudo)
│   ├── 04-teardown.sh         # dọn dẹp để chạy lại demo
│   └── run-demo.sh            # chạy tất cả từ đầu đến cuối (luyện tập/quay video)
├── onlineshop.service         # (tùy chọn) chạy app như systemd service dưới user riêng
├── COMMANDS_CHEATSHEET.md     # tra cứu nhanh mọi lệnh của buổi này
├── DEMO_TRANSCRIPT.md         # bản ghi output THẬT khi chạy
├── HUONG_DAN_THUYET_TRINH.md  # kịch bản thuyết trình + demo + Q&A
└── README.md                  # file này
```

---

## 1. Linux & WSL — chuẩn bị môi trường

**Linux**: hệ điều hành mã nguồn mở, nền tảng chuẩn để chạy server/backend.
**WSL (Windows Subsystem for Linux)**: chạy Ubuntu ngay trong Windows, không cần máy ảo.

Cài WSL (PowerShell **Admin**, một lần):

```powershell
wsl --install -d Ubuntu
```

Mở "Ubuntu" từ Start Menu, đặt username/password lần đầu. Kiểm tra:

```bash
lsb_release -a      # phiên bản Ubuntu
whoami              # user hiện tại (KHÔNG phải root)
pwd                 # thư mục hiện tại
```

Đưa mã nguồn app vào WSL: ổ Windows nằm ở `/mnt/c/...`. Ví dụ copy dự án vào home Linux:

```bash
cp -r "/mnt/d/PTUD PMCNC/OnlineShopMini" ~/OnlineShopMini
```

---

## 2. Lệnh điều hướng & quản lý file (cơ bản)

| Lệnh | Ý nghĩa | Ví dụ |
|---|---|---|
| `pwd` | thư mục hiện tại | `pwd` |
| `ls -l` | liệt kê (kèm quyền/chủ sở hữu) | `ls -l /opt/onlineshop` |
| `cd` | chuyển thư mục | `cd ~/OnlineShopMini` |
| `mkdir` | tạo thư mục | `mkdir logs` |
| `touch` | tạo file rỗng | `touch .env` |
| `cp` / `mv` | copy / di chuyển-đổi tên | `cp a.txt b.txt` |
| `rm` / `rmdir` | xóa file / thư mục rỗng | `rm old.log` |
| `cat` / `less` | xem nội dung file | `cat appsettings.json` |
| `man <lệnh>` | xem hướng dẫn chi tiết | `man chmod` |

---

## 3. Người dùng, Nhóm & Phân quyền (phần cốt lõi)

### 3.1. User & Group
```bash
sudo groupadd appgrp                       # tạo group
sudo useradd --system --gid appgrp \
     --no-create-home --shell /usr/sbin/nologin appuser   # tạo user hệ thống, không cho đăng nhập
sudo usermod -aG appgrp someuser           # thêm user vào group
id appuser                                 # xem uid/gid/nhóm
# gỡ bỏ: sudo userdel appuser ; sudo groupdel appgrp
```

Vì sao user riêng, `--shell /usr/sbin/nologin`? → **Least privilege**: app bị tấn công cũng không
đăng nhập/không có quyền root, giảm thiệt hại.

### 3.2. Quyền file: r (read) / w (write) / x (execute)
Đối tượng: **u** (user/chủ), **g** (group), **o** (others), **a** (all).
Đọc `ls -l`: `-rwxr-x---` = chủ `rwx`, nhóm `r-x`, others không có gì.

Số bát phân (mỗi nhóm 3 bit): `r=4, w=2, x=1` → cộng lại.
`750` = chủ 7(rwx) · nhóm 5(r-x) · others 0(---).  `640` = chủ 6(rw-) · nhóm 4(r--) · others 0.

```bash
chmod 750 script.sh        # cấp quyền theo số
chmod u+x script.sh        # thêm quyền execute cho chủ
chmod -R 750 /opt/onlineshop/app   # đệ quy cả thư mục
```

### 3.3. `chown` — đổi chủ sở hữu (user) và nhóm (group)
```bash
sudo chown appuser:appgrp file.txt
sudo chown -R appuser:appgrp /opt/onlineshop   # đệ quy
```

### 3.4. `sudo` — chạy với quyền root
Chỉ dùng khi **bắt buộc** (cài gói, ghi vào `/etc`, `/opt`, tạo user…). **Không** chạy app bằng root.

---

## 4. Quản lý gói với `apt` & công cụ

```bash
sudo apt update                  # cập nhật danh sách gói
sudo apt upgrade -y              # nâng cấp gói đã cài
sudo apt install -y curl nano    # cài công cụ test API + soạn thảo
sudo apt install -y iputils-ping # lệnh ping (Ubuntu đóng gói ping ở đây)
# gỡ: sudo apt remove -y <package>
```

Cài **runtime .NET** để chạy app backend (ví dụ .NET 10 trên Ubuntu):
```bash
sudo apt update && sudo apt install -y aspnetcore-runtime-10.0
dotnet --info
```

Công cụ: `curl` (gọi/test API), `ping` (kiểm tra mạng), `nano` (soạn thảo file trong terminal).

---

## 5. Soạn thảo file cấu hình bằng `nano`

```bash
nano /opt/onlineshop/config/.env
```
Trong nano: gõ nội dung trực tiếp → **Ctrl+O** rồi Enter (lưu) → **Ctrl+X** (thoát).
`Ctrl+K` cắt dòng, `Ctrl+U` dán. Ví dụ nội dung `.env`:
```
ASPNETCORE_URLS=http://localhost:5113
DOTNET_ENVIRONMENT=Production
```

---

## 6. TRIỂN KHAI APP CỦA NHÓM LÊN LINUX (phần bắt buộc — gắn app thật)

Chạy lần lượt (xem output thật trong `DEMO_TRANSCRIPT.md`):

```bash
# 1) Publish app của nhóm ra bản chạy
bash scripts/00-publish.sh ~/OnlineShopMini /tmp/onlineshop-publish

# 2) Tạo user/group riêng + thư mục + chown + chmod (cần sudo)
sudo bash scripts/01-setup-server.sh /tmp/onlineshop-publish

# 3) Chạy app bằng appuser (không phải root) + curl thử API
sudo bash scripts/02-run-app.sh 5113

# 4) Minh họa khác biệt quyền user thường vs root
bash scripts/03-permissions-demo.sh

# Dọn dẹp để chạy lại: sudo bash scripts/04-teardown.sh
```

Cấu trúc server được tạo và phân quyền theo **least privilege**:

```
/opt/onlineshop/
├── app/     (750, appuser:appgrp)  ma nguon/binaries — others KHÔNG đọc/ghi
├── config/  (750) + file 640       cau hinh bi mat — others KHÔNG đọc được
└── logs/    (750, appuser)         app (chạy bằng appuser) GHI log vào đây
```

**Vì sao không chạy bằng root?** Nếu app dính lỗ hổng, kẻ tấn công chiếm được tiến trình sẽ có
**toàn quyền hệ thống** nếu app là root. Chạy bằng `appuser` giới hạn thiệt hại chỉ trong phạm vi app.

---

## 7. (Tùy chọn) Chạy như service với systemd

File `onlineshop.service` chạy app dưới `appuser` tự động, khởi động lại khi lỗi:
```bash
sudo cp onlineshop.service /etc/systemd/system/
sudo systemctl daemon-reload
sudo systemctl enable --now onlineshop
sudo systemctl status onlineshop        # xem trạng thái
curl http://localhost:5113/api/products  # test
```
> WSL bản cũ có thể chưa bật systemd. Nếu `systemctl` báo lỗi, dùng cách chạy thủ công ở mục 6,
> hoặc bật systemd trong `/etc/wsl.conf` (`[boot] systemd=true`) rồi `wsl --shutdown`.

---

## 8. Liên hệ với các buổi trước (SOLID/Kiến trúc)

Đây là bước **đưa app đã viết (Bài 1–3) ra môi trường server thật**. File cấu hình `appsettings.json`
được tách và bảo vệ chính là lý do ta đã tách cấu hình khỏi code (SRP/DIP): trên server, chỉ user chạy
app mới đọc được bí mật, những người khác thì không.
