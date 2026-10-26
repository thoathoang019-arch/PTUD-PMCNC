# Hướng dẫn thuyết trình & demo — Bài 4 (Buổi 5): Linux/WSL – User – Phân quyền

Buổi này là **kỹ năng vận hành (Ops)**. Điểm ăn điểm với thầy: **mọi thao tác gắn với app backend
thật của nhóm** (OnlineShopMini), thể hiện quy trình chuẩn bị một server Linux cho app.

---

## Phần 1. Ý tưởng cần nói rõ (why, không chỉ how)

- **Least privilege (đặc quyền tối thiểu):** app chạy bằng **user riêng, không phải root**. Nếu app
  bị tấn công, thiệt hại bị giới hạn — kẻ tấn công không có quyền toàn hệ thống.
- **Phân quyền để bảo mật:** mã nguồn/binaries, **file cấu hình bí mật** (mật khẩu DB, JWT secret),
  và **log** cần quyền khác nhau; others **không được** đọc cấu hình hay ghi bừa.
- **sudo chỉ khi bắt buộc:** cài gói, tạo user, ghi `/etc` `/opt` cần root; chạy app thì **không**.

---

## Phần 2. Chuẩn bị trước buổi demo

1. Đã cài WSL/Ubuntu; đã copy dự án vào `~/OnlineShopMini` (hoặc từ `/mnt/d/...`).
2. Đã cài runtime: `sudo apt install -y aspnetcore-runtime-10.0 curl nano`.
3. Mở sẵn 2 cửa sổ terminal (1 để thao tác, 1 để xem log/curl).
4. (Tùy chọn) chạy thử `bash scripts/run-demo.sh` một lần ở nhà cho mượt.

---

## Phần 3. Kịch bản demo (bám 6 mục thầy yêu cầu)

**Mở đầu:** "Nhóm em có API cửa hàng (OnlineShopMini). Em sẽ chuẩn bị một server Linux để chạy nó
đúng chuẩn: user riêng, phân quyền, rồi khởi chạy và test."

**(1) Tạo user/group riêng.** Chạy `sudo bash scripts/01-setup-server.sh /tmp/onlineshop-publish`
(đã publish trước bằng `00-publish.sh`). Rồi `id appuser` — chỉ cho thầy user `appuser` thuộc
`appgrp`, shell `nologin`. Nói: "user này chỉ để chạy app, không đăng nhập được."

**(2) Phân quyền thư mục (chmod).** Chạy `ls -l /opt/onlineshop`, chỉ:
- `app/` và `logs/` là `drwxr-x---` (750) → others không đụng được.
- `config/appsettings.Production.json` là `-rw-r-----` (640) → **others không đọc được bí mật**.
Giải thích số 750/640 (r=4,w=2,x=1).

**(3) Đổi chủ sở hữu (chown).** Chỉ cột chủ/nhóm trong `ls -l` là `appuser appgrp` — "em đã
`chown -R appuser:appgrp /opt/onlineshop` để đúng user chạy app sở hữu."

**(4) Demo các lệnh tiện ích (man, rmdir, apt remove).**
- Giải thích cách đọc tài liệu: Gõ lệnh `man chmod` để mở manual, nói với thầy "Bọn em dùng lệnh man để xem hướng dẫn gốc của Linux". Ấn phím `q` để thoát.
- Tạo folder rỗng: Gõ `mkdir thu_muc_tam`, sau đó xóa an toàn bằng `rmdir thu_muc_tam`.
- Gỡ cài đặt thừa: Cài thử 1 gói thừa `sudo apt install -y vim`, sau đó lập tức gỡ bằng lệnh `sudo apt remove -y vim` để chứng minh biết cách dọn dẹp hệ thống.

**(5) Cài gói chính thức + Sửa cấu hình trực tiếp bằng nano.**
- `sudo apt install -y curl nano` (test API và soạn thảo).
- Gõ lệnh: `nano /opt/onlineshop/config/appsettings.Production.json`
- Màn hình đen hiện lên, TỰ TAY THỰC HIỆN: 
  - Gõ vào một dòng chú thích ngẫu nhiên.
  - Ấn **Ctrl + K** (Cut) để cắt dòng đó, rồi **Ctrl + U** (Paste) để dán lại (đúng chuẩn yêu cầu 2.4).
  - Ấn **Ctrl + O** -> Enter để lưu (Write Out).
  - Ấn **Ctrl + X** để thoát.

**(6) Chạy app bằng user riêng + curl.** `sudo bash scripts/02-run-app.sh 5113`. Chỉ:
- `ps -o user,pid,cmd -C dotnet` → cột USER là **appuser** (không phải root).
- Output curl `/api/products` trả JSON thật → "app chạy được với quyền hạn chế".

**(7) So sánh user thường vs root.** `bash scripts/03-permissions-demo.sh`. Chỉ 3 kết quả:
- `nobody` **không đọc được** file cấu hình (bảo mật OK).
- `appuser` **không ghi được** `/etc` (thiếu quyền).
- Với `sudo` (root) thì ghi được — "nhưng app không nên chạy root."

**Kết:** "Quy trình này đảm bảo app chạy an toàn theo least privilege — nền tảng để deploy backend thật."

---

## Phần 4. Cách học để thành thạo

1. Gõ tay từng lệnh (đừng chỉ chạy script) ít nhất 1 lần để hiểu.
2. Đọc `ls -l` và **tự giải mã** chuỗi quyền `-rwxr-x---` ra số 750.
3. Dùng `man chmod`, `man chown` khi quên — thầy thích thấy bạn biết tra `man`.
4. Thử "phá": chạy app bằng root rồi hỏi "khác gì?" → hiểu vì sao least privilege quan trọng.

---

## Phần 5. Câu hỏi thầy hay hỏi & cách đáp gọn

- **"Vì sao không chạy app bằng root?"** → Nếu app bị chiếm, chạy root = kẻ tấn công có toàn quyền
  máy. User riêng giới hạn thiệt hại (least privilege).
- **"750 nghĩa là gì?"** → chủ rwx(7), nhóm r-x(5), others ---(0). r=4,w=2,x=1.
- **"Khác nhau giữa chmod và chown?"** → `chmod` đổi **quyền**; `chown` đổi **chủ sở hữu/nhóm**.
- **"Khi nào cần sudo?"** → Khi thao tác vượt quyền user thường: cài gói, tạo user, ghi `/etc` `/opt`.
- **"usermod -aG để làm gì? Sao có -a?"** → thêm user vào group; `-a` (append) để **không xóa** các
  group cũ của user.
- **"apt update khác apt upgrade?"** → `update` làm mới **danh sách** gói; `upgrade` **cài bản mới** cho gói đã có.
- **"File .env/appsettings để 640 để làm gì?"** → chứa bí mật (mật khẩu DB, secret); 640 để others
  không đọc được.

---

## Phần 6. Checklist trước khi demo

- [ ] `wsl`/Ubuntu chạy, đã copy app vào `~/OnlineShopMini`, đã cài runtime + curl + nano.
- [ ] Chạy trọn `run-demo.sh` không lỗi.
- [ ] Giải mã được chuỗi quyền trong `ls -l` ra số.
- [ ] Chỉ được tiến trình app chạy bằng `appuser` (không root) + curl ra JSON.
- [ ] Diễn được 3 tình huống so sánh quyền (nobody / appuser / root).
- [ ] Trả lời được các câu hỏi Phần 5.
