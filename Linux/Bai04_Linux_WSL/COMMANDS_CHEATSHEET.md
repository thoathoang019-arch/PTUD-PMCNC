# Cheat-sheet lệnh Linux — Buổi 5 (cầm khi demo)

## Điều hướng & file
```
pwd                      # thư mục hiện tại
ls -l / ls -la           # liệt kê + quyền + chủ sở hữu (a: cả file ẩn)
cd <dir> / cd .. / cd ~  # chuyển thư mục / lên 1 cấp / về home
mkdir <dir> / rmdir      # tạo / xóa thư mục rỗng
touch <file>             # tạo file rỗng
cp a b / mv a b          # copy / di chuyển-đổi tên
rm <file> / rm -r <dir>  # xóa file / xóa thư mục (đệ quy)
cat / less <file>        # xem nội dung
man <lệnh>               # hướng dẫn chi tiết
```

## User / Group
```
sudo groupadd appgrp                 # tạo group
sudo useradd --system --gid appgrp --no-create-home --shell /usr/sbin/nologin appuser
sudo usermod -aG appgrp <user>       # thêm user vào group
id <user> / whoami / groups          # xem thông tin user
sudo userdel <user> / sudo groupdel <group>
```

## Phân quyền
```
# r=4 w=2 x=1 ; u=chủ g=nhóm o=others a=all
chmod 750 file        # chủ rwx, nhóm r-x, others ---
chmod 640 secret      # chủ rw-, nhóm r--, others ---
chmod u+x script.sh   # thêm execute cho chủ
chmod -R 750 dir/     # đệ quy
sudo chown user:group file      # đổi chủ sở hữu + nhóm
sudo chown -R appuser:appgrp /opt/onlineshop
```

## apt & công cụ
```
sudo apt update                 # cập nhật danh sách gói
sudo apt upgrade -y             # nâng cấp
sudo apt install -y curl nano iputils-ping
sudo apt remove -y <package>
curl http://localhost:5113/api/products   # test API
ping -c 3 google.com                       # kiểm tra mạng
```

## nano
```
nano <file>     # mở
Ctrl+O, Enter   # lưu (Write Out)
Ctrl+X          # thoát
Ctrl+K / Ctrl+U # cắt dòng / dán
```

## Chạy app bằng user riêng
```
sudo -u appuser dotnet /opt/onlineshop/app/OnlineShopMini.dll
ps -o user,pid,cmd -C dotnet     # kiểm tra tiến trình chạy bằng user nào
```
