#!/usr/bin/env bash
# =============================================================================
# Buoi 5 - Thiet lap moi truong Linux de chay backend .NET voi USER RIENG
# (khong chay bang root - nguyen tac least privilege).
# Chay tren WSL/Ubuntu:  sudo bash 01-setup-server.sh /duong/dan/ban-publish
# =============================================================================
set -euo pipefail

APP_NAME="onlineshop"
APP_USER="appuser"          # user rieng de chay app (khong phai root)
APP_GROUP="appgrp"          # group cua app
APP_ROOT="/opt/${APP_NAME}" # thu muc server chua app
DLL_NAME="OnlineShopMini.dll"
PUBLISH_SRC="${1:-/tmp/onlineshop-publish}"   # noi chua ban 'dotnet publish'

echo "==> [1] Tao GROUP va USER rieng (least privilege)"
groupadd -f "$APP_GROUP"
if id "$APP_USER" &>/dev/null; then
  echo "    user $APP_USER da ton tai, bo qua."
else
  # 1. Tao user rieng (--system: user he thong, KHONG cho dang nhap)
  useradd --system --no-create-home --shell /usr/sbin/nologin "$APP_USER"
  # 2. Dung usermod -aG de gán user vao group (Theo dung yeu cau cua giang vien)
  usermod -aG "$APP_GROUP" "$APP_USER"
  echo "    da tao user $APP_USER va them vao group $APP_GROUP bang usermod -aG."
fi

echo "==> [2] Tao cau truc thu muc: app/ (ma nguon), config/ (cau hinh), logs/"
mkdir -p "$APP_ROOT"/{app,config,logs}

echo "==> [3] Copy ban publish vao $APP_ROOT/app"
cp -r "$PUBLISH_SRC"/. "$APP_ROOT/app/"

echo "==> [4] Tao file cau hinh mau (bi mat) trong config/"
cat > "$APP_ROOT/config/appsettings.Production.json" <<JSON
{
  "ConnectionStrings": { "Default": "Server=db;Database=shop;User=app;Password=SIEU_BI_MAT" },
  "Jwt": { "Secret": "khong-duoc-lo-ra-ngoai" }
}
JSON

echo "==> [5] chown: doi CHU SO HUU toan bo cho user chay app"
chown -R "$APP_USER":"$APP_GROUP" "$APP_ROOT"

echo "==> [6] chmod theo least privilege"
# app/: chu & nhom doc + chay; OTHERS khong duoc gi (750)
chmod -R 750 "$APP_ROOT/app"
# config/: chu & nhom vao duoc; file cau hinh 640 (others KHONG doc duoc bi mat)
chmod 750 "$APP_ROOT/config"
find "$APP_ROOT/config" -type f -exec chmod 640 {} \;
# logs/: user chay app duoc GHI
chmod 750 "$APP_ROOT/logs"

echo "==> XONG. Kiem tra quyen:"
ls -ld "$APP_ROOT" "$APP_ROOT"/app "$APP_ROOT"/config "$APP_ROOT"/logs
echo "--- file cau hinh (chu y quyen 640, chu so huu $APP_USER) ---"
ls -l "$APP_ROOT/config"
