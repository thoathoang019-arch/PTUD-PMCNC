# Bản ghi demo thực tế (đã chạy trên Ubuntu, gắn với app OnlineShopMini của nhóm)

> Đây là output THẬT khi chạy các script trong thư mục `scripts/` trên Ubuntu.
> Trên WSL/Ubuntu của bạn kết quả sẽ tương tự. Bạn có thể tự chạy lại bằng
> `bash scripts/run-demo.sh` (hoặc từng script 00→04).

## Bước 1 — Publish app của nhóm

```console
$ bash 00-publish.sh ~/OnlineShopMini /tmp/onlineshop-publish
  OnlineShopMini -> /root/OnlineShopMini/bin/Release/net10.0/OnlineShopMini.dll
  OnlineShopMini -> /tmp/onlineshop-publish/
Da publish vao: /tmp/onlineshop-publish
```

## Bước 2 — Tạo user/group riêng + phân quyền (`01-setup-server.sh`)

```console
$ sudo bash 01-setup-server.sh /tmp/onlineshop-publish
==> [1] Tao GROUP va USER rieng (least privilege)
    da tao user appuser thuoc group appgrp.
==> [2] Tao cau truc thu muc: app/ (ma nguon), config/ (cau hinh), logs/
==> [3] Copy ban publish vao /opt/onlineshop/app
==> [4] Tao file cau hinh mau (bi mat) trong config/
==> [5] chown: doi CHU SO HUU toan bo cho user chay app
==> [6] chmod theo least privilege
==> XONG. Kiem tra quyen:
drwxr-xr-x 5 appuser appgrp 4096 /opt/onlineshop
drwxr-x--- 2 appuser appgrp 4096 /opt/onlineshop/app
drwxr-x--- 2 appuser appgrp 4096 /opt/onlineshop/config
drwxr-x--- 2 appuser appgrp 4096 /opt/onlineshop/logs
--- file cau hinh (chu y quyen 640, chu so huu appuser) ---
-rw-r----- 1 appuser appgrp 147 appsettings.Production.json

$ id appuser
uid=996(appuser) gid=1002(appgrp) groups=1002(appgrp)

$ getent group appgrp
appgrp:x:1002:
```

Giải thích: thư mục có quyền `drwxr-x---` = chủ (appuser) rwx, nhóm (appgrp) r-x,
**others không có gì**. File cấu hình `-rw-r-----` (640) = others **không đọc được** bí mật.

## Bước 3 — Chạy app bằng `appuser` (không phải root) + gọi API bằng curl

```console
$ sudo bash 02-run-app.sh 5113
==> Khoi chay app bang user 'appuser' tren cong 5113 (log -> /opt/onlineshop/logs/app.log)
   Doi app khoi dong...
==> Goi thu API bang curl (chung minh app chay duoc voi quyen han che):
[{"id":1,"name":"Ao thun cotton","price":150000,"stock":20},{"id":2,"name":"Tai nghe Bluetooth","price":450000,"stock":10},{"id":3,"name":"Ban phim co","price":890000,"stock":8},{"id":4,"name":"Chuot khong day","price":220000,"stock":15},{"id":5,"name":"Sach Clean Code (TV)","price":180000,"stock":30}]

==> Tien trinh dang chay (chu USER la 'appuser', KHONG phai root):
USER       PID CMD
appuser   4807 dotnet /opt/onlineshop/app/OnlineShopMini.dll
```

App lắng nghe `Now listening on: http://localhost:5113`, cột USER của tiến trình là **appuser**.

## Bước 4 — So sánh quyền user thường vs root (`03-permissions-demo.sh`)

```console
### A) User 'nobody' (others) co doc duoc file cau hinh bi mat khong? ###
$ sudo -u nobody cat /opt/onlineshop/config/appsettings.Production.json
cat: /opt/onlineshop/config/appsettings.Production.json: Permission denied
>> Permission denied — file cau hinh duoc bao ve (quyen 640, others khong doc).

### B) User chay app co ghi vao vung he thong (/etc) khong? ###
$ sudo -u appuser touch /etc/test-appuser.txt
touch: cannot touch '/etc/test-appuser.txt': Permission denied
>> Permission denied — user thuong KHONG duoc dung /etc (can root).

### C) Cung thao tac do voi sudo (root) thi duoc: ###
$ sudo touch /etc/test-root.txt && ls -l /etc/test-root.txt
-rw-r--r-- 1 root root 0 /etc/test-root.txt
>> Voi sudo (root) thi ghi duoc — nhung app KHONG nen chay bang root.
```

## Bước 5 — Log do chính `appuser` ghi

```console
$ ls -l /opt/onlineshop/logs/
-rw-r--r-- 1 appuser appgrp ... app.log
$ head /opt/onlineshop/logs/app.log
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5113
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Production
```

→ Chứng minh: thư mục `logs/` do appuser sở hữu nên app (chạy bằng appuser) **ghi log được**,
trong khi others bị hạn chế. Đúng nguyên tắc least privilege.
